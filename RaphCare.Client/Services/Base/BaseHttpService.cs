using RaphCare.Client.Contracts;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace RaphCare.Client.Services.Base;

/// <summary>
/// Base service with generic HTTP helpers returning standardized <see cref="Response{T}"/>.
/// </summary>
public abstract class BaseHttpService(HttpClient httpClient)
{
    protected HttpClient HttpClient { get; } = httpClient;

    protected async Task<Response<T>> GetAsync<T>(string requestUri, CancellationToken cancellationToken = default)
    {
        try
        {
            using var response = await HttpClient.GetAsync(requestUri, cancellationToken).ConfigureAwait(false);
            if (!response.IsSuccessStatusCode)
                return await ToErrorResponseAsync<T>(response, cancellationToken).ConfigureAwait(false);
            var data = await response.Content.ReadFromJsonAsync<T>(ApiJson.Options, cancellationToken).ConfigureAwait(false);
            return Response<T>.Success(data!);
        }
        catch (HttpRequestException)
        {
            return Response<T>.Failure("We couldn't reach the server. Check your connection and try again.");
        }
    }

    protected async Task<Response<T>> PostAsync<T>(string requestUri, object? body, CancellationToken cancellationToken = default)
    {
        try
        {
            using var response = await HttpClient.PostAsJsonAsync(requestUri, body, ApiJson.Options, cancellationToken).ConfigureAwait(false);
            if (!response.IsSuccessStatusCode)
                return await ToErrorResponseAsync<T>(response, cancellationToken).ConfigureAwait(false);
            var data = response.StatusCode == HttpStatusCode.NoContent
                ? default
                : await response.Content.ReadFromJsonAsync<T>(ApiJson.Options, cancellationToken).ConfigureAwait(false);
            return Response<T>.Success(data!);
        }
        catch (HttpRequestException)
        {
            return Response<T>.Failure("We couldn't reach the server. Check your connection and try again.");
        }
    }

    protected async Task<Response<T>> PutAsync<T>(string requestUri, object? body, CancellationToken cancellationToken = default)
    {
        try
        {
            using var response = await HttpClient.PutAsJsonAsync(requestUri, body, ApiJson.Options, cancellationToken).ConfigureAwait(false);
            if (!response.IsSuccessStatusCode)
                return await ToErrorResponseAsync<T>(response, cancellationToken).ConfigureAwait(false);
            var data = response.StatusCode == HttpStatusCode.NoContent
                ? default
                : await response.Content.ReadFromJsonAsync<T>(ApiJson.Options, cancellationToken).ConfigureAwait(false);
            return Response<T>.Success(data!);
        }
        catch (HttpRequestException)
        {
            return Response<T>.Failure("We couldn't reach the server. Check your connection and try again.");
        }
    }

    protected async Task<Response<bool>> DeleteAsync(string requestUri, CancellationToken cancellationToken = default)
    {
        try
        {
            using var response = await HttpClient.DeleteAsync(requestUri, cancellationToken).ConfigureAwait(false);
            if (!response.IsSuccessStatusCode)
                return await ToErrorResponseAsync<bool>(response, cancellationToken).ConfigureAwait(false);
            return Response<bool>.Success(true);
        }
        catch (HttpRequestException)
        {
            return Response<bool>.Failure("We couldn't reach the server. Check your connection and try again.");
        }
    }

    protected async Task<Response<bool>> PutNoContentAsync(string requestUri, object? body, CancellationToken cancellationToken = default)
    {
        try
        {
            using var response = await HttpClient.PutAsJsonAsync(requestUri, body, ApiJson.Options, cancellationToken).ConfigureAwait(false);
            if (!response.IsSuccessStatusCode)
                return await ToErrorResponseAsync<bool>(response, cancellationToken).ConfigureAwait(false);
            return Response<bool>.Success(true);
        }
        catch (HttpRequestException)
        {
            return Response<bool>.Failure("We couldn't reach the server. Check your connection and try again.");
        }
    }

    protected async Task<Response<bool>> PostNoContentAsync(string requestUri, object? body, CancellationToken cancellationToken = default)
    {
        try
        {
            using var response = await HttpClient.PostAsJsonAsync(requestUri, body, ApiJson.Options, cancellationToken).ConfigureAwait(false);
            if (!response.IsSuccessStatusCode)
                return await ToErrorResponseAsync<bool>(response, cancellationToken).ConfigureAwait(false);
            return Response<bool>.Success(true);
        }
        catch (HttpRequestException)
        {
            return Response<bool>.Failure("We couldn't reach the server. Check your connection and try again.");
        }
    }

    private static async Task<Response<T>> ToErrorResponseAsync<T>(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        var body = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
        var message = string.IsNullOrEmpty(body) ? $"API error: {response.StatusCode}" : body;
        return Response<T>.Failure(message, (int)response.StatusCode);
    }
}
