
namespace Nuuvify.CommonPack.StandardHttpClient.xTest.Configs;

internal sealed class ProcessamentoResult
{
    public string Id { get; set; } = string.Empty;
    public DateTimeOffset DataCadastro { get; set; }
    public string UsuarioCadastro { get; set; } = string.Empty;
    public string Tipo { get; set; } = string.Empty;
    public string Ambiente { get; set; } = string.Empty;
    public string IdExecucao { get; set; } = string.Empty;
    public string UsuarioSolicitante { get; set; } = string.Empty;
    public string EmpresaSap { get; set; } = string.Empty;
    public string TipoDocumento { get; set; } = string.Empty;
    public string? TipoMovimento { get; set; }
    public int DiasParaProcessar { get; set; }
    public DateTime DataInicioPagamento { get; set; }
    public DateTimeOffset ProximaExecucao { get; set; }
    public string TipoIncremento { get; set; } = string.Empty;
}
