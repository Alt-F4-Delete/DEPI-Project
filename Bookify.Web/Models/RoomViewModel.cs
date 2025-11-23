using System.ComponentModel.DataAnnotations;

namespace Bookify.Web.Models;

public class RoomViewModel
{
    public int? Id { get; set; }

    [Required(ErrorMessage = "Room number is required")]
    [StringLength(50, ErrorMessage = "Room number cannot exceed 50 characters")]
    public string RoomNumber { get; set; } = string.Empty;

    [Required(ErrorMessage = "Room type is required")]
    [Display(Name = "Room Type")]
    public int RoomTypeId { get; set; }

    [Display(Name = "Available")]
    public bool IsAvailable { get; set; } = true;

    [StringLength(500)]
    public string? Notes { get; set; }
}

