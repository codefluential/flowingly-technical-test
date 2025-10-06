using Flowingly.ParsingService.Domain.Models;

namespace Flowingly.ParsingService.Domain.Interfaces;

/// <summary>
/// Repository interface for expense persistence operations.
/// </summary>
public interface IExpenseRepository
{
    /// <summary>
    /// Saves an expense entity asynchronously.
    /// </summary>
    /// <param name="expense">The expense to save.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The unique identifier of the saved expense.</returns>
    Task<Guid> SaveAsync(Expense expense, CancellationToken ct);
}
