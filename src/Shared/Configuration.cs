// Copyright (c) 2025 Christian Flessa. All rights reserved.
// This file is licensed under the MIT license. See LICENSE in the project root for more information.
namespace Shared;

public static class Configuration
{
    public const String ExchangeName = "TestExchange";

    public const String QueueName = "TestQueue";

    public const String RoutingKey = "TestRoutingKey";

    public static Uri ServerUri { get; } = new("amqp://guest:guest@localhost:5672/");
}
