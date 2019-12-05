using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

// ReSharper disable once CheckNamespace
namespace ApiFramework.IntegrationTests
{
    public static class HttpClientJsonExtensions
    {
        /// <summary>
        /// Sends a GET request to the specified URI, and parses the JSON response body
        /// to create an object of the generic type.
        /// </summary>
        /// <typeparam name="T">A type into which the response body can be JSON-deserialized.</typeparam>
        /// <param name="httpClient">The <see cref="HttpClient"/>.</param>
        /// <param name="requestUri">The URI that the request will be sent to.</param>
        /// <param name="actionOnFailure">An action to be executed if the response from the server cannot be
        /// serialized into the expected type <typeparamref name="T"/></param>
        /// <returns>The response parsed as an object of the generic type.</returns>
        public static async Task<T> GetJsonAsync<T>(this HttpClient httpClient, string requestUri, Action<string> actionOnFailure = null)
        {
            var stringContent = await httpClient.GetStringAsync(requestUri);
            T result = default;

            try
            {
                result = JsonSerializer.Deserialize<T>(stringContent, JsonSerializerOptionsProvider.Options);
            }
            catch
            {
                if (actionOnFailure != null)
                {
                    actionOnFailure.Invoke(stringContent);
                }
            }

            return result;
        }

        /// <summary>
        /// Sends a POST request to the specified URI, including the specified <paramref name="content"/>
        /// in JSON-encoded format, and parses the JSON response body to create an object of the generic type.
        /// </summary>
        /// <param name="httpClient">The <see cref="HttpClient"/>.</param>
        /// <param name="requestUri">The URI that the request will be sent to.</param>
        /// <param name="content">Content for the request body. This will be JSON-encoded and sent as a string.</param>
        /// <param name="actionOnFailure">An action to be executed if the response from the server cannot be
        /// serialized into the expected type <typeparamref name="T"/></param>
        /// <returns>The response parsed as an object of the generic type.</returns>
        public static Task PostJsonAsync(this HttpClient httpClient,
            string requestUri, 
            object content,
            Action<HttpResponseMessage> actionOnFailure = null)
            => httpClient.SendJsonAsync(HttpMethod.Post, requestUri, content);

        /// <summary>
        /// Sends a POST request to the specified URI, including the specified <paramref name="content"/>
        /// in JSON-encoded format, and parses the JSON response body to create an object of the generic type.
        /// </summary>
        /// <typeparam name="T">A type into which the response body can be JSON-deserialized.</typeparam>
        /// <param name="httpClient">The <see cref="HttpClient"/>.</param>
        /// <param name="requestUri">The URI that the request will be sent to.</param>
        /// <param name="content">Content for the request body. This will be JSON-encoded and sent as a string.</param>
        /// <param name="actionOnFailure">An action to be executed if the response from the server cannot be
        /// serialized into the expected type <typeparamref name="T"/></param>
        /// <returns>The response parsed as an object of the generic type.</returns>
        public static Task<T> PostJsonAsync<T>(this HttpClient httpClient,
            string requestUri, 
            object content, 
            Action<HttpResponseMessage> actionOnFailure = null)
            => httpClient.SendJsonAsync<T>(HttpMethod.Post, requestUri, content, actionOnFailure);

        /// <summary>
        /// Sends a PUT request to the specified URI, including the specified <paramref name="content"/>
        /// in JSON-encoded format.
        /// </summary>
        /// <param name="httpClient">The <see cref="HttpClient"/>.</param>
        /// <param name="requestUri">The URI that the request will be sent to.</param>
        /// <param name="content">Content for the request body. This will be JSON-encoded and sent as a string.</param>
        /// <param name="actionOnFailure">An action to be executed if the response from the server cannot be
        /// serialized into the expected type <typeparamref name="T"/></param>
        public static Task PutJsonAsync(this HttpClient httpClient, 
            string requestUri,
            object content,
            Action<HttpResponseMessage> actionOnFailure = null)
            => httpClient.SendJsonAsync(HttpMethod.Put, requestUri, content, actionOnFailure);

