using Dapper;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Npgsql;

namespace Warehouse.Web.Reporting.Integrations;
internal class AgentRemainsIngestionService
{
    private readonly ILogger<AgentRemainsIngestionService> _logger;
    private readonly string _connString;
    private static bool _ensureTableCreated = false;

    public AgentRemainsIngestionService(IConfiguration config,
      ILogger<AgentRemainsIngestionService> logger)
    {
        _connString = config.GetConnectionString("ReportingConnectionString")!;
        _logger = logger;
    }

    private async Task CreateTableAsync()
    {
        try
        {
            string sql = @"
                CREATE EXTENSION IF NOT EXISTS pgcrypto;

                CREATE SCHEMA IF NOT EXISTS reporting;

                CREATE TABLE IF NOT EXISTS reporting.agent_remains (
                    id           uuid            PRIMARY KEY DEFAULT gen_random_uuid(),
                    store_id     bigint,
                    store_name   varchar(50),
                    manager_id   bigint,
                    manager_name varchar(50),
                    agent_id     bigint,
                    agent_name   varchar(50),
                    object_id    bigint,
                    object_code  integer,
                    object_name  varchar(50),
                    object_type  smallint,
                    amount       numeric(18,2),
                    discount     numeric(18,2),
                    date         timestamp     NOT NULL
                );

                -- Полезные индексы (по ситуации)
               CREATE UNIQUE INDEX IF NOT EXISTS uq_agent_remains_object ON reporting.agent_remains (object_id, object_name);
               CREATE INDEX IF NOT EXISTS idx_agent_remains_date ON reporting.agent_remains(date);
            ";
            using var conn = new NpgsqlConnection(_connString);
            _logger.LogInformation("Executing query: {sql}", sql);

            await conn.ExecuteAsync(sql);
        }
        catch (Exception ex)
        {
            throw;
        }
        finally
        {
            _ensureTableCreated = true;
        }
    }


    public async Task AddReportAsync(AgentRemains remains)
    {
        if (!_ensureTableCreated) await CreateTableAsync();

        var sql = @"INSERT INTO reporting.agent_remains
            (store_id, store_name, manager_id, manager_name, agent_id, agent_name, 
             object_id, object_code, object_name, object_type, amount, discount, date)
        VALUES
            (@StoreId, @StoreName, @ManagerId, @ManagerName, @AgentId, @AgentName, 
             @ObjectId, @ObjectCode, @ObjectName, @ObjectType, @Amount, @Discount, @Date)
        ON CONFLICT (object_id, object_name)
        DO UPDATE SET
            store_id     = EXCLUDED.store_id,
            store_name   = EXCLUDED.store_name,
            manager_id   = EXCLUDED.manager_id,
            manager_name = EXCLUDED.manager_name,
            agent_id     = EXCLUDED.agent_id,
            agent_name   = EXCLUDED.agent_name,
            object_code  = EXCLUDED.object_code,
            object_type  = EXCLUDED.object_type,
            amount       = EXCLUDED.amount,
            discount     = EXCLUDED.discount,
            date         = EXCLUDED.date
        RETURNING id, date;";

        using var conn = new NpgsqlConnection(_connString);
        _logger.LogInformation("Executing query: {sql}", sql);
        await conn.ExecuteAsync(sql, new
        {
            remains.StoreId,
            remains.StoreName,
            remains.ManagerId,
            remains.ManagerName,
            remains.AgentId,
            remains.AgentName,
            remains.ObjectId,
            remains.ObjectCode,
            remains.ObjectName,
            remains.ObjectType,
            remains.Amount,
            remains.Discount,
            remains.Date,
        });
    }

    public async Task<bool> UpdateReportAsync(AgentRemains remains)
    {
        if (remains == null) throw new ArgumentNullException(nameof(remains));

        if (!_ensureTableCreated) await CreateTableAsync();

        var sql = @"
            UPDATE reporting.agent_remains
            SET
                store_id     = @StoreId,
                store_name   = @StoreName,
                manager_id   = @ManagerId,
                manager_name = @ManagerName,
                agent_id     = @AgentId,
                agent_name   = @AgentName,
                object_code  = @ObjectCode,
                object_type  = @ObjectType,
                amount       = @Amount,
                discount     = @Discount,
                date         = @Date
            WHERE object_id = @ObjectId AND object_type = @ObjectType
            RETURNING id;
            ";

        await using var conn = new NpgsqlConnection(_connString);
        await conn.OpenAsync();

        var id = await conn.QuerySingleOrDefaultAsync<Guid?>(
            sql,
            new
            {
                remains.StoreId,
                remains.StoreName,
                remains.ManagerId,
                remains.ManagerName,
                remains.AgentId,
                remains.AgentName,
                remains.ObjectId,
                remains.ObjectCode,
                remains.ObjectName,
                remains.ObjectType,
                remains.Amount,
                remains.Discount,
                remains.Date
            });

        return id.HasValue;
    }

    public async Task<int> DeleteReportAsync(long objectId, int objectType)
    {
        if (!_ensureTableCreated) await CreateTableAsync();

        var sql = @"
            DELETE FROM reporting.agent_remains
            WHERE object_id = @ObjectId AND object_type = @ObjectType;";

        await using var conn = new NpgsqlConnection(_connString);
        await conn.OpenAsync();

        var rows = await conn.ExecuteAsync(sql, new { ObjectId = objectId, ObjectType = objectType });
        return rows;
    }

    public async Task<IEnumerable<AgentRemains>> GetDebtsByIdAsync(long agentId)
    {
        try
        {
            if (!_ensureTableCreated) await CreateTableAsync();

            const string sql = @"
            SELECT id,
                   store_id AS StoreId,
                   store_name AS StoreName,
                   manager_id AS ManagerId,
                   manager_name AS ManagerName,
                   agent_id AS AgentId,
                   agent_name AS AgentName,
                   object_id AS ObjectId,
                   object_code AS ObjectCode,
                   object_name AS ObjectName,
                   object_type AS ObjectType,
                   amount AS Amount,
                   discount AS Discount,
                   date AS Date
            FROM reporting.agent_remains
            WHERE agent_id = @AgentId
            ORDER BY date DESC;";

            using var conn = new NpgsqlConnection(_connString);
            return await conn.QueryAsync<AgentRemains>(sql, param: new { AgentId = agentId });
        }
        catch (Exception ex)
        {
            _logger.LogError($"Executing query error: {ex}");
            throw;
        }
    }

    public async Task<IEnumerable<AgentRemains>> GetAllDebtsAsync()
    {
        try
        {
            if (!_ensureTableCreated) await CreateTableAsync();

            const string sql = @"
            SELECT id,
                   store_id AS StoreId,
                   store_name AS StoreName,
                   manager_id AS ManagerId,
                   manager_name AS ManagerName,
                   agent_id AS AgentId,
                   agent_name AS AgentName,
                   object_id AS ObjectId,
                   object_code AS ObjectCode,
                   object_name AS ObjectName,
                   object_type AS ObjectType,
                   amount AS Amount,
                   discount AS Discount,
                   date AS Date
            FROM reporting.agent_remains
            ORDER BY date DESC;";

            using var conn = new NpgsqlConnection(_connString);
            return await conn.QueryAsync<AgentRemains>(sql);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Executing query error: {ex}");
            throw;
        }
    }
}
