// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Text.Json;
using Xunit;

namespace Microsoft.Extensions.AI.Abstractions.Tests
{
    public class ErrorContentTests
    {
        [Fact]
        public void Constructor_ShouldInitializeProperties()
        {
            // Arrange
            string errorMessage = "Error occurred";
            string errorCode = "ERR001";
            string errorDetails = "Something went wrong";

            // Act
            var errorContent = new ErrorContent(errorMessage)
            {
                Code = errorCode,
                Details = errorDetails
            };

            // Assert
            Assert.Equal(errorMessage, errorContent.Message);
            Assert.Equal(errorCode, errorContent.Code);
            Assert.Equal(errorDetails, errorContent.Details);
        }

        [Fact]
        public void JsonSerialization_ShouldSerializeAndDeserializeCorrectly()
        {
            // Arrange
            var errorContent = new ErrorContent("Error occurred")
            {
                Code = "ERR001",
                Details = "Something went wrong"
            };
            var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

            // Act
            var json = JsonSerializer.Serialize(errorContent, options);
            var deserializedErrorContent = JsonSerializer.Deserialize<ErrorContent>(json, options);

            // Assert
            Assert.NotNull(deserializedErrorContent);
            Assert.Equal(errorContent.Message, deserializedErrorContent!.Message);
            Assert.Equal(errorContent.Code, deserializedErrorContent.Code);
            Assert.Equal(errorContent.Details, deserializedErrorContent.Details);
        }
    }
}
