// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;
using Microsoft.Extensions.AI;
using Microsoft.Shared.Diagnostics;

namespace Microsoft.Extensions.AI;

public class StreamingAudioTranscriptionUpdate
{
    private IList<AIContent>? _contents;

    /// <summary>Initializes a new instance of the <see cref="StreamingAudioTranscriptionUpdate"/> class.</summary>
    [JsonConstructor]
    public StreamingAudioTranscriptionUpdate()
    {
    }

    /// <summary>Initializes a new instance of the <see cref="StreamingAudioTranscriptionUpdate"/> class.</summary>
    /// <param name="contents">The contents for this message.</param>
    public StreamingAudioTranscriptionUpdate(IList<AIContent> contents)
    {
        _contents = Throw.IfNull(contents);
    }

    /// <summary>Initializes a new instance of the <see cref="StreamingAudioTranscriptionUpdate"/> class.</summary>
    /// <param name="content">Content of the message.</param>
    public StreamingAudioTranscriptionUpdate(string? content)
        : this(content is null ? [] : [new TextContent(content)])
    {
    }

    /// <summary>Gets or sets the ID of the completion of which this update is a part.</summary>
    public string? CompletionId { get; set; }

    public required AudioTranscriptionUpdateKind Kind { get; init; }

    [JsonIgnore]
    public object? RawRepresentation { get; set; }

    public AdditionalPropertiesDictionary? AdditionalProperties { get; set; }

    public TimeSpan? StartTime { get; set; }

    public TimeSpan? EndTime { get; set; }

    /// <summary>
    /// Gets or sets the text of the first <see cref="TextContent"/> instance in <see cref="Contents" />.
    /// </summary>
    /// <remarks>
    /// If there is no <see cref="TextContent"/> instance in <see cref="Contents" />, then the getter returns <see langword="null" />,
    /// and the setter adds a new <see cref="TextContent"/> instance with the provided value.
    /// </remarks>
    [JsonIgnore]
    public string? Text
    {
        get => Contents.FindFirst<TextContent>()?.Text;
        set
        {
            if (Contents.FindFirst<TextContent>() is { } textContent)
            {
                textContent.Text = value;
            }
            else if (value is not null)
            {
                Contents.Add(new TextContent(value));
            }
        }
    }

    /// <summary>Gets or sets the chat message content items.</summary>
    [AllowNull]
    public IList<AIContent> Contents
    {
        get => _contents ??= [];
        set => _contents = value;
    }
}
