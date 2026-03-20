namespace BaseCleanArchitecture.Application.Features.V1.Products.Queries.GetProducts;

using BaseCleanArchitecture.Application.Common.Messaging;
using BaseCleanArchitecture.Application.Common.Models;
using BaseCleanArchitecture.Application.Features.V1.Products.Models.Responses;


public sealed class GetProductsQuery : PaginationFilter, IQuery<PaginationResponse<ProductResponse>>
{
}
