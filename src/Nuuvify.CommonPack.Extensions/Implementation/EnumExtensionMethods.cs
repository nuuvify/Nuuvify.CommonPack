using System.Reflection;

namespace Nuuvify.CommonPack.Extensions.Implementation;


public static class EnumExtensionMethods
{

    /// <summary>
    /// Obtem a descrição (Attributo) de um objeto tipo Enum
    /// </summary>
    public static string GetDescription(this Enum GenericEnum)
    {
        Type genericEnumType = GenericEnum.GetType();
        MemberInfo[] memberInfo = genericEnumType.GetMember(GenericEnum.ToString());
        if (memberInfo?.Length > 0)
        {
            var _Attribs = memberInfo[0].GetCustomAttributes(typeof(System.ComponentModel.DescriptionAttribute), false);
            if (_Attribs?.Count() > 0)
            {
                return ((System.ComponentModel.DescriptionAttribute)_Attribs.ElementAt(0)).Description;
            }
        }
        return GenericEnum.ToString();
    }

    /// <summary>
    /// Obtem o numero do Enum atraves do texto passado
    /// </summary>
    /// <typeparam name="T">Tipo generico que representa o Enum</typeparam>
    /// <param name="value">Nome ou o texto do Enum</param>
    /// <returns>Retorna o numero do Enum</returns>
    public static int ToEnumNumero<T>(this string value)
    {
        if (value == null)
            return int.MaxValue;

        if (Enum.IsDefined(typeof(T), value))
        {
            var numeroEnum = $@"{Enum.Parse(typeof(T), value):D}";
            if (int.TryParse(numeroEnum, out int numero))
                return numero;
        }

        return int.MaxValue;
    }

    /// <summary>
    /// Obtem o texto do Enum atraves do numero passado
    /// </summary>
    /// <typeparam name="T">Tipo generico que representa o Enum</typeparam>
    /// <param name="value">Numero do Enum</param>
    /// <returns>Retorna o texto do Enum</returns>
    public static string ToEnumCodigo<T>(this string value)
    {
        if (int.TryParse(value, out int intCode))
        {
            return intCode.ToEnumCodigo<T>();
        }

        return null;
    }

    /// <summary>
    /// Obtem o texto do Enum atraves do numero passado
    /// </summary>
    /// <typeparam name="T">Tipo generico que representa o Enum</typeparam>
    /// <param name="value">Numero do Enum</param>
    /// <returns>Retorna o texto do Enum</returns>
    public static string ToEnumCodigo<T>(this int value)
    {
        if (Enum.IsDefined(typeof(T), value))
        {
            var codigoEnum = (T)Enum.Parse(typeof(T), value.ToString());
            return codigoEnum.ToString();
        }

        return null;
    }

    /// <summary>
    /// Obter a descrição do atributo definido no enum, a partir de um int
    /// </summary>
    /// <param name="value">numero do Enum</param>
    /// <returns>String contendo a descrição do Enum</returns>
    public static string ToEnumDescricao<T>(this int value)
    {
        var descricaoEnum = value.ToEnumCodigo<T>().ToEnumDescricao<T>();

        return descricaoEnum;
    }
    /// <summary>
    /// Obter a descrição do atributo definido no enum, a partir de uma string.
    /// <para>Esse metodo verifica se a string é uma descrição do TIPO passado em (T)</para>
    /// </summary>
    /// <param name="value">Literal do Enum</param>
    /// <returns>String contendo a descrição do Enum</returns>
    public static string ToEnumDescricao<T>(this string value)
    {
        var descricao = "";

        foreach (var item in Enum.GetValues(typeof(T)))
        {
            if (item.ToString() == value)
            {
                var field = item.GetType().GetField(item.ToString());
                var attributes = field.GetCustomAttributes(false);

                dynamic displayAttribute = null;

                if (attributes.Any())
                {
                    displayAttribute = attributes.ElementAt(0);
                }

                descricao = displayAttribute?.Description;
                break;
            }
        }

        return descricao;
    }

    /// <summary>
    /// Verifica se o valor informado, esta presente em um Enum, na Descricao ou como Literal
    /// O valor passado é sensitivo.
    /// Exemplo de uso:
    ///     pessoa.IsEnum{TipoPessoa}(out int enumCodigo)
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="value"></param>
    /// <param name="enumCode"></param>
    /// <returns>Retorna true/false para um object Enum, e enumCode invalido ou valido caso for um objecto enum</returns>
    public static bool IsEnum<T>(this string value, out int enumCode)
    {
        var numberEnum = int.MaxValue;

        var description = value.ToEnumDescricao<T>();
        var number = value.ToEnumNumero<T>();
        var literal = value.GetCodeEnumByDescription<T>();
        var codigo = value.ToEnumCodigo<T>();

        if (string.IsNullOrWhiteSpace(description) && string.IsNullOrWhiteSpace(literal) &&
            number == int.MaxValue && string.IsNullOrWhiteSpace(codigo))
        {
            enumCode = numberEnum;
            return false;
        }

        numberEnum = number;

        if (!string.IsNullOrWhiteSpace(codigo) && int.MaxValue == number)
        {
            numberEnum = codigo.ToEnumNumero<T>();
        }

        if (!string.IsNullOrWhiteSpace(description) && int.MaxValue == number)
        {
            numberEnum = description.ToEnumNumero<T>();
        }

        if (!string.IsNullOrWhiteSpace(literal) && int.MaxValue == number)
        {
            numberEnum = literal.ToEnumNumero<T>();
        }

        enumCode = numberEnum;
        return true;
    }

    /// <summary>
    /// Valida se um atributo possui a descrição informada
    /// </summary>
    /// <typeparam name="T">Tipo do atributo </typeparam>
    /// <param name="value">Valor que se espera encontrar na descrição do atributo</param>
    /// <returns>Retorno a literal (a parte string de um Enum) do atributo correspondente a descrição informada</returns>
    public static string GetCodeEnumByDescription<T>(this string value)
    {
        var descricao = "";
        var codigo = "";

        if (value == null) return codigo;

        foreach (var item in Enum.GetValues(typeof(T)))
        {
            var field = item.GetType().GetField(item.ToString());
            var attributes = field.GetCustomAttributes(false);

            dynamic displayAttribute = null;

            if (attributes.Any())
                displayAttribute = attributes.ElementAt(0);

            descricao = displayAttribute?.Description;
            if (value.Equals(descricao))
            {
                codigo = item.ToString();
                break;
            }
        }

        return codigo;
    }

}
