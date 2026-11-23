
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SubWatch.Api.data;
using SubWatch.Api.DTOs;
using SubWatch.Api.models;
using SubWatch.Api.services;

namespace SubWatch.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly TokenService _tokenService;
    private readonly AppDbContext _db;
    public AuthController(UserManager<ApplicationUser> userManager, TokenService tokenService, AppDbContext appDbContext)
    {
        _userManager = userManager;
        _tokenService = tokenService;
        _db = appDbContext;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterDto dto)
    {
        var user = new ApplicationUser
        {
            UserName = dto.Email,
            Email = dto.Email
        };

        var rez = await _userManager.CreateAsync(user, dto.Password);

        if (!rez.Succeeded)
        {
            return BadRequest(rez.Errors);
        }

        return Ok(new {Message = "User registered successfully."});
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginDto dto)
    {
        var user = await _userManager.FindByEmailAsync(dto.Email);

        if(user == null)
        {
            return Unauthorized(new { Message = "Invalid email or password."});
        }
        
        var checkPassword = await _userManager.CheckPasswordAsync(user!, dto.Password);


        if (!checkPassword)
        {
            return Unauthorized(new {Message = "Invalid email or password."});
        }

        var token = _tokenService.GenerateToken(user);
        var refreshToken = await _tokenService.CreateRefreshTokenAsync(user);

        return Ok(new {Token = token, RefreshToken=refreshToken.Token});
    }

    [Authorize]
    [HttpPut("update")]
    public async Task<IActionResult> UpdateProfileInfo(UpdateProfileDto dto,[FromQuery] string email)
    {

        var user = await _userManager.FindByEmailAsync(email);

        if(user == null)
        {
            return Unauthorized(new {Message="could not udpate profile information."});
        }

        user.Email = !string.IsNullOrWhiteSpace(dto.Email) ? dto.Email : email ;
        user.FullName = dto.FullName;

        await _userManager.UpdateAsync(user);

        return Ok(new
        {
            dto.FullName,
            dto.Email
        });
    }

    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh(RefreshTokenDto dto)
    {
        var storedToken =  await _db.RefreshToken.FirstOrDefaultAsync(rt => rt.Token == dto.RefreshToken);

        if(storedToken == null || storedToken.isRevoked || storedToken.ExpiresAt <= DateTime.UtcNow)
        {
            return Unauthorized(new{message="Invalid or expired refresh token."});
        }

        var user = await _userManager.FindByIdAsync(storedToken.UserId);

        if(user == null)
        {
            return Unauthorized(new {message ="User not found."});
        }
        //Revoke
        storedToken.isRevoked = true;

        //Generate new tokens
        var newAccessToken = _tokenService.GenerateToken(user);
        var newRefreshToken = await _tokenService.CreateRefreshTokenAsync(user);
        return Ok(new
        {
            Token = newAccessToken,
            RefreshToken = newRefreshToken.Token
        });
    }

    [Authorize]
    [HttpDelete("delete-account")]
    public async Task<IActionResult> DeleteAccount([FromBody] DeleteAccountDto deleteAccountDto)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier!);

        if(string.IsNullOrEmpty(userId)) return Unauthorized();

        var user = await _userManager.FindByIdAsync(userId);

        if(user == null) return NotFound(new {message="Account not found."});

        var passwordValid = await _userManager.CheckPasswordAsync(user, deleteAccountDto.Password);

        if(!passwordValid) return BadRequest(new {message = "Incorrect password."});

        var result =  await _userManager.DeleteAsync(user);

        if(!result.Succeeded) return BadRequest(new {message = "Unable to delete account.",errors=result.Errors.Select(e=>e.Description)});

        return Ok(new {message="Account deleted successfully."});
    }

}

