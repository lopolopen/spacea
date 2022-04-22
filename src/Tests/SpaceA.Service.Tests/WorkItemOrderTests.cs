using System;
using SpaceA.WebApi.Services;
using NUnit.Allure.Attributes;
using NUnit.Allure.Core;
using NUnit.Framework;

namespace SpaceA.UnitTests
{
    [TestFixture]
    [AllureNUnit]
    [AllureSuite("Workitem order tests")]
    public class WorkItemOrderTests
    {
        [TestCase("k", null, "l")]
        [TestCase("xk", null, "xl")]
        [TestCase("k", "l", "k:k")]
        [TestCase("k", "z", "k:k")]
        [TestCase("k", "k:k", "k:j")]
        [TestCase("z", null, "z:k")]
        public void Increase_Order_Exact(string befor, string guard, string afterInc)
        {
            Assert.True(afterInc == OrderService.Increase(befor, guard));
        }

        [TestCase("k", null, "j")]
        [TestCase("xk", null, "xj")]
        [TestCase("k", "j", "j:k")]
        [TestCase("k", "b", "b:k")]
        [TestCase("k", "j:k", "j:l")]
        [TestCase("b", null, "a:k")]
        public void Decrease_Order_Exact(string befor, string guard, string after)
        {
            Assert.True(after == OrderService.Decrease(befor, guard));
        }

        [TestCase("x", null)]
        [TestCase("c", "d")]
        [TestCase("c", "c:c")]
        [TestCase("z", "z:k")]
        public void Increase_Order_Rough(string befor, string guard)
        {
            var after = OrderService.Increase(befor, guard);
            Assert.True(string.Compare(befor, after) < 0);
            Assert.True(guard == null || string.Compare(guard, after) > 0);
        }

        [TestCase("y", null)]
        [TestCase("k", "f")]
        [TestCase("f", "b:k:k")]
        [TestCase("a:k", "a:j")]
        public void Decrease_Order_Rough(string befor, string guard)
        {
            var after = OrderService.Decrease(befor, guard);
            Assert.True(string.Compare(befor, after) > 0);
            Assert.True(guard == null || string.Compare(guard, after) < 0);
        }

        [Test]
        public void Increase_Order_Exception()
        {
            Assert.Throws<Exception>(() =>
            {
                OrderService.Increase("k", "a");
            });
        }

        [Test]
        public void Decrease_Order_Exception()
        {
            Assert.Throws<Exception>(() =>
            {
                OrderService.Decrease("a", "k");
            });
        }
    }
}
