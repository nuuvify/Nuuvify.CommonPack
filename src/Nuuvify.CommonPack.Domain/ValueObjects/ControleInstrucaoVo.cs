using Nuuvify.CommonPack.Extensions.Implementation;
using Nuuvify.CommonPack.Extensions.Notificator;
using System.Collections.Generic;
using System.Globalization;

namespace Nuuvify.CommonPack.Domain.ValueObjects
{

    public class ControleInstrucao : NotifiableR
    {

        
        protected ControleInstrucao() { }

        public ControleInstrucao(string situacao)
        {
            Validar(situacao);
        }


        public string Codigo { get; private set; }


        private void Validar(string situacao)
        {
            situacao = situacao?.ToUpper();

            var codigo = situacao.GetCodeEnumByDescription<EnumControleInstrucao>();
            var _numero = codigo.ToEnumNumero<EnumControleInstrucao>();
            if (_numero.Equals(int.MaxValue))
            {
                Codigo = null;
                AddNotification(this.ToString(), MsgValueObjects.ResourceManager.GetString("ValueObjectInvalid", CultureInfo.CurrentCulture).Replace("{valueObject}", this.ToString()));
            }

            Codigo = situacao;
        }


        public const int maxCodigo = 1;

        public override string ToString()
        {
            return Codigo?.ToString();
        }

        public override int GetHashCode()
        {
            var textoEnum = Codigo.GetCodeEnumByDescription<EnumControleInstrucao>();
            return textoEnum.ToEnumNumero<EnumControleInstrucao>();
        }

        public override bool Equals(object obj)
        {
            return obj is ControleInstrucao instrucao &&
                   EqualityComparer<IReadOnlyCollection<NotificationR>>.Default.Equals(Notifications, instrucao.Notifications) &&
                   Codigo == instrucao.Codigo;
        }
    }
}
