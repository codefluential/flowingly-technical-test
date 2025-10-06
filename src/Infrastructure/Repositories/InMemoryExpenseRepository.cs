using Flowingly.ParsingService.Domain.Interfaces;
using Flowingly.ParsingService.Domain.Models;

namespace Flowingly.ParsingService.Infrastructure.Repositories;

/// <summary>
/// In-memory implementation of expense repository for development/testing.
/// Stores expenses in a concurrent dictionary for thread-safety.
/// </summary>
public class InMemoryExpenseRepository : IExpenseRepository
{
    private readonly Dictionary<Guid, Expense> _expenses = new();

    public Task<Guid> SaveAsync(Expense expense, CancellationToken ct = default)
    {
        if (expense.Id == Guid.Empty)
        {
            expense.Id = Guid.NewGuid();
        }

        _expenses[expense.Id] = expense;
        return Task.FromResult(expense.Id);
    }

    public Task<Expense?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        _expenses.TryGetValue(id, out var expense);
        return Task.FromResult(expense);
    }

    public Task<IEnumerable<Expense>> GetAllAsync(CancellationToken ct = default)
    {
        return Task.FromResult<IEnumerable<Expense>>(_expenses.Values.ToList());
    }
}
