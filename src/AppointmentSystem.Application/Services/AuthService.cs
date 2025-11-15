using AppointmentSystem.Application.DTOs;
using AppointmentSystem.Domain.Entities;
using AppointmentSystem.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AppointmentSystem.Application.Services
{
    public class AuthService : IAuthService
    {
        private readonly AppointmentDbContext _context;

        public AuthService(AppointmentDbContext context)
        {
            _context = context;
        }

        public async Task<LoginResponseDto> LoginAsync(LoginDto loginDto)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Username == loginDto.Username && u.IsActive);

            if (user == null || user.Password != loginDto.Password) // Gerçek uygulamada hash kontrolü yapılmalı
            {
                return new LoginResponseDto
                {
                    Success = false,
                    ErrorMessage = "Kullanıcı adı veya şifre hatalı"
                };
            }

            // Basit token (gerçek uygulamada JWT kullanılmalı)
            var token = $"{user.Id}:{user.Username}:{user.Role}";

            return new LoginResponseDto
            {
                Success = true,
                Token = token,
                User = new UserDto
                {
                    Id = user.Id,
                    Username = user.Username,
                    Email = user.Email,
                    FullName = user.FullName,
                    Role = user.Role
                }
            };
        }

        public async Task<UserDto?> GetUserByIdAsync(int userId)
        {
            var user = await _context.Users.FindAsync(userId);
            return user != null ? MapToUserDto(user) : null;
        }

        public async Task<RegisterResponseDto> RegisterAsync(RegisterDto registerDto)
        {
            var existingUser = await FindExistingUserAsync(registerDto.Username, registerDto.Email);
            if (existingUser != null)
            {
                return CreateDuplicateUserError(existingUser, registerDto);
            }

            // Yeni kullanıcı oluştur (sadece User rolü ile)
            var newUser = new User
            {
                Username = registerDto.Username,
                Password = registerDto.Password, // Gerçek uygulamada hash'lenmiş olmalı
                Email = registerDto.Email,
                FullName = registerDto.FullName,
                Role = UserRole.User, // Yeni kayıt olanlar sadece User rolüne sahip
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            _context.Users.Add(newUser);
            await _context.SaveChangesAsync();

            return new RegisterResponseDto
            {
                Success = true,
                User = MapToUserDto(newUser)
            };
        }

        public async Task<UserDto?> GetUserByUsernameAsync(string username)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Username == username);

            return user != null ? MapToUserDto(user) : null;
        }

        // Private helper methods
        private async Task<User?> FindExistingUserAsync(string username, string email)
        {
            return await _context.Users
                .FirstOrDefaultAsync(u => u.Username == username || u.Email == email);
        }

        private RegisterResponseDto CreateDuplicateUserError(User existingUser, RegisterDto registerDto)
        {
            var errorMessage = existingUser.Username == registerDto.Username
                ? "Bu kullanıcı adı zaten kullanılıyor"
                : "Bu e-posta adresi zaten kullanılıyor";

            return new RegisterResponseDto
            {
                Success = false,
                ErrorMessage = errorMessage
            };
        }

        private UserDto MapToUserDto(User user)
        {
            return new UserDto
            {
                Id = user.Id,
                Username = user.Username,
                Email = user.Email,
                FullName = user.FullName,
                Role = user.Role
            };
        }
    }
}

