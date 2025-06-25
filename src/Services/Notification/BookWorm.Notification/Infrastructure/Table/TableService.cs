using Azure.Data.Tables;

namespace BookWorm.Notification.Infrastructure.Table;

public sealed class TableService : ITableService
{
    private readonly TableClient _tableClient;
    private readonly string _tableName = nameof(Notification).ToLowerInvariant();

    public TableService(TableServiceClient client)
    {
        client.CreateTableIfNotExists(_tableName);
        _tableClient = client.GetTableClient(_tableName);
    }

    public async Task<Guid> UpsertAsync<T>(
        T entity,
        string partitionKey,
        CancellationToken cancellationToken = default
    )
        where T : class
    {
        var idProperty = typeof(T).GetProperty("Id");

        var entityId = idProperty?.GetValue(entity) is Guid id ? id : Guid.CreateVersion7();

        var tableEntity = new TableEntity(partitionKey, entityId.ToString())
        {
            { nameof(T).ToLowerInvariant(), JsonSerializer.Serialize(entity) },
        };

        await _tableClient.UpsertEntityAsync(tableEntity, cancellationToken: cancellationToken);

        return entityId;
    }

    public async Task<IEnumerable<T>> ListAsync<T>(
        string partitionKey,
        CancellationToken cancellationToken = default
    )
        where T : class
    {
        var tableEntity = _tableClient.QueryAsync<TableEntity>(
            TableClient.CreateQueryFilter($"{nameof(TableEntity.PartitionKey)} eq {partitionKey}"),
            cancellationToken: cancellationToken
        );

        List<T> entities = [];

        await foreach (var entity in tableEntity)
        {
            var json = entity[nameof(T).ToLowerInvariant()].ToString();

            if (json is null)
            {
                continue;
            }

            var item = JsonSerializer.Deserialize<T>(json);
            if (item is not null)
            {
                entities.Add(item);
            }
        }

        return entities;
    }

    public async Task DeleteAsync(
        string partitionKey,
        string rowKey,
        CancellationToken cancellationToken = default
    )
    {
        await _tableClient.DeleteEntityAsync(
            partitionKey,
            rowKey,
            cancellationToken: cancellationToken
        );
    }

    public async Task BulkDeleteAsync(
        string partitionKey,
        CancellationToken cancellationToken = default
    )
    {
        var entities = _tableClient.QueryAsync<TableEntity>(
            TableClient.CreateQueryFilter($"{nameof(TableEntity.PartitionKey)} eq {partitionKey}"),
            select: [nameof(TableEntity.PartitionKey), nameof(TableEntity.RowKey)],
            cancellationToken: cancellationToken
        );

        List<TableEntity> entitiesToDelete = [];

        await foreach (var entity in entities)
        {
            entitiesToDelete.Add(entity);
        }

        if (entitiesToDelete.Count == 0)
        {
            return;
        }

        // Azure Table Storage supports batch operations with up to 100 entities per batch
        // and all entities in a batch must have the same partition key
        const int batchSize = 100;

        for (var i = 0; i < entitiesToDelete.Count; i += batchSize)
        {
            var batch = entitiesToDelete.Skip(i).Take(batchSize);
            var batchOperations = batch
                .Select(entity => new TableTransactionAction(
                    TableTransactionActionType.Delete,
                    entity
                ))
                .ToList();

            if (batchOperations.Count > 0)
            {
                await _tableClient
                    .SubmitTransactionAsync(batchOperations, cancellationToken)
                    .ConfigureAwait(false);
            }
        }
    }
}
