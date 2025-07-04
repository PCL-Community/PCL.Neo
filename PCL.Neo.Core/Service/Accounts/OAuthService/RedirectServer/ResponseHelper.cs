using System.Net;

namespace PCL.Neo.Core.Service.Accounts.OAuthService.RedirectServer;

public class ResponseHelper(HttpListenerResponse response)
{
    private readonly Stream _outputStream = response.OutputStream;

    public void WriteContent(FileStream fileStream)
    {
        response.StatusCode = 200;
        var buffer = new byte[1024];
        var obj = new FileObject(fileStream, buffer);
        fileStream.BeginRead(buffer, 0, buffer.Length, EndWirte, obj);
    }

    private void EndWirte(IAsyncResult asyncResult)
    {
        var obj = asyncResult.AsyncState as FileObject;
        var number = obj.FileStream.EndRead(asyncResult);
        _outputStream.Write(obj.Buffer, 0, number);
        if (number < 1)
        {
            obj.FileStream.Close();
            _outputStream.Close();
            return;
        }

        obj.FileStream.BeginRead(obj.Buffer, 0, obj.Buffer.Length, EndWirte, obj);
    }

    public record FileObject(
        FileStream FileStream,
        byte[] Buffer
    );
}
