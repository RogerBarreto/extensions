// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Microsoft.Extensions.AI;

public class AudioTranscriptionClientTests
{
    [Fact]
    public async Task TranscribeAsync_CreatesTextMessageAsync()
    {
        // Arrange
        // We simulate a transcription result by returning an AudioTranscriptionCompletion built from an AudioTranscriptionChoice.
        var expectedResponse = new AudioTranscriptionCompletion(new AudioTranscriptionChoice("hello"));
        var expectedOptions = new AudioTranscriptionOptions();
        using var cts = new CancellationTokenSource();

        using TestAudioTranscriptionClient client = new()
        {
            TranscribeAsyncCallback = (audioContents, options, cancellationToken) =>
            {
                // In our simulated client, we expect a single async enumerable.
                Assert.Single(audioContents);

                // For the purpose of the test, we assume that the underlying implementation converts the DataContent into a transcription choice.
                // (In a real implementation, the audio data would be processed.)
                // Here, we simply return an AudioTranscriptionChoice with the text "hello".
                AudioTranscriptionChoice choice = new("hello");
                return Task.FromResult(new AudioTranscriptionCompletion(choice));
            },
        };

        // Act – call the extension method with a valid DataContent.
        AudioTranscriptionCompletion response = await AudioTranscriptionClientExtensions.TranscribeAsync(
            client,
            new DataContent("data:,hello"),
            expectedOptions,
            cts.Token);

        // Assert
        Assert.Same(expectedResponse.FirstChoice.Text, response.FirstChoice.Text);
    }
}
