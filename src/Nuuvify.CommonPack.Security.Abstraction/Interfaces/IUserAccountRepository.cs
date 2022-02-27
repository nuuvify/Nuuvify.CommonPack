using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Nuuvify.CommonPack.Security.Abstraction
{
    public interface IUserAccountRepository
    {
        Task<IEnumerable<PersonRoleQueryResult>> GetUserRoles(string login);
        Task<bool> PersonIsMemberOf(string login, string claimType);
    }
}