namespace BaseCleanArchitecture.Domain.AggregatesModels.Products;

using BaseCleanArchitecture.Domain.Common;


public static class ProductErrors
{
    public static Error NotFound = new(
        "Product.NotFound",
        "Product not found!");
}
