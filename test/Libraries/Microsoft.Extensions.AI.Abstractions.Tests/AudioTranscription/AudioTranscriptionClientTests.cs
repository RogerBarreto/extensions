// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Microsoft.Extensions.AI;

public class AudioTranscriptionClientTests
{
    [Fact]
    public void GetService_InvalidArgs_Throws()
    {
        Assert.Throws<ArgumentNullException>("client", () => AudioTranscriptionClientExtensions.GetService<object>(null!));
    }

    [Fact]
    public void CompleteAsync_InvalidArgs_Throws()
    {
        Assert.Throws<ArgumentNullException>("client", () =>
        {
            _ = AudioTranscriptionClientExtensions.CompleteAsync(null!, "hello");
        });

        Assert.Throws<ArgumentNullException>("audioTranscription", () =>
        {
            _ = AudioTranscriptionClientExtensions.CompleteAsync(new TestAudioTranscriptionClient(), null!);
        });
    }

    [Fact]
    public void CompleteStreamingAsync_InvalidArgs_Throws()
    {
        Assert.Throws<ArgumentNullException>("client", () =>
        {
            _ = AudioTranscriptionClientExtensions.CompleteStreamingAsync(null!, "hello");
        });

        Assert.Throws<ArgumentNullException>("audioTranscription", () =>
        {
            _ = AudioTranscriptionClientExtensions.CompleteStreamingAsync(new TestAudioTranscriptionClient(), null!);
        });
    }

    [Fact]
    public async Task CompleteAsync_CreatesTextMessageAsync()
    {
        var expectedResponse = new AudioTranscriptionCompletion([new AudioTranscriptionChoice()]);
        var expectedOptions = new AudioTranscriptionOptions();
        using var cts = new CancellationTokenSource();

        using TestAudioTranscriptionClient client = new()
        {
            CompleteAsyncCallback = (audioTranscriptions, options, cancellationToken) =>
            {
                AudioTranscriptionChoice m = Assert.Single(audioTranscriptions);
                Assert.Equal("hello", m.Text);

                Assert.Same(expectedOptions, options);

                Assert.Equal(cts.Token, cancellationToken);

                return Task.FromResult(expectedResponse);
            },
        };

        AudioTranscriptionCompletion response = await client.CompleteAsync("hello", expectedOptions, cts.Token);

        Assert.Same(expectedResponse, response);
    }

    [Fact]
    public async Task CompleteStreamingAsync_CreatesTextMessageAsync()
    {
        var expectedOptions = new AudioTranscriptionOptions();
        using var cts = new CancellationTokenSource();

        using TestAudioTranscriptionClient client = new()
        {
            CompleteStreamingAsyncCallback = (audioTranscriptions, options, cancellationToken) =>
            {
                AudioTranscriptionChoice m = Assert.Single(audioTranscriptions);
                Assert.Equal("hello", m.Text);

                Assert.Same(expectedOptions, options);

                Assert.Equal(cts.Token, cancellationToken);

                return YieldAsync([new StreamingAudioTranscriptionUpdate { Text = "world" }]);
            },
        };

        int count = 0;
        await foreach (var update in client.CompleteStreamingAsync("hello", expectedOptions, cts.Token))
        {
            Assert.Equal(0, count);
            Assert.Equal("world", update.Text);
            count++;
        }

        Assert.Equal(1, count);
    }

    private static async IAsyncEnumerable<StreamingAudioTranscriptionUpdate> YieldAsync(params StreamingAudioTranscriptionUpdate[] updates)
    {
        await Task.Yield();
        foreach (var update in updates)
        {
            yield return update;
        }
    }
}
