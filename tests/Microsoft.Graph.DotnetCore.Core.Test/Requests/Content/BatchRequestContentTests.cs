// ------------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All Rights Reserved.  Licensed under the MIT License.  See License in the project root for license information.
// ------------------------------------------------------------------------------

namespace Microsoft.Graph.DotnetCore.Core.Test.Requests.Content
{
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using Microsoft.Graph.DotnetCore.Core.Test.Mocks;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net.Http;
    using Xunit;

    public class BatchRequestContentTests
    {
        private const string REQUEST_URL = "https://graph.microsoft.com/v1.0/me";
        [Fact]
        public void BatchRequestContent_DefaultInitialize()
        {
            BatchRequestContent batchRequestContent = new BatchRequestContent();

            Assert.NotNull(batchRequestContent.BatchRequestSteps);
            Assert.True(batchRequestContent.BatchRequestSteps.Count.Equals(0));
        }

        [Fact]
        public void BatchRequestContent_InitializeWithBatchRequestSteps()
        {
            List<BatchRequestStep> requestSteps = new List<BatchRequestStep>();
            for (int i = 0; i < 5; i++)
            {
                requestSteps.Add(new BatchRequestStep(i.ToString(), new HttpRequestMessage(HttpMethod.Get, REQUEST_URL)));
            }

            BatchRequestContent batchRequestContent = new BatchRequestContent(requestSteps.ToArray());

            Assert.NotNull(batchRequestContent.BatchRequestSteps);
            Assert.True(batchRequestContent.BatchRequestSteps.Count.Equals(5));
        }

        [Fact]
        public void BatchRequestContent_InitializeWithInvalidDependsOnIds()
        {
            BatchRequestStep batchRequestStep1 = new BatchRequestStep("1", new HttpRequestMessage(HttpMethod.Get, REQUEST_URL));
            BatchRequestStep batchRequestStep2 = new BatchRequestStep("2", new HttpRequestMessage(HttpMethod.Get, REQUEST_URL), new List<string> { "3" });

            ClientException ex = Assert.Throws<ClientException>(() => new BatchRequestContent(batchRequestStep1, batchRequestStep2));

            Assert.Equal(ErrorConstants.Codes.InvalidArgument, ex.Error.Code);
            Assert.Equal(ErrorConstants.Messages.InvalidDependsOnRequestId, ex.Error.Message);
        }

        [Fact]
        public void BatchRequestContent_AddBatchRequestStepWithNewRequestStep()
        {
            // Arrange
            BatchRequestStep batchRequestStep = new BatchRequestStep("1", new HttpRequestMessage(HttpMethod.Get, REQUEST_URL));
            BatchRequestContent batchRequestContent = new BatchRequestContent();
            
            // Act
            Assert.False(batchRequestContent.BatchRequestSteps.Any());//Its empty
            bool isSuccess = batchRequestContent.AddBatchRequestStep(batchRequestStep);

            // Assert
            Assert.True(isSuccess);
            Assert.NotNull(batchRequestContent.BatchRequestSteps);
            Assert.True(batchRequestContent.BatchRequestSteps.Count.Equals(1));
        }

        [Fact]
        public void BatchRequestContent_AddBatchRequestStepToBatchRequestContentWithMaxSteps()
        {
            // Arrange
            BatchRequestContent batchRequestContent = new BatchRequestContent();
            //Add MaxNumberOfRequests number of steps
            for (var i = 0; i < CoreConstants.BatchRequest.MaxNumberOfRequests; i++)
            {
                BatchRequestStep batchRequestStep = new BatchRequestStep(i.ToString(), new HttpRequestMessage(HttpMethod.Get, REQUEST_URL));
                bool isSuccess = batchRequestContent.AddBatchRequestStep(batchRequestStep);
                Assert.True(isSuccess);//Assert we can add steps up to the max
            }

            // Act
            BatchRequestStep extraBatchRequestStep = new BatchRequestStep("failing_id", new HttpRequestMessage(HttpMethod.Get, REQUEST_URL));
            bool result = batchRequestContent.AddBatchRequestStep(extraBatchRequestStep);

            // Assert
            Assert.False(result);//Assert we did not add any more steps
            Assert.NotNull(batchRequestContent.BatchRequestSteps);
            Assert.True(batchRequestContent.BatchRequestSteps.Count.Equals(CoreConstants.BatchRequest.MaxNumberOfRequests));
        }

