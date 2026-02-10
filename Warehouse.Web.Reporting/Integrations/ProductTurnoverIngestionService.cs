using Dapper;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Npgsql;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Warehouse.Web.Reporting.Integrations;

internal class ProductTurnoverIngestionService
{
    private readonly ILogger<ProductTurnoverIngestionService> _logger;
    private readonly string _connString;
    private static bool _ensureTableCreated = false;

    public ProductTurnoverIngestionService(IConfiguration config,
      ILogger<ProductTurnoverIngestionService> logger)
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

                -- Таблица шапки (оборот по объекту/агенту/магазину)
                CREATE TABLE IF NOT EXISTS reporting.product_turnovers (
                    id            uuid PRIMARY KEY DEFAULT gen_random_uuid(),
                    store_id      bigint           NOT NULL,
                    store_name    varchar(50),
                    -- to_store_id      bigint,
                    -- to_store_name    varchar(50),
                    manager_id    bigint,
                    manager_name  varchar(50),
                    manager_phone  varchar(15),
                    agent_id      bigint,
                    agent_name    varchar(50),
                    agent_phone    varchar(50),
                    agent_address    varchar(50),
                    object_id     bigint           NOT NULL,
                    object_parent_id     bigint           NOT NULL,
                    object_code   integer,
                    object_name   varchar(50),
                    object_type   integer,
                    is_received   boolean NOT NULL DEFAULT false,
                    amount        numeric(18,2),
                    discount      numeric(18,2),
                    date          timestamp      NOT NULL
                );

                -- Таблица товаров (связана с оборотом)
                CREATE TABLE IF NOT EXISTS reporting.products (
                    id            uuid PRIMARY KEY DEFAULT gen_random_uuid(),
                    product_id    bigint           NOT NULL,
                    product_code    int           NOT NULL,
                    product_name  varchar(200),
                    manufacturer  varchar(100),
                    unit          varchar(20),
                    quantity         integer,
                    price     numeric(18,2),
                    buy_price     numeric(18,2),
                    sell_price    numeric(18,2),
                    difference  integer,

                    turnover_id   uuid NOT NULL REFERENCES reporting.product_turnovers(id) ON DELETE CASCADE
                );

                -- Индексы для ускорения выборок
                CREATE UNIQUE INDEX IF NOT EXISTS uq_product_turnovers_object ON reporting.product_turnovers (object_id, object_name);
                CREATE INDEX IF NOT EXISTS idx_turnovers_date ON reporting.product_turnovers(date);
                CREATE INDEX IF NOT EXISTS idx_turnovers_store ON reporting.product_turnovers(store_id);
                CREATE INDEX IF NOT EXISTS idx_turnovers_agent ON reporting.product_turnovers(agent_id);
                CREATE INDEX IF NOT EXISTS idx_products_turnover ON reporting.products(turnover_id);
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


