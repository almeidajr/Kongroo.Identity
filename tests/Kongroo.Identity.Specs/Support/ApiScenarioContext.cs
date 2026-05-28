using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.Json.Serialization;
using Kongroo.Identity.Application;
using Microsoft.AspNetCore.Mvc.Testing;

namespace Kongroo.Identity.Specs.Support;

public sealed class ApiScenarioContext : IDisposable
{
    public static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) },
    };

    private HttpClient? _client;

    public HttpClient Client =>
        _client ??= SpecsEnvironment.Factory.CreateClient(
            new WebApplicationFactoryClientOptions { AllowAutoRedirect = false }
        );

    public HttpResponseMessage? LastResponse { get; private set; }

    public string LastResponseContent { get; private set; } = string.Empty;

    public CreateUserResponse? RegistrationResponse { get; set; }

    public AuthenticateUserResponse? TokenResponse { get; set; }

    public GetUserResponse? ProfileResponse { get; set; }

    public void SetLastResponse(HttpResponseMessage response, string responseContent)
    {
        LastResponse?.Dispose();
        LastResponse = response;
        LastResponseContent = responseContent;
    }

    public void Authenticate(string accessToken) =>
        Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

    public void Dispose()
    {
        LastResponse?.Dispose();
        _client?.Dispose();
    }
}

