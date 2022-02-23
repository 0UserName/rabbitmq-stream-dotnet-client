// This source code is dual-licensed under the Apache License, version
// 2.0, and the Mozilla Public License, version 2.0.
// Copyright (c) 2007-2020 VMware, Inc.

using System;
using System.Diagnostics.Tracing;

namespace RabbitMQ.Stream.Client;

public static class Keywords
{
    public const EventKeywords Log = (EventKeywords)1;
}

[EventSource(Name = "rabbitmq-client-stream")]
internal sealed class LogEventSource : EventSource
{
    private static readonly char[] _newLineChars = Environment.NewLine.ToCharArray();

    /// <summary>
    ///     Default <see cref="LogEventSource" /> implementation for logging.
    /// </summary>
    public static readonly LogEventSource Log = new();

    private LogEventSource() : base(EventSourceSettings.EtwSelfDescribingEventFormat)
    {
    }

    /// <summary>
    /// </summary>
    [NonEvent]
    private string ConvertToString(Exception exception)
    {
        return exception?.ToString();
    }

    /// <summary>
    ///     Writes an informational log message.
    /// </summary>
    /// <param name="message">
    /// </param>
    [Event(1, Level = EventLevel.Informational)]
    public LogEventSource LogInformation(string message)
    {
        if (IsEnabled())
        {
            WriteEvent(1, message);
        }

        return this;
    }

    /// <summary>
    ///     Writes a warning log message.
    /// </summary>
    /// <param name="message">
    /// </param>
    [Event(2, Level = EventLevel.Warning)]
    public LogEventSource LogWarning(string message)
    {
        if (IsEnabled())
        {
            WriteEvent(2, message);
        }

        return this;
    }

    /// <summary>
    ///     Writes an error log message.
    /// </summary>
    /// <param name="message">
    /// </param>
    [Event(3, Level = EventLevel.Error)]
    public LogEventSource LogError(string message)
    {
        if (IsEnabled())
        {
            WriteEvent(3, message);
        }

        return this;
    }

    /// <summary>
    ///     Writes an error log message.
    /// </summary>
    /// <param name="message">
    /// </param>
    /// <param name="exception">
    ///     The exception to log.
    /// </param>
    [NonEvent]
    public LogEventSource LogError(string message, Exception exception)
    {
        LogError($"{message}{Environment.NewLine}{ConvertToString(exception)}".Trim(_newLineChars));

        return this;
    }
}
