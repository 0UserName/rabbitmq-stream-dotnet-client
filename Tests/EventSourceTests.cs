// This source code is dual-licensed under the Apache License, version
// 2.0, and the Mozilla Public License, version 2.0.
// Copyright (c) 2007-2020 VMware, Inc.

using System;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using RabbitMQ.Stream.Client;
using Xunit;

namespace Tests;

public static class EventSourceTestsHelper
{
    
    public static void ValidateSimpleEventData(this EventWrittenEventArgs args, EventLevel level,
        string expectedPayloadName,
        string expectedPayloadText)
    {
        Assert.Equal(level,
            args.Level);

        Assert.Equal(expectedPayloadName,
            args.PayloadNames[0]);

        Assert.Equal(expectedPayloadText,
            args.Payload[0]);
    }

    public static void ValidateExceptionEventData(this EventWrittenEventArgs args, EventLevel level,
        string expectedPayloadName,
        IEnumerable<string> expectedPayloadText)
    {
        Assert.Equal(level,
            args.Level);

        Assert.Equal(expectedPayloadName,
            args.PayloadNames[0]);

        foreach (var payloadText in expectedPayloadText)
        {
            Assert.Contains(payloadText, args.Payload[0].ToString(), StringComparison.OrdinalIgnoreCase);
        }
    }
}

public class EventSourceTests
{
    /// <summary>
    ///     The name of the argument whose value is used as the payload when writing the event.
    /// </summary>
    private const string ExpectedPayloadName = "message";

    /// <summary>
    ///     Verifies the contents of the event
    ///     object: event level match, payload
    ///     name match, payload match.
    /// </summary>
    [Fact]
    public void GenerateInfoEvent()
    {
        Exception resultException = default;

        new LogEventListener().EventWritten += (sender, args) =>
        {
            resultException = Record.Exception(() =>
                args.ValidateSimpleEventData(EventLevel.Informational, ExpectedPayloadName, nameof(GenerateInfoEvent)));
        };

        LogEventSource.Log.LogInformation(nameof(GenerateInfoEvent));

        Assert.Null(resultException);
    }

    /// <summary>
    ///     Verifies the contents of the event
    ///     object: event level match, payload
    ///     name match, payload match.
    /// </summary>
    [Fact]
    public void GenerateWarningEvent()
    {
        Exception resultException = default;

        new LogEventListener().EventWritten += (sender, args) =>
        {
            resultException = Record.Exception(() =>
                args.ValidateSimpleEventData(EventLevel.Warning, ExpectedPayloadName, nameof(GenerateWarningEvent)));
        };

        LogEventSource.Log.LogWarning(nameof(GenerateWarningEvent));

        Assert.Null(resultException);
    }

    /// <summary>
    ///     Verifies the contents of the event
    ///     object: event level match, payload
    ///     name match, payload match.
    /// </summary>
    [Fact]
    public void GenerateErrorEvent()
    {
        Exception resultException = default;

        new LogEventListener().EventWritten += (sender, args) =>
        {
            resultException = Record.Exception(() =>
                args.ValidateSimpleEventData(EventLevel.Error, ExpectedPayloadName, nameof(GenerateErrorEvent)));
        };

        LogEventSource.Log.LogError(nameof(GenerateErrorEvent));
        LogEventSource.Log.LogError(nameof(GenerateErrorEvent), default);

        Assert.Null(resultException);
    }

    /// <summary>
    ///     Verifies the contents of the event
    ///     object: event level match, payload
    ///     name match, payload match.
    /// </summary>
    [Fact]
    public void GenerateErrorWithExceptionEvent()
    {
        Exception resultException = default;

        const string exception1 = "TextExceptionMessage1";
        const string exception2 = "TextExceptionMessage2";

        var expectedSubstrings = new List<string> { nameof(GenerateErrorEvent), exception1, exception2, "dqaeqe" };

        new LogEventListener().EventWritten += (sender, args) =>
        {
            resultException = Record.Exception(() =>
                args.ValidateExceptionEventData(EventLevel.Error, ExpectedPayloadName, expectedSubstrings));
        };

        try
        {
            try
            {
                throw new Exception(exception1);
            }
            catch (Exception exception)
            {
                throw new Exception(exception2, exception);
            }
        }
        catch (Exception exception)
        {
            LogEventSource.Log.LogError(nameof(GenerateErrorEvent), exception);
        }

        Assert.Null(resultException);
    }
}
