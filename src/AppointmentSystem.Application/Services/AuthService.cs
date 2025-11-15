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
            if (user == null) return null;

            return new UserDto
            {
                Id = user.Id,
                Username = user.Username,
                Email = user.Email,
                FullName = user.FullName,
                Role = user.Role
            };
        }

        public async Task<RegisterResponseDto> RegisterAsync(RegisterDto registerDto)
        {
            // Kullanıcı adı kontrolü
            var existingUser = await _context.Users
                .FirstOrDefaultAsync(u => u.Username == registerDto.Username || u.Email == registerDto.Email);

            if (existingUser != null)
            {
                return new RegisterResponseDto
                {
                    Success = false,
                    ErrorMessage = existingUser.Username == registerDto.Username 
                        ? "Bu kullanıcı adı zaten kullanılıyor" 
                        : "Bu e-posta adresi zaten kullanılıyor"
                };
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
                User = new UserDto
                {
                    Id = newUser.Id,
                    Username = newUser.Username,
                    Email = newUser.Email,
                    FullName = newUser.FullName,
                    Role = newUser.Role
                }
            };
        }

        public async Task<UserDto?> GetUserByUsernameAsync(string username)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Username == username);

            if (user == null) return null;

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

