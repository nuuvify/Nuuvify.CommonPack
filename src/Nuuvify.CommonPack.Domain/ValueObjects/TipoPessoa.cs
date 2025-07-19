using System;
using System.Globalization;
using Nuuvify.CommonPack.Extensions.Implementation;
using Nuuvify.CommonPack.Extensions.Notificator;

namespace Nuuvify.CommonPack.Domain.ValueObjects
{
    /// <summary>
    /// Toda pessoa fisica precisa ter uma data de nascimento valida
    /// </summary>
    public class TipoPessoa : NotifiableR
    {

        
        protected TipoPessoa() { }

        public TipoPessoa(string pessoa, DateTime? nascimento)
        {
            if (!pessoa.IsEnum<EnumFisicaJuridica>(out int enumCodigo))
            {
                Codigo = null;

                AddNotification(nameof(TipoPessoa), MsgValueObjects.ResourceManager.GetString("ValueObjectInvalid",
                    CultureInfo.CurrentCulture).Replace("{valueObject}", ToString()));
                return;
            }
            else
            {
                Codigo = enumCodigo.ToEnumDescricao<EnumFisicaJuridica>();
            }

            if (enumCodigo == EnumFisicaJuridica.Fisica.GetHashCode())
            {
                var ehValido = ValidarNascimento(nascimento);
                if (!ehValido)
                {
                    Codigo = null;
                }
            }
        }


        public string Codigo { get; private set; }
        public DateTime DataDeNascimento { get; private set; }


        private bool ValidarNascimento(DateTime? nascimento)
        {

            new ValidationConcernR<TipoPessoa>(this)
                .AssertNotDateTimeNull(x => nascimento.Value);

            if (IsValid())
            {
                DataDeNascimento = new DateTime(nascimento.Value.Year, nascimento.Value.Month, nascimento.Value.Day);
                return true;
            }

            return false;
        }

        public const int maxCodigo = 1;


        public override string ToString()
        {
            return Codigo?.ToString();
        }

        public override int GetHashCode()
        {
            var textoEnum = Codigo.GetCodeEnumByDescription<EnumFisicaJuridica>();
            return textoEnum.ToEnumNumero<EnumFisicaJuridica>();
        }

        public override bool Equals(object obj)
        {
            return ReferenceEquals(this, obj);
        }
    }

}
