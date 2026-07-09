using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MealExpenseTracker.Api.Models;

public class Meal
{
    public int Id { get; set; }

    [Required]
    public int UserId { get; set; }
    public User? User { get; set; }

    [Required]
    public DateTime Date { get; set; }

    // e.g. 0.5 = half meal, 1 = full meal, 2 = two meals that day
    [Column(TypeName = "decimal(4,2)")]
    public decimal Count { get; set; } = 1;
    public bool IsGuest { get; set; } = false;
    
    // e.g. 0.5 = half meal, 1 = full meal, 2 = two meals that day
    [Column(TypeName = "decimal(4,2)")]
    public decimal GuestCount { get; set; } = 0;

    [MaxLength(250)]
    public string? Note { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
