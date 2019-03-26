using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Configuration;
using System.Linq;
using System.Linq.Expressions;
using hannon._2factorAuth;
using hannon.TwoFactorAuth.Models;
using hannon._2factorAuth.Models;
using System.Web;
using ECommerce.Controllers;
using ECommerce.Data.Entities.Catalog;
using ECommerce.Data.Entities.Customers;
using ECommerce.Data.Entities.Orders;
using ECommerce.Data.Repository;
using ECommerce.Models;
using ECommerce.Services;
using ECommerce.Services.Catalog;
using ECommerce.Services.Catalog.Impl;
using ECommerce.Services.Customer;
using ECommerce.Services.Customer.Impl;
using ECommerce.Services.Impl;
using ECommerce.Services.Menu;
using ECommerce.Services.Menu.Impl;
using Hannon.PayFabric;
using Moq;
using Nop.Core.Domain.Common;
using System.Diagnostics;

namespace Hannon.UnitTest
{
    [TestClass]
    public class ECommerceUnitTests
    {
        private int _twoFactorAuthTimeSpan;
        private string _twoFactorAuthCookie;
        private string _twoFactorAuthSmtpHost;
        private string _twoFactorAuthFromEmail;
        private string _twoFactorAuthFromPhone;
        private string _authToken;
        private string _accountSID;
        private static string _payfabricDeviceId;
        private static string _payfabricDevicePassword;
        private static string _payfabricDeviceUrl;
        private int _twoFactorTimeOut;
        private ITwoFactorAuth _twoFactorAuth;
        public string connectionString = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;
        private string _emailPassword;
        private bool _twoFactorEnabled;
        private IMenuService _menuService;
        private ICategoryService _categoryService;
        private ICartService _cartService;
        private IProductService _productService;
        private ICustomerService _customerService;
        private ITransaction _transaction;
        
        public ECommerceUnitTests()
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

            //Get payfabric configs
            _payfabricDeviceId = ConfigurationManager.AppSettings["PayfabricDeviceId"];
            _payfabricDevicePassword = ConfigurationManager.AppSettings["PayfabricDevicePassword"];
            _payfabricDeviceUrl = ConfigurationManager.AppSettings["PayfabricDeviceUrl"];


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
            _twoFactorAuth = new TwoFactorAuth(smsConfigs);

            //repositories
            var catalogSettings = new CatalogSettings();
            var commonSettings = new CommonSettings();
            var categoryRepository = new CategoryRepository(connectionString);
            var productRepository = new ProductRepository(connectionString);
            var productCategory = new ProductCategoryMappingRepository(connectionString);
            var shoppingCartRepository = new ShoppingCartItemRepository(connectionString);

            var _pictureBinaryRepository = new PictureBinaryRepository(connectionString);
            var _pictureRepository = new PictureRepository(connectionString);
            var _productAttributeCombinationRepository = new ProductAttributeCombinationRepository(connectionString);
            var _productAttributeRepository = new ProductAttributeRepository(connectionString);
            var _productAttributeValueRepository = new ProductAttributeValueRepository(connectionString);
            var _productAvailabilityRangeRepository = new ProductAvailabilityRangeRepository(connectionString);
            var _productCategoryMappingRepository = new ProductCategoryMappingRepository(connectionString);
            var _productProductAttributeMappingRepository = new ProductProductAttributeMappingRepository(connectionString);
            var _productProductTagMappingRepository = new ProductTagMappingRepository(connectionString);
            var _productSpecificationAttributeMappingRepository = new ProductSpecificationAttributeRepository(connectionString);
            var _specificationAttributeOptionRepository = new SpecificationAttributeOptionRepository(connectionString);
            var _specificationAttributeRepository = new SpecificationAttributeRepository(connectionString);
            var _productRepository = new ProductRepository(connectionString);
            var _productManufacturerMappingRepository = new ProductManufacturerMappingRepository(connectionString);
            var _productPictureRepository = new ProductPictureRepository(connectionString);
            var _productReviewsRepository = new ProductReviewsRepository(connectionString);
            var _tierPricesRepository = new TierPricesRepository(connectionString);
            var _discountProductMappingRepository = new DiscountProductMappingRepository(connectionString);
            var _productWarehouseInventoryRepository = new ProductWarehouseInventoryRepository(connectionString);
            var _customerRepository = new CustomerRepository(connectionString);
            //services
            _categoryService = new CategoryService(catalogSettings, commonSettings, categoryRepository, productRepository, productCategory );

            _menuService = new MenuService(_categoryService);
            _cartService = new CartService(commonSettings, shoppingCartRepository);
            _productService = new ProductService(
                _pictureBinaryRepository,
                _pictureRepository,
                _productAttributeCombinationRepository,
                _productAttributeRepository,
                _productAttributeValueRepository,
                _productAvailabilityRangeRepository,
                _productCategoryMappingRepository,
                _productProductAttributeMappingRepository,
                _productProductTagMappingRepository,
                _productSpecificationAttributeMappingRepository,
                _specificationAttributeOptionRepository,
                _specificationAttributeRepository,
                _productRepository,
                _productManufacturerMappingRepository,
                _productPictureRepository,
                _productReviewsRepository,
                _tierPricesRepository,
                _discountProductMappingRepository,
                _productWarehouseInventoryRepository
            );
            _customerService = new CustomerService(_customerRepository);
        }

