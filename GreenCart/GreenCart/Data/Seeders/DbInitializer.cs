using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GreenCart.Data;
using GreenCart.Entities;
using GreenCart.Entities.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace GreenCart.Data.Seeders
{
    public static class DbInitializer
    {
        public static async Task InitializeAsync(IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            // Ensure DB is created (especially useful for local/dev envs)
            await context.Database.EnsureCreatedAsync();

            // Check if already seeded (Idempotency check)
            if (await context.Users.AnyAsync())
            {
                return; // Database has already been seeded
            }

            // 1. Seed Users (Admin & Staff with full profile details)
            var adminUser = new User
            {
                FullName = "DuyTu",
                Email = "admin@greencart.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin@123"),
                PhoneNumber = "+84901234567",
                Address = "123 GreenCart HQ, Eco City",
                Role = UserRole.Admin,
                CreatedAt = DateTime.UtcNow,
                IsDeleted = false
            };

            var staffUser = new User
            {
                FullName = "QuangKiet",
                Email = "staff@greencart.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Staff@123"),
                PhoneNumber = "+84907654321",
                Address = "456 GreenCart Warehouse, Eco City",
                Role = UserRole.Staff,
                CreatedAt = DateTime.UtcNow,
                IsDeleted = false
            };

            var defaultCustomer = new User
            {
                FullName = "Jane Customer",
                Email = "customer@greencart.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Customer@123"),
                PhoneNumber = "+84988776655",
                Address = "789 Residential Street, Eco City",
                Role = UserRole.Customer,
                CreatedAt = DateTime.UtcNow,
                IsDeleted = false
            };

            await context.Users.AddRangeAsync(adminUser, staffUser, defaultCustomer);
            await context.SaveChangesAsync();

            // 2. Seed Categories
            var categories = new List<Category>
            {
                new Category { Name = "Herbal Teas", Slug = "herbal-teas", Description = "Organic relaxing and restorative herbal teas." },
                new Category { Name = "Vitamins & Minerals", Slug = "vitamins-minerals", Description = "Essential dietary supplements and vitamins." },
                new Category { Name = "Natural Skincare", Slug = "natural-skincare", Description = "Plant-based organic skincare products." }
            };
            await context.Categories.AddRangeAsync(categories);
            await context.SaveChangesAsync();

            // 3. Seed Brands
            var brands = new List<Brand>
            {
                new Brand { Name = "OrganicLife", Slug = "organic-life", Description = "Certified organic herbal remedies." },
                new Brand { Name = "PureNature", Slug = "pure-nature", Description = "Pure natural botanical products." },
                new Brand { Name = "HerbStrong", Slug = "herb-strong", Description = "Potent herbal extract formulations." }
            };
            await context.Brands.AddRangeAsync(brands);
            await context.SaveChangesAsync();

            // 4. Seed 10 Products with Inventories
            var products = new List<Product>
            {
                new Product
                {
                    Name = "Organic Chamomile Relaxation Tea",
                    Slug = "organic-chamomile-tea",
                    SKU = "TEA-CHAM-001",
                    ShortDescription = "Calming chamomile flower blend.",
                    Description = "Sustainably harvested 100% organic chamomile flowers for a peaceful night's rest.",
                    BasePrice = 14.99m,
                    SalePrice = 12.49m,
                    StockQuantity = 150,
                    CategoryId = categories[0].Id,
                    BrandId = brands[0].Id,
                    IsActive = true
                },
                new Product
                {
                    Name = "Peppermint Digestive Infusion",
                    Slug = "peppermint-digestive-tea",
                    SKU = "TEA-PEP-002",
                    ShortDescription = "Refreshing peppermint leaf tea.",
                    Description = "Pure dried peppermint leaves promoting healthy digestive wellness.",
                    BasePrice = 12.99m,
                    StockQuantity = 120,
                    CategoryId = categories[0].Id,
                    BrandId = brands[1].Id,
                    IsActive = true
                },
                new Product
                {
                    Name = "Elderberry Immune Support Elixir",
                    Slug = "elderberry-immune-support",
                    SKU = "VIT-ELD-003",
                    ShortDescription = "Potent elderberry and vitamin C supplement.",
                    Description = "Concentrated black elderberry extract enriched with zinc and vitamin C for immune health.",
                    BasePrice = 24.99m,
                    SalePrice = 19.99m,
                    StockQuantity = 80,
                    CategoryId = categories[1].Id,
                    BrandId = brands[2].Id,
                    IsActive = true
                },
                new Product
                {
                    Name = "Ashwagandha KSM-66 Stress Balance",
                    Slug = "ashwagandha-ksm66",
                    SKU = "VIT-ASH-004",
                    ShortDescription = "Organic adaptogenic ashwagandha root.",
                    Description = "Full-spectrum ashwagandha root extract supporting stress response and vitality.",
                    BasePrice = 29.99m,
                    StockQuantity = 100,
                    CategoryId = categories[1].Id,
                    BrandId = brands[0].Id,
                    IsActive = true
                },
                new Product
                {
                    Name = "Organic Rosehip Hydrating Face Cream",
                    Slug = "rosehip-face-cream",
                    SKU = "SKIN-ROSE-005",
                    ShortDescription = "Nourishing wild rosehip seed oil cream.",
                    Description = "Rich organic facial moisturizer formulated with cold-pressed rosehip seed oil.",
                    BasePrice = 34.99m,
                    SalePrice = 29.99m,
                    StockQuantity = 60,
                    CategoryId = categories[2].Id,
                    BrandId = brands[1].Id,
                    IsActive = true
                },
                new Product
                {
                    Name = "Aloe Vera & Cucumber Soothing Gel",
                    Slug = "aloe-cucumber-gel",
                    SKU = "SKIN-ALOE-006",
                    ShortDescription = "Cooling organic aloe vera body gel.",
                    Description = "Hydrating cooling gel infused with organic aloe inner leaf gel and fresh cucumber.",
                    BasePrice = 18.50m,
                    StockQuantity = 90,
                    CategoryId = categories[2].Id,
                    BrandId = brands[2].Id,
                    IsActive = true
                },
                new Product
                {
                    Name = "Green Tea Detox Booster",
                    Slug = "green-tea-detox",
                    SKU = "TEA-GRN-007",
                    ShortDescription = "Antioxidant green tea blend.",
                    Description = "Premium organic Japanese sencha leaves combined with lemongrass and ginger.",
                    BasePrice = 15.99m,
                    StockQuantity = 110,
                    CategoryId = categories[0].Id,
                    BrandId = brands[0].Id,
                    IsActive = true
                },
                new Product
                {
                    Name = "Vitamin D3 + K2 Liquid Drops",
                    Slug = "vitamin-d3-k2-drops",
                    SKU = "VIT-D3K2-008",
                    ShortDescription = "Highly bioavailable bone & heart drops.",
                    Description = "Synergistic plant-sourced D3 and Menaquinone-7 K2 in organic coconut MCT oil.",
                    BasePrice = 21.99m,
                    StockQuantity = 75,
                    CategoryId = categories[1].Id,
                    BrandId = brands[1].Id,
                    IsActive = true
                },
                new Product
                {
                    Name = "Jojoba & Argan Hair Serum",
                    Slug = "jojoba-argan-serum",
                    SKU = "SKIN-HAIR-009",
                    ShortDescription = "Restorative organic hair oil serum.",
                    Description = "Deep conditioning serum with cold-pressed Moroccan argan oil and golden jojoba oil.",
                    BasePrice = 27.99m,
                    SalePrice = 22.99m,
                    StockQuantity = 50,
                    CategoryId = categories[2].Id,
                    BrandId = brands[2].Id,
                    IsActive = true
                },
                new Product
                {
                    Name = "Golden Turmeric Curcumin Complex",
                    Slug = "turmeric-curcumin-complex",
                    SKU = "VIT-TURM-010",
                    ShortDescription = "High-potency joint health complex.",
                    Description = "Standardized 95% curcuminoids enhanced with BioPerine black pepper extract.",
                    BasePrice = 26.50m,
                    StockQuantity = 95,
                    CategoryId = categories[1].Id,
                    BrandId = brands[0].Id,
                    IsActive = true
                }
            };

            await context.Products.AddRangeAsync(products);
            await context.SaveChangesAsync();

            // Create initial Inventory records for each product
            foreach (var p in products)
            {
                context.Inventories.Add(new Inventory
                {
                    ProductId = p.Id,
                    Quantity = p.StockQuantity,
                    ReservedQuantity = 0,
                    Location = "Main Warehouse - A1",
                    MinimumStockLevel = 10,
                    ReorderQuantity = 50,
                    LastRestockedAt = DateTime.UtcNow
                });
            }
            await context.SaveChangesAsync();

            // 5. Seed 1 Active Voucher (WELCOME10)
            var voucher = new Voucher
            {
                Code = "WELCOME10",
                Description = "10% off welcome voucher for orders over $50",
                DiscountType = "Percentage",
                DiscountValue = 10.0m, // 10%
                MinimumOrderAmount = 50.0m,
                MaximumDiscountAmount = 25.0m,
                StartDate = DateTime.UtcNow.AddDays(-1),
                EndDate = DateTime.UtcNow.AddYears(1),
                UsageLimit = 500,
                TimesUsed = 0,
                IsActive = true
            };
            await context.Vouchers.AddAsync(voucher);
            await context.SaveChangesAsync();
        }
    }
}
