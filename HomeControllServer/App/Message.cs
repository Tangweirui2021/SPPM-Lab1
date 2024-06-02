// C4
using System.Net.WebSockets;
using System.Text;

namespace HomeControllServer.App;

public class Message
{
    public static async Task HandleWebSocket(WebSocket socket, Data data)
    {
        // Handle the socket
        var buffer = new byte[1024];
        var message = new StringBuilder();

        while (socket.State == WebSocketState.Open)
        {
            WebSocketReceiveResult result;
            do
            {
                result = await socket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                message.Append(Encoding.UTF8.GetString(buffer, 0, result.Count));
            } while (!result.EndOfMessage);
            var split = message.ToString().Split(' ');
            message.Clear();
            if (split.Length is > 3)
            {
                await socket.SendAsync(new ArraySegment<byte>("Invalid message, type A"u8.ToArray()), WebSocketMessageType.Text, true, CancellationToken.None);
                continue;
            }
            var key = split[0].ToLower();
            switch (key)
            {
                case "ping":
                    await socket.SendAsync(new ArraySegment<byte>("pong"u8.ToArray()), WebSocketMessageType.Text, true, CancellationToken.None);
                    break;
                case "init":
                    if (split.Length is not 3 and not 4)
                    {
                        await socket.SendAsync(new ArraySegment<byte>("Invalid message, type I"u8.ToArray()), WebSocketMessageType.Text, true, CancellationToken.None);
                        continue;
                    }
                    var tagInit = split[1];
                    var valueInit = split[2];
                    var typeInit = split.Length == 4 ? split[3] : "0";
                    
                    if (data.HttpMessages.Contains(tagInit) || data.WebSocketMessages.Contains(tagInit))
                    {
                        await socket.SendAsync(new ArraySegment<byte>("Key type not match"u8.ToArray()), WebSocketMessageType.Text, true, CancellationToken.None);
                        continue;
                    }
                    switch (typeInit)
                    {
                        case "0":
                            data.HttpMessages.Add(tagInit);
                            data.WebSocketMessages.Add(tagInit);
                            break;
                        case "1":
                            data.HttpMessages.Add(tagInit);
                            break;
                        case "2":
                            data.WebSocketMessages.Add(tagInit);
                            break;
                        default:
                            await socket.SendAsync(new ArraySegment<byte>("Invalid type"u8.ToArray()), WebSocketMessageType.Text, true, CancellationToken.None);
                            continue;
                    }
                    data.Messages.TryAdd(tagInit, valueInit);
                    await socket.SendAsync(new ArraySegment<byte>("OK"u8.ToArray()), WebSocketMessageType.Text, true, CancellationToken.None);
                    break;
                case "get":
                    if (split.Length != 2)
                    {
                        await socket.SendAsync(new ArraySegment<byte>("Invalid message, type G"u8.ToArray()), WebSocketMessageType.Text, true, CancellationToken.None);
                        continue;
                    }
                    if (!data.Messages.ContainsKey(split[1]))
                    {
                        await socket.SendAsync(new ArraySegment<byte>("Tag not found, type G"u8.ToArray()), WebSocketMessageType.Text, true, CancellationToken.None);
                        continue;
                    }
                    await socket.SendAsync(new ArraySegment<byte>(Encoding.UTF8.GetBytes(data.Messages[split[1]])), WebSocketMessageType.Text, true, CancellationToken.None);
                    break;
                case "set":
                    if (split.Length != 3)
                    {
                        await socket.SendAsync(new ArraySegment<byte>("Invalid message, type S"u8.ToArray()), WebSocketMessageType.Text, true, CancellationToken.None);
                        continue;
                    }
                    var tag = split[1];
                    var value = split[2];
                    if (!data.WebSocketMessages.Contains(tag))
                    {
                        data.WebSocketMessages.Add(tag);
                    }
                    data.Messages[tag] = value;
                    await socket.SendAsync(new ArraySegment<byte>("OK"u8.ToArray()), WebSocketMessageType.Text, true, CancellationToken.None);
                    break;
                case "delete":
                    if (split.Length != 2)
                    {
                        await socket.SendAsync(new ArraySegment<byte>("Invalid message, type D"u8.ToArray()), WebSocketMessageType.Text, true, CancellationToken.None);
                        continue;
                    }
                    if (!data.Messages.ContainsKey(split[1]))
                    {
                        await socket.SendAsync(new ArraySegment<byte>("Tag not found, type D"u8.ToArray()), WebSocketMessageType.Text, true, CancellationToken.None);
                        continue;
                    }
                    data.Messages.Remove(split[1]);
                    await socket.SendAsync(new ArraySegment<byte>("OK"u8.ToArray()), WebSocketMessageType.Text, true, CancellationToken.None);
                    break;
                default:
                    await socket.SendAsync(new ArraySegment<byte>("Invalid message, type X"u8.ToArray()), WebSocketMessageType.Text, true, CancellationToken.None);
                    continue;
            }
        }
    }
}