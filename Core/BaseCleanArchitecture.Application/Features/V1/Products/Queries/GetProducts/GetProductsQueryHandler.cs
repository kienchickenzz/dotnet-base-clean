/**
 * Handler for GetProductsQuery - retrieves paginated list of products.
 *
 * <p>Uses LINQ extensions for filtering, sorting, and pagination
 * instead of Specification pattern.</p>
 */
namespace BaseCleanArchitecture.Application.Features.V1.Products.Queries.GetProducts;

using BaseCleanArchitecture.Application.Common.ApplicationServices.Persistence;
using BaseCleanArchitecture.Application.Common.Extensions;
using BaseCleanArchitecture.Application.Common.Messaging;
using BaseCleanArchitecture.Application.Common.Models;
using BaseCleanArchitecture.Application.Features.V1.Products.Extensions;
using BaseCleanArchitecture.Application.Features.V1.Products.Models.Responses;
using BaseCleanArchitecture.Domain.Common;


public sealed class GetProductsQueryHandler : IQueryHandler<GetProductsQuery, PaginationResponse<ProductResponse>>
{
    private readonly IProductRepository _productRepository;

    public GetProductsQueryHandler(IProductRepository productRepository)
    {
        _productRepository = productRepository ?? throw new ArgumentNullException(nameof(productRepository));
    }

    public async Task<Result<PaginationResponse<ProductResponse>>> Handle(
        GetProductsQuery request,
        CancellationToken cancellationToken)
    {
        var result = await _productRepository.Query
            .WhereKeywordMatches(request.Keyword)
            .OrderByNewest()
            .SelectAsResponse()
            .ToPaginatedListAsync(
                request.PageNumber,
                request.PageSize,
                cancellationToken);

        return Result.Success(result);
    }
}
