using Nuuvify.CommonPack.Extensions.Notificator;

namespace Nuuvify.CommonPack.Domain.ValueObjects;


public class DocumentoPessoa : NotifiableR
{

    protected DocumentoPessoa() { }

    public DocumentoPessoa(Cpf cpf, Cnpj cnpj, TipoPessoa tipoPessoa)
    {
        _ = Validar(cpf, cnpj, tipoPessoa);
    }

    public string Cpf { get; private set; }
    public string Cnpj { get; private set; }
    public DateTime DataDeNascimento { get; private set; }
    public string TipoDaPessoa { get; private set; }
    public string Codigo { get; private set; }

    public override string ToString()
    {
        return Cpf ?? Cnpj;
    }

    public bool Validar(Cpf cpf, Cnpj cnpj, TipoPessoa tipoPessoa)
    {

        if (!cpf.IsValid() && !cnpj.IsValid() && !tipoPessoa.IsValid())
        {
            AddNotifications(cpf.Notifications);
            AddNotifications(cnpj.Notifications);
            AddNotifications(tipoPessoa.Notifications);
            return false;
        }

        if (!tipoPessoa.IsValid())
        {
            AddNotifications(tipoPessoa.Notifications);
            return false;
        }

        if (tipoPessoa.Codigo == "F")
        {
            if (cpf.IsValid())
            {
                Cpf = cpf.Codigo;
                TipoDaPessoa = tipoPessoa.Codigo;
                DataDeNascimento = tipoPessoa.DataDeNascimento;
                Codigo = Cpf;
                return true;
            }
            else
            {
                AddNotifications(cpf.Notifications);
            }
        }

        if (tipoPessoa.Codigo == "J")
        {
            if (cnpj.IsValid())
            {
                Cnpj = cnpj.Codigo;
                TipoDaPessoa = tipoPessoa.Codigo;
                DataDeNascimento = new DateTime();
                Codigo = Cnpj;
                return true;
            }
            else
            {
                AddNotifications(cnpj.Notifications);
            }
        }
        return false;

    }

    public const int maxCodigo = 14;

}
