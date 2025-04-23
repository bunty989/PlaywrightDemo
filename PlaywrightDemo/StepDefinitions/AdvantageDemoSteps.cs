using Microsoft.Playwright;
using PlaywrightDemo.Pages;
using PlaywrightDemo.Utilities;
using Reqnroll;

namespace PlaywrightDemo.StepDefinitions
{
    [Binding]
    public class AdvantageDemoSteps
    {
        private readonly LandingPage? _landingPage;
        private readonly CreateAccountPage? _createAccountPage;
        private readonly ScenarioContext _scenarioContext;
        private readonly IPage? _page;
        private List<string> errorLabels = [];
        private string[] _errorLabel;

        public AdvantageDemoSteps(ScenarioContext scenarioContext)
        {
            _scenarioContext = scenarioContext;
            _page = (IPage)_scenarioContext["Page"];
            _landingPage = new LandingPage(_page);
            _createAccountPage = new CreateAccountPage(_page);
        }


        [Given("I navigate to the landing page of the app")]
        public async Task GivenINavigateToTheLandingPageOfTheApp()
        {
            await _page?.GotoAsync(ConfigHelper.ReadConfigValue
                    (TestConstant.ConfigTypes.AppConfig, TestConstant.ConfigTypesKey.AppUrl));
        }

        [When("I see the page is loaded")]
        public async Task WhenISeeThePageIsLoaded()
        {
            await _landingPage?.WaitForLoadingSpinnerToDisappear();
            await _landingPage?.LandingPageIsDisplayed();  
        }

        [When("I click the user button to create new user")]
        public async Task WhenIClickTheUserButtonToCreateNewUser()
        {
            await _landingPage?.ClickUserBtn();
            await _landingPage?.ClickCreateNewAccount();
            await _createAccountPage?.WaitForHeaderTextToBeDisplayed();
        }

        [When("I dont enter anything to username and email fields")]
        public async Task WhenIDontEnterAnythingToUsernameAndEmailFields()
        {
           errorLabels?.Add(await _createAccountPage?.GetUserNameErrorLabel());
           errorLabels?.Add(await _createAccountPage?.GetEmailErrorLabel());
        }

        [When("I dont enter anything to password and confirm password fields")]
        public async Task WhenIDontEnterAnythingToPasswordAndConfirmPasswordFields()
        {
            errorLabels.Add(await _createAccountPage?.GetPasswordErrorLabel());
            errorLabels.Add(await _createAccountPage?.GetConfirmPasswordErrorLabel());
        }

        [Then("I see the {string} error message is displayed")]
        public void ThenISeeTheErrorMessageIsDisplayed(string errorMessage)
        {
            _errorLabel = errorLabels.ToArray();
            var errorKey = errorMessage.Split(" ")[0];
            var selectedError = _errorLabel.FirstOrDefault(label => label?.Contains(errorKey) == true);
            Assert.That(selectedError, Is.EqualTo(errorMessage));
        }

        [When("I enter {string} to username field")]
        public async Task WhenIEnterToUsernameField(string userName)
        {
            await _createAccountPage?.EnterUserName(userName);
        }

        [When("I enter {string} to email field")]
        public async Task WhenIEnterToEmailField(string email)
        {
            await _createAccountPage?.EnterEmail(email);
        }

        [When("I enter {string} to password field")]
        public async Task WhenIEnterToPasswordField(string password)
        {
            await _createAccountPage?.EnterPassword(password);
        }

        [When("I enter {string} to confirm password field")]
        public async Task WhenIEnterToConfirmPasswordField(string confirmPassword)
        {
            await _createAccountPage?.EnterConfirmPassword(confirmPassword);
            _errorLabel = await GetErrorLabels();
        }

        [Then("I dont see any error message for {string} field")]
        public void ThenIDontSeeAnyErrorMessageForField(string fieldName)
        {
            var selectedError = _errorLabel.FirstOrDefault(label => label?.ToLowerInvariant().Contains(fieldName) == true);
            Assert.That(selectedError.ToLowerInvariant(), Is.EqualTo(fieldName.ToLowerInvariant()));
        }

        private async Task<string[]> GetErrorLabels()
        {
            errorLabels.Add(await _createAccountPage?.GetUserNameErrorLabel());
            errorLabels.Add(await _createAccountPage?.GetEmailErrorLabel());
            errorLabels.Add(await _createAccountPage?.GetPasswordErrorLabel());
            errorLabels.Add(await _createAccountPage?.GetConfirmPasswordErrorLabel());
            return errorLabels.ToArray();

        }
    }
}
