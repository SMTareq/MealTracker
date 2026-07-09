using System.Security.Claims;

namespace MealExpenseTracker.Api.Services;

public static class ClaimsPrincipalExtensions
{
    public static int GetUserId(this ClaimsPrincipal principal)
    {
        var idClaim = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return int.Parse(idClaim ?? "0");
    }

    public static bool IsAdmin(this ClaimsPrincipal principal)
    {
        return principal.IsInRole("Admin");
    }
}
