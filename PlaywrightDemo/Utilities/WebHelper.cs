using Microsoft.Playwright;
using Serilog;
using System.Threading.Tasks;
using LocatorType = PlaywrightDemo.Utilities.TestConstant.LocatorType;
using WebElementAction = PlaywrightDemo.Utilities.TestConstant.WebElementAction;

namespace PlaywrightDemo.Utilities
{
    public class WebHelper
    {
        private readonly IPage _page;

        public WebHelper(IPage page)
        {
            _page = page;
        }

        public ILocator? IdentifyWebElement(LocatorType locatorType, string? selector)
        {
            try
            {
                Log.Information("Identifying web element with {locatorType} selector: {Selector}", 
                    locatorType.ToString(),selector);
                var locator = locatorType switch
                {
                    LocatorType.Id => GetElementById(selector),
                    LocatorType.Name => GetElementByName(selector),
                    LocatorType.ClassName => GetElementByClass(selector),
                    LocatorType.Text => GetElementByText(selector),
                    LocatorType.CssSelector => GetElementBySelector(selector),
                    LocatorType.XPath => GetElementByXPath(selector),
                    LocatorType.AltText => GetElementByAltText(selector),
                    LocatorType.Placeholder => GetElementByPlaceholder(selector),
                    LocatorType.DataTestId => GetElementByDataTestId(selector),
                    _ => throw new ArgumentOutOfRangeException(nameof(locatorType), locatorType, null)
                };
                return locator;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to locate element with {locatorType} selector: {Selector}", locatorType.ToString(),
                    selector);
                throw;
            }
        }

