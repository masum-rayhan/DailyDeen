using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DailyDeen.Models.Auth;

public class ApplicationUser
{
    public int Id { get; set; }
    public string FirstName { get; set; }
    public string Surname { get; set; }
    public string Email { get; set; }
    public string PasswordHash { get; set; }
    public DateTime DateOfBirth { get; set; }
    public string Gender { get; set; }
}
