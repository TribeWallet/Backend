namespace TribeWallet.Domain.Entities;

/// <summary>
/// Base de todas as entidades persistidas. O identificador primário (UUID) circula só dentro
/// da aplicação e do banco; para fora — URLs, payloads da API, links de comprovante — vai o
/// <see cref="Token"/>, que é opaco e não revela nada sobre o registro.
/// </summary>
public abstract class EntidadeBase
{
    /// <summary>
    /// Token público de 64 caracteres hex. Preenchido automaticamente pelo
    /// <c>AppDbContext</c> no momento do insert, se ainda estiver vazio.
    /// </summary>
    public string Token { get; set; } = string.Empty;
}
