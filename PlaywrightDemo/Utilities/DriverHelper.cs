using Microsoft.Playwright;
using System.Collections.Concurrent;

namespace PlaywrightDemo.Utilities
{
    public static class DriverHelper
    {
        private static readonly ConcurrentDictionary<string, IBrowser> BrowserInstances = new();
        private static readonly ConcurrentDictionary<string, IBrowserContext> ContextInstances = new();
        private static readonly ConcurrentDictionary<string, IPage> PageInstances = new();

        public static async Task<IPage> InitializeDriverAsync(string scenarioId)
        {
            var playwright = await Playwright.CreateAsync();
            var browser = ConfigHelper.ReadConfigValue(TestConstant.ConfigTypes.WebDriverConfig, 
                TestConstant.ConfigTypesKey.Browser);
            var Browser = browser?.ToLowerInvariant() switch
            {
                "chrome" =>
                    await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
                    {
                        Headless = false,
                        Args = ["--start-maximized", "--disable-gpu", "--no-sandbox"]
                    }),
                "firefox" =>
                    await playwright.Firefox.LaunchAsync(new BrowserTypeLaunchOptions
                    {
                        Headless = false
                    }),
                "edge" =>
                    await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
                    {
                        Channel = "msedge",
                        Headless = false,
                        Args = ["--start-maximized", "--disable-gpu", "--no-sandbox"]
                    }),
                "chromeheadless" =>
                    await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
                    {
                        Headless = true,
                        Args = ["--disable-gpu", "--no-sandbox"]
                    }),
                _ =>
                    await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
                    {
                        Headless = false
                    })
            };

            var context = await Browser.NewContextAsync(new BrowserNewContextOptions
            {
                ViewportSize = null
            });
            var page = await context.NewPageAsync();
            page.SetDefaultTimeout((float)(int.Parse(ConfigHelper.ReadConfigValue(
            TestConstant.ConfigTypes.WebDriverConfig, TestConstant.ConfigTypesKey.ImplicitWaitTimeout) ?? "0") * 1000));
            page.SetDefaultNavigationTimeout((float)(int.Parse(ConfigHelper.ReadConfigValue(
                TestConstant.ConfigTypes.WebDriverConfig, TestConstant.ConfigTypesKey.PageLoadTimeOut) ?? "0") * 1000));
            page?.SetViewportSizeAsync(1920,1200);
            BrowserInstances[scenarioId] = Browser;
            ContextInstances[scenarioId] = context;
            PageInstances[scenarioId] = page;
            return page;
        }
        

        public static async Task QuitDriverAsync(string scenarioId)
        {
            if (ContextInstances.TryRemove(scenarioId, out var context))
            {
                await context.CloseAsync();
            }

            if (BrowserInstances.TryRemove(scenarioId, out var browser))
            {
                await browser.CloseAsync();
            }
            PageInstances.TryRemove(scenarioId, out _);
        }

        public static IPage? GetPage(string scenarioId)
        {
            return PageInstances.TryGetValue(scenarioId, out var page) ? page : null;
        }

        public static IBrowser? GetBrowser(string scenarioId)
        {
            return BrowserInstances.TryGetValue(scenarioId, out var browser) ? browser : null;
        }
    }
}