        [Fact]
        public void BatchRequestContent_AddBatchRequestStepWithExistingRequestStep()
        {
            BatchRequestStep batchRequestStep = new BatchRequestStep("1", new HttpRequestMessage(HttpMethod.Get, REQUEST_URL));
            BatchRequestContent batchRequestContent = new BatchRequestContent(batchRequestStep);
            bool isSuccess = batchRequestContent.AddBatchRequestStep(batchRequestStep);

            Assert.False(isSuccess);
            Assert.NotNull(batchRequestContent.BatchRequestSteps);
            Assert.True(batchRequestContent.BatchRequestSteps.Count.Equals(1));
        }

        [Fact]
        public void BatchRequestContent_AddBatchRequestStepWithNullRequestStep()
        {
            BatchRequestStep batchRequestStep = new BatchRequestStep("1", new HttpRequestMessage(HttpMethod.Get, REQUEST_URL));
            BatchRequestContent batchRequestContent = new BatchRequestContent(batchRequestStep);

            bool isSuccess = batchRequestContent.AddBatchRequestStep(batchRequestStep: null);

            Assert.False(isSuccess);
            Assert.NotNull(batchRequestContent.BatchRequestSteps);
            Assert.True(batchRequestContent.BatchRequestSteps.Count.Equals(1));
        }

        [Fact]
        public void BatchRequestContent_RemoveBatchRequestStepWithIdForExistingId()
        {
            BatchRequestStep batchRequestStep1 = new BatchRequestStep("1", new HttpRequestMessage(HttpMethod.Get, REQUEST_URL));
            BatchRequestStep batchRequestStep2 = new BatchRequestStep("2", new HttpRequestMessage(HttpMethod.Get, REQUEST_URL), new List<string> { "1" });

            BatchRequestContent batchRequestContent = new BatchRequestContent(batchRequestStep1, batchRequestStep2);

            bool isSuccess = batchRequestContent.RemoveBatchRequestStepWithId("1");

            Assert.True(isSuccess);
            Assert.True(batchRequestContent.BatchRequestSteps.Count.Equals(1));
            Assert.True(batchRequestContent.BatchRequestSteps["2"].DependsOn.Count.Equals(0));
        }

        [Fact]
        public void BatchRequestContent_RemoveBatchRequestStepWithIdForNonExistingId()
        {
            BatchRequestStep batchRequestStep1 = new BatchRequestStep("1", new HttpRequestMessage(HttpMethod.Get, REQUEST_URL));
            BatchRequestStep batchRequestStep2 = new BatchRequestStep("2", new HttpRequestMessage(HttpMethod.Get, REQUEST_URL), new List<string> { "1" });

            BatchRequestContent batchRequestContent = new BatchRequestContent(batchRequestStep1, batchRequestStep2);

            bool isSuccess = batchRequestContent.RemoveBatchRequestStepWithId("5");

            Assert.False(isSuccess);
            Assert.True(batchRequestContent.BatchRequestSteps.Count.Equals(2));
            Assert.Same(batchRequestStep2.DependsOn.First(), batchRequestContent.BatchRequestSteps["2"].DependsOn.First());
        }

        [Fact]
        public async System.Threading.Tasks.Task BatchRequestContent_GetBatchRequestContentFromStepAsync()
        {
            BatchRequestStep batchRequestStep1 = new BatchRequestStep("1", new HttpRequestMessage(HttpMethod.Get, REQUEST_URL));
            BatchRequestStep batchRequestStep2 = new BatchRequestStep("2", new HttpRequestMessage(HttpMethod.Get, REQUEST_URL), new List<string> { "1" });

            BatchRequestContent batchRequestContent = new BatchRequestContent();
            batchRequestContent.AddBatchRequestStep(batchRequestStep1);
            batchRequestContent.AddBatchRequestStep(batchRequestStep2);

            batchRequestContent.RemoveBatchRequestStepWithId("1");

            JObject requestContent = await batchRequestContent.GetBatchRequestContentAsync();

            string expectedJson = "{\"requests\":[{\"id\":\"2\",\"url\":\"/me\",\"method\":\"GET\"}]}";
            JObject expectedContent = JsonConvert.DeserializeObject<JObject>(expectedJson);

            Assert.NotNull(requestContent);
            Assert.True(batchRequestContent.BatchRequestSteps.Count.Equals(1));
            Assert.Equal(expectedContent, requestContent);
        }

        [Fact]
        public void BatchRequestContent_AddBatchRequestStepWithHttpRequestMessage()
        {
            // Arrange 
            BatchRequestContent batchRequestContent = new BatchRequestContent();
            Assert.False(batchRequestContent.BatchRequestSteps.Any());//Its empty

            // Act
            HttpRequestMessage httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, REQUEST_URL);
            string batchRequestStepId = batchRequestContent.AddBatchRequestStep(httpRequestMessage);

