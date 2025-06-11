using GameStore.Controllers;
using GameStore.Data;
using GameStore.Models;
using GameStore.Services;
using GameStore.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace GameStore.Tests
{
    public class OrderControllerTests
    {

        [Fact]
        public void CartService_ShouldReturnEmpty_WhenSessionIsNull()
        {
            var cartService = TestHelper.CreateCartServiceWithoutSession();
            var cart = cartService.GetCart();

            Assert.Empty(cart);
        }

        [Fact]
        public void CartService_ShouldSaveAndGetCartCorrectly()
        {
            var cartService = TestHelper.CreateCartServiceWithSession();
            var testCart = new Dictionary<int, int> { [1] = 2 };
            cartService.SaveCart(testCart);

            var result = cartService.GetCart();
            Assert.Equal(2, result[1]);
        }


        [Fact]
        public void OrderViewModel_TotalAmount_ShouldSumCorrectly()
        {
            var model = new OrderViewModel
            {
                CartItems = new List<OrderItemViewModel>
                {
                    new() { Quantity = 1, TotalPrice = 30 },
                    new() { Quantity = 2, TotalPrice = 60 }
                }
            };

            Assert.Equal(90, model.TotalAmount);
        }

        [Fact]
        public void DiscountedPriceStrategy_Should_ApplyDiscount()
        {
            var product = new Product { Price = 100m, Discount = 20 };
            var strategy = new DiscountPriceStrategy();
            var result = strategy.CalculatePrice(product);
            Assert.Equal(80m, result);
        }

        [Fact]
        public void DiscountedPriceStrategy_Should_ReturnFullPrice_IfNoDiscount()
        {
            var product = new Product { Price = 100m, Discount = 0 };
            var strategy = new DiscountPriceStrategy();
            var result = strategy.CalculatePrice(product);
            Assert.Equal(100m, result);
        }

        [Fact]
        public void CartService_GetCart_Should_ReturnEmptyIfSessionNull()
        {
            var context = new DefaultHttpContext();
            context.Session = null!;
            var accessor = new Mock<IHttpContextAccessor>();
            accessor.Setup(a => a.HttpContext).Returns(context);

            var cartService = new CartService(accessor.Object, new Func<IPriceStrategy>(
                () => new DiscountPriceStrategy()), () => TestHelper.GetInMemoryContext());
            var cart = cartService.GetCart();

            Assert.Empty(cart);
        }

        [Fact]
        public void OrderViewModel_TotalAmount_ShouldBeCalculated()
        {
            var model = new OrderViewModel
            {
                CartItems = new List<OrderItemViewModel>
            {
                new OrderItemViewModel { Quantity = 2, TotalPrice = 10 },
                new OrderItemViewModel { Quantity = 1, TotalPrice = 5 }
            }
            };

            Assert.Equal(15, model.TotalAmount);
        }

        [Fact]
        public void CartService_ClearCart_ShouldEmptySession()
        {
            var cartService = TestHelper.CreateCartServiceWithSession();
            var cart = cartService.GetCart();
            cart[1] = 2;
            cartService.SaveCart(cart);

            cartService.ClearCart();

            var cleared = cartService.GetCart();
            Assert.Empty(cleared);
        }
    }
}
