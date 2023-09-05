using DailyDeen.DataAccess.Data;
using DailyDeen.DataAccess.Repo.IRepo.Auth;
using DailyDeen.Models.Auth;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace DailyDeen.Web.Controllers.Auth;

[Route("auth")]
[ApiController]
public class AuthController : ControllerBase
{
    private readonly IAuthRepo _authRepo;

    public AuthController(IAuthRepo authRepo)
    {
        _authRepo = authRepo;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegistrationRequest model)
    {
        if (ModelState.IsValid)
        {
            try
            {
                if (model.Password != model.ConfirmPassword)
                    return BadRequest("Password do not match.");

                var user = new ApplicationUser
                {
                    FirstName = model.FirstName,
                    Surname = model.Surname,
                    Email = model.Email,
                    DateOfBirth = model.DateOfBirth,
                    Gender = model.Gender,
                };

                var registerUser = await _authRepo.RegisterUserAsync(user, model.Password);

                if (registerUser != null)
                {
                    var token = _authRepo.GenerateJwtToken(registerUser);
                    return Ok(new { Token = token });
                }

                return BadRequest("User Registration failed");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        return BadRequest(ModelState);
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest model)
    {
        if (ModelState.IsValid)
        {
            if (model.Email == null && model.Password == null)
                return BadRequest("Please Enter Email And Password");

            var user = await _authRepo.LoginUserAsync(model.Email, model.Password);

            if (user != null)
            {
                var token = _authRepo.GenerateJwtToken(user);
                return Ok(new { Token = token });
            }

            return BadRequest("Invalid login credential");
        }

        return BadRequest(ModelState);
    }

    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassWord([FromBody] ResetPasswordRequest model)
    {
        if (ModelState.IsValid)
        {
            var result = await _authRepo.ResetPasswordAsync(model.Email, model.NewPassword);

            if (result)
                return Ok("Password reset successful");

            return BadRequest("Password reset failed");
        }
        return BadRequest(ModelState);
    }
}
