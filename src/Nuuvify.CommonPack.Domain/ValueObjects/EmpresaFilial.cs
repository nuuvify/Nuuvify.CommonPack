using Nuuvify.CommonPack.Extensions.Notificator;

namespace Nuuvify.CommonPack.Domain.ValueObjects
{
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


            new ValidationConcernR<EmpresaFilial>(this)
                .AssertHasMinLength(x => empresa, minEmpresa) 
                .AssertHasMaxLength(x => empresa, maxEmpresa);


            if (validacao.Equals(Notifications.Count))
                CodigoEmpresa = empresa;
        }

        private void DefinirFilial(string filial)
        {
            var validacao = Notifications.Count;


            new ValidationConcernR<EmpresaFilial>(this)
                .AssertHasMinLength(x => filial, minFilial)
                .AssertHasMaxLength(x => filial, maxFilial);


            if (validacao.Equals(Notifications.Count))
                CodigoFilial = filial;
        }


        public const int minEmpresa = 0;
        public const int maxEmpresa = 5;

        public const int minFilial = 0;
        public const int maxFilial= 4;

    }
}
