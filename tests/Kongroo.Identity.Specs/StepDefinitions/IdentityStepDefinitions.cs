using System.Net;
using System.Text.Json;
using Kongroo.Identity.Domain;
using Kongroo.Identity.Specs.Drivers;
using Kongroo.Identity.Specs.Support;
using Reqnroll;
using Shouldly;

namespace Kongroo.Identity.Specs.StepDefinitions;

[Binding]
public sealed class IdentityStepDefinitions(IdentityApiDriver identityApiDriver, ApiScenarioContext scenarioContext)
{
    [Given("a user exists with username {string}, email {string}, password {string}, and name {string}")]
    public async Task GivenAUserExistsWithUsernameEmailPasswordAndName(
        string username,
        string email,
        string password,
        string name
    )
    {
        await identityApiDriver.RegisterAsync(username, email, password, name);
        scenarioContext.LastResponse.ShouldNotBeNull().StatusCode.ShouldBe(HttpStatusCode.Created);
    }

    [When("the visitor registers with username {string}, email {string}, password {string}, and name {string}")]
    public async Task WhenTheVisitorRegistersWithUsernameEmailPasswordAndName(
        string username,
        string email,
        string password,
        string name
    ) => await identityApiDriver.RegisterAsync(username, email, password, name);

    [Then("the registration response should be created")]
    public void ThenTheRegistrationResponseShouldBeCreated()
    {
        scenarioContext.LastResponse.ShouldNotBeNull().StatusCode.ShouldBe(HttpStatusCode.Created);
        scenarioContext.RegistrationResponse.ShouldNotBeNull().Id.ShouldNotBe(Guid.Empty);
    }

    [Then("the registered user profile should contain username {string}, email {string}, and name {string}")]
    public void ThenTheRegisteredUserProfileShouldContainUsernameEmailAndName(
        string username,
        string email,
        string name
    )
    {
        var response = scenarioContext.RegistrationResponse.ShouldNotBeNull();
        response.Username.ShouldBe(username);
        response.Email.ShouldBe(email);
        response.Name.ShouldBe(name);
    }

    [Then("the response should be bad request")]
    public void ThenTheResponseShouldBeBadRequest() =>
        scenarioContext.LastResponse.ShouldNotBeNull().StatusCode.ShouldBe(HttpStatusCode.BadRequest);

    [Then("the response should contain problem details")]
    public void ThenTheResponseShouldContainProblemDetails()
    {
        scenarioContext.LastResponseContent.ShouldNotBeNullOrWhiteSpace();

        using var document = JsonDocument.Parse(scenarioContext.LastResponseContent);
        document.RootElement.TryGetProperty("title", out _).ShouldBeTrue();
        document.RootElement.TryGetProperty("status", out _).ShouldBeTrue();
    }

    [Then("the response should be conflict")]
    public void ThenTheResponseShouldBeConflict() =>
        scenarioContext.LastResponse.ShouldNotBeNull().StatusCode.ShouldBe(HttpStatusCode.Conflict);

    [Given("the user is logged in with username {string} and password {string}")]
    public async Task GivenTheUserIsLoggedInWithUsernameAndPassword(string username, string password)
    {
        await identityApiDriver.LoginAsync(username, password);
        scenarioContext.LastResponse.ShouldNotBeNull().StatusCode.ShouldBe(HttpStatusCode.OK);
        identityApiDriver.AuthenticateWithLastToken();
    }

    [When("the user logs in with username {string} and password {string}")]
    public async Task WhenTheUserLogsInWithUsernameAndPassword(string username, string password) =>
        await identityApiDriver.LoginAsync(username, password);

    [When("the authenticated user requests their profile")]
    public async Task WhenTheAuthenticatedUserRequestsTheirProfile() => await identityApiDriver.GetCurrentUserAsync();

    [Then("the login response should be ok")]
    public void ThenTheLoginResponseShouldBeOk() =>
        scenarioContext.LastResponse.ShouldNotBeNull().StatusCode.ShouldBe(HttpStatusCode.OK);

    [Then("the login response should contain a bearer access token")]
    public void ThenTheLoginResponseShouldContainABearerAccessToken()
    {
        var response = scenarioContext.TokenResponse.ShouldNotBeNull();
        response.TokenType.ShouldBe("Bearer");
        response.AccessToken.ShouldNotBeNullOrWhiteSpace();
        response.ExpiresIn.ShouldBeGreaterThan(0);
    }

    [Then("the response should be unauthorized")]
    public void ThenTheResponseShouldBeUnauthorized() =>
        scenarioContext.LastResponse.ShouldNotBeNull().StatusCode.ShouldBe(HttpStatusCode.Unauthorized);

    [Then("the profile response should be ok")]
    public void ThenTheProfileResponseShouldBeOk() =>
        scenarioContext.LastResponse.ShouldNotBeNull().StatusCode.ShouldBe(HttpStatusCode.OK);

    [Then("the profile should contain username {string}, email {string}, and name {string}")]
    public void ThenTheProfileShouldContainUsernameEmailAndName(string username, string email, string name)
    {
        var response = scenarioContext.ProfileResponse.ShouldNotBeNull();
        response.Username.ShouldBe(username);
        response.Email.ShouldBe(email);
        response.Name.ShouldBe(name);
        response.Role.ShouldBe(UserRole.User);
    }
}
