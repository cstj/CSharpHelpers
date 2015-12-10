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

        public static HashSet<Principal> SearchForUser(string domain, string search)
        {
            if (search != null)
            {
                if (search.Length >= 3)
                {
                    System.Collections.Concurrent.ConcurrentDictionary<Guid, Principal> tmp = new System.Collections.Concurrent.ConcurrentDictionary<Guid, Principal>();
                    string strSearch = "*" + search + "*";

                    PrincipalContext ctx = new PrincipalContext(ContextType.Domain, domain);
                    List<UserPrincipal> searchPrinciples = new List<UserPrincipal>();
                    searchPrinciples.Add(new UserPrincipal(ctx) { DisplayName = strSearch });
                    searchPrinciples.Add(new UserPrincipal(ctx) { SamAccountName = strSearch });
                    searchPrinciples.Add(new UserPrincipal(ctx) { MiddleName = strSearch });
                    searchPrinciples.Add(new UserPrincipal(ctx) { GivenName = strSearch });
                    searchPrinciples.Add(new UserPrincipal(ctx) { Surname = strSearch });
                    searchPrinciples.Add(new UserPrincipal(ctx) { UserPrincipalName = strSearch });

                    System.Threading.Tasks.Parallel.ForEach(searchPrinciples, p =>
                    {
                        PrincipalSearcher sch = new PrincipalSearcher(p);
                        foreach (var found in sch.FindAll())
                        {
                            if (found.Guid.HasValue)
                            {
                                if (!tmp.Keys.Contains(found.Guid.Value)) tmp.TryAdd(found.Guid.Value, found);
                            }
                        }
                    });
                    return new HashSet<Principal>(tmp.Values);
                }
            }
            return new HashSet<Principal>();
        }
    }
}