        public List<ILocator> IdentifyWebElements(LocatorType locatorType, string? selector)
        {
            try
            {
                Log.Information("Identifying web elements with {locatorType} selector: {Selector}", locatorType.ToString(), selector);

                var locator = locatorType switch
                {
                    LocatorType.Id => GetElementById(selector),
                    LocatorType.Name => GetElementByName(selector),
                    LocatorType.ClassName => GetElementByClass(selector),
                    LocatorType.Text => GetElementByText(selector),
                    LocatorType.CssSelector => GetElementBySelector(selector),
                    LocatorType.XPath => GetElementByXPath(selector),
                    LocatorType.AltText => GetElementByAltText(selector),
                    LocatorType.Placeholder => GetElementByPlaceholder(selector),
                    LocatorType.DataTestId => GetElementByDataTestId(selector),
                    _ => throw new ArgumentOutOfRangeException(nameof(locatorType), locatorType, null)
                };

                var locators = new List<ILocator>();
                var count = locator.CountAsync().Result;
                for (int i = 0; i < count; i++)
                {
                    locators.Add(locator.Nth(i));
                }

                return locators;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to locate elements with {locatorType} selector: {Selector}", 
                    locatorType.ToString(), selector);
                throw;
            }
        }

        public async Task PerformWebELementAction(ILocator webElement, 
            WebElementAction webElementAction, string? actionData = null)
        {
            Log.Information("Performing web element action.");
            try
            {
                switch (webElementAction)
                {
                    case WebElementAction.Click:
                        await ClickAsync(webElement);
                        break;
                    case WebElementAction.Hover:
                        await HoverAsync(webElement);
                        break;
                    case WebElementAction.EnterText:
                        if (actionData != null)
                            await EnterTextAsync(webElement, actionData);
                        break;
                    case WebElementAction.ClearAndEnterText:
                        if (actionData != null)
                            await EnterTextAsync(webElement, actionData, false);
                        break;
                    case WebElementAction.Select:
                        if (actionData != null)
                            await SelectOptionAsync(webElement, actionData);
                        break;
                    case WebElementAction.Check:
                        await CheckAsync(webElement);
                        break;
                    case WebElementAction.Uncheck:
                        await UncheckAsync(webElement);
                        break;
                    case WebElementAction.ScrollTo:
                        await ScrollToElementAsync(webElement);
                        break;
                    case WebElementAction.Focus:
                        await FocusOnElementAsync(webElement);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(webElementAction), webElementAction, null);
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An error occurred while performing the web element action: {WebElementAction}",
                    webElementAction);
                throw;
            }
        }

        public async Task PerformKeyboardEvent(string keyName)
        {
            Log.Information("Performing keyboard {KeyName} event.", keyName);
            await _page.Keyboard.DownAsync(keyName);
            Log.Information("Keyboard {KeyName} event performed successfully.", keyName);
        }

        public async Task<string?> GetTextAsync(ILocator element)
        {
            Log.Information("Getting text from element.");
            var text = await element.InnerTextAsync();
            Log.Information("Retrieved text: {Text}", text);
            return text;
        }

        public async Task<string?> GetAttributeAsync(ILocator element, string attributeName)
        {
            Log.Information("Getting attribute: {AttributeName} from element.", attributeName);
            var attributeValue = await element.GetAttributeAsync(attributeName);
            Log.Information("Retrieved attribute value: {AttributeValue}", attributeValue);
            return attributeValue;
        }

        public async Task<bool> IsVisibleAsync(ILocator element)
        {
            Log.Information("Checking if element is visible.");
            var isVisible = await element.IsVisibleAsync();
            Log.Information("Element visibility: {IsVisible}", isVisible);
            return isVisible;
        }

        public async Task<bool> IsEnabledAsync(ILocator element)
        {
            Log.Information("Checking if element is enabled.");
            var isEnabled = await element.IsEnabledAsync();
            Log.Information("Element enabled state: {IsEnabled}", isEnabled);
            return isEnabled;
        }

        public async Task<bool> IsCheckedAsync(ILocator element)
        {
            Log.Information("Checking if element is checked.");
            var isChecked = await element.IsCheckedAsync();
            Log.Information("Element checked state: {IsChecked}", isChecked);
            return isChecked;
        }

        public async Task<string> GetWindowTitle()
        {
            Log.Information("Getting window title.");
            var title = await _page.TitleAsync();
            Log.Information("Window title: {Title}", title);
            return title;
        }

        public async Task<string> ReadAndHandleAlert()
        {
            Log.Information("Reading and handling alert.");
            var alertMessage = string.Empty;
            _page.Dialog += async (_, dialog) =>
            {
                alertMessage = dialog.Message;
                Log.Information("Alert message: {AlertMessage}", alertMessage);
                await dialog.AcceptAsync();
            };
            return alertMessage;
        }

        public async Task WaitForElementToBeVisible(ILocator elementLocator,int timeOut = 10000)
        {
            Log.Information("Waiting for element to be visible.");
            await elementLocator.WaitForAsync(new LocatorWaitForOptions
            {
                State = WaitForSelectorState.Visible,
                Timeout = timeOut
            });
            Log.Information("Element is now visible.");
        }

        public async Task WaitForElementToBeInVisible(ILocator elementLocator, int timeOut = 10000)
        {
            Log.Information("Waiting for element to be invisible.");
            await elementLocator.WaitForAsync(new LocatorWaitForOptions
            {
                State = WaitForSelectorState.Hidden,
                Timeout = timeOut
            });
            Log.Information("Element is now invisible.");
        }

        private ILocator GetElementBySelector(string selector)
        {
            Log.Information("Locating element by selector: {Selector}", selector);
            return _page.Locator(selector);
        }

        private ILocator GetElementByText(string text)
        {
            Log.Information("Locating element by text: {Text}", text);
            return _page.Locator($"text={text}");
        }

        private ILocator GetElementByRole(string role, string? name = null)
        {
            Log.Information("Locating element by role: {Role}, name: {Name}", role, name);
            return _page.Locator($"role={role}{(name != null ? $"[name=\"{name}\"]" : "")}");
        }

        private ILocator GetElementById(string id)
        {
            Log.Information("Locating element by ID: {Id}", id);
            return _page.Locator($"#{id}");
        }

        private ILocator GetElementByClass(string className)
        {
            Log.Information("Locating element by class: {ClassName}", className);
            return _page.Locator($".{className}");
        }

        private ILocator GetElementByXPath(string xpath)
        {
            Log.Information("Locating element by XPath: {XPath}", xpath);
            return _page.Locator($"xpath={xpath}");
        }

        private ILocator GetElementByAltText(string altText)
        {
            Log.Information("Locating element by alt text: {AltText}", altText);
            return _page.Locator($"[alt=\"{altText}\"]");
        }

        private ILocator GetElementByPlaceholder(string placeholder)
        {
            Log.Information("Locating element by placeholder: {Placeholder}", placeholder);
            return _page.Locator($"[placeholder=\"{placeholder}\"]");
        }

        private ILocator GetElementByName(string name)
        {
            Log.Information("Locating element by name: {Name}", name);
            return _page.Locator($"[name=\"{name}\"]");
        }

        private ILocator GetElementByDataTestId(string testId)
        {
            Log.Information("Locating element by data-testid: {TestId}", testId);
            return _page.Locator($"[data-testid=\"{testId}\"]");
        }

        private async Task ClickAsync(ILocator element)
        {
            Log.Information("Clicking on element.");
            await element.ClickAsync();
            Log.Information("Clicked on element successfully.");
        }

        private async Task HoverAsync(ILocator element)
        {
            Log.Information("Hovering over element.");
            await element.HoverAsync();
            Log.Information("Hovered over element successfully.");
        }

        private async Task EnterTextAsync(ILocator element, string text, bool clearBeforeTyping = true)
        {
            Log.Information("Entering text: {Text} into element. Clear before typing: {ClearBeforeTyping}",
                text, clearBeforeTyping);
            if (clearBeforeTyping)
            {
                await element.FillAsync(text);
            }
            else
            {
                await element.EvaluateAsync("(el, value) => el.value += value", text);
            }
            Log.Information("Entered text successfully.");
        }

        private async Task SelectOptionAsync(ILocator element, string value)
        {
            Log.Information("Selecting option: {Value} in element.", value);
            await element.SelectOptionAsync(value);
            Log.Information("Option selected successfully.");
        }

        private async Task CheckAsync(ILocator element)
        {
            Log.Information("Checking element.");
            await element.CheckAsync();
            Log.Information("Element checked successfully.");
        }

        private async Task UncheckAsync(ILocator element)
        {
            Log.Information("Unchecking element.");
            await element.UncheckAsync();
            Log.Information("Element unchecked successfully.");
        }

        private async Task ScrollToElementAsync(ILocator element)
        {
            Log.Information("Scrolling to element.");
            await element.ScrollIntoViewIfNeededAsync();
            Log.Information("Scrolled to element successfully.");
        }

        private async Task FocusOnElementAsync(ILocator element)
        {
            Log.Information("Focusing on element.");
            await element.FocusAsync();
            Log.Information("Focused on element successfully.");
        }
    }
}
