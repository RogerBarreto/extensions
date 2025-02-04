// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Text.Json;
using Xunit;

namespace Microsoft.Extensions.AI;

public class StreamingAudioTranscriptionUpdateTests
{
    [Fact]
    public void Constructor_PropsDefaulted()
    {
        StreamingAudioTranscriptionUpdate update = new();
        Assert.Null(update.AuthorName);
        Assert.Equal(AudioTranscriptionUpdateKind.Transcribed, update.Kind);
        Assert.Null(update.Text);
        Assert.Empty(update.Contents);
        Assert.Null(update.RawRepresentation);
        Assert.Null(update.AdditionalProperties);
        Assert.Null(update.CompletionId);
        Assert.Null(update.CreatedAt);
        Assert.Null(update.FinishReason);
        Assert.Equal(0, update.ChoiceIndex);
        Assert.Equal(string.Empty, update.ToString());
    }

    [Fact]
    public void Properties_Roundtrip()
    {
        StreamingAudioTranscriptionUpdate update = new();

        Assert.Null(update.AuthorName);
        update.AuthorName = "author";
        Assert.Equal("author", update.AuthorName);

        Assert.Equal(AudioTranscriptionUpdateKind.Transcribed, update.Kind);
        update.Kind = AudioTranscriptionUpdateKind.SessionOpen;
        Assert.Equal(AudioTranscriptionUpdateKind.SessionOpen, update.Kind);

        Assert.Empty(update.Contents);
        update.Contents.Add(new TextContent("text"));
        Assert.Single(update.Contents);
        Assert.Equal("text", update.Text);
        Assert.Same(update.Contents, update.Contents);
        IList<AIContent> newList = new List<AIContent> { new TextContent("text") };
        update.Contents = newList;
        Assert.Same(newList, update.Contents);
        update.Contents = null;
        Assert.NotNull(update.Contents);
        Assert.Empty(update.Contents);

        Assert.Null(update.Text);
        update.Text = "text";
        Assert.Equal("text", update.Text);

        Assert.Null(update.RawRepresentation);
        object raw = new();
        update.RawRepresentation = raw;
        Assert.Same(raw, update.RawRepresentation);

        Assert.Null(update.AdditionalProperties);
        AdditionalPropertiesDictionary props = new() { ["key"] = "value" };
        update.AdditionalProperties = props;
        Assert.Same(props, update.AdditionalProperties);

        Assert.Null(update.CompletionId);
        update.CompletionId = "id";
        Assert.Equal("id", update.CompletionId);

        Assert.Null(update.CreatedAt);
        update.CreatedAt = new DateTimeOffset(2022, 1, 1, 0, 0, 0, TimeSpan.Zero);
        Assert.Equal(new DateTimeOffset(2022, 1, 1, 0, 0, 0, TimeSpan.Zero), update.CreatedAt);

        Assert.Equal(0, update.ChoiceIndex);
        update.ChoiceIndex = 42;
        Assert.Equal(42, update.ChoiceIndex);

        Assert.Null(update.FinishReason);
        update.FinishReason = AudioTranscriptionUpdateKind.Error;
        Assert.Equal(AudioTranscriptionUpdateKind.Error, update.FinishReason);
    }

    [Fact]
    public void Text_GetSet_UsesFirstTextContent()
    {
        StreamingAudioTranscriptionUpdate update = new()
        {
            Kind = AudioTranscriptionUpdateKind.SessionOpen,
            Contents =
            [
                new DataContent("http://localhost/audio"),
                new DataContent("http://localhost/image"),
                new TextContent("text-1"),
                new TextContent("text-2"),
            ],
        };

        TextContent textContent = Assert.IsType<TextContent>(update.Contents[2]);
        Assert.Equal("text-1", textContent.Text);
        Assert.Equal("text-1", update.Text);
        Assert.Equal("text-1text-2", update.ToString());

        update.Text = "text-3";
        Assert.Equal("text-3", update.Text);
        Assert.Equal("text-3", update.Text);
        Assert.Same(textContent, update.Contents[2]);
        Assert.Equal("text-3text-2", update.ToString());
    }

    [Fact]
    public void Text_Set_AddsTextMessageToEmptyList()
    {
        StreamingAudioTranscriptionUpdate update = new()
        {
            Kind = AudioTranscriptionUpdateKind.SessionOpen,
        };
        Assert.Empty(update.Contents);

        update.Text = "text-1";
        Assert.Equal("text-1", update.Text);

        Assert.Single(update.Contents);
        TextContent textContent = Assert.IsType<TextContent>(update.Contents[0]);
        Assert.Equal("text-1", textContent.Text);
    }

