using Microsoft.AspNetCore.Identity;

namespace Hulki.Web.Models;

public class AppUser : IdentityUser
{
    public string FirstName { get; set; }
    public string LastName { get; set; }
    
    public virtual ICollection<PatientGroup> PatientGroups { get; set; }
    
    public virtual Wallet Wallet { get; set; } // Relacja 1 do 1 z portfelem
    public virtual ICollection<DailyReport> DailyReports { get; set; } // Wpisy pacjenta
    public virtual ICollection<PatientInventory> InventoryItems { get; set; } // Ekwipunek pacjenta
}