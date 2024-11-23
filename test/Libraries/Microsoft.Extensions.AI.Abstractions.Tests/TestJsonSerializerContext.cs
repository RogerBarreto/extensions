// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
#if NET
using System.Buffers;
#endif
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace Microsoft.Extensions.AI;

[JsonSourceGenerationOptions(
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    NumberHandling = JsonNumberHandling.AllowReadingFromString,
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    Converters = new[] { typeof(JsonStringBooleanConverter) },
    UseStringEnumConverter = true)]
[JsonSerializable(typeof(ChatCompletion))]
[JsonSerializable(typeof(StreamingChatCompletionUpdate))]
[JsonSerializable(typeof(ChatOptions))]
[JsonSerializable(typeof(EmbeddingGenerationOptions))]
[JsonSerializable(typeof(Dictionary<string, object?>))]
[JsonSerializable(typeof(int[]))] // Used in ChatMessageContentTests
[JsonSerializable(typeof(Embedding))] // Used in EmbeddingTests
[JsonSerializable(typeof(Dictionary<string, JsonDocument>))] // Used in Content tests
[JsonSerializable(typeof(Dictionary<string, JsonNode>))] // Used in Content tests
[JsonSerializable(typeof(Dictionary<string, string>))] // Used in Content tests
[JsonSerializable(typeof(ReadOnlyDictionary<string, string>))] // Used in Content tests
[JsonSerializable(typeof(DayOfWeek[]))] // Used in Content tests
[JsonSerializable(typeof(Guid))] // Used in Content tests
[JsonSerializable(typeof(decimal))] // Used in Content tests
[JsonSerializable(typeof(bool))] // Used in Content tests
[JsonSerializable(typeof(float))] // Used in Content tests
internal sealed partial class TestJsonSerializerContext : JsonSerializerContext
{
    private sealed class JsonStringBooleanConverter : JsonConverter<bool>
    {
        public override bool Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            bool? boolResult = null;

            if (reader.TokenType == JsonTokenType.String)
            {
#if NET
                static int GetValueLength(ref Utf8JsonReader reader)
                    => reader.HasValueSequence
                        ? checked((int)reader.ValueSequence.Length)
                        : reader.ValueSpan.Length;

                const int StackallocByteThreshold = 256;
                const int StackallocCharThreshold = StackallocByteThreshold / 2;

                int bufferLength = GetValueLength(ref reader);
                char[]? rentedBuffer = null;
                if (bufferLength > StackallocCharThreshold)
                {
                    rentedBuffer = ArrayPool<char>.Shared.Rent(bufferLength);
                }

                Span<char> charBuffer = rentedBuffer ?? stackalloc char[StackallocCharThreshold];

                int actualLength = reader.CopyString(charBuffer);
                ReadOnlySpan<char> stringSpan = charBuffer.Slice(0, actualLength);

                if (bool.TryParse(stringSpan, out var result))
                {
                    boolResult = result;
                }

                if (rentedBuffer != null)
                {
                    charBuffer.Clear();
                    ArrayPool<char>.Shared.Return(rentedBuffer);
                }
#else
                if (bool.TryParse(reader.GetString(), out var result))
                {
                    boolResult = result;
                }
#endif
            }

            return boolResult ?? reader.GetBoolean();
        }

        public override void Write(Utf8JsonWriter writer, bool value, JsonSerializerOptions options)
        {
            writer.WriteBooleanValue(value);
        }
    }
}
