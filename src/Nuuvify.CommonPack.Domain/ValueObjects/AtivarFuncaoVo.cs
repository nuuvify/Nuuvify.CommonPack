using System.Collections.Generic;
using System.Globalization;
using Nuuvify.CommonPack.Extensions.Implementation;
using Nuuvify.CommonPack.Extensions.Notificator;

namespace Nuuvify.CommonPack.Domain.ValueObjects
{

    public class AtivarFuncaoVo : NotifiableR
    {
        
        protected AtivarFuncaoVo() { }

        public AtivarFuncaoVo(string situacao)
        {
            if (!ValidarDescricao(situacao) && !ValidarLiteral(situacao))
            {
                Codigo = null;

                AddNotification(this.ToString(), MsgValueObjects.ResourceManager.GetString("ValueObjectInvalid", CultureInfo.CurrentCulture).Replace("{valueObject}", this.ToString()));
            }
        }

        public string Codigo { get; private set; }

        private bool ValidarLiteral(string literal)
        {
            var intCodigo = literal.ToEnumNumero<AtivarFuncao>();
            var ehValido = !intCodigo.Equals(int.MaxValue);

            if (ehValido)
            {
                Codigo = literal.ToEnumDescricao<AtivarFuncao>();
            }

            return ehValido;
        }

        private bool ValidarDescricao(string descricao)
        {

            descricao = descricao?.ToUpper();

            var literal = descricao.GetCodeEnumByDescription<AtivarFuncao>();
            var intCodigo = literal.ToEnumNumero<AtivarFuncao>();
            var ehValido = !intCodigo.Equals(int.MaxValue);

            if (ehValido)
            {
                Codigo = descricao;
            }

            return ehValido;
        }

        public const int maxCodigo = 1;

        public override string ToString()
        {
            return Codigo?.ToString();
        }

        public override int GetHashCode()
        {
            var textoEnum = Codigo.GetCodeEnumByDescription<AtivarFuncao>();
            return textoEnum.ToEnumNumero<AtivarFuncao>();
        }

        public override bool Equals(object obj)
        {
            return obj is AtivarFuncaoVo vo &&
                   EqualityComparer<IReadOnlyCollection<NotificationR>>.Default.Equals(Notifications, vo.Notifications) &&
                   Codigo == vo.Codigo;
        }
    }

}