    public async Task AddAsync(ProductTurnover turnover)
    {
        if (turnover == null) throw new ArgumentNullException(nameof(turnover));
        if (turnover.Products == null) turnover.Products = new List<Product>();

        if (!_ensureTableCreated) await CreateTableAsync();

        const string upsertTurnoverSql = @"
        INSERT INTO reporting.product_turnovers
            (store_id, store_name, manager_id, manager_name, manager_phone, agent_id, agent_name, agent_phone, agent_address,
             object_id, object_parent_id, object_code, object_name, object_type, is_received, amount, discount, date)
        VALUES
            (@StoreId, @StoreName, @ManagerId, @ManagerName, @ManagerPhone, @AgentId, @AgentName, @AgentPhone, @AgentAddress,
             @ObjectId, @ObjectParentId, @ObjectCode, @ObjectName, @ObjectType, @IsReceived, @Amount, @Discount, @Date)
        ON CONFLICT (object_id, object_name)
        DO UPDATE SET
            store_id     = EXCLUDED.store_id,
            store_name   = EXCLUDED.store_name,
            --to_store_id     = EXCLUDED.to_store_id,
            --to_store_name   = EXCLUDED.to_store_name,
            manager_id   = EXCLUDED.manager_id,
            manager_name = EXCLUDED.manager_name,
            manager_phone = EXCLUDED.manager_phone,
            agent_id     = EXCLUDED.agent_id,
            agent_name   = EXCLUDED.agent_name,
            agent_phone   = EXCLUDED.agent_phone,
            agent_address   = EXCLUDED.agent_address,
            object_parent_id  = EXCLUDED.object_parent_id,
            object_code  = EXCLUDED.object_code,
            object_type  = EXCLUDED.object_type,
            is_received  = EXCLUDED.is_received,
            amount       = EXCLUDED.amount,
            discount     = EXCLUDED.discount,
            date         = EXCLUDED.date
        RETURNING id;
        ";

                const string deleteProductsSql = @"
        DELETE FROM reporting.products
        WHERE turnover_id = @TurnoverId;
        ";

                const string insertProductSql = @"
        INSERT INTO reporting.products
            (product_id, product_code, product_name, manufacturer, unit, quantity, price, buy_price, sell_price, turnover_id, difference)
        VALUES
            (@ProductId, @ProductCode, @ProductName, @Manufacturer, @Unit, @Quantity, @Price, @BuyPrice, @SellPrice, @TurnoverId, @Difference);
        ";

        await using var conn = new NpgsqlConnection(_connString);
        await conn.OpenAsync();

        // Начинаем транзакцию
        await using var tx = await conn.BeginTransactionAsync();

        try
        {
            // 1) UPSERT turnover -> получаем turnoverId
            var turnoverId = await conn.QuerySingleAsync<Guid>(
                upsertTurnoverSql,
                new
                {
                    StoreId = turnover.StoreId,
                    StoreName = turnover.StoreName,
                    //ToStoreId = turnover.ToStoreId,
                    //ToStoreName = turnover.ToStoreName,
                    ManagerId = turnover.ManagerId,
                    ManagerName = turnover.ManagerName,
                    ManagerPhone = turnover.ManagerPhone,
                    AgentId = turnover.AgentId,
                    AgentName = turnover.AgentName,
                    AgentPhone = turnover.AgentPhone,
                    AgentAddress = turnover.AgentAddress,
                    ObjectId = turnover.ObjectId,
                    ObjectParentId = turnover.ObjectParentId,
                    ObjectCode = turnover.ObjectCode,
                    ObjectName = turnover.ObjectName,
                    ObjectType = turnover.ObjectType,
                    IsReceived = turnover.IsReceived,
                    Amount = turnover.Amount,
                    Discount = turnover.Discount,
                    Date = turnover.Date
                },
                transaction: tx
            );

            // 2) Удаляем старые продукты для этого turnover
            await conn.ExecuteAsync(deleteProductsSql, new { TurnoverId = turnoverId }, transaction: tx);

            // 3) Вставляем новые продукты (если есть)
            if (turnover.Products.Count > 0)
            {
                // Подготовим объект-параметр для каждой строки продукта
                var productParams = new List<object>(turnover.Products.Count);
                foreach (var p in turnover.Products)
                {
                    productParams.Add(new
                    {
                        ProductId = p.ProductId,
                        ProductCode = p.ProductCode,
                        ProductName = p.ProductName,
                        Manufacturer = p.Manufacturer,
                        Unit = p.Unit,
                        Quantity = p.Quantity,
                        Price = p.Price,
                        BuyPrice = p.BuyPrice,
                        SellPrice = p.SellPrice,
                        Difference = p.Difference,
                        TurnoverId = turnoverId
                    });
                }

                // Dapper выполнит INSERT для каждого элемента коллекции
                await conn.ExecuteAsync(insertProductSql, productParams, transaction: tx);
            }

            // Commit транзакции
            await tx.CommitAsync();
        }
        catch
        {
            // В случае исключения откатываем
            try { await tx.RollbackAsync(); } catch { /* игнорируем ошибки при rollback */ }
            throw;
        }
        finally
        {
            await conn.CloseAsync();
        }
    }

