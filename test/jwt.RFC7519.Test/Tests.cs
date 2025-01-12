﻿using FluentAssertions;
using jwt.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace jwt.RFC7519.Test;

[TestClass]
public class RFC7519 {

    public static JwtHandlerOptions DefaultTestOptions => new() {
        ExpirationOptions = new() {
            ExpirationRequired = false,
        }
    };

    /// <summary>
    /// Asserts as per Section 4 of RFC 7159 [RFC7159], the JSON object consists of zero
    /// or more name/value pairs (or members)
    /// </summary>
    [TestMethod]
    public void WhenNoClaims_ThenDoeNotFail() {
        const string raw = "eyJhbGciOiJIUzI1NiJ9.e30.ZRrHA1JJJW8opsbCGfG_HACGpVUMN_a9IV7pAx_Zmeo";

        new JwtHandler(DefaultTestOptions).TryGetValue(raw, out var token, out var error).Should().BeTrue();
    }

    /// <summary

    /// <summary>
    /// Asserts Decoding Example JWT.
    /// </summary>
    [TestClass]
    public class ExampleToken {

        [TestMethod]
        public void ShouldDecode() {
            const string raw = "eyJ0eXAiOiJKV1QiLA0KICJhbGciOiJIUzI1NiJ9.eyJpc3MiOiJqb2UiLA0KICJleHAiOjEzMDA4MTkzODAsDQogImh0dHA6Ly9leGFtcGxlLmNvbS9pc19yb290Ijp0cnVlfQ.dBjftJeZ4CVP-mB92K27uhbUJU1p1r_wW1gFWFOEjXk";

            new JwtHandler(
                DefaultTestOptions,
                clock: new Clock(getCurrentTime: () => 1300819379)
            ).TryGetValue(raw, out var token, out var error).Should().BeTrue();
            token!.Header.Type.Should().Be("JWT");
            token!.Header.Claims.Should().ContainKey("alg").WhoseValue.GetString().Should().Be("HS256");
            token!.Body.Issuer.Should().Be("joe");
            token!.Body.ExpirationTime.Should().Be(1300819380);
            token!.Body.Claims.Should().ContainKey("http://example.com/is_root").WhoseValue.GetBoolean().Should().Be(true);
        }
    }


    /// <summary>
    /// Asserts the Claim Names within a JWT Claims Set MUST be unique; JWT
    /// parsers MUST either reject JWTs with duplicate Claim Names or use a JSON
    /// parser that returns only the lexically last duplicate member name, as
    /// specified in Section 15.12 ("The JSON Object") of ECMAScript 5.1
    /// [ECMAScript].
    /// </summary>
    /// <remarks>
    /// We choose to use the lexically last duplicate member name.
    /// </remarks>
    [TestClass]
    public class ClaimShouldBeUnique {

        [TestMethod]
        public void ClaimShouldBeUnique_WhenRegisteredClaim() {
            const string raw = "eyJ0eXAiOiJKV1QiLCJhbGciOiJIUzI1NiJ9.eyJpc3MiOiJiZW4iLCJodHRwOi8vZXhhbXBsZS5jb20vaXNfcm9vdCI6dHJ1ZX0.pKBFZvgZzz1HKAmBNapgM4SDDo53zekCcs6cIM7sVxQ";

            new JwtHandler(DefaultTestOptions).TryGetValue(raw, out var token, out var error).Should().BeTrue();
            token!.Body.Issuer.Should().Be("ben");
        }

        [TestMethod]
        public void ClaimShouldBeUnique_WhenCustomClaim() {
            const string raw = "eyJ0eXAiOiJKV1QiLCJhbGciOiJIUzI1NiJ9.eyJpc3MiOiJqb2UiLCJodHRwOi8vZXhhbXBsZS5jb20vaXNfcm9vdCI6dHJ1ZSwiY3VzdG9tIjoiYiJ9.M6ZXKV11MZ5-cXmPJ6vipk9DH-VD6JUJaXfMc-4KHh0";

            new JwtHandler(DefaultTestOptions).TryGetValue(raw, out var token, out var error).Should().BeTrue();
            token!.Body.Claims.Should().ContainKey("custom").WhoseValue.GetString().Should().Be("b");
        }
    }

