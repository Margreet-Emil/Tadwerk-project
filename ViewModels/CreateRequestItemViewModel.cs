
namespace RecyclingApp.ViewModels
{
    public class CreateRequestItemViewModel
    {
        public int WasteTypeId { get; set; }

        public string WasteTypeName { get; set; } = string.Empty;

        public decimal PricePerKg { get; set; }

        public decimal QuantityKg { get; set; }

        public bool IsSelected { get; set; }
    }
}