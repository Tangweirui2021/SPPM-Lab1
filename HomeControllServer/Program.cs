// This one is B3.
using HomeControllServer.App;

var MyAllowSpecificOrigins = "_myAllowSpecificOrigins";

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy(name: MyAllowSpecificOrigins,
        policy =>
        {
            policy.AllowAnyOrigin();
            policy.AllowAnyHeader();
            policy.AllowAnyMethod();
        });
});

var app = builder.Build();

app.UseCors(MyAllowSpecificOrigins);

// Websocket
app.UseWebSockets();
// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
// app.UseHttpsRedirection();

var data = new Data();
var readMsg = Config.ReadMsgFile();
if (readMsg is not null)
{
    for (var i = 0; i < readMsg.Value.Item1.Count; i++)
    {
        var key = readMsg.Value.Item1.Keys.ElementAt(i);
        var value = readMsg.Value.Item1.Values.ElementAt(i);
        data.Messages.TryAdd(key, value);
        data.HttpMessages.Add(key);
        data.WebSocketMessages.Add(key);
    }
    for (var i = 0; i < readMsg.Value.Item2.Count; i++)
    {
        var key = readMsg.Value.Item2.Keys.ElementAt(i);
        var value = readMsg.Value.Item2.Values.ElementAt(i);
        if (!data.Messages.ContainsKey(key))
        {
            data.Messages.TryAdd(key, value);
            data.HttpMessages.Add(key);
        }
    }
    for (var i = 0; i < readMsg.Value.Item3.Count; i++)
    {
        var key = readMsg.Value.Item3.Keys.ElementAt(i);
        var value = readMsg.Value.Item3.Values.ElementAt(i);
        if (!data.Messages.ContainsKey(key))
        {
            data.Messages.TryAdd(key, value);
            data.WebSocketMessages.Add(key);
        }
    }
}

app.MapPost("/message/init", (string key, string value, int? type) =>
    {
        type ??= 0;
        if (data.HttpMessages.Contains(key) || data.WebSocketMessages.Contains(key))
        {
            return Results.BadRequest("Key type not match");
        }
        switch (type)
        {
            case 1:
                data.HttpMessages.Add(key);
                break;
            case 2:
                data.WebSocketMessages.Add(key);
                break;
            case 0:
                data.HttpMessages.Add(key);
                data.WebSocketMessages.Add(key);
                break;
            default:
                return Results.BadRequest("Invalid type");
        }
        data.Messages.TryAdd(key, value);
        return Results.Ok();
    })
    .WithName("MessageInit")
    .WithOpenApi();

app.MapPost("/message/set", (string key, string value, int? type) =>
    {
        type ??= 0;
        switch (type)
        {
            case 1:
                if (data.WebSocketMessages.Contains(key))
                {
                    return Results.BadRequest("Key type not match");
                }
                if (!data.HttpMessages.Contains(key))
                { 
                    data.HttpMessages.Add(key);
                }
                break;
            case 2:
                if (data.HttpMessages.Contains(key))
                {
                    return Results.BadRequest("Key type not match");
                }
                if (!data.WebSocketMessages.Contains(key))
                {
                    data.WebSocketMessages.Add(key);
                }
                break;
            case 0:
                if (!data.WebSocketMessages.Contains(key) && !data.HttpMessages.Contains(key))
                {
                    data.HttpMessages.Add(key);
                    data.WebSocketMessages.Add(key);
                }
                data.HttpMessages.Add(key);
                data.WebSocketMessages.Add(key);
                break;
            default:
                return Results.BadRequest("Invalid type");
        }
        data.Messages[key] = value;
        return Results.Ok();
    })
    .WithName("MessageSet")
    .WithOpenApi();

app.MapPost("/message/get", (string key) => 
        !data.HttpMessages.Contains(key) ? 
            Results.BadRequest("Key not found") : 
            Results.Ok(data.Messages[key]))
    .WithName("MessageGet")
    .WithOpenApi();

app.MapPost("/message/delete", (string key) =>
    {
        if (!data.HttpMessages.Contains(key) && !data.WebSocketMessages.Contains(key))
        {
            return Results.BadRequest("Key not found");
        }
        data.Messages.Remove(key);
        data.HttpMessages.Remove(key);
        data.WebSocketMessages.Remove(key);
        return Results.Ok();
    })
    .WithName("MessageDelete")
    .WithOpenApi();

app.MapPost("/reload", () =>
    {
        var readMsgReload = Config.ReadMsgFile();
        if (readMsgReload is null) 
            return Results.Ok();
        for (var i = 0; i < readMsgReload.Value.Item1.Count; i++)
        {
            var key = readMsgReload.Value.Item1.Keys.ElementAt(i);
            var value = readMsgReload.Value.Item1.Values.ElementAt(i);
            if (!data.Messages.ContainsKey(key))
            {
                data.Messages.TryAdd(key, value);
            } else {
                data.Messages[key] = value;
            }
            if (!data.HttpMessages.Contains(key))
            {
                data.HttpMessages.Add(key);
            }
            if (!data.WebSocketMessages.Contains(key))
            {
                data.WebSocketMessages.Add(key);
            }
        }
        for (var i = 0; i < readMsgReload.Value.Item2.Count; i++)
        {
            var key = readMsgReload.Value.Item2.Keys.ElementAt(i);
            var value = readMsgReload.Value.Item2.Values.ElementAt(i);
            if (!data.Messages.ContainsKey(key))
            {
                data.Messages.TryAdd(key, value);
            } else {
                data.Messages[key] = value;
            }
            if (!data.HttpMessages.Contains(key))
            {
                data.HttpMessages.Add(key);
            }
        }
        for (var i = 0; i < readMsgReload.Value.Item3.Count; i++)
        {
            var key = readMsgReload.Value.Item3.Keys.ElementAt(i);
            var value = readMsgReload.Value.Item3.Values.ElementAt(i);
            if (!data.Messages.ContainsKey(key))
            {
                data.Messages.TryAdd(key, value);
            } else {
                data.Messages[key] = value;
            }
            if (!data.WebSocketMessages.Contains(key))
            {
                data.WebSocketMessages.Add(key);
            }
        }
        return Results.Ok();
    })
    .WithName("Reload")
    .WithOpenApi();

app.Use(async (context, next) =>
{
    if (context.Request.Path == "/message/ws")
    {
        if (context.WebSockets.IsWebSocketRequest)
        {
            using var webSocket = await context.WebSockets.AcceptWebSocketAsync();
            await Message.HandleWebSocket(webSocket, data);
        }
        else
        {
            context.Response.StatusCode = StatusCodes.Status400BadRequest;
        }
    }
    else
    {
        await next(context);
    }
});

app.Run();
