using Microsoft.Playwright;
using PlaywrightDemo.Utilities;
using LocatorType = PlaywrightDemo.Utilities.TestConstant.LocatorType;
using WebElementAction = PlaywrightDemo.Utilities.TestConstant.WebElementAction;

namespace PlaywrightDemo.Pages
{
    internal class CreateAccountPage
    {
        private readonly WebHelper _webHelper;
        private ILocator HeaderText => 
            _webHelper.IdentifyWebElement(LocatorType.CssSelector, "#registerPage h3[class^='robo']");
        private ILocator UserNameTxtBox => 
            _webHelper.IdentifyWebElement(LocatorType.CssSelector, "[name='usernameRegisterPage']");
        private ILocator UserNameErrorLabel => 
            _webHelper.IdentifyWebElement(LocatorType.CssSelector, "[name='usernameRegisterPage'] +label");
        private ILocator EmailTxtBox => 
            _webHelper.IdentifyWebElement(LocatorType.CssSelector, "[name='emailRegisterPage']");
        private ILocator EmailErrorLabel => 
            _webHelper.IdentifyWebElement(LocatorType.CssSelector, "[name='emailRegisterPage'] +label");
        private ILocator PasswordTxtBox => 
            _webHelper.IdentifyWebElement(LocatorType.CssSelector, "[name='passwordRegisterPage']");
        private ILocator PasswordErrorLabel => 
            _webHelper.IdentifyWebElement(LocatorType.CssSelector, "[name='passwordRegisterPage'] +label");
        private ILocator ConfirmPasswordTxtBox => 
            _webHelper.IdentifyWebElement(LocatorType.CssSelector, "[name='confirm_passwordRegisterPage']");
        private ILocator ConfirmPasswordErrorLabel => 
            _webHelper.IdentifyWebElement(LocatorType.CssSelector, "[name='confirm_passwordRegisterPage'] +label");

        public CreateAccountPage(IPage page)
        {
            _webHelper = new WebHelper(page);
        }

        public async Task WaitForHeaderTextToBeDisplayed()
        {
            await _webHelper.WaitForElementToBeVisible(HeaderText);
        }

        public async Task EnterUserName(string userName)
        {
            await _webHelper.PerformWebELementAction(UserNameTxtBox, WebElementAction.EnterText, userName);
        }

        public async Task EnterEmail(string email)
        {
            await _webHelper.PerformWebELementAction(EmailTxtBox, WebElementAction.EnterText, email);
        }

        public async Task EnterPassword(string password)
        {
            await _webHelper.PerformWebELementAction(PasswordTxtBox, WebElementAction.EnterText, password);
        }

        public async Task EnterConfirmPassword(string confirmPassword)
        {
            await _webHelper.PerformWebELementAction(ConfirmPasswordTxtBox, WebElementAction.EnterText, confirmPassword);
        }

        public async Task<string> GetUserNameErrorLabel()
        {
            await _webHelper.PerformWebELementAction(UserNameTxtBox,WebElementAction.Click);
            await _webHelper.PerformKeyboardEvent("Tab");
            return await _webHelper.GetTextAsync(UserNameErrorLabel);
        }

        public async Task<string> GetEmailErrorLabel()
        {
            await _webHelper.PerformWebELementAction(EmailTxtBox, WebElementAction.Click);
            await _webHelper.PerformKeyboardEvent("Tab");
            return await _webHelper.GetTextAsync(EmailErrorLabel);
        }

        public async Task<string> GetPasswordErrorLabel()
        {
            await _webHelper.PerformWebELementAction(PasswordTxtBox, WebElementAction.Click);
            await _webHelper.PerformKeyboardEvent("Tab");
            return await _webHelper.GetTextAsync(PasswordErrorLabel);
        }

        public async Task<string> GetConfirmPasswordErrorLabel()
        {
            await _webHelper.PerformWebELementAction(ConfirmPasswordTxtBox, WebElementAction.Click);
            await _webHelper.PerformKeyboardEvent("Tab");
            return await _webHelper.GetTextAsync(ConfirmPasswordErrorLabel);
        }
    }
}
