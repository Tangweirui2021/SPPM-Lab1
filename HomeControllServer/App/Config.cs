// B3
namespace HomeControllServer.App;

public class Config
{
    public static (Dictionary<string, string>, Dictionary<string, string>, Dictionary<string, string>)? ReadMsgFile()
    {
        if (Directory.Exists("messages"))
        {
            var both = new Dictionary<string, string>();
            var http = new Dictionary<string, string>();
            var webSocket = new Dictionary<string, string>();
            if (Directory.Exists("messages/http"))
            {
                foreach (var file in Directory.GetFiles("messages/http"))
                {
                    var key = Path.GetFileNameWithoutExtension(file);
                    var value = File.ReadAllText(file);
                    http.Add(key, value);
                }
            }
            else
            {
                Directory.CreateDirectory("messages/http");
            }
            if (Directory.Exists("messages/webSocket"))
            {
                foreach (var file in Directory.GetFiles("messages/webSocket"))
                {
                    var key = Path.GetFileNameWithoutExtension(file);
                    var value = File.ReadAllText(file);
                    webSocket.Add(key, value);
                }
            }
            else
            {
                Directory.CreateDirectory("messages/webSocket");
            }
            if (Directory.Exists("messages/both"))
            {
                foreach (var file in Directory.GetFiles("messages/both"))
                {
                    var key = Path.GetFileNameWithoutExtension(file);
                    var value = File.ReadAllText(file);
                    both.Add(key, value);
                }
            }
            else
            {
                Directory.CreateDirectory("messages/both");
            }
            return (both, http, webSocket);
        }
        Directory.CreateDirectory("messages");
        Directory.CreateDirectory("messages/http");
        Directory.CreateDirectory("messages/webSocket");
        Directory.CreateDirectory("messages/both");
        return null;
    }
}