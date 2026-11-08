namespace Core.Application.Contracts.Products;

public sealed record SetProductStatusRequest(string Status);

/// <summary>Foto já guardada na Shopify. A Url vai em images[].url ao salvar o produto.</summary>
public sealed record UploadedImageResponse(string Url);
