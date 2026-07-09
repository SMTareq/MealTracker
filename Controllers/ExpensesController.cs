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
public class ExpensesController : ControllerBase
{
    private readonly AppDbContext _db;

    public ExpensesController(AppDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<ExpenseResponse>>> GetExpenses(
        [FromQuery] int? year, [FromQuery] int? month, [FromQuery] int? userId)
    {
        var currentUserId = User.GetUserId();
        var targetUserId = (User.IsAdmin() && userId.HasValue) ? userId.Value : currentUserId;

        var query = _db.Expenses.Where(e => e.UserId == targetUserId);

        if (year.HasValue)
            query = query.Where(e => e.Date.Year == year.Value);
        if (month.HasValue)
            query = query.Where(e => e.Date.Month == month.Value);

        var expenses = await query
            .OrderByDescending(e => e.Date)
            .Select(e => new ExpenseResponse
            {
                Id = e.Id,
                UserId = e.UserId,
                Date = e.Date,
                Category = e.Category,
                Amount = e.Amount,
                Note = e.Note
            })
            .ToListAsync();

        return Ok(expenses);
    }

    [HttpPost]
    public async Task<ActionResult<ExpenseResponse>> CreateExpense(ExpenseCreateRequest request)
    {
        var userId = User.GetUserId();

        var expense = new Expense
        {
            UserId = userId,
            Date = request.Date.ToUtc(),
            Category = request.Category,
            Amount = request.Amount,
            Note = request.Note
        };

        _db.Expenses.Add(expense);
        await _db.SaveChangesAsync();

        return CreatedAtAction(nameof(GetExpenses), new { }, new ExpenseResponse
        {
            Id = expense.Id,
            UserId = expense.UserId,
            Date = expense.Date,
            Category = expense.Category,
            Amount = expense.Amount,
            Note = expense.Note
        });
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateExpense(int id, ExpenseCreateRequest request)
    {
        var userId = User.GetUserId();
        var expense = await _db.Expenses.FirstOrDefaultAsync(e => e.Id == id);

        if (expense is null) return NotFound();
        if (expense.UserId != userId && !User.IsAdmin()) return Forbid();

        expense.Date = request.Date.ToUtc();
        expense.Category = request.Category;
        expense.Amount = request.Amount;
        expense.Note = request.Note;

        await _db.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteExpense(int id)
    {
        var userId = User.GetUserId();
        var expense = await _db.Expenses.FirstOrDefaultAsync(e => e.Id == id);

        if (expense is null) return NotFound();
        if (expense.UserId != userId && !User.IsAdmin()) return Forbid();

        _db.Expenses.Remove(expense);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}
