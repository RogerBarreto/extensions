// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.Extensions.AI;
using Microsoft.Shared.Diagnostics;

namespace Microsoft.Extensions.AI;

/// <summary>Provides extension methods for working with <see cref="IAudioTranscriptionClient"/> in the context of <see cref="AudioTranscriptionClientBuilder"/>.</summary>
public static class AudioTranscriptionClientBuilderAudioTranscriptionClientExtensions
{
    /// <summary>Creates a new <see cref="AudioTranscriptionClientBuilder"/> using <paramref name="innerClient"/> as its inner client.</summary>
    /// <param name="innerClient">The client to use as the inner client.</param>
    /// <returns>The new <see cref="AudioTranscriptionClientBuilder"/> instance.</returns>
    /// <remarks>
    /// This method is equivalent to using the <see cref="AudioTranscriptionClientBuilder"/> constructor directly,
    /// specifying <paramref name="innerClient"/> as the inner client.
    /// </remarks>
    public static AudioTranscriptionClientBuilder AsBuilder(this IAudioTranscriptionClient innerClient)
    {
        _ = Throw.IfNull(innerClient);

        return new AudioTranscriptionClientBuilder(innerClient);
    }
}
