using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
#if NET8_0_OR_GREATER
using Microsoft.IdentityModel.JsonWebTokens;

#else
using System.IdentityModel.Tokens.Jwt;
#endif

namespace FastEndpoints.Security;

/// <summary>
/// static class for easy creation of jwt bearer tokens
/// </summary>
public static class JwtBearer
{
    /// <summary>
    /// generates jwt tokens with supplied settings.
    /// </summary>
    /// <param name="options">action to configure jwt creation options.</param>
    /// <exception cref="InvalidOperationException">thrown if a token signing key is not supplied.</exception>
    public static string CreateToken(Action<JwtCreationOptions> options)
    {
        //TODO: remove all other overloads in favor of this at v6.0

        var opts = new JwtCreationOptions();
        options(opts);

        if (string.IsNullOrEmpty(opts.SigningKey))
            throw new InvalidOperationException($"{nameof(JwtCreationOptions.SigningKey)} is required!");

        var claimList = new List<Claim>();

        if (opts.User.Claims.Any())
            claimList.AddRange(opts.User.Claims);

        if (opts.User.Permissions.Any())
            claimList.AddRange(opts.User.Permissions.Select(p => new Claim(Conf.SecOpts.PermissionsClaimType, p)));

        if (opts.User.Roles.Any())
            claimList.AddRange(opts.User.Roles.Select(r => new Claim(Conf.SecOpts.RoleClaimType, r)));

        var descriptor = new SecurityTokenDescriptor
        {
            Issuer = opts.Issuer,
            Audience = opts.Audience,
            IssuedAt = (Conf.ServiceResolver.TryResolve<TimeProvider>() ?? TimeProvider.System).GetUtcNow().UtcDateTime,
            Subject = new(claimList),
            Expires = opts.ExpireAt,
            SigningCredentials = GetSigningCredentials(opts)
        };

    #if NET8_0_OR_GREATER
        var handler = new JsonWebTokenHandler();

        return handler.CreateToken(descriptor);
    #else
        var handler = new JwtSecurityTokenHandler();

        return handler.WriteToken(handler.CreateToken(descriptor));
    #endif

        static SigningCredentials GetSigningCredentials(JwtCreationOptions opts)
        {
            // ReSharper disable once InvertIf
            if (opts.SigningStyle == TokenSigningStyle.Asymmetric)
            {
                var rsa = RSA.Create(); // don't dispose this
                if (opts.KeyIsPemEncoded)
                    rsa.ImportFromPem(opts.SigningKey);
                else
                    rsa.ImportRSAPrivateKey(Convert.FromBase64String(opts.SigningKey), out _);

                return new(new RsaSecurityKey(rsa), opts.AsymmetricKeyAlgorithm);
            }

            return new(new SymmetricSecurityKey(Encoding.ASCII.GetBytes(opts.SigningKey)), opts.SymmetricKeyAlgorithm);
        }
    }
}

/// <summary>
/// static class for easy creation of jwt bearer tokens
/// </summary>
public static class JWTBearer
{
    /// <summary>
    /// generate a jwt token with the supplied parameters
    /// </summary>
    /// <param name="signingKey">the secret key to use for signing the tokens</param>
    /// <param name="expireAt">the expiry date</param>
    /// <param name="permissions">one or more permissions to assign to the user principal</param>
    /// <param name="roles">one or more roles to assign the user principal</param>
    /// <param name="claims">one or more claims to assign to the user principal</param>
    [Obsolete("Use JwtBearer.CreateToken() method.")]
    public static string CreateToken(string signingKey,
                                     DateTime? expireAt = null,
                                     IEnumerable<string>? permissions = null,
                                     IEnumerable<string>? roles = null,
                                     params (string claimType, string claimValue)[] claims)
        => CreateToken(signingKey, expireAt, permissions, roles, claims.Select(c => new Claim(c.claimType, c.claimValue)));

