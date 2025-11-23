using System.ComponentModel.DataAnnotations;

namespace Bookify.Web.Models;

public class RoomTypeViewModel
{
    public int? Id { get; set; }

    [Required(ErrorMessage = "Name is required")]
    [StringLength(100, ErrorMessage = "Name cannot exceed 100 characters")]
    public string Name { get; set; } = string.Empty;

    [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters")]
    public string? Description { get; set; }

    [Required(ErrorMessage = "Price per night is required")]
    [Range(0.01, double.MaxValue, ErrorMessage = "Price must be greater than 0")]
    public decimal PricePerNight { get; set; }

    [Required(ErrorMessage = "Max occupancy is required")]
    [Range(1, int.MaxValue, ErrorMessage = "Max occupancy must be at least 1")]
    public int MaxOccupancy { get; set; }

    [StringLength(200)]
    public string? ImageUrl { get; set; }

    public bool IsActive { get; set; } = true;
}

