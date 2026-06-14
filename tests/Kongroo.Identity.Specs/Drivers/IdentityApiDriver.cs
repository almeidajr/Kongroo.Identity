using Kongroo.Identity.Application;
using Kongroo.Identity.Presentation;
using Kongroo.Identity.Specs.Support;

namespace Kongroo.Identity.Specs.Drivers;

public sealed class IdentityApiDriver(ApiScenarioContext scenarioContext)
{
    public async Task RegisterAsync(string username, string email, string password, string name)
    {
        var request = new CreateUserRequest(username, email, password, name);
        var response = await scenarioContext.Client.PostAsJsonAsync("/users", request, ApiScenarioContext.JsonOptions);

        scenarioContext.RegistrationResponse = await ReadResponseAsync<CreateUserResponse>(response);
    }

    public async Task LoginAsync(string username, string password)
    {
        var request = new CreateAccessTokenRequest(username, password);
        var response = await scenarioContext.Client.PostAsJsonAsync("/tokens", request, ApiScenarioContext.JsonOptions);

        scenarioContext.TokenResponse = await ReadResponseAsync<AuthenticateUserResponse>(response);
    }

    public async Task GetCurrentUserAsync()
    {
        var response = await scenarioContext.Client.GetAsync("/users/me");
        scenarioContext.ProfileResponse = await ReadResponseAsync<GetUserResponse>(response);
    }

    public void AuthenticateWithLastToken()
    {
        var accessToken =
            scenarioContext.TokenResponse?.AccessToken
            ?? throw new InvalidOperationException("The scenario does not have a successful login token.");

        scenarioContext.Authenticate(accessToken);
    }

    private async Task<TResponse?> ReadResponseAsync<TResponse>(HttpResponseMessage response)
    {
        if (response.IsSuccessStatusCode)
        {
            scenarioContext.SetLastResponse(response, string.Empty);
            return await response.Content.ReadFromJsonAsync<TResponse>(ApiScenarioContext.JsonOptions);
        }

        var content = await response.Content.ReadAsStringAsync();
        scenarioContext.SetLastResponse(response, content);
        return default;
    }
}
