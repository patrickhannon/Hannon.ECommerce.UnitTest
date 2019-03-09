using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ECommerce.Data.Repository;
using ECommerce.Data.Entities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Diagnostics;

namespace Hannon.UnitTest
{
    [TestClass]
    class DataLayerUnitTests
    {
        public string connectionString = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;

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
    }
}
