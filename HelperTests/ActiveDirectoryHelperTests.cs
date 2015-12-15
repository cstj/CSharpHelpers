using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Helpers;
using System.Collections.Generic;
using System.DirectoryServices.AccountManagement;

namespace HelperTests
{
    [TestClass]
    public class ActiveDirectoryHelperTests
    {
        public const string domain = "myergon.local";

        public const string userName = "cstjohn";
        public const string userNameBad = "sdlfkjsdlkfj";

        public const string group = "Asset Governance Team";
        public const string groupBad = ";lkj;lkj;lkj";

        [TestMethod]
        public void IsUserMemberOfGroupTest()
        {
            bool value;

            //value = Helpers.ActiveDirectoryHelpers.IsUserMemberOfGroup("myergon.local", "Domain Users", "cstjohn");
            value = Helpers.ActiveDirectoryHelpers.IsUserMemberOfGroup(domain, group, userName);
            Assert.AreEqual(true, value, "Valid User and Group Failed");

            value = Helpers.ActiveDirectoryHelpers.IsUserMemberOfGroup(domain, groupBad, userName);
            Assert.AreEqual(false, value, "InValid Group Invalid User Failed");

            value = Helpers.ActiveDirectoryHelpers.IsUserMemberOfGroup(domain, group, userNameBad);
            Assert.AreEqual(false, value, "Valid Group Valid User Failed");

            value = Helpers.ActiveDirectoryHelpers.IsUserMemberOfGroup(domain, groupBad, userNameBad);
            Assert.AreEqual(false, value, "InValid Group InValid User Failed");
        }

        [TestMethod]
        public void GetAccountGroupsTest()
        {
            List<string> value;
            value = Helpers.ActiveDirectoryHelpers.GetAccountGroups(domain, userName);
            Assert.IsTrue(value.Count > 0, "Valid Account Failed");

            value = Helpers.ActiveDirectoryHelpers.GetAccountGroups(domain, userNameBad);
            Assert.IsTrue(value.Count == 0, "Invalid Account Failed");
        }

        [TestMethod]
        public void GetGroupFromDomainTests()
        {
            GroupPrincipal value;
            
            value = Helpers.ActiveDirectoryHelpers.GetGroupFromDomain(domain, group);
            Assert.IsNotNull(value, "Valid Group Failed");

            value = Helpers.ActiveDirectoryHelpers.GetGroupFromDomain(domain, groupBad);
            Assert.IsNull(value, "InValid Group Failed");
        }

        [TestMethod]
        public void GetUserFromDomainTests()
        {
            UserPrincipal value;

            value = Helpers.ActiveDirectoryHelpers.GetUserFromDomain(domain, userName);
            Assert.IsNotNull(value, "Valid Group Failed");

            value = Helpers.ActiveDirectoryHelpers.GetUserFromDomain(domain, userNameBad);
            Assert.IsNull(value, "InValid Group Failed");
        }

        [TestMethod]
        public void IsUserDomainMemberTests()
        {
            bool value;

            value = Helpers.ActiveDirectoryHelpers.IsUserDomainMember(domain, userName);
            Assert.IsTrue(value, "Valid User Failed");

            value = Helpers.ActiveDirectoryHelpers.IsUserDomainMember(domain, userNameBad);
            Assert.IsFalse(value, "InValid Group Failed");
        }

        [TestMethod]
        public void SearchForUserTests()
        {
            Assert.IsTrue(Helpers.ActiveDirectoryHelpers.SearchForUser(domain, userNameBad).Count == 0);

            Assert.IsTrue(Helpers.ActiveDirectoryHelpers.SearchForUser(domain, userName).Count > 0);
        }
    }
}