    /// <summary>
    /// Asserts as per Section 4 of RFC 7159 [RFC7159], the JSON object consists of zero
    /// or more name/value pairs (or members), where the names are strings and
    /// the values are arbitrary JSON values.
    ///
    /// Also, Asserts specific applications of JWTs will require implementations
    /// to understand and process some claims in particular ways. However, in
    /// the absence of such requirements, all claims that are not understood by
    /// implementations MUST be ignored.
    ///
    /// Also, Asserts "iss", "sub", "aud", "exp", "nbf", "iat", "jti" are optional.
    /// </summary>
    [TestMethod]
    public void UnknownClaim_DoesNotFail() {
        const string raw = "eyJ0eXAiOiJKV1QiLCJhbGciOiJIUzI1NiJ9.eyJjdXN0b20iOiJqb2UifQ.QEk0Kc-0TWZXlczNULRLPszkB4k5fM1a4AZUVGgQx7U";

        new JwtHandler(DefaultTestOptions).TryGetValue(raw, out var token, out var error).Should().BeTrue();
    }
}

/// <summary>
/// Asserts 4.1.4. "exp" (Expiration Time) Claim.
///
/// The "exp" (expiration time) claim identifies the expiration time on or
/// after which the JWT MUST NOT be accepted for processing. The processing
/// of the "exp" claim requires that the current date/time MUST be before
/// the expiration date/time listed in the "exp" claim.
/// </summary>
[TestClass]
public class Section4_1_4 {

    /// <summary>
    /// Validates time exactly on expiration is invalid.
    /// </summary>
    [TestMethod]
    public void WhenTimeIsSameAsExp_ThenValidationFails() {
        // Token with exp set to 1736691481
        const string raw = "eyJ0eXAiOiJKV1QiLCJhbGciOiJIUzI1NiJ9.eyJleHAiOjE3MzY2OTE0ODF9.w_5MK3o_6rqJpH8Fl0q9WdSZEs413a2tS_j2Ly0XlH0";

        new JwtHandler(
            clock: new Clock(getCurrentTime: () => 1736691481)
        ).TryGetValue(raw, out var token, out var error).Should().BeFalse();
        error.Should().Be(Errors.TokenExpired);
    }

    /// <summary>
    /// Validates any time after expiration is invalid.
    /// </summary>
    [TestMethod]
    public void WhenTimeIsAfterExp_ThenValidationFails() {
        // Token with exp set to 1736691481
        const string raw = "eyJ0eXAiOiJKV1QiLCJhbGciOiJIUzI1NiJ9.eyJleHAiOjE3MzY2OTE0ODF9.w_5MK3o_6rqJpH8Fl0q9WdSZEs413a2tS_j2Ly0XlH0";

        new JwtHandler(
            clock: new Clock(getCurrentTime: () => 1736691482)
        ).TryGetValue(raw, out var token, out var error).Should().BeFalse();
        error.Should().Be(Errors.TokenExpired);
    }

    /// <summary>
    /// Validates any time before expiration is valid.
    /// </summary>
    [TestMethod]
    public void WhenTimeIsBeforeExp_ThenValidationSucceeds() {
        // Token with exp set to 1736691481
        const string raw = "eyJ0eXAiOiJKV1QiLCJhbGciOiJIUzI1NiJ9.eyJleHAiOjE3MzY2OTE0ODF9.w_5MK3o_6rqJpH8Fl0q9WdSZEs413a2tS_j2Ly0XlH0";

        new JwtHandler(
            clock: new Clock(getCurrentTime: () => 1736691480)
        ).TryGetValue(raw, out var token, out var error).Should().BeTrue();
    }

