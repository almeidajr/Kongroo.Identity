namespace Kongroo.BuildingBlocks.Application;

public interface IUnitOfWork
{
    Task CommitAsync(CancellationToken cancellationToken = default);
}
