/**
 * Repository implementation for Product aggregate.
 *
 * <p>Inherits all CRUD operations from base Repository.
 * Add domain-specific query methods here if needed.</p>
 */
namespace BaseCleanArchitecture.Persistence.Repositories;

using BaseCleanArchitecture.Domain.AggregatesModels.Products;

using BaseCleanArchitecture.Application.Common.ApplicationServices.Repositories;

using BaseCleanArchitecture.Persistence.Common;
using BaseCleanArchitecture.Persistence.DatabaseContext;


public class ProductRepository : Repository<Product>, IProductRepository
{
    public ProductRepository(ApplicationDbContext dbContext) : base(dbContext)
    {
    }
}