    /// <summary>
    /// generate a jwt token with the supplied parameters
    /// </summary>
    /// <param name="signingKey">the secret key to use for signing the tokens</param>
    /// <param name="issuer">the issue</param>
    /// <param name="audience">the audience</param>
    /// <param name="expireAt">the expiry date</param>
    /// <param name="permissions">one or more permissions to assign to the user principal</param>
    /// <param name="roles">one or more roles to assign the user principal</param>
    /// <param name="claims">one or more claims to assign to the user principal</param>
    [Obsolete("Use JwtBearer.CreateToken() method.")]
    public static string CreateToken(string signingKey,
                                     string? issuer,
                                     string? audience,
                                     DateTime? expireAt = null,
                                     IEnumerable<string>? permissions = null,
                                     IEnumerable<string>? roles = null,
                                     params (string claimType, string claimValue)[] claims)
        => CreateToken(signingKey, expireAt, permissions, roles, claims.Select(c => new Claim(c.claimType, c.claimValue)), issuer, audience);

    /// <summary>
    /// generate a jwt token with the supplied parameters and token signing style
    /// </summary>
    /// <param name="signingKey">the secret key to use for signing the tokens</param>
    /// <param name="signingStyle">the signing style to use (Symmertic or Asymmetric)</param>
    /// <param name="issuer">the issue</param>
    /// <param name="audience">the audience</param>
    /// <param name="expireAt">the expiry date</param>
    /// <param name="permissions">one or more permissions to assign to the user principal</param>
    /// <param name="roles">one or more roles to assign the user principal</param>
    /// <param name="claims">one or more claims to assign to the user principal</param>
    [Obsolete("Use JwtBearer.CreateToken() method.")]
    public static string CreateToken(string signingKey,
                                     TokenSigningStyle signingStyle,
                                     string? issuer = null,
                                     string? audience = null,
                                     DateTime? expireAt = null,
                                     IEnumerable<string>? permissions = null,
                                     IEnumerable<string>? roles = null,
                                     params (string claimType, string claimValue)[] claims)
        => CreateToken(signingKey, expireAt, permissions, roles, claims.Select(c => new Claim(c.claimType, c.claimValue)), issuer, audience, signingStyle);

    /// <summary>
    /// generate a jwt token with the supplied parameters
    /// </summary>
    /// <param name="signingKey">the secret key to use for signing the tokens</param>
    /// <param name="privileges">an action to specify the privileges of the user</param>
    /// <param name="issuer">the issuer</param>
    /// <param name="audience">the audience</param>
    /// <param name="expireAt">the expiry date</param>
    /// <param name="signingStyle">the signing style to use (Symmertic or Asymmetric)</param>
    [Obsolete("Use JwtBearer.CreateToken() method.")]
    public static string CreateToken(string signingKey,
                                     Action<UserPrivileges> privileges,
                                     string? issuer = null,
                                     string? audience = null,
                                     DateTime? expireAt = null,
                                     TokenSigningStyle signingStyle = TokenSigningStyle.Symmetric)
    {
        var privs = new UserPrivileges();
        privileges(privs);

        return CreateToken(signingKey, expireAt, privs.Permissions, privs.Roles, privs.Claims, issuer, audience, signingStyle);
    }

    /// <summary>
    /// generate a jwt token with the supplied parameters
    /// </summary>
    /// <param name="signingKey">the secret key to use for signing the tokens</param>
    /// <param name="expireAt">the expiry date</param>
    /// <param name="permissions">one or more permissions to assign to the user principal</param>
    /// <param name="roles">one or more roles to assign the user principal</param>
    /// <param name="claims">one or more claims to assign to the user principal</param>
    /// <param name="issuer">the issuer</param>
    /// <param name="audience">the audience</param>
    /// <param name="signingStyle">the signing style to use (Symmetric or Asymmetric)</param>
    [Obsolete("Use JwtBearer.CreateToken() method.")]
    public static string CreateToken(string signingKey,
                                     DateTime? expireAt = null,
                                     IEnumerable<string>? permissions = null,
                                     IEnumerable<string>? roles = null,
                                     IEnumerable<Claim>? claims = null,
                                     string? issuer = null,
                                     string? audience = null,
                                     TokenSigningStyle signingStyle = TokenSigningStyle.Symmetric)
    {
        return JwtBearer.CreateToken(
            o =>
            {
                o.SigningKey = signingKey;
                o.SigningStyle = signingStyle;
                o.ExpireAt = expireAt;
                if (permissions?.Any() is true)
                    o.User.Permissions.AddRange(permissions);
                if (roles?.Any() is true)
                    o.User.Roles.AddRange(roles);
                if (claims?.Any() is true)
                    o.User.Claims.AddRange(claims);
                o.Issuer = issuer;
                o.Audience = audience;
            });
    }
}