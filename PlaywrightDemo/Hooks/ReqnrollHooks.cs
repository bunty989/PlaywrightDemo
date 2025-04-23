using AventStack.ExtentReports;
using Microsoft.Playwright;
using PlaywrightDemo.Utilities;
using Reqnroll;
using AventStack.ExtentReports.Gherkin.Model;
using AventStack.ExtentReports.Reporter;
using Serilog.Core;
using Serilog;
using Serilog.Events;
using TestConstant = PlaywrightDemo.Utilities.TestConstant;
using Log = Serilog.Log;

namespace PlaywrightDemo.Hooks
{
    [Binding]
    public sealed class ReqnrollHooks
    {
        private static ExtentReports? _extent;
        private static string BrowserType => ConfigHelper.ReadConfigValue(
            TestConstant.ConfigTypes.WebDriverConfig, 
            TestConstant.ConfigTypesKey.Browser);
        private readonly ScenarioContext? _scenarioContext;
        private readonly FeatureContext? _featureContext;
        private static string? BrowserVersion;
        private static bool addSystemInfo = false;

        public ReqnrollHooks(ScenarioContext scenarioContext,
            FeatureContext featureContext)
        {
            _scenarioContext = scenarioContext;
            _featureContext = featureContext;
        }


        [BeforeFeature]
        public static async Task BeforeFeature(FeatureContext featureContext)
        {
            var featureNode = _extent?.CreateTest<Feature>(featureContext.FeatureInfo.Title)
                ?? throw new InvalidOperationException("Failed to create feature node in Extent report.");
            featureContext["FeatureNode"] = featureNode;
            Log.Information("Selecting feature file {0} to run", featureContext.FeatureInfo.Title);
            await Task.CompletedTask;
        }

        [BeforeScenario]
        public async Task BeforeScenario(ScenarioContext scenarioContext)
        {
            var featureNode = (ExtentTest)_featureContext["FeatureNode"];
            var scenarioName = scenarioContext.ScenarioInfo.Title;
            if (scenarioContext.ScenarioInfo.Arguments?.Count > 0)
            {
                scenarioName = scenarioName +
                               "{" +
                               scenarioContext.ScenarioInfo.Arguments.Keys.OfType<string>()
                                   .Skip(0)
                                   .First() +
                               ", " +
                               (string?)scenarioContext.ScenarioInfo.Arguments[0] +
                               "}";
            }
            var scenarioNode = featureNode.CreateNode<Scenario>(scenarioName);
            scenarioContext["ScenarioNode"] = scenarioNode;
            Log.Information("Selecting Scenario {0} to run", scenarioName);
            var page = await DriverHelper.InitializeDriverAsync(scenarioName);
            scenarioContext["Page"] = page;
            BrowserVersion = BrowserVersion ?? GetBrowserVersion(scenarioName);
            await Task.CompletedTask;
        }

        [BeforeTestRun]
        public static async Task BeforeTestRun()
        {
            var rootDirectory = AppContext.BaseDirectory.Split(["bin"], StringSplitOptions.None)[0];
            var logsDirectory = Path.Combine(rootDirectory, TestConstant.PathVariables.HtmlReportFolder);
            Directory.CreateDirectory(logsDirectory);

            var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            var _executionFolderPath = Path.Combine(logsDirectory, timestamp);
            Directory.CreateDirectory(_executionFolderPath);
            var levelSwitch = new LoggingLevelSwitch(GetLogLevel());
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.ControlledBy(levelSwitch)
                .WriteTo.File(_executionFolderPath + TestConstant.PathVariables.LogName,
                    outputTemplate: "{Timestamp: yyyy-MM-dd HH:mm:ss.fff} | {Level:u3} | {Message} | {NewLine}",
                    rollingInterval: RollingInterval.Day).CreateLogger();
            var htmlReport = new ExtentSparkReporter(Path.Combine(_executionFolderPath, "ExtentReport.html"));
            htmlReport.LoadJSONConfig(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, TestConstant.PathVariables.ExtentConfigName));
            _extent = new ExtentReports();
            Dictionary<string, string?> sysInfo = new()
            {
                { "Host Name", Environment.MachineName },
                { "Domain", Environment.UserDomainName },
                { "Username", Environment.UserName },
                { "OS Version", GetOSNameAndVersion() },
                {"Browser Name", BrowserType},
            };
            foreach (var (key, value) in sysInfo) { _extent.AddSystemInfo(key, value); }
            _extent.AttachReporter(htmlReport);
            Log.Information("ExtentReports initialized successfully.");
            await Task.CompletedTask;
        }


        [BeforeStep]
        public void BeforeStep()
        {
            var scenarioNode = (ExtentTest)_scenarioContext["ScenarioNode"];
            _scenarioContext["StepNode"] = scenarioNode;
        }