    public async Task<bool> UpdateByObjectAsync(ProductTurnover turnover)
    {
        if (turnover == null) throw new ArgumentNullException(nameof(turnover));
        if (turnover.Products == null) turnover.Products = new List<Product>();

        if (!_ensureTableCreated) await CreateTableAsync();

        // Обновляем turnover и получаем его id (если существует)
        var updateTurnoverSql = @"
            UPDATE reporting.product_turnovers
            SET
                store_id     = @StoreId,
                store_name   = @StoreName,
                --to_store_id     = @ToStoreId,
                --to_store_name   = @ToStoreName,
                manager_id   = @ManagerId,
                manager_name = @ManagerName,
                manager_phone = @ManagerPhone,
                agent_id     = @AgentId,
                agent_name   = @AgentName,
                agent_phone   = @AgentPhone,
                agent_address   = @AgentAddress,
                object_parent_id  = @ObjectParentId,
                object_code  = @ObjectCode,
                object_type  = @ObjectType,
                is_received  = @IsReceived,
                amount       = @Amount,
                discount     = @Discount,
                date         = @Date
            WHERE object_id = @ObjectId AND object_type = @ObjectType
            RETURNING id;
            ";

        const string deleteProductsSql = @"
            DELETE FROM reporting.products
            WHERE turnover_id = @TurnoverId;
            ";

        const string insertProductSql = @"
            INSERT INTO reporting.products
                (product_id, product_code, product_name, manufacturer, unit, quantity, price, buy_price, sell_price, turnover_id, difference)
            VALUES
                (@ProductId, @ProductCode, @ProductName, @Manufacturer, @Unit, @Quantity, @Price, @BuyPrice, @SellPrice, @TurnoverId, @Difference);
            ";

        await using var conn = new NpgsqlConnection(_connString);
        await conn.OpenAsync();
        await using var tx = await conn.BeginTransactionAsync();

        try
        {
            // Попытка обновить turnover и получить id
            Guid? turnoverId = null;
            try
            {
                turnoverId = await conn.QuerySingleOrDefaultAsync<Guid?>(
                    updateTurnoverSql,
                    new
                    {
                        StoreId = turnover.StoreId,
                        StoreName = turnover.StoreName,
                        //ToStoreId = turnover.ToStoreId,
                        //ToStoreName = turnover.ToStoreName,
                        ManagerId = turnover.ManagerId,
                        ManagerName = turnover.ManagerName,
                        ManagerPhone = turnover.ManagerPhone,
                        AgentId = turnover.AgentId,
                        AgentName = turnover.AgentName,
                        AgentPhone = turnover.AgentPhone,
                        AgentAddress = turnover.AgentAddress,
                        ObjectParentId = turnover.ObjectParentId,
                        ObjectCode = turnover.ObjectCode,
                        ObjectType = turnover.ObjectType,
                        IsReceived = turnover.IsReceived,
                        Amount = turnover.Amount,
                        Discount = turnover.Discount,
                        Date = turnover.Date,
                        ObjectId = turnover.ObjectId,
                        ObjectName = turnover.ObjectName
                    },
                    transaction: tx
                );
            }
            catch (InvalidOperationException)
            {
                // если возвращается более одной строки — но по уникальности это маловероятно
                throw;
            }

            if (!turnoverId.HasValue)
            {
                // Ничего не обновлено — такой записи нет
                await tx.RollbackAsync();
                return false;
            }

            // Удаляем старые продукты
            await conn.ExecuteAsync(deleteProductsSql, new { TurnoverId = turnoverId.Value }, transaction: tx);

            // Вставляем новые продукты (если есть)
            if (turnover.Products.Count > 0)
            {
                var productParams = new List<object>(turnover.Products.Count);
                foreach (var p in turnover.Products)
                {
                    productParams.Add(new
                    {
                        ProductId = p.ProductId,
                        ProductCode = p.ProductCode,
                        ProductName = p.ProductName,
                        Manufacturer = p.Manufacturer,
                        Unit = p.Unit,
                        Quantity = p.Quantity,
                        Price = p.Price,
                        BuyPrice = p.BuyPrice,
                        SellPrice = p.SellPrice,
                        Difference = p.Difference,
                        TurnoverId = turnoverId.Value
                    });
                }

                await conn.ExecuteAsync(insertProductSql, productParams, transaction: tx);
            }

            await tx.CommitAsync();
            return true;
        }
        catch
        {
            try { await tx.RollbackAsync(); } catch { }
            throw;
        }
        finally
        {
            await conn.CloseAsync();
        }
    }

