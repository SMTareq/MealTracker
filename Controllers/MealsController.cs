using MealExpenseTracker.Api.Data;
using MealExpenseTracker.Api.DTOs;
using MealExpenseTracker.Api.Models;
using MealExpenseTracker.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MealExpenseTracker.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class MealsController : ControllerBase
{
    private readonly AppDbContext _db;

    public MealsController(AppDbContext db)
    {
        _db = db;
    }

    // GET api/meals?month=7&year=2026
    // Regular users see only their own; admins can pass userId to view others'
    [HttpGet]
    public async Task<ActionResult<IEnumerable<MealResponse>>> GetMeals(
        [FromQuery] int? year, [FromQuery] int? month, [FromQuery] int? userId)
    {
        var currentUserId = User.GetUserId();
        var targetUserId = (User.IsAdmin() && userId.HasValue) ? userId.Value : currentUserId;

        var query = _db.Meals.Where(m => m.UserId == targetUserId);

        if (year.HasValue)
            query = query.Where(m => m.Date.Year == year.Value);
        if (month.HasValue)
            query = query.Where(m => m.Date.Month == month.Value);

        var meals = await query
            .OrderByDescending(m => m.Date)
            .Select(m => new MealResponse
            {
                Id = m.Id,
                UserId = m.UserId,
                Date = m.Date,
                Count = m.Count,
                GuestCount = m.GuestCount,
                Note = m.Note
            })
            .ToListAsync();

        return Ok(meals);
    }

    [HttpPost]
    public async Task<ActionResult<MealResponse>> CreateMeal(MealCreateRequest request)
    {
        var userId = User.GetUserId();

        var meal = new Meal
        {
            UserId = userId,
            Date = request.Date.ToUtc(),
            Count = request.Count,
            Note = request.Note,
            IsGuest = request.IsGuest,
            GuestCount = request.GuestCount,
            CreatedAt = DateTime.UtcNow
        };

        _db.Meals.Add(meal);
        await _db.SaveChangesAsync();

        return CreatedAtAction(nameof(GetMeals), new { }, new MealResponse
        {
            Id = meal.Id,
            UserId = meal.UserId,
            Date = meal.Date,
            Count = meal.Count,
            IsGuest = meal.IsGuest,
            GuestCount = meal.GuestCount,   
            Note = meal.Note
        });
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateMeal(int id, MealCreateRequest request)
    {
        var userId = User.GetUserId();
        var meal = await _db.Meals.FirstOrDefaultAsync(m => m.Id == id);

        if (meal is null) return NotFound();
        if (meal.UserId != userId && !User.IsAdmin()) return Forbid();

        meal.Date = request.Date.ToUtc();
        meal.Count = request.Count;
        meal.Note = request.Note;

        await _db.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteMeal(int id)
    {
        var userId = User.GetUserId();
        var meal = await _db.Meals.FirstOrDefaultAsync(m => m.Id == id);

        if (meal is null) return NotFound();
        if (meal.UserId != userId && !User.IsAdmin()) return Forbid();

        _db.Meals.Remove(meal);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}
