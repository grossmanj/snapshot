using CustomerSnapshot.Domain.Models;

namespace CustomerSnapshot.Application.Interfaces;

public interface ICustomerKpiService
{
    Task<CustomerKpis> GetCustomerKpisAsync(int customerId, CancellationToken cancellationToken = default);
}

public interface IPersonalityProfileService
{
    Task<PersonalityProfile> BuildProfileAsync(int customerId, CancellationToken cancellationToken = default);
}

public interface ITalkingPointsService
{
    Task<IReadOnlyList<TalkingPoint>> BuildTalkingPointsAsync(int customerId, PersonalityProfile profile, CancellationToken cancellationToken = default);
}

public interface INextBestActionService
{
    Task<NextBestAction> DetermineNextActionAsync(int customerId, CancellationToken cancellationToken = default);
}

public interface ICustomerSnapshotService
{
    Task<CustomerSnapshotViewModel?> GetSnapshotAsync(int customerId, CancellationToken cancellationToken = default);
}
