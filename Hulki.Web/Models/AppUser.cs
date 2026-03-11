using Microsoft.AspNetCore.Identity;

namespace Hulki.Web.Models;

public class AppUser : IdentityUser
{
    public string FirstName { get; set; }
    public string LastName { get; set; }
    
    public virtual ICollection<PatientGroup> PatientGroups { get; set; }
}