using Nuuvify.CommonPack.Mediator.Implementation;

namespace Nuuvify.CommonPack.Extensions.Brazil;

public class EmpresaFilial : NotifiableR
{

    protected EmpresaFilial() { }

    public EmpresaFilial(string empresa, string filial)
    {
        DefinirEmpresa(empresa);
        DefinirFilial(filial);

    }

    public string CodigoEmpresa { get; private set; }
    public string CodigoFilial { get; private set; }

    private void DefinirEmpresa(string empresa)
    {
        var validacao = Notifications.Count;

        if (empresa.Length < MinEmpresa || empresa.Length > MaxEmpresa)
        {
            AddNotification(nameof(CodigoEmpresa), $"Empresa must be between {MinEmpresa} and {MaxEmpresa} characters.");
        }

        if (validacao.Equals(Notifications.Count))
            CodigoEmpresa = empresa;
    }

    private void DefinirFilial(string filial)
    {
        var validacao = Notifications.Count;

        if (filial.Length < MinFilial || filial.Length > MaxFilial)
        {
            AddNotification(nameof(CodigoFilial), $"Filial must be between {MinFilial} and {MaxFilial} characters.");
        }

        if (validacao.Equals(Notifications.Count))
            CodigoFilial = filial;
    }

    public const int MinEmpresa = 0;
    public const int MaxEmpresa = 5;

    public const int MinFilial = 0;
    public const int MaxFilial = 4;

}
