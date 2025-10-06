using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Flowingly.ParsingService.Contracts.Requests;
using Flowingly.ParsingService.Contracts.Responses;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace Flowingly.ParsingService.IntegrationTests.Api;

/// <summary>
/// Integration tests verifying currency symbol and comma handling in expense parsing.
/// Tests Issue #3 fix - NumberNormalizer integration.
/// </summary>
public class CurrencyNormalizationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public CurrencyNormalizationTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    [Theory]
    [InlineData("$35,000.00", 35000.00, 30434.78, 4565.22)]
    [InlineData("1,024.99", 1024.99, 891.30, 133.69)]
    [InlineData("$50.00", 50.00, 43.48, 6.52)]
    [InlineData("100", 100.00, 86.96, 13.04)]
    public async Task ParseExpense_WithCurrencySymbolsAndCommas_CalculatesGstCorrectly(
        string totalInput,
        decimal expectedTotal,
        decimal expectedTotalExclTax,
        decimal expectedSalesTax)
    {
        // Arrange
        var request = new ParseRequest
        {
            Text = $"<total>{totalInput}</total>"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/v1/parse", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<ExpenseParseResponse>();

        result.Should().NotBeNull();
        result!.Classification.Should().Be("expense");
        result.Expense.Should().NotBeNull();
        result.Expense!.Total.Should().Be(expectedTotal);
        result.Expense.TotalExclTax.Should().BeApproximately(expectedTotalExclTax, 0.01m);
        result.Expense.SalesTax.Should().BeApproximately(expectedSalesTax, 0.01m);
    }

    [Fact]
    public async Task ParseExpense_WithXmlIslandCurrencySymbols_ParsesCorrectly()
    {
        // Arrange
        var request = new ParseRequest
        {
            Text = @"Hi, please process this: <expense><total>$35,000.00</total><cost_centre>DEV</cost_centre></expense>"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/v1/parse", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<ExpenseParseResponse>();

        result.Should().NotBeNull();
        result!.Expense!.Total.Should().Be(35000.00m);
        result.Expense.TotalExclTax.Should().BeApproximately(30434.78m, 0.01m);
        result.Expense.SalesTax.Should().BeApproximately(4565.22m, 0.01m);
    }
}
