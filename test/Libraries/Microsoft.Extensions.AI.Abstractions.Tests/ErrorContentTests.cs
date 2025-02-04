using System.Text.Json;
using Microsoft.Extensions.AI.Abstractions;
using Xunit;

namespace Microsoft.Extensions.AI.Abstractions.Tests
{
    public class ErrorContentTests
    {
        [Fact]
        public void Constructor_ShouldInitializeProperties()
        {
            // Arrange
            var errorCode = "ErrorCode";
            var errorMessage = "ErrorMessage";

            // Act
            var errorContent = new ErrorContent(errorCode, errorMessage);

            // Assert
            Assert.Equal(errorCode, errorContent.ErrorCode);
            Assert.Equal(errorMessage, errorContent.ErrorMessage);
        }

        [Fact]
        public void JsonSerialization_ShouldSerializeAndDeserializeCorrectly()
        {
            // Arrange
            var errorContent = new ErrorContent("ErrorCode", "ErrorMessage");
            var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

            // Act
            var json = JsonSerializer.Serialize(errorContent, options);
            var deserializedErrorContent = JsonSerializer.Deserialize<ErrorContent>(json, options);

            // Assert
            Assert.NotNull(deserializedErrorContent);
            Assert.Equal(errorContent.ErrorCode, deserializedErrorContent.ErrorCode);
            Assert.Equal(errorContent.ErrorMessage, deserializedErrorContent.ErrorMessage);
        }
    }
}
