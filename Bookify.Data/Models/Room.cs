using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Bookify.Data.Models;

public class Room
{
    public int Id { get; set; }
    
    [Required]
    [StringLength(50)]
    public string RoomNumber { get; set; } = string.Empty;
    
    [Required]
    public int RoomTypeId { get; set; }
    
    [Required]
    public bool IsAvailable { get; set; } = true;
    
    [StringLength(500)]
    public string? Notes { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    // Navigation properties
    [ForeignKey(nameof(RoomTypeId))]
    public virtual RoomType RoomType { get; set; } = null!;
    
    public virtual ICollection<Booking> Bookings { get; set; } = new List<Booking>();
}

