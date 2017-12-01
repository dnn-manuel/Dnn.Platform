﻿using System;
using System.Reflection;
using System.Web;
using DotNetNuke.Common;
using DotNetNuke.Common.Utilities;
using DotNetNuke.ComponentModel;
using DotNetNuke.Data;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Users;
using DotNetNuke.Entities.Users.Membership;
using DotNetNuke.Security.Membership;
using DotNetNuke.Security.Profile;
using DotNetNuke.Security.Roles;
using DotNetNuke.Services.Cache;
using DotNetNuke.Services.Log.EventLog;
using DotNetNuke.Tests.Utilities;
using NUnit.Framework;

namespace DotNetNuke.Tests.Core.Providers.Membership
{
    [TestFixture]
    public class MembershipProviderTests : DnnUnitTest
    {
        [SetUp]
        public void SetUp()
        {
            Globals.SetStatus(Globals.UpgradeStatus.None);

            ComponentFactory.Container = new SimpleContainer();
            ComponentFactory.InstallComponents(new ProviderInstaller("data", typeof(DataProvider), typeof(SqlDataProvider)));
            ComponentFactory.InstallComponents(new ProviderInstaller("caching", typeof(CachingProvider), typeof(FBCachingProvider)));
            ComponentFactory.InstallComponents(new ProviderInstaller("logging", typeof(LoggingProvider), typeof(DBLoggingProvider)));
            ComponentFactory.InstallComponents(new ProviderInstaller("members", typeof(MembershipProvider), typeof(AspNetMembershipProvider)));
            ComponentFactory.InstallComponents(new ProviderInstaller("roles", typeof(RoleProvider), typeof(DNNRoleProvider)));
            ComponentFactory.InstallComponents(new ProviderInstaller("profiles", typeof(ProfileProvider), typeof(DNNProfileProvider)));
            ComponentFactory.RegisterComponent<IPortalSettingsController, PortalSettingsController>();

            PortalController.ClearInstance();
            UserController.ClearInstance();
            RoleController.ClearInstance();

            var roleController = RoleController.Instance;
            var roleProviderField = roleController.GetType().GetField("provider", BindingFlags.NonPublic | BindingFlags.Static);
            if (roleProviderField != null)
            {
                roleProviderField.SetValue(roleController, RoleProvider.Instance());
            }

            var membershipType = typeof(System.Web.Security.Membership);
            var initializedDefaultProviderField = membershipType.GetField("s_InitializedDefaultProvider", BindingFlags.NonPublic | BindingFlags.Static);
            var defaultProviderField = membershipType.GetField("s_Provider", BindingFlags.NonPublic | BindingFlags.Static);
            if (initializedDefaultProviderField != null
                && defaultProviderField != null
                && (bool)initializedDefaultProviderField.GetValue(null) == false)
            {
                initializedDefaultProviderField.SetValue(null, true);
                defaultProviderField.SetValue(null, System.Web.Security.Membership.Providers["AspNetSqlMembershipProvider"]);
            }
        }

        [TearDown]
        public void TearDown()
        {
        }

        //TODO: Must be moved to integration tests.
        //Note: this is the only test in core unit testing project that requires a working db connection to run.
        [Test]
        public void Password_Should_Saved_In_History_During_Create_User()
        {
            var username = $"{Constants.RuFirstName}{DateTime.Now.Ticks}";
            var email = $"{username}@dnn.com";

            UserInfo user = null;

            Assert.DoesNotThrow(() =>
            {
                user = new UserInfo
                {
                    PortalID = Constants.PORTAL_Zero,
                    UserID = Null.NullInteger,
                    Username = username,
                    Email = email,
                    FirstName = username, // accessing this and others requires Profile property access
                    LastName = string.Empty,
                    Membership = new UserMembership
                    {
                        Approved = true,
                        Password = Constants.DefaultPassword
                    }
                };
            }, "Make sure your connection string is set correctly in the App.config file");

            var status = UserController.CreateUser(ref user);

            Assert.AreEqual(UserCreateStatus.Success, status);

            var simulator = new Instance.Utilities.HttpSimulator.HttpSimulator("/", AppDomain.CurrentDomain.BaseDirectory);
            simulator.SimulateRequest(new Uri(WebsiteAppPath));
            HttpContextBase httpContextBase = new HttpContextWrapper(HttpContext.Current);
            HttpContextSource.RegisterInstance(httpContextBase);

            var isPasswordInHistory = new MembershipPasswordController().IsPasswordInHistory(user.UserID, user.PortalID, user.Membership.Password);

            Assert.AreEqual(true, isPasswordInHistory);
        }
    }
}