    public async Task<int> DeleteByObjectAsync(long objectId, int objectType)
    {
        if (!_ensureTableCreated) await CreateTableAsync();

        // Сначала выбираем id записи(ей) совпадающих по критерию (обычно 0 или 1 из-за уникального ограничения)
        var selectIdsSql = @"
            SELECT id FROM reporting.product_turnovers
            WHERE object_id = @ObjectId AND object_type = @ObjectType;";

        const string deleteProductsByTurnoverIdsSql = @"
            DELETE FROM reporting.products
            WHERE turnover_id = ANY(@TurnoverIds);
";

        const string deleteTurnoversSql = @"
            DELETE FROM reporting.product_turnovers
            WHERE id = ANY(@TurnoverIds);
            ";

        await using var conn = new NpgsqlConnection(_connString);
        await conn.OpenAsync();
        await using var tx = await conn.BeginTransactionAsync();

        try
        {
            var ids = (await conn.QueryAsync<Guid>(
                selectIdsSql,
                new { ObjectId = objectId, ObjectType = objectType },
                transaction: tx)).ToArray();

            if (ids.Length == 0)
            {
                await tx.RollbackAsync();
                return 0;
            }

            // Удаляем продукты, связанные с этими turnover id
            await conn.ExecuteAsync(deleteProductsByTurnoverIdsSql, new { TurnoverIds = ids }, transaction: tx);

            // Удаляем сами turnover записи
            var deletedCount = await conn.ExecuteAsync(deleteTurnoversSql, new { TurnoverIds = ids }, transaction: tx);

            await tx.CommitAsync();
            return deletedCount;
        }
        catch
        {
            try { await tx.RollbackAsync(); } catch { }
            throw;
        }
        finally
        {
            await conn.CloseAsync();
        }
    }

    public async Task AddReport1Async(AgentRemains remains)
    {
        if (!_ensureTableCreated) await CreateTableAsync();

        var sql = @"INSERT INTO reporting.product_turnovers
            (store_id, store_name, manager_id, manager_name, agent_id, agent_name,
             object_id, object_code, object_name, object_type,
             amount, discount, date)
        VALUES
            (@StoreId, @StoreName, @ManagerId, @ManagerName, @AgentId, @AgentName,
             @ObjectId, @ObjectCode, @ObjectName, @ObjectType,
             @Amount, @Discount, @Date)
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
        RETURNING id;";

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
    } ///----------------


    public async Task<ProductTurnover?> GetTurnoverWithProductsByIdAsync(Guid id)
    {
        if (!_ensureTableCreated) await CreateTableAsync();

        const string selectTurnoverSql = @"
        SELECT
            id,
            store_id     AS StoreId,
            store_name   AS StoreName,
            --to_store_id     AS ToStoreId,
            --to_store_name   AS ToStoreName,
            manager_id   AS ManagerId,
            manager_name AS ManagerName,
            manager_phone AS ManagerPhone,
            agent_id     AS AgentId,
            agent_name   AS AgentName,
            agent_phone   AS AgentPhone,
            agent_address   AS AgentAddress,
            object_id    AS ObjectId,
            object_parent_id    AS ObjectParentId,
            object_code  AS ObjectCode,
            object_name  AS ObjectName,
            object_type  AS ObjectType,
            is_received  AS IsReceived,
            amount       AS Amount,
            discount     AS Discount,
            date         AS Date
        FROM reporting.product_turnovers
        WHERE id = @Id;
        ";

        const string selectProductsSql = @"
        SELECT
            id,
            product_id   AS ProductId,
            product_code   AS ProductCode,
            product_name AS ProductName,
            manufacturer AS Manufacturer,
            unit         AS Unit,
            quantity        AS Quantity,
            price    AS Price,
            buy_price    AS BuyPrice,
            sell_price   AS SellPrice,
            difference  AS Difference,
            turnover_id  AS TurnoverId
        FROM reporting.products
        WHERE turnover_id = @TurnoverId
        ORDER BY product_name;
        ";

        await using var conn = new NpgsqlConnection(_connString);
        await conn.OpenAsync();

        // Получаем шапку
        var turnover = await conn.QuerySingleOrDefaultAsync<ProductTurnover>(selectTurnoverSql, new { Id = id });
        if (turnover == null) return null;

        // Получаем продукты
        var products = await conn.QueryAsync<Product>(selectProductsSql, new { TurnoverId = turnover.Id });
        turnover.Products = products.AsList();

        return turnover;
    }

