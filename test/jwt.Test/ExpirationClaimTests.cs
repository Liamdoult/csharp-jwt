﻿using FluentAssertions;
using jwt.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace jwt.Test;

[TestClass]
public class ExpirationClaimTests {

    /// <summary>
    /// Ensures that tokens are being validated in the most secure manner by
    /// default.
    /// </summary>
    [TestMethod]
    public void ExpirationOptions_AreSecureByDefault() {
        var expirationOptions = new ExpirationOptions();

        expirationOptions.ClockSkew.Should().Be(TimeSpan.Zero);
        expirationOptions.IsExpirationClaimRequired.Should().BeTrue();
    }

    [TestMethod]
    public void WhenExpClaimNotPresent_AndExpirationIsRequired_ThenFails() {
        const string raw = "eyJhbGciOiJIUzI1NiJ9.e30.ZRrHA1JJJW8opsbCGfG_HACGpVUMN_a9IV7pAx_Zmeo";

        new TokenValidator(
            new() {
                ExpirationOptions = new() {
                    IsExpirationClaimRequired = true,
                }
            })
            .TryGetValue(raw, out var token, out var error)
            .Should()
            .BeFalse();
    }

    [TestMethod]
    public void WhenExpClaimNotPresent_AndExpirationIsNotRequired_ThenFails() {
        const string raw = "eyJ0eXAiOiJKV1QiLCJhbGciOiJIUzI1NiJ9.e30.yXvILkvUUCBqAFlAv6wQ1Q-QRAjfe3eSosO949U73Vo";

        new TokenValidator(
            new() {
                AudianceOptions = new() {
                    IsAudianceValidationEnabled = false,
                },
                NotBeforeOptions = new() {
                    IsNotBeforeClaimRequired = false,
                },
                ExpirationOptions = new() {
                    IsExpirationValidationEnabled = false,
                }
            })
            .TryGetValue(raw, out var token, out var error)
            .Should()
            .BeTrue();
    }
}