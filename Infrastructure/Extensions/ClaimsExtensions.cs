using System.Security.Claims;

public static class ClaimsExtensions
{
    public static Guid GetUserId(this ClaimsPrincipal user)
    {
        var claim =
            user.FindFirst(ClaimTypes.NameIdentifier)
            ?? user.FindFirst("sub");

        if (claim == null)
            throw new UnauthorizedAccessException("UserId claim not found");

        return Guid.Parse(claim.Value);
    }
}
