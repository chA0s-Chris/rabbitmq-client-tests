// Copyright (c) 2025 Christian Flessa. All rights reserved.
// This file is licensed under the MIT license. See LICENSE in the project root for more information.
namespace PubSub;

using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Shared;
using System.Text.Json;

public class Subscriber : IAsyncDisposable
{
    public Subscriber(RabbitMqSession session, List<Message> messages)
    {
        Session = session ?? throw new ArgumentNullException(nameof(session));
        ExpectedMessages = messages ?? throw new ArgumentNullException(nameof(messages));
    }

    private RabbitMqSession Session { get; }

    private List<Message> ExpectedMessages { get; }

    private List<Message> ReceivedMessages { get; } = [];

    public async ValueTask DisposeAsync() => await Session.DisposeAsync();

    public async Task RunAsync()
    {
        await Session.Channel.ExchangeDeclareAsync(Configuration.ExchangeName, ExchangeType.Topic, true, false);
        var queueResult = await Session.Channel.QueueDeclareAsync(Configuration.QueueName,
                                                                  true,
                                                                  false,
                                                                  false,
                                                                  new Dictionary<String, Object?>
                                                                  {
                                                                      ["x-queue-type"] = "quorum"
                                                                  });
        Console.WriteLine($"Declared queue: {queueResult.QueueName}  ({queueResult.MessageCount} messages)");

        await Session.Channel.QueueBindAsync(queueResult.QueueName, Configuration.ExchangeName, Configuration.RoutingKey);

        var consumer = new AsyncEventingBasicConsumer(Session.Channel);
        consumer.ReceivedAsync += OnReceivedAsync;

        var tag = await Session.Channel.BasicConsumeAsync(Configuration.QueueName, false, consumer);
        Console.WriteLine($"Consuming using tag: {tag}");
    }

    private async Task OnReceivedAsync(Object sender, BasicDeliverEventArgs args)
    {
        Console.WriteLine($"Received message: {args.DeliveryTag}");
        var message = JsonSerializer.Deserialize<Message>(args.Body.Span) ??
                      throw new InvalidOperationException();

        ReceivedMessages.Add(message);
        await Session.Channel.BasicAckAsync(args.DeliveryTag, false);

        var matchingMessages = ReceivedMessages.Count(x => ExpectedMessages.Contains(x));
        Console.WriteLine($"Received messages: {matchingMessages}/{ExpectedMessages.Count}");
    }
}