    [Fact]
    public void Text_Set_AddsTextMessageToListWithNoText()
    {
        StreamingAudioTranscriptionUpdate update = new()
        {
            Contents =
            [
                new DataContent("http://localhost/audio"),
                new DataContent("http://localhost/image"),
            ]
        };
        Assert.Equal(2, update.Contents.Count);

        update.Text = "text-1";
        Assert.Equal("text-1", update.Text);
        Assert.Equal(3, update.Contents.Count);

        update.Text = "text-2";
        Assert.Equal("text-2", update.Text);
        Assert.Equal(3, update.Contents.Count);

        update.Contents.RemoveAt(2);
        Assert.Equal(2, update.Contents.Count);

        update.Text = "text-3";
        Assert.Equal("text-3", update.Text);
        Assert.Equal(3, update.Contents.Count);
    }

    [Fact]
    public void JsonSerialization_Roundtrips()
    {
        StreamingAudioTranscriptionUpdate original = new()
        {
            AuthorName = "author",
            Kind = AudioTranscriptionUpdateKind.SessionOpen,
            Contents =
            [
                new TextContent("text-1"),
                new DataContent("http://localhost/image"),
                new DataContent("data"u8.ToArray()),
                new TextContent("text-2"),
            ],
            RawRepresentation = new object(),
            CompletionId = "id",
            CreatedAt = new DateTimeOffset(2022, 1, 1, 0, 0, 0, TimeSpan.Zero),
            FinishReason = AudioTranscriptionUpdateKind.Error,
            AdditionalProperties = new() { ["key"] = "value" },
            ChoiceIndex = 42,
        };

        string json = JsonSerializer.Serialize(original, TestJsonSerializerContext.Default.StreamingAudioTranscriptionUpdate);

        StreamingAudioTranscriptionUpdate? result = JsonSerializer.Deserialize(json, TestJsonSerializerContext.Default.StreamingAudioTranscriptionUpdate);

        Assert.NotNull(result);
        Assert.Equal(4, result.Contents.Count);

        Assert.IsType<TextContent>(result.Contents[0]);
        Assert.Equal("text-1", ((TextContent)result.Contents[0]).Text);

        Assert.IsType<DataContent>(result.Contents[1]);
        Assert.Equal("http://localhost/image", ((DataContent)result.Contents[1]).Uri);

        Assert.IsType<DataContent>(result.Contents[2]);
        Assert.Equal("data"u8.ToArray(), ((DataContent)result.Contents[2]).Data?.ToArray());

        Assert.IsType<TextContent>(result.Contents[3]);
        Assert.Equal("text-2", ((TextContent)result.Contents[3]).Text);

        Assert.Equal("author", result.AuthorName);
        Assert.Equal(AudioTranscriptionUpdateKind.SessionOpen, result.Kind);
        Assert.Equal("id", result.CompletionId);
        Assert.Equal(new DateTimeOffset(2022, 1, 1, 0, 0, 0, TimeSpan.Zero), result.CreatedAt);
        Assert.Equal(AudioTranscriptionUpdateKind.Error, result.FinishReason);
        Assert.Equal(42, result.ChoiceIndex);

        Assert.NotNull(result.AdditionalProperties);
        Assert.Single(result.AdditionalProperties);
        Assert.True(result.AdditionalProperties.TryGetValue("key", out object? value));
        Assert.IsType<JsonElement>(value);
        Assert.Equal("value", ((JsonElement)value!).GetString());
    }

    [Fact]
    public void Kind_SetToExistingValues()
    {
        StreamingAudioTranscriptionUpdate update = new();

        update.Kind = AudioTranscriptionUpdateKind.SessionOpen;
        Assert.Equal(AudioTranscriptionUpdateKind.SessionOpen, update.Kind);

        update.Kind = AudioTranscriptionUpdateKind.Error;
        Assert.Equal(AudioTranscriptionUpdateKind.Error, update.Kind);

        update.Kind = AudioTranscriptionUpdateKind.Transcribing;
        Assert.Equal(AudioTranscriptionUpdateKind.Transcribing, update.Kind);

        update.Kind = AudioTranscriptionUpdateKind.Transcribed;
        Assert.Equal(AudioTranscriptionUpdateKind.Transcribed, update.Kind);

        update.Kind = AudioTranscriptionUpdateKind.SessionClose;
        Assert.Equal(AudioTranscriptionUpdateKind.SessionClose, update.Kind);
    }

    [Fact]
    public void Kind_SetToRandomValue()
    {
        StreamingAudioTranscriptionUpdate update = new();

        AudioTranscriptionUpdateKind randomKind = new("any");
        update.Kind = randomKind;
        Assert.Equal(randomKind, update.Kind);
    }
}
