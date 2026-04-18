namespace BaseCleanArchitecture.Application.Features.V1.Products.Queries.GetProductById;

using BaseCleanArchitecture.Application.Common.Messaging;
using BaseCleanArchitecture.Application.Features.V1.Products.Models.Responses;


public sealed record GetProductByIdQuery(Guid Id) : IQuery<ProductResponse>;
