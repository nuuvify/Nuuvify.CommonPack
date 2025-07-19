using Nuuvify.CommonPack.Extensions.Interfaces;

namespace Nuuvify.CommonPack.UnitOfWork.PostgreSQL.xTest
{
    public class Endereco : INotPersistingAsTable
    {

        protected Endereco() { }

        public Endereco(string logradouro, string cidade)
        {
            Logradouro = logradouro.Length > MaxLogradouro
                ? logradouro.Substring(0, MaxLogradouro)
                : logradouro;

            Cidade = cidade.Length > MaxCidade
                ? cidade.Substring(0, MaxCidade)
                : cidade;
        }

        public string Logradouro { get; private set; }
        public string Cidade { get; private set; }

        public static int MaxLogradouro { get; set; } = 60;
        public static int MaxCidade { get; set; } = 50;
    }
}