    public async Task<ProductTurnover?> GetTurnoverWithProductsByObjectAsync(long objectId, string objectName)
    {
        if (!_ensureTableCreated) await CreateTableAsync();

        const string selectTurnoverByObjectSql = @"
        SELECT
            id,
            store_id     AS StoreId,
            store_name   AS StoreName,
            --to_store_id     AS ToStoreId,
            --to_store_name   AS ToStoreName,
            manager_id   AS ManagerId,
            manager_name AS ManagerName,
            manager_phone AS ManagerPhone,
            agent_id     AS AgentId,
            agent_name   AS AgentName,
            agent_phone   AS AgentPhone,
            agent_address   AS AgentAddress,
            object_id    AS ObjectId,
            object_parent_id    AS ObjectParentId,
            object_code  AS ObjectCode,
            object_name  AS ObjectName,
            object_type  AS ObjectType,
            is_received  AS IsReceived,
            amount       AS Amount,
            discount     AS Discount,
            date         AS Date
        FROM reporting.product_turnovers
        WHERE object_id = @ObjectId
          AND object_name = @ObjectName;
        ";

        const string selectProductsSql = @"
        SELECT
            id,
            product_id   AS ProductId,
            product_code   AS ProductCode,
            product_name AS ProductName,
            manufacturer AS Manufacturer,
            unit         AS Unit,
            quantity        AS Quantity,
            price    AS Price,
            buy_price    AS BuyPrice,
            sell_price   AS SellPrice,
            difference  AS Difference,
            turnover_id  AS TurnoverId
        FROM reporting.products
        WHERE turnover_id = @TurnoverId
        ORDER BY product_name;
        ";

        await using var conn = new NpgsqlConnection(_connString);
        await conn.OpenAsync();

        var turnover = await conn.QuerySingleOrDefaultAsync<ProductTurnover>(
            selectTurnoverByObjectSql,
            new { ObjectId = objectId, ObjectName = objectName }
        );

        if (turnover == null) return null;

        var products = await conn.QueryAsync<Product>(selectProductsSql, new { TurnoverId = turnover.Id });
        turnover.Products = products.AsList();

        return turnover;
    }

    public async Task<DateTime> GetLastAuditProductsDateAsync(DateTime upToDate)
    {
        const string sql = @"
        SELECT MAX(date)
        FROM reporting.product_turnovers
        WHERE object_type = 6
            AND date < @UpToDate;

        SELECT MIN(date)
        FROM reporting.product_turnovers;
    ";

        await using var conn = new NpgsqlConnection(_connString);
        await conn.OpenAsync();

        await using var multi = await conn.QueryMultipleAsync(sql, new { UpToDate = upToDate });

        var lastAudit = await multi.ReadFirstOrDefaultAsync<DateTime?>();
        var minDate = await multi.ReadFirstOrDefaultAsync<DateTime?>();

        return lastAudit ?? minDate ?? DateTime.MinValue;
    }

