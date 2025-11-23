using System.ComponentModel.DataAnnotations;

namespace Bookify.Data.Models;

public class RoomType
{
    public int Id { get; set; }
    
    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;
    
    [StringLength(500)]
    public string? Description { get; set; }
    
    [Required]
    [Range(0.01, double.MaxValue, ErrorMessage = "Price must be greater than 0")]
    public decimal PricePerNight { get; set; }
    
    [Range(1, int.MaxValue)]
    public int MaxOccupancy { get; set; }
    
    [StringLength(200)]
    public string? ImageUrl { get; set; }
    
    public bool IsActive { get; set; } = true;
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    // Navigation property
    public virtual ICollection<Room> Rooms { get; set; } = new List<Room>();
}

