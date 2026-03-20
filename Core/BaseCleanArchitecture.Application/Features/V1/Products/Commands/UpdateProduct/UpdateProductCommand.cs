namespace BaseCleanArchitecture.Application.Features.V1.Products.Commands.UpdateProduct;

using BaseCleanArchitecture.Application.Common.Messaging;


public sealed record UpdateProductCommand(
    int Id,
    string Name,
    string? Description,
    decimal Price) : ICommand<int>;
