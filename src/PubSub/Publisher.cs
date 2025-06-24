// Copyright (c) 2025 Christian Flessa. All rights reserved.
// This file is licensed under the MIT license. See LICENSE in the project root for more information.
namespace PubSub;

using RabbitMQ.Client;
using Shared;
using System.Text;
using System.Text.Json;

public class Publisher : IAsyncDisposable
{
    public Publisher(RabbitMqSession session)
    {
        Session = session ?? throw new ArgumentNullException(nameof(session));
    }

    private RabbitMqSession Session { get; }

    public async ValueTask DisposeAsync() => await Session.DisposeAsync();

    public async Task RunAsync(List<Message> messages)
    {
        Console.WriteLine($"Publishing {messages.Count} messages...");

        foreach (var message in messages)
        {
            await PublishMessageAsync(message);
        }
    }

    private async Task PublishMessageAsync(Message message)
    {
        var properties = new BasicProperties
        {
            ContentType = "application/json"
        };

        var json = JsonSerializer.Serialize(message);
        var data = Encoding.UTF8.GetBytes(json);
        var body = new ReadOnlyMemory<Byte>(data);

        await Session.Channel.BasicPublishAsync(Configuration.ExchangeName, Configuration.RoutingKey, false, properties, body);
    }
}
