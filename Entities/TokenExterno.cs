using System.Security.Cryptography;

namespace TribeWallet.Entities;

/// <summary>Gera os tokens públicos das entidades.</summary>
public static class TokenExterno
{
    /// <summary>Tamanho do token em caracteres: SHA-256 em hexadecimal.</summary>
    public const int Tamanho = 64;

    /// <summary>
    /// Sorteia 32 bytes criptograficamente seguros e devolve o SHA-256 deles em hex minúsculo.
    /// A aleatoriedade vem do RandomNumberGenerator — o hash só dá ao token o formato fixo
    /// de 64 caracteres, sem revelar nada sobre o registro.
    /// </summary>
    public static string Gerar()
    {
        Span<byte> aleatorio = stackalloc byte[32];
        RandomNumberGenerator.Fill(aleatorio);

        Span<byte> hash = stackalloc byte[32];
        SHA256.HashData(aleatorio, hash);

        return Convert.ToHexString(hash).ToLowerInvariant();
    }
}
