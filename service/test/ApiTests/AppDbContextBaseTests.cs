using Medusa.Enap.Common.Core.Exceptions;
using Medusa.Enap.Common.Core.Persistence;
using Medusa.Enap.DemoService.Domain.Models;
using Microsoft.Extensions.DependencyInjection;

namespace ApiTests;

public class AppDbContextBaseTests

    : DemoServiceApiTestBase
{
    [Fact]
    public async Task EntitiesWithSameUniqueIdAdded_ShouldThrowUniqueKeyConflictException()
    {
        using var scope = Services.CreateScope();
        
        var repository = scope.ServiceProvider.GetRequiredService<IRepository<DemoModel>>();
        var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

        var uniqueId = Guid.NewGuid();

        AddDemoModelWithUniqueId(repository, uniqueId);
        AddDemoModelWithUniqueId(repository, uniqueId);

        await Assert.ThrowsAsync<UniqueKeyConflictException>(() => unitOfWork.SaveChangesAsync(CancellationToken.None));        
    }

    private void AddDemoModelWithUniqueId(IRepository<DemoModel> repository, Guid uniqueId)
    {
        repository.Add(new DemoModel()
        {
            UniqueId = uniqueId
        });

    }
}
