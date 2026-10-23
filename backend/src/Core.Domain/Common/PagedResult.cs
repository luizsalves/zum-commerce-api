namespace Core.Domain.Common;

/// <summary>Página de resultados com cursor (paginação da Shopify).</summary>
public sealed record PagedResult<T>(IReadOnlyList<T> Items, bool HasNextPage, string? EndCursor);
