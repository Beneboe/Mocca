using System.Text.Json;
using Microsoft.Extensions.Options;
using Mocca.Data;
using Mocca.Interfaces;

namespace Mocca.Services;

public sealed class MoccaJsonRepository : IMoccaRepository
{
    private static readonly SemaphoreSlim Semaphore = new SemaphoreSlim(0, 1);
    private readonly string _destination;
    private readonly JsonSerializerOptions _jsonOptions;

    static MoccaJsonRepository()
    {
        Semaphore.Release();
    }

    public MoccaJsonRepository(IOptions<MoccaOptions> options)
    {
        _destination = options.Value.ResponseFile;
        _jsonOptions = new JsonSerializerOptions(JsonSerializerDefaults.Web);
        
        if (string.IsNullOrWhiteSpace(_destination))
        {
            throw new InvalidDataException("Destination cannot be empty.");
        }
    }
    
    public async Task AddAsync(MoccaRequest request, MoccaResponse response)
    {
        await Semaphore.WaitAsync();

        try
        {
            if (File.Exists(_destination))
            {
                await using var readStream = File.OpenRead(_destination);
                using var reader = new StreamReader(readStream);

                await SeekToResponse(reader, request);

                if (!reader.EndOfStream)
                {
                    // Request already exists.
                    return;
                }
            }

            // var tempFileName = _destination.Replace(
            //     oldValue: Path.GetFileName(_destination), 
            //     newValue: Path.GetFileNameWithoutExtension(_destination) + ".temp" + Path.GetExtension(_destination));
            //
            // File.Copy(_destination, tempFileName);
            // await using var writeStream = File.OpenWrite(tempFileName);
            // using var writer = new StreamWriter(writeStream);
        
            // Write at end of file.
            await using var writeStream = File.Open(_destination, FileMode.Open, FileAccess.Write, FileShare.None);
            writeStream.Seek(0, SeekOrigin.End);
            await using var writer = new StreamWriter(writeStream);

            var line = JsonSerializer.Serialize(request);
            await writer.WriteLineAsync(line);

            line = JsonSerializer.Serialize(response);
            await writer.WriteLineAsync(line);
        }
        finally
        {
            Semaphore.Release();
        }
    }

    public async Task<MoccaResponse?> ResolveAsync(MoccaRequest request)
    {
        await using var stream = File.OpenRead(_destination);
        using var reader = new StreamReader(stream);

        await SeekToResponse(reader, request);
        
        if (reader.EndOfStream)
        {
            return null;
        }
        
        var line = await reader.ReadLineAsync() ?? string.Empty;
        var response = JsonSerializer.Deserialize<MoccaResponse>(line, _jsonOptions);
        return response;
    }

    private async Task SeekToResponse(StreamReader reader, MoccaRequest request)
    {
        while (!reader.EndOfStream)
        {
            string line = await reader.ReadLineAsync() ?? string.Empty;
            var fileRequest = JsonSerializer.Deserialize<MoccaRequest>(line, _jsonOptions);

            if (fileRequest is null)
            {
                throw new InvalidDataException("Cannot parse request from file.");
            }

            if (fileRequest.IsDefault)
            {
                throw new InvalidDataException("Default request in file.");
            }

            if (reader.EndOfStream)
            {
                throw new InvalidDataException("Missing response.");
            }
            
            if (fileRequest.Equals(request))
            {
                return;
            }

            _ = await reader.ReadLineAsync();
        }
    }
}