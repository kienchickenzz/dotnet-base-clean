namespace BaseCleanArchitecture.Application.Features.V1.Products.Commands.DeleteProduct;

using BaseCleanArchitecture.Application.Common.Messaging;


public sealed record DeleteProductCommand(int Id) : ICommand<int>;
