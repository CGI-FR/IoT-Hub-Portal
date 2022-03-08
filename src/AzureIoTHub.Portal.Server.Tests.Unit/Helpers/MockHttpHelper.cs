using Bunit;
using Microsoft.Extensions.DependencyInjection;
using RichardSzalay.MockHttp;
using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading.Tasks;

namespace AzureIoTHub.Portal.Server.Tests.Unit.Helpers
{
    public static class MockHttpHelper
    {
        public static MockHttpMessageHandler AddMockHttpClient(this TestServiceProvider services)
        {
            var mockHttpHandler = new MockHttpMessageHandler();
            var httpClient = mockHttpHandler.ToHttpClient();
            httpClient.BaseAddress = new Uri("http://fake.local");
            services.AddSingleton<HttpClient>(httpClient);

            return mockHttpHandler;
        }

        public static MockedRequest RespondJson<T>(this MockedRequest request, T content)
        {
            request.Respond(req =>
            {
                var response = new HttpResponseMessage(HttpStatusCode.OK);
                response.Content = new StringContent(JsonSerializer.Serialize(content));
                response.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                return response;
            });

            return request;
        }

        public static MockedRequest RespondJsonAsync<T>(this MockedRequest request, Task<T> content)
        {
            request.Respond(req =>
            {
                var response = new HttpResponseMessage(HttpStatusCode.OK);
                response.Content = new StringContent(JsonSerializer.Serialize(content));
                response.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                return response;
            });

            return request;
        }

        public static MockedRequest RespondText(this MockedRequest request, string content)
        {
            request.Respond(req =>
            {
                var response = new HttpResponseMessage(HttpStatusCode.OK);
                response.Content = new StringContent(content);
                response.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                return response;
            });

            return request;
        }

        public static MockedRequest RespondJson<T>(this MockedRequest request, Func<T> contentProvider)
        {
            request.Respond(req =>
            {
                var response = new HttpResponseMessage(HttpStatusCode.OK);
                response.Content = new StringContent(JsonSerializer.Serialize(contentProvider()));
                response.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                return response;
            });

            return request;
        }

        public static IMockedRequest AddRequestExpectation(this IMockedRequest request, MockHttpMessageHandler mockHttpClient)
        {
            mockHttpClient.AddRequestExpectation(request);

            return request;
        }
    }
}
