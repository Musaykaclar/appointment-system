using AppointmentSystem.Application.DTOs;
using AppointmentSystem.Domain.Entities;
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
        private const string StorageKeyUser = "auth_user";
        private const string StorageKeyToken = "auth_token";

        public AuthStateService(HttpClient httpClient, IJSRuntime jsRuntime)
        {
            _httpClient = httpClient;
            _jsRuntime = jsRuntime;
        }

        public UserDto? CurrentUser => _currentUser;
        public bool IsAuthenticated => _currentUser != null;
        public bool IsAdmin => _currentUser?.Role == UserRole.Admin;

        public event Action? OnAuthStateChanged;

        public async Task InitializeAsync()
        {
            await LoadFromLocalStorage();
        }

        private async Task LoadFromLocalStorage()
        {
            try
            {
                var userJson = await _jsRuntime.InvokeAsync<string>("localStorage.getItem", StorageKeyUser);
                var token = await _jsRuntime.InvokeAsync<string>("localStorage.getItem", StorageKeyToken);

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
                    await _jsRuntime.InvokeVoidAsync("localStorage.setItem", StorageKeyUser, userJson);
                    await _jsRuntime.InvokeVoidAsync("localStorage.setItem", StorageKeyToken, _token);
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
                await _jsRuntime.InvokeVoidAsync("localStorage.removeItem", StorageKeyUser);
                await _jsRuntime.InvokeVoidAsync("localStorage.removeItem", StorageKeyToken);
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

