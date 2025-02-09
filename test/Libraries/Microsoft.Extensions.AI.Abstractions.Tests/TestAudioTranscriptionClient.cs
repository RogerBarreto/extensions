// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Extensions.AI;

public sealed class TestAudioTranscriptionClient : IAudioTranscriptionClient
{
    // Mimic the TestChatClient pattern by exposing a Services property.
    public IServiceProvider? Services { get; set; }

    // The metadata property is implemented with a getter and setter.
    public AudioTranscriptionClientMetadata Metadata { get; set; } =
        new AudioTranscriptionClientMetadata("TestAudioTranscriptionClient", new Uri("http://localhost"), "test-model");

    // Callbacks for asynchronous operations.
    public Func<IList<
        IAsyncEnumerable<DataContent>>,
        AudioTranscriptionOptions,
        CancellationToken,
        Task<AudioTranscriptionCompletion>>?
        TranscribeAsyncCallback
    { get; set; }

    public Func<IList<IAsyncEnumerable<DataContent>>,
        AudioTranscriptionOptions,
        CancellationToken,
        IAsyncEnumerable<StreamingAudioTranscriptionUpdate>>?
        TranscribeStreamingAsyncCallback
    { get; set; }

    // A GetService callback similar to the one used in TestChatClient.
    public Func<Type, object?, object?> GetServiceCallback { get; set; }

    public TestAudioTranscriptionClient()
    {
        GetServiceCallback = DefaultGetServiceCallback;
    }

    private object? DefaultGetServiceCallback(Type serviceType, object? serviceKey)
    {
        // If no key is provided and the requested type is assignable from this instance, return this.
        if (serviceKey is null && serviceType.IsInstanceOfType(this))
        {
            return this;
        }

        // If the caller is requesting AudioTranscriptionClientMetadata, return our Metadata.
        if (serviceType == typeof(AudioTranscriptionClientMetadata))
        {
            return Metadata;
        }

        return null;
    }

    public Task<AudioTranscriptionCompletion> TranscribeAsync(
        IList<IAsyncEnumerable<DataContent>> audioContents,
        AudioTranscriptionOptions? options = null,
        CancellationToken cancellationToken = default)
        => TranscribeAsyncCallback!(audioContents, options ?? new AudioTranscriptionOptions(), cancellationToken);

    public IAsyncEnumerable<StreamingAudioTranscriptionUpdate> TranscribeStreamingAsync(
        IList<IAsyncEnumerable<DataContent>> audioContents,
        AudioTranscriptionOptions? options = null,
        CancellationToken cancellationToken = default)
        => TranscribeStreamingAsyncCallback!(audioContents, options ?? new AudioTranscriptionOptions(), cancellationToken);

    public object? GetService(Type serviceType, object? serviceKey = null)
        => GetServiceCallback(serviceType, serviceKey);

    public void Dispose()
    {
        // Dispose of resources if any.
    }
}
