using System.ComponentModel.DataAnnotations;

namespace EcommercePWA.Models
{
    public class TrackingViewModel
    {
        public Order Order { get; set; } = null!;
        public List<TrackingStep> TrackingSteps { get; set; } = new List<TrackingStep>();
        public string CurrentStatus { get; set; } = string.Empty;
        public int CurrentStepIndex { get; set; }
        public DateTime? EstimatedDeliveryDate { get; set; }
        public string TrackingCode { get; set; } = string.Empty;
    }

    public class TrackingStep
    {
        public string Status { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Icon { get; set; } = string.Empty;
        public DateTime? CompletedDate { get; set; }
        public bool IsCompleted { get; set; }
        public bool IsActive { get; set; }
        public string Color { get; set; } = string.Empty;
    }
}