            // Assert we added successfully and contents are as expected
            Assert.NotNull(batchRequestStepId);
            Assert.NotNull(batchRequestContent.BatchRequestSteps);
            Assert.True(batchRequestContent.BatchRequestSteps.Count.Equals(1));
            Assert.Equal(batchRequestContent.BatchRequestSteps.First().Value.Request.RequestUri.AbsoluteUri, httpRequestMessage.RequestUri.AbsoluteUri);
            Assert.Equal(batchRequestContent.BatchRequestSteps.First().Value.Request.Method.Method, httpRequestMessage.Method.Method);
        }

        [Fact]
        public void BatchRequestContent_AddBatchRequestStepWithHttpRequestMessageToBatchRequestContentWithMaxSteps()
        {
            // Arrange
            BatchRequestContent batchRequestContent = new BatchRequestContent();
            // Add MaxNumberOfRequests number of steps
            for (var i = 0; i < CoreConstants.BatchRequest.MaxNumberOfRequests; i++)
            {
                HttpRequestMessage httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, REQUEST_URL);
                string batchRequestStepId = batchRequestContent.AddBatchRequestStep(httpRequestMessage);
                Assert.NotNull(batchRequestStepId);
                Assert.True(batchRequestContent.BatchRequestSteps.Count.Equals(i+1));//Assert we can add steps up to the max
            }

            // Act
            HttpRequestMessage extraHttpRequestMessage = new HttpRequestMessage(HttpMethod.Get, REQUEST_URL);
            
            // Assert
            var exception = Assert.Throws<ClientException>(() => batchRequestContent.AddBatchRequestStep(extraHttpRequestMessage));//Assert we throw exception on excess add
            Assert.Equal(ErrorConstants.Codes.MaximumValueExceeded, exception.Error.Code);
            Assert.NotNull(batchRequestContent.BatchRequestSteps);
            Assert.True(batchRequestContent.BatchRequestSteps.Count.Equals(CoreConstants.BatchRequest.MaxNumberOfRequests));
        }

        [Fact]
        public void BatchRequestContent_AddBatchRequestStepWithBaseRequest()
        {
            // Arrange
            BaseClient client = new BaseClient(REQUEST_URL, new MockAuthenticationProvider().Object);
            BaseRequest baseRequest = new BaseRequest(REQUEST_URL, client);
            BatchRequestContent batchRequestContent = new BatchRequestContent();
            Assert.False(batchRequestContent.BatchRequestSteps.Any());//Its empty

            // Act
            string batchRequestStepId = batchRequestContent.AddBatchRequestStep(baseRequest);

            // Assert we added successfully and contents are as expected
            Assert.NotNull(batchRequestStepId);
            Assert.NotNull(batchRequestContent.BatchRequestSteps);
            Assert.True(batchRequestContent.BatchRequestSteps.Count.Equals(1));
            Assert.Equal(batchRequestContent.BatchRequestSteps.First().Value.Request.RequestUri.OriginalString, baseRequest.RequestUrl);
            Assert.Equal(batchRequestContent.BatchRequestSteps.First().Value.Request.Method.Method, baseRequest.Method);
        }

        [Fact]
        public void BatchRequestContent_AddBatchRequestStepWithBaseRequestToBatchRequestContentWithMaxSteps()
        {
            // Arrange
            BatchRequestContent batchRequestContent = new BatchRequestContent();
            BaseClient client = new BaseClient(REQUEST_URL, new MockAuthenticationProvider().Object);
            // Add MaxNumberOfRequests number of steps
            for (var i = 0; i < CoreConstants.BatchRequest.MaxNumberOfRequests; i++)
            {
                BaseRequest baseRequest = new BaseRequest(REQUEST_URL, client);
                string batchRequestStepId = batchRequestContent.AddBatchRequestStep(baseRequest);
                Assert.NotNull(batchRequestStepId);
                Assert.True(batchRequestContent.BatchRequestSteps.Count.Equals(i + 1));//Assert we can add steps up to the max
            }

            // Act
            BaseRequest extraBaseRequest = new BaseRequest(REQUEST_URL, client);
            var exception = Assert.Throws<ClientException>(() => batchRequestContent.AddBatchRequestStep(extraBaseRequest));
            
            // Assert
            Assert.Equal(ErrorConstants.Codes.MaximumValueExceeded, exception.Error.Code);
            Assert.NotNull(batchRequestContent.BatchRequestSteps);
            Assert.True(batchRequestContent.BatchRequestSteps.Count.Equals(CoreConstants.BatchRequest.MaxNumberOfRequests));
        }

    }
}
