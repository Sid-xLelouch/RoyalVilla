using Asp.Versioning;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using RoyalVilla.DTO;
using RoyalVilla_API.Services.IServices;

namespace RoyalVilla_API.Controllers
{
    [Route("api/auth")]
    [ApiVersionNeutral]
    [ApiController]
    public class AuthController(IAuthService authService) : ControllerBase
    {
        private readonly IAuthService _authService = authService;

        [HttpPost("register")]
        [ProducesResponseType(typeof(ApiResponse<UserDTO>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        public async Task<ActionResult<ApiResponse<UserDTO>>> Register([FromBody]RegistrationRequestDTO registrationRequestDTO)
        {
            try
            {
                if (registrationRequestDTO == null)
                {
                    return BadRequest(ApiResponse<object>.BadRequest("Registration data is required"));
                }

                if (await _authService.IsEmailExistsAsync(registrationRequestDTO.Email))
                {
                    return Conflict(ApiResponse<object>.Conflict($"User with email '{registrationRequestDTO.Email}' already exisits"));
                }

                var user = await _authService.RegisterAsync(registrationRequestDTO);

                if (user == null)
                {
                    return BadRequest(ApiResponse<object>.BadRequest("Registration failed"));
                }

                // auth service 
                var response = ApiResponse<UserDTO>.CreatedAt(user, "User registered successfully");
                return CreatedAtAction(nameof(Register), response);
            }

            catch (Exception ex)
            {
                var errorResponse = ApiResponse<object>.Error(500, "An error occured during registration", ex.Message);
                return StatusCode(500, errorResponse);
            }
        }


        [HttpPost("login")]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<LoginRequestDTO>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ApiResponse<TokenDTO>>> Login([FromBody] LoginRequestDTO loginRequestDTO)
        {
            try
            {
                if (loginRequestDTO == null)
                {
                    return BadRequest(ApiResponse<object>.BadRequest("Login data is required"));
                }

                var loginResponse = await _authService.LoginAsync(loginRequestDTO);

                if (loginResponse == null)
                {
                    return BadRequest(ApiResponse<object>.BadRequest("Login failed. Please check your credentials."));
                }

                // auth service 
                var response = ApiResponse<TokenDTO>.Ok(loginResponse, "Login successful");
                return Ok(response);
            }

            catch (Exception ex)
            {
                var errorResponse = ApiResponse<object>.Error(500, "An error occured during login", ex.Message);
                return StatusCode(500, errorResponse);
            }
        }


        [HttpPost("refresh-token")]
        [ProducesResponseType(typeof(ApiResponse<UserDTO>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        public async Task<ActionResult<ApiResponse<UserDTO>>> RefreshAccessToken([FromBody] RefreshTokenRequestDTO refreshTokenRequestDTO)
        {
            try
            {
                if (refreshTokenRequestDTO == null || String.IsNullOrEmpty(refreshTokenRequestDTO.RefreshToken))
                {
                    return BadRequest(ApiResponse<object>.BadRequest("Refresh Token is required"));
                }

                var tokenResponse = await _authService.RefreshAccessTokenAsync(refreshTokenRequestDTO);

                if (tokenResponse == null)
                {
                    // Token reuse or invalid token - log for security monitoring
                    var errorResponse = ApiResponse<object>.Error(401, "Invalid or expired refresh token. If token reuse was detected" +
                        ", all your sessions have been terminated");

                    return Unauthorized(errorResponse);
                }

                // auth service 
                var response = ApiResponse<TokenDTO>.Ok(tokenResponse, "Token refreshed successfully");
                return Ok(response);
            }

            catch (Exception ex)
            {
                var errorResponse = ApiResponse<object>.Error(500, "An error occured during token refresh", ex.Message);
                return StatusCode(500, errorResponse);
            }
        }
    }
}
