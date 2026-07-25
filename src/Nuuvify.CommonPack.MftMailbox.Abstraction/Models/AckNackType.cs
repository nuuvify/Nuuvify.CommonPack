namespace Nuuvify.CommonPack.MftMailbox.Abstraction.Models;

/// <summary>
/// Indica o tipo de confirmação a ser enviado ao servidor MFT após o processamento de um arquivo recebido.
/// </summary>
public enum AckNackType
{
    /// <summary>Não especifica decisão de confirmação.</summary>
    None = 0,

    /// <summary>Confirmação positiva: o arquivo foi processado com sucesso e pode ser arquivado ou removido.</summary>
    Ack = 1,

    /// <summary>Rejeição: o arquivo não pôde ser processado e deve ser movido para a pasta de erro.</summary>
    Nack = 2
}
