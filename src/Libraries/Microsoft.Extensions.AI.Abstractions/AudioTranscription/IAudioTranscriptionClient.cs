// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Extensions.AI;

public interface IAudioTranscriptionClient : IDisposable
{
    Task<AudioTranscriptionCompletion> TranscribeAsync(
        IAsyncEnumerable<DataContent> audioContents,
        AudioTranscriptionOptions? options = null,
        CancellationToken cancellationToken = default);

    IAsyncEnumerable<StreamingAudioTranscriptionUpdate> TranscribeStreamingAsync(
        IAsyncEnumerable<DataContent> audioContents,
        AudioTranscriptionOptions? options = null,
        CancellationToken cancellationToken = default);
}
