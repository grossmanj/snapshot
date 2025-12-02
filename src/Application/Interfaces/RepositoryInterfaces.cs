using CustomerSnapshot.Domain.Models;

namespace CustomerSnapshot.Application.Interfaces;

public interface ICustomerRepository
{
    Task<Customer?> GetCustomerAsync(int customerId, CancellationToken cancellationToken = default);
}

public interface IOrderRepository
{
    Task<IReadOnlyList<Order>> GetOrdersAsync(int customerId, DateTime? from = null, DateTime? to = null, CancellationToken cancellationToken = default);
}

public interface IInvoiceRepository
{
    Task<IReadOnlyList<Invoice>> GetInvoicesAsync(int customerId, CancellationToken cancellationToken = default);
}

public interface IQuoteRepository
{
    Task<IReadOnlyList<Quote>> GetOpenQuotesAsync(int customerId, int top = 3, CancellationToken cancellationToken = default);
}

public interface IIssueRepository
{
    Task<IReadOnlyList<Issue>> GetOpenIssuesAsync(int customerId, int top = 3, CancellationToken cancellationToken = default);
}

public interface IInteractionRepository
{
    Task<IReadOnlyList<Interaction>> GetRecentInteractionsAsync(int customerId, int take, CancellationToken cancellationToken = default);
}
