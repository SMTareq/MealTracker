using MealExpenseTracker.Api.Data;
using MealExpenseTracker.Api.DTOs;
using MealExpenseTracker.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MealExpenseTracker.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ReportsController : ControllerBase
{
    private readonly AppDbContext _db;

    public ReportsController(AppDbContext db)
    {
        _db = db;
    }

    // GET api/reports/monthly-summary?year=2026&month=7
    // User gets own summary; admin can pass userId for someone else's
    [HttpGet("monthly-summary")]
    public async Task<ActionResult<MonthlySummaryResponse>> GetMonthlySummary(
        [FromQuery] int year, [FromQuery] int month, [FromQuery] int? userId)
    {
        var currentUserId = User.GetUserId();
        var targetUserId = (User.IsAdmin() && userId.HasValue) ? userId.Value : currentUserId;

        var user = await _db.Users.FindAsync(targetUserId);
        if (user is null) return NotFound();

        var totalMeals = await _db.Meals
            .Where(m => m.UserId == targetUserId && m.Date.Year == year && m.Date.Month == month)
            .SumAsync(m => (decimal?)m.Count) ?? 0;

        var totalExpense = await _db.Expenses
            .Where(e => e.UserId == targetUserId && e.Date.Year == year && e.Date.Month == month)
            .SumAsync(e => (decimal?)e.Amount) ?? 0;

        var mealRate = totalMeals > 0 ? Math.Round(totalExpense / totalMeals, 2) : 0;

        return Ok(new MonthlySummaryResponse
        {
            UserId = targetUserId,
            UserName = user.FullName,
            Year = year,
            Month = month,
            TotalMeals = totalMeals,
            TotalExpense = totalExpense,
            MealRate = mealRate
        });
    }

    // Admin only: summary across all users for a given month, for the admin dashboard table
    [HttpGet("monthly-summary/all")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<IEnumerable<MonthlySummaryResponse>>> GetAllUsersMonthlySummary(
        [FromQuery] int year, [FromQuery] int month)
    {
        var users = await _db.Users.Where(u => u.IsActive).ToListAsync();
        var results = new List<MonthlySummaryResponse>();

        foreach (var user in users)
        {
            var totalMeals = await _db.Meals
                .Where(m => m.UserId == user.Id && m.Date.Year == year && m.Date.Month == month)
                .SumAsync(m => (decimal?)m.Count) ?? 0;

            var totalExpense = await _db.Expenses
                .Where(e => e.UserId == user.Id && e.Date.Year == year && e.Date.Month == month)
                .SumAsync(e => (decimal?)e.Amount) ?? 0;

            var mealRate = totalMeals > 0 ? Math.Round(totalExpense / totalMeals, 2) : 0;

            results.Add(new MonthlySummaryResponse
            {
                UserId = user.Id,
                UserName = user.FullName,
                Year = year,
                Month = month,
                TotalMeals = totalMeals,
                TotalExpense = totalExpense,
                MealRate = mealRate
            });
        }

        return Ok(results);
    }
}
