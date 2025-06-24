// Copyright (c) 2025 Christian Flessa. All rights reserved.
// This file is licensed under the MIT license. See LICENSE in the project root for more information.
namespace Shared;

using RabbitMQ.Client;

public class RabbitMqSession : IAsyncDisposable
{
    private RabbitMqSession(IConnection connection, IChannel channel)
    {
        Connection = connection;
        Channel = channel;
    }

    public IConnection Connection { get; private set; }

    public IChannel Channel { get; private set; }

    public async ValueTask DisposeAsync()
    {
        await Channel.DisposeAsync();
        await Connection.DisposeAsync();
    }

    public static async Task<RabbitMqSession> CreateAsync(IConnectionFactory factory)
    {
        ArgumentNullException.ThrowIfNull(factory);

        var connection = await factory.CreateConnectionAsync();
        var channel = await connection.CreateChannelAsync(new(true,
                                                              true,
                                                              consumerDispatchConcurrency: 1));

        return new(connection, channel);
    }
}
