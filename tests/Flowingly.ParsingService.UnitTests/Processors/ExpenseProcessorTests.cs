using FluentAssertions;
using Moq;
using Xunit;
using Flowingly.ParsingService.Domain.Processors;
using Flowingly.ParsingService.Domain.Models;
using Flowingly.ParsingService.Domain.Interfaces;
using Flowingly.ParsingService.Domain.Exceptions;

namespace Flowingly.ParsingService.Tests.Processors;

/// <summary>
/// TDD RED Phase: Expense Processor Tests (London School - Mockist Style)
///
/// Tests for ExpenseProcessor that implements the Strategy pattern pipeline:
/// 1. Validate → Check <total> tag exists (required)
/// 2. Extract → Pull expense fields from inline tags and XML islands
/// 3. Normalize → Call ITaxCalculator for tax breakdown
/// 4. Persist → Save to IExpenseRepository
/// 5. BuildResponse → Return ProcessingResult with classification='expense'
///
/// All tests MUST FAIL initially (no implementation exists yet).
///
/// Business Rules:
/// - <total> (tax-inclusive) is REQUIRED; reject with MISSING_TOTAL if absent
/// - <cost_centre> is OPTIONAL; default to 'UNKNOWN' if absent
/// - Extract: vendor, description, date, time, total, cost_centre, payment_method
/// - Prefer <total> within <expense> island over global <total>
/// - Calculate tax breakdown using ITaxCalculator.CalculateFromInclusive()
/// - Persist expense using IExpenseRepository.SaveAsync()
/// - Build ExpenseResponse with classification='expense'
///
/// Reference: ADR-0003, ADR-0007, ADR-0008, PRD Section 4.2
/// </summary>
public class ExpenseProcessorTests
{
    // All test helper models now imported from Domain layer
    // See using statements at top of file

    #region Happy Path Tests

