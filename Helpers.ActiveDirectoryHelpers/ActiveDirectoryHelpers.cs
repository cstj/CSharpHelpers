using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.DirectoryServices.AccountManagement;

namespace Helpers
{
    public class ActiveDirectoryHelpers
    {
        public static bool IsUserDomainMember(string domain, string group)
        {
            if (GetUserFromDomain(domain, group) != null) return true;
            return false;
        }

        public static bool IsUserMemberOfGroup(string domain, string group, string userName)
        {
            UserPrincipal user = GetUserFromDomain(domain, userName);
            GroupPrincipal grp = GetGroupFromDomain(domain, group);
            if (user != null && grp != null)
            {
                // check if user is member of that group
                if (user.IsMemberOf(grp))
                {
                    return true;
                }
            }
            return false;
        }

        public static UserPrincipal GetUserFromDomain(string domain, string userName)
        {
            PrincipalContext ctx = new PrincipalContext(ContextType.Domain, domain);
            return UserPrincipal.FindByIdentity(ctx, userName);
        }

        public static GroupPrincipal GetGroupFromDomain(string domain, string group)
        {
            PrincipalContext ctx = new PrincipalContext(ContextType.Domain, domain);
            return GroupPrincipal.FindByIdentity(ctx, group);
        }

        public static List<string> GetAccountGroups(string domain, string userName)
        {
            UserPrincipal user = GetUserFromDomain(domain, userName);
            if (user != null)
            {
                var src = user.GetGroups(new PrincipalContext(ContextType.Domain, domain));
                var result = new List<string>();
                src.ToList().ForEach(sr => result.Add(sr.SamAccountName));
                return result;
            }
            return new List<string>();
        }
    }
}
