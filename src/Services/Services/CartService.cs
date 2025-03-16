using Microsoft.Extensions.DependencyInjection;
using Repositories.Repositories;

namespace Services.Services;

public interface ICartService
{
    // get cart by user id, including cart items

    // add product to cart
}

public class CartService(IServiceProvider serviceProvider) : ICartService
{
    private readonly ICartRepository _cartRepository = serviceProvider.GetRequiredService<ICartRepository>();
}