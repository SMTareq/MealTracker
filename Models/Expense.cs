using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MealExpenseTracker.Api.Models;

public class Expense
{
    public int Id { get; set; }

    [Required]
    public int UserId { get; set; }
    public User? User { get; set; }

    [Required]
    public DateTime Date { get; set; }

    public DateTime CountDate { get; set; }

    [Required, MaxLength(100)]
    public string Category { get; set; } = string.Empty; // e.g. Groceries, Utility, Rent

    [Required]
    [Column(TypeName = "decimal(10,2)")]
    public decimal Amount { get; set; }

    [MaxLength(250)]
    public string? Note { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
