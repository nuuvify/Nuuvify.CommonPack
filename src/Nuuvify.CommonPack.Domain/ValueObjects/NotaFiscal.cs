
using System;
using Nuuvify.CommonPack.Extensions.Implementation;
using Nuuvify.CommonPack.Extensions.Notificator;

namespace Nuuvify.CommonPack.Domain.ValueObjects
{
    public class NotaFiscal : NotifiableR
    {

        protected NotaFiscal() { }

        public NotaFiscal(string numero, string serie, DateTime emissao, EnumSimNao eletronica)
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

            new ValidationConcernR<NotaFiscal>(this)
                .AssertHasMinLength(x => numeroNota, minNumero)
                .AssertHasMaxLength(x => numeroNota, maxNumero);


            if (validacao.Equals(Notifications.Count))
                Numero = numeroNota;
        }

        private void DefinirSerie(string serieNota)
        {
            var validacao = Notifications.Count;


            new ValidationConcernR<NotaFiscal>(this)
                .AssertHasMinLength(x => serieNota, minSerie)
                .AssertHasMaxLength(x => serieNota, maxSerie);

            if (validacao.Equals(Notifications.Count))
                Serie = serieNota;
        }

        private void DefinirEmissao(DateTime emissao)
        {

            new ValidationConcernR<NotaFiscal>(this)
                .AssertNotDateTimeNull(x => emissao);

            if (IsValid())
            {
                Emissao = new DateTime(emissao.Year, emissao.Month, emissao.Day);
            }
            else
            {
                Emissao = new DateTime();
            }

        }

        public const int minNumero = 1;
        public const int maxNumero = 12;

        public const int minSerie = 1;
        public const int maxSerie = 5;


    }
}
