namespace BaseCleanArchitecture.Persistence.Common;

using Ardalis.Specification.EntityFrameworkCore;
using Ardalis.Specification;
using Mapster;

using BaseCleanArchitecture.Application.Common.ApplicationServices.Persistence;
using BaseCleanArchitecture.Domain.Common;


public class Repository<TEntity> : RepositoryBase<TEntity>, IRepository<TEntity>
    where TEntity : BaseEntity
{
    private readonly ApplicationDbContext _dbContext;

    public Repository(ApplicationDbContext dbContext) : base(dbContext)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
    }

    public async Task SoftDeleteAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        entity.DeletedOn = DateTime.UtcNow;

        _dbContext.Set<TEntity>().Update(entity);

        await SaveChangesAsync(cancellationToken);
    }

    protected override IQueryable<TResult> ApplySpecification<TResult>(ISpecification<TEntity, TResult> specification) =>
        specification.Selector is not null
            ? base.ApplySpecification(specification)
            : ApplySpecification(specification, false)
                .ProjectToType<TResult>();
}
