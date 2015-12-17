using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.DirectoryServices.AccountManagement;
using System.DirectoryServices;

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

        public static List<DirectoryEntry> SearchForUser(string domain, string search)
        {
            var tmp = new List<DirectoryEntry>();
            if (search != null)
            {
                //Remove domain if they have put it in the search string
                if (search.Length > domain.Length)
                {
                    if (search.Substring(0, domain.Length) == domain) search = search.Substring(domain.Length + 1);
                }
                if (search.Length >= 3)
                {
                    System.DirectoryServices.DirectoryEntry de = new System.DirectoryServices.DirectoryEntry("LDAP://" + domain);
                    System.DirectoryServices.DirectorySearcher directorySearcher = new System.DirectoryServices.DirectorySearcher(de);
                    directorySearcher.Filter = "(&(|(objectclass=user)(objectclass=computer))(|(samaccountname=" + search + ")(displayname=" + search + ")(sn=" + search + ")(mail=" + search + ")))";
                    System.DirectoryServices.SearchResultCollection srCollection = directorySearcher.FindAll();

                    System.Threading.Tasks.Parallel.For(0, srCollection.Count, (i) =>
                    {
                        var item = srCollection[i].GetDirectoryEntry();
                        lock (tmp) tmp.Add(item);
                    });
                }
            }
            return tmp;
        }
    }
}
