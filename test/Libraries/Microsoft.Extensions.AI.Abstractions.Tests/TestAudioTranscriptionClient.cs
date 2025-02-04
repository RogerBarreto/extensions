// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Extensions.AI;

public class TestAudioTranscriptionClient : IAudioTranscriptionClient, IDisposable
{
    public Func<IEnumerable<AudioTranscriptionChoice>, AudioTranscriptionOptions, CancellationToken, Task<AudioTranscriptionCompletion>>? TranscribeAsyncCallback { get; set; }
    public Func<IEnumerable<AudioTranscriptionChoice>, AudioTranscriptionOptions, CancellationToken, IAsyncEnumerable<StreamingAudioTranscriptionUpdate>>? TranscribeStreamingAsyncCallback { get; set; }

    public Task<AudioTranscriptionCompletion> TranscribeAsync(IEnumerable<AudioTranscriptionChoice> audioTranscriptions, AudioTranscriptionOptions options, CancellationToken cancellationToken = default)
    {
        if (TranscribeAsyncCallback is null)
        {
            throw new NotImplementedException();
        }

        return TranscribeAsyncCallback(audioTranscriptions, options, cancellationToken);
    }

    public IAsyncEnumerable<StreamingAudioTranscriptionUpdate> TranscribeStreamingAsync(IEnumerable<AudioTranscriptionChoice> audioTranscriptions, AudioTranscriptionOptions options, CancellationToken cancellationToken = default)
    {
        if (TranscribeStreamingAsyncCallback is null)
        {
            throw new NotImplementedException();
        }

        return TranscribeStreamingAsyncCallback(audioTranscriptions, options, cancellationToken);
    }

    public TService GetService<TService>()
    {
        throw new NotImplementedException();
    }

    public void Dispose()
    {
        // Dispose resources if any
    }
}
