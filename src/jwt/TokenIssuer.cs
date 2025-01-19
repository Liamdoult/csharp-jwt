﻿using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using jwt.Options;

namespace jwt;

public class TokenIssuer
{
    private Clock _clock { get; init; }

    private IssuingOptions _options { get; init; }

    public TokenIssuer(
        IssuingOptions? options = null,
        Clock? clock = null
    ) {
        _options = options ?? new();
        _clock = clock ?? new();
    }

    public bool TryGetValue(Token.Token token, [NotNullWhen(true)] out string? rawToken, [NotNullWhen(false)] out string? error) {
        var options = new JsonSerializerOptions {
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        };
        var header = JsonSerializer.Serialize(token.Header, options);
        var body = JsonSerializer.Serialize(token.Body, options);
        var signature = "";

        var headerB64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(header));
        var bodyB64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(body));

        var headerB64Url = ToBase64Url(headerB64);
        var bodyB64Url = ToBase64Url(bodyB64);

        rawToken = $"{headerB64Url}.{bodyB64Url}.{signature}";
        error = null;
        return true;
    }

    private static string ToBase64Url(string b64) => b64
        .Replace('/', '_')
        .Replace('+', '-')
        .Trim('=');
}