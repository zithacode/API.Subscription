

using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using SubWatch.Api.data;
using SubWatch.Api.models;

namespace SubWatch.Api.services;

public class TokenService
{
    private readonly IConfiguration _configuration;
    private readonly AppDbContext _db;

    public TokenService(IConfiguration configuration,AppDbContext appDbContext)
    {
        _configuration = configuration;
        _db = appDbContext;
    }

    public string GenerateToken(ApplicationUser user)
    {
        //ckst
        //Claims
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id),
            new Claim(ClaimTypes.Email, user.Email!),
            new Claim("fullName", user.FullName!)
        };

        //Key
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!));
        //Sign
        var credentials = new SigningCredentials(key,SecurityAlgorithms.HmacSha256);
        //Token
        var token = new JwtSecurityToken(

            issuer: _configuration["Jwt:Issuer"],
            audience: _configuration["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(Convert.ToDouble(_configuration["Jwt:ExpiryInMinutes"])),
            signingCredentials:credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
    public async Task<RefreshToken> CreateRefreshTokenAsync(ApplicationUser user)
    {
        var refreshToken = new RefreshToken
        {
            Token =  GenerateRefreshToken(),
            UserId = user.Id,
            ExpiresAt = DateTime.UtcNow.AddDays(Convert.ToDouble(_configuration["Jwt:RefreshTokenExpiryInDays"]))
        };

        _db.RefreshToken.Add(refreshToken);

        await _db.SaveChangesAsync();

       return new RefreshToken
        {
            Id = refreshToken.Id,
            Token = refreshToken.Token,
            ExpiresAt = refreshToken.ExpiresAt,
            User = new ApplicationUser
            {
                Id = refreshToken.User.Id,
                Email = refreshToken.User.Email,
                FullName = refreshToken.User.FullName
            }
        };
    }

    public string GenerateRefreshToken()
    {
        var randomBytes = RandomNumberGenerator.GetBytes(64);

        return Convert.ToBase64String(randomBytes);
    }
}