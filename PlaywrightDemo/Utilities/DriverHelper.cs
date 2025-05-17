using Microsoft.Playwright;
using System.Collections.Concurrent;
using BrowserType = PlaywrightDemo.Utilities.TestConstant.BrowserType;

namespace PlaywrightDemo.Utilities
{
    public static class DriverHelper
    {
        private static readonly ConcurrentDictionary<string, IBrowser> BrowserInstances = new();
        private static readonly ConcurrentDictionary<string, IBrowserContext> ContextInstances = new();
        private static readonly ConcurrentDictionary<string, IPage> PageInstances = new();
        private static readonly ConcurrentDictionary<string, IPlaywright> PlaywrightInstances = new();

        public static async Task<IPage> InitializeDriverAsync(string scenarioId)
        {
            var playwright = await Playwright.CreateAsync();
            var browser = ConfigHelper.ReadConfigValue(TestConstant.ConfigTypes.WebDriverConfig, 
                TestConstant.ConfigTypesKey.Browser);
            var Browser = Enum.Parse<BrowserType>(browser, true) switch
                //browser?.ToLowerInvariant() switch
            {
                BrowserType.Chrome =>
                    await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
                    {
                        Headless = false,
                        Args = ["--start-maximized", "--disable-gpu", "--no-sandbox"]
                    }),
                BrowserType.Firefox =>
                    await playwright.Firefox.LaunchAsync(new BrowserTypeLaunchOptions
                    {
                        Headless = false
                    }),
                BrowserType.Edge =>
                    await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
                    {
                        Channel = "msedge",
                        Headless = false,
                        Args = ["--start-maximized", "--disable-gpu", "--no-sandbox"]
                    }),
                BrowserType.ChromeHeadless =>
                    await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
                    {
                        Headless = true,
                        Args = ["--disable-gpu", "--no-sandbox"]
                    }),
                BrowserType.ChromeIncognito =>
                    await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
                    {
                        Headless = false,
                        Args = ["--start-maximized", "--disable-gpu", "--no-sandbox", "--incognito"]
                    }),
                BrowserType.FirefoxHeadless =>
                    await playwright.Firefox.LaunchAsync(new BrowserTypeLaunchOptions
                    {
                        Headless = true
                    }),
                BrowserType.EdgeHeadless =>
                    await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
                    {
                        Channel = "msedge",
                        Headless = true,
                        Args = ["--disable-gpu", "--no-sandbox"]
                    }),
                BrowserType.Webkit =>
                    await playwright.Webkit.LaunchAsync(new BrowserTypeLaunchOptions
                    {
                        Headless = false

                    }),
                BrowserType.WebkitHeadless =>
                    await playwright.Webkit.LaunchAsync(new BrowserTypeLaunchOptions
                    {
                        Headless = true
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
        
        public static async Task InitializeAPIDriverAsync(string scenarioId)
        {
            var playwright = await Playwright.CreateAsync();
            PlaywrightInstances[scenarioId] = playwright;
        }

        public static async Task QuitDriverAsync(string scenarioId)
        {
            if (PlaywrightInstances.TryRemove(scenarioId, out _))
            {
                PlaywrightInstances.Clear();
            }
            if (ContextInstances.TryRemove(scenarioId, out var context))
            {
                await context.CloseAsync();
                await context.DisposeAsync();
                context = null;
            }

            if (BrowserInstances.TryRemove(scenarioId, out var browser))
            {
                await browser.CloseAsync();
                await browser.DisposeAsync();
                browser = null;
            }
            PageInstances.TryRemove(scenarioId, out _);
        }

        public static IPlaywright? GetPlaywright(string scenarioId)
        {
            return PlaywrightInstances.TryGetValue(scenarioId, out var playwright) ? playwright : null;
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
