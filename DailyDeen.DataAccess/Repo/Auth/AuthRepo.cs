using DailyDeen.DataAccess.Data;
using DailyDeen.DataAccess.Repo.IRepo.Auth;
using DailyDeen.Models.Auth;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Reflection.Metadata;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace DailyDeen.DataAccess.Repo.Auth;

public class AuthRepo : IAuthRepo
{
    private readonly AppDbContext _db;
    private readonly IConfiguration _config;
    public AuthRepo(AppDbContext db, IConfiguration config)
    {
        _db = db;
        _config = config;
    }
    
    public async Task<ApplicationUser> LoginUserAsync(string email, string password)
    {
        if(email == null || password == null)
            return null;

        var user =  await _db.Users.FirstOrDefaultAsync(u => u.Email == email);

        if(user != null && BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
            return user;

        return null;
    }

    public async Task<ApplicationUser> RegisterUserAsync(ApplicationUser user, string password)
    {
        if(user == null || password == null)
            return null;

        // Check if a user with the same email exists
        if (await UserExistsAsync(user.Email))
            throw new ApplicationException("Email address is already registered.");

        var hashedPassword = BCrypt.Net.BCrypt.HashPassword(password);
        user.PasswordHash = hashedPassword;

        // Add the user to the database
        await _db.Users.AddAsync(user);
        await _db.SaveChangesAsync();

        return user;

    }

    public async Task<bool> ResetPasswordAsync(string email, string newPasswor)
    {
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == email);

        if (user == null && newPasswor == null)
            return false;

        // Update the user's password with the new password
        var hashedPassword = BCrypt.Net.BCrypt.HashPassword(newPassword);
        user.PasswordHash = hashedPassword;

        // Save changes to the database
        var result = await _db.SaveChangesAsync();

        return result > 0; // Password reset successful if changes were saved
    }

    public async Task<bool> UserExistsAsync(string email)
    {
        return await _db.Users.AnyAsync(u => u.Email == email);
    }

    public string GenerateJwtToken(ApplicationUser user)
    {
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Email, user.Email),
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["JwtSettings:Key"]));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.Now.AddMinutes(10),
            SigningCredentials = creds,
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        var token = tokenHandler.CreateToken(tokenDescriptor);

        return tokenHandler.WriteToken(token);
    }
}
