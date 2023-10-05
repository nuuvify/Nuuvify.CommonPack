using System.Collections.Generic;
using System.Globalization;
using Nuuvify.CommonPack.Extensions.Implementation;
using Nuuvify.CommonPack.Extensions.Notificator;

namespace Nuuvify.CommonPack.Domain.ValueObjects
{

    public class AtivoInativo : NotifiableR
    {

        
        protected AtivoInativo() { }

        /// <summary>
        /// Valida a situação conforme o <see cref="EnumAtivoInativo"/>
        /// </summary>
        /// <param name="situacao">Ativo, Invativo, Ambos ou A, I, N</param>
        public AtivoInativo(string situacao)
        {
            if (!ValidarDescricao(situacao) && !ValidarLiteral(situacao))
            {
                Codigo = null;

                AddNotification(this.ToString(), 
                    MsgValueObjects.ResourceManager.GetString("ValueObjectInvalid", CultureInfo.CurrentCulture)
                        .Replace("{valueObject}", this.ToString()));
            }
        }


        /// <summary>
        /// Retorna os valores literais ou description do enum
        /// <see cref="EnumAtivoInativo"/>
        /// </summary>
        public string Codigo { get; private set; }


        private bool ValidarLiteral(string literal)
        {
            var intCodigo = literal.ToEnumNumero<EnumAtivoInativo>();
            var ehValido = !intCodigo.Equals(int.MaxValue);

            if (ehValido)
            {
                Codigo = literal.ToEnumDescricao<EnumAtivoInativo>();
            }

            return ehValido;
        }

        private bool ValidarDescricao(string descricao)
        {

            descricao = descricao?.ToUpper();

            var literal = descricao.GetCodeEnumByDescription<EnumAtivoInativo>();
            var intCodigo = literal.ToEnumNumero<EnumAtivoInativo>();
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
            var textoEnum = Codigo.GetCodeEnumByDescription<EnumAtivoInativo>();
            return textoEnum.ToEnumNumero<EnumAtivoInativo>();
        }

        public override bool Equals(object obj)
        {
            return obj is AtivoInativo inativo &&
                   EqualityComparer<IReadOnlyCollection<NotificationR>>.Default.Equals(Notifications, inativo.Notifications) &&
                   Codigo == inativo.Codigo;
        }
    }

}
