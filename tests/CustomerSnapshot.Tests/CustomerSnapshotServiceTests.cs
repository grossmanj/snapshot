using CustomerSnapshot.Application.Interfaces;
using CustomerSnapshot.Application.Services;
using CustomerSnapshot.Domain.Models;
using FluentAssertions;
using Moq;

namespace CustomerSnapshot.Tests;

public class CustomerSnapshotServiceTests
{
    [Fact]
    public async Task Returns_null_when_customer_missing()
    {
        var customerRepo = new Mock<ICustomerRepository>();
        customerRepo.Setup(r => r.GetCustomerAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Customer?)null);

        var service = new CustomerSnapshotService(
            customerRepo.Object,
            Mock.Of<ICustomerKpiService>(),
            Mock.Of<IQuoteRepository>(),
            Mock.Of<IIssueRepository>(),
            Mock.Of<IInteractionRepository>(),
            Mock.Of<IPersonalityProfileService>(),
            Mock.Of<ITalkingPointsService>(),
            Mock.Of<INextBestActionService>());

        var snapshot = await service.GetSnapshotAsync(99);

        snapshot.Should().BeNull();
    }
}
