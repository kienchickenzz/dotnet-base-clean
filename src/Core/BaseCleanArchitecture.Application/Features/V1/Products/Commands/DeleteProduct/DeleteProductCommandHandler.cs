namespace BaseCleanArchitecture.Application.Features.V1.Products.Commands.DeleteProduct;

using BaseCleanArchitecture.Application.Common.Messaging;
using BaseCleanArchitecture.Application.Common.ApplicationServices.Repositories;
using BaseCleanArchitecture.Domain.AggregatesModels.Products;
using BaseCleanArchitecture.Domain.Primitives;


public sealed class DeleteProductCommandHandler : ICommandHandler<DeleteProductCommand, Guid>
{
    private readonly IProductRepository _productRepository;
    public DeleteProductCommandHandler(IProductRepository productRepository)
    {
        _productRepository= productRepository ?? throw new ArgumentNullException(nameof(productRepository));
    }

    public async Task<Result<Guid>> Handle(DeleteProductCommand request, CancellationToken cancellationToken)
    {
        var product = await _productRepository.GetByIdAsync(request.Id);
        if (product == null)
            return Result.Failure<Guid>(ProductErrors.NotFound);

        await _productRepository.DeleteAsync(product);

        return product.Id;
    }
}
