using System.Collections.Generic;
using RecyclingApp.Models;

namespace RecyclingApp.ViewModels
{
    public class DashboardViewModel
    {
        public int TotalRequests { get; set; }
        public int PendingRequests { get; set; }
        public int CompletedRequests { get; set; }
        
        public int RejectedRequests { get; set; }
        public int FailedRequests { get; set; }
        public int InProgressRequests { get; set; }

        public decimal CompletedValue { get; set; }
        public decimal InProgressValue { get; set; }
        public decimal PendingValue { get; set; }
        public decimal RejectedValue { get; set; }
        public decimal FailedValue { get; set; }
        public decimal CompletedWeightKg { get; set; }

        public decimal TotalWeightKg { get; set; }
        public decimal TotalRevenue { get; set; }
        
        public List<PickupRequest> RecentRequests { get; set; } = new List<PickupRequest>();
        
        public int UserCount { get; set; }
        public int CollectorCount { get; set; }
    }
}