    /// <summary>
    /// Validates time exactly on the expiration+clockskew is invalid.
    /// </summary>
    [TestMethod]
    public void WhenTimeAndClockSkewIsSameAsExp_ThenValidationFails() {
        // Token with exp set to 1736691481
        const string raw = "eyJ0eXAiOiJKV1QiLCJhbGciOiJIUzI1NiJ9.eyJleHAiOjE3MzY2OTE0ODF9.w_5MK3o_6rqJpH8Fl0q9WdSZEs413a2tS_j2Ly0XlH0";

        new JwtHandler(
            clock: new Clock(clockSkew: TimeSpan.FromSeconds(5), getCurrentTime: () => 1736691481 - 5)
        ).TryGetValue(raw, out var token, out var error).Should().BeFalse();
        error.Should().Be(Errors.TokenExpired);
    }

    /// <summary>
    /// Validates any after expiration+clockskew is invalid.
    /// </summary>
    [TestMethod]
    public void WhenTimeAndClockSkewIsAfterExp_ThenValidationFails() {
        // Token with exp set to 1736691481
        const string raw = "eyJ0eXAiOiJKV1QiLCJhbGciOiJIUzI1NiJ9.eyJleHAiOjE3MzY2OTE0ODF9.w_5MK3o_6rqJpH8Fl0q9WdSZEs413a2tS_j2Ly0XlH0";

        new JwtHandler(
            clock: new Clock(clockSkew: TimeSpan.FromSeconds(5), getCurrentTime: () => 1736691482 - 5)
        ).TryGetValue(raw, out var token, out var error).Should().BeFalse();
        error.Should().Be(Errors.TokenExpired);
    }

    /// <summary>
    /// Validates any time before expiration+clockskew is valid.
    /// </summary>
    [TestMethod]
    public void WhenTimeAndClockSkewIsBeforeExp_ThenValidationSucceeds() {
        // Token with exp set to 1736691481
        const string raw = "eyJ0eXAiOiJKV1QiLCJhbGciOiJIUzI1NiJ9.eyJleHAiOjE3MzY2OTE0ODF9.w_5MK3o_6rqJpH8Fl0q9WdSZEs413a2tS_j2Ly0XlH0";

        new JwtHandler(
            clock: new Clock(clockSkew: TimeSpan.FromSeconds(5), getCurrentTime: () => 1736691480 - 5)
        ).TryGetValue(raw, out var token, out var error).Should().BeTrue();
    }

    /// <summary>
    /// Validates non-NumericDate fails.
    /// </summary>
    [TestMethod]
    [DataRow("eyJ0eXAiOiJKV1QiLCJhbGciOiJIUzI1NiJ9.eyJleHAiOiJ0ZXN0In0.OmovxMNN77dbgc_5j1-K-K6GhLoNh1Lyhgolw9x0N2g")] // Token with exp set to "test"
    [DataRow("eyJ0eXAiOiJKV1QiLCJhbGciOiJIUzI1NiJ9.eyJleHAiOjEuMX0.T48rjzoG09qg2goAL_-8GLGDwM5MS1VhKZdkyooi_3c")] // Token with exp set to 1.100
    public void WhenExpClaimIsNotNumericDate_ThenFails(string raw) {

        new JwtHandler().TryGetValue(raw, out var token, out var error).Should().BeFalse();
        error.Should().Be(Errors.InvalidTokenStructure);
    }

    /// <summary>
    /// Use of exp claim is optional.
    /// </summary>
    [TestMethod]
    public void ExpClaimIsOptional() {
        const string raw = "eyJ0eXAiOiJKV1QiLCJhbGciOiJIUzI1NiJ9.e30.yXvILkvUUCBqAFlAv6wQ1Q-QRAjfe3eSosO949U73Vo";

        new JwtHandler(
            new() {
                ExpirationOptions = new() {
                    ExpirationRequired = false,
                }
            })
            .TryGetValue(raw, out var token, out var error)
            .Should()
            .BeTrue();
    }
}