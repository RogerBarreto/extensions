# Problem Statement

Provide a way to handle multi modalities.

# The Protocol Way (Chat Completion Messages)

TLDR, using the Chat Completion Message API as a protocol to handle multi modalities.

OpenAI came up with the Chat Completion Message API to handle chat completion models which in essence are multi modality models.
The message API allows for multiple content types to be provided in single or multiple messages as a context for the model to generate completions.
The message API also allows for multiple content types to be returned in a single message.
It's pretty clear that this message API can be used as a protocol to handle multi modalities in a generic way.

## Current Limitations

### Allow specialized TextContent

Currently the `TextContent` is sealed, which makes it hard to extend it to support more specialized types of text content, like `TranscriptionContent`, `TranslationContent`, etc. Unsealing this type would be desirable.

### Lack of options in the Content

Currently majority of the API's consider the configuration as a separate argument from the content, initially this was a good approach to keep the content and configuration separated, but may not age well with multi modalities support, and is starting to hit some limitations (including own OpenAI API's), like providing the image quality in the content request body itself. 

See: [Request.Message[Role=User].ContentPart[Type=Image].Image_Url.Detail](https://platform.openai.com/docs/api-reference/chat/create).

To support scenarios like this we might consider passing the configuration in the content as default, this would allow for a more flexible and extensible way to handle multi modalities.

`AIContent.Options`, this would give a more robust and future proof way to handle multi modalities, but I won't be diving much on this approach as this investigation would eventually diverge from a leaner proposal to the APIs that currently suffice for 95%+ of use cases.

### ChatOptions as AIOptions

Currently **Chat Completion** abstractions and implementations have only Chat dedicated options that aren't extendable for multi modalities (audio, image) except for using the `AdditionalProperties` which is not ideal.

i.e.:

| Modality | Options          |
|----------|------------------|
| Audio    | Response Format¹, Language, Granularities |
| Image    | Response Format¹, Quality, Size, Scale, Orientation |
| Chat     | Response Format¹, Temperature, Max Tokens, Top P, Frequency Penalty, Presence Penalty |

**1** Response Format is permeates for all options potentially a good `AIOptions` abstract class property candidate.

**To support multiple modalities for **Options** some approaches can be considered:**

Response Format although being a common setting might combine together targeting specific for a content result types, i.e: 

When providing a chat with Audio, Image and Text, each response type can target a different modality result. Like:

- Transcription/Text Content = Response Format: JSON, Text
- Audio Format = Response Format: Wav / Mp3
- Image Format = Response Format: Png / Jpeg

To achieve such capability a new way of combining options must be considered.
i.e:

1. **Composition approach (similar to IChatClient).**

   - `new AIOptions(chatOptions, audioOptions)`
   - `new ChatOptions(new AudioOptions())` 
   - `chatOptions.With(new AudioOptions())`

2. **Collection of options / Pipelining** 
    
    Allowing a list of `AIOptions` to be provided on `Generate` methods.
   
   `GenerateAsync(input, [chatOptions, audioOptions])`

   Might require a specialized type collection/pipeline to handle conflict/prioritization between options.

### ChatClient promotion to `ModelClient` abstractions

Currently IChatClient interface have methods that can be shared across different modalities and client abstractions, moving `Metadata` and `GetService` requirements to a lower level abstraction can work best for more potential `IChatClient` specializations that we are going to explore further on.:

Since each client implementation is model specific removing the purpose (Chat) when bringing this class to a lower level, makes more sense using `Model` in the name. Suggest using `IModelClient`.

Suggested changes:

- IChatClient -> IModelClient
- ChatClientMetadata -> ModelClientMetadata (Same structure, potentially just a renaming would be enough)

```csharp
// Moved Metadata and GetService to a lower level IModelClient abstraction
public interface IModelClient : IDisposable
{
    ModelClientMetadata Metadata { get; }

    object? GetService(Type serviceType, object? serviceKey = null);
}

// Old IChatClient only requires the Chat specific methods
public interface IChatClient : IModelClient
{
    // (Generate methods only)
}

public class ModelClientMetadata
{
    // Same structure of ChatClientMetadata
}

// Old ChatClientMetadata 
// We may consider dropping this abstraction from Abstractions namespace.
public class ChatClientMetadata : ModelClientMetadata;
```

This new lower lever abstraction will ensure that further higher level abstractions (Chat + other modalities) share IDisposable, Metadata and GetService behaviors without duplicating those signature on any other modality interfaces.

i.e:

```csharp
// We will explore such abstractions further on
public interface IModelClient<TInput, TOutput> : IModelClient;
public interface IStreamingModelClient<TInput, TOutput> : IModelClient;
public interface IAudioToTextClient : IModelClient;
```

## Audio-to-Text IChatClients Use Case (Protocol Way)

This is just a demonstration how this could be achieved using a pure ChatClient implementation.

Following a pure Protocol way, the `IChatClient` abstraction can be used for Any-to-Any where the internal implementation gets all audio contents from the last message in the chat history message and send them to be processed returning back the transcription output as a ChatMessage with a [specialized TextContent](#allow-specialized-textcontent) containing the transcription.

```csharp
byte[] audioBytes = [0x00, 0x01, 0x02, 0x03];
using var client = new AudioClient();
var audioContent = new AudioContent(audioBytes, "audio/wav");
List<ChatMessage> chatHistory = [new ChatMessage(ChatRole.User, [audioContent])];

ChatOptions options = new();

// Adding audio options thru Additional Properties
options.AdditionalProperties ??= [];
options.AdditionalProperties.Add("language", "pt");

var completion = await client.CompleteAsync(chatHistory, options, CancellationToken.None);
```

### Result as **Message.Text**

For majority of use cases where the transcription text content is enough, the transcript result content can be a specialized version of `TextContent` and be available in the `Text` property of a message.

```csharp
// Transcript comes back as TextContent Text in the message
var transcriptText = completion.Message.Text;
```

### Result as **Message.Contents**

If you require additional the metadata and know the specialized type for the transcription, the same could be achieved using the `FindFirst` from the message or traversing the `AdditionalProperties` for additional metadata provided.

```csharp
// Or can be get from the specialized content in the message
var specializedContent = completion.Message.Contents.FindFirst<SpecializedTranscriptionContent>();
var transcriptText = specializedContent.Text;

// Getting metadata
var requestId = completion.AdditionalProperties["id"];
var stopReason = completion.Message.AdditionalProperties["stop_reason"];
```

### Notes

Clearly not the most efficient way to handle audio-to-text, but it's a good demonstration of how the protocol can be used to handle multi modalities, we will explore other simpler options ahead. 

## `<Modality>-to-<Modality>` - Abstraction Approaches

Is important to keep in mind that the internal implementation of any of this abstractions can be similar to a IChatClient implementation but not necessarily needs to be based in `ChatMessage` or `ChatHistory`, for this reason, was necessary to abstract also the concept of `ChatCompletion` in a generic way, allowing the internal implementation to be more flexible and not necessarily tied to a chat completion result.

### Generic Completion`<TOutput>`

The `Completion<TOutput>` abstraction will handle scenarios specific for modality-to-modality transformations without bringing the overhead of handling messages. 

> [!NOTE] 
> If we identify that there's a need to handle the messages and have access to messaging, we can consider simple `ChatCompletion<TOutput>` or a base `ModelCompletion` abstraction similar to `IModelClient`, not including specifics of a chat completion metadata.

```csharp
// Output type here is not necessarily tied to an AIContent, as we can think on having `Completion<AIContent>` for messages.
public class Completion<TOutput> 
    where TOutput : AIContent
{
    [JsonConstructor]
    public Completion(IReadOnlyList<TOutput> outputs)
    {
        Outputs = Throw.IfNull(outputs);
    }

    // Multiple outputs for the desired format
    public IReadOnlyList<TOutput> Outputs { get; }

    [JsonIgnore]
    public object? RawRepresentation { get; set; }

    public AdditionalPropertiesDictionary? AdditionalProperties { get; set; }
}
```

When implementing those generic interfaces is important to keep in mind that the internal implementation will be relying in the protocol way described above, where the abstractions will work mostly to simplify the end-user experience with the API's.

### Generic `<TInput, TOutput>`

Since there can be an infinite potential of modality permutations, would be very risky and unpractical narrowing down non generic abstractions only for main use cases of modality combinations.

The generic approach embraces this infinite potential quite easily allowing providers to __specify a generic `Input-to-Output` permutation__.

Introducing generic `IChatClient` and `Options` abstractions for input and output types, can improve significantly experience of the caller giving more clarity to what to expect from the API.

Since a combination of APIs and modality permutations that exists, would be very risky and unpractical specifying a generic `Option` for each permutation.
To address different providers modality permutation will require specific options that embrace a generic  input and output specification types. 

```csharp
public abstract class AIOptions<TInput, TOutput>
    where TInput : AIContent
    where TOutput : AIContent;
```

Generic options allow them to be specific to the classes and modality-to-modality combinations only.
This allows constructs like:

### Provider A (Streaming only options): 
```csharp
public class ProviderAOptions : AIOptions<AudioContent, StreamingTranscriptionContent>
{
    public int StreamingBufferSize { get; set; }
}

var client = new ProviderAChatClient();
await foreach (var audioChunk in client.CompleteStreamingAsync(audioContent, new(), cancellationToken)) 
{
    // Process
}
```

### Provider B (Audio Non-Streaming only options): 
```csharp
public class ProviderBOptions : AIOptions<AudioContent, SpecializedTranscriptionContent>
{
    public string Language { get; set; }
}

var client = new ProviderBChatClient();
var result = await client.CompleteAsync(audioContent, new AudioOptions(provider1Options), cancellationToken);
```

### IChatClient(IModelClient) Abstractions

Segregating the streaming and non-streaming API's can be beneficial for the caller to understand what to expect from the API.
As the streaming and non-streaming API's are different and return different types of content and configurations, would make sense to have those segregated in specific interfaces. 
Some services even provide one or another, i.e: OpenAI's transcription only provide Non-Streaming API's, where the audio client for those would just need to implement the non-streaming interface. 

**Non-Streaming**

```csharp
public interface IModelClient<TInput, TOutput> : IModelClient
    where TInput : AIContent
    where TOutput : AIContent
{
    Task<Completion<TOutput>> CompleteAsync(
        TInput input,
        AIOptions<TInput, TOutput>? options = null,
        CancellationToken cancellationToken = default);
}
```

**Streaming**

Streaming APIs for different contents may return dedicated StreamingContentUpdate types, as well as having a different category of options.

```csharp
public interface IStreamingModelClient<TInput, TOutput> : IModelClient
    where TInput : AIContent
    where TOutput : AIContent
{
    IAsyncEnumerable<TOutput> CompleteStreamingAsync(
        TInput input,
        AIOptions<TInput, TOutput>? options = null,
        CancellationToken cancellationToken = default);
}
```

### Options Abstractions

Options can be segregated in a similar way to the IChatClient, where the options are specific to the input and output types.

Options differently from the `IModelClient`, can be more generic and not require a segregation between streaming and non-streaming and can be used for both API's.

```csharp
public abstract class AIOptions 
{
    public string ResponseFormat { get; set; }
}

public abstract class AIOptions<TInput, TOutput> : AIOptions
    where TInput : AIContent
    where TOutput : AIContent;
```

### Options and IModelClient Implementations for OpenAI

```csharp
public class OpenAIAudioOptions : AIOptions<AudioContent, SpecializedTranscriptionContent>
{
    public string Language { get; set; }
    public string ResponseFormat { get; set; }
}

public class OpenAIAudioChatClient : 
    IChatClient<AudioContent, SpecializedTranscriptionContent>, 

    // OpenAI doesn't support streaming in Audio transcription and the client won't need to implement the streaming interface
    // IStreamingChatClient<AudioContent, StreamingAudioContentUpdate> 
{
    public Task<ChatCompletion<SpecializedTranscriptionContent>> CompleteAsync(
        AudioContent input,
        OpenAIAudioOptions options = null,
        CancellationToken cancellationToken = default)
    {
        // Implementation
    }
}
```

Usage:

```csharp	
using var client = new AudioClient();
var input = new AudioContent(new Uri("https://example"));
var options = new AudioOptions { Language = "pt" };

var completion = await client.CompleteAsync(input, options, CancellationToken.None);

// Get the specialized content straight from the completion
var specializedContent = completion.Outputs.First();

// Get the transcript text
var transcriptText = specializedContent.Transcript; // Specific property
// Or 
var transcriptText = specializedContent.Text; // Gets from the TextContent inheritance
```

### Options Implementation

One interesting thing to observe is that each API (streaming vs non-streaming may also require different configurations), having the specific options for each API (thru generic Output spec) can be beneficial for discerning what are the correct Option settings supported for each API.

This also comes in line with the latest OpenAI API's that are starting to provide more specific options for streaming ([streaming options](https://platform.openai.com/docs/api-reference/chat/create#chat-create-stream_options)) that aren't required when the flag is false and may be only available in the streaming API option definition.

**Non-Streaming**

```csharp
public class OpenAIAudioOptions : AIOptions<AudioContent, SpecializedTranscriptionContent>
{
    public List<string> Granularities { get; set; }
    public string Language { get; set; }
    public string ResponseFormat { get; set; }
}
```

**Streaming**

Options:
```csharp
public class AssemblyAIStreamingOptions : AIOptions<AudioContent, AssemblyAIStreamingTranscriptionContentUpdate>
{
    public TranscriptLanguageCode LanguageCode { get; set; }
    public bool IncludeUsage { get; set; }
}
```

Content:

Specialized content for streaming updates

```csharp
public class AssemblyAIStreamingTranscriptionContentUpdate
{
    public string? Transcript { get; set; }

    public TimeSpan TimeStamp { get; set; }
}
```

Assembly AI Audio Client (Supports Streaming only)
```csharp
public class AssemblyAIAudioClient : IStreamingChatClient<AudioContent, AssemblyAIStreamingTranscriptionContentUpdate>
{
    public IAsyncEnumerable<AssemblyAIStreamingTranscriptionContentUpdate> CompleteStreamingAsync(
        AudioContent input,
        AssemblyAIStreamingOptions options = null,
        CancellationToken cancellationToken = default)
    {
        // Implementation
    }
}
```

Usage:

```csharp
var client = new AssemblyAIAudioClient();
var audioContent = new AudioContent(audioBytes, "audio/wav");
var options = new AssemblyAIStreamingOptions { LanguageCode = TranscriptLanguageCode.EnglishUS };

await foreach (var audioChunk in client.CompleteStreamingAsync(audioContent, options, cancellationToken)) 
{
    switch (audioChunk.Event)
    {
        case Recognizing:
            // Process
            break;
        case Recognized:
            // Process
            break;
        case Canceled:
            // Process
            break;
    }
}
```

**Dependency Injection Consideration** (WIP)

As rule of thumb to improve the discoverability of the generic services, would be recommended to register the services using the M.E.AI.Abstraction contents (Image, Text, Audio, Binary).

```csharp
services.AddScoped<IChatClient<AudioContent, SpecializedTranscriptionContent>, AudioChatClient>(); // Avoid
// Instead register the generic ones
services.AddScoped<IChatClient<AudioContent, TextContent>, AudioChatClient>(); // Preferred
```
