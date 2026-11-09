using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RecyclingApp.Models
{
    public class RequestItem
    {
        [Key]
        public int Id { get; set; }

        public int RequestId { get; set; }

        public int WasteTypeId { get; set; }

        public decimal QuantityKg { get; set; }

        [ForeignKey(nameof(RequestId))]
        public PickupRequest PickupRequest { get; set; }

        [ForeignKey(nameof(WasteTypeId))]
        public WasteType WasteType { get; set; }
    }
}
