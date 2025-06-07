using GameStore.Data;
using GameStore.Models;
using GameStore.Services;
using GameStore.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace GameStore.Tests
{
    public static class TestHelper
    {
        public static ApplicationDbContext GetInMemoryContext()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: $"TestDb_{Guid.NewGuid()}")
                .Options;
            return new ApplicationDbContext(options);
        }

        public static Mock<UserManager<ApplicationUser>> MockUserManager(ApplicationUser user)
        {
            var store = new Mock<IUserStore<ApplicationUser>>();
            var userManager = new Mock<UserManager<ApplicationUser>>(store.Object, null, null, null, null, null, null, null, null);
            userManager.Setup(m => m.FindByIdAsync(It.IsAny<string>())).ReturnsAsync(user);
            userManager.Setup(m => m.GetUserId(It.IsAny<ClaimsPrincipal>())).Returns(user.Id);
            return userManager;
        }

        public static void SetUser(Controller controller, ApplicationUser user)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(ClaimTypes.Name, user.UserName)
            };
            var identity = new ClaimsIdentity(claims, "TestAuth");
            controller.ControllerContext.HttpContext.User = new ClaimsPrincipal(identity);
        }

        public static CartService CreateCartServiceWithSession()
        {
            var context = new DefaultHttpContext();
            var session = new TestSession();
            context.Session = session;

            var accessor = new Mock<IHttpContextAccessor>();
            accessor.Setup(a => a.HttpContext).Returns(context);

            var dbContext = GetInMemoryContext();

            return new CartService(accessor.Object, new Func<IPriceStrategy>(() => new DiscountPriceStrategy()), () => dbContext);
        }

        public static CartService CreateCartServiceWithoutSession()
        {
            var context = new DefaultHttpContext();
            context.Session = null!;
            var accessor = new Mock<IHttpContextAccessor>();
            accessor.Setup(a => a.HttpContext).Returns(context);
            var dbContext = GetInMemoryContext();
            return new CartService(accessor.Object, new Func<IPriceStrategy>(() => new DiscountPriceStrategy()), () => dbContext);
        }

        public class TestSession : ISession
        {
            private readonly Dictionary<string, byte[]> _store = new();
            public IEnumerable<string> Keys => _store.Keys;
            public string Id => "test";
            public bool IsAvailable => true;
            public void Clear() => _store.Clear();
            public void Remove(string key) => _store.Remove(key);
            public void Set(string key, byte[] value) => _store[key] = value;
            public bool TryGetValue(string key, out byte[] value) => _store.TryGetValue(key, out value);
            public void SetString(string key, string value) => Set(key, Encoding.UTF8.GetBytes(value));
            public string? GetString(string key) => _store.ContainsKey(key) ? Encoding.UTF8.GetString(_store[key]) : null;
            public Task LoadAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;
            public Task CommitAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;
            public void Load() { }
            public void Commit() { }
        }
    }
}
