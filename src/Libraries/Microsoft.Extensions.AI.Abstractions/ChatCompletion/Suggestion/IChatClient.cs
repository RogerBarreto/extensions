// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Shared.Diagnostics;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
#pragma warning disable S1694
#pragma warning disable S3717
#pragma warning disable SA1649
#pragma warning disable SA1600
#pragma warning disable SA1694
#pragma warning disable SA1402
#pragma warning disable CA1065 // Do not raise exceptions in unexpected locations
#pragma warning disable S3877 // Do not raise exceptions in unexpected locations
#pragma warning disable CA1822 // Mark members as static
#pragma warning disable EA0014 // The async method doesn't support cancellation
#pragma warning disable S1075 // URIs should not be hardcoded
#pragma warning disable CA1822 // Mark members as static
#pragma warning disable CA2007 // Consider calling ConfigureAwait on the awaited task

namespace Microsoft.Extensions.AI;

public abstract class AIOptions;

public abstract class AIOptions<TInput, TOutput> : AIOptions
    where TInput : AIContent
    where TOutput : AIContent;

public class Completion<TOutput>
{
    /// <summary>Initializes a new instance of the <see cref="Completion{TOutput}"/> class.</summary>
    /// <param name="outputs">The list of outputs in the completion.</param>
    [JsonConstructor]
    public Completion(IReadOnlyList<TOutput> outputs)
    {
        Outputs = Throw.IfNull(outputs);
    }

    public IReadOnlyList<TOutput> Outputs { get; }

    /// <summary>Gets or sets the raw representation of the chat completion from an underlying implementation.</summary>
    /// <remarks>
    /// If a <see cref="Completion{TOutput}"/> is created to represent some underlying object from another object
    /// model, this property can be used to store that original object. This can be useful for debugging or
    /// for enabling a consumer to access the underlying object model if needed.
    /// </remarks>
    [JsonIgnore]
    public object? RawRepresentation { get; set; }

    /// <summary>Gets or sets any additional properties associated with the chat completion.</summary>
    public AdditionalPropertiesDictionary? AdditionalProperties { get; set; }
}

public class ModelClientMetadata
{
    public ModelClientMetadata(string? providerName = null, Uri? providerUri = null, string? modelId = null)
    {
        ModelId = modelId;
        ProviderName = providerName;
        ProviderUri = providerUri;
    }

    /// <summary>Gets the name of the chat completion provider.</summary>
    /// <remarks>
    /// Where possible, this maps to the appropriate name defined in the
    /// OpenTelemetry Semantic Conventions for Generative AI systems.
    /// </remarks>
    public string? ProviderName { get; }

    /// <summary>Gets the URL for accessing the chat completion provider.</summary>
    public Uri? ProviderUri { get; }

    /// <summary>Gets the ID of the model used by this chat completion provider.</summary>
    /// <remarks>
    /// This value can be null if either the name is unknown or there are multiple possible models associated with this instance.
    /// An individual request may override this value via <see cref="ChatOptions.ModelId"/>.
    /// </remarks>
    public string? ModelId { get; }
}

public interface IModelClient : IDisposable
{
    /// <summary>Gets metadata that describes the <see cref="IModelClient"/>.</summary>
    ModelClientMetadata Metadata { get; }

    /// <summary>Asks the <see cref="IModelClient"/> for an object of the specified type <paramref name="serviceType"/>.</summary>
    /// <param name="serviceType">The type of object being requested.</param>
    /// <param name="serviceKey">An optional key that can be used to help identify the target service.</param>
    /// <returns>The found object, otherwise <see langword="null"/>.</returns>
    /// <remarks>
    /// The purpose of this method is to allow for the retrieval of strongly typed services that might be provided by the <see cref="IModelClient"/>,
    /// including itself or any services it might be wrapping.
    /// </remarks>
    object? GetService(Type serviceType, object? serviceKey = null);
}

