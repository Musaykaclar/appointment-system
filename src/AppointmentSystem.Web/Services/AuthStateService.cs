using AppointmentSystem.Application.DTOs;
using AppointmentSystem.Domain.Entities;
using AppointmentSystem.Web.Constants;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.JSInterop;

namespace AppointmentSystem.Web.Services
{
    public class AuthStateService
    {
        private readonly HttpClient _httpClient;
        private readonly IJSRuntime _jsRuntime;
        private UserDto? _currentUser;
        private string? _token;
        private bool _isInitialized = false;
        private readonly SemaphoreSlim _initSemaphore = new SemaphoreSlim(1, 1);

        public AuthStateService(HttpClient httpClient, IJSRuntime jsRuntime)
        {
            _httpClient = httpClient;
            _jsRuntime = jsRuntime;
        }

        public UserDto? CurrentUser => _currentUser;
        public bool IsAuthenticated => _currentUser != null;
        public bool IsAdmin => _currentUser?.Role == UserRole.Admin;
        public bool IsInitialized => _isInitialized;

        public event Action? OnAuthStateChanged;

        public async Task InitializeAsync()
        {
            if (_isInitialized)
                return;

            await _initSemaphore.WaitAsync();
            try
            {
                if (_isInitialized)
                    return;

                await LoadFromLocalStorage();
                _isInitialized = true;
            }
            finally
            {
                _initSemaphore.Release();
            }
        }

        public async Task WaitForInitializationAsync()
        {
            if (!_isInitialized)
            {
                await InitializeAsync();
            }
        }

        private async Task LoadFromLocalStorage()
        {
            try
            {
                var userJson = await _jsRuntime.InvokeAsync<string>("localStorage.getItem", StorageConstants.AuthUserKey);
                var token = await _jsRuntime.InvokeAsync<string>("localStorage.getItem", StorageConstants.AuthTokenKey);

                if (!string.IsNullOrEmpty(userJson) && !string.IsNullOrEmpty(token))
                {
                    _currentUser = JsonSerializer.Deserialize<UserDto>(userJson, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });
                    _token = token;
                    OnAuthStateChanged?.Invoke();
                }
            }
            catch
            {
                // localStorage yüklenemezse sessizce devam et
            }
        }

        public async Task<bool> LoginAsync(string username, string password)
        {
            try
            {
                var loginDto = new LoginDto { Username = username, Password = password };
                var response = await _httpClient.PostAsJsonAsync("/api/auth/login", loginDto);
                
                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<LoginResponseDto>();
                    if (result?.Success == true && result.User != null)
                    {
                        _currentUser = result.User;
                        _token = result.Token;
                        
                        // localStorage'a kaydet
                        await SaveToLocalStorage();
                        
                        OnAuthStateChanged?.Invoke();
                        return true;
                    }
                }
                return false;
            }
            catch
            {
                return false;
            }
        }

        public async Task LogoutAsync()
        {
            _currentUser = null;
            _token = null;
            await ClearLocalStorage();
            OnAuthStateChanged?.Invoke();
        }

        private async Task SaveToLocalStorage()
        {
            try
            {
                if (_currentUser != null && _token != null)
                {
                    var userJson = JsonSerializer.Serialize(_currentUser);
                    await _jsRuntime.InvokeVoidAsync("localStorage.setItem", StorageConstants.AuthUserKey, userJson);
                    await _jsRuntime.InvokeVoidAsync("localStorage.setItem", StorageConstants.AuthTokenKey, _token);
                }
            }
            catch
            {
                // localStorage kaydedilemezse sessizce devam et
            }
        }

        private async Task ClearLocalStorage()
        {
            try
            {
                await _jsRuntime.InvokeVoidAsync("localStorage.removeItem", StorageConstants.AuthUserKey);
                await _jsRuntime.InvokeVoidAsync("localStorage.removeItem", StorageConstants.AuthTokenKey);
            }
            catch
            {
                // localStorage temizlenemezse sessizce devam et
            }
        }

        public async Task<bool> RegisterAsync(string username, string password, string email, string fullName)
        {
            try
            {
                var registerDto = new RegisterDto 
                { 
                    Username = username, 
                    Password = password,
                    Email = email,
                    FullName = fullName
                };
                var response = await _httpClient.PostAsJsonAsync("/api/auth/register", registerDto);
                
                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<RegisterResponseDto>();
                    if (result?.Success == true && result.User != null)
                    {
                        _currentUser = result.User;
                        // Token oluştur (basit implementasyon)
                        _token = $"{result.User.Id}:{result.User.Username}:{result.User.Role}";
                        
                        await SaveToLocalStorage();
                        OnAuthStateChanged?.Invoke();
                        return true;
                    }
                }
                return false;
            }
            catch
            {
                return false;
            }
        }

        public string? GetToken() => _token;
    }
}

