using Dapper;
using Microsoft.Extensions.Options;
using Oracle.ManagedDataAccess.Client;
using OrdersManagement.Application.Interfaces;
using OrdersManagement.Domain.Entities;
using OrdersManagement.Domain.Enums;

namespace OrdersManagement.Infrastructure.Persistence.Oracle;

/// <summary>
/// Repositorio Oracle con Dapper. Es el único lugar donde se usa Dapper.
/// </summary>
public class OracleOrderRepository(IOptions<OracleOptions> options) : IOrderRepository
{
    private readonly string _connectionString = options.Value.ConnectionString;

    public async Task AddAsync(Order order, CancellationToken cancellationToken)
    {
        await using var connection = await OpenConnectionAsync(cancellationToken);
        await using var transaction = await connection.BeginTransactionAsync(cancellationToken);

        order.Id = await NextValueAsync(connection, transaction, "SEQ_ORDERS", cancellationToken);

        await connection.ExecuteAsync(new CommandDefinition(
            @"INSERT INTO Orders (Id, Customer, Total, Status, CreatedAt)
              VALUES (:Id, :Customer, :Total, :Status, :CreatedAt)",
            new
            {
                order.Id,
                order.Customer,
                order.Total,
                Status = order.Status.ToString(),
                order.CreatedAt
            },
            transaction,
            cancellationToken: cancellationToken));

        foreach (var item in order.Items)
        {
            item.Id = await NextValueAsync(connection, transaction, "SEQ_ORDERITEMS", cancellationToken);
            item.OrderId = order.Id;

            await connection.ExecuteAsync(new CommandDefinition(
                @"INSERT INTO OrderItems (Id, OrderId, Description, Quantity, UnitPrice)
                  VALUES (:Id, :OrderId, :Description, :Quantity, :UnitPrice)",
                new { item.Id, item.OrderId, item.Description, item.Quantity, item.UnitPrice },
                transaction,
                cancellationToken: cancellationToken));
        }

        await transaction.CommitAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Order>> GetPagedAsync(int pageNumber, int pageSize, CancellationToken cancellationToken)
    {
        await using var connection = await OpenConnectionAsync(cancellationToken);

        var rows = (await connection.QueryAsync<OrderRow>(new CommandDefinition(
            @"SELECT Id, Customer, Total, Status, CreatedAt
              FROM Orders
              ORDER BY CreatedAt DESC, Id DESC
              OFFSET :Offset ROWS FETCH NEXT :PageSize ROWS ONLY",
            new { Offset = (pageNumber - 1) * pageSize, PageSize = pageSize },
            cancellationToken: cancellationToken))).ToList();

        if (rows.Count == 0)
        {
            return new List<Order>();
        }

        var items = await connection.QueryAsync<OrderItem>(new CommandDefinition(
            @"SELECT Id, OrderId, Description, Quantity, UnitPrice
              FROM OrderItems
              WHERE OrderId IN :OrderIds
              ORDER BY Id",
            new { OrderIds = rows.Select(r => r.Id).ToList() },
            cancellationToken: cancellationToken));

        var itemsByOrder = items.ToLookup(i => i.OrderId);

        return rows.Select(row => ToOrder(row, itemsByOrder[row.Id])).ToList();
    }

    public async Task<int> CountAsync(CancellationToken cancellationToken)
    {
        await using var connection = await OpenConnectionAsync(cancellationToken);

        return await connection.ExecuteScalarAsync<int>(new CommandDefinition(
            "SELECT COUNT(*) FROM Orders",
            cancellationToken: cancellationToken));
    }

    public async Task<Order?> GetByIdAsync(long id, CancellationToken cancellationToken)
    {
        await using var connection = await OpenConnectionAsync(cancellationToken);

        var row = await connection.QuerySingleOrDefaultAsync<OrderRow>(new CommandDefinition(
            "SELECT Id, Customer, Total, Status, CreatedAt FROM Orders WHERE Id = :Id",
            new { Id = id },
            cancellationToken: cancellationToken));

        if (row is null)
        {
            return null;
        }

        var items = await connection.QueryAsync<OrderItem>(new CommandDefinition(
            @"SELECT Id, OrderId, Description, Quantity, UnitPrice
              FROM OrderItems
              WHERE OrderId = :OrderId
              ORDER BY Id",
            new { OrderId = id },
            cancellationToken: cancellationToken));

        return ToOrder(row, items);
    }

    public async Task UpdateStatusAsync(long id, OrderStatus status, CancellationToken cancellationToken)
    {
        await using var connection = await OpenConnectionAsync(cancellationToken);

        await connection.ExecuteAsync(new CommandDefinition(
            "UPDATE Orders SET Status = :Status WHERE Id = :Id",
            new { Status = status.ToString(), Id = id },
            cancellationToken: cancellationToken));
    }

    private async Task<OracleConnection> OpenConnectionAsync(CancellationToken cancellationToken)
    {
        var connection = new OracleConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);
        return connection;
    }

    private static Task<long> NextValueAsync(OracleConnection connection, System.Data.Common.DbTransaction transaction, string sequence, CancellationToken cancellationToken)
    {
        // El nombre de la secuencia es una constante interna, no proviene de la entrada del usuario.
        return connection.ExecuteScalarAsync<long>(new CommandDefinition(
            $"SELECT {sequence}.NEXTVAL FROM DUAL",
            transaction: transaction,
            cancellationToken: cancellationToken));
    }

    private static Order ToOrder(OrderRow row, IEnumerable<OrderItem> items)
    {
        return Order.Restore(
            row.Id,
            row.Customer,
            items,
            row.Total,
            Enum.Parse<OrderStatus>(row.Status),
            row.CreatedAt);
    }

    // Modelo de persistencia: refleja las columnas de la tabla Orders tal como las lee Dapper.
    private class OrderRow
    {
        public long Id { get; set; }

        public string Customer { get; set; } = string.Empty;

        public decimal Total { get; set; }

        public string Status { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; }
    }
}
