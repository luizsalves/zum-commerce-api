using System.Text;
using Core.Application.Exceptions;

namespace Core.Application.Common;

/// <summary>Regras das fotos de produto enviadas pelo painel.</summary>
public static class ImageFiles
{
    /// <summary>Limite da Shopify para imagens.</summary>
    public const long MaxBytes = 20 * 1024 * 1024;

    private static readonly Dictionary<string, string> Extensions = new()
    {
        ["image/jpeg"] = ".jpg",
        ["image/png"] = ".png",
        ["image/webp"] = ".webp",
        ["image/gif"] = ".gif",
        ["image/heic"] = ".heic",
        ["image/heif"] = ".heif",
    };

    /// <summary>Confere tamanho e formato e devolve o tipo normalizado (ex.: "image/jpeg").</summary>
    public static string Validate(string? contentType, long length)
    {
        if (length <= 0) throw AppException.Validation("Escolha uma foto.");
        if (length > MaxBytes) throw AppException.Validation("A foto passa de 20 MB. Escolha uma imagem menor.");

        var type = (contentType ?? "").Trim().ToLowerInvariant();
        if (!Extensions.ContainsKey(type))
            throw AppException.Validation("Formato não aceito. Envie fotos JPG, PNG, WEBP, GIF ou HEIC.");
        return type;
    }

    // Tabela fixa porque a API roda com InvariantGlobalization (string.Normalize não separa os acentos).
    private const string Accented = "áàâãäéèêëíìîïóòôõöúùûüçñÁÀÂÃÄÉÈÊËÍÌÎÏÓÒÔÕÖÚÙÛÜÇÑ";
    private const string Plain = "aaaaaeeeeiiiiooooouuuucnaaaaaeeeeiiiiooooouuuucn";

    /// <summary>"Tiara Rosa (1).JPG" + image/jpeg → "tiara-rosa-1.jpg". Sem nome aproveitável, usa "foto".</summary>
    public static string SafeName(string? fileName, string contentType)
    {
        var slug = new StringBuilder();
        foreach (var raw in Path.GetFileNameWithoutExtension(fileName ?? ""))
        {
            var index = Accented.IndexOf(raw);
            var c = char.ToLowerInvariant(index >= 0 ? Plain[index] : raw);
            if (c is (>= 'a' and <= 'z') or (>= '0' and <= '9')) slug.Append(c);
            else if (slug.Length > 0 && slug[^1] != '-') slug.Append('-');
        }

        var clean = slug.ToString().Trim('-');
        if (clean.Length > 60) clean = clean[..60].TrimEnd('-');
        return (clean.Length == 0 ? "foto" : clean) + Extensions[contentType];
    }
}