    public async Task<List<ProductTurnover>> GetAllTurnoversWithProductsAsync(DateTime date)
    {
        if (!_ensureTableCreated) await CreateTableAsync();

        const string sql = @"
        SELECT
            t.id                AS Id,
            t.store_id          AS StoreId,
            t.store_name        AS StoreName,
            --t.to_store_id          AS ToStoreId,
            --t.to_store_name        AS ToStoreName,
            t.manager_id        AS ManagerId,
            t.manager_name      AS ManagerName,
            t.manager_phone      AS ManagerPhone,
            t.agent_id          AS AgentId,
            t.agent_name        AS AgentName,
            t.agent_phone        AS AgentPhone,
            t.agent_address        AS AgentAddress,
            t.object_id         AS ObjectId,
            t.object_parent_id         AS ObjectParentId,
            t.object_code       AS ObjectCode,
            t.object_name       AS ObjectName,
            t.object_type       AS ObjectType,
            t.is_received       AS IsReceived,
            t.amount            AS Amount,
            t.discount          AS Discount,
            t.date              AS Date,

            p.id                AS ProductIdGuid,
            p.product_id        AS ProductId,
            p.product_code        AS ProductCode,
            p.product_name      AS ProductName,
            p.manufacturer      AS Manufacturer,
            p.unit              AS Unit,
            p.quantity             AS Quantity,
            p.price         AS Price,
            p.buy_price         AS BuyPrice,
            p.sell_price        AS SellPrice,
            p.difference  AS Difference
        FROM reporting.product_turnovers t
        LEFT JOIN reporting.products p
            ON p.turnover_id = t.id
        WHERE t.date <= @Date
        ORDER BY t.date DESC, p.product_name;
        ";

        await using var conn = new NpgsqlConnection(_connString);

        var lookup = new Dictionary<Guid, ProductTurnover>();

        var result = await conn.QueryAsync(sql, (ProductTurnover t, Product p) =>
        {
            if (!lookup.TryGetValue(t.Id, out var turnover))
            {
                turnover = t;
                turnover.Products = new List<Product>();
                lookup.Add(turnover.Id, turnover);
            }

            if (p != null && p.ProductId != 0) // если у оборота есть продукт
            {
                turnover.Products.Add(p);
            }

            return turnover;
        },
        param: new { Date = date },
        splitOn: "ProductIdGuid"); // указываем точку разделения для Dapper

        return lookup.Values.ToList();
    }

    public async Task<DateTime> GetLastAuditProductsDateAsync(long storeId, DateTime upToDate)
    {
        const string sql = @"
            SELECT MAX(date)
            FROM reporting.product_turnovers
            WHERE store_id=@StoreId 
                AND object_type = 6
                AND date < @UpToDate;

            SELECT MIN(date)
            FROM reporting.product_turnovers;
        ";

        await using var conn = new NpgsqlConnection(_connString);
        await conn.OpenAsync();

        await using var multi = await conn.QueryMultipleAsync(sql, new { StoreId = storeId, UpToDate = upToDate });

        var lastAudit = await multi.ReadFirstOrDefaultAsync<DateTime?>();
        var minDate = await multi.ReadFirstOrDefaultAsync<DateTime?>();

        return lastAudit ?? minDate ?? DateTime.MinValue;
    }

