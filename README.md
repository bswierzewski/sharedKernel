# SharedKernel

[![Build Status](https://github.com/yourusername/SharedKernel/workflows/Publish%20NuGet%20Packages/badge.svg)](https://github.com/yourusername/SharedKernel/actions)
[![NuGet](https://img.shields.io/nuget/v/SharedKernel.Domain.svg)](https://www.nuget.org/packages/SharedKernel.Domain/)

Universal shared kernel components for .NET applications following Clean Architecture and Domain-Driven Design principles. Perfect for modular monoliths and microservices.

## 🚀 Quick Start - Modular Monolith Implementation

### 1. Installation

Install SharedKernel packages in your projects:

```bash
# In each module (Orders, Invoices, Contractors)
dotnet add package SharedKernel.Domain
dotnet add package SharedKernel.Application
dotnet add package SharedKernel.Infrastructure

# In Web project
dotnet add package SharedKernel.Application
dotnet add package SharedKernel.Infrastructure
```

### 2. Web Project Setup (Program.cs)

```csharp
var builder = WebApplication.CreateBuilder(args);

// Authentication & Authorization
builder.Services.AddAuthentication()...
builder.Services.AddAuthorization()...
builder.Services.AddHttpContextAccessor();

// 1. FIRST: Register SharedKernel services
builder.Services.AddSharedKernelApplication();
builder.Services.AddSharedKernelInfrastructure();

// 2. Register IUser implementation (for auditing)
builder.Services.AddScoped<IUser, CurrentUser>();

// 3. THEN: Register your modules
builder.Services.AddOrdersModule(builder.Configuration);
builder.Services.AddInvoicesModule(builder.Configuration);
builder.Services.AddContractorsModule(builder.Configuration);

var app = builder.Build();
```

### 3. IUser Implementation (Web/Infrastructure/CurrentUser.cs)

```csharp
using SharedKernel.Application.Abstractions;

public class CurrentUser : IUser
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUser(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public int? Id => GetUserId();

    private int? GetUserId()
    {
        var userIdClaim = _httpContextAccessor.HttpContext?.User
            .FindFirst(ClaimTypes.NameIdentifier)?.Value;

        return int.TryParse(userIdClaim, out var userId) ? userId : null;
    }
}
```

### 4. Module Structure (Example: Orders Module)

#### Domain Layer (Orders.Domain/Order.cs)

```csharp
using SharedKernel;

public class Order : AggregateRoot<int>
{
    public string CustomerName { get; private set; } = default!;
    public decimal TotalAmount { get; private set; }
    public OrderStatus Status { get; private set; }

    private readonly List<OrderItem> _items = new();
    public IReadOnlyCollection<OrderItem> Items => _items.AsReadOnly();

    private Order() { } // For EF Core

    public static Order Create(string customerName)
    {
        var order = new Order { CustomerName = customerName, Status = OrderStatus.Pending };
        order.AddDomainEvent(new OrderCreatedEvent(order.Id, customerName));
        return order;
    }

    public void AddItem(string productName, decimal price, int quantity)
    {
        _items.Add(new OrderItem(productName, price, quantity));
        RecalculateTotal();
    }

    private void RecalculateTotal() => TotalAmount = _items.Sum(i => i.Price * i.Quantity);
}

// Domain Event
public class OrderCreatedEvent : DomainEvent
{
    public int OrderId { get; }
    public string CustomerName { get; }

    public OrderCreatedEvent(int orderId, string customerName)
    {
        OrderId = orderId;
        CustomerName = customerName;
    }
}
```

#### Infrastructure Layer (Orders.Infrastructure/OrdersDbContext.cs)

```csharp
using Microsoft.EntityFrameworkCore;
using Orders.Domain;

public class OrdersDbContext : DbContext
{
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();

    public OrdersDbContext(DbContextOptions<OrdersDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Order>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.CustomerName).IsRequired().HasMaxLength(200);
            entity.Property(e => e.TotalAmount).HasColumnType("decimal(18,2)");

            entity.HasMany(e => e.Items)
                  .WithOne()
                  .HasForeignKey("OrderId")
                  .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
```

#### Module Dependency Injection (Orders.Infrastructure/DependencyInjection.cs)

```csharp
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SharedKernel.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddOrdersModule(this IServiceCollection services, IConfiguration configuration)
    {
        // 1. Register DbContext with SharedKernel interceptors
        services.AddDbContext<OrdersDbContext>((serviceProvider, options) =>
        {
            options.UseSqlServer(configuration.GetConnectionString("DefaultConnection"));
            // This adds AuditableEntityInterceptor and DispatchDomainEventsInterceptor
            options.AddInterceptors(serviceProvider);
        });

        // 2. Register repositories and UnitOfWork for this module
        services.AddRepositories<OrdersDbContext>();

        // 3. Optional: Add module-specific repositories
        // services.AddScoped<IOrderRepository, OrderRepository>();

        return services;
    }
}
```

#### Application Layer (Orders.Application/Commands/CreateOrderCommandHandler.cs)

```csharp
using MediatR;
using Orders.Domain;
using SharedKernel.Domain.Interfaces;

public record CreateOrderCommand(string CustomerName, List<OrderItemDto> Items) : IRequest<int>;
public record OrderItemDto(string ProductName, decimal Price, int Quantity);

public class CreateOrderCommandHandler : IRequestHandler<CreateOrderCommand, int>
{
    private readonly IRepository<Order> _orderRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateOrderCommandHandler(IRepository<Order> orderRepository, IUnitOfWork unitOfWork)
    {
        _orderRepository = orderRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<int> Handle(CreateOrderCommand request, CancellationToken cancellationToken)
    {
        // 1. Create aggregate root
        var order = Order.Create(request.CustomerName);

        // 2. Add items
        foreach (var item in request.Items)
        {
            order.AddItem(item.ProductName, item.Price, item.Quantity);
        }

        // 3. Add to repository
        await _orderRepository.AddAsync(order, cancellationToken);

        // 4. Save changes - this will:
        //    - Set audit fields (Created, CreatedBy, LastModified, LastModifiedBy)
        //    - Dispatch domain events (OrderCreatedEvent)
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return order.Id;
    }
}
```

#### Domain Event Handler (Orders.Application/EventHandlers/OrderCreatedEventHandler.cs)

```csharp
using MediatR;
using Microsoft.Extensions.Logging;
using Orders.Domain;

public class OrderCreatedEventHandler : INotificationHandler<OrderCreatedEvent>
{
    private readonly ILogger<OrderCreatedEventHandler> _logger;

    public OrderCreatedEventHandler(ILogger<OrderCreatedEventHandler> logger)
    {
        _logger = logger;
    }

    public async Task Handle(OrderCreatedEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Order {OrderId} was created for customer {CustomerName}",
            notification.OrderId, notification.CustomerName);

        // Here you can:
        // - Send notifications to other modules
        // - Trigger business processes
        // - Send emails, etc.

        await Task.CompletedTask;
    }
}
```

### 5. What You Get Automatically

#### Automatic Auditing

Every entity automatically gets:

- `Created` & `CreatedBy` - set when entity is first saved
- `LastModified` & `LastModifiedBy` - updated on every change

#### Domain Events

- Events are automatically dispatched after successful database save
- Perfect for cross-module communication in modular monolith

#### CQRS Pipeline Behaviors

- **Validation**: FluentValidation integration
- **Logging**: Request logging with user context
- **Performance**: Monitoring for slow requests (>500ms)
- **Exception Handling**: Centralized error handling

#### Repository Pattern

- Generic `IRepository<T>` with standard CRUD operations
- `IUnitOfWork` for transaction management
- Easy to extend with custom repositories

## 📦 Package Overview

### SharedKernel.Domain

Core building blocks for Domain-Driven Design:

- **`Entity<TId>`** - Base entity with audit fields
- **`AggregateRoot<TId>`** - Aggregate root with domain events
- **`ValueObject`** - Base value object with equality comparison
- **`DomainEvent`** - Base domain event for MediatR
- **`IRepository<T>`** - Generic repository interface
- **`IUnitOfWork`** - Unit of work pattern interface

### SharedKernel.Application

CQRS and application service patterns:

- **MediatR Behaviors**: Validation, Logging, Performance, Exception handling
- **`IUser`** - User context abstraction
- **`ValidationException`** - Custom validation exception

### SharedKernel.Infrastructure

Infrastructure implementations:

- **`BaseRepository<T>`** - Generic EF Core repository
- **`UnitOfWork<TContext>`** - EF Core unit of work
- **`AuditableEntityInterceptor`** - Automatic audit field population
- **`DispatchDomainEventsInterceptor`** - Automatic domain event dispatching
- **Extension methods** - Easy DI registration

## 🏗️ Architecture Patterns

### Clean Architecture Layers

```
┌─────────────────────────────────────┐
│              Web                    │ ← IUser implementation, DI setup
├─────────────────────────────────────┤
│         Module.Application          │ ← CQRS handlers, use cases
│     ┌─────────────────────────┐     │
│     │    Module.Domain        │     │ ← Entities, aggregates, events
│     │  ┌─────────────────┐    │     │
│     │  │  SharedKernel   │    │     │ ← Base classes, interfaces
│     │  │     Domain      │    │     │
│     │  └─────────────────┘    │     │
│     └─────────────────────────┘     │
├─────────────────────────────────────┤
│      Module.Infrastructure          │ ← DbContext, repositories
└─────────────────────────────────────┘
```

### Modular Monolith Structure

```
Solution/
├── src/
│   ├── SharedKernel.Domain/
│   ├── SharedKernel.Application/
│   ├── SharedKernel.Infrastructure/
│   ├── Orders/
│   │   ├── Orders.Domain/
│   │   ├── Orders.Application/
│   │   └── Orders.Infrastructure/
│   ├── Invoices/
│   │   ├── Invoices.Domain/
│   │   ├── Invoices.Application/
│   │   └── Invoices.Infrastructure/
│   └── Web/                      ← Bootstrapper project
└── Examples/                     ← Complete examples
```

## 🔧 Advanced Usage

### Custom Repository with Additional Methods

```csharp
public interface IOrderRepository : IRepository<Order>
{
    Task<List<Order>> GetOrdersByCustomerAsync(string customerName, CancellationToken cancellationToken = default);
    Task<List<Order>> GetPendingOrdersAsync(CancellationToken cancellationToken = default);
}

public class OrderRepository : BaseRepository<Order>, IOrderRepository
{
    public OrderRepository(OrdersDbContext context) : base(context) { }

    public async Task<List<Order>> GetOrdersByCustomerAsync(string customerName, CancellationToken cancellationToken = default)
    {
        return await DbSet.Where(o => o.CustomerName == customerName)
                          .ToListAsync(cancellationToken);
    }

    public async Task<List<Order>> GetPendingOrdersAsync(CancellationToken cancellationToken = default)
    {
        return await DbSet.Where(o => o.Status == OrderStatus.Pending)
                          .ToListAsync(cancellationToken);
    }
}

// Register custom repository
services.AddScoped<IOrderRepository, OrderRepository>();
```

### Value Objects Example

```csharp
public class Money : ValueObject
{
    public decimal Amount { get; }
    public string Currency { get; }

    public Money(decimal amount, string currency)
    {
        Amount = amount;
        Currency = currency;
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Amount;
        yield return Currency;
    }
}
```

### Cross-Module Communication via Domain Events

```csharp
// In Orders module - raise event
public class Order : AggregateRoot<int>
{
    public void Complete()
    {
        Status = OrderStatus.Completed;
        AddDomainEvent(new OrderCompletedEvent(Id, CustomerName, TotalAmount));
    }
}

// In Invoices module - handle event
public class CreateInvoiceWhenOrderCompletedHandler : INotificationHandler<OrderCompletedEvent>
{
    public async Task Handle(OrderCompletedEvent notification, CancellationToken cancellationToken)
    {
        var invoice = Invoice.CreateFromOrder(notification.OrderId, notification.TotalAmount);
        // ... create invoice
    }
}
```

## 📋 Requirements

- **.NET 9.0** or later
- **Entity Framework Core 9.0+**
- **MediatR 12.0+**
- **FluentValidation 12.0+**

## 🚢 Development & Contributing

### Building Locally

```bash
dotnet restore
dotnet build
dotnet pack --output ./artifacts/
```

### Project Structure

- **`/src`** - Source packages
- **`/Examples`** - Complete modular monolith examples
- **`/artifacts`** - Built NuGet packages

## 📄 License

MIT License - see [LICENSE](LICENSE) file for details.

## 🙏 Acknowledgments

- **Clean Architecture** by Robert C. Martin
- **Domain-Driven Design** by Eric Evans
- **MediatR** by Jimmy Bogard
- **Entity Framework Core** by Microsoft
