
namespace PlaywrightDemo.Utilities
{
    public class TestConstant
    {
        public static class ConfigTypes
        {
            public const string WebDriverConfig = "webDriverConfig:";
            public const string AppConfig = "AppConfig:";
            public const string APIConfig = "APIConfig:";
        }

        public static class ConfigTypesKey
        {
            public const string Browser = "Browser";
            public const string PageLoadTimeOut = "PageLoadTimeOut";
            public const string ImplicitWaitTimeout = "ImplicitWaitTimeout";
            public const string ObjectIdentificationTimeOut = "ObjectIdentificationTimeOut";
            public const string Protocol = "Protocol";
            public const string AppUrl = "Url";
            public const string BaseUrl = "BaseUrl";
            public const string TimeOut = "TimeOut";
            public const string AppId = "AppId";
        }

        public static class PathVariables
        {
            public static string? GetBaseDirectory = Directory.GetParent(@"../../../")?.FullName;
            public static string? ReportPath = Path.Combine(GetBaseDirectory ?? throw new DirectoryNotFoundException());
            public const string HtmlReportFolder = "Logs";
            public const string ConfigFileName = "appsettings.json";
            public const string LogName = @"\Log";
            public const string ExtentConfigName = "ExtentConfig.json";
            public static string GetWeather200Schema = Path.Combine(AppDomain.CurrentDomain.BaseDirectory,
                                                        "ApiModels/GetWeather/Schema/ResponseSchema200.json");
            public static string GetWeather401Schema = Path.Combine(AppDomain.CurrentDomain.BaseDirectory,
                                                        "ApiModels/GetWeather/Schema/ResponseSchema401.json");
            public static string GetWeatherById200Schema = Path.Combine(AppDomain.CurrentDomain.BaseDirectory,
                                                        "ApiModels/GetWeatherById/Schema/ResponseSchema200.json");
            public static string GetWeatherById401Schema = Path.Combine(AppDomain.CurrentDomain.BaseDirectory,
                                                        "ApiModels/GetWeatherById/Schema/ResponseSchema401.json");
        }

        public static class LoggerLevel
        {
            public const string LogLevel = "LogLevel";
        }

        public enum BrowserType
        {
            Chrome,
            Firefox,
            InternetExplorer,
            Edge,
            ChromeHeadless,
            ChromeIncognito
        }

        public enum LocatorType
        {
            Id,
            Name,
            ClassName,
            Text,
            CssSelector,
            XPath,
            AltText,
            DataTestId,
            Placeholder,
        }

        public enum WebElementAction
        {
            Click,
            EnterText,
            ClearAndEnterText,
            Select,
            Check,
            Uncheck,
            Hover,
            Press,
            Clear,
            Focus,
            ScrollTo,
        }
    }
}
