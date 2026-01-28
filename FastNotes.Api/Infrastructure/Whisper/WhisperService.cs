using Microsoft.Extensions.Options;
using FastNotes.Api.Infrastructure.Config;
using OpenAI.GPT3;
using OpenAI.GPT3.Managers;
using OpenAI.GPT3.ObjectModels.RequestModels;

namespace FastNotes.Api.Infrastructure.Whisper;

public class WhisperService
{
    private readonly OpenAIService _openAi;

    public WhisperService(IOptions<OpenAISettings> options)
    {
        _openAi = new OpenAIService(new OpenAiOptions
        {
            ApiKey = options.Value.ApiKey
        });
    }

    public async Task<string> TranscribeAsync(Stream audioStream)
    {
        using var memoryStream = new MemoryStream();
        await audioStream.CopyToAsync(memoryStream);
        var audioBytes = memoryStream.ToArray();

        Console.WriteLine($"Audio received. Size: {audioBytes.Length} bytes");
        if (audioBytes.Length == 0)
        {
            Console.WriteLine("Error: empty audio file");
            return "[failed to transcribe]";
        }

        var transcriptionRequest = new AudioCreateTranscriptionRequest
        {
            FileName = "voice.ogg",
            File = audioBytes,
            Model = "whisper-1",
            Language = "en",
            ResponseFormat = "json"
        };

        var response = await _openAi.Audio.CreateTranscription(transcriptionRequest);
        if (!response.Successful)
        {
            Console.WriteLine("Error Whisper:");
            Console.WriteLine(response.Error?.Message);
            Console.WriteLine(response.Error?.Code);
        }

        return response.Successful ? response.Text : "[failed to transcribe]";
    }
}