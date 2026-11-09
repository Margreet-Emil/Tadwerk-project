using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using RecyclingApp.Models;

namespace RecyclingApp.ViewModels
{
    public class CreatePickupRequestViewModel
    {
        [Required]
        public DateTime ScheduledAt { get; set; }

        [Range(typeof(bool), "true", "true", ErrorMessage = "You must agree to the Accuracy & Privacy Policy before submitting.")]
        public bool AcceptPolicy { get; set; }

        public List<CreateRequestItemViewModel> Items { get; set; } = new List<CreateRequestItemViewModel>();

        public List<WasteType> AvailableWasteTypes { get; set; } = new List<WasteType>();
    }
}
