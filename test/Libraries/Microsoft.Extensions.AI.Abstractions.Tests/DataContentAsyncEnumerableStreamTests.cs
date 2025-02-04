// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Microsoft.Extensions.AI;

public class DataContentAsyncEnumerableStreamTests
{
    [Fact]
    public void Constructor_InvalidArgs_Throws()
    {
        Assert.Throws<ArgumentNullException>("source", () => new DataContentAsyncEnumerableStream(null!));
    }

    [Fact]
    public async Task ReadAsync_EmptySource_ReturnsEmpty()
    {
        var source = GetAsyncEnumerable(Array.Empty<byte[]>());
        var stream = new DataContentAsyncEnumerableStream(source);

        byte[] buffer = new byte[10];
        int bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);

        Assert.Equal(0, bytesRead);
    }

    [Fact]
    public async Task ReadAsync_SingleChunk_ReturnsChunk()
    {
        var source = GetAsyncEnumerable(new[] { new byte[] { 1, 2, 3, 4, 5 } });
        var stream = new DataContentAsyncEnumerableStream(source);

        byte[] buffer = new byte[10];
        int bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);

        Assert.Equal(5, bytesRead);
        Assert.Equal(new byte[] { 1, 2, 3, 4, 5, 0, 0, 0, 0, 0 }, buffer);
    }

    [Fact]
    public async Task ReadAsync_MultipleChunks_ReturnsConcatenatedChunks()
    {
        var source = GetAsyncEnumerable(new[]
        {
            new byte[] { 1, 2, 3 },
            new byte[] { 4, 5, 6 },
            new byte[] { 7, 8, 9 }
        });
        var stream = new DataContentAsyncEnumerableStream(source);

        byte[] buffer = new byte[10];
        int bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);

        Assert.Equal(9, bytesRead);
        Assert.Equal(new byte[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 0 }, buffer);
    }

    [Fact]
    public async Task ReadAsync_BufferTooSmall_ReturnsPartialChunk()
    {
        var source = GetAsyncEnumerable(new[] { new byte[] { 1, 2, 3, 4, 5 } });
        var stream = new DataContentAsyncEnumerableStream(source);

        byte[] buffer = new byte[3];
        int bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);

        Assert.Equal(3, bytesRead);
        Assert.Equal(new byte[] { 1, 2, 3 }, buffer);
    }

    [Fact]
    public async Task ReadAsync_BufferTooSmall_MultipleReads()
    {
        var source = GetAsyncEnumerable(new[] { new byte[] { 1, 2, 3, 4, 5 } });
        var stream = new DataContentAsyncEnumerableStream(source);

        byte[] buffer = new byte[3];
        int bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);

        Assert.Equal(3, bytesRead);
        Assert.Equal(new byte[] { 1, 2, 3 }, buffer);

        bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);

        Assert.Equal(2, bytesRead);
        Assert.Equal(new byte[] { 4, 5, 3 }, buffer);
    }

    [Fact]
    public async Task ReadAsync_WithFirstDataContent_ReturnsFirstDataContentThenSource()
    {
        var firstDataContent = new byte[] { 0, 0, 0, 0, 0 };
        var source = GetAsyncEnumerable(new[] { new byte[] { 1, 2, 3, 4, 5 } });
        var stream = new DataContentAsyncEnumerableStream(source, firstDataContent);

        byte[] buffer = new byte[10];
        int bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);

        Assert.Equal(10, bytesRead);
        Assert.Equal(new byte[] { 0, 0, 0, 0, 0, 1, 2, 3, 4, 5 }, buffer);
    }

    [Fact]
    public void Constructor_WithFirstDataContent_InitializesCorrectly()
    {
        var firstDataContent = new byte[] { 0, 0, 0, 0, 0 };
        var source = GetAsyncEnumerable(new[] { new byte[] { 1, 2, 3, 4, 5 } });
        var stream = new DataContentAsyncEnumerableStream(source, firstDataContent);

        Assert.NotNull(stream);
    }

    [Fact]
    public async Task ReadAsync_EmptySource_WithFirstDataContent_ReturnsFirstDataContent()
    {
        var firstDataContent = new byte[] { 0, 0, 0, 0, 0 };
        var source = GetAsyncEnumerable(Array.Empty<byte[]>());
        var stream = new DataContentAsyncEnumerableStream(source, firstDataContent);

        byte[] buffer = new byte[10];
        int bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);

        Assert.Equal(5, bytesRead);
        Assert.Equal(new byte[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 }, buffer);
    }

    private static async IAsyncEnumerable<byte[]> GetAsyncEnumerable(IEnumerable<byte[]> chunks)
    {
        foreach (var chunk in chunks)
        {
            await Task.Yield();
            yield return chunk;
        }
    }
}
