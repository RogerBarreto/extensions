// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Text.Json;
using Xunit;

namespace Microsoft.Extensions.AI;

public class AudioTranscriptionUpdateKindTests
{
    [Fact]
    public void Constructor_Value_Roundtrips()
    {
        var kind = new AudioTranscriptionUpdateKind("abc");
        Assert.Equal("abc", kind.Value);
    }

    [Fact]
    public void Constructor_NullOrWhiteSpace_Throws()
    {
        Assert.Throws<ArgumentNullException>("value", () => new AudioTranscriptionUpdateKind(null!));
        Assert.Throws<ArgumentException>("value", () => new AudioTranscriptionUpdateKind("  "));
    }

    [Fact]
    public void Equality_UsesOrdinalIgnoreCaseComparison()
    {
        var kind1 = new AudioTranscriptionUpdateKind("abc");
        var kind2 = new AudioTranscriptionUpdateKind("ABC");
        Assert.True(kind1.Equals(kind2));
        Assert.True(kind1.Equals((object)kind2));
        Assert.True(kind1 == kind2);
        Assert.False(kind1 != kind2);

        var kind3 = new AudioTranscriptionUpdateKind("def");
        Assert.False(kind1.Equals(kind3));
        Assert.False(kind1.Equals((object)kind3));
        Assert.False(kind1 == kind3);
        Assert.True(kind1 != kind3);

        Assert.Equal(kind1.GetHashCode(), new AudioTranscriptionUpdateKind("abc").GetHashCode());
        Assert.Equal(kind1.GetHashCode(), new AudioTranscriptionUpdateKind("ABC").GetHashCode());
    }

    [Fact]
    public void Singletons_UseKnownValues()
    {
        // These constants are defined in AudioTranscriptionUpdateKind
        Assert.Equal("sessionopen", AudioTranscriptionUpdateKind.SessionOpen.Value);
        Assert.Equal("error", AudioTranscriptionUpdateKind.Error.Value);
        Assert.Equal("transcribing", AudioTranscriptionUpdateKind.Transcribing.Value);
        Assert.Equal("transcribed", AudioTranscriptionUpdateKind.Transcribed.Value);
        Assert.Equal("sessionclose", AudioTranscriptionUpdateKind.SessionClose.Value);
    }

    [Fact]
    public void JsonSerialization_Roundtrips()
    {
        var kind = new AudioTranscriptionUpdateKind("abc");
        string json = JsonSerializer.Serialize(kind, TestJsonSerializerContext.Default.AudioTranscriptionUpdateKind);
        Assert.Equal("\"abc\"", json);

        var result = JsonSerializer.Deserialize<AudioTranscriptionUpdateKind>(json, TestJsonSerializerContext.Default.AudioTranscriptionUpdateKind);
        Assert.Equal(kind, result);
    }
}
