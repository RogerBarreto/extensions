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
        Assert.Equal("abc", new AudioTranscriptionUpdateKind("abc").Value);
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
        Assert.True(new AudioTranscriptionUpdateKind("abc").Equals(new AudioTranscriptionUpdateKind("ABC")));
        Assert.True(new AudioTranscriptionUpdateKind("abc").Equals((object)new AudioTranscriptionUpdateKind("ABC")));
        Assert.True(new AudioTranscriptionUpdateKind("abc") == new AudioTranscriptionUpdateKind("ABC"));
        Assert.False(new AudioTranscriptionUpdateKind("abc") != new AudioTranscriptionUpdateKind("ABC"));

        Assert.False(new AudioTranscriptionUpdateKind("abc").Equals(new AudioTranscriptionUpdateKind("def")));
        Assert.False(new AudioTranscriptionUpdateKind("abc").Equals((object)new AudioTranscriptionUpdateKind("def")));
        Assert.False(new AudioTranscriptionUpdateKind("abc").Equals(null));
        Assert.False(new AudioTranscriptionUpdateKind("abc").Equals("abc"));
        Assert.False(new AudioTranscriptionUpdateKind("abc") == new AudioTranscriptionUpdateKind("def"));
        Assert.True(new AudioTranscriptionUpdateKind("abc") != new AudioTranscriptionUpdateKind("def"));

        Assert.Equal(new AudioTranscriptionUpdateKind("abc").GetHashCode(), new AudioTranscriptionUpdateKind("abc").GetHashCode());
        Assert.Equal(new AudioTranscriptionUpdateKind("abc").GetHashCode(), new AudioTranscriptionUpdateKind("ABC").GetHashCode());
        Assert.NotEqual(new AudioTranscriptionUpdateKind("abc").GetHashCode(), new AudioTranscriptionUpdateKind("def").GetHashCode()); // not guaranteed
    }

    [Fact]
    public void Singletons_UseKnownValues()
    {
        Assert.Equal("assistant", AudioTranscriptionUpdateKind.Assistant.Value);
        Assert.Equal("system", AudioTranscriptionUpdateKind.System.Value);
        Assert.Equal("user", AudioTranscriptionUpdateKind.User.Value);
    }

    [Fact]
    public void JsonSerialization_Roundtrips()
    {
        AudioTranscriptionUpdateKind kind = new("abc");
        string? json = JsonSerializer.Serialize(kind, TestJsonSerializerContext.Default.AudioTranscriptionUpdateKind);
        Assert.Equal("\"abc\"", json);

        AudioTranscriptionUpdateKind? result = JsonSerializer.Deserialize(json, TestJsonSerializerContext.Default.AudioTranscriptionUpdateKind);
        Assert.Equal(kind, result);
    }
}
