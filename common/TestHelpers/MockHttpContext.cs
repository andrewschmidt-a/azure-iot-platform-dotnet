// Copyright (c) Microsoft. All rights reserved.

using System;
using System.IO;
using Microsoft.AspNetCore.Http;
using Moq;

namespace Mmm.Platform.IoT.Common.TestHelpers
{
    public sealed class MockHttpContext : IDisposable
    {
        private bool disposedValue;
        private readonly HeaderDictionary requestHeaders = new HeaderDictionary();
        private readonly HeaderDictionary responseHeaders = new HeaderDictionary();
        private readonly MemoryStream requestBody = new MemoryStream();
        private readonly MemoryStream responseBody = new MemoryStream();
        private readonly Mock<HttpContext> mockContext = new Mock<HttpContext>();

        public MockHttpContext()
        {
            this.disposedValue = false;

            var request = new Mock<Microsoft.AspNetCore.Http.HttpRequest>();
            request.SetupGet(x => x.Headers).Returns(this.requestHeaders);
            request.SetupGet(x => x.Body).Returns(this.requestBody);
            request.SetupProperty(x => x.ContentType);

            var response = new Mock<Microsoft.AspNetCore.Http.HttpResponse>();
            response.SetupGet(x => x.Headers).Returns(this.responseHeaders);
            response.SetupGet(x => x.Body).Returns(this.responseBody);
            response.SetupProperty(x => x.ContentType);

            this.mockContext.SetupGet(x => x.Request).Returns(request.Object);
            this.mockContext.SetupGet(x => x.Response).Returns(response.Object);
        }

        public HttpContext Object => this.mockContext.Object;

        public void SetHeader(string key, string value)
        {
            this.requestHeaders.Add(key, value);
        }

        public string GetHeader(string key)
        {
            return this.responseHeaders[key];
        }

        public void SetBody(string content)
        {
            var bytes = Convert.FromBase64String(content);
            this.requestBody.Write(bytes, 0, bytes.Length);
            this.requestBody.Seek(0, SeekOrigin.Begin);
        }

        public string GetBody()
        {
            this.responseBody.Seek(0, SeekOrigin.Begin);
            var bytes = this.responseBody.ToArray();
            return Convert.ToBase64String(bytes);
        }

        private void Dispose(bool disposing)
        {
            if (!this.disposedValue)
            {
                if (disposing)
                {
                    this.requestBody.Dispose();
                    this.responseBody.Dispose();
                }

                this.disposedValue = true;
            }
        }

        public void Dispose()
        {
            this.Dispose(true);
        }
    }
}
