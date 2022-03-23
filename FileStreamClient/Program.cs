using FileStreamServer;
using Google.Protobuf;
using Grpc.Core;
using Grpc.Net.Client;
using static System.Console;

WriteLine("File Streaming With gRPC!");
const int size = 1024;

var channel = GrpcChannel.ForAddress("https://localhost:7266");
var client = new StreamFile.StreamFileClient(channel);

Write("Enter Your File Address : ");
var fileAddress = ReadLine();
Metadata headers = new();
var extension = Path.GetExtension(fileAddress);
headers.Add("extension", extension ?? ".jpg");

using Stream source = File.OpenRead(fileAddress ?? "");
using var call = client.UpStreamFile(headers);

byte[] buffer = new byte[size];
var itemLength = source.Length / 100;
var uploaded = 0;
WriteLine(source.Length);
int bytesRead = 0;
while ((bytesRead = source.Read(buffer, 0, buffer.Length)) > 0)
{
    await call.RequestStream.WriteAsync(new() { Buffer = ByteString.CopyFrom(buffer) });
    uploaded += bytesRead;
    var itemUploaded = uploaded / itemLength;
    WriteLine($"{itemUploaded}%");
}

await call.RequestStream.CompleteAsync();
var result = await call;
WriteLine(result.Status);