        [AfterStep]
        public void AfterStep()
        {
            var stepNode = (ExtentTest)_scenarioContext["StepNode"];
            var stepType = _scenarioContext.StepContext.StepInfo.StepDefinitionType + " ";
            var stepStatus = _scenarioContext.StepContext.Status;
            var screenshotBase64 = Convert.ToBase64String(GetPageScreenshot(_scenarioContext.ScenarioInfo.Title));
            var mediaEntity = MediaEntityBuilder
                .CreateScreenCaptureFromBase64String(screenshotBase64, null).Build();
            switch (stepStatus)
            {
                case ScenarioExecutionStatus.OK:
                {
                    switch (stepType.ToUpper().Trim())
                    {
                        case "GIVEN":
                        {
                            stepNode.CreateNode<Given>(_scenarioContext.StepContext.StepInfo.Text).Pass(mediaEntity);
                            break;
                        }
                        case "WHEN":
                        {
                            stepNode.CreateNode<When>(_scenarioContext.StepContext.StepInfo.Text).Pass(mediaEntity);
                            break;
                        }
                        case "THEN":
                        {
                            stepNode.CreateNode<Then>(_scenarioContext.StepContext.StepInfo.Text).Pass(mediaEntity);
                            break;
                        }
                        case "AND":
                        {
                            stepNode.CreateNode<And>(_scenarioContext.StepContext.StepInfo.Text).Pass(mediaEntity);
                            break;
                        }
                    }
                    break;
                }
                case ScenarioExecutionStatus.TestError:
                {
                    switch (stepType.ToUpper().Trim())
                    {
                        case "GIVEN":
                        {
                            stepNode.CreateNode<Given>(_scenarioContext.StepContext.StepInfo.Text).Fail(_scenarioContext.TestError.Message, mediaEntity);
                            break;
                        }
                        case "WHEN":
                        {
                            stepNode.CreateNode<When>(_scenarioContext.StepContext.StepInfo.Text).Fail(_scenarioContext.TestError.Message, mediaEntity);
                            break;
                        }
                        case "THEN":
                        {
                            stepNode.CreateNode<Then>(_scenarioContext.StepContext.StepInfo.Text).Fail(_scenarioContext.TestError.Message, mediaEntity);
                            break;
                        }
                        case "AND":
                        {
                            stepNode.CreateNode<And>(_scenarioContext.StepContext.StepInfo.Text).Fail(_scenarioContext.TestError.Message, mediaEntity);
                            break;
                        }
                    }
                    break;
                }
            }
        }

        [AfterFeature]
        public static void AfterFeature(FeatureContext featureContext)
        {
            if (!addSystemInfo)
            {
                _extent?.AddSystemInfo("Browser Version", BrowserVersion);
                addSystemInfo = true;
            }
            _extent?.Flush();
            Log.Information("Ending feature file {0} execution", featureContext?.FeatureInfo.Title);
        }

        [AfterScenario]
        public async Task AfterScenario()
        {
            var scenarioId = _scenarioContext?.ScenarioInfo.Title;
            await DriverHelper.QuitDriverAsync(scenarioId);
            Log.Debug("Ending Scenario {0} execution", scenarioId);
        }

        [AfterTestRun]
        public static async Task AfterTestRun()
        {
            Log.CloseAndFlush();
            await Task.CompletedTask;
        }

        private static LogEventLevel GetLogLevel()
        {
            return ConfigHelper.ReadConfigValue("", TestConstant.LoggerLevel.LogLevel)?.ToLower() switch
            {
                "all" => LogEventLevel.Verbose,
                "info" => LogEventLevel.Information,
                "warning" => LogEventLevel.Warning,
                "error" => LogEventLevel.Error,
                "debug" => LogEventLevel.Debug,
                _ => LogEventLevel.Debug
            };
        }

        private static byte[] GetPageScreenshot(string scenarioId)
        {
            if (DriverHelper.GetPage(scenarioId) != null)
            {
                return DriverHelper.GetPage(scenarioId).ScreenshotAsync(new PageScreenshotOptions
                {
                    FullPage = true
                }).Result;
            }
            else
            {
                throw new InvalidOperationException("Current page is not available.");
            }
        }

        private static string GetOSNameAndVersion()
        {
            if (OperatingSystem.IsWindows())
            {
                return Environment.OSVersion.Version.Build >= 22000 ? "Windows 11" : "Windows";
            }
            if (OperatingSystem.IsLinux())
            {
                try
                {
                    var lines = File.ReadAllLines("/etc/os-release");
                    var name = lines.FirstOrDefault(l => l.StartsWith("NAME="))?.Split('=')[1].Trim('"');
                    return name ?? "Linux";
                }
                catch
                {
                    return "Linux";
                }
            }
            if (OperatingSystem.IsMacOS())
            {
                return "MacOS";
            }
            return "Unknown OS";
        }

        private static string GetBrowserVersion(string scenarioId)
        {
            if (DriverHelper.GetBrowser(scenarioId) != null)
            {
                return DriverHelper.GetBrowser(scenarioId).Version;
            }
            else
            {
                throw new InvalidOperationException("Current browser is not available.");
            }
        }
    }
}