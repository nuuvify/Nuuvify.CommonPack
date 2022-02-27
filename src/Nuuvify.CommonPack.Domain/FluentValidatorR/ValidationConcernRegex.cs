using System;
using System.Linq.Expressions;
using System.Text.RegularExpressions;
using Nuuvify.CommonPack.Domain;
using Nuuvify.CommonPack.Extensions.Notificator;

namespace Nuuvify.CommonPack.Domain
{
    public partial class ValidationConcernR<T> where T : NotifiableR
    {

        public ValidationConcernR<T> AssertRegexIsMatches(string pattern, string property, string value, string message = "", string aggregateId = null)
        {
            var ehNulo = (string.IsNullOrWhiteSpace(pattern) || string.IsNullOrWhiteSpace(value));

            if (ehNulo)
            {
                Name = property;
                ConfigConcernMenssage("AssertRegexIsMatches_Null", typeof(T), message: message, aggregateId: aggregateId);
            }
            else
            {
                if (!Regex.IsMatch(value, pattern) || !Regex.Match(value, pattern).Value.Equals(value))
                {
                    Name = property;
                    Field = value;
                    ConfigConcernMenssage($"AssertRegexIsMatches", typeof(T), message: message, aggregateId: aggregateId);
                }
                else
                {
                    AssertValid = true;
                }
            }

            return this;
        }

        public ValidationConcernR<T> AssertIsUrl(Expression<Func<T, string>> selector, string message = "", string aggregateId = null)
        {
            ConfigConcern(selector);

            if (DataString.Contains("localhost"))
                DataString = DataString.Replace("localhost", "google.com");

            if (string.IsNullOrWhiteSpace(DataString) ||
                !Regex.IsMatch(DataString, @"^(http:\/\/www\.|https:\/\/www\.|http:\/\/|https:\/\/)[a-z0-9]+([\-\.]{1}[a-z0-9]+)*\.[a-z]{2,5}(:[0-9]{1,5})?(\/.*)?$",
                    RegexOptions.Compiled))
            {
                ConfigConcernMenssage(nameof(AssertIsUrl), typeof(T), message: message, val: DataString, aggregateId: aggregateId);
            }
            else
            {
                AssertValid = true;
            }

            return this;
        }


        public ValidationConcernR<T> AssertIsEmail(Expression<Func<T, string>> selector, string message = "", string aggregateId = null)
        {
            ConfigConcern(selector);

            if (string.IsNullOrWhiteSpace(DataString) || !Regex.IsMatch(DataString, @"^\w+([-+.']\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*$"))
            {
                ConfigConcernMenssage(nameof(AssertIsEmail), typeof(T), message: message, val: DataString, aggregateId: aggregateId);
            }
            else
            {
                AssertValid = true;
            }


            return this;
        }

        /// <summary>
        /// Atende ao formato 08007713451 sem espaços ou traço
        /// Atende ao formato (19)2106-2597 dois ou trs digitos no DDD e numero com 8 ou 9 digitos, seprado por traço
        /// Atende ao formato XXX telefone de operadoras
        /// </summary>
        /// <param name="selector">Prorpiedade ou variavel</param>
        /// <param name="message">Não é obrigatorio, se não informado retornara mensagem padrão</param>
        /// <param name="aggregateId"></param>
        /// <returns></returns>
        public ValidationConcernR<T> AssertIsTelephone(Expression<Func<T, string>> selector, string message = "", string aggregateId = null)
        {
            ConfigConcern(selector);

            if (string.IsNullOrWhiteSpace(DataString) ||
                !Regex.IsMatch(DataString, @"^1\d\d(\d\d)?$|^0800?\d{3}?\d{4}$|^(\(0?([1-9][0-9])?[1-9]\d\)?|0?([1-9][0-9])?[1-9]\d[-])?(9|9[-])?[2-9]\d{3}[-]\d{4}$"))
            {
                ConfigConcernMenssage(nameof(AssertIsTelephone), typeof(T), message: message, val: DataString, aggregateId: aggregateId);
            }
            else
            {
                AssertValid = true;
            }

            return this;
        }

    }
}
