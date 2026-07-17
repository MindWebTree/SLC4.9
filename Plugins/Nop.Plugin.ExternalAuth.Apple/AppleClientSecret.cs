using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using Microsoft.IdentityModel.Tokens;

public static class AppleClientSecret
{
    public static string Generate(string teamId, string clientId, string keyId, string privateKey)
    {
        privateKey = privateKey.Replace("\\n", "\n").Trim();
        using var ecdsa = ECDsa.Create();
        var pkcs8 = Convert.FromBase64String(privateKey);
        ecdsa.ImportPkcs8PrivateKey(pkcs8, out _);

        var securityKey = new ECDsaSecurityKey(ecdsa) { KeyId = keyId };

        // Important: include the 'sub' claim as the clientId
        var claims = new[]
        {
    new Claim(JwtRegisteredClaimNames.Sub, clientId)
};

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Issuer = teamId,
            Audience = "https://appleid.apple.com",
            Expires = DateTime.UtcNow.AddMonths(5),
            NotBefore = DateTime.UtcNow,
            Subject = new ClaimsIdentity(claims),
            SigningCredentials = new SigningCredentials(securityKey, SecurityAlgorithms.EcdsaSha256)
        };

        var handler = new JwtSecurityTokenHandler();
        var securityToken = handler.CreateToken(tokenDescriptor);
        return handler.WriteToken(securityToken);
    }
}
