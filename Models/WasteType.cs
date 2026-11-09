using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace RecyclingApp.Models
{
    public class WasteType
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }

        public decimal PricePerKg { get; set; }

        public ICollection<RequestItem> RequestItems { get; set; }
    }
}
