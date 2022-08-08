// Copyright (c) CGI France. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace AzureIoTHub.Portal.Tests.Unit.UnitTests.Helpers
{
    using Bunit;
    using Microsoft.Extensions.DependencyInjection;
    using RichardSzalay.MockHttp;
    using System;
    using System.Net;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Text.Json;
    using System.Threading.Tasks;

    public static class MockHttpHelper
    {
        public static MockHttpMessageHandler AddMockHttpClient(this TestServiceProvider services)
        {
            var mockHttpHandler = new MockHttpMessageHandler();
            var httpClient = mockHttpHandler.ToHttpClient();
            httpClient.BaseAddress = new Uri("http://fake.local");

            _ = services.AddSingleton(httpClient);

            return mockHttpHandler;
        }

        public static MockedRequest RespondJson<T>(this MockedRequest request, T content)
        {
            _ = request.Respond(_ =>
              {
                  var response = new HttpResponseMessage(HttpStatusCode.OK)
                  {
                      Content = new StringContent(JsonSerializer.Serialize(content))
                  };
                  response.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                  return response;
              });

            return request;
        }

        public static MockedRequest RespondJson<T>(this MockedRequest request, Task<T> content)
        {
            _ = request.Respond(_ =>
              {
                  var response = new HttpResponseMessage(HttpStatusCode.OK)
                  {
                      Content = new StringContent(JsonSerializer.Serialize(content))
                  };
                  response.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                  return response;
              });

            return request;
        }

        public static MockedRequest RespondText(this MockedRequest request, string content)
        {
            _ = request.Respond(_ =>
              {
                  var response = new HttpResponseMessage(HttpStatusCode.OK)
                  {
                      Content = new StringContent(content)
                  };
                  response.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                  return response;
              });

            return request;
        }

        public static MockedRequest RespondJson<T>(this MockedRequest request, Func<T> contentProvider)
        {
            _ = request.Respond(_ =>
              {
                  var response = new HttpResponseMessage(HttpStatusCode.OK)
                  {
                      Content = new StringContent(JsonSerializer.Serialize(contentProvider()))
                  };
                  response.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                  return response;
              });

            return request;
        }
    }
}
