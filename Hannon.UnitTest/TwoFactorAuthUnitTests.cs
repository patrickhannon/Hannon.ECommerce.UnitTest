using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Configuration;
using hannon._2factorAuth;
using hannon.TwoFactorAuth.Models;
using hannon._2factorAuth.Models;
using System.Web;
namespace Hannon.UnitTest
{
    [TestClass]
    public class TwoFactorAuthUnitTests
    {
        private int _twoFactorAuthTimeSpan;
        private string _twoFactorAuthCookie;
        private string _twoFactorAuthSmtpHost;
        private string _twoFactorAuthFromEmail;
        private string _twoFactorAuthFromPhone;
        private string _authToken;
        private string _accountSID;
        private int _twoFactorTimeOut;
        private ITwoFactorAuth _twoFactorAuth;

        public TwoFactorAuthUnitTests()
        {
            int.TryParse(ConfigurationManager.AppSettings["TwoFactorAuthTimeSpan"], out _twoFactorAuthTimeSpan);
            int.TryParse(ConfigurationManager.AppSettings["TwoFactorTimeOut"], out _twoFactorTimeOut);
            _twoFactorAuthCookie = ConfigurationManager.AppSettings["TwoFactorAuthCookie"];
            _twoFactorAuthSmtpHost = ConfigurationManager.AppSettings["TwoFactorAuthSmtpHost"];
            _twoFactorAuthFromEmail = ConfigurationManager.AppSettings["TwoFactorAuthFromEmail"];
            _twoFactorAuthFromPhone = ConfigurationManager.AppSettings["TwoFactorAuthFromPhone"];
            _authToken = ConfigurationManager.AppSettings["TwilioAuthToken"];
            _accountSID = ConfigurationManager.AppSettings["TwilioAccountSID"];

            var smsConfigs = new InitTwoFactor()
            {
                TwoFactorAuthTimeSpan = _twoFactorAuthTimeSpan,
                TwoFactorAuthCookie = _twoFactorAuthCookie,
                TwoFactorAuthSmtpHost = _twoFactorAuthSmtpHost,
                TwoFactorAuthFromEmail = _twoFactorAuthFromEmail,
                TwoFactorAuthFromPhone = _twoFactorAuthFromPhone,
                AuthToken = _authToken,
                AccountSID = _accountSID
            };

            //_identity = new TranparentIdentity();
            _twoFactorAuth = new TwoFactorAuth(smsConfigs);
        }

        [TestMethod]
        public void ValidateSMSIsSent()
        {
            try
            {
                var mockHttpContext = MockHttpSession.FakeHttpContext();
                var mockSession = mockHttpContext.Session;
                var request = new TwoFactorRequestModel()
                {
                    Provider = Provider.SMS,
                    UserValue = "18174120313"
                };
                var response = _twoFactorAuth.CreateTwoFactorAuth(request, mockSession);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        [TestMethod]
        public void ValidateEmailIsSent()
        {
            try
            {
                var mockHttpContext = MockHttpSession.FakeHttpContext();
                //get the mock session
                var mockSession = mockHttpContext.Session;
                var request = new TwoFactorRequestModel()
                {
                    Provider = Provider.Email,
                    UserValue = "patrick_hannon@hotmail.com"
                };
                var response = _twoFactorAuth.CreateTwoFactorAuth(request, mockSession);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        public void TestLogin()
        {
            //AccountController acct = new AccountController();
            //acct.SetFakeControllerContext();
            //acct.SetBusinessObject(mockBO.Object);

            //RedirectResult results = (RedirectResult)acct.LogOn(userName, password, rememberMe, returnUrl);
            //Assert.AreEqual(returnUrl, results.Url);
            //Assert.AreEqual(userName, acct.Session["txtUserName"]);
            //Assert.IsNotNull(acct.Session["SessionGUID"]);
        }
    }
}
