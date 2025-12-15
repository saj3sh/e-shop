# DDD Implementation Improvements

This document summarizes the Domain-Driven Design patterns that were implemented to improve the architecture.

## Summary of Changes

### ✅ 1. Base Entity Class

**Location:** `src/EShop.Domain/Common/Entity.cs`

- Created abstract `Entity<TId>` base class with proper identity and equality semantics
- Implements `Equals`, `GetHashCode`, and equality operators based on ID
- Prevents comparison of entities with different types
- Updated `OrderItem` and `RefreshToken` to extend `Entity<Guid>`

**Benefits:**

- Consistent identity comparison across all entities
- Proper equality semantics for domain entities
- Type-safe entity operations

---

### ✅ 2. Unit of Work Pattern

**Location:**

- `src/EShop.Application/Common/IUnitOfWork.cs`
- `src/EShop.Infrastructure/Persistence/UnitOfWork.cs`

- Centralizes transaction management and SaveChanges coordination
- Automatically collects and dispatches domain events from aggregates
- Supports explicit transaction control (Begin/Commit/Rollback)
- Dispatches events **after** successful save to maintain consistency

**Key Implementation:**

```csharp
public async Task<int> SaveChangesAsync(CancellationToken ct)
{
    // 1. Collect domain events from all aggregates
    var domainEvents = aggregates.SelectMany(a => a.DomainEvents).ToList();

    // 2. Clear events (prevents re-dispatching)
    foreach (var aggregate in aggregates)
        aggregate.ClearDomainEvents();

    // 3. Save to database
    var result = await _context.SaveChangesAsync(ct);

    // 4. Dispatch events after successful save
    await _eventDispatcher.DispatchAsync(domainEvents, ct);

    return result;
}
```

**Benefits:**

- Single save point for all changes
- Automatic domain event dispatching
- Transaction boundaries respected
- Decouples repositories from persistence concerns

---

### ✅ 3. Domain Event Dispatcher

**Location:**

- `src/EShop.Application/Common/IDomainEventDispatcher.cs`
- `src/EShop.Application/Common/IDomainEventHandler.cs`
- `src/EShop.Infrastructure/Services/DomainEventDispatcher.cs`
- `src/EShop.Application/Orders/EventHandlers/OrderPlacedEventHandler.cs`
- `src/EShop.Application/Orders/EventHandlers/OrderCompletedEventHandler.cs`

- Publishes domain events to registered handlers using dependency injection
- Supports multiple handlers per event type
- Uses reflection to find and invoke handlers dynamically

**Example Handlers:**

- `OrderPlacedEventHandler` - Handles order placement (email, inventory, logging)
- `OrderCompletedEventHandler` - Handles order completion (notifications, loyalty points)

**Registration in Program.cs:**

```csharp
builder.Services.AddScoped<IDomainEventDispatcher, DomainEventDispatcher>();
builder.Services.AddScoped<IDomainEventHandler<OrderPlaced>, OrderPlacedEventHandler>();
builder.Services.AddScoped<IDomainEventHandler<OrderCompleted>, OrderCompletedEventHandler>();
```

**Benefits:**

- Decouples domain logic from side effects
- Easy to add new event handlers without changing aggregates
- Promotes single responsibility principle
- Enables cross-aggregate business processes

---

### ✅ 4. Specification Pattern

**Location:**

- `src/EShop.Domain/Common/ISpecification.cs`
- `src/EShop.Domain/Common/Specification.cs`
- `src/EShop.Infrastructure/Persistence/SpecificationEvaluator.cs`
- `src/EShop.Domain/Orders/Specifications/IncompleteOrdersSpecification.cs`
- `src/EShop.Domain/Orders/Specifications/OrdersByCustomerSpecification.cs`

- Encapsulates query logic in reusable, testable specifications
- Supports filtering, sorting, paging, includes, and tracking control
- Specifications live in Domain layer (business logic)
- Evaluator lives in Infrastructure layer (EF Core implementation)

**Example Specifications:**

```csharp
// Find incomplete orders
public class IncompleteOrdersSpecification : Specification<Order>
{
    public IncompleteOrdersSpecification()
    {
        AddCriteria(o => o.Status != OrderStatus.Completed
                      && o.Status != OrderStatus.Cancelled);
        ApplyOrderBy(o => o.PurchaseDate);
    }
}

// Find orders by customer
public class OrdersByCustomerSpecification : Specification<Order>
{
    public OrdersByCustomerSpecification(CustomerId customerId)
    {
        AddCriteria(o => o.CustomerId == customerId);
        ApplyOrderByDescending(o => o.PurchaseDate);
    }
}
```

**Repository Usage:**

```csharp
var spec = new IncompleteOrdersSpecification();
var orders = await _orderRepo.FindAsync(spec, ct);
```

**Benefits:**

- Reusable query logic
- Testable without database
- Domain-driven queries
- Reduces repository method explosion
- Easy to compose complex queries

---

## Repository Changes

All repositories were updated to use the Unit of Work pattern:

### Before:

```csharp
public async Task AddAsync(Order order, CancellationToken ct = default)
{
    await _context.Orders.AddAsync(order, ct);
    await _context.SaveChangesAsync(ct);  // ❌ SaveChanges in repo
}
```

### After:

```csharp
public void Add(Order order)
{
    _context.Orders.Add(order);  // ✅ Just add to context
}

// In handler:
_orderRepo.Add(order);
await _unitOfWork.SaveChangesAsync(ct);  // ✅ UnitOfWork handles save + events
```

