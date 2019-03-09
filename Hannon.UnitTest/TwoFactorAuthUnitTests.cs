using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Configuration;
using hannon._2factorAuth;
using hannon.TwoFactorAuth.Models;
using hannon._2factorAuth.Models;
using System.Web;
using ECommerce.Controllers;
using ECommerce.Data.Repository;
using ECommerce.Models;
using Moq;
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
        public string connectionString = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;
        private string _emailPassword;
        private bool _twoFactorEnabled;

        public TwoFactorAuthUnitTests()
        {
            int.TryParse(ConfigurationManager.AppSettings["TwoFactorAuthTimeSpan"], out _twoFactorAuthTimeSpan);
            int.TryParse(ConfigurationManager.AppSettings["TwoFactorTimeOut"], out _twoFactorTimeOut);
            bool.TryParse(ConfigurationManager.AppSettings["TwoFactorEnabled"], out _twoFactorEnabled);
            _twoFactorAuthCookie = ConfigurationManager.AppSettings["TwoFactorAuthCookie"];
            _twoFactorAuthSmtpHost = ConfigurationManager.AppSettings["TwoFactorAuthSmtpHost"];
            _twoFactorAuthFromEmail = ConfigurationManager.AppSettings["TwoFactorAuthFromEmail"];
            _twoFactorAuthFromPhone = ConfigurationManager.AppSettings["TwoFactorAuthFromPhone"];
            _emailPassword = ConfigurationManager.AppSettings["EmailPassword"];
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
                AccountSID = _accountSID,
                TwoFactorEnabled = _twoFactorEnabled,
                EmailPassword = _emailPassword
            };
            //_identity = new TranparentIdentity();
            _twoFactorAuth = new TwoFactorAuth(smsConfigs);
        }

        [TestMethod]
        public void VerifyDataLayerRepositories()
        {
            var p = new ProductRepository(connectionString);
            var product = p.GetById(7);
            Assert.IsNotNull(product);

            var c = new CategoryRepository(connectionString);
            var count = c.Get().Count;
            Assert.IsNotNull(c.Get());
            Assert.IsTrue(count > 0, "c.GetCategories");
        }

        [TestMethod]
        [Ignore()]
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
        [Ignore()]
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

        [TestMethod]
        [Ignore()]
        public void VerifyCodeTest()
        {
            AccountController acct = new AccountController();
            //var mockedPrincipal = new Mock<WindowsPrincipal>(WindowsIdentity.GetCurrent());

            //mockedPrincipal.SetupGet(x => x.Identity.IsAuthenticated).Returns(true);
            //mockedPrincipal.SetupGet(x => x.Identity.Name).Returns("HANNON\\phannon");
            //mockedPrincipal.Setup(x => x.IsInRole("Domain\\Group1")).Returns(true);
            //mockedPrincipal.Setup(x => x.IsInRole("Domain\\Group2")).Returns(false);


            var model = new VerifyCodeViewModel()
            {
                Code = "123456"
            };

            var mockHttpContext = MockHttpSession.FakeHttpContext();
            var mockSession = mockHttpContext.Session;
            mockSession["AuthCode"]= "123456";
            var r = acct.VerifyCode(model);
            Assert.IsNotNull(r);
            
            //RedirectResult results = (RedirectResult)acct.LogOn(userName, password, rememberMe, returnUrl);
            //Assert.AreEqual(returnUrl, results.Url);
            //Assert.AreEqual(userName, acct.Session["txtUserName"]);
            //Assert.IsNotNull(acct.Session["SessionGUID"]);
        }
    }
}
