using Dapper;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Npgsql;

namespace Warehouse.Web.Reporting.Integrations;
internal class HistoryIngestionService
{
    private readonly ILogger<HistoryIngestionService> _logger;
    private readonly string _connString;
    private static bool _ensureTableCreated = false;

    public HistoryIngestionService(IConfiguration config,
      ILogger<HistoryIngestionService> logger)
    {
        _connString = config.GetConnectionString("ReportingConnectionString")!;
        _logger = logger;
    }

    private async Task CreateTableAsync()
    {
        try
        {
            string sql = @"
                -- (опционально) для DEFAULT gen_random_uuid()
                CREATE EXTENSION IF NOT EXISTS pgcrypto;

                CREATE SCHEMA IF NOT EXISTS reporting;

                CREATE TABLE IF NOT EXISTS reporting.history
                (
                    id          uuid PRIMARY KEY DEFAULT gen_random_uuid(),
                    store_name    varchar(50),
                    user_name     varchar(50),
                    method      smallint,
                    object_name varchar(50),
                    -- если это JSON, лучше jsonb:
                    old_data    jsonb NULL,
                    new_data    jsonb NULL,
                    object_id   bigint,
                    object_store_name varchar(100),
                    object_manager_name varchar(100),
                    object_agent_name varchar(100),
                    created_date timestamp NOT NULL DEFAULT now()
                );

                -- Полезные индексы (по ситуации)
                CREATE INDEX IF NOT EXISTS ix_history_store_name       ON reporting.history (store_name);
                CREATE INDEX IF NOT EXISTS ix_history_user_name       ON reporting.history (user_name);
                CREATE INDEX IF NOT EXISTS ix_history_object         ON reporting.history (object_name, object_id);
                CREATE INDEX IF NOT EXISTS ix_history_created_date   ON reporting.history (created_date);
            ";
            using var conn = new NpgsqlConnection(_connString);
            _logger.LogInformation("Executing query: {sql}", sql);

            await conn.ExecuteAsync(sql);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create reporting history table.");
            throw;
        }
        finally
        {
            _ensureTableCreated = true;
        }
    }


    public async Task AddHistoryAsync(History history)
    {
        if (!_ensureTableCreated) await CreateTableAsync();

        var sql = @"INSERT INTO reporting.history
                (store_name, user_name, method, object_name, old_data, new_data, object_id, object_store_name, object_manager_name, object_agent_name)
            VALUES
                (@StoreName, @UserName, @Method, @ObjectName, @OldData::jsonb, @NewData::jsonb, @ObjectId, @ObjectStoreName, @ObjectManagerName, @ObjectAgentName)
            RETURNING id, created_date;";

        using var conn = new NpgsqlConnection(_connString);
        _logger.LogInformation("Executing query: {sql}", sql);
        await conn.ExecuteAsync(sql, new
        {
            history.StoreName,
            history.UserName,
            history.Method,
            history.ObjectName,
            history.OldData,
            history.NewData,
            history.ObjectId,
            history.CreatedDate,
            history.ObjectStoreName,
            history.ObjectManagerName,
            history.ObjectAgentName
        });
    }


    public async Task<IEnumerable<History>> GetHistoriesAsync()
    {
        try
        {
            if (!_ensureTableCreated) await CreateTableAsync();

            const string sql = @"
                SELECT 
                    id AS Id,
                    store_name   AS StoreName,
                    user_name    AS UserName,
                    method       AS Method,
                    object_name  AS ObjectName,
                    old_data::text AS OldData,
                    new_data::text AS NewData,
                    object_id    AS ObjectId,
                    created_date AS CreatedDate,
                    object_store_name    AS ObjectStoreName,
                    object_manager_name    AS ObjectManagerName,
                    object_agent_name    AS ObjectAgentName
                FROM reporting.history order by created_date desc;";

            using var conn = new NpgsqlConnection(_connString);
            return await conn.QueryAsync<History>(sql);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Executing query error: {ex}");
            throw;
        }
    }
}