**Updated Interfaces:**

- `IOrderRepository`
- `IProductRepository`
- `ICustomerRepository`
- `IUserAccountRepository`

---

## Command Handler Updates

All command handlers now inject and use `IUnitOfWork`:

### Example:

```csharp
public class CheckoutOrderCommandHandler
{
    private readonly IOrderRepository _orderRepo;
    private readonly IUnitOfWork _unitOfWork;

    public async Task<Result<OrderDto>> HandleAsync(CheckoutOrderCommand cmd, CancellationToken ct)
    {
        // Business logic...
        var order = new Order(...);  // Raises OrderPlaced event

        _orderRepo.Add(order);
        await _unitOfWork.SaveChangesAsync(ct);  // Saves + dispatches events

        return Result.Success((OrderDto)order);
    }
}
```

**Updated Handlers:**

- `CheckoutOrderCommandHandler`
- `CompleteOrderCommandHandler`
- `UpdateCustomerCommandHandler`
- `RegisterCommandHandler`
- `LoginCommandHandler`
- `RefreshTokenCommandHandler`
- `LogoutCommandHandler`

---

## Configuration

Added DI registrations in `Program.cs`:

```csharp
// Unit of Work and Domain Events
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<IDomainEventDispatcher, DomainEventDispatcher>();

// Domain Event Handlers
builder.Services.AddScoped<IDomainEventHandler<OrderPlaced>, OrderPlacedEventHandler>();
builder.Services.AddScoped<IDomainEventHandler<OrderCompleted>, OrderCompletedEventHandler>();
```

---

## Testing the Changes

### 1. Domain Events Work

When an order is placed:

1. `Order` constructor raises `OrderPlaced` event
2. `UnitOfWork.SaveChangesAsync()` saves order to DB
3. `DomainEventDispatcher` invokes `OrderPlacedEventHandler`
4. Handler can send emails, update inventory, log activity

### 2. Specifications Work

```csharp
var spec = new OrdersByCustomerSpecification(customerId);
var orders = await _orderRepo.FindAsync(spec, ct);
// Returns customer's orders sorted by date descending
```

### 3. Unit of Work Works

```csharp
// Multiple operations in one transaction
_customerRepo.Add(customer);
_customerRepo.AddAddress(address);
_userAccountRepo.Add(user);
await _unitOfWork.SaveChangesAsync(ct);  // All or nothing
```

---

## Next Steps (Optional Improvements)

### High Priority:

1. **Add Domain Services** - Extract cross-aggregate business logic
2. **Implement Optimistic Concurrency** - Add RowVersion to aggregates
3. **Add Business Rule Validation** - Create `IBusinessRule` interface

### Medium Priority:

4. **Factory Pattern** - Create factories for complex aggregate creation
5. **Value Object Base Class** - Abstract `ValueObject` with equality helpers
6. **Repository Specification Support** - Add more specification examples

### Low Priority:

7. **Audit Trail** - Add CreatedBy, ModifiedAt, ModifiedBy fields
8. **Soft Delete** - Add IsDeleted, DeletedAt patterns
9. **Bounded Context Separation** - Anti-corruption layers
10. **Event Sourcing** - Store domain events for audit/replay

---

## Architecture Benefits

✅ **Single Responsibility** - Repositories only query/add, UnitOfWork handles persistence
✅ **Open/Closed** - Easy to add new event handlers without changing aggregates
✅ **Dependency Inversion** - Domain doesn't depend on infrastructure
✅ **Testability** - Can test specifications and handlers in isolation
✅ **Maintainability** - Clear separation of concerns
✅ **Extensibility** - Easy to add new business rules via events

---

## File Structure

```
backend/src/
├── EShop.Domain/
│   └── Common/
│       ├── Entity.cs                    ✅ NEW
│       ├── ISpecification.cs           ✅ NEW
│       ├── Specification.cs            ✅ NEW
│       ├── AggregateRoot.cs            (existing)
│       ├── EntityId.cs                 (existing)
│       └── IDomainEvent.cs             (existing)
│   └── Orders/
│       └── Specifications/
│           ├── IncompleteOrdersSpecification.cs    ✅ NEW
│           └── OrdersByCustomerSpecification.cs    ✅ NEW
├── EShop.Application/
│   └── Common/
│       ├── IUnitOfWork.cs              ✅ NEW
│       ├── IDomainEventDispatcher.cs   ✅ NEW
│       └── IDomainEventHandler.cs      ✅ NEW
│   └── Orders/EventHandlers/
│       ├── OrderPlacedEventHandler.cs           ✅ NEW
│       └── OrderCompletedEventHandler.cs        ✅ NEW
└── EShop.Infrastructure/
    ├── Persistence/
    │   ├── UnitOfWork.cs               ✅ NEW
    │   └── SpecificationEvaluator.cs   ✅ NEW
    └── Services/
        └── DomainEventDispatcher.cs    ✅ NEW
```

---

## Conclusion

The implementation now follows proper DDD tactical patterns:

- ✅ **Entity Base Class** - Proper identity and equality
- ✅ **Unit of Work** - Transaction coordination
- ✅ **Domain Events** - Decoupled side effects
- ✅ **Specifications** - Reusable query logic
- ✅ **Aggregate Roots** - Consistency boundaries respected
- ✅ **Value Objects** - Immutable domain concepts
- ✅ **Repository Pattern** - Abstracted persistence

The codebase is now more maintainable, testable, and aligned with DDD best practices.
