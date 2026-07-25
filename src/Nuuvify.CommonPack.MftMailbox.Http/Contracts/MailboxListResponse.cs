namespace Nuuvify.CommonPack.MftMailbox.Http.Contracts;

internal sealed class MailboxListResponse
{
    public List<MailboxListItem> Items { get; set; } = new();
}

internal sealed class MailboxListItem
{
    public string ItemId { get; set; } = string.Empty;

    public string FileName { get; set; } = string.Empty;

    public string RemotePath { get; set; } = string.Empty;

    public long? SizeBytes { get; set; }

    public string? ChecksumSha256 { get; set; }
}
