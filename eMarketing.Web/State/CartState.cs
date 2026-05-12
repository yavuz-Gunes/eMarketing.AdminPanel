using eMarketing.Service.Dtos;
using eMarketing.Web.Models;

namespace eMarketing.Web.State;

public sealed class CartState
{
    public List<CartItem> Items { get; } = [];
    public decimal Total => Items.Sum(item => item.LineTotal);
    public int Count => Items.Sum(item => item.Quantity);
    public event Action? Changed;

    public void Add(ProductDto product)
    {
        CartItem? item = Items.FirstOrDefault(x => x.Product.UrunId == product.UrunId);
        if (item is null)
            Items.Add(new CartItem { Product = product, Quantity = 1 });
        else
            item.Quantity++;

        Changed?.Invoke();
    }

    public void Increase(int productId)
    {
        CartItem? item = Items.FirstOrDefault(x => x.Product.UrunId == productId);
        if (item is not null)
            item.Quantity++;

        Changed?.Invoke();
    }

    public void Decrease(int productId)
    {
        CartItem? item = Items.FirstOrDefault(x => x.Product.UrunId == productId);
        if (item is null)
            return;

        item.Quantity--;
        if (item.Quantity <= 0)
            Items.Remove(item);

        Changed?.Invoke();
    }

    public void Remove(int productId)
    {
        Items.RemoveAll(x => x.Product.UrunId == productId);
        Changed?.Invoke();
    }

    public void Clear()
    {
        Items.Clear();
        Changed?.Invoke();
    }
}
