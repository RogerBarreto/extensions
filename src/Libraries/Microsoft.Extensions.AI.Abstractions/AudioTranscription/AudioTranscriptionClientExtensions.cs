﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Shared.Diagnostics;

namespace Microsoft.Extensions.AI;

/// <summary>Extensions for <see cref="IAudioTranscriptionClient"/>.</summary>
public static class AudioTranscriptionClientExtensions
{
    /// <summary>Asks the <see cref="IAudioTranscriptionClient"/> for an object of type <typeparamref name="TService"/>.</summary>
    /// <typeparam name="TService">The type of the object to be retrieved.</typeparam>
    /// <param name="client">The client.</param>
    /// <param name="serviceKey">An optional key that can be used to help identify the target service.</param>
    /// <returns>The found object, otherwise <see langword="null"/>.</returns>
    /// <remarks>
    /// The purpose of this method is to allow for the retrieval of strongly typed services that may be provided by the <see cref="IAudioTranscriptionClient"/>,
    /// including itself or any services it might be wrapping.
    /// </remarks>
    public static TService? GetService<TService>(this IAudioTranscriptionClient client, object? serviceKey = null)
    {
        _ = Throw.IfNull(client);

        return (TService?)client.GetService(typeof(TService), serviceKey);
    }

    /// <summary>Transcribes the audio content providing a single audio <see cref="DataContent"/>.</summary>
    /// <param name="client">The client.</param>
    /// <param name="audioContent">The single audio content to be transcribed.</param>
    /// <param name="options">The audio transcription options to configure the request.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> to monitor for cancellation requests. The default is <see cref="CancellationToken.None"/>.</param>
    /// <returns>The transcriptions generated by the client.</returns>
    public static Task<AudioTranscriptionCompletion> TranscribeAsync(
        this IAudioTranscriptionClient client,
        DataContent audioContent,
        AudioTranscriptionOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        IEnumerable<DataContent> audioContents = [audioContent];
        return Throw.IfNull(client)
            .TranscribeAsync(
                [audioContents.ToAsyncEnumerableAsync()],
                options,
                cancellationToken);
    }

    /// <summary>Transcribes the audio content providing a single audio <see cref="Stream"/>.</summary>
    /// <param name="client">The client.</param>
    /// <param name="audioStream">The single audio stream to be transcribed.</param>
    /// <param name="options">The audio transcription options to configure the request.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> to monitor for cancellation requests. The default is <see cref="CancellationToken.None"/>.</param>
    /// <returns>The transcriptions generated by the client.</returns>
    public static Task<AudioTranscriptionCompletion> TranscribeAsync(
        this IAudioTranscriptionClient client,
        Stream audioStream,
        AudioTranscriptionOptions? options = null,
        CancellationToken cancellationToken = default)
        => Throw.IfNull(client)
            .TranscribeAsync(
                [audioStream.ToAsyncEnumerable<DataContent>(mediaType: null, cancellationToken)],
                options,
                cancellationToken);

    /// <summary>Transcribes the audio content providing a single audio <see cref="DataContent"/>.</summary>
    /// <param name="client">The client.</param>
    /// <param name="audioStream">The single audio stream to be transcribed.</param>
    /// <param name="options">The audio transcription options to configure the request.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> to monitor for cancellation requests. The default is <see cref="CancellationToken.None"/>.</param>
    /// <returns>The transcriptions generated by the client.</returns>
    public static IAsyncEnumerable<StreamingAudioTranscriptionUpdate> TranscribeStreamingAsync(
        this IAudioTranscriptionClient client,
        Stream audioStream,
        AudioTranscriptionOptions? options = null,
        CancellationToken cancellationToken = default)
        => Throw.IfNull(client)
            .TranscribeStreamingAsync(
                [audioStream.ToAsyncEnumerable<DataContent>(mediaType: null, cancellationToken)],
                options,
                cancellationToken);

    /// <summary>Transcribes the audio content providing a single audio <see cref="DataContent"/>.</summary>
    /// <param name="client">The client.</param>
    /// <param name="audioContent">The single audio content to be transcribed.</param>
    /// <param name="options">The audio transcription options to configure the request.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> to monitor for cancellation requests. The default is <see cref="CancellationToken.None"/>.</param>
    /// <returns>The transcriptions generated by the client.</returns>
    public static IAsyncEnumerable<StreamingAudioTranscriptionUpdate> TranscribeStreamingAsync(
        this IAudioTranscriptionClient client,
        DataContent audioContent,
        AudioTranscriptionOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        IEnumerable<DataContent> audioContents = [audioContent];
        return Throw.IfNull(client)
            .TranscribeStreamingAsync(
                [audioContents.ToAsyncEnumerableAsync()],
                options,
                cancellationToken);
    }

    private static async IAsyncEnumerable<T> ToAsyncEnumerableAsync<T>(this IEnumerable<T> source)
    {
        await Task.CompletedTask.ConfigureAwait(false);

        foreach (var item in source)
        {
            yield return item;
        }
    }
}
