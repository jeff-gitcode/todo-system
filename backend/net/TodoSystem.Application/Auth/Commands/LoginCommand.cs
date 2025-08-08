using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;

namespace TodoSystem.Application.Auth.Commands
{
    public class LoginCommand : IRequest<LoginResponse>
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }

    public class LoginResponse
    {
        public string Token { get; set; } = string.Empty;
        public string RefreshToken { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public long Expiration { get; set; }
    }

    public class LoginCommandValidator : AbstractValidator<LoginCommand>
    {
        public LoginCommandValidator()
        {
            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Email is required")
                .EmailAddress().WithMessage("A valid email address is required");

            RuleFor(x => x.Password)
                .NotEmpty().WithMessage("Password is required")
                .MinimumLength(6).WithMessage("Password must be at least 6 characters");
        }
    }

    public class LoginCommandHandler : IRequestHandler<LoginCommand, LoginResponse>
    {
        private readonly IConfiguration _configuration;

        // In a real application, you would inject your user repository or service here
        // private readonly IUserRepository _userRepository;

        public LoginCommandHandler(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task<LoginResponse> Handle(LoginCommand request, CancellationToken cancellationToken)
        {
            // TODO: Replace this with actual user authentication from your database
            // In a real app, you would verify credentials against your database
            // For demo purposes, we'll accept a simple hardcoded credential
            if (request.Email != "admin@example.com" || request.Password != "admin123")
            {
                throw new UnauthorizedAccessException("Invalid email or password");
            }

            // Create claims for the token
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Email, request.Email),
                new Claim(ClaimTypes.Name, "Admin User"), // In real app, get from user profile
                new Claim(ClaimTypes.Role, "User"),
                new Claim(JwtRegisteredClaimNames.Sub, request.Email),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            // Get the JWT settings from configuration
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(
                _configuration["Jwt:Key"] ?? throw new InvalidOperationException("JWT Key not configured")));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var expiry = DateTime.Now.AddHours(1);

            // Create the JWT token
            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: expiry,
                signingCredentials: creds
            );

            // Return the token and related information
            return new LoginResponse
            {
                Token = new JwtSecurityTokenHandler().WriteToken(token),
                Email = request.Email,
                DisplayName = "Admin User", // In real app, get from user profile
                Expiration = ((DateTimeOffset)expiry).ToUnixTimeSeconds(),
                RefreshToken = Convert.ToBase64String(Guid.NewGuid().ToByteArray()) // Simple refresh token for demo
            };
        }
    }
}