﻿using Marten.Services;

namespace BookWorm.Ordering.Infrastructure.Marten;

/// <summary>
///     ref: https://github.com/oskardudycz/EventSourcing.NetCore/blob/main/Core.Marten/MartenConfig.cs
/// </summary>
public sealed class StoreConfigs
{
    public static StoreOptions SetStoreOptions(MartenConfigs martenConfig)
    {
        var options = new StoreOptions();
        var schemaName = Environment.GetEnvironmentVariable("SchemaName");

        options.Events.DatabaseSchemaName = schemaName ?? martenConfig.WriteModelSchema;
        options.DatabaseSchemaName = schemaName ?? martenConfig.ReadModelSchema;

        options.UseSystemTextJsonForSerialization(EnumStorage.AsString);

        options.Projections.Errors.SkipApplyErrors = false;
        options.Projections.Errors.SkipSerializationErrors = false;
        options.Projections.Errors.SkipUnknownEvents = false;

        if (martenConfig.UseMetadata)
        {
            options.Events.MetadataConfig.CausationIdEnabled = true;
            options.Events.MetadataConfig.CorrelationIdEnabled = true;
            options.Events.MetadataConfig.HeadersEnabled = true;
        }

        // Turn on Otel tracing for connection activity, and
        // also tag events to each span for all the Marten "write"
        // operations
        options.OpenTelemetry.TrackConnections = TrackLevel.Normal;

        // This opts into exporting a counter just on the number
        // of events being appended. Kinda a duplication
        options.OpenTelemetry.TrackEventCounters();

        return options;
    }
}
