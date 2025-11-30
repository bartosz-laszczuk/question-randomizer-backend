namespace QuestionRandomizer.E2ETests.Infrastructure;

using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using FluentAssertions;

/// <summary>
/// Base class for E2E tests with helper methods for common HTTP operations
/// </summary>
public abstract class E2ETestBase : IClassFixture<E2ETestWebApplicationFactory>
{
    protected readonly E2ETestWebApplicationFactory Factory;
    protected readonly HttpClient Client;

    protected E2ETestBase(E2ETestWebApplicationFactory factory)
    {
        Factory = factory;
        Client = factory.CreateClient();
    }

    #region HTTP Helper Methods

    /// <summary>
    /// Sends a GET request and returns the deserialized response
    /// </summary>
    protected async Task<TResponse?> GetAsync<TResponse>(string url)
    {
        var response = await Client.GetAsync(url);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<TResponse>();
    }

    /// <summary>
    /// Sends a GET request and returns the HttpResponseMessage
    /// </summary>
    protected async Task<HttpResponseMessage> GetAsync(string url)
    {
        return await Client.GetAsync(url);
    }

    /// <summary>
    /// Sends a POST request with JSON body and returns the deserialized response
    /// </summary>
    protected async Task<TResponse?> PostAsync<TRequest, TResponse>(string url, TRequest request)
    {
        var response = await Client.PostAsJsonAsync(url, request);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<TResponse>();
    }

    /// <summary>
    /// Sends a POST request with JSON body and returns the HttpResponseMessage
    /// </summary>
    protected async Task<HttpResponseMessage> PostAsync<TRequest>(string url, TRequest request)
    {
        return await Client.PostAsJsonAsync(url, request);
    }

    /// <summary>
    /// Sends a PUT request with JSON body and returns the deserialized response
    /// </summary>
    protected async Task<TResponse?> PutAsync<TRequest, TResponse>(string url, TRequest request)
    {
        var response = await Client.PutAsJsonAsync(url, request);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<TResponse>();
    }

    /// <summary>
    /// Sends a PUT request with JSON body and returns the HttpResponseMessage
    /// </summary>
    protected async Task<HttpResponseMessage> PutAsync<TRequest>(string url, TRequest request)
    {
        return await Client.PutAsJsonAsync(url, request);
    }

    /// <summary>
    /// Sends a DELETE request and returns the HttpResponseMessage
    /// </summary>
    protected async Task<HttpResponseMessage> DeleteAsync(string url)
    {
        return await Client.DeleteAsync(url);
    }

    #endregion

    #region Assertion Helpers

    /// <summary>
    /// Asserts that the response has the expected status code
    /// </summary>
    protected void AssertStatusCode(HttpResponseMessage response, System.Net.HttpStatusCode expectedStatusCode)
    {
        response.StatusCode.Should().Be(expectedStatusCode);
    }

    /// <summary>
    /// Asserts that the response is successful (2xx status code)
    /// </summary>
    protected void AssertSuccess(HttpResponseMessage response)
    {
        response.IsSuccessStatusCode.Should().BeTrue();
    }

    /// <summary>
    /// Asserts that the response is Not Found (404)
    /// </summary>
    protected void AssertNotFound(HttpResponseMessage response)
    {
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.NotFound);
    }

    /// <summary>
    /// Asserts that the response is Bad Request (400)
    /// </summary>
    protected void AssertBadRequest(HttpResponseMessage response)
    {
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.BadRequest);
    }

    #endregion

    #region Test Data Helpers

    /// <summary>
    /// Creates a unique test string with prefix and timestamp
    /// </summary>
    protected string CreateTestString(string prefix)
    {
        return $"{prefix}_{DateTime.UtcNow:yyyyMMddHHmmssfff}";
    }

    /// <summary>
    /// Waits for a short delay (useful for timing-sensitive tests)
    /// </summary>
    protected async Task WaitAsync(int milliseconds = 100)
    {
        await Task.Delay(milliseconds);
    }

    #endregion
}
