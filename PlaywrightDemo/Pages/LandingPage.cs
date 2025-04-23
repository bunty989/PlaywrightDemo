using Microsoft.Playwright;
using PlaywrightDemo.Utilities;
using LocatorType = PlaywrightDemo.Utilities.TestConstant.LocatorType;
using WebElementAction = PlaywrightDemo.Utilities.TestConstant.WebElementAction;

namespace PlaywrightDemo.Pages
{
    internal class LandingPage
    {
        private readonly WebHelper _webHelper;
        private ILocator Spinner => _webHelper.IdentifyWebElements(LocatorType.CssSelector, ".loader div svg")[0];
        private ILocator Products => _webHelper.IdentifyWebElement(LocatorType.CssSelector, "#our_products");
        private ILocator UserBtn => _webHelper.IdentifyWebElement(LocatorType.CssSelector, "#menuUserLink"); 
        private ILocator CreateNewAccount => _webHelper.IdentifyWebElement(LocatorType.CssSelector, "[class='login ng-scope'] [translate='CREATE_NEW_ACCOUNT']");

        public LandingPage(IPage page)
        {
            _webHelper = new WebHelper(page);
        }

        public async Task WaitForLoadingSpinnerToDisappear()
        {
            await _webHelper.WaitForElementToBeInVisible(Spinner);
        }

        public async Task<bool> LandingPageIsDisplayed()
        {
            return await _webHelper.IsVisibleAsync(Products);
        }

        public async Task ClickUserBtn()
        {
            await _webHelper.PerformWebELementAction(UserBtn, WebElementAction.Click);
        }

        public async Task ClickCreateNewAccount()
        {
            await _webHelper.PerformWebELementAction(CreateNewAccount, WebElementAction.Click);
        }
    }
}