/// <summary>
/// Represents a client for interacting with a chat service.
/// </summary>
/// <typeparam name="TInput">Input content type for the chat client.</typeparam>
/// <typeparam name="TOutput">Output content type for the chat client.</typeparam>
public interface IModelClient<TInput, TOutput> : IModelClient
    where TInput : AIContent
    where TOutput : AIContent
{
    Task<Completion<TOutput>> CompleteAsync(
        TInput input,
        AIOptions<TInput, TOutput>? options = null,
        CancellationToken cancellationToken = default);
}

public interface IStreamingModelClient<TInput, TOutput> : IModelClient
    where TInput : AIContent
    where TOutput : AIContent
{
    IAsyncEnumerable<TOutput> CompleteStreamingAsync(
        TInput input,
        AIOptions<TInput, TOutput>? options = null,
        CancellationToken cancellationToken = default);
}

public class AudioOptions : AIOptions<AudioContent, AudioTranscriptionContent>
{
    public string Language { get; set; } = "en";
}

public class AudioTranscriptionContent : TextContent
{
    public AudioTranscriptionContent(string? text)
        : base(text)
    {
    }

    public string Transcript => Text;

    public TimeSpan Duration { get; set; }
}

public class AudioClientMetadata : ModelClientMetadata;

public sealed class AudioClient :
    IChatClient,
    IModelClient<AudioContent, AudioTranscriptionContent>,
    IStreamingModelClient<AudioContent, AudioTranscriptionContent>
{
    public ModelClientMetadata Metadata => new AudioClientMetadata();

    public Task<Completion<AudioTranscriptionContent>> CompleteAsync(
        AudioContent input,
        AIOptions<AudioContent, AudioTranscriptionContent>? options = null,
        CancellationToken cancellationToken = default)
            => throw new NotImplementedException();

    public IAsyncEnumerable<AudioTranscriptionContent> CompleteStreamingAsync(
        AudioContent input,
        AIOptions<AudioContent, AudioTranscriptionContent>? options = null,
        CancellationToken cancellationToken = default)
            => throw new NotImplementedException();

    public Task<ChatCompletion> CompleteAsync(
        IList<ChatMessage> chatMessages,
        ChatOptions? options = null,
        CancellationToken cancellationToken = default)
            => throw new NotImplementedException();

    public IAsyncEnumerable<StreamingChatCompletionUpdate> CompleteStreamingAsync(
        IList<ChatMessage> chatMessages,
        ChatOptions? options = null,
        CancellationToken cancellationToken = default)
        => throw new NotImplementedException();

    public void Dispose()
    {
        throw new NotSupportedException();
    }

    public object? GetService(Type serviceType, object? serviceKey = null)
        => throw new NotImplementedException();
}

public class UseCase
{
    public async Task ProtocolWayAsync()
    {
        byte[] audioBytes = [0x00, 0x01, 0x02, 0x03];
        using var client = new AudioClient();
        var audioContent = new AudioContent(audioBytes, "audio/wav");
        List<ChatMessage> chatHistory = [new ChatMessage(ChatRole.User, [audioContent])];
        var options = new ChatOptions { ModelId = "123" };
        options.AdditionalProperties ??= [];
        options.AdditionalProperties.Add("language", "pt");

        var completion = await client.CompleteAsync(chatHistory, options, CancellationToken.None);
        var transcriptText = completion.Message.Text;

        var specializedTranscript = completion.Message.Contents.FindFirst<AudioTranscriptionContent>()!.Transcript;

        await foreach (var output in client.CompleteStreamingAsync(chatHistory, options))
        {
            // Process output
        }
    }

    public async Task GenericUseCaseAsync()
    {
        using var client = new AudioClient();
        var input = new AudioContent(new Uri("https://example"));
        var options = new AudioOptions { Language = "pt" };

        var completion = await client.CompleteAsync(input, options, CancellationToken.None);

        // Get Transcripts
        var transcripts = completion.Outputs[0].Transcript;

        await foreach (var output in client.CompleteStreamingAsync(input, options))
        {
            // Process output
        }
    }
}
