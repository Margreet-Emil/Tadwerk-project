using System.Collections.Generic;
using RecyclingApp.Models;

namespace RecyclingApp.ViewModels
{
    public class HomeViewModel
    {
        public List<WasteType> WasteTypes { get; set; } = new List<WasteType>();
        public decimal TotalKg { get; set; }
        public int CompletedRequests { get; set; }
        public int Collectors { get; set; }
        public int Customers { get; set; }
    }
}
