using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using GreenCart.Data;
using GreenCart.Entities;
using Microsoft.EntityFrameworkCore.Storage;

namespace GreenCart.Repositories
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly AppDbContext _context;
        private IDbContextTransaction? _transaction;
        private readonly ConcurrentDictionary<Type, object> _repositories;

        private IProductRepository? _products;
        private IOrderRepository? _orders;
        private IGenericRepository<User>? _users;
        private IGenericRepository<Category>? _categories;
        private IGenericRepository<Brand>? _brands;
        private IGenericRepository<Inventory>? _inventories;
        private IGenericRepository<ProductImage>? _productImages;
        private IGenericRepository<CartItem>? _cartItems;
        private IGenericRepository<OrderDetail>? _orderDetails;
        private IGenericRepository<Voucher>? _vouchers;
        private IGenericRepository<Review>? _reviews;
        private IGenericRepository<Wishlist>? _wishlists;
        private IGenericRepository<ShippingAddress>? _shippingAddresses;

        public UnitOfWork(AppDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _repositories = new ConcurrentDictionary<Type, object>();
        }

        public IProductRepository Products => _products ??= new ProductRepository(_context);
        public IOrderRepository Orders => _orders ??= new OrderRepository(_context);
        public IGenericRepository<User> Users => _users ??= new GenericRepository<User>(_context);
        public IGenericRepository<Category> Categories => _categories ??= new GenericRepository<Category>(_context);
        public IGenericRepository<Brand> Brands => _brands ??= new GenericRepository<Brand>(_context);
        public IGenericRepository<Inventory> Inventories => _inventories ??= new GenericRepository<Inventory>(_context);
        public IGenericRepository<ProductImage> ProductImages => _productImages ??= new GenericRepository<ProductImage>(_context);
        public IGenericRepository<CartItem> CartItems => _cartItems ??= new GenericRepository<CartItem>(_context);
        public IGenericRepository<OrderDetail> OrderDetails => _orderDetails ??= new GenericRepository<OrderDetail>(_context);
        public IGenericRepository<Voucher> Vouchers => _vouchers ??= new GenericRepository<Voucher>(_context);
        public IGenericRepository<Review> Reviews => _reviews ??= new GenericRepository<Review>(_context);
        public IGenericRepository<Wishlist> Wishlists => _wishlists ??= new GenericRepository<Wishlist>(_context);
        public IGenericRepository<ShippingAddress> ShippingAddresses => _shippingAddresses ??= new GenericRepository<ShippingAddress>(_context);

        public IGenericRepository<T> Repository<T>() where T : BaseEntity
        {
            var type = typeof(T);

            if (!_repositories.ContainsKey(type))
            {
                var repositoryInstance = new GenericRepository<T>(_context);
                _repositories.TryAdd(type, repositoryInstance);
            }

            return (IGenericRepository<T>)_repositories[type];
        }

        public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            return await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task BeginTransactionAsync()
        {
            if (_transaction != null)
                return;

            _transaction = await _context.Database.BeginTransactionAsync();
        }

        public async Task CommitTransactionAsync()
        {
            if (_transaction == null)
                return;

            try
            {
                await SaveChangesAsync();
                await _transaction.CommitAsync();
            }
            catch
            {
                await RollbackTransactionAsync();
                throw;
            }
            finally
            {
                if (_transaction != null)
                {
                    _transaction.Dispose();
                    _transaction = null;
                }
            }
        }

        public async Task RollbackTransactionAsync()
        {
            if (_transaction == null)
                return;

            try
            {
                await _transaction.RollbackAsync();
            }
            finally
            {
                _transaction.Dispose();
                _transaction = null;
            }
        }

        public void Dispose()
        {
            _transaction?.Dispose();
            _context.Dispose();
            GC.SuppressFinalize(this);
        }

        public async ValueTask DisposeAsync()
        {
            if (_transaction != null)
            {
                await _transaction.DisposeAsync();
            }
            await _context.DisposeAsync();
            GC.SuppressFinalize(this);
        }
    }
}
