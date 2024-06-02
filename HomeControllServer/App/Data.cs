namespace HomeControllServer.App;

public class Data
{
    public List<string> HttpMessages { get; } = [];
    public List<string> WebSocketMessages { get; } = [];
    public Dictionary<string, string> Messages { get; } = new();
}