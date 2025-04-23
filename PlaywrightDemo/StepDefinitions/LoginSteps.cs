using Microsoft.Playwright;
using PlaywrightDemo.Utilities;
using Reqnroll;
using LocatorType = PlaywrightDemo.Utilities.TestConstant.LocatorType;
using WebElementAction = PlaywrightDemo.Utilities.TestConstant.WebElementAction;

namespace PlaywrightDemo.StepDefinitions
{
    [Binding]
    public class LoginSteps
    {
        private readonly ScenarioContext _scenarioContext;
        private readonly WebHelper _webHelper;
        private readonly IPage? _page;

        public LoginSteps(ScenarioContext scenarioContext)
        {
            _scenarioContext = scenarioContext;
            _page = (IPage)_scenarioContext["Page"];
            _webHelper = new WebHelper(_page);
        }

        [Given(@"the browser is launched")]
        public void GivenTheBrowserIsLaunched()
        {
            if (_page == null)
                throw new InvalidOperationException("The browser page is not initialized. Ensure ReqnrollHooks.BeforeScenario is executed.");
        }

        [When(@"the user navigates to ""(.*)""")]
        public async Task WhenTheUserNavigatesTo(string url)
        {
            await _page.GotoAsync(url);
        }

        [Then(@"the homepage should be displayed")]
        public async Task ThenTheHomepageShouldBeDisplayed()
        {
            var title = await _page.TitleAsync();
            Assert.That(title != null, "The homepage title should not be null.");
        }

        [When("the user clicks on the {string} button")]
        public async Task WhenTheUserClicksOnTheButton(string buttonName)
        {
            var spinner = _webHelper.IdentifyWebElement(LocatorType.CssSelector, ".loader div svg").First;
            await spinner.WaitForAsync(new LocatorWaitForOptions
            {
                State = WaitForSelectorState.Hidden,
                Timeout = 10000
            });

            if (buttonName.Equals("User"))
            {
                var userBtn = _webHelper.IdentifyWebElement(LocatorType.CssSelector, "#menuUserLink");
                await _webHelper.PerformWebELementAction(userBtn, WebElementAction.Click);
            }
            else
            {
                var createNewUserBtn = _webHelper.IdentifyWebElement(LocatorType.CssSelector, "[class='login ng-scope'] [translate='CREATE_NEW_ACCOUNT']");
                await _webHelper.PerformWebELementAction(createNewUserBtn, WebElementAction.Click);
            }
        }

        [Then("the user should be shown the new user creation page")]
        public async Task ThenTheUserShouldBeShownTheNewUserCreationPage()
        {
            var header = _webHelper.IdentifyWebElement(LocatorType.CssSelector, "#registerPage h3[class^='robo']");
            var headerText = await _webHelper.GetTextAsync(header);
            Assert.That(headerText, Is.EqualTo("CREATE ACCOUNT"));
        }
    }
}
