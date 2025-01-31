// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Globalization;

namespace Microsoft.Extensions.AI;

public class AudioTranscriptionOptions
{
    public string? CompletionId { get; set; }

    /// <summary>Gets or sets the model ID for the audio transcription.</summary>
    public string? ModelId { get; set; }

    public CultureInfo? AudioLanguage { get; set; }

    public int? AudioSampleRate { get; set; }

    public AdditionalPropertiesDictionary? AdditionalProperties { get; set; }
}
