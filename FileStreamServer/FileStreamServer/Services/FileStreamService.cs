using FileStreamServer;
using Grpc.Core;

namespace FileStreamServer.Services;

public class FileStreamService : StreamFile.StreamFileBase
{
    readonly IWebHostEnvironment _env;

    public FileStreamService(IWebHostEnvironment env)
    {
        _env = env;
    }

    public override async Task<FileResult> UpStreamFile(IAsyncStreamReader<Chunk> requestStream, ServerCallContext context)
    {
        string folderPath = _env.ContentRootPath + "/Files/";
        if (!Directory.Exists(folderPath))
            Directory.CreateDirectory(folderPath);
        string filePath = folderPath + Guid.NewGuid().ToString() + context.RequestHeaders.Get("extension").Value;
        Stream fs = new FileStream(filePath, FileMode.Create);

        try
        {
            await foreach (var item in requestStream.ReadAllAsync())
            {
                await fs.WriteAsync(item.Buffer.ToByteArray(), 0, item.Buffer.Length);
            }
            return new FileResult { Status = 200 };
        }
        catch
        {
            return new FileResult { Status = 500 };
        }
        finally
        {
            fs.Close();
        }
    }
}