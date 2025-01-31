// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Extensions.AI;

#if !NET8_0_OR_GREATER
internal sealed class DataContentAsyncEnumerableStream<T> : Stream, IAsyncDisposable
#else
internal sealed class DataContentAsyncEnumerableStream<T> : Stream
#endif
    where T : DataContent
{
    private readonly IAsyncEnumerator<T> _enumerator;
    private bool _isCompleted;
    private byte[] _remainingData;
    private int _remainingDataOffset;
    private long _position;
    private T? _firstChunk;

    internal DataContentAsyncEnumerableStream(IAsyncEnumerable<T> asyncEnumerable, T? firstChunk = null, CancellationToken cancellationToken = default)
    {
        _enumerator = asyncEnumerable.GetAsyncEnumerator(cancellationToken);
        _remainingData = Array.Empty<byte>();
        _remainingDataOffset = 0;
        _position = 0;
        _firstChunk = firstChunk;
    }

    public override bool CanRead => true;
    public override bool CanSeek => false;
    public override bool CanWrite => false;
    public override long Length => throw new NotSupportedException();
    public override long Position
    {
        get => _position;
        set => throw new NotSupportedException();
    }

    public override void Flush() => throw new NotSupportedException();

    public override long Seek(long offset, SeekOrigin origin) =>
        throw new NotSupportedException();

    public override void SetLength(long value) =>
        throw new NotSupportedException();

    public override int Read(byte[] buffer, int offset, int count)
    {
        throw new NotSupportedException("Use ReadAsync instead for asynchronous reading.");
    }

#if NET8_0_OR_GREATER
    public override Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        => ReadAsync(buffer, cancellationToken).AsTask();
#else
    public override async Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
    {
        if (_isCompleted)
        {
            return 0;
        }

        int bytesRead = 0;

        while (bytesRead < count)
        {
            if (_remainingDataOffset < _remainingData.Length)
            {
                int bytesToCopy = Math.Min(count - bytesRead, _remainingData.Length - _remainingDataOffset);
                Array.Copy(_remainingData, _remainingDataOffset, buffer, offset + bytesRead, bytesToCopy);
                _remainingDataOffset += bytesToCopy;
                bytesRead += bytesToCopy;
                _position += bytesToCopy;
            }
            else
            {
                // Special case when the first chunk was skipped and needs to be read
                if (_position == 0 && _firstChunk is not null && _firstChunk.Data.HasValue)
                {
                    _remainingData = _firstChunk.Data.Value.ToArray();
                    _remainingDataOffset = 0;

                    continue;
                }

                if (!await _enumerator.MoveNextAsync().ConfigureAwait(false))
                {
                    _isCompleted = true;
                    break;
                }

                if (!_enumerator.Current.Data.HasValue)
                {
                    _isCompleted = true;
                    break;
                }

                _remainingData = _enumerator.Current.Data.Value.ToArray();
                _remainingDataOffset = 0;
            }
        }

        return bytesRead;
    }
#endif

#if NET8_0_OR_GREATER
    public override async ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default)
    {
        if (_isCompleted)
        {
            return 0;
        }

        int bytesRead = 0;
        int totalToRead = buffer.Length;

        while (bytesRead < totalToRead)
        {
            // If there's still data in the current chunk
            if (_remainingDataOffset < _remainingData.Length)
            {
                int bytesToCopy = Math.Min(totalToRead - bytesRead, _remainingData.Length - _remainingDataOffset);
                _remainingData.AsSpan(_remainingDataOffset, bytesToCopy)
                              .CopyTo(buffer.Span.Slice(bytesRead, bytesToCopy));

                _remainingDataOffset += bytesToCopy;
                bytesRead += bytesToCopy;
                _position += bytesToCopy;
            }
            else
            {
                // If the first chunk was never read, attempt to read it now
                if (_position == 0 && _firstChunk is not null && _firstChunk.Data.HasValue)
                {
                    _remainingData = _firstChunk.Data.Value.ToArray();
                    _remainingDataOffset = 0;
                    continue;
                }

                // Move to the next chunk in the async enumerator
                if (!await _enumerator.MoveNextAsync().ConfigureAwait(false) ||
                    !_enumerator.Current.Data.HasValue)
                {
                    _isCompleted = true;
                    break;
                }

                _remainingData = _enumerator.Current.Data.Value.ToArray();
                _remainingDataOffset = 0;
            }
        }

        return bytesRead;
    }
#endif

#if NET8_0_OR_GREATER
    public override async ValueTask DisposeAsync()
    {
        await _enumerator.DisposeAsync().ConfigureAwait(false);

        await base.DisposeAsync().ConfigureAwait(false);
    }
#else
    public async ValueTask DisposeAsync()
    {
        await _enumerator.DisposeAsync().ConfigureAwait(false);

        Dispose();
    }
#endif

    public override void Write(byte[] buffer, int offset, int count)
    {
        throw new NotSupportedException();
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            var task = Task.Run(_enumerator.DisposeAsync);
        }

        base.Dispose(disposing);
    }
}

