using DailyDeen.Models.Auth;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DailyDeen.DataAccess.Repo.IRepo.Auth;

public interface IAuthRepo
{
    Task<ApplicationUser> RegisterUserAsync(ApplicationUser user, string password);
    Task<ApplicationUser> LoginUserAsync(string email, string password);
    Task<bool> UserExistsAsync(string email);
    Task<bool> ResetPasswordAsync(string email, string newPasswor);
    string GenerateJwtToken(ApplicationUser user);
}
