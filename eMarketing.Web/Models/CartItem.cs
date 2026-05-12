using eMarketing.Service.Dtos;

namespace eMarketing.Web.Models;

public sealed class CartItem
{
    public ProductDto Product { get; init; } = new();
    public int Quantity { get; set; }
    public decimal LineTotal => Product.Fiyat * Quantity;
}
