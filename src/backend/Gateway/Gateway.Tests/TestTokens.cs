namespace Gateway.Tests;

/// <summary>Tokens assinados com uma chave de teste, no lugar dos do Keycloak (mesmo emissor e audience).</summary>
public static class TestTokens
{
    public const string Issuer = "http://localhost:8080/realms/scorponok";

    public static readonly SymmetricSecurityKey Key = new(Encoding.UTF8.GetBytes("chave-de-teste-do-gateway-com-32-bytes!!"));

    public static string Create(string audience = "gateway-api", DateTime? expires = null)
        => new JsonWebTokenHandler().CreateToken(new SecurityTokenDescriptor
        {
            Issuer = Issuer,
            Audience = audience,
            Expires = expires ?? DateTime.UtcNow.AddMinutes(5),
            NotBefore = (expires ?? DateTime.UtcNow.AddMinutes(5)).AddMinutes(-10),
            Subject = new ClaimsIdentity([
                new Claim("sub", "6f1c2c35-3c4b-4f5e-9a51-2b0d7a3e9c11"), new Claim("email", "cliente@exemplo.com"),
                new Claim("email_verified", "true")
            ]),
            SigningCredentials = new SigningCredentials(Key, SecurityAlgorithms.HmacSha256)
        });
}
