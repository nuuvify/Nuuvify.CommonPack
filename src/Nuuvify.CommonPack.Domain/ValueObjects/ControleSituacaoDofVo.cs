using Nuuvify.CommonPack.Extensions.Implementation;
using Nuuvify.CommonPack.Extensions.Notificator;
using System.Collections.Generic;
using System.Globalization;

namespace Nuuvify.CommonPack.Domain.ValueObjects
{

    public class ControleSituacaoDof : NotifiableR
    {
        
        protected ControleSituacaoDof() { }

        public ControleSituacaoDof(string situacao)
        {
            Validar(situacao);
        }


        public string Codigo { get; private set; }


        private void Validar(string situacao)
        {
            situacao = situacao?.ToUpper();

            var codigo = situacao.GetCodeEnumByDescription<EnumControleSituacaoDof>();
            var _numero = codigo.ToEnumNumero<EnumControleSituacaoDof>();
            if (_numero.Equals(int.MaxValue))
            {
                Codigo = null;
                AddNotification(ToString(), MsgValueObjects.ResourceManager.GetString("ValueObjectInvalid", CultureInfo.CurrentCulture).Replace("{valueObject}", ToString()));
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
            var textoEnum = Codigo.GetCodeEnumByDescription<EnumControleSituacaoDof>();
            return textoEnum.ToEnumNumero<EnumControleSituacaoDof>();
        }

        public override bool Equals(object obj)
        {
            return obj is ControleSituacaoDof dof &&
                   EqualityComparer<IReadOnlyCollection<NotificationR>>.Default.Equals(Notifications, dof.Notifications) &&
                   Codigo == dof.Codigo;
        }
    }
}
