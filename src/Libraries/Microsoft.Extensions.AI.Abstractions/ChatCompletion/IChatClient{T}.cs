// Protocol Usage for IChatClient

// Libraries implementation
public AudioChatClient : IChatClient

// Generic Protocol usage, transcript comes back as TextContent in a message
var client = new AudioChatClient()
var audioContent = new AudioContent(audioBytes, "audio/wav");
List<ChatMessage> chatHistory = [new ChatMessage(ChatRole.User, [audioContent])];

// Options currently are very specific to Text, (temperature, etc), may worth having a more generic `AIOptions` class for multi modalities.
var audioOptions = new AudioChatOptions { ResponseFormat = json, Language = ...  }

// https://platform.openai.com/docs/api-reference/audio/createTranscription
public class ProviderAudioOptions : AIOptions 
{
    public string Language { get; set; }
    public string ResponseFormat { get; set; }
}

var completion = await client.CompleteAsync(audioContent, audioOptions, cancellationToken)
var transcript = completion.Message.Text;

// Option 1:  Getting the specialized content from the protocol message (As a new content in the message)
var specializedTranscript = completion.Message.Contents.OfType<SpecializedTranscriptionContent>().First();

// Option 2: Getting the specialized content from the TextContent itself (SpecializedTranscriptionContent inherits from TextContent)
var specializedTranscript = completion.Message.RawRepresentation as SpecializedTranscriptionContent;

// This might be a problem if you want to get more metadata from the transcription (potentially available in AdditionalProperties)
var transcript = completion.Message.Text;

// Generic way for IChatCLient

// Option 1 (Input generics only)
public interface IChatClient<TInput> : IDisposable, where TInput : AIContent
{
    Task<ChatCompletion> CompleteAsync(
        TInput input,
        ChatOptions? options = null,
        CancellationToken cancellationToken = default);

    IAsyncEnumerable<StreamingChatCompletionUpdate> CompleteStreamingAsync(
        TInput input,
        ChatOptions? options = null,
        CancellationToken cancellationToken = default);
}

// Libraries implementation
public AudioChatClient : IChatClient<AudioContent>

// Generic Protocol usage, transcript comes back as TextContent in a message
var client = new AudioChatClient()
var completion = await client.CompleteAsync(audioContent, options, cancellationToken)
// This might be a problem if you want to get more metadata from the transcription (potentially available in AdditionalProperties)
var transcript = completion.Message.Text;


// To solve the problem above we can introduce a generic type for the output
public interface IChatClient<TInput, TOutput> : IDisposable, where TInput : AIContent, TOutput : AIContent
{
    Task<ChatCompletion<TOutput>> CompleteAsync(
        TInput input,
        ChatOptions? options = null,
        CancellationToken cancellationToken = default);

    IAsyncEnumerable<TOutput> CompleteStreamingAsync(
        TInput input,
        ChatOptions? options = null,
        CancellationToken cancellationToken = default);
}

public ChatCompletion<TOutput> : ChatCompletion where TOutput : AIContent
{
    // Can return multiple (for multiple choices), this will be an additional property returning the expected type
    private IEnumerable<TOutput> Contents {
        get 
        {
            foreach (var message in Choices)
            {
                foreach (var content in message.Contents.OfType<TOutput>())
                {
                    yield return content;
                }
            }
        }
    }
}

// Now customers can use the specialization from libraries
class MyAudioChatClient : IChatClient<AudioContent, SpecializedTranscriptionContent>

// usage, transcript comes back as TextContent in a message
var client = new AudioChatClient()
var completion = await client.CompleteAsync(audioContent, options, cancellationToken)
// This might be a problem if you want to get more metadata from the transcription (potentially available in AdditionalProperties)
var myTranscript = completion.Contents.First();