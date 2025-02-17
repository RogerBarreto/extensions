// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Shared.Diagnostics;

namespace Microsoft.Extensions.AI;

/// <summary>Provides extensions for configuring <see cref="LoggingAudioTranscriptionClient"/> instances.</summary>
public static class LoggingAudioTranscriptionClientExtensions
{
    /// <summary>Adds logging to the audio transcription client pipeline.</summary>
    /// <param name="builder">The <see cref="AudioTranscriptionClientBuilder"/>.</param>
    /// <param name="loggerFactory">
    /// An optional <see cref="ILoggerFactory"/> used to create a logger with which logging should be performed.
    /// If not supplied, a required instance will be resolved from the service provider.
    /// </param>
    /// <param name="configure">An optional callback that can be used to configure the <see cref="LoggingAudioTranscriptionClient"/> instance.</param>
    /// <returns>The <paramref name="builder"/>.</returns>
    public static AudioTranscriptionClientBuilder UseLogging(
        this AudioTranscriptionClientBuilder builder,
        ILoggerFactory? loggerFactory = null,
        Action<LoggingAudioTranscriptionClient>? configure = null)
    {
        _ = Throw.IfNull(builder);

        return builder.Use((innerClient, services) =>
        {
            loggerFactory ??= services.GetRequiredService<ILoggerFactory>();

            // If the factory we resolve is for the null logger, the LoggingAudioTranscriptionClient will end up
            // being an expensive nop, so skip adding it and just return the inner client.
            if (loggerFactory == NullLoggerFactory.Instance)
            {
                return innerClient;
            }

            var audioClient = new LoggingAudioTranscriptionClient(innerClient, loggerFactory.CreateLogger(typeof(LoggingAudioTranscriptionClient)));
            configure?.Invoke(audioClient);
            return audioClient;
        });
    }
}