        /// <summary>
        /// Sends a PUT request to the specified URI, including the specified <paramref name="content"/>
        /// in JSON-encoded format, and parses the JSON response body to create an object of the generic type.
        /// </summary>
        /// <typeparam name="T">A type into which the response body can be JSON-deserialized.</typeparam>
        /// <param name="httpClient">The <see cref="HttpClient"/>.</param>
        /// <param name="requestUri">The URI that the request will be sent to.</param>
        /// <param name="content">Content for the request body. This will be JSON-encoded and sent as a string.</param>
        /// <param name="actionOnFailure">An action to be executed if the response from the server cannot be
        /// serialized into the expected type <typeparamref name="T"/></param>
        /// <returns>The response parsed as an object of the generic type.</returns>
        public static Task<T> PutJsonAsync<T>(this HttpClient httpClient,
            string requestUri, 
            object content,
            Action<HttpResponseMessage> actionOnFailure = null)
            => httpClient.SendJsonAsync<T>(HttpMethod.Put, requestUri, content, actionOnFailure);

        /// <summary>
        /// Sends an HTTP request to the specified URI, including the specified <paramref name="content"/>
        /// in JSON-encoded format.
        /// </summary>
        /// <param name="httpClient">The <see cref="HttpClient"/>.</param>
        /// <param name="method">The HTTP method.</param>
        /// <param name="requestUri">The URI that the request will be sent to.</param>
        /// <param name="actionOnFailure">An action to be executed if the response from the server cannot be
        /// serialized into the expected type <typeparamref name="T"/></param>
        /// <param name="content">Content for the request body. This will be JSON-encoded and sent as a string.</param>
        public static Task SendJsonAsync(this HttpClient httpClient,
            HttpMethod method, 
            string requestUri,
            object content,
            Action<HttpResponseMessage> actionOnFailure = null)
            => httpClient.SendJsonAsync<IgnoreResponse>(method, requestUri, content, actionOnFailure);

        /// <summary>
        /// Sends an HTTP request to the specified URI, including the specified <paramref name="content"/>
        /// in JSON-encoded format, and parses the JSON response body to create an object of the generic type.
        /// </summary>
        /// <typeparam name="T">A type into which the response body can be JSON-deserialized.</typeparam>
        /// <param name="httpClient">The <see cref="HttpClient"/>.</param>
        /// <param name="method">The HTTP method.</param>
        /// <param name="requestUri">The URI that the request will be sent to.</param>
        /// <param name="content">Content for the request body. This will be JSON-encoded and sent as a string.</param>
        /// <param name="actionOnFailure">An action to be executed if the response from the server cannot be
        /// serialized into the expected type <typeparamref name="T"/></param>
        /// <returns>The response parsed as an object of the generic type.</returns>
        public static async Task<T> SendJsonAsync<T>(this HttpClient httpClient, 
            HttpMethod method, 
            string requestUri,
            object content,
            Action<HttpResponseMessage> actionOnFailure = null)
        {
            var requestJson = JsonSerializer.Serialize(content, JsonSerializerOptionsProvider.Options);

            var response = await httpClient.SendAsync(new HttpRequestMessage(method, requestUri)
            {
                Content = new StringContent(requestJson, Encoding.UTF8, "application/json")
            });

            if (actionOnFailure is null)
            {
                // Make sure the call was successful before we
                // attempt to process the response content
                response.EnsureSuccessStatusCode();
            }

            if (typeof(T) == typeof(IgnoreResponse))
            {
                return default;
            }
            else
            {
                var stringContent = await response.Content.ReadAsStringAsync();

                try
                {
                    return JsonSerializer.Deserialize<T>(stringContent, JsonSerializerOptionsProvider.Options);
                }
                catch
                {
                    actionOnFailure.Invoke(response);
                    return default;
                }
            }
        }

        private class IgnoreResponse { }
        
        internal static class JsonSerializerOptionsProvider
        {
            public static readonly JsonSerializerOptions Options = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                PropertyNameCaseInsensitive = true,
            };
        }
    }
}
