using System.ComponentModel.DataAnnotations;

namespace MealExpenseTracker.Api.DTOs;

public class MealCreateRequest
{
    [Required]
    public DateTime Date { get; set; }

    [Range(0, 10)]
    public decimal Count { get; set; } = 1;

    public bool IsGuest { get; set; } = false;

    [Range(0, 10)]
    public decimal GuestCount { get; set; } = 0;

    [MaxLength(250)]
    public string? Note { get; set; }
}

public class MealResponse
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public DateTime Date { get; set; }
    public decimal Count { get; set; }
    public string? Note { get; set; }
}

public class ExpenseCreateRequest
{
    [Required]
    public DateTime Date { get; set; }

    [Required, MaxLength(100)]
    public string Category { get; set; } = string.Empty;

    [Required, Range(0.01, double.MaxValue)]
    public decimal Amount { get; set; }

    [MaxLength(250)]
    public string? Note { get; set; }
}

public class ExpenseResponse
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public DateTime Date { get; set; }
    public string Category { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string? Note { get; set; }
}

public class MonthlySummaryResponse
{
    public int UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public int Year { get; set; }
    public int Month { get; set; }
    public decimal TotalMeals { get; set; }
    public decimal TotalExpense { get; set; }
    public decimal MealRate { get; set; } // TotalExpense / TotalMeals
}
