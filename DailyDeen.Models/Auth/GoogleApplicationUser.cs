using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DailyDeen.Models.Auth;

public class GoogleApplicationUser
{
    [Key]
    public string GoogleId { get; set; }
    public string GoogleEmail { get; set; }
    public string GoogleFirstName { get; set; }
    public string GoogleLastName { get; set; }
}
