using System;
using System.Threading;
using System.Threading.Tasks;
using GreenCart.Entities;

namespace GreenCart.Repositories
{
    public interface IUnitOfWork : IDisposable, IAsyncDisposable
    {
        IProductRepository Products { get; }
        IOrderRepository Orders { get; }
        IGenericRepository<User> Users { get; }
        IGenericRepository<Category> Categories { get; }
        IGenericRepository<Brand> Brands { get; }
        IGenericRepository<Inventory> Inventories { get; }
        IGenericRepository<ProductImage> ProductImages { get; }
        IGenericRepository<CartItem> CartItems { get; }
        IGenericRepository<OrderDetail> OrderDetails { get; }
        IGenericRepository<Voucher> Vouchers { get; }
        IGenericRepository<Review> Reviews { get; }
        IGenericRepository<Wishlist> Wishlists { get; }
        IGenericRepository<ShippingAddress> ShippingAddresses { get; }

        IGenericRepository<T> Repository<T>() where T : BaseEntity;

        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
        Task BeginTransactionAsync();
        Task CommitTransactionAsync();
        Task RollbackTransactionAsync();
    }
}
