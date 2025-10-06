using FluentAssertions;
using Flowingly.ParsingService.Domain.Interfaces;
using Flowingly.ParsingService.Domain.Models;
using Flowingly.ParsingService.Domain.Services;
using Moq;
using Xunit;

namespace Flowingly.ParsingService.Tests.Services;

/// <summary>
/// TDD GREEN Phase: ContentRouter Tests
/// Tests for content classification and processor selection using Strategy pattern.
/// Implementation completed in task_028 (GREEN phase).
///
/// Router Algorithm:
/// 1. Iterate registered IContentProcessor instances
/// 2. Call CanProcess() on each processor in order
/// 3. Select first processor where CanProcess() returns true
/// 4. Call ProcessAsync() on selected processor
/// 5. Return ProcessingResult unchanged
///
/// Classification Rules:
/// - Content with <total> tag OR <expense> island → ExpenseProcessor
/// - Content without expense indicators → OtherProcessor (fallback)
/// </summary>
public class ContentRouterTests
{

    [Fact]
    public async Task RouteAsync_WithTotalTag_ShouldRouteToExpenseProcessor()
    {
        // Arrange
        var mockExpenseProcessor = new Mock<IContentProcessor>();
        mockExpenseProcessor.Setup(p => p.ContentType).Returns("expense");
        mockExpenseProcessor.Setup(p => p.CanProcess(It.IsAny<ParsedContent>())).Returns(true);
        mockExpenseProcessor.Setup(p => p.ProcessAsync(It.IsAny<ParsedContent>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ProcessingResult { Classification = "expense", Success = true });

        var mockOtherProcessor = new Mock<IContentProcessor>();
        mockOtherProcessor.Setup(p => p.ContentType).Returns("other");
        mockOtherProcessor.Setup(p => p.CanProcess(It.IsAny<ParsedContent>())).Returns(true);

        var processors = new[] { mockExpenseProcessor.Object, mockOtherProcessor.Object };
        var router = new ContentRouter(processors);

        var content = new ParsedContent
        {
            InlineTags = new Dictionary<string, string> { ["total"] = "100.00", ["vendor"] = "Mojo Coffee" },
            XmlIslands = new List<XmlIsland>()
        };

        // Act
        var result = await router.RouteAsync(content, CancellationToken.None);

        // Assert
        result.Classification.Should().Be("expense");
        mockExpenseProcessor.Verify(p => p.CanProcess(It.Is<ParsedContent>(c =>
            c.InlineTags.ContainsKey("total") && c.InlineTags["total"] == "100.00")), Times.Once);
        mockExpenseProcessor.Verify(p => p.ProcessAsync(It.IsAny<ParsedContent>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task RouteAsync_WithExpenseIsland_ShouldRouteToExpenseProcessor()
    {
        // Arrange
        var mockExpenseProcessor = new Mock<IContentProcessor>();
        mockExpenseProcessor.Setup(p => p.ContentType).Returns("expense");
        mockExpenseProcessor.Setup(p => p.CanProcess(It.IsAny<ParsedContent>())).Returns(true);
        mockExpenseProcessor.Setup(p => p.ProcessAsync(It.IsAny<ParsedContent>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ProcessingResult { Classification = "expense", Success = true });

        var processors = new[] { mockExpenseProcessor.Object };
        var router = new ContentRouter(processors);

        var content = new ParsedContent
        {
            InlineTags = new Dictionary<string, string> { ["vendor"] = "Mojo Coffee" },
            XmlIslands = new List<XmlIsland>
            {
                new XmlIsland { Name = "expense", Content = "<total>100</total>" }
            }
        };

        // Act
        var result = await router.RouteAsync(content, CancellationToken.None);

        // Assert
        result.Classification.Should().Be("expense");
        mockExpenseProcessor.Verify(p => p.CanProcess(It.Is<ParsedContent>(c =>
            c.XmlIslands.Any(i => i.Name == "expense"))), Times.Once);
        mockExpenseProcessor.Verify(p => p.ProcessAsync(It.IsAny<ParsedContent>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task RouteAsync_WithNoExpenseIndicators_ShouldRouteToOtherProcessor()
    {
        // Arrange
        var mockExpenseProcessor = new Mock<IContentProcessor>();
        mockExpenseProcessor.Setup(p => p.ContentType).Returns("expense");
        mockExpenseProcessor.Setup(p => p.CanProcess(It.IsAny<ParsedContent>())).Returns(false);

        var mockOtherProcessor = new Mock<IContentProcessor>();
        mockOtherProcessor.Setup(p => p.ContentType).Returns("other");
        mockOtherProcessor.Setup(p => p.CanProcess(It.IsAny<ParsedContent>())).Returns(true);
        mockOtherProcessor.Setup(p => p.ProcessAsync(It.IsAny<ParsedContent>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ProcessingResult { Classification = "other", Success = true });

        var processors = new[] { mockExpenseProcessor.Object, mockOtherProcessor.Object };
        var router = new ContentRouter(processors);

        var content = new ParsedContent
        {
            InlineTags = new Dictionary<string, string> { ["reservation_date"] = "2024-12-25", ["party_size"] = "4" },
            XmlIslands = new List<XmlIsland>()
        };

        // Act
        var result = await router.RouteAsync(content, CancellationToken.None);

        // Assert
        result.Classification.Should().Be("other");
        mockExpenseProcessor.Verify(p => p.CanProcess(It.IsAny<ParsedContent>()), Times.Once);
        mockOtherProcessor.Verify(p => p.CanProcess(It.IsAny<ParsedContent>()), Times.Once);
        mockOtherProcessor.Verify(p => p.ProcessAsync(It.IsAny<ParsedContent>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task RouteAsync_WithBothTotalAndIsland_ShouldRouteToExpenseProcessor()
    {
        // Arrange
        var mockExpenseProcessor = new Mock<IContentProcessor>();
        mockExpenseProcessor.Setup(p => p.ContentType).Returns("expense");
        mockExpenseProcessor.Setup(p => p.CanProcess(It.IsAny<ParsedContent>())).Returns(true);
        mockExpenseProcessor.Setup(p => p.ProcessAsync(It.IsAny<ParsedContent>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ProcessingResult { Classification = "expense", Success = true });

        var processors = new[] { mockExpenseProcessor.Object };
        var router = new ContentRouter(processors);

        var content = new ParsedContent
        {
            InlineTags = new Dictionary<string, string> { ["total"] = "50.00" },
            XmlIslands = new List<XmlIsland>
            {
                new XmlIsland { Name = "expense", Content = "<total>100</total>" }
            }
        };

        // Act
        var result = await router.RouteAsync(content, CancellationToken.None);

        // Assert
        result.Classification.Should().Be("expense");
        mockExpenseProcessor.Verify(p => p.CanProcess(It.Is<ParsedContent>(c =>
            c.InlineTags.ContainsKey("total") &&
            c.XmlIslands.Any(i => i.Name == "expense"))), Times.Once);
    }

    [Fact]
    public async Task RouteAsync_WithEmptyContent_ShouldRouteToOtherProcessor()
    {
        // Arrange
        var mockOtherProcessor = new Mock<IContentProcessor>();
        mockOtherProcessor.Setup(p => p.ContentType).Returns("other");
        mockOtherProcessor.Setup(p => p.CanProcess(It.IsAny<ParsedContent>())).Returns(true);
        mockOtherProcessor.Setup(p => p.ProcessAsync(It.IsAny<ParsedContent>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ProcessingResult { Classification = "other", Success = true });

        var processors = new[] { mockOtherProcessor.Object };
        var router = new ContentRouter(processors);

        var content = new ParsedContent
        {
            InlineTags = new Dictionary<string, string>(),
            XmlIslands = new List<XmlIsland>()
        };

        // Act
        var result = await router.RouteAsync(content, CancellationToken.None);

        // Assert
        result.Classification.Should().Be("other");
        mockOtherProcessor.Verify(p => p.CanProcess(It.Is<ParsedContent>(c =>
            c.InlineTags.Count == 0 && c.XmlIslands.Count == 0)), Times.Once);
        mockOtherProcessor.Verify(p => p.ProcessAsync(It.IsAny<ParsedContent>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task RouteAsync_WithMultipleProcessors_ShouldSelectFirstMatching()
    {
        // Arrange
        var mockProcessor1 = new Mock<IContentProcessor>();
        mockProcessor1.Setup(p => p.ContentType).Returns("processor1");
        mockProcessor1.Setup(p => p.CanProcess(It.IsAny<ParsedContent>())).Returns(false);

        var mockProcessor2 = new Mock<IContentProcessor>();
        mockProcessor2.Setup(p => p.ContentType).Returns("processor2");
        mockProcessor2.Setup(p => p.CanProcess(It.IsAny<ParsedContent>())).Returns(true);
        mockProcessor2.Setup(p => p.ProcessAsync(It.IsAny<ParsedContent>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ProcessingResult { Classification = "processor2", Success = true });

        var mockProcessor3 = new Mock<IContentProcessor>();
        mockProcessor3.Setup(p => p.ContentType).Returns("processor3");
        mockProcessor3.Setup(p => p.CanProcess(It.IsAny<ParsedContent>())).Returns(true);

        var processors = new[] { mockProcessor1.Object, mockProcessor2.Object, mockProcessor3.Object };
        var router = new ContentRouter(processors);

        var content = new ParsedContent
        {
            InlineTags = new Dictionary<string, string> { ["total"] = "100.00" },
            XmlIslands = new List<XmlIsland>()
        };

        // Act
        var result = await router.RouteAsync(content, CancellationToken.None);

        // Assert
        result.Classification.Should().Be("processor2");
        mockProcessor1.Verify(p => p.CanProcess(It.IsAny<ParsedContent>()), Times.Once);
        mockProcessor2.Verify(p => p.CanProcess(It.IsAny<ParsedContent>()), Times.Once);
        mockProcessor2.Verify(p => p.ProcessAsync(It.IsAny<ParsedContent>(), It.IsAny<CancellationToken>()), Times.Once);
        mockProcessor3.Verify(p => p.CanProcess(It.IsAny<ParsedContent>()), Times.Never);
        mockProcessor3.Verify(p => p.ProcessAsync(It.IsAny<ParsedContent>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task RouteAsync_ShouldPassParsedContentUnchanged()
    {
        // Arrange
        var mockProcessor = new Mock<IContentProcessor>();
        mockProcessor.Setup(p => p.ContentType).Returns("expense");
        mockProcessor.Setup(p => p.CanProcess(It.IsAny<ParsedContent>())).Returns(true);
        mockProcessor.Setup(p => p.ProcessAsync(It.IsAny<ParsedContent>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ProcessingResult { Classification = "expense", Success = true });

        var processors = new[] { mockProcessor.Object };
        var router = new ContentRouter(processors);

        var originalContent = new ParsedContent
        {
            InlineTags = new Dictionary<string, string> { ["total"] = "100.00", ["vendor"] = "Mojo" },
            XmlIslands = new List<XmlIsland>(),
            RawText = "Sample raw text"
        };

        // Act
        await router.RouteAsync(originalContent, CancellationToken.None);

        // Assert
        mockProcessor.Verify(p => p.ProcessAsync(
            It.Is<ParsedContent>(c =>
                c.InlineTags.Count == 2 &&
                c.InlineTags["total"] == "100.00" &&
                c.InlineTags["vendor"] == "Mojo" &&
                c.RawText == "Sample raw text"),
            It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task RouteAsync_ShouldPropagateCancellationToken()
    {
        // Arrange
        var cts = new CancellationTokenSource();
        var cancellationToken = cts.Token;

        var mockProcessor = new Mock<IContentProcessor>();
        mockProcessor.Setup(p => p.ContentType).Returns("expense");
        mockProcessor.Setup(p => p.CanProcess(It.IsAny<ParsedContent>())).Returns(true);
        mockProcessor.Setup(p => p.ProcessAsync(It.IsAny<ParsedContent>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ProcessingResult { Classification = "expense", Success = true });

        var processors = new[] { mockProcessor.Object };
        var router = new ContentRouter(processors);

        var content = new ParsedContent
        {
            InlineTags = new Dictionary<string, string> { ["total"] = "100.00" },
            XmlIslands = new List<XmlIsland>()
        };

        // Act
        await router.RouteAsync(content, cancellationToken);

        // Assert
        mockProcessor.Verify(p => p.ProcessAsync(
            It.IsAny<ParsedContent>(),
            It.Is<CancellationToken>(ct => ct == cancellationToken)),
            Times.Once);
    }

    [Fact]
    public async Task RouteAsync_ShouldReturnProcessingResultUnchanged()
    {
        // Arrange
        var expectedResult = new ProcessingResult
        {
            Classification = "expense",
            Success = true,
            Data = new { Total = 100.00m, Vendor = "Mojo" },
            ErrorCode = ""
        };

        var mockProcessor = new Mock<IContentProcessor>();
        mockProcessor.Setup(p => p.ContentType).Returns("expense");
        mockProcessor.Setup(p => p.CanProcess(It.IsAny<ParsedContent>())).Returns(true);
        mockProcessor.Setup(p => p.ProcessAsync(It.IsAny<ParsedContent>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResult);

        var processors = new[] { mockProcessor.Object };
        var router = new ContentRouter(processors);

        var content = new ParsedContent
        {
            InlineTags = new Dictionary<string, string> { ["total"] = "100.00" },
            XmlIslands = new List<XmlIsland>()
        };

        // Act
        var actualResult = await router.RouteAsync(content, CancellationToken.None);

        // Assert
        actualResult.Should().BeSameAs(expectedResult);
        actualResult.Classification.Should().Be("expense");
        actualResult.Success.Should().BeTrue();
        actualResult.ErrorCode.Should().BeEmpty();
    }

    [Fact]
    public async Task RouteAsync_ShouldCallCanProcessBeforeProcessAsync()
    {
        // Arrange
        var callSequence = new List<string>();

        var mockProcessor = new Mock<IContentProcessor>();
        mockProcessor.Setup(p => p.ContentType).Returns("expense");
        mockProcessor.Setup(p => p.CanProcess(It.IsAny<ParsedContent>()))
            .Returns(() =>
            {
                callSequence.Add("CanProcess");
                return true;
            });
        mockProcessor.Setup(p => p.ProcessAsync(It.IsAny<ParsedContent>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(() =>
            {
                callSequence.Add("ProcessAsync");
                return new ProcessingResult { Classification = "expense", Success = true };
            });

        var processors = new[] { mockProcessor.Object };
        var router = new ContentRouter(processors);

        var content = new ParsedContent
        {
            InlineTags = new Dictionary<string, string> { ["total"] = "100.00" },
            XmlIslands = new List<XmlIsland>()
        };

        // Act
        await router.RouteAsync(content, CancellationToken.None);

        // Assert
        callSequence.Should().HaveCount(2);
        callSequence[0].Should().Be("CanProcess");
        callSequence[1].Should().Be("ProcessAsync");
    }
}
