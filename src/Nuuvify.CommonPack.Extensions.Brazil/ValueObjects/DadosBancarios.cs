using Nuuvify.CommonPack.Domain.ValueObjects;
using Nuuvify.CommonPack.Extensions.Implementation;
using Nuuvify.CommonPack.Mediator.Implementation;

namespace Nuuvify.CommonPack.Extensions.Brazil;

public class DadosBancarios : NotifiableR
{

    protected DadosBancarios() { }

    public DadosBancarios(string bancoNumero, string agenciaNumero, string agenciaNome, string contaCorrente, TipoContaBancaria tipoConta)
    {
        if (!tipoConta.GetHashCode().Equals(TipoContaBancaria.NaoPossuiConta.GetHashCode()))
        {
            DefinirBancoNumero(bancoNumero);
            DefinirAgenciaNumero(agenciaNumero);
            DefinirAgenciaNome(agenciaNome);
            DefinirContaCorrente(contaCorrente);
        }

        DefinirTipoConta(tipoConta);

    }

    public string BancoNumero { get; private set; }
    public string AgenciaNumero { get; private set; }
    public string AgenciaNome { get; private set; }
    public string ContaCorrente { get; private set; }
    public string TipoDaConta { get; private set; }

    private void DefinirBancoNumero(string bancoNumero)
    {
        var validacao = Notifications.Count;

        var _bancoNumero = bancoNumero.GetNumbers();

        if (_bancoNumero.Length < MinBancoNumero)
        {
            AddNotification(nameof(DadosBancarios), $"O número do banco deve ter no mínimo {MinBancoNumero} caracteres.");
        }

        if (_bancoNumero.Length > MaxBancoNumero)
        {
            AddNotification(nameof(DadosBancarios), $"O número do banco deve ter no máximo {MaxBancoNumero} caracteres.");
        }

        if (validacao.Equals(Notifications.Count))
            BancoNumero = _bancoNumero;
    }

    private void DefinirAgenciaNumero(string agenciaNumero)
    {
        var validacao = Notifications.Count;

        if (agenciaNumero.Length < MinAgenciaNumero)
        {
            AddNotification(nameof(DadosBancarios), $"O número da agência deve ter no mínimo {MinAgenciaNumero} caracteres.");
        }

        if (agenciaNumero.Length > MaxAgenciaNumero)
        {
            AddNotification(nameof(DadosBancarios), $"O número da agência deve ter no máximo {MaxAgenciaNumero} caracteres.");
        }

        if (validacao.Equals(Notifications.Count))
            AgenciaNumero = agenciaNumero;
    }

    private void DefinirAgenciaNome(string agenciaNome)
    {
        var validacao = Notifications.Count;

        var _nome = StringExtensionMethods.ToTitleCase(agenciaNome);

        if (_nome.Length < MinAgenciaNome)
        {
            AddNotification(nameof(DadosBancarios), $"O nome da agência deve ter no mínimo {MinAgenciaNome} caracteres.");
        }

        if (_nome.Length > MaxAgenciaNome)
        {
            AddNotification(nameof(DadosBancarios), $"O nome da agência deve ter no máximo {MaxAgenciaNome} caracteres.");
        }

        if (validacao.Equals(Notifications.Count))
            AgenciaNome = _nome;
    }

    private void DefinirContaCorrente(string cc)
    {
        var validacao = Notifications.Count;

        if (cc.Length < MinContaCorrente)
        {
            AddNotification(nameof(DadosBancarios), $"A conta corrente deve ter no mínimo {MinContaCorrente} caracteres.");
        }

        if (cc.Length > MaxContaCorrente)
        {
            AddNotification(nameof(DadosBancarios), $"A conta corrente deve ter no máximo {MaxContaCorrente} caracteres.");
        }

        if (validacao.Equals(Notifications.Count))
            ContaCorrente = cc;
    }

    private void DefinirTipoConta(TipoContaBancaria tipoConta)
    {

        TipoDaConta = tipoConta.ToString();

        if (tipoConta.GetHashCode().Equals(TipoContaBancaria.NaoPossuiConta.GetHashCode()))
        {
            if (string.IsNullOrWhiteSpace(BancoNumero))
            {
                AddNotification(nameof(DadosBancarios), "O número do banco não pode ser vazio ou nulo.");
            }

            if (string.IsNullOrWhiteSpace(AgenciaNumero))
            {
                AddNotification(nameof(DadosBancarios), "O número da agência não pode ser vazio ou nulo.");
            }

            if (string.IsNullOrWhiteSpace(AgenciaNome))
            {
                AddNotification(nameof(DadosBancarios), "O nome da agência não pode ser vazio ou nulo.");
            }

            if (string.IsNullOrWhiteSpace(ContaCorrente))
            {
                AddNotification(nameof(DadosBancarios), "A conta corrente não pode ser vazia ou nula.");
            }
        }

    }

    public const int MinBancoNumero = 0;
    public const int MaxBancoNumero = 3;

    public const int MinAgenciaNumero = 0;
    public const int MaxAgenciaNumero = 8;
    public const int MinAgenciaNome = 0;
    public const int MaxAgenciaNome = 30;
    public const int MinContaCorrente = 0;
    public const int MaxContaCorrente = 20;

}