        [TestMethod()]
        public void TestCreateCustomer()
        {
            var c = new Customer()
            {
                CustomerGuid = Guid.NewGuid(),
                Username = "test",
                Email = "c@r.com",
                EmailToRevalidate = string.Empty,
                AdminComment = string.Empty,
                IsTaxExempt = false,
                AffiliateId = 0,
                VendorId = 0,
                HasShoppingCartItems = false,
                RequireReLogin = false,
                FailedLoginAttempts = 0,
                Active = true,
                Deleted = false,
                IsSystemAccount = false,
                SystemName = string.Empty,
                LastIpAddress = String.Empty,
                CreatedOnUtc = DateTime.UtcNow,
                LastActivityDateUtc = DateTime.UtcNow,
                RegisteredInStoreId = 1
            };
            var r = _customerService.Create(c);
            Assert.IsTrue(r.Status);
        }

        [TestMethod()]
        public void TestGetCustomer()
        {
            var c = _customerService.GetCustomer(2);
            Assert.IsNotNull(c);
        }

        [TestMethod()]
        public void TestAddProductCart()
        {
            //4 Apple MacBook Pro 13 - inch
            var p = _productService.GetProductById(4);
            var c = _customerService.GetCustomer(2);
            var item = new ShoppingCartItem()
            {
                ProductId = Int32.Parse(p.Id),
                StoreId = 1,
                ShoppingCartTypeId = (int)ShoppingCartType.ShoppingCart,
                CreatedOnUtc = DateTime.UtcNow,
                CustomerId = c.Id,
                Quantity = 1,
                UpdatedOnUtc = DateTime.UtcNow
            };
            _cartService.AddProductToCart(item);
        }

        [TestMethod()]
        public void TestGetProductById()
        {
            var p = _productService.GetProductById(4);
            Assert.IsNotNull(p);
        }

        [TestMethod()]
        public void TestRemoveProductCart()
        {

        }

        [TestMethod()]
        public void TestPayfabricTokenCreate()
        {
            _transaction = new PayfabricTransaction(_payfabricDeviceId,
                _payfabricDevicePassword, _payfabricDeviceUrl);
            var response = _transaction.CreateSecurityToken();
            Assert.IsTrue(response.StatusCode.Equals("OK"));
        }

        [TestMethod()]
        public void TestPayfabricCreateTransaction()
        {
            _transaction = new PayfabricTransaction(_payfabricDeviceId,
                _payfabricDevicePassword, _payfabricDeviceUrl);
            var response = _transaction.CreateSecurityToken();
            Assert.IsTrue(response.StatusCode.Equals("OK"));
        }

        [TestMethod()]
        public void TestGetFeaturedProducts()
        {
            //Use flag IsFeaturedProduct in [Product_Category_Mapping]
            var featured = _categoryService.GetFeaturedProducts(true);
        }

        [TestMethod()]
        public void TestSummaryProduct()
        {
            //6 Samsung Series 9 NP900X4C Premium Ultrabook
            var p = _productService.GetProductById(6);
            //picture
            var pictures = p.ProductPictures;
            //name
            var name = p.Name;
            var shortDesc = p.ShortDescription;
            var longDesc = p.FullDescription;
            var metaDesc = p.MetaDescription;
            var metaTitle = p.MetaTitle;

            //from price 
            var price = p.Price;
            //test add to cart link
            
            //stars
            var stars = p.ProductReviews;
        }

        [TestMethod()]
        public void TestDetailProduct()
        {
            //9 Lenovo Thinkpad X1 Carbon Laptop
            var p = _productService.GetProductById(9);
            //picture
            //name
            //short description
            //full description
            //from price 
            //stars
            //show details link
            //add to wish list link
            //product tags 
        }

        [TestMethod()]
        public void TestAllProductsBelongingToCategory()
        {
            //2 Desktops
            var products = _categoryService.GetAllProductsBelongingToCategory(2);
            Assert.IsTrue(products.Any());
        }

        [TestMethod()]
        public void TestGetPopularTags()
        {

        }

        [TestMethod()]
        public void TestGetWishList(){}

        [TestMethod()]
        public void TestSearch() { }

        [TestMethod()]
        public void TestGetMyAccount() { }

        [TestMethod()]
        public void TestGetMyShoppingCart() { }

        [TestMethod()]
        public void ApplyForVendorAccount() { }

        [TestMethod()]
        public void TestRecentlyViewedProducts() { }

        [TestMethod()]
        public void TestPlaceOrder() { }

        [TestMethod()]
        public void TestSignIn()
        {

        }

        [TestMethod()]
        public void TestRegister()
        {

        }

        [TestMethod()]
        public void VerifyCatalogWithSubMenus()
        {
            var menu = _menuService.LoadCategories();
            Assert.IsNotNull(menu);
            var electronics = menu.MenuItems.Where(x => x.MenuId == 5).FirstOrDefault();
            Assert.IsTrue(electronics.SubMenus.Count > 0, "c.GetCategories");
        }

        [TestMethod]
        [Ignore()]
        public void VerifyProductService()
        {
            var productModel = _productService.GetProductById(7);
            Assert.IsNotNull(productModel);
            Assert.IsTrue(productModel.ProductPictures.Count > 0);
        }

        [TestMethod]
        [Ignore()]
        public void VerifyDataLayerRepositories()
        {
            var p = new ProductRepository(connectionString);
            var product = p.GetById(7);
            Assert.IsNotNull(product);
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
            mockSession["AuthCode"] = "123456";
            var r = acct.VerifyCode(model);
            Assert.IsNotNull(r);

            //RedirectResult results = (RedirectResult)acct.LogOn(userName, password, rememberMe, returnUrl);
            //Assert.AreEqual(returnUrl, results.Url);
            //Assert.AreEqual(userName, acct.Session["txtUserName"]);
            //Assert.IsNotNull(acct.Session["SessionGUID"]);
        }
    }
}
