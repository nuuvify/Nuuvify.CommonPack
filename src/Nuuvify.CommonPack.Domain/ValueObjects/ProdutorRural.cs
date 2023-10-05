using Nuuvify.CommonPack.Extensions.Implementation;
using Nuuvify.CommonPack.Extensions.Notificator;

namespace Nuuvify.CommonPack.Domain.ValueObjects
{
    public class ProdutorRural : NotifiableR
    {

        public ProdutorRural(EnumSimNao simNao, string cei)
        {

            DefinirIndicador(simNao);
            DefinirCeiDoProdutorRural(cei, simNao);

        }



        public string IndicadorProdutorRural { get; private set; }
        public string CeiDoProdutorRural { get; private set; }



        private void DefinirIndicador(EnumSimNao simNao)
        {

            if (simNao == EnumSimNao.Sim)
                IndicadorProdutorRural = EnumSimNao.Sim.GetDescription();
            else
                IndicadorProdutorRural = EnumSimNao.Nao.GetDescription();
        }

        private void DefinirCeiDoProdutorRural(string cei, EnumSimNao simNao)
        {

            if (simNao == EnumSimNao.Sim)
            {
                var validacao = Notifications.Count;

                new ValidationConcernR<ProdutorRural>(this)
                    .AssertHasMinLength(x => cei, minCei)
                    .AssertHasMaxLength(x => cei, maxCei);


                if (validacao.Equals(Notifications.Count))
                    CeiDoProdutorRural = cei;

            }

        }




        public const int minCei = 0;
        public const int maxCei = 12;

    }
}
