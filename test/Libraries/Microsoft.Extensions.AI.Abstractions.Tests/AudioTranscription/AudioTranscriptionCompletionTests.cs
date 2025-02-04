// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Text.Json;
using Xunit;

namespace Microsoft.Extensions.AI;

public class AudioTranscriptionCompletionTests
{
    [Fact]
    public void Constructor_InvalidArgs_Throws()
    {
        Assert.Throws<ArgumentNullException>("choice", () => new AudioTranscriptionCompletion((AudioTranscriptionChoice)null!));
        Assert.Throws<ArgumentNullException>("choices", () => new AudioTranscriptionCompletion((IList<AudioTranscriptionChoice>)null!));
    }

    [Fact]
    public void Constructor_Choice_Roundtrips()
    {
        AudioTranscriptionChoice choice = new();

        AudioTranscriptionCompletion completion = new(choice);
        Assert.Same(choice, completion.Message);
        Assert.Same(choice, Assert.Single(completion.Choices));
    }

    [Fact]
    public void Constructor_Choices_Roundtrips()
    {
        List<AudioTranscriptionChoice> choices =
        [
            new AudioTranscriptionChoice(),
            new AudioTranscriptionChoice(),
            new AudioTranscriptionChoice(),
        ];

        AudioTranscriptionCompletion completion = new(choices);
        Assert.Same(choices, completion.Choices);
        Assert.Equal(3, choices.Count);
    }

    [Fact]
    public void Message_EmptyChoices_Throws()
    {
        AudioTranscriptionCompletion completion = new([]);

        Assert.Empty(completion.Choices);
        Assert.Throws<InvalidOperationException>(() => completion.Message);
    }

    [Fact]
    public void Message_SingleChoice_Returned()
    {
        AudioTranscriptionChoice choice = new();
        AudioTranscriptionCompletion completion = new([choice]);

        Assert.Same(choice, completion.Message);
        Assert.Same(choice, completion.Choices[0]);
    }

    [Fact]
    public void Message_MultipleChoices_ReturnsFirst()
    {
        AudioTranscriptionChoice first = new();
        AudioTranscriptionCompletion completion = new([
            first,
            new AudioTranscriptionChoice(),
        ]);

        Assert.Same(first, completion.Message);
        Assert.Same(first, completion.Choices[0]);
    }

    [Fact]
    public void Choices_SetNull_Throws()
    {
        AudioTranscriptionCompletion completion = new([]);
        Assert.Throws<ArgumentNullException>("value", () => completion.Choices = null!);
    }

    [Fact]
    public void Properties_Roundtrip()
    {
        AudioTranscriptionCompletion completion = new([]);

        Assert.Null(completion.CompletionId);
        completion.CompletionId = "id";
        Assert.Equal("id", completion.CompletionId);

        Assert.Null(completion.ModelId);
        completion.ModelId = "modelId";
        Assert.Equal("modelId", completion.ModelId);

        Assert.Null(completion.CreatedAt);
        completion.CreatedAt = new DateTimeOffset(2022, 1, 1, 0, 0, 0, TimeSpan.Zero);
        Assert.Equal(new DateTimeOffset(2022, 1, 1, 0, 0, 0, TimeSpan.Zero), completion.CreatedAt);

        Assert.Null(completion.FinishReason);
        completion.FinishReason = AudioTranscriptionUpdateKind.ContentFilter;
        Assert.Equal(AudioTranscriptionUpdateKind.ContentFilter, completion.FinishReason);

        Assert.Null(completion.Usage);
        UsageDetails usage = new();
        completion.Usage = usage;
        Assert.Same(usage, completion.Usage);

        Assert.Null(completion.RawRepresentation);
        object raw = new();
        completion.RawRepresentation = raw;
        Assert.Same(raw, completion.RawRepresentation);

        Assert.Null(completion.AdditionalProperties);
        AdditionalPropertiesDictionary additionalProps = [];
        completion.AdditionalProperties = additionalProps;
        Assert.Same(additionalProps, completion.AdditionalProperties);

        List<AudioTranscriptionChoice> newChoices = [new AudioTranscriptionChoice(), new AudioTranscriptionChoice()];
        completion.Choices = newChoices;
        Assert.Same(newChoices, completion.Choices);
    }

