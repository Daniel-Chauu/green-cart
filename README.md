# GreenCart API — Comprehensive Technical Documentation

> **Version**: 1.0.0  
> **Framework**: ASP.NET Core 8.0 LTS  
> **Database**: Microsoft SQL Server / Entity Framework Core 8  
> **Architecture**: Clean 3-Layer Architecture with Generic Repository & Unit of Work Patterns  
> **Target Audience**: Solution Architects, Backend Engineers, Frontend Engineers, DevOps, Academic Reviewers  

---

## Table of Contents
1. [Project Overview & Executive Summary](#1-project-overview--executive-summary)
2. [System Architecture](#2-system-architecture)
3. [Technology Stack & Dependencies](#3-technology-stack--dependencies)
4. [NuGet Package Reference](#4-nuget-package-reference)
5. [Database Design & Entity Schemas (ERD)](#5-database-design--entity-schemas-erd)
6. [Business Logic Workflows](#6-business-logic-workflows)
7. [Complete API Reference](#7-complete-api-reference)
8. [Security Implementation](#8-security-implementation)
9. [Payment Integration (VNPAY & COD)](#9-payment-integration-vnpay--cod)
10. [Frontend Integration Guide](#10-frontend-integration-guide)
11. [Testing Strategy & Coverage](#11-testing-strategy--coverage)
12. [Deployment & Containerization Guide](#12-deployment--containerization-guide)
13. [Troubleshooting & FAQ](#13-troubleshooting--faq)
14. [Appendix: PDF Export Instructions](#14-appendix-pdf-export-instructions)

---

## 1. Project Overview & Executive Summary

### 1.1 Overview
**GreenCart API** is a high-performance, enterprise-grade e-commerce RESTful Web API engineered with **ASP.NET Core 8.0**. Built specifically for an organic herbal supplement and wellness retailer, the system provides a robust backend capable of handling customer management, product cataloging, cart operations, transactional order fulfillment, online payment gateway integration, review moderation, and administrative business intelligence.

### 1.2 Key System Features
- **Authentication & Security**: JWT Authentication with Refresh Token Rotation, BCrypt password hashing (12 rounds), 6-digit numeric OTP password reset with a 2-minute expiry window, 60-second request rate limiting, and brute-force attempt invalidation (max 5 attempts).
- **Role-Based Access Control (RBAC)**: Fine-grained permissions for `Guest`, `Customer`, `Staff`, and `Admin` roles.
- **Product & Inventory Management**: Hierarchical multi-level category navigation, brand management, SKU tracking, inventory stock reservation, soft deletion across all entities, and multi-file image uploads served via ASP.NET Core Static File Middleware.
- **Order & Fulfillment Pipeline**: Transactional checkout handling (`IDbContextTransaction`) with stock verification, coupon/voucher validation, Cash on Delivery (COD) auto-settlement upon delivery, customer order cancellation with automatic stock restoration, and staff-wide order fulfillment tracking.
- **VNPAY Gateway Integration**: Full VNPAY Sandbox integration featuring native HMAC-SHA512 signature calculation/validation, payment URL generation, redirect return callback handling, server-to-server Instant Payment Notification (IPN) with idempotency protections, and payment status verification.
- **Review & Moderation System**: Verified purchase checking (only users with a `Delivered` order containing the item can review), 1-to-5 star rating calculation, and an Admin moderation approval queue (`IsApproved` workflow).
- **User Address Book**: Multi-address management per user with automatic single-default address promotion.
- **Admin Dashboard**: Real-time business intelligence featuring daily revenue metrics, order status distributions, customer registration totals, and top-selling product aggregation.

---

## 2. System Architecture

### 2.1 Architectural Pattern
The GreenCart API follows a strict **3-Layer Architecture** decoupled using the **Repository** and **Unit of Work** design patterns. This guarantees clear separation of concerns, high testability, maintainability, and independence from external frameworks.

```
       ┌─────────────────────────────────────────────────────────┐
       │                   PRESENTATION LAYER                    │
       │     Controllers / Middleware / DTOs / Filters / Swagger  │
       └───────────────────────────┬─────────────────────────────┘
                                   │
                                   ▼
       ┌─────────────────────────────────────────────────────────┐
       │                  BUSINESS LOGIC LAYER                   │
       │    Services (Auth, Product, Order, VnPay, Review, etc.) │
       └───────────────────────────┬─────────────────────────────┘
                                   │
                                   ▼
       ┌─────────────────────────────────────────────────────────┐
       │                   DATA ACCESS LAYER                     │
       │    Generic Repository / IUnitOfWork / EF Core DbContext  │
       └───────────────────────────┬─────────────────────────────┘
                                   │
                                   ▼
       ┌─────────────────────────────────────────────────────────┐
       │                 PERSISTENCE STORAGE                     │
       │            Microsoft SQL Server Database                │
       └─────────────────────────────────────────────────────────┘
```

### 2.2 Layer Breakdown

| Layer | Namespace/Directory | Primary Responsibilities |
|---|---|---|
| **Presentation** | `GreenCart.Controllers` | HTTP request routing, Model state validation, status code formatting, JWT authorization enforcement, and file upload parsing. |
| **Business Logic** | `GreenCart.Services` | Core domain rules, stock reservation, VNPAY signature hashing, OTP code generation, discount calculation, and email dispatch. |
| **Data Access** | `GreenCart.Repositories` | Data persistence, EF Core query optimization, `.Include()` eager loading, eager pagination (`PagedResult<T>`), and transaction control. |
| **Domain Model** | `GreenCart.Entities` | Domain entities, enums, BaseEntity properties (`CreatedAt`, `UpdatedAt`, `IsDeleted`), and Fluent API entity configurations. |

### 2.3 Key Design Patterns Applied
1. **Generic Repository Pattern** (`IGenericRepository<T>`): Encapsulates CRUD and LINQ query expressions for any entity inheriting from `BaseEntity`.
2. **Unit of Work Pattern** (`IUnitOfWork`): Manages a single `AppDbContext` instance across repositories, coordinating database writes and multi-entity database transactions.
3. **Options Pattern**: Strongly-typed binding of configuration sections (`Jwt`, `SmtpSettings`, `VnPaySettings`, `AppSettings`) injected via `IOptions<T>`.
4. **Data Transfer Object (DTO) Pattern**: Decouples external API representations from internal EF Core database entities.
5. **Middleware Pipeline**: Global exception capturing (`GlobalExceptionHandler`), ASP.NET Static Files for image delivery, and custom request/response logging (`LoggingMiddleware`).

### 2.4 Project Directory Structure
```
GreenCart/
├── Configuration/               # POCO settings models (Jwt, Smtp, VnPay, App)
├── Controllers/                 # REST Web API Controllers
│   ├── AddressesController.cs
│   ├── AuthController.cs
│   ├── CartController.cs
│   ├── CategoriesController.cs
│   ├── CouponsController.cs
│   ├── DashboardController.cs
│   ├── OrdersController.cs
│   ├── PaymentsController.cs
│   ├── ProductsController.cs
│   ├── ReviewsController.cs
│   ├── UsersController.cs
│   └── WishlistsController.cs
├── Data/                        # EF Core DbContext & Entity Configurations
│   ├── Configurations/          # Fluent API mappings per entity
│   ├── Seeders/                 # Automatic Admin/Staff DbInitializer
│   └── AppDbContext.cs
├── Dtos/                        # Request and Response DTO contracts
│   ├── Requests/                # Auth, Cart, Orders, Products, Payments, etc.
│   └── Responses/               # Structured API response models
├── Middleware/                  # Custom HTTP middleware (Logging, Exception)
├── Models/                      # Domain Entities & Enums
│   ├── Enums/                   # UserRole, OrderStatus, PaymentStatus
│   ├── BaseEntity.cs            # Id, CreatedAt, UpdatedAt, IsDeleted
│   ├── Category.cs
│   ├── Order.cs
│   ├── Product.cs
│   ├── ShippingAddress.cs
│   ├── User.cs
│   ├── Voucher.cs
│   └── ...
├── Repositories/                # Generic Repository & Unit of Work
│   ├── Helpers/                 # PagedResult<T> pagination helper
│   ├── GenericRepository.cs
│   ├── IUnitOfWork.cs
│   ├── OrderRepository.cs
│   └── UnitOfWork.cs
├── Services/                    # Application Business Logic
│   ├── AuthService.cs
│   ├── EmailService.cs
│   ├── OrderService.cs
│   ├── ProductService.cs
│   ├── VnPayService.cs
│   └── ...
├── Program.cs                   # Application entrypoint & DI container setup
└── appsettings.json             # Configuration file
```

---

## 3. Technology Stack & Dependencies

- **Runtime & Framework**: .NET 8.0 LTS / ASP.NET Core Web API
- **Database Server**: Microsoft SQL Server 2022 / LocalDB
- **Object-Relational Mapper (ORM)**: Entity Framework Core 8.0
- **Security & Cryptography**: BCrypt.Net-Next, System.IdentityModel.Tokens.Jwt, HMAC-SHA512
- **Email Delivery**: MailKit v4.17.0 & MimeKit
- **Payment Gateway**: VNPAY Sandbox API (v2.1.0 specification)
- **Unit & Integration Testing**: xUnit, Moq, Microsoft.AspNetCore.Mvc.Testing, EntityFrameworkCore.InMemory
- **API Documentation**: Swashbuckle.AspNetCore (Swagger / OpenAPI 3.0)
- **Containerization**: Docker & Docker Compose

---

## 4. NuGet Package Reference

### Main Web API Project (`GreenCart.csproj`)

| Package Name | Version | Primary Purpose |
|---|---|---|
| `Microsoft.EntityFrameworkCore` | `8.0.11` | Core Object-Relational Mapping (ORM) framework |
| `Microsoft.EntityFrameworkCore.SqlServer` | `8.0.11` | EF Core database provider for Microsoft SQL Server |
| `Microsoft.EntityFrameworkCore.Design` | `8.0.11` | Design-time tools for EF Core migrations |
| `Microsoft.EntityFrameworkCore.Tools` | `8.0.11` | Package Manager Console tools for migrations |
| `Microsoft.AspNetCore.Authentication.JwtBearer` | `8.0.11` | JWT Bearer token authentication middleware |
| `BCrypt.Net-Next` | `4.0.3` | Secure password hashing using BCrypt algorithm |
| `MailKit` | `4.17.0` | Asynchronous SMTP email client for MailKit/Gmail |
| `MimeKit` | `4.17.0` | MIME creation and HTML email parsing |
| `FluentValidation.AspNetCore` | `11.3.0` | Automatic ASP.NET Core request model validation |
| `Swashbuckle.AspNetCore` | `6.6.2` | Swagger UI and OpenAPI specification generator |

### Test Project (`GreenCart.Tests.csproj`)

| Package Name | Version | Primary Purpose |
|---|---|---|
| `xunit` | `2.9.3` | Test runner and framework |
| `xunit.runner.visualstudio` | `3.1.4` | Visual Studio and `dotnet test` integration |
| `Moq` | `4.20.72` | Mocking framework for unit testing dependencies |
| `Microsoft.AspNetCore.Mvc.Testing` | `8.0.11` | In-memory `WebApplicationFactory` for integration tests |
| `Microsoft.EntityFrameworkCore.InMemory` | `8.0.11` | High-speed in-memory database provider for tests |

---

## 5. Database Design & Entity Schemas (ERD)

### 5.1 Entity Relationship Diagram (Textual Representation)

```
 ┌──────────────┐          ┌────────────────────┐          ┌──────────────┐
 │     User     │1        *│  ShippingAddress   │          │   Category   │
 ├──────────────┤──────────├────────────────────┤          ├──────────────┤
 │ Id (PK)      │          │ Id (PK)            │          │ Id (PK)      │
 │ Email        │          │ UserId (FK)        │          │ Name         │
 │ PasswordHash │          └────────────────────┘          │ ParentId(FK) │
 │ Role         │                                          └──────┬───────┘
 └──────┬───────┘                                                 │1
        │1                                                        │
        │                                                         │*
        │*                                                 ┌──────┴───────┐
 ┌──────┴───────┐*        1┌────────────────────┐          │   Product    │
 │    Order     ├──────────┤      Voucher       │          ├──────────────┤
 ├──────────────┤          ├────────────────────┤          │ Id (PK)      │
 │ Id (PK)      │          │ Id (PK)            │          │ Category(FK) │
 │ UserId (FK)  │          │ Code               │          │ BrandId (FK) │
 │ Voucher(FK)  │          └────────────────────┘          └──────┬───────┘
 └──────┬───────┘                                                 │1
        │1                                                        │
        │                                                         │*
        │*                                                 ┌──────┴───────┐
 ┌──────┴───────┐                                          │ ProductImage │
 │ OrderDetail  │                                          ├──────────────┤
 ├──────────────┤                                          │ Id (PK)      │
 │ Id (PK)      │                                          │ ProductId(FK)│
 │ OrderId (FK) │                                          └──────────────┘
 │ Product(FK)  │
 └──────────────┘
```

### 5.2 Entity Schemas

#### 1. User (`Users` Table)
Inherits from `BaseEntity` (`Id`, `CreatedAt`, `UpdatedAt`, `IsDeleted`).

| Column Name | Data Type | Nullable | Constraints / Details |
|---|---|---|---|
| `Id` | `INT` | No | Primary Key, Identity(1,1) |
| `FullName` | `NVARCHAR(150)` | No | Full display name |
| `Email` | `VARCHAR(256)` | No | Unique Index, Lowercase enforced |
| `PasswordHash` | `NVARCHAR(MAX)` | No | BCrypt hashed string |
| `PhoneNumber` | `VARCHAR(20)` | Yes | Contact phone number |
| `Address` | `NVARCHAR(500)` | Yes | Default primary address |
| `Role` | `VARCHAR(50)` | No | Enum stored as String (`Customer`, `Staff`, `Admin`) |
| `RefreshToken` | `VARCHAR(256)` | Yes | Unique Index (filtered non-null) |
| `RefreshTokenExpiryTime` | `DATETIME2` | Yes | Token expiration date |
| `ResetToken` | `VARCHAR(256)` | Yes | 6-digit numeric OTP code |
| `ResetTokenExpiry` | `DATETIME2` | Yes | 2-minute OTP expiry timestamp |
| `FailedResetAttempts` | `INT` | No | Default `0` (Max 5 attempts before invalidation) |

#### 2. Category (`Categories` Table)

| Column Name | Data Type | Nullable | Constraints / Details |
|---|---|---|---|
| `Id` | `INT` | No | Primary Key, Identity(1,1) |
| `Name` | `NVARCHAR(100)` | No | Category name |
| `Slug` | `VARCHAR(150)` | No | URL-safe slug |
| `Description` | `NVARCHAR(500)` | Yes | Description text |
| `ParentCategoryId` | `INT` | Yes | Self-referencing FK (`Categories.Id`) |

#### 3. Product (`Products` Table)

| Column Name | Data Type | Nullable | Constraints / Details |
|---|---|---|---|
| `Id` | `INT` | No | Primary Key, Identity(1,1) |
| `Name` | `NVARCHAR(200)` | No | Product title |
| `Slug` | `VARCHAR(250)` | No | URL-safe slug |
| `SKU` | `VARCHAR(50)` | No | Stock Keeping Unit |
| `ShortDescription` | `NVARCHAR(500)` | Yes | Brief description |
| `Description` | `NVARCHAR(MAX)` | Yes | Full detailed text |
| `BasePrice` | `DECIMAL(18,2)` | No | Standard retail price |
| `SalePrice` | `DECIMAL(18,2)` | Yes | Promotional price |
| `StockQuantity` | `INT` | No | Current stock quantity |
| `RatingAverage` | `FLOAT` | No | Default `0.0` (Calculated from approved reviews) |
| `ReviewCount` | `INT` | No | Default `0` |
| `IsActive` | `BIT` | No | Default `1` |
| `CategoryId` | `INT` | No | Foreign Key (`Categories.Id`) |
| `BrandId` | `INT` | Yes | Foreign Key (`Brands.Id`) |

#### 4. Order (`Orders` Table)

| Column Name | Data Type | Nullable | Constraints / Details |
|---|---|---|---|
| `Id` | `INT` | No | Primary Key, Identity(1,1) |
| `OrderCode` | `VARCHAR(50)` | No | Unique Code (e.g. `GC-20260809-A1B2C3D4`) |
| `UserId` | `INT` | No | Foreign Key (`Users.Id`) |
| `VoucherId` | `INT` | Yes | Foreign Key (`Vouchers.Id`) |
| `OrderDate` | `DATETIME2` | No | Timestamp of placement |
| `Status` | `VARCHAR(50)` | No | Enum (`Pending`, `Confirmed`, `Processing`, `Shipping`, `Delivered`, `Cancelled`) |
| `PaymentStatus` | `VARCHAR(50)` | No | Enum (`Pending`, `Paid`, `Failed`, `Refunded`) |
| `PaymentMethod` | `VARCHAR(50)` | No | `COD` or `VNPAY` |
| `SubTotal` | `DECIMAL(18,2)` | No | Cart sum before discount |
| `DiscountAmount` | `DECIMAL(18,2)` | No | Applied voucher discount |
| `ShippingFee` | `DECIMAL(18,2)` | No | Standard shipping charge |
| `TotalAmount` | `DECIMAL(18,2)` | No | Final payable total |
| `ShippingAddress` | `NVARCHAR(500)` | No | Recipient shipping address |
| `RecipientName` | `NVARCHAR(150)` | No | Recipient contact name |
| `RecipientPhone` | `VARCHAR(20)` | No | Recipient phone number |
| `Note` | `NVARCHAR(500)` | Yes | Customer order delivery note |

#### 5. OrderDetail (`OrderDetails` Table)

| Column Name | Data Type | Nullable | Constraints / Details |
|---|---|---|---|
| `Id` | `INT` | No | Primary Key, Identity(1,1) |
| `OrderId` | `INT` | No | Foreign Key (`Orders.Id`) |
| `ProductId` | `INT` | No | Foreign Key (`Products.Id`) |
| `ProductName` | `NVARCHAR(200)` | No | Snapshot of product title at checkout |
| `UnitPrice` | `DECIMAL(18,2)` | No | Snapshot unit price |
| `Quantity` | `INT` | No | Quantity ordered |
| `TotalPrice` | `DECIMAL(18,2)` | No | `UnitPrice * Quantity` |

#### 6. Review (`Reviews` Table)

| Column Name | Data Type | Nullable | Constraints / Details |
|---|---|---|---|
| `Id` | `INT` | No | Primary Key, Identity(1,1) |
| `UserId` | `INT` | No | Foreign Key (`Users.Id`) |
| `ProductId` | `INT` | No | Foreign Key (`Products.Id`) |
| `Rating` | `INT` | No | 1 to 5 stars |
| `Comment` | `NVARCHAR(1000)` | Yes | Customer text review |
| `IsApproved` | `BIT` | No | Default `0` (Requires Admin approval) |

#### 7. ShippingAddress (`ShippingAddresses` Table)

| Column Name | Data Type | Nullable | Constraints / Details |
|---|---|---|---|
| `Id` | `INT` | No | Primary Key, Identity(1,1) |
| `UserId` | `INT` | No | Foreign Key (`Users.Id`) |
| `FullName` | `NVARCHAR(100)` | No | Recipient name |
| `PhoneNumber` | `VARCHAR(20)` | No | Recipient phone number |
| `AddressLine1` | `NVARCHAR(200)` | No | Street address |
| `AddressLine2` | `NVARCHAR(200)` | Yes | Apartment/Suite |
| `City` | `NVARCHAR(100)` | No | City |
| `State` | `NVARCHAR(100)` | No | Province / State |
| `PostalCode` | `VARCHAR(20)` | No | Zip / Postal Code |
| `Country` | `NVARCHAR(100)` | No | Country name |
| `IsDefault` | `BIT` | No | Default `0` |

---

## 6. Business Logic Workflows

### 6.1 Password Reset via 6-Digit OTP Flow
```
 User                       API / AuthService                       MailKit / Database
  │                                │                                         │
  ├─── 1. POST /forgot-password ──►│                                         │
  │    { email }                   ├─── 2. Check 60s Rate Limit              │
  │                                ├─── 3. Generate 6-Digit OTP Code        │
  │                                ├─── 4. Save ResetToken & 2-Min Expiry ──►│ User Table
  │                                ├─── 5. Send HTML Email ─────────────────►│ MailKit SMTP
  │◄── 6. "Success Message" ───────┤                                         │
  │                                │                                         │
  ├─── 7. POST /verify-reset-code ►│                                         │
  │    { email, code }             ├─── 8. Validate Token & Expiry           │
  │◄── 9. { isValid: true } ───────┤                                         │
  │                                │                                         │
  ├─── 10. POST /reset-password ──►│                                         │
  │    { email, code, newPass }    ├─── 11. Verify OTP & Failed Attempts     │
  │                                ├─── 12. Hash Password (BCrypt)           │
  │                                ├─── 13. Clear Token & Expiry ───────────►│ User Table
  │◄── 14. "Password Reset OK" ────┘                                         │
```

---

## 7. Complete API Reference

### 7.1 Authentication & Profile (`/api/auth`)

| Method | Endpoint | Authorization | Description |
|---|---|---|---|
| `POST` | `/api/auth/register` | Public | Register new customer (`Role = Customer`). Returns JWT tokens. |
| `POST` | `/api/auth/login` | Public | User login. Returns AccessToken + RefreshToken + Profile. |
| `POST` | `/api/auth/refresh-token` | Public | Exchange Refresh Token for new Access & Refresh tokens. |
| `POST` | `/api/auth/logout` | Authorized | Invalidates active user Refresh Token in DB. |
| `POST` | `/api/auth/forgot-password` | Public | Request 6-digit OTP code sent via email (60s rate limit). |
| `POST` | `/api/auth/verify-reset-code` | Public | Verify 6-digit OTP code before password entry screen. |
| `POST` | `/api/auth/reset-password` | Public | Reset password using 6-digit OTP code. |
| `GET` | `/api/auth/me` | Authorized | Fetch current user profile. |
| `PUT` | `/api/auth/profile` | Authorized | Update full name, phone number, address. |
| `POST` | `/api/auth/change-password` | Authorized | Change password for logged-in user. |

### 7.2 Categories (`/api/categories`)

| Method | Endpoint | Authorization | Description |
|---|---|---|---|
| `GET` | `/api/categories` | Public | Fetch full hierarchical tree of active categories. |
| `GET` | `/api/categories/{id}` | Public | Fetch specific category details by ID. |
| `POST` | `/api/categories` | Admin, Staff | Create a new product category. |
| `PUT` | `/api/categories/{id}` | Admin, Staff | Update category details. |
| `DELETE` | `/api/categories/{id}` | Admin, Staff | Soft delete category (Use `?force=true` for child categories). |

### 7.3 Products (`/api/products`)

| Method | Endpoint | Authorization | Description |
|---|---|---|---|
| `GET` | `/api/products` | Public | Search, filter by Category/Brand/Price, and paginate products. |
| `GET` | `/api/products/{id}` | Public | Fetch product by ID including active images. |
| `GET` | `/api/products/slug/{slug}` | Public | Fetch product by URL slug. |
| `POST` | `/api/products` | Admin, Staff | Create product with multipart image file uploads. |
| `PUT` | `/api/products/{id}` | Admin, Staff | Update product, add new images, or remove image IDs. |
| `DELETE` | `/api/products/{id}` | Admin, Staff | Soft delete product. |

### 7.4 Cart (`/api/cart`)

| Method | Endpoint | Authorization | Description |
|---|---|---|---|
| `GET` | `/api/cart` | Authorized | Fetch user's current shopping cart items and subtotal. |
| `POST` | `/api/cart` | Authorized | Add product to cart or increment quantity. |
| `PUT` | `/api/cart/items/{id}` | Authorized | Update cart item quantity. |
| `DELETE` | `/api/cart/items/{id}` | Authorized | Remove item from cart. |
| `DELETE` | `/api/cart` | Authorized | Clear entire cart. |

### 7.5 Orders (`/api/orders`)

| Method | Endpoint | Authorization | Description |
|---|---|---|---|
| `POST` | `/api/orders` | Authorized | Place an order (COD or VNPAY). |
| `GET` | `/api/orders` | Authorized | Fetch personal order history for logged-in user. |
| `GET` | `/api/orders/{id}` | Authorized | Fetch specific order details by ID. |
| `PUT` | `/api/orders/{id}/cancel` | Authorized | Cancel order (restores inventory stock). |
| `GET` | `/api/orders/admin` | Admin, Staff | Fetch system-wide customer orders with filtering & pagination. |
| `PUT` | `/api/orders/{id}/status` | Admin, Staff | Update order status (auto-triggers COD payment status). |

### 7.6 Payments (`/api/payments`)

| Method | Endpoint | Authorization | Description |
|---|---|---|---|
| `POST` | `/api/payments/vnpay/create` | Authorized | Generate signed VNPAY Sandbox payment URL for an order. |
| `GET` | `/api/payments/vnpay-return` | Public | VNPAY redirect return callback; redirects user to frontend. |
| `GET/POST`| `/api/payments/vnpay-ipn` | Public | VNPAY background IPN webhook handler with idempotency. |
| `GET` | `/api/payments/vnpay/status/{id}` | Authorized | Check current payment status of an order. |

### 7.7 Coupons (`/api/coupons`)

| Method | Endpoint | Authorization | Description |
|---|---|---|---|
| `POST` | `/api/coupons/apply` | Authorized | Validate voucher against cart and calculate discount preview. |

### 7.8 Addresses (`/api/addresses`)

| Method | Endpoint | Authorization | Description |
|---|---|---|---|
| `GET` | `/api/addresses` | Authorized | Fetch all saved shipping addresses for current user. |
| `POST` | `/api/addresses` | Authorized | Create new shipping address. |
| `PUT` | `/api/addresses/{id}` | Authorized | Update shipping address. |
| `DELETE` | `/api/addresses/{id}` | Authorized | Soft delete address. |
| `PATCH` | `/api/addresses/{id}/default`| Authorized | Promote address to default. |

### 7.9 Reviews (`/api/reviews`)

| Method | Endpoint | Authorization | Description |
|---|---|---|---|
| `GET` | `/api/reviews/products/{id}`| Public | Fetch approved customer reviews for a product. |
| `POST` | `/api/reviews` | Authorized | Submit review (Requires `Delivered` order containing product). |
| `GET` | `/api/reviews/admin` | Admin, Staff | Moderation queue for customer reviews. |
| `PUT` | `/api/reviews/admin/{id}/approve` | Admin, Staff | Approve review for public display. |
| `PUT` | `/api/reviews/admin/{id}/reject`  | Admin, Staff | Reject and soft delete review. |

### 7.10 Dashboard (`/api/dashboard`)

| Method | Endpoint | Authorization | Description |
|---|---|---|---|
| `GET` | `/api/dashboard/summary` | Admin, Staff | Fetch total revenue, orders, customers, and top products. |

---

## 8. Security Implementation

1. **Authentication**: JWT signed via HMAC-SHA512. Access Tokens expire after 15 minutes.
2. **Refresh Token Rotation**: Refresh tokens expire in 7 days and are rotated on every use to prevent replay attacks.
3. **Password Hashing**: Passwords are hashed using `BCrypt.Net-Next` with a work factor of 12.
4. **OTP Security**: 6-digit numeric codes generated via `Random.Shared`, 2-minute expiry, 60s request cooldown, and automatic token invalidation after 5 failed attempts.
5. **Soft Deletion**: Global EF Core Query Filter (`!e.IsDeleted`) guarantees data privacy while preserving audit trails.

---

## 9. Payment Integration (VNPAY & COD)

### 9.1 VNPAY Integration Workflow
1. User places order with `PaymentMethod = "VNPAY"`. Order is saved with `PaymentStatus = Pending`.
2. Frontend calls `POST /api/payments/vnpay/create` with `orderId`.
3. `VnPayService` formats `vnp_Amount` (order total * 100 in VND), constructs parameter dictionary, sorts alphabetically by key (`StringComparer.Ordinal`), and calculates HMAC-SHA512 hex signature using `HashSecret`.
4. User completes payment on VNPAY Sandbox page.
5. VNPAY redirects browser to `/api/payments/vnpay-return`. Signature is verified; `PaymentStatus` is updated to `Paid` (if `vnp_ResponseCode == "00"`).
6. VNPAY server issues asynchronous background HTTP request to `/api/payments/vnpay-ipn`. Signature, order existence, amount, and idempotency are validated before returning standard JSON `{ "RspCode": "00", "Message": "Confirm Success" }`.

---

## 10. Frontend Integration Guide

### 10.1 Axios Interceptor with Automatic Token Refresh

```javascript
import axios from 'axios';

const api = axios.create({
  baseURL: 'https://localhost:7123/api',
  headers: { 'Content-Type': 'application/json' },
});

// Attach Bearer Token to Requests
api.interceptors.request.use((config) => {
  const token = localStorage.getItem('accessToken');
  if (token) {
    config.headers.Authorization = `Bearer ${token}`;
  }
  return config;
});

// Automatic Refresh Token Handling
api.interceptors.response.use(
  (response) => response,
  async (error) => {
    const originalRequest = error.config;
    if (error.response?.status === 401 && !originalRequest._retry) {
      originalRequest._retry = true;
      try {
        const refreshToken = localStorage.getItem('refreshToken');
        const res = await axios.post('https://localhost:7123/api/auth/refresh-token', { refreshToken });
        
        const { accessToken, refreshToken: newRefreshToken } = res.data;
        localStorage.setItem('accessToken', accessToken);
        localStorage.setItem('refreshToken', newRefreshToken);
        
        originalRequest.headers.Authorization = `Bearer ${accessToken}`;
        return api(originalRequest);
      } catch (refreshErr) {
        localStorage.clear();
        window.location.href = '/login';
        return Promise.reject(refreshErr);
      }
    }
    return Promise.reject(error);
  }
);

export default api;
```

---

## 11. Testing Strategy & Coverage

The project maintains a unit and integration test suite executing via `xUnit` and `Moq`.

- **Unit Tests**: Test core domain rules in isolation (AuthService, OrderService, VnPayService, ReviewService, ProductService).
- **Integration Tests**: Use `WebApplicationFactory<Program>` with Entity Framework Core `InMemory` database provider to test end-to-end HTTP request processing, authorization middleware, database updates, and dashboard aggregations.

```bash
# Execute unit and integration tests
dotnet test GreenCart.Tests/GreenCart.Tests.csproj --verbosity normal
```

---

## 12. Deployment & Containerization Guide

### 12.1 Dockerfile (`GreenCart/Dockerfile`)

```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["GreenCart/GreenCart.csproj", "GreenCart/"]
RUN dotnet restore "GreenCart/GreenCart.csproj"
COPY . .
WORKDIR "/src/GreenCart"
RUN dotnet build "GreenCart.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "GreenCart.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "GreenCart.dll"]
```

### 12.2 Docker Compose (`docker-compose.yml`)

```yaml
version: '3.8'

services:
  sqlserver:
    image: mcr.microsoft.com/mssql/server:2022-latest
    container_name: greencart-db
    environment:
      - ACCEPT_EULA=Y
      - SA_PASSWORD=Your_password123!
    ports:
      - "1433:1433"

  api:
    image: greencart-api
    build:
      context: .
      dockerfile: GreenCart/Dockerfile
    ports:
      - "5000:80"
    environment:
      - ASPNETCORE_ENVIRONMENT=Production
      - ConnectionStrings__DBConnection=Server=sqlserver;Database=GreenCartDb;User Id=sa;Password=Your_password123!;TrustServerCertificate=True;
    depends_on:
      - sqlserver
```

---

## 13. Troubleshooting & FAQ

- **"Invalid VNPAY Signature"**: Ensure `HashSecret` matches the VNPAY merchant portal. Parameters must be sorted alphabetically by key using `Ordinal` comparison.
- **"Rate limit triggered for password reset"**: Wait 60 seconds between password reset code requests.
- **"Cannot review product"**: Only users with a `Delivered` order containing the specific product ID can post a review.

---

## 14. Appendix: PDF Export Instructions

To convert this technical documentation markdown file into a formatted PDF document:

```bash
# Convert markdown to PDF using Pandoc & wkhtmltopdf / pdfengine
pandoc greencart_technical_documentation.md -o greencart_technical_documentation.pdf --pdf-engine=xhtml2pdf
```
