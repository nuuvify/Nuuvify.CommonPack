using Nuuvify.CommonPack.Extensions.Implementation;
using Nuuvify.CommonPack.Extensions.Notificator;

namespace Nuuvify.CommonPack.Domain.ValueObjects
{
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

            new ValidationConcernR<DadosBancarios>(this)
                .AssertHasMinLength(x => _bancoNumero, minBancoNumero)
                .AssertHasMaxLength(x => _bancoNumero, maxBancoNumero);



            if (validacao.Equals(Notifications.Count))
                BancoNumero = _bancoNumero;
        }

        private void DefinirAgenciaNumero(string agenciaNumero)
        {
            var validacao = Notifications.Count;


            new ValidationConcernR<DadosBancarios>(this)
                .AssertHasMinLength(x => agenciaNumero, minAgenciaNumero)
                .AssertHasMaxLength(x => agenciaNumero, maxAgenciaNumero);


            if (validacao.Equals(Notifications.Count))
                AgenciaNumero = agenciaNumero;
        }

        private void DefinirAgenciaNome(string agenciaNome)
        {
            var validacao = Notifications.Count;

            var _nome = StringExtensionMethods.ToTitleCase(agenciaNome);

            new ValidationConcernR<DadosBancarios>(this)
                .AssertHasMinLength(x => _nome, minAgenciaNome)
                .AssertHasMaxLength(x => _nome, maxAgenciaNome);


            if (validacao.Equals(Notifications.Count))
                AgenciaNome = _nome;
        }

        private void DefinirContaCorrente(string cc)
        {
            var validacao = Notifications.Count;


            new ValidationConcernR<DadosBancarios>(this)
                .AssertHasMinLength(x => cc, minContaCorrente)
                .AssertHasMaxLength(x => cc, maxContaCorrente);


            if (validacao.Equals(Notifications.Count))
                ContaCorrente = cc;
        }


        private void DefinirTipoConta(TipoContaBancaria tipoConta)
        {

            TipoDaConta = tipoConta.ToString();

            if (tipoConta.GetHashCode().Equals(TipoContaBancaria.NaoPossuiConta.GetHashCode()))
            {
                new ValidationConcernR<DadosBancarios>(this)
                    .AssertIsNullOrWhiteSpace(x => x.BancoNumero)
                    .AssertIsNullOrWhiteSpace(x => x.AgenciaNumero)
                    .AssertIsNullOrWhiteSpace(x => x.AgenciaNome)
                    .AssertIsNullOrWhiteSpace(x => x.ContaCorrente);
            }

        }




        public const int minBancoNumero = 0;
        public const int maxBancoNumero = 3;

        public const int minAgenciaNumero = 0;
        public const int maxAgenciaNumero = 8;

        public const int minAgenciaNome = 0;
        public const int maxAgenciaNome = 30;

        public const int minContaCorrente = 0;
        public const int maxContaCorrente = 20;

    }
}