    [Fact]
    public async Task ProcessAsync_ValidExpenseWithAllFields_ShouldProcessSuccessfully()
    {
        // Arrange
        var mockTaxCalculator = new Mock<ITaxCalculator>();
        mockTaxCalculator.Setup(c => c.CalculateFromInclusive(120.50m, 0.15m))
            .Returns(new TaxCalculationResult
            {
                TaxInclusive = 120.50m,
                TaxExclusive = 104.78m,
                Gst = 15.72m,
                TaxRate = 0.15m
            });

        var mockRepository = new Mock<IExpenseRepository>();
        mockRepository.Setup(r => r.SaveAsync(It.IsAny<Expense>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Guid.NewGuid());

        var processor = new ExpenseProcessor(mockTaxCalculator.Object, mockRepository.Object);

        var content = new ParsedContent
        {
            InlineTags = new Dictionary<string, string>
            {
                ["vendor"] = "Mojo Coffee",
                ["description"] = "Team lunch",
                ["total"] = "120.50",
                ["cost_centre"] = "DEV-TEAM",
                ["date"] = "2024-10-05",
                ["time"] = "12:30"
            },
            XmlIslands = new List<XmlIsland>()
        };

        // Act
        var result = await processor.ProcessAsync(content, CancellationToken.None);

        // Assert
        result.Classification.Should().Be("expense", "content is classified as expense");
        result.Success.Should().BeTrue("processing should succeed");

        mockTaxCalculator.Verify(c => c.CalculateFromInclusive(120.50m, 0.15m), Times.Once,
            "should calculate tax from inclusive total");

        mockRepository.Verify(r => r.SaveAsync(
            It.Is<Expense>(e =>
                e.Vendor == "Mojo Coffee" &&
                e.Description == "Team lunch" &&
                e.Total == 120.50m &&
                e.TotalExclTax == 104.78m &&
                e.SalesTax == 15.72m &&
                e.CostCentre == "DEV-TEAM" &&
                e.Date == "2024-10-05" &&
                e.Time == "12:30" &&
                e.TaxRate == 0.15m),
            It.IsAny<CancellationToken>()),
            Times.Once,
            "should persist expense with all fields");
    }

    [Fact]
    public async Task ProcessAsync_DefaultCostCentreToUNKNOWN_WhenMissing()
    {
        // Arrange
        var mockTaxCalculator = new Mock<ITaxCalculator>();
        mockTaxCalculator.Setup(c => c.CalculateFromInclusive(100.00m, 0.15m))
            .Returns(new TaxCalculationResult
            {
                TaxInclusive = 100.00m,
                TaxExclusive = 86.96m,
                Gst = 13.04m,
                TaxRate = 0.15m
            });

        var mockRepository = new Mock<IExpenseRepository>();
        mockRepository.Setup(r => r.SaveAsync(It.IsAny<Expense>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Guid.NewGuid());

        var processor = new ExpenseProcessor(mockTaxCalculator.Object, mockRepository.Object);

        var content = new ParsedContent
        {
            InlineTags = new Dictionary<string, string>
            {
                ["vendor"] = "Mojo Coffee",
                ["total"] = "100.00"
            },
            XmlIslands = new List<XmlIsland>()
        };

        // Act
        var result = await processor.ProcessAsync(content, CancellationToken.None);

        // Assert
        result.Classification.Should().Be("expense");

        mockRepository.Verify(r => r.SaveAsync(
            It.Is<Expense>(e => e.CostCentre == "UNKNOWN"),
            It.IsAny<CancellationToken>()),
            Times.Once,
            "should default cost_centre to UNKNOWN when missing");
    }

    #endregion

    #region Validation Tests

    [Fact]
    public async Task ProcessAsync_MissingTotalTag_ShouldThrowValidationException()
    {
        // Arrange
        var mockTaxCalculator = new Mock<ITaxCalculator>();
        var mockRepository = new Mock<IExpenseRepository>();

        var processor = new ExpenseProcessor(mockTaxCalculator.Object, mockRepository.Object);

        var content = new ParsedContent
        {
            InlineTags = new Dictionary<string, string>
            {
                ["vendor"] = "Mojo Coffee",
                ["description"] = "Lunch"
            },
            XmlIslands = new List<XmlIsland>()
        };

        // Act
        Func<Task> act = async () => await processor.ProcessAsync(content, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ValidationException>()
            .WithMessage("*MISSING_TOTAL*")
            .Where(ex => ex.ErrorCode == "MISSING_TOTAL",
                "should reject expense without <total> tag");

        mockTaxCalculator.Verify(c => c.CalculateFromInclusive(It.IsAny<decimal>(), It.IsAny<decimal>()),
            Times.Never,
            "should not calculate tax when validation fails");

        mockRepository.Verify(r => r.SaveAsync(It.IsAny<Expense>(), It.IsAny<CancellationToken>()),
            Times.Never,
            "should not persist when validation fails");
    }

    #endregion

    #region XML Island Extraction Tests

    [Fact]
    public async Task ProcessAsync_ExtractExpenseFromXmlIsland_ShouldProcessCorrectly()
    {
        // Arrange
        var mockTaxCalculator = new Mock<ITaxCalculator>();
        mockTaxCalculator.Setup(c => c.CalculateFromInclusive(150.00m, 0.15m))
            .Returns(new TaxCalculationResult
            {
                TaxInclusive = 150.00m,
                TaxExclusive = 130.43m,
                Gst = 19.57m,
                TaxRate = 0.15m
            });

        var mockRepository = new Mock<IExpenseRepository>();
        mockRepository.Setup(r => r.SaveAsync(It.IsAny<Expense>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Guid.NewGuid());

        var processor = new ExpenseProcessor(mockTaxCalculator.Object, mockRepository.Object);

        var content = new ParsedContent
        {
            InlineTags = new Dictionary<string, string>
            {
                ["vendor"] = "Mojo Coffee"
            },
            XmlIslands = new List<XmlIsland>
            {
                new XmlIsland
                {
                    Name = "expense",
                    Content = "<total>150.00</total><cost_centre>SALES</cost_centre>"
                }
            }
        };

        // Act
        var result = await processor.ProcessAsync(content, CancellationToken.None);

        // Assert
        result.Classification.Should().Be("expense");

        mockRepository.Verify(r => r.SaveAsync(
            It.Is<Expense>(e =>
                e.Total == 150.00m &&
                e.CostCentre == "SALES"),
            It.IsAny<CancellationToken>()),
            Times.Once,
            "should extract expense data from XML island");
    }

    [Fact]
    public async Task ProcessAsync_MixedInlineTagsAndXmlIsland_ShouldMergeData()
    {
        // Arrange
        var mockTaxCalculator = new Mock<ITaxCalculator>();
        mockTaxCalculator.Setup(c => c.CalculateFromInclusive(200.00m, 0.15m))
            .Returns(new TaxCalculationResult
            {
                TaxInclusive = 200.00m,
                TaxExclusive = 173.91m,
                Gst = 26.09m,
                TaxRate = 0.15m
            });

        var mockRepository = new Mock<IExpenseRepository>();
        mockRepository.Setup(r => r.SaveAsync(It.IsAny<Expense>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Guid.NewGuid());

        var processor = new ExpenseProcessor(mockTaxCalculator.Object, mockRepository.Object);

        var content = new ParsedContent
        {
            InlineTags = new Dictionary<string, string>
            {
                ["vendor"] = "Mojo Coffee",
                ["description"] = "Lunch"
            },
            XmlIslands = new List<XmlIsland>
            {
                new XmlIsland
                {
                    Name = "expense",
                    Content = "<total>200.00</total><cost_centre>ADMIN</cost_centre>"
                }
            }
        };

        // Act
        var result = await processor.ProcessAsync(content, CancellationToken.None);

        // Assert
        mockRepository.Verify(r => r.SaveAsync(
            It.Is<Expense>(e =>
                e.Vendor == "Mojo Coffee" &&
                e.Description == "Lunch" &&
                e.Total == 200.00m &&
                e.CostCentre == "ADMIN"),
            It.IsAny<CancellationToken>()),
            Times.Once,
            "should merge data from inline tags and XML island");
    }

    #endregion

    #region Tax Calculation Integration Tests

    [Fact]
    public async Task ProcessAsync_CalculateTaxUsingITaxCalculator_ShouldIntegrateCorrectly()
    {
        // Arrange
        var mockTaxCalculator = new Mock<ITaxCalculator>();
        mockTaxCalculator.Setup(c => c.CalculateFromInclusive(115.00m, 0.15m))
            .Returns(new TaxCalculationResult
            {
                TaxInclusive = 115.00m,
                TaxExclusive = 100.00m,
                Gst = 15.00m,
                TaxRate = 0.15m
            });

        var mockRepository = new Mock<IExpenseRepository>();
        mockRepository.Setup(r => r.SaveAsync(It.IsAny<Expense>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Guid.NewGuid());

        var processor = new ExpenseProcessor(mockTaxCalculator.Object, mockRepository.Object);

        var content = new ParsedContent
        {
            InlineTags = new Dictionary<string, string>
            {
                ["total"] = "115.00"
            },
            XmlIslands = new List<XmlIsland>()
        };

        // Act
        var result = await processor.ProcessAsync(content, CancellationToken.None);

        // Assert
        mockTaxCalculator.Verify(c => c.CalculateFromInclusive(115.00m, 0.15m), Times.Once,
            "should use ITaxCalculator for tax breakdown");

        mockRepository.Verify(r => r.SaveAsync(
            It.Is<Expense>(e =>
                e.Total == 115.00m &&
                e.TotalExclTax == 100.00m &&
                e.SalesTax == 15.00m &&
                e.TaxRate == 0.15m),
            It.IsAny<CancellationToken>()),
            Times.Once,
            "should populate expense with calculated tax values");
    }

    [Fact]
    public async Task ProcessAsync_WithCustomTaxRate_ShouldUseCustomRate()
    {
        // Arrange
        var mockTaxCalculator = new Mock<ITaxCalculator>();
        mockTaxCalculator.Setup(c => c.CalculateFromInclusive(120.00m, 0.20m))
            .Returns(new TaxCalculationResult
            {
                TaxInclusive = 120.00m,
                TaxExclusive = 100.00m,
                Gst = 20.00m,
                TaxRate = 0.20m
            });

        var mockRepository = new Mock<IExpenseRepository>();
        mockRepository.Setup(r => r.SaveAsync(It.IsAny<Expense>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Guid.NewGuid());

        var processor = new ExpenseProcessor(mockTaxCalculator.Object, mockRepository.Object);

        var content = new ParsedContent
        {
            InlineTags = new Dictionary<string, string>
            {
                ["total"] = "120.00"
            },
            XmlIslands = new List<XmlIsland>(),
            TaxRate = 0.20m
        };

        // Act
        var result = await processor.ProcessAsync(content, CancellationToken.None);

        // Assert
        mockTaxCalculator.Verify(c => c.CalculateFromInclusive(120.00m, 0.20m), Times.Once,
            "should use custom tax rate from request");

        mockRepository.Verify(r => r.SaveAsync(
            It.Is<Expense>(e => e.TaxRate == 0.20m),
            It.IsAny<CancellationToken>()),
            Times.Once,
            "should persist expense with custom tax rate");
    }

    #endregion

    #region Repository Persistence Tests

    [Fact]
    public async Task ProcessAsync_PersistExpenseToRepository_ShouldCallSaveAsync()
    {
        // Arrange
        var mockTaxCalculator = new Mock<ITaxCalculator>();
        mockTaxCalculator.Setup(c => c.CalculateFromInclusive(120.50m, 0.15m))
            .Returns(new TaxCalculationResult
            {
                TaxInclusive = 120.50m,
                TaxExclusive = 104.78m,
                Gst = 15.72m,
                TaxRate = 0.15m
            });

        var savedExpenseId = Guid.NewGuid();
        var mockRepository = new Mock<IExpenseRepository>();
        mockRepository.Setup(r => r.SaveAsync(It.IsAny<Expense>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(savedExpenseId);

        var processor = new ExpenseProcessor(mockTaxCalculator.Object, mockRepository.Object);

        var content = new ParsedContent
        {
            InlineTags = new Dictionary<string, string>
            {
                ["vendor"] = "Mojo Coffee",
                ["total"] = "120.50"
            },
            XmlIslands = new List<XmlIsland>()
        };

        // Act
        var result = await processor.ProcessAsync(content, CancellationToken.None);

        // Assert
        mockRepository.Verify(r => r.SaveAsync(
            It.Is<Expense>(e =>
                e.Vendor == "Mojo Coffee" &&
                e.Total == 120.50m),
            It.IsAny<CancellationToken>()),
            Times.Once,
            "should persist expense to repository");
    }

    [Fact]
    public async Task ProcessAsync_PropagateCancellationToken_ShouldPassToRepository()
    {
        // Arrange
        var cts = new CancellationTokenSource();
        var cancellationToken = cts.Token;

        var mockTaxCalculator = new Mock<ITaxCalculator>();
        mockTaxCalculator.Setup(c => c.CalculateFromInclusive(100.00m, 0.15m))
            .Returns(new TaxCalculationResult
            {
                TaxInclusive = 100.00m,
                TaxExclusive = 86.96m,
                Gst = 13.04m,
                TaxRate = 0.15m
            });

        var mockRepository = new Mock<IExpenseRepository>();
        mockRepository.Setup(r => r.SaveAsync(It.IsAny<Expense>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Guid.NewGuid());

        var processor = new ExpenseProcessor(mockTaxCalculator.Object, mockRepository.Object);

        var content = new ParsedContent
        {
            InlineTags = new Dictionary<string, string>
            {
                ["total"] = "100.00"
            },
            XmlIslands = new List<XmlIsland>()
        };

        // Act
        await processor.ProcessAsync(content, cancellationToken);

        // Assert
        mockRepository.Verify(r => r.SaveAsync(
            It.IsAny<Expense>(),
            It.Is<CancellationToken>(ct => ct == cancellationToken)),
            Times.Once,
            "should propagate cancellation token to repository");
    }

    #endregion

    #region Response Building Tests

    [Fact]
    public async Task ProcessAsync_BuildExpenseResponse_ShouldSetClassificationToExpense()
    {
        // Arrange
        var mockTaxCalculator = new Mock<ITaxCalculator>();
        mockTaxCalculator.Setup(c => c.CalculateFromInclusive(120.50m, 0.15m))
            .Returns(new TaxCalculationResult
            {
                TaxInclusive = 120.50m,
                TaxExclusive = 104.78m,
                Gst = 15.72m,
                TaxRate = 0.15m
            });

        var mockRepository = new Mock<IExpenseRepository>();
        mockRepository.Setup(r => r.SaveAsync(It.IsAny<Expense>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Guid.NewGuid());

        var processor = new ExpenseProcessor(mockTaxCalculator.Object, mockRepository.Object);

        var content = new ParsedContent
        {
            InlineTags = new Dictionary<string, string>
            {
                ["vendor"] = "Mojo Coffee",
                ["total"] = "120.50"
            },
            XmlIslands = new List<XmlIsland>()
        };

        // Act
        var result = await processor.ProcessAsync(content, CancellationToken.None);

        // Assert
        result.Classification.Should().Be("expense",
            "pipeline step 5: build ExpenseResponse with classification='expense'");
        result.Success.Should().BeTrue("processing should succeed");
        result.Data.Should().NotBeNull("should include expense data in response");
    }

    #endregion

    #region Optional Fields Tests

    [Fact]
    public async Task ProcessAsync_OptionalDateAndTimeFields_ShouldHandleAbsence()
    {
        // Arrange
        var mockTaxCalculator = new Mock<ITaxCalculator>();
        mockTaxCalculator.Setup(c => c.CalculateFromInclusive(100.00m, 0.15m))
            .Returns(new TaxCalculationResult
            {
                TaxInclusive = 100.00m,
                TaxExclusive = 86.96m,
                Gst = 13.04m,
                TaxRate = 0.15m
            });

        var mockRepository = new Mock<IExpenseRepository>();
        mockRepository.Setup(r => r.SaveAsync(It.IsAny<Expense>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Guid.NewGuid());

        var processor = new ExpenseProcessor(mockTaxCalculator.Object, mockRepository.Object);

        var content = new ParsedContent
        {
            InlineTags = new Dictionary<string, string>
            {
                ["total"] = "100.00"
            },
            XmlIslands = new List<XmlIsland>()
        };

        // Act
        var result = await processor.ProcessAsync(content, CancellationToken.None);

        // Assert
        mockRepository.Verify(r => r.SaveAsync(
            It.Is<Expense>(e =>
                e.Date == string.Empty &&
                e.Time == string.Empty),
            It.IsAny<CancellationToken>()),
            Times.Once,
            "should handle optional date and time fields as empty strings");
    }

    [Fact]
    public async Task ProcessAsync_ExtractPaymentMethod_WhenPresent()
    {
        // Arrange
        var mockTaxCalculator = new Mock<ITaxCalculator>();
        mockTaxCalculator.Setup(c => c.CalculateFromInclusive(100.00m, 0.15m))
            .Returns(new TaxCalculationResult
            {
                TaxInclusive = 100.00m,
                TaxExclusive = 86.96m,
                Gst = 13.04m,
                TaxRate = 0.15m
            });

        var mockRepository = new Mock<IExpenseRepository>();
        mockRepository.Setup(r => r.SaveAsync(It.IsAny<Expense>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Guid.NewGuid());

        var processor = new ExpenseProcessor(mockTaxCalculator.Object, mockRepository.Object);

        var content = new ParsedContent
        {
            InlineTags = new Dictionary<string, string>
            {
                ["total"] = "100.00",
                ["payment_method"] = "Credit Card"
            },
            XmlIslands = new List<XmlIsland>()
        };

        // Act
        var result = await processor.ProcessAsync(content, CancellationToken.None);

        // Assert
        mockRepository.Verify(r => r.SaveAsync(
            It.Is<Expense>(e => e.PaymentMethod == "Credit Card"),
            It.IsAny<CancellationToken>()),
            Times.Once,
            "should extract payment_method if provided");
    }

    #endregion

    #region CanProcess Tests

    [Fact]
    public void CanProcess_WithTotalTag_ShouldReturnTrue()
    {
        // Arrange
        var mockTaxCalculator = new Mock<ITaxCalculator>();
        var mockRepository = new Mock<IExpenseRepository>();
        var processor = new ExpenseProcessor(mockTaxCalculator.Object, mockRepository.Object);

        var content = new ParsedContent
        {
            InlineTags = new Dictionary<string, string>
            {
                ["total"] = "100.00",
                ["vendor"] = "Mojo Coffee"
            },
            XmlIslands = new List<XmlIsland>()
        };

        // Act
        var result = processor.CanProcess(content);

        // Assert
        result.Should().BeTrue("content with <total> tag should be processed as expense");
    }

    [Fact]
    public void CanProcess_WithExpenseIsland_ShouldReturnTrue()
    {
        // Arrange
        var mockTaxCalculator = new Mock<ITaxCalculator>();
        var mockRepository = new Mock<IExpenseRepository>();
        var processor = new ExpenseProcessor(mockTaxCalculator.Object, mockRepository.Object);

        var content = new ParsedContent
        {
            InlineTags = new Dictionary<string, string>
            {
                ["vendor"] = "Mojo Coffee"
            },
            XmlIslands = new List<XmlIsland>
            {
                new XmlIsland { Name = "expense", Content = "<total>100</total>" }
            }
        };

        // Act
        var result = processor.CanProcess(content);

        // Assert
        result.Should().BeTrue("content with <expense> island should be processed as expense");
    }

    [Fact]
    public void CanProcess_WithoutExpenseIndicators_ShouldReturnFalse()
    {
        // Arrange
        var mockTaxCalculator = new Mock<ITaxCalculator>();
        var mockRepository = new Mock<IExpenseRepository>();
        var processor = new ExpenseProcessor(mockTaxCalculator.Object, mockRepository.Object);

        var content = new ParsedContent
        {
            InlineTags = new Dictionary<string, string>
            {
                ["reservation_date"] = "2024-12-25",
                ["party_size"] = "4"
            },
            XmlIslands = new List<XmlIsland>()
        };

        // Act
        var result = processor.CanProcess(content);

        // Assert
        result.Should().BeFalse("content without <total> tag or <expense> island should not be processed as expense");
    }

    #endregion
}
