namespace Hpt.Contracts;

public static class Topics
{
    public const string BasketCommands = "basket.commands";
    public const string OrdersSubmitted = "orders.submitted";
    public const string InventoryResults = "orders.inventory-results";
}

public sealed record BasketCommand(
    Guid EventId,
    Guid BasketId,
    string Type,
    DateTimeOffset OccurredAt,
    Guid? ProductId = null,
    int? Quantity = null,
    Guid? OrderId = null);

public sealed record BasketLine(Guid ProductId, int Quantity);

public sealed record OrderSubmitted(
    Guid EventId,
    Guid OrderId,
    Guid BasketId,
    DateTimeOffset CreatedAt,
    IReadOnlyList<BasketLine> Items,
    string? RejectionReason = null);

public sealed record InventoryResult(
    Guid EventId,
    Guid OrderId,
    string Status,
    string? Reason,
    DateTimeOffset ProcessedAt);

public sealed record SetQuantityRequest(int Quantity);

public sealed record BatchSetQuantityRequest(IReadOnlyList<BatchSetQuantityItem> Items);

public sealed record BatchSetQuantityItem(Guid BasketId, Guid ProductId, int Quantity, Guid EventId = default);

public sealed record CheckoutRequest(Guid IdempotencyKey);
