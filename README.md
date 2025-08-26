# SharedKernel

[![Build Status](https://github.com/yourusername/SharedKernel/workflows/Publish%20NuGet%20Packages/badge.svg)](https://github.com/yourusername/SharedKernel/actions)
[![NuGet](https://img.shields.io/nuget/v/SharedKernel.Domain.svg)](https://www.nuget.org/packages/SharedKernel.Domain/)

Universal shared kernel components for .NET applications following Clean Architecture and Domain-Driven Design principles.

## 📦 Packages

This repository contains three NuGet packages that provide essential building blocks for Clean Architecture applications:

### SharedKernel.Domain

Core domain entities, value objects, and domain events.

**Key Components:**

- `Entity<TId>` - Base class for all entities with audit fields
- `AggregateRoot<TId>` - Base class for aggregate roots with domain events
- `IDomainEvent` - Interface for domain events compatible with MediatR

### SharedKernel.Application

CQRS patterns, behaviors, and application services.

**Key Components:**

- `IUser` - User abstraction interface
- `ValidationBehaviour<,>` - FluentValidation pipeline behavior
- `LoggingBehaviour<>` - Request logging pipeline behavior
- `PerformanceBehaviour<,>` - Performance monitoring pipeline behavior
- `UnhandledExceptionBehaviour<,>` - Exception handling pipeline behavior
- `ValidationException` - Custom validation exception with error details

### SharedKernel.Infrastructure

Entity Framework interceptors, extensions, and infrastructure utilities.

**Key Components:**

- `AuditableEntityInterceptor` - Automatically sets audit fields on entities
- `DispatchDomainEventsInterceptor` - Automatically publishes domain events during SaveChanges
- Extension methods for dependency injection setup

## 🚀 Quick Start

### Installation

```bash
dotnet add package SharedKernel.Domain
dotnet add package SharedKernel.Application
dotnet add package SharedKernel.Infrastructure
```

Or install from GitHub Packages:

```xml
<PackageReference Include="SharedKernel.Domain" Version="1.0.0" />
<PackageReference Include="SharedKernel.Application" Version="1.0.0" />
<PackageReference Include="SharedKernel.Infrastructure" Version="1.0.0" />
```

### Basic Usage

#### 1. Domain Layer

```csharp
using SharedKernel.Domain.Common;

// Domain Event
public record ProductCreatedEvent(Guid ProductId, string Name) : IDomainEvent;

// Entity
public class Product : Entity<Guid>
{
    public string Name { get; private set; } = default!;
    public decimal Price { get; private set; }

    private Product(Guid id) : base(id) { }

    public static Product Create(string name, decimal price)
    {
        var product = new Product(Guid.NewGuid())
        {
            Name = name,
            Price = price
        };

        return product;
    }
}

// Aggregate Root
public class Order : AggregateRoot<Guid>
{
    private readonly List<OrderItem> _items = new();
    public IReadOnlyList<OrderItem> Items => _items.AsReadOnly();

    private Order(Guid id) : base(id) { }

    public static Order Create()
    {
        var order = new Order(Guid.NewGuid());
        order.AddDomainEvent(new OrderCreatedEvent(order.Id));
        return order;
    }

    public void AddItem(Product product, int quantity)
    {
        _items.Add(new OrderItem(product.Id, quantity, product.Price));
        AddDomainEvent(new OrderItemAddedEvent(Id, product.Id, quantity));
    }
}
```

#### 2. Application Layer

```csharp
using SharedKernel.Application;
using SharedKernel.Application.Abstractions;

// Register services
public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddSharedKernel(); // Registers MediatR behaviors and validation

        return services;
    }
}

// Command with validation
public record CreateProductCommand(string Name, decimal Price) : IRequest<Guid>;

public class CreateProductCommandValidator : AbstractValidator<CreateProductCommand>
{
    public CreateProductCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Price).GreaterThan(0);
    }
}

// Command Handler
public class CreateProductCommandHandler : IRequestHandler<CreateProductCommand, Guid>
{
    private readonly IApplicationDbContext _context;

    public CreateProductCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Guid> Handle(CreateProductCommand request, CancellationToken cancellationToken)
    {
        var product = Product.Create(request.Name, request.Price);

        _context.Products.Add(product);
        await _context.SaveChangesAsync(cancellationToken);

        return product.Id;
    }
}
```

#### 3. Infrastructure Layer

```csharp
using SharedKernel.Infrastructure;
using Microsoft.EntityFrameworkCore;

// DbContext with interceptors
public class ApplicationDbContext : DbContext, IApplicationDbContext
{
    public DbSet<Product> Products { get; set; }
    public DbSet<Order> Orders { get; set; }

    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.AddInterceptors(
            serviceProvider.GetRequiredService<AuditableEntityInterceptor>(),
            serviceProvider.GetRequiredService<DispatchDomainEventsInterceptor>()
        );
    }
}

// Register infrastructure services
public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        services.AddSharedKernelInfrastructure(); // Registers interceptors and TimeProvider

        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(connectionString));

        services.AddScoped<IApplicationDbContext>(provider =>
            provider.GetRequiredService<ApplicationDbContext>());

        return services;
    }
}
```

#### 4. Program.cs Setup

```csharp
var builder = WebApplication.CreateBuilder(args);

// Register all layers
builder.Services.AddApplication();
builder.Services.AddInfrastructure();

// User service implementation
builder.Services.AddScoped<IUser, CurrentUser>();

var app = builder.Build();
```

## 🏗️ Architecture

This SharedKernel follows Clean Architecture principles:

```
┌─────────────────────────────────────┐
│              Web/API                │
├─────────────────────────────────────┤
│           Application               │
│     ┌─────────────────────────┐     │
│     │       Domain            │     │
│     │  ┌─────────────────┐    │     │
│     │  │  SharedKernel   │    │     │
│     │  │     Domain      │    │     │
│     │  └─────────────────┘    │     │
│     └─────────────────────────┘     │
├─────────────────────────────────────┤
│          Infrastructure             │
└─────────────────────────────────────┘
```

## 🔧 Features

### Automatic Auditing

Entities automatically get audit fields populated:

- `Created` / `CreateBy` - Set when entity is first saved
- `Modified` / `ModifiedBy` - Updated on every save

### Domain Events

- Automatic domain event publishing during SaveChanges
- Integrates seamlessly with MediatR
- Events are dispatched after successful database transaction

### CQRS Pipeline Behaviors

- **Validation**: Automatic FluentValidation integration
- **Logging**: Request/response logging with user context
- **Performance**: Monitoring for long-running requests (>500ms)
- **Exception Handling**: Centralized exception logging

### Central Package Management

- All package versions managed centrally via `Directory.Packages.props`
- Consistent versioning across all packages
- Easy dependency updates

## 📋 Requirements

- .NET 9.0 or later
- Entity Framework Core 9.0+
- MediatR 12.0+
- FluentValidation 11.0+

## 🚢 Publishing

Packages are automatically published to:

- **GitHub Packages**: On every push to main branch and tags
- **NuGet.org**: On version tags (v1.0.0, v1.1.0, etc.)

### Creating a Release

1. Update version in `Directory.Build.props`
2. Create and push a version tag:
   ```bash
   git tag v1.0.0
   git push origin v1.0.0
   ```
3. Packages will be automatically built and published

## 🔨 Development

### Building Locally

```bash
dotnet restore
dotnet build
```

### Running Tests

```bash
dotnet test
```

### Creating Packages

```bash
dotnet pack --configuration Release --output ./artifacts/
```

## 🤝 Contributing

1. Fork the repository
2. Create a feature branch
3. Make your changes
4. Add tests if applicable
5. Create a pull request

## 📄 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## 🙏 Acknowledgments

- Inspired by Clean Architecture by Robert C. Martin
- Domain-Driven Design principles by Eric Evans
- MediatR library by Jimmy Bogard
