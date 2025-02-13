﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;
using Microsoft.Shared.Diagnostics;

namespace Microsoft.Extensions.AI;

/// <summary>Represents the result of an audio transcription request.</summary>
public class AudioTranscriptionResponse
{
    /// <summary>The list of choices in the completion.</summary>
    private IList<AudioTranscriptionChoice> _choices;

    /// <summary>Initializes a new instance of the <see cref="AudioTranscriptionResponse"/> class.</summary>
    /// <param name="choices">The list of choices in the completion, one message per choice.</param>
    [JsonConstructor]
    public AudioTranscriptionResponse(IList<AudioTranscriptionChoice> choices)
    {
        _choices = Throw.IfNull(choices);
    }

    /// <summary>Initializes a new instance of the <see cref="AudioTranscriptionResponse"/> class.</summary>
    /// <param name="choice">The transcription representing the singular choice in the completion.</param>
    public AudioTranscriptionResponse(AudioTranscriptionChoice choice)
    {
        _ = Throw.IfNull(choice);
        _choices = [choice];
    }

    /// <summary>Gets or sets the list of audio transcription choices.</summary>
    public IList<AudioTranscriptionChoice> Choices
    {
        get => _choices;
        set => _choices = Throw.IfNull(value);
    }

    /// <summary>Gets the first transcription choice details.</summary>
    /// <remarks>
    /// If there are multiple choices, this property returns the first choice.
    /// If <see cref="Choices"/> is empty, this property will throw. Use <see cref="Choices"/> to access all choices directly.
    /// </remarks>
    [JsonIgnore]
    public AudioTranscriptionChoice Choice
    {
        get
        {
            var choices = Choices;
            if (choices.Count == 0)
            {
                throw new InvalidOperationException($"The {nameof(AudioTranscriptionResponse)} instance does not contain any {nameof(AudioTranscriptionChoice)} choices.");
            }

            return choices[0];
        }
    }

    /// <summary>Gets or sets the ID of the audio transcription completion.</summary>
    public string? CompletionId { get; set; }

    /// <summary>Gets or sets the model ID used in the creation of the audio transcription completion.</summary>
    public string? ModelId { get; set; }

    /// <summary>Gets or sets the raw representation of the audio transcription completion from an underlying implementation.</summary>
    /// <remarks>
    /// If a <see cref="AudioTranscriptionResponse"/> is created to represent some underlying object from another object
    /// model, this property can be used to store that original object. This can be useful for debugging or
    /// for enabling a consumer to access the underlying object model if needed.
    /// </remarks>
    [JsonIgnore]
    public object? RawRepresentation { get; set; }

    /// <summary>Gets or sets any additional properties associated with the audio transcription completion.</summary>
    public AdditionalPropertiesDictionary? AdditionalProperties { get; set; }

    /// <inheritdoc />
    public override string ToString()
    {
        if (Choices.Count == 1)
        {
            return Choices[0].ToString();
        }

        StringBuilder sb = new();
        for (int i = 0; i < Choices.Count; i++)
        {
            if (i > 0)
            {
                _ = sb.AppendLine().AppendLine();
            }

            _ = sb.Append("Choice ").Append(i).AppendLine(":").Append(Choices[i]);
        }

        return sb.ToString();
    }

    /// <summary>Creates an array of <see cref="AudioTranscriptionResponseUpdate" /> instances that represent this <see cref="AudioTranscriptionResponse" />.</summary>
    /// <returns>An array of <see cref="AudioTranscriptionResponseUpdate" /> instances that may be used to represent this <see cref="AudioTranscriptionResponse" />.</returns>
    public AudioTranscriptionResponseUpdate[] ToStreamingAudioTranscriptionUpdates()
    {
        AudioTranscriptionResponseUpdate? extra = null;
        if (AdditionalProperties is not null)
        {
            extra = new AudioTranscriptionResponseUpdate
            {
                Kind = AudioTranscriptionResponseUpdateKind.Transcribed,
                AdditionalProperties = AdditionalProperties,
            };
        }

        int choicesCount = Choices.Count;
        var updates = new AudioTranscriptionResponseUpdate[choicesCount + 1 + (extra is null ? 0 : 1)];

        for (int choiceIndex = 0; choiceIndex < choicesCount; choiceIndex++)
        {
            AudioTranscriptionChoice choice = Choices[choiceIndex];
            updates[choiceIndex] = new AudioTranscriptionResponseUpdate
            {
                ChoiceIndex = choiceIndex,
                InputIndex = choice.InputIndex,

                AdditionalProperties = choice.AdditionalProperties,
                Contents = choice.Contents,
                RawRepresentation = choice.RawRepresentation,
                StartTime = choice.StartTime,
                EndTime = choice.EndTime,

                Kind = AudioTranscriptionResponseUpdateKind.Transcribed,
                CompletionId = CompletionId,
                ModelId = ModelId,
            };
        }

        if (extra is not null)
        {
            updates[choicesCount] = extra;
        }

        return updates;
    }
}
