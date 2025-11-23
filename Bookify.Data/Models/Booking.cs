using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Bookify.Data.Models;

public class Booking
{
    public int Id { get; set; }
    
    [Required]
    public string UserId { get; set; } = string.Empty;
    
    [Required]
    public int RoomId { get; set; }
    
    [Required]
    public DateTime CheckInDate { get; set; }
    
    [Required]
    public DateTime CheckOutDate { get; set; }
    
    [Required]
    [Range(0.01, double.MaxValue)]
    public decimal TotalPrice { get; set; }
    
    [Required]
    [StringLength(50)]
    public string Status { get; set; } = "Pending"; // Pending, Confirmed, Cancelled, Completed
    
    [StringLength(500)]
    public string? SpecialRequests { get; set; }
    
    [StringLength(200)]
    public string? StripePaymentIntentId { get; set; }
    
    [StringLength(200)]
    public string? StripeSessionId { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public DateTime? ConfirmedAt { get; set; }
    
    // Navigation properties
    [ForeignKey(nameof(UserId))]
    public virtual ApplicationUser User { get; set; } = null!;
    
    [ForeignKey(nameof(RoomId))]
    public virtual Room Room { get; set; } = null!;
}

