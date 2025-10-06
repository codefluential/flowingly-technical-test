using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Flowingly.ParsingService.Contracts.Requests;
using Flowingly.ParsingService.Contracts.Responses;
using Flowingly.ParsingService.Contracts.Errors;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace Flowingly.ParsingService.IntegrationTests.Api;

[Trait("Category", "Contract")]
public class ParseEndpointTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public ParseEndpointTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    #region Happy Path Tests

    [Fact]
    public async Task ParseEndpoint_ExpenseClassification_ReturnsExpenseResponse()
    {
        // Arrange
        var request = new ParseRequest
        {
            Text = "Expense for <vendor>Mojo Coffee</vendor> total <total>120.50</total> <cost_centre>DEV</cost_centre>",
            TaxRate = 0.15m
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/v1/parse", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<ExpenseParseResponse>();
        result.Should().NotBeNull();
        result!.Classification.Should().Be("expense");
        result.Expense.Should().NotBeNull();
        result.Expense!.Vendor.Should().Be("Mojo Coffee");
        result.Expense.Total.Should().Be(120.50m);
        result.Expense.TotalExclTax.Should().Be(104.78m); // Banker's Rounding
        result.Expense.SalesTax.Should().Be(15.72m);
        result.Expense.CostCentre.Should().Be("DEV");
        result.Meta.Should().NotBeNull();
        result.Meta!.CorrelationId.Should().NotBeEmpty();
    }

    [Fact]
    public async Task ParseEndpoint_OtherClassification_ReturnsOtherResponse()
    {
        // Arrange
        var request = new ParseRequest
        {
            Text = "<reservation_date>2024-12-25</reservation_date> <venue>The French Café</venue>"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/v1/parse", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<OtherParseResponse>();
        result.Should().NotBeNull();
        result!.Classification.Should().Be("other");
        result.Other.Should().NotBeNull();
        result.Other!.RawTags.Should().ContainKey("reservation_date");
        result.Other.RawTags.Should().ContainKey("venue");
        result.Meta.Should().NotBeNull();
        result.Meta!.CorrelationId.Should().NotBeEmpty();
    }

    #endregion

    #region Validation Error Tests

    [Fact]
    public async Task ParseEndpoint_UnclosedTag_Returns400WithErrorCode()
    {
        // Arrange
        var request = new ParseRequest
        {
            Text = "<total>120"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/v1/parse", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var error = await response.Content.ReadFromJsonAsync<ErrorResponse>();
        error.Should().NotBeNull();
        error!.ErrorCode.Should().Be("UNCLOSED_TAGS");
        error.Message.Should().Contain("tag");
    }

    [Fact]
    public async Task ParseEndpoint_OverlappingTags_Returns400()
    {
        // Arrange
        var request = new ParseRequest
        {
            Text = "<a><b></a></b>"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/v1/parse", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var error = await response.Content.ReadFromJsonAsync<ErrorResponse>();
        error.Should().NotBeNull();
        error!.ErrorCode.Should().Be("UNCLOSED_TAGS");
    }

    [Fact]
    public async Task ParseEndpoint_MissingTotal_Returns400()
    {
        // Arrange
        var request = new ParseRequest
        {
            Text = "<vendor>Test Vendor</vendor>"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/v1/parse", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var error = await response.Content.ReadFromJsonAsync<ErrorResponse>();
        error.Should().NotBeNull();
        error!.ErrorCode.Should().Be("MISSING_TOTAL");
    }

    #endregion

    #region Business Rule Tests

    [Fact]
    public async Task ParseEndpoint_MissingCostCentre_DefaultsToUnknown()
    {
        // Arrange
        var request = new ParseRequest
        {
            Text = "<total>100</total>"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/v1/parse", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<ExpenseParseResponse>();
        result.Should().NotBeNull();
        result!.Expense.Should().NotBeNull();
        result.Expense!.CostCentre.Should().Be("UNKNOWN");
    }

    #endregion

    #region Tax Calculation Tests

    [Theory]
    [InlineData(120.50, 0.15, 104.78, 15.72)] // Banker's Rounding test case
    [InlineData(100.00, 0.15, 86.96, 13.04)]
    public async Task ParseEndpoint_TaxCalculation_UsesBankersRounding(
        decimal totalIncl,
        decimal taxRate,
        decimal expectedExcl,
        decimal expectedTax)
    {
        // Arrange
        var request = new ParseRequest
        {
            Text = $"<total>{totalIncl}</total>",
            TaxRate = taxRate
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/v1/parse", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<ExpenseParseResponse>();
        result.Should().NotBeNull();
        result!.Expense.Should().NotBeNull();
        result.Expense!.TotalExclTax.Should().Be(expectedExcl);
        result.Expense.SalesTax.Should().Be(expectedTax);
    }

    [Fact]
    public async Task ParseEndpoint_TaxRatePrecedence_RequestWins()
    {
        // Arrange - request tax_rate parameter should override config default
        var request = new ParseRequest
        {
            Text = "<total>100</total>",
            TaxRate = 0.10m // Request parameter (should win)
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/v1/parse", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<ExpenseParseResponse>();
        result.Should().NotBeNull();
        result!.Expense.Should().NotBeNull();
        // With TaxRate=0.10, 100/(1+0.10) = 90.91, tax = 9.09
        result.Expense!.TotalExclTax.Should().Be(90.91m);
        result.Expense.SalesTax.Should().Be(9.09m);
    }

    [Fact]
    public async Task ParseEndpoint_TaxRateMissing_UsesConfigDefault()
    {
        // Arrange - no tax_rate in request, should use config default (0.15)
        var request = new ParseRequest
        {
            Text = "<total>100</total>"
            // TaxRate not specified, should use config default
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/v1/parse", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<ExpenseParseResponse>();
        result.Should().NotBeNull();
        result!.Expense.Should().NotBeNull();
        // With default TaxRate=0.15, 100/(1+0.15) = 86.96, tax = 13.04
        result.Expense!.TotalExclTax.Should().Be(86.96m);
        result.Expense.SalesTax.Should().Be(13.04m);
    }

    #endregion

    #region Correlation ID and Response Structure Tests

    [Fact]
    public async Task ParseEndpoint_AllRequests_IncludeUniqueCorrelationId()
    {
        // Arrange
        var request1 = new ParseRequest { Text = "<total>100</total>" };
        var request2 = new ParseRequest { Text = "<total>200</total>" };

        // Act
        var response1 = await _client.PostAsJsonAsync("/api/v1/parse", request1);
        var response2 = await _client.PostAsJsonAsync("/api/v1/parse", request2);

        // Assert
        var result1 = await response1.Content.ReadFromJsonAsync<ExpenseParseResponse>();
        var result2 = await response2.Content.ReadFromJsonAsync<ExpenseParseResponse>();

        result1.Should().NotBeNull();
        result2.Should().NotBeNull();
        result1!.Meta.CorrelationId.Should().NotBeEmpty();
        result2!.Meta.CorrelationId.Should().NotBeEmpty();
        result1.Meta.CorrelationId.Should().NotBe(result2.Meta.CorrelationId);
    }

    [Fact]
    public async Task ParseEndpoint_ExpenseResponse_HasExpenseFieldNotOtherField()
    {
        // Arrange
        var request = new ParseRequest
        {
            Text = "<total>100</total>"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/v1/parse", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<ExpenseParseResponse>();
        result.Should().NotBeNull();
        result!.Classification.Should().Be("expense");
        result.Expense.Should().NotBeNull(); // Expense field exists
        // Other field should not exist in ExpenseParseResponse type
    }

    [Fact]
    public async Task ParseEndpoint_OtherResponse_HasOtherFieldNotExpenseField()
    {
        // Arrange
        var request = new ParseRequest
        {
            Text = "<reservation_date>2024-12-25</reservation_date>"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/v1/parse", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<OtherParseResponse>();
        result.Should().NotBeNull();
        result!.Classification.Should().Be("other");
        result.Other.Should().NotBeNull(); // Other field exists
        // Expense field should not exist in OtherParseResponse type
    }

    #endregion
}
