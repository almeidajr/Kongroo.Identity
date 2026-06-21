using System.Net;
using Kongroo.Identity.Specs.Support;
using Reqnroll;
using Shouldly;

namespace Kongroo.Identity.Specs.StepDefinitions;

[Binding]
public sealed class HealthStepDefinitions(ApiScenarioContext scenarioContext)
{
    [When("the {string} probe endpoint is requested")]
    public async Task WhenTheProbeEndpointIsRequested(string path)
    {
        var response = await scenarioContext.Client.GetAsync(path);
        scenarioContext.SetLastResponse(response, await response.Content.ReadAsStringAsync());
    }

    [Then("the probe response should be ok")]
    public void ThenTheProbeResponseShouldBeOk() =>
        scenarioContext.LastResponse.ShouldNotBeNull().StatusCode.ShouldBe(HttpStatusCode.OK);
}
