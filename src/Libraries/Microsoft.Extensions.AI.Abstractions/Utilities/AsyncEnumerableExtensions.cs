// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections.Generic;
using System.IO;
using System.Threading;
using Microsoft.Shared.Diagnostics;

namespace Microsoft.Extensions.AI;

public static class AsyncEnumerableExtensions
{
    public static Stream ToStream<T>(this IAsyncEnumerable<T> stream, T? firstChunk = null, CancellationToken cancellationToken = default)
        where T : DataContent
        => new DataContentAsyncEnumerableStream<T>(Throw.IfNull(stream), firstChunk, cancellationToken);
}
