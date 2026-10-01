using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.VisualBasic;
using RoyalVilla.DTO;
using RoyalVilla_API.Data;
using RoyalVilla_API.Models;
using RoyalVilla_API.Services.IServices;
using System.Diagnostics.Tracing;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace RoyalVilla_API.Services
{
    public class AuthService : IAuthService
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IConfiguration _configuration;
        private readonly IMapper _mapper;
        private readonly ITokenService _tokenService;

        public AuthService(ApplicationDbContext db, IConfiguration configuration, IMapper mapper, 
            UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager, ITokenService tokenService)
        {
            _db = db;
            _configuration = configuration;
            _mapper = mapper;
            _userManager = userManager;
            _roleManager = roleManager;
            _tokenService = tokenService;
        }

        public async Task<bool> IsEmailExistsAsync(string email)
        {
            return await _db.ApplicationUsers.AnyAsync(u => u.Email.ToLower() == email.ToLower());
        }

        public async Task<TokenDTO?> LoginAsync(LoginRequestDTO loginRequestDTO)
        {
            try
            {
                var user = await _db.ApplicationUsers.FirstOrDefaultAsync(u => u.Email.ToLower() == loginRequestDTO.Email.ToLower());

                if (user == null)
                {
                    return null; // user not found 
                }

                bool isValid = await _userManager.CheckPasswordAsync(user, loginRequestDTO.Password);

                if (!isValid)
                {
                    return null; // invalid password 
                }

                // generate TOKEN
                var token = await _tokenService.GenerateJwtTokenAsync(user);

                var tokenHandler = new JwtSecurityTokenHandler();
                var jwtToken = tokenHandler.ReadJwtToken(token);
                var jwtTokenId = jwtToken.Claims.FirstOrDefault(u => u.Type == JwtRegisteredClaimNames.Jti)?.Value;

                // generate new refresh token
                var newRefreshToken = await _tokenService.GenerateRefreshTokenAsync();
                var refreshTokenExpiry = DateTime.UtcNow.AddMinutes(5);

                await _tokenService.SaveRefreshTokenAsync(user.Id, jwtTokenId, newRefreshToken, refreshTokenExpiry);

                var roles = await _userManager.GetRolesAsync(user);
                var userDTO = _mapper.Map<UserDTO>(user);
                userDTO.Role = roles.FirstOrDefault() ?? string.Empty;

                TokenDTO tokenDTO = new TokenDTO
                {
                    AccessToken = token,
                    RefreshToken = newRefreshToken,
                    ExpiresAt = jwtToken.ValidTo,
                    UserDTO = userDTO
                };

                return tokenDTO;
            }

            catch (Exception ex)
            {
                // Handle any other unexpected errors
                throw new InvalidOperationException("An unexpected error occured during user login", ex);
            }
        }

        public async Task<UserDTO?> RegisterAsync(RegistrationRequestDTO registrationRequestDTO)
        {
            try 
            {  
                if (await IsEmailExistsAsync(registrationRequestDTO.Email))
                {
                    throw new InvalidOperationException($"User with email '{registrationRequestDTO.Email}' already exists");
                }

                ApplicationUser user = new()
                {
                    Email = registrationRequestDTO.Email,
                    Name = registrationRequestDTO.Name,
                    UserName = registrationRequestDTO.Email,
                    NormalizedEmail = registrationRequestDTO.Email.ToUpper(),
                    EmailConfirmed = true
                };

                var result = await _userManager.CreateAsync(user, registrationRequestDTO.Password);
                if(!result.Succeeded)
                {
                    var errors= string.Join(",", result.Errors.Select(e => e.Description));
                    throw new InvalidOperationException($"User registration failed: {errors}"); 
                }

                var role = string.IsNullOrEmpty(registrationRequestDTO.Role) ? "Customer" : registrationRequestDTO.Role;

                if(!await _roleManager.RoleExistsAsync(role))
                {
                    await _roleManager.CreateAsync(new IdentityRole(role));
                }

                await _userManager.AddToRoleAsync(user, role);

                var userDTO = _mapper.Map<UserDTO>(user);
                userDTO.Role = role;

                return userDTO;
            }

            catch(Exception ex)
            {
                throw new InvalidOperationException("An unexpected error occured during user registration", ex);
            }
        }

        public async Task<TokenDTO?> RefreshAccessTokenAsync(RefreshTokenRequestDTO refreshTokenRequestDTO)
        {
            try
            {
                if (string.IsNullOrEmpty(refreshTokenRequestDTO.RefreshToken))
                {
                    return null;
                }

                //validate refresh token
                var (isValid, userId, tokenFamilyId, tokenReused) = await _tokenService.ValidateRefreshTokenAsync(refreshTokenRequestDTO.RefreshToken);

                // Token reuse Detection
                if(tokenReused)
                {
                    return null;
                }

                // token is invalid or expired 
                if(!isValid || string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(tokenFamilyId))
                {
                    return null;
                }
                // get user  
                var user = await _db.ApplicationUsers.FindAsync(userId);
                if(user == null)
                {
                    return null;
                }

                // revoke old refresh token 
                await _tokenService.RevokeRefreshTokenAsync(refreshTokenRequestDTO.RefreshToken);

                // generate new access & refresh token 
                // generate TOKEN
                var token = await _tokenService.GenerateJwtTokenAsync(user);

                var tokenHandler = new JwtSecurityTokenHandler();
                var jwtToken = tokenHandler.ReadJwtToken(token);

                // generate new refresh token
                var newRefreshToken = await _tokenService.GenerateRefreshTokenAsync();
                var refreshTokenExpiry = DateTime.UtcNow.AddMinutes(5);

                await _tokenService.SaveRefreshTokenAsync(user.Id, tokenFamilyId, newRefreshToken, refreshTokenExpiry);

                var roles = await _userManager.GetRolesAsync(user);
                var userDTO = _mapper.Map<UserDTO>(user);
                userDTO.Role = roles.FirstOrDefault() ?? string.Empty;

                TokenDTO tokenDTO = new TokenDTO
                {
                    AccessToken = token,
                    RefreshToken = newRefreshToken,
                    ExpiresAt = jwtToken.ValidTo,
                    UserDTO = userDTO
                };

                return tokenDTO;
            }

            catch (Exception ex)
            {
                throw new InvalidOperationException("An unexpected error occured during user registration", ex);
            }
        }
    }
}

