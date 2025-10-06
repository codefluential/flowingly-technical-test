using FluentAssertions;
using Flowingly.ParsingService.Domain.Models;
using Flowingly.ParsingService.Infrastructure.Repositories;
using Xunit;

namespace Flowingly.ParsingService.UnitTests.Infrastructure;

/// <summary>
/// Tests verifying thread safety of InMemoryExpenseRepository.
/// Tests Issue #9 fix - ConcurrentDictionary usage.
/// </summary>
public class ConcurrentRepositoryTests
{
    [Fact]
    public async Task SaveAsync_WithConcurrentRequests_AllExpensesSavedWithoutLoss()
    {
        // Arrange
        var repository = new InMemoryExpenseRepository();
        var expenseCount = 100;
        var expenses = Enumerable.Range(1, expenseCount)
            .Select(i => new Expense
            {
                Vendor = $"Vendor{i}",
                Total = i * 100m,
                CostCentre = $"CC{i}"
            })
            .ToList();

        // Act - Save all expenses concurrently
        var saveTasks = expenses.Select(e => repository.SaveAsync(e));
        var savedIds = await Task.WhenAll(saveTasks);

        // Assert - All expenses saved without loss
        savedIds.Should().HaveCount(expenseCount);
        savedIds.Should().OnlyHaveUniqueItems();

        var allExpenses = await repository.GetAllAsync();
        allExpenses.Should().HaveCount(expenseCount);
    }

    [Fact]
    public async Task SaveAsync_WithConcurrentUpdates_LastWriteWins()
    {
        // Arrange
        var repository = new InMemoryExpenseRepository();
        var expenseId = Guid.NewGuid();
        var updateCount = 50;

        // Act - Update same expense concurrently
        var updateTasks = Enumerable.Range(1, updateCount)
            .Select(i => repository.SaveAsync(new Expense
            {
                Id = expenseId,
                Vendor = $"Vendor{i}",
                Total = i * 10m,
                CostCentre = "TEST"
            }));

        await Task.WhenAll(updateTasks);

        // Assert - No corruption, one version won
        var saved = await repository.GetByIdAsync(expenseId);
        saved.Should().NotBeNull();
        saved!.Id.Should().Be(expenseId);
        saved.CostCentre.Should().Be("TEST");
    }
}