    public async Task<List<ProductTurnover>> GetDashboardTurnoversAsync(DateTime fromDate, DateTime toDate)
    {
        if (!_ensureTableCreated) await CreateTableAsync();

        const string sql = @"
        SELECT
            t.id                AS Id,
            t.store_id          AS StoreId,
            t.store_name        AS StoreName,
            --t.to_store_id          AS ToStoreId,
            --t.to_store_name        AS ToStoreName,
            t.manager_id        AS ManagerId,
            t.manager_name      AS ManagerName,
            t.manager_phone      AS ManagerPhone,
            t.agent_id          AS AgentId,
            t.agent_name        AS AgentName,
            t.agent_phone        AS AgentPhone,
            t.agent_address        AS AgentAddress,
            t.object_id         AS ObjectId,
            t.object_parent_id         AS ObjectParentId,
            t.object_code       AS ObjectCode,
            t.object_name       AS ObjectName,
            t.object_type       AS ObjectType,
            t.is_received       AS IsReceived,
            t.amount            AS Amount,
            t.discount          AS Discount,
            t.date              AS Date,

            p.id                AS ProductIdGuid,
            p.product_id        AS ProductId,
            p.product_code        AS ProductCode,
            p.product_name      AS ProductName,
            p.manufacturer      AS Manufacturer,
            p.unit              AS Unit,
            p.quantity             AS Quantity,
            p.price         AS Price,
            p.buy_price         AS BuyPrice,
            p.sell_price        AS SellPrice,
            p.difference  AS Difference
        FROM reporting.product_turnovers t
        LEFT JOIN reporting.products p
            ON p.turnover_id = t.id
        WHERE (t.object_type = 4 or t.object_type = 5) and t.date between @FromDate AND @ToDate
        ORDER BY t.date DESC, p.product_name;
        ";
        
        await using var conn = new NpgsqlConnection(_connString);

        var lookup = new Dictionary<Guid, ProductTurnover>();

        var result = await conn.QueryAsync(sql, (ProductTurnover t, Product p) =>
        {
            if (!lookup.TryGetValue(t.Id, out var turnover))
            {
                turnover = t;
                turnover.Products = new List<Product>();
                lookup.Add(turnover.Id, turnover);
            }

            if (p != null && p.ProductId != 0) // если у оборота есть продукт
            {
                turnover.Products.Add(p);
            }

            return turnover;
        },
        param: new { FromDate = fromDate, ToDate = toDate },
        splitOn: "ProductIdGuid"); // указываем точку разделения для Dapper

        return lookup.Values.ToList();
    }

    public async Task<List<ProductTurnover>> GetTurnoversWithProductsByStoreIdAsync(long storeId, DateTime fromDate, DateTime toDate)
    {
        if (!_ensureTableCreated) await CreateTableAsync();

        const string sql = @"
        SELECT
            t.id                AS Id,
            t.store_id          AS StoreId,
            t.store_name        AS StoreName,
            --t.to_store_id          AS ToStoreId,
            --t.to_store_name        AS ToStoreName,
            t.manager_id        AS ManagerId,
            t.manager_name      AS ManagerName,
            t.manager_phone      AS ManagerPhone,
            t.agent_id          AS AgentId,
            t.agent_name        AS AgentName,
            t.agent_phone        AS AgentPhone,
            t.agent_address        AS AgentAddress,
            t.object_id         AS ObjectId,
            t.object_parent_id         AS ObjectParentId,
            t.object_code       AS ObjectCode,
            t.object_name       AS ObjectName,
            t.object_type       AS ObjectType,
            t.is_received       AS IsReceived,
            t.amount            AS Amount,
            t.discount          AS Discount,
            t.date              AS Date,

            p.id                AS ProductIdGuid,
            p.product_id        AS ProductId,
            p.product_code        AS ProductCode,
            p.product_name      AS ProductName,
            p.manufacturer      AS Manufacturer,
            p.unit              AS Unit,
            p.quantity             AS Quantity,
            p.price         AS Price,
            p.buy_price         AS BuyPrice,
            p.sell_price        AS SellPrice,
            p.difference  AS Difference
        FROM reporting.product_turnovers t
        LEFT JOIN reporting.products p
            ON p.turnover_id = t.id
        WHERE t.store_id = @StoreId and t.date between @FromDate AND @ToDate
        ORDER BY t.date DESC, p.product_name;
        ";

        await using var conn = new NpgsqlConnection(_connString);

        var lookup = new Dictionary<Guid, ProductTurnover>();

        var result = await conn.QueryAsync(sql, (ProductTurnover t, Product p) =>
        {
            if (!lookup.TryGetValue(t.Id, out var turnover))
            {
                turnover = t;
                turnover.Products = new List<Product>();
                lookup.Add(turnover.Id, turnover);
            }

            if (p != null && p.ProductId != 0) // если у оборота есть продукт
            {
                turnover.Products.Add(p);
            }

            return turnover;
        },
        param: new { StoreId = storeId, FromDate = fromDate, ToDate = toDate },
        splitOn: "ProductIdGuid"); // указываем точку разделения для Dapper

        return lookup.Values.ToList();
    }
}
