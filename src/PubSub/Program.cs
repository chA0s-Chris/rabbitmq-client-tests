// Copyright (c) 2025 Christian Flessa. All rights reserved.
// This file is licensed under the MIT license. See LICENSE in the project root for more information.
using PubSub;
using RabbitMQ.Client;
using Shared;

var connectionFactory = new ConnectionFactory
{
    Endpoint = new(Configuration.ServerUri)
};

var messages = Enumerable.Range(1, 20)
                         .Select(_ => new Message(Guid.CreateVersion7()))
                         .ToList();

try
{
    var subscriberSession = await RabbitMqSession.CreateAsync(connectionFactory);
    await using var subscriber = new Subscriber(subscriberSession, messages);
    await subscriber.RunAsync();

    var publisherSession = await RabbitMqSession.CreateAsync(connectionFactory);
    await using var publisher = new Publisher(publisherSession);
    await publisher.RunAsync(messages);

    Console.WriteLine("###########");
    await Task.Delay(Timeout.Infinite);
}
catch (Exception e)
{
    Console.WriteLine(e);
}
