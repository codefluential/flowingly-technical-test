using FluentAssertions;
using Flowingly.ParsingService.Domain.Validation;
using Flowingly.ParsingService.Domain.Exceptions;
using Xunit;

namespace Flowingly.ParsingService.Tests.Validators;

/// <summary>
/// TDD RED Phase: Tag Validation Tests
/// Tests for stack-based tag validator that detects overlapping and unclosed tags.
/// All tests MUST fail initially - implementation comes in task_015 (GREEN phase).
/// </summary>
public class TagValidatorTests
{
    [Fact]
    public void Validate_OverlappingTags_ShouldThrowValidationException()
    {
        // Arrange
        var validator = new TagValidator();
        var input = "<a><b></a></b>";

        // Act
        Action act = () => validator.Validate(input);

        // Assert
        act.Should().Throw<ValidationException>()
            .WithMessage("*UNCLOSED_TAGS*")
            .And.ErrorCode.Should().Be("UNCLOSED_TAGS");
    }

    [Fact]
    public void Validate_UnclosedTags_ShouldThrowValidationException()
    {
        // Arrange
        var validator = new TagValidator();
        var input = "<a><b>";

        // Act
        Action act = () => validator.Validate(input);

        // Assert
        act.Should().Throw<ValidationException>()
            .WithMessage("*UNCLOSED_TAGS*")
            .And.ErrorCode.Should().Be("UNCLOSED_TAGS");
    }

    [Fact]
    public void Validate_ProperNesting_ShouldNotThrowException()
    {
        // Arrange
        var validator = new TagValidator();
        var input = "<a><b></b></a>";

        // Act
        Action act = () => validator.Validate(input);

        // Assert
        act.Should().NotThrow();
    }

    [Fact]
    public void Validate_SelfClosingTags_ShouldNotThrowException()
    {
        // Arrange
        var validator = new TagValidator();
        var input = "<expense><total>100</total></expense>";

        // Act
        Action act = () => validator.Validate(input);

        // Assert
        act.Should().NotThrow();
    }

    [Fact]
    public void Validate_MultipleNestingLevels_ShouldNotThrowException()
    {
        // Arrange
        var validator = new TagValidator();
        var input = "<a><b><c></c></b></a>";

        // Act
        Action act = () => validator.Validate(input);

        // Assert
        act.Should().NotThrow();
    }

    [Fact]
    public void Validate_EmptyContent_ShouldNotThrowException()
    {
        // Arrange
        var validator = new TagValidator();
        var input = "";

        // Act
        Action act = () => validator.Validate(input);

        // Assert
        act.Should().NotThrow();
    }

    [Fact]
    public void Validate_MixedOverlappingAtDifferentLevels_ShouldThrowValidationException()
    {
        // Arrange
        var validator = new TagValidator();
        var input = "<expense><a><b></expense></b></a>";

        // Act
        Action act = () => validator.Validate(input);

        // Assert
        act.Should().Throw<ValidationException>()
            .WithMessage("*UNCLOSED_TAGS*")
            .And.ErrorCode.Should().Be("UNCLOSED_TAGS");
    }

    [Fact]
    public void Validate_OnlyOpeningTags_ShouldThrowValidationException()
    {
        // Arrange
        var validator = new TagValidator();
        var input = "<a><b><c>";

        // Act
        Action act = () => validator.Validate(input);

        // Assert
        act.Should().Throw<ValidationException>()
            .WithMessage("*UNCLOSED_TAGS*")
            .And.ErrorCode.Should().Be("UNCLOSED_TAGS");
    }

    [Fact]
    public void Validate_OnlyClosingTags_ShouldThrowValidationException()
    {
        // Arrange
        var validator = new TagValidator();
        var input = "</a></b></c>";

        // Act
        Action act = () => validator.Validate(input);

        // Assert
        act.Should().Throw<ValidationException>()
            .WithMessage("*UNCLOSED_TAGS*")
            .And.ErrorCode.Should().Be("UNCLOSED_TAGS");
    }

    [Fact]
    public void Validate_MismatchedTagNames_ShouldThrowValidationException()
    {
        // Arrange
        var validator = new TagValidator();
        var input = "<expense></total>";

        // Act
        Action act = () => validator.Validate(input);

        // Assert
        act.Should().Throw<ValidationException>()
            .WithMessage("*UNCLOSED_TAGS*")
            .And.ErrorCode.Should().Be("UNCLOSED_TAGS");
    }
}
