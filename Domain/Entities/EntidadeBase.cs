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

    /// <summary>Momento do insert. Preenchido pelo <c>AppDbContext</c>; nunca muda depois.</summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>Momento do último update. Preenchido pelo <c>AppDbContext</c> a cada alteração.</summary>
    public DateTime UpdatedAt { get; set; }

    /// <summary>
    /// Soft delete: nulo enquanto o registro está ativo, com o horário da exclusão depois dela.
    /// Quem consome a API é que traduz isso em ativo/inativo — o backend não filtra nada por conta
    /// própria, então toda consulta continua enxergando os registros excluídos.
    /// </summary>
    public DateTime? DeletedAt { get; set; }
}
