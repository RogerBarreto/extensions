// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Shared.Diagnostics;

namespace Microsoft.Extensions.AI;

public static class AudioTranscriptionClientExtensions
{
    public static Task<AudioTranscriptionCompletion> TranscribeAsync(
        this IAudioTranscriptionClient client,
        DataContent audioContent,
        AudioTranscriptionOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        IEnumerable<DataContent> audioContents = [audioContent];
        return Throw.IfNull(client)
            .TranscribeAsync(
                audioContents.ToAsyncEnumerableAsync(),
                options,
                cancellationToken);
    }

    public static Task<AudioTranscriptionCompletion> TranscribeAsync(
        this IAudioTranscriptionClient client,
        Stream audioStream,
        AudioTranscriptionOptions? options = null,
        CancellationToken cancellationToken = default)
        => Throw.IfNull(client)
            .TranscribeAsync(
                audioStream.ToAsyncEnumerable<DataContent>(mediaType: null, cancellationToken),
                options,
                cancellationToken);

    public static IAsyncEnumerable<StreamingAudioTranscriptionUpdate> TranscribeStreamingAsync(
        this IAudioTranscriptionClient client,
        Stream audioStream,
        AudioTranscriptionOptions? options = null,
        CancellationToken cancellationToken = default)
        => Throw.IfNull(client)
            .TranscribeStreamingAsync(
                audioStream.ToAsyncEnumerable<DataContent>(mediaType: null, cancellationToken),
                options,
                cancellationToken);

    private static async IAsyncEnumerable<T> ToAsyncEnumerableAsync<T>(this IEnumerable<T> source)
    {
        await Task.CompletedTask.ConfigureAwait(false);

        foreach (var item in source)
        {
            yield return item;
        }
    }
}
