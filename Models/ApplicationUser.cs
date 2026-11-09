using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace RecyclingApp.Models
{
    public class ApplicationUser : IdentityUser
    {
        [Required]
        public string Name { get; set; }

        public string Phone { get; set; }

        public string Role { get; set; }

        public string Address { get; set; }

        public ICollection<PickupRequest> PickupRequests { get; set; }
        public ICollection<PickupRequest> CollectedRequests { get; set; }
    }
}
