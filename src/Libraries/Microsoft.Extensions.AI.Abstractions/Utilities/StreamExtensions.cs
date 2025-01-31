// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading;
using Microsoft.Shared.Diagnostics;

namespace Microsoft.Extensions.AI;

public static class StreamExtensions
{
#pragma warning disable VSTHRD200 // Use "Async" suffix for async methods
    public static async IAsyncEnumerable<T> ToAsyncEnumerable<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] T>(
        this Stream audioStream,
        string? mediaType = null,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
        where T : DataContent
    {
        _ = Throw.IfNull(audioStream);

#if NET8_0_OR_GREATER
        Memory<byte> buffer = new byte[4096];
        while ((await audioStream.ReadAsync(buffer, cancellationToken).ConfigureAwait(false)) > 0)
#else
        var buffer = new byte[4096];
        while ((await audioStream.ReadAsync(buffer, 0, buffer.Length, cancellationToken).ConfigureAwait(false)) > 0)
#endif
        {
            yield return (T)Activator.CreateInstance(typeof(T), [(ReadOnlyMemory<byte>)buffer, mediaType])!;
        }
    }
#pragma warning restore VSTHRD200 // Use "Async" suffix for async methods
}