    [Fact]
    public void JsonSerialization_Roundtrips()
    {
        AudioTranscriptionCompletion original = new(
        [
            new AudioTranscriptionChoice("Choice1"),
            new AudioTranscriptionChoice("Choice2"),
            new AudioTranscriptionChoice("Choice3"),
            new AudioTranscriptionChoice("Choice4"),
        ])
        {
            CompletionId = "id",
            ModelId = "modelId",
            CreatedAt = new DateTimeOffset(2022, 1, 1, 0, 0, 0, TimeSpan.Zero),
            FinishReason = AudioTranscriptionUpdateKind.ContentFilter,
            Usage = new UsageDetails(),
            RawRepresentation = new(),
            AdditionalProperties = new() { ["key"] = "value" },
        };

        string json = JsonSerializer.Serialize(original, TestJsonSerializerContext.Default.AudioTranscriptionCompletion);

        AudioTranscriptionCompletion? result = JsonSerializer.Deserialize(json, TestJsonSerializerContext.Default.AudioTranscriptionCompletion);

        Assert.NotNull(result);
        Assert.Equal(4, result.Choices.Count);

        for (int i = 0; i < original.Choices.Count; i++)
        {
            Assert.Equal($"Choice{i + 1}", result.Choices[i].Text);
        }

        Assert.Equal("id", result.CompletionId);
        Assert.Equal("modelId", result.ModelId);
        Assert.Equal(new DateTimeOffset(2022, 1, 1, 0, 0, 0, TimeSpan.Zero), result.CreatedAt);
        Assert.Equal(AudioTranscriptionUpdateKind.ContentFilter, result.FinishReason);
        Assert.NotNull(result.Usage);

        Assert.NotNull(result.AdditionalProperties);
        Assert.Single(result.AdditionalProperties);
        Assert.True(result.AdditionalProperties.TryGetValue("key", out object? value));
        Assert.IsType<JsonElement>(value);
        Assert.Equal("value", ((JsonElement)value!).GetString());
    }

    [Fact]
    public void ToString_OneChoice_OutputsAudioTranscriptionChoiceToString()
    {
        AudioTranscriptionCompletion completion = new(
        [
            new AudioTranscriptionChoice("This is a test." + Environment.NewLine + "It's multiple lines.")
        ]);

        Assert.Equal(completion.Choices[0].Text, completion.ToString());
    }

    [Fact]
    public void ToString_MultipleChoices_OutputsAllChoicesWithPrefix()
    {
        AudioTranscriptionCompletion completion = new(
        [
            new AudioTranscriptionChoice("This is a test." + Environment.NewLine + "It's multiple lines."),
            new AudioTranscriptionChoice("So is" + Environment.NewLine + " this."),
            new AudioTranscriptionChoice("And this."),
        ]);

        Assert.Equal(
            "Choice 0:" + Environment.NewLine +
            completion.Choices[0] + Environment.NewLine + Environment.NewLine +

            "Choice 1:" + Environment.NewLine +
            completion.Choices[1] + Environment.NewLine + Environment.NewLine +

            "Choice 2:" + Environment.NewLine +
            completion.Choices[2],

            completion.ToString());
    }

    [Fact]
    public void ToStreamingAudioTranscriptionUpdates_SingleChoice()
    {
        AudioTranscriptionCompletion completion = new(new AudioTranscriptionChoice("Text"))
        {
            CompletionId = "12345",
            ModelId = "someModel",
            FinishReason = AudioTranscriptionUpdateKind.ContentFilter,
            CreatedAt = new DateTimeOffset(2024, 11, 10, 9, 20, 0, TimeSpan.Zero),
            AdditionalProperties = new() { ["key1"] = "value1", ["key2"] = 42 },
        };

        StreamingAudioTranscriptionUpdate[] updates = completion.ToStreamingAudioTranscriptionUpdates();
        Assert.NotNull(updates);
        Assert.Equal(2, updates.Length);

        StreamingAudioTranscriptionUpdate update0 = updates[0];
        Assert.Equal("12345", update0.CompletionId);
        Assert.Equal("someModel", update0.ModelId);
        Assert.Equal(AudioTranscriptionUpdateKind.ContentFilter, update0.FinishReason);
        Assert.Equal(new DateTimeOffset(2024, 11, 10, 9, 20, 0, TimeSpan.Zero), update0.CreatedAt);
        Assert.Equal("Text", update0.Text);

        StreamingAudioTranscriptionUpdate update1 = updates[1];
        Assert.Equal("value1", update1.AdditionalProperties?["key1"]);
        Assert.Equal(42, update1.AdditionalProperties?["key2"]);
    }

    [Fact]
    public void ToStreamingAudioTranscriptionUpdates_MultiChoice()
    {
        AudioTranscriptionCompletion completion = new(
        [
            new AudioTranscriptionChoice(
            [
                new TextContent("Hello, "),
                new DataContent("http://localhost/image.png", mediaType: "image/png"),
                new TextContent("world!"),
            ])
            {
                AdditionalProperties = new() { ["choice1Key"] = "choice1Value" },
            },

            new AudioTranscriptionChoice(
            [
                new FunctionCallContent("call123", "name"),
                new FunctionResultContent("call123", "name", 42),
            ])
            {
                AdditionalProperties = new() { ["choice2Key"] = "choice2Value" },
            },
        ])
        {
            CompletionId = "12345",
            ModelId = "someModel",
            FinishReason = AudioTranscriptionUpdateKind.ContentFilter,
            CreatedAt = new DateTimeOffset(2024, 11, 10, 9, 20, 0, TimeSpan.Zero),
            AdditionalProperties = new() { ["key1"] = "value1", ["key2"] = 42 },
            Usage = new UsageDetails { TotalTokenCount = 123 },
        };

        StreamingAudioTranscriptionUpdate[] updates = completion.ToStreamingAudioTranscriptionUpdates();
        Assert.NotNull(updates);
        Assert.Equal(3, updates.Length);

        StreamingAudioTranscriptionUpdate update0 = updates[0];
        Assert.Equal("12345", update0.CompletionId);
        Assert.Equal("someModel", update0.ModelId);
        Assert.Equal(AudioTranscriptionUpdateKind.ContentFilter, update0.FinishReason);
        Assert.Equal(new DateTimeOffset(2024, 11, 10, 9, 20, 0, TimeSpan.Zero), update0.CreatedAt);
        Assert.Equal("Hello, ", Assert.IsType<TextContent>(update0.Contents[0]).Text);
        Assert.Equal("image/png", Assert.IsType<DataContent>(update0.Contents[1]).MediaType);
        Assert.Equal("world!", Assert.IsType<TextContent>(update0.Contents[2]).Text);
        Assert.Equal("choice1Value", update0.AdditionalProperties?["choice1Key"]);

        StreamingAudioTranscriptionUpdate update1 = updates[1];
        Assert.Equal("12345", update1.CompletionId);
        Assert.Equal("someModel", update1.ModelId);
        Assert.Equal(AudioTranscriptionUpdateKind.ContentFilter, update1.FinishReason);
        Assert.Equal(new DateTimeOffset(2024, 11, 10, 9, 20, 0, TimeSpan.Zero), update1.CreatedAt);
        Assert.IsType<FunctionCallContent>(update1.Contents[0]);
        Assert.IsType<FunctionResultContent>(update1.Contents[1]);
        Assert.Equal("choice2Value", update1.AdditionalProperties?["choice2Key"]);

        StreamingAudioTranscriptionUpdate update2 = updates[2];
        Assert.Equal("value1", update2.AdditionalProperties?["key1"]);
        Assert.Equal(42, update2.AdditionalProperties?["key2"]);
        Assert.Equal(123, Assert.IsType<UsageContent>(Assert.Single(update2.Contents)).Details.TotalTokenCount);
    }
}
