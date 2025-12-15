namespace EShop.Domain.Common;

/// <summary>
/// base class for strongly typed ids
/// </summary>
public abstract record EntityId<T>(T Value) where T : notnull
{
    public override string ToString() => Value.ToString() ?? string.Empty;
}
