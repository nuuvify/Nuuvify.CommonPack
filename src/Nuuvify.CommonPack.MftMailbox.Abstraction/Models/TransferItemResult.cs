namespace Nuuvify.CommonPack.MftMailbox.Abstraction.Models;

/// <summary>
/// Resultado do processamento de um único arquivo em uma operação MFT.
/// </summary>
/// <remarks>
/// Retornado por <c>IMftTransferClient.SendSingleAsync</c> e como elemento de
/// <see cref="TransferBatchResult.Items"/>. Quando <see cref="State"/> é <see cref="TransferState.Failed"/>,
/// verifique <see cref="IsTransientFailure"/> para decidir se o item pode ser reenviado.
/// </remarks>
public sealed class TransferItemResult
{
    /// <summary>Identificador do item processado, correspondente a <see cref="TransferItem.ItemId"/>.</summary>
    public string ItemId { get; set; } = string.Empty;

    /// <summary>Nome do arquivo processado.</summary>
    public string FileName { get; set; } = string.Empty;

    /// <summary>Estado final da operação para este item.</summary>
    public TransferState State { get; set; }

    /// <summary>
    /// Código de status retornado pelo servidor (ex.: código HTTP ou código de erro SFTP).
    /// <see langword="null"/> quando não aplicável.
    /// </summary>
    public string? StatusCode { get; set; }

    /// <summary>Mensagem descritiva do resultado ou detalhes do erro.</summary>
    public string? Message { get; set; }

    /// <summary>
    /// Hash SHA-256 do arquivo em hexadecimal minúsculo, calculado após o envio.
    /// <see langword="null"/> quando não aplicável ou quando a operação falhou antes do envio.
    /// </summary>
    public string? ChecksumSha256 { get; set; }

    /// <summary>Tamanho efetivo enviado em bytes. <see langword="null"/> quando não mensurável.</summary>
    public long? SizeBytes { get; set; }

    /// <summary>
    /// Indica se a falha é transitória (rede, timeout) e o item pode ser reenviado com segurança.
    /// <see langword="false"/> para falhas permanentes (arquivo inválido, permissão negada).
    /// </summary>
    public bool IsTransientFailure { get; set; }
}
