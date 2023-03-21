using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using OrchardCore.Environment.Shell;
using OrganiMedCore.Navigation.Models;
using OrganiMedCore.Navigation.Services;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace OrganiMedCore.Navigation.Tests
{
    public class MenuManagerTests
    {
        [Fact]
        public async Task BuildMenuAsync_ShouldBeEmpty()
        {
            var menuManager = new MenuManager(null, Enumerable.Empty<IMenuItemProvider>(), null, null);
            Assert.Empty(await menuManager.BuildMenuAsync(null, "MainMenu"));
        }

        [Fact]
        public async Task BuildMenuAsync_ShouldReturn_WithHrefs()
        {
            var menuA = new[]
            {
                new MenuItem() { Href = "a/1" },
                new MenuItem() { Href = "a/2" },
                new MenuItem() { Href = "a/3" }
            };

            var menuB = new[]
            {
                new MenuItem() { Href = "b/1" },
                new MenuItem() { Href = "b/2" },
                new MenuItem() { Href = "b/3" }
            };

            var menuManager = new MenuManager(
                null,
                new[]
                {
                    new MenuItemProviderMock(menuA, "X"),
                    new MenuItemProviderMock(menuB, "X")
                },
                new ShellSettingsMock(),
                new UrlHelperFactoryMock());

            var menuItems = await menuManager.BuildMenuAsync(null, "X");
            Assert.Contains(menuA, a => menuItems.Any(mi => mi.Href == a.Href));
            Assert.Contains(menuB, b => menuItems.Any(mi => mi.Href == b.Href));
        }

        [Fact]
        public async Task BuildMenuAsync_HashMark()
        {
            var menuA = new[]
            {
                new MenuItem() { Href = "" }
            };

            var menuManager = new MenuManager(
                null,
                new[]
                {
                    new MenuItemProviderMock(menuA, "X")
                },
                new ShellSettingsMock(),
                new UrlHelperFactoryMock());

            var menuItems = await menuManager.BuildMenuAsync(null, "X");
            Assert.Contains(menuItems, x => x.Href == "#");
        }

        [Fact]
        public async Task BuildMenuAsync_AspRouted()
        {
            const string area = "MyModule.Core";
            const string controller = "MyController";
            const string action = "Index";
            const string id = "15";

            var expectedUrl = $"/{area}/{controller}/{action}/{id}";

            var menuA = new[]
            {
                new MenuItem()
                {
                    IsAspRouted = true
                }
            };

            var menuManager = new MenuManager(
                null,
                new[]
                {
                    new MenuItemProviderMock(menuA, "X")
                },
                new ShellSettingsMock(),
                new UrlHelperFactoryMock(expectedUrl));

            var menuItems = await menuManager.BuildMenuAsync(null, "X");

            Assert.Contains(menuItems, m => m.Href == expectedUrl);

        }

        [Theory]
        [InlineData("prefix", "apple", "/prefix/apple")]
        [InlineData("", "apple", "/apple")]
        [InlineData(null, "apple", "/apple")]
        public async Task BuildMenuAsync_AspRouted_WithHttpContext(string prefix, string url, string expectedResult)
        {
            var menuA = new[]
            {
                new MenuItem()
                {
                    Href = $"~/{url}"
                }
            };

            var menuManager = new MenuManager(
                null,
                new[]
                {
                    new MenuItemProviderMock(menuA, "X")
                },
                new ShellSettingsMock(prefix),
                new UrlHelperFactoryMock());

            var menuItems = await menuManager.BuildMenuAsync(new ActionContextMock(), "X");

            Assert.Contains(menuItems, m => m.Href == expectedResult);
        }

        [Fact]
        public async Task BuildMenuAsync_MenuId_ShouldReturnOnlyMenuA()
        {
            var menuA = new[]
            {
                new MenuItem() { Href = "a/1" },
                new MenuItem() { Href = "a/2" },
                new MenuItem() { Href = "a/3" }
            };

            var menuB = new[]
            {
                new MenuItem() { Href = "b/1" },
                new MenuItem() { Href = "b/2" },
                new MenuItem() { Href = "b/3" }
            };

            var menuManager = new MenuManager(
                null,
                new[]
                {
                    new MenuItemProviderMock(menuA, "X"),
                    new MenuItemProviderMock(menuB, "Y")
                },
                new ShellSettingsMock(),
                new UrlHelperFactoryMock());

            var menuItems = await menuManager.BuildMenuAsync(null, "X");
            Assert.Contains(menuA, a => menuItems.Any(mi => mi.Href == a.Href));
            Assert.DoesNotContain(menuB, b => menuItems.Any(mi => mi.Href == b.Href));
        }


        private class MenuItemProviderMock : IMenuItemProvider
        {
            private readonly MenuItem[] _menuItems;


            public string MenuId { get; private set; }


            public MenuItemProviderMock(MenuItem[] menuItems, string menuId)
            {
                _menuItems = menuItems;

                MenuId = menuId;
            }


            public Task BuildMenuAsync(Menu builder, object additionalData = null)
            {
                foreach (var menuItem in _menuItems)
                {
                    builder.Add(menuItem);
                }

                return Task.CompletedTask;
            }
        }

        private class ShellSettingsMock : ShellSettings
        {
            public ShellSettingsMock(string prefix = null)
            {
                RequestUrlPrefix = prefix;
            }
        }

        private class UrlHelperFactoryMock : IUrlHelperFactory
        {
            private readonly string _expectedUrl;


            public UrlHelperFactoryMock(string expectedUrl = null)
            {
                _expectedUrl = expectedUrl;
            }


            public IUrlHelper GetUrlHelper(ActionContext context) => new UrlHelperMock(_expectedUrl);
        }

        private class UrlHelperMock : IUrlHelper
        {
            private readonly string _expectedResult;


            [ExcludeFromCodeCoverage]
            public ActionContext ActionContext => throw new NotImplementedException();


            public UrlHelperMock(string expectedResult)
            {
                _expectedResult = expectedResult;
            }



            public string RouteUrl(UrlRouteContext routeContext)
                => _expectedResult;

            [ExcludeFromCodeCoverage]
            public string Action(UrlActionContext actionContext)
                => throw new NotImplementedException();

            [ExcludeFromCodeCoverage]
            public string Content(string contentPath)
                => throw new NotImplementedException();

            [ExcludeFromCodeCoverage]
            public bool IsLocalUrl(string url)
                => throw new NotImplementedException();

            [ExcludeFromCodeCoverage]
            public string Link(string routeName, object values)
                => throw new NotImplementedException();
        }

        private class ActionContextMock : ActionContext
        {
            public ActionContextMock()
            {
                HttpContext = new HttpContextMock();
            }
        }

        [ExcludeFromCodeCoverage]
        private class HttpContextMock : HttpContext
        {
            public override IFeatureCollection Features => throw new NotImplementedException();

            public override HttpRequest Request => throw new NotImplementedException();

            public override HttpResponse Response => throw new NotImplementedException();

            public override ConnectionInfo Connection => throw new NotImplementedException();

            public override WebSocketManager WebSockets => throw new NotImplementedException();

            public override ClaimsPrincipal User { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

            public override IDictionary<object, object> Items { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

            public override IServiceProvider RequestServices { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

            public override CancellationToken RequestAborted { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

            public override string TraceIdentifier { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

            public override ISession Session { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

            public override void Abort() => throw new NotImplementedException();
        }
    }
}
