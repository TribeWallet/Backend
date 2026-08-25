namespace TribeWallet.Entities;

/// <summary>Como o valor total de um compromisso é rateado entre os integrantes.</summary>
public enum TipoDivisao
{
    Igual = 1,
    Porcentagem = 2,
    ValorExato = 3,
    Proporcional = 4
}

/// <summary>Meio usado para quitar a dívida.</summary>
public enum MetodoPagamento
{
    Pix = 1,
    Dinheiro = 2,
    CartaoCredito = 3,
    CartaoDebito = 4,
    TransferenciaBancaria = 5,
    Boleto = 6
}

/// <summary>Recorte de dados que o relatório apresenta.</summary>
public enum TipoRelatorio
{
    DetalhesCompromisso = 1,
    ResumoGrupo = 2,
    ExtratoIndividual = 3,
    Pendencias = 4
}

/// <summary>Evento que originou a notificação.</summary>
public enum TipoNotificacao
{
    ConviteGrupo = 1,
    NovoCompromisso = 2,
    PagamentoRegistrado = 3,
    CobrancaPendente = 4,
    CompromissoQuitado = 5,
    AlteracaoCompromisso = 6
}

/// <summary>Operação registrada na trilha de auditoria.</summary>
public enum TipoAlteracao
{
    Criacao = 1,
    Atualizacao = 2,
    Exclusao = 3
}
