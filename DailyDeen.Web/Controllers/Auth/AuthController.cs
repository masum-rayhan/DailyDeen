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

    [HttpGet("google-response")]
    public async Task<IActionResult> GoogleResponse()
    {
        var authResult = await HttpContext.AuthenticateAsync("Google");

        // Check if the authentication was successful
        if (authResult.Succeeded)
        {
            // Access user information from Google response
            var googleId = authResult.Principal.FindFirstValue(ClaimTypes.NameIdentifier);
            var googleEmail = authResult.Principal.FindFirstValue(ClaimTypes.Email);
            var googleFirstName = authResult.Principal.FindFirstValue(ClaimTypes.GivenName);
            var googleLastName = authResult.Principal.FindFirstValue(ClaimTypes.Surname);

            // Find or create the user in your database
            var user = await _authRepo.FindUserByGoogleIdAsync(googleId);
            if (user == null)
            {
                user = new ApplicationUser
                {
                    // Map Google data to ApplicationUser properties
                    GoogleId = googleId,
                    GoogleEmail = googleEmail,
                    GoogleFirstName = googleFirstName,
                    GoogleLastName = googleLastName,

                    // Other properties (if applicable)
                };

                // Create the user in your database
                await _authRepo.RegisterUserAsync(user, null); // Use a placeholder password or null
            }

            // Log the user in
            await _authRepo.LoginUserAsync(user.Email, null); // Use a placeholder password or null
        }

        return RedirectToAction("Home"); // Redirect to your application's home page
    }
    [HttpGet("google-login")]
    public IActionResult GoogleLogin()
    {
        var properties = new AuthenticationProperties
        {
            RedirectUri = Url.Action("GoogleResponse", "Auth"),
        };

        return Challenge(properties, "Google");
    }
}
