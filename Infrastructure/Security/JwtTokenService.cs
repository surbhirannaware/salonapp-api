using Microsoft.IdentityModel.Tokens;
using SalonApp.Domain.Entities;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

public class JwtTokenService
{
    private readonly IConfiguration _config;

    public JwtTokenService(IConfiguration config)
    {
        _config = config;
    }

    public string GenerateToken(User user, List<string> roles)
    {
        var jwt = _config.GetSection("Jwt");

        var keyValue = jwt["Key"];
        var issuer = jwt["Issuer"];
        var audience = jwt["Audience"];
        var expiryValue = jwt["ExpiryMinutes"];

        if (string.IsNullOrWhiteSpace(keyValue))
            throw new Exception("JWT Key is missing");

        if (string.IsNullOrWhiteSpace(issuer))
            throw new Exception("JWT Issuer is missing");

        if (string.IsNullOrWhiteSpace(audience))
            throw new Exception("JWT Audience is missing");

        if (!int.TryParse(expiryValue, out int expiryMinutes))
            throw new Exception("JWT ExpiryMinutes is missing or invalid. Value = " + expiryValue);

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(keyValue));

        var claims = new List<Claim>
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.UserId.ToString()),
            new Claim("name", user.FullName ?? "")
        };

        if (!string.IsNullOrWhiteSpace(user.Email))
        {
            claims.Add(new Claim(JwtRegisteredClaimNames.Email, user.Email));
        }

        foreach (var role in roles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(expiryMinutes),
            signingCredentials: new SigningCredentials(
                key,
                SecurityAlgorithms.HmacSha256
            )
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}