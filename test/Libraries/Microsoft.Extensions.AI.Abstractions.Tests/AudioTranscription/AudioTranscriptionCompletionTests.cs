// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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

        // The choice property returns the first (and only) choice.
        Assert.Same(choice, completion.FirstChoice);
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
    public void Transcription_EmptyChoices_Throws()
    {
        AudioTranscriptionCompletion completion = new([]);
        Assert.Empty(completion.Choices);
        Assert.Throws<InvalidOperationException>(() => completion.FirstChoice);
    }

    [Fact]
    public void Transcription_SingleChoice_Returned()
    {
        AudioTranscriptionChoice choice = new();
        AudioTranscriptionCompletion completion = new([choice]);
        Assert.Same(choice, completion.FirstChoice);
        Assert.Same(choice, completion.Choices[0]);
    }

    [Fact]
    public void Transcription_MultipleChoices_ReturnsFirst()
    {
        AudioTranscriptionChoice first = new();
        AudioTranscriptionCompletion completion = new([
            first,
            new AudioTranscriptionChoice(),
        ]);
        Assert.Same(first, completion.FirstChoice);
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

        // AudioTranscriptionCompletion does not support CreatedAt, FinishReason or Usage.
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

        Assert.NotNull(result.AdditionalProperties);
        Assert.Single(result.AdditionalProperties);
        Assert.True(result.AdditionalProperties.TryGetValue("key", out object? value));
        Assert.IsType<JsonElement>(value);
        Assert.Equal("value", ((JsonElement)value!).GetString());
    }

    [Fact]
    public void ToString_SingleChoice_OutputsChoiceText()
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

        StringBuilder sb = new();

        for (int i = 0; i < completion.Choices.Count; i++)
        {
            if (i > 0)
            {
                sb.AppendLine().AppendLine();
            }

            sb.Append("Choice ").Append(i).AppendLine(":").Append(completion.Choices[i].ToString());
        }

        string expected = sb.ToString();
        Assert.Equal(expected, completion.ToString());
    }

    [Fact]
    public void ToStreamingAudioTranscriptionUpdates_SingleChoice_ReturnsExpectedUpdates()
    {
        // Arrange: create a completion with one choice.
        AudioTranscriptionChoice choice = new("Text")
        {
            InputIndex = 0,
            StartTime = TimeSpan.FromSeconds(1),
            EndTime = TimeSpan.FromSeconds(2)
        };

        AudioTranscriptionCompletion completion = new(choice)
        {
            CompletionId = "12345",
            ModelId = "someModel",
            AdditionalProperties = new() { ["key1"] = "value1", ["key2"] = 42 },
        };

        // Act: convert to streaming updates.
        StreamingAudioTranscriptionUpdate[] updates = completion.ToStreamingAudioTranscriptionUpdates();

        // Filter out any null entries (if any).
        StreamingAudioTranscriptionUpdate[] nonNullUpdates = updates.Where(u => u is not null).ToArray();

        // Our implementation creates one update per choice plus an extra update if AdditionalProperties exists.
        Assert.Equal(2, nonNullUpdates.Length);

        StreamingAudioTranscriptionUpdate update0 = nonNullUpdates[0];
        Assert.Equal("12345", update0.CompletionId);
        Assert.Equal("someModel", update0.ModelId);
        Assert.Equal(AudioTranscriptionUpdateKind.Transcribed, update0.Kind);
        Assert.Equal(choice.Text, update0.Text);
        Assert.Equal(choice.InputIndex, update0.InputIndex);
        Assert.Equal(choice.StartTime, update0.StartTime);
        Assert.Equal(choice.EndTime, update0.EndTime);

        StreamingAudioTranscriptionUpdate updateExtra = nonNullUpdates[1];

        // The extra update carries the AdditionalProperties from the completion.
        Assert.Null(updateExtra.Text);
        Assert.Equal("value1", updateExtra.AdditionalProperties?["key1"]);
        Assert.Equal(42, updateExtra.AdditionalProperties?["key2"]);
    }

    [Fact]
    public void ToStreamingAudioTranscriptionUpdates_MultiChoice_ReturnsExpectedUpdates()
    {
        // Arrange: create two choices.
        AudioTranscriptionChoice choice1 = new(
        [
            new TextContent("Hello, "),
            new DataContent("http://localhost/image.png", mediaType: "image/png"),
            new TextContent("world!")
        ])
        {
            AdditionalProperties = new() { ["choice1Key"] = "choice1Value" },
            InputIndex = 0
        };

        AudioTranscriptionChoice choice2 = new(
        [
            new FunctionCallContent("call123", "name"),
            new FunctionResultContent("call123", "name", 42),
        ])
        {
            AdditionalProperties = new() { ["choice2Key"] = "choice2Value" },
            InputIndex = 1
        };

        AudioTranscriptionCompletion completion = new([choice1, choice2])
        {
            CompletionId = "12345",
            ModelId = "someModel",
            AdditionalProperties = new() { ["key1"] = "value1", ["key2"] = 42 },
        };

        // Act: convert to streaming updates.
        StreamingAudioTranscriptionUpdate[] updates = completion.ToStreamingAudioTranscriptionUpdates();
        StreamingAudioTranscriptionUpdate[] nonNullUpdates = updates.Where(u => u is not null).ToArray();

        // Two choices plus an extra update should yield 3 updates.
        Assert.Equal(3, nonNullUpdates.Length);

        // Validate update from first choice.
        StreamingAudioTranscriptionUpdate update0 = nonNullUpdates[0];
        Assert.Equal("12345", update0.CompletionId);
        Assert.Equal("someModel", update0.ModelId);
        Assert.Equal(AudioTranscriptionUpdateKind.Transcribed, update0.Kind);
        Assert.Equal("Hello, ", Assert.IsType<TextContent>(update0.Contents[0]).Text);
        Assert.Equal("image/png", Assert.IsType<DataContent>(update0.Contents[1]).MediaType);
        Assert.Equal("world!", Assert.IsType<TextContent>(update0.Contents[2]).Text);
        Assert.Equal(choice1.InputIndex, update0.InputIndex);
        Assert.Equal("choice1Value", update0.AdditionalProperties?["choice1Key"]);

        // Validate update from second choice.
        StreamingAudioTranscriptionUpdate update1 = nonNullUpdates[1];
        Assert.Equal("12345", update1.CompletionId);
        Assert.Equal("someModel", update1.ModelId);
        Assert.Equal(AudioTranscriptionUpdateKind.Transcribed, update1.Kind);

        // For choice2 (function call and result), we do not expect a concatenated text.
        Assert.True(update1.Contents.Count >= 2);
        Assert.IsType<FunctionCallContent>(update1.Contents[0]);
        Assert.IsType<FunctionResultContent>(update1.Contents[1]);
        Assert.Equal("choice2Value", update1.AdditionalProperties?["choice2Key"]);

        // Validate the extra update.
        StreamingAudioTranscriptionUpdate updateExtra = nonNullUpdates[2];
        Assert.Null(updateExtra.Text);
        Assert.Equal("value1", updateExtra.AdditionalProperties?["key1"]);
        Assert.Equal(42, updateExtra.AdditionalProperties?["key2"]);
    }
}
