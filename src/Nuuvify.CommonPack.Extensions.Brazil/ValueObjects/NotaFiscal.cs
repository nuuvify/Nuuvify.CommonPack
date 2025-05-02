using Nuuvify.CommonPack.Domain.ValueObjects;
using Nuuvify.CommonPack.Extensions.Implementation;
using Nuuvify.CommonPack.Mediator.Implementation;

namespace Nuuvify.CommonPack.Extensions.Brazil;

public class NotaFiscal : NotifiableR
{

    protected NotaFiscal() { }

    public NotaFiscal(
        string numero,
        string serie,
        DateTime emissao,
        EnumSimNao eletronica)
    {
        DefinirNumero(numero);
        DefinirSerie(serie);
        DefinirNotaEletronica(eletronica);
        DefinirEmissao(emissao);

        if (!IsValid())
        {
            Numero = null;
            Serie = null;
            Emissao = new DateTime();
            NotaEletronica = null;
        }
    }

    public string Numero { get; private set; }
    public string Serie { get; private set; }
    public DateTime Emissao { get; private set; }
    public string NotaEletronica { get; private set; }

    private void DefinirNotaEletronica(EnumSimNao eletronica)
    {
        NotaEletronica = eletronica.GetDescription();

    }
    private void DefinirNumero(string numero)
    {
        var validacao = Notifications.Count;

        var numeroNota = numero.GetNumbers();

        if (numeroNota.Length < MinNumero || numeroNota.Length > MaxNumero)
        {
            AddNotification(nameof(Numero), $"O número deve ter entre {MinNumero} e {MaxNumero} caracteres.");
        }

        if (validacao.Equals(Notifications.Count))
            Numero = numeroNota;
    }

    private void DefinirSerie(string serieNota)
    {
        var validacao = Notifications.Count;

        if (serieNota.Length < MinSerie || serieNota.Length > MaxSerie)
        {
            AddNotification(nameof(Serie), $"A série deve ter entre {MinSerie} e {MaxSerie} caracteres.");
        }

        if (validacao.Equals(Notifications.Count))
            Serie = serieNota;
    }

    private void DefinirEmissao(DateTime emissao)
    {

        if (emissao == default)
        {
            AddNotification(nameof(Emissao), "A data de emissão não pode ser nula ou inválida.");
        }

        if (IsValid())
        {
            Emissao = new DateTime(emissao.Year, emissao.Month, emissao.Day);
        }
        else
        {
            Emissao = new DateTime();
        }

    }

    public const int MinNumero = 1;
    public const int MaxNumero = 12;

    public const int MinSerie = 1;
    public const int MaxSerie = 5;

}
