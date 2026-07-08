namespace Nuuvify.CommonPack.MftMailbox.Abstraction.Models;

/// <summary>
/// Estado do ciclo de vida de uma transferência MFT individual.
/// </summary>
public enum TransferState
{
    /// <summary>A transferência foi criada, mas ainda não foi iniciada.</summary>
    Pending = 1,

    /// <summary>A transferência está em andamento (upload ou download em execução).</summary>
    InProgress = 2,

    /// <summary>A transferência foi concluída com êxito e o arquivo chegou ao destino.</summary>
    Succeeded = 3,

    /// <summary>A transferência falhou após esgotar as tentativas de retry configuradas.</summary>
    Failed = 4,

    /// <summary>A transferência foi ignorada porque o item já foi processado anteriormente (idempotência).</summary>
    Skipped = 5
}
