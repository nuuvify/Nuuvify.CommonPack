using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using Microsoft.Extensions.Configuration;

namespace Nuuvify.CommonPack.Security.Jwt
{

    /// <summary>
    /// Essa classe cria dinamicamente um objeto que fizer parte da seguinte entrada do arquivo appsettings.json
    /// <example>
    /// <para>
    /// Sera exibido o valor de PolicyGroupsApplication: GroupUsers contido no appsettings.json <br/>
    /// Voce pode criar quantas entradas quiser na tag PolicyGroupsApplication: MeuGrupo, Meugrupo1, Meugrupo2 <br/>
    /// </para>
    /// <code>
    ///             dynamic suaVariavel = new PolicyGroupsApplication(_configuration);
    ///             Console.WriteLine(suaVariavel.GroupUsers);
    /// </code>
    /// </example>    
    /// </summary>
    public class PolicyGroupsApplication : DynamicObject
    {


        public IDictionary<string, string> PolicyGroups { get; set; }

        public PolicyGroupsApplication(IConfiguration configuration)
        {
            PolicyGroups = configuration.GetSection(nameof(PolicyGroupsApplication))
                .GetChildren()?
                .Where(x => !string.IsNullOrWhiteSpace(x.Value))?
                .ToDictionary(x => x.Key, x => x.Value);
        }


        public string GetPropertyValue(string propertyName)
        {

            if (PolicyGroups.TryGetValue(propertyName, out string value))
            {
               return value;
            }

            return null;
        }

        /// <summary>
        /// É assionado quando usado: suaVariavel.GroupUsers;
        /// </summary>
        /// <param name="binder">Nome da propriedade</param>
        /// <param name="result"></param>
        /// <returns></returns>
        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            result = GetPropertyValue(binder.Name);
            return result != null;
        }

        /// <summary>
        /// É assionado quando usado: suaVariavel.GroupUsers("ZAZAZA");
        /// </summary>
        /// <param name="binder">Nome do metodo, o mesmo que a chave do appsettings "PolicyGroupsApplication":"GroupUsers"</param>
        /// <param name="args">Valores que deseja verificar se fazem parte do metodo</param>
        /// <param name="result"></param>
        /// <returns></returns>
        public override bool TryInvokeMember(InvokeMemberBinder binder, object[] args, out object result)
        {
            result = GetPropertyValue(binder.Name);

            if (result != null)
            {
                var text = result;
                var exists = args.Any(x => x.ToString().ToUpperInvariant().Equals(text.ToString().ToUpperInvariant()));
                return exists;
            }

            return result != null;
        }


    }

}