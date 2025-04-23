@Retry
Feature: Login

Scenario: User navigates to the homepage
    Given the browser is launched
    When the user navigates to "https://www.advantageonlineshopping.com/"
    Then the homepage should be displayed


Scenario: User logs in
	Given the browser is launched
	When the user navigates to "https://www.advantageonlineshopping.com/"
	And the user clicks on the "User" button
	And the user clicks on the "CreateNewUSer" button
	Then the user should be shown the new user creation page