using Blazored.LocalStorage;
using MudBlazor;
using Warehouse.Web.Client.Models;
using Warehouse.Web.Shared.Responses;

namespace Warehouse.Web.Client.Helpers;
public static class ColumnsStoreage
{
    public static async Task<List<ColumnDefinition>> GetStoreColumns(this ILocalStorageService localStorage, string? localStorageKey = null)
    {
        if (localStorageKey is not null)
        {
            var columnsBase64 = await localStorage.GetItemAsStringAsync(localStorageKey) ?? string.Empty;

            if (!string.IsNullOrEmpty(columnsBase64))
            {
                var colunsStr = columnsBase64.FromBas64();

                return colunsStr.Deserialize<List<ColumnDefinition>>() ?? GetColumns();
            }
        }

        return GetColumns();

        static List<ColumnDefinition> GetColumns()
        {
            return new List<ColumnDefinition>
            {
                new ColumnDefinition { Title = "Наименование", PropertyName = nameof(StoreResponse.Name) }
            };
        }
    }
    public static async Task<List<ColumnDefinition>> GetManagerColumns(this ILocalStorageService localStorage, string? localStorageKey = null)
    {
        if (localStorageKey is not null)
        {
            var columnsBase64 = await localStorage.GetItemAsStringAsync(localStorageKey) ?? string.Empty;

            if (!string.IsNullOrEmpty(columnsBase64))
            {
                var colunsStr = columnsBase64.FromBas64();

                return colunsStr.Deserialize<List<ColumnDefinition>>() ?? GetColumns();
            }
        }

        return GetColumns();

        static List<ColumnDefinition> GetColumns()
        {
            return new List<ColumnDefinition>
            {
                new ColumnDefinition { Title = "Фамилия", PropertyName = nameof(ManagerResponse.Lastname) },
                new ColumnDefinition { Title = "Имя", PropertyName = nameof(ManagerResponse.Firstname) },
                new ColumnDefinition { Title = "Номер телефона", PropertyName = nameof(ManagerResponse.Phone) },
                new ColumnDefinition { Title = "Адрес", PropertyName = nameof(ManagerResponse.Address) },
                new ColumnDefinition { Title = "Склад", PropertyName = nameof(ManagerResponse.StoreName) }
            };
        }
    }
    public static async Task<List<ColumnDefinition>> GetEmployeeColumns(this ILocalStorageService localStorage, string? localStorageKey = null)
    {
        if (localStorageKey is not null)
        {
            var columnsBase64 = await localStorage.GetItemAsStringAsync(localStorageKey) ?? string.Empty;

            if (!string.IsNullOrEmpty(columnsBase64))
            {
                var colunsStr = columnsBase64.FromBas64();

                return colunsStr.Deserialize<List<ColumnDefinition>>() ?? GetColumns();
            }
        }

        return GetColumns();

        static List<ColumnDefinition> GetColumns()
        {
            return new List<ColumnDefinition>
            {
                new ColumnDefinition { Title = "Логин", PropertyName = nameof(UserResponse.Login) },
                new ColumnDefinition { Title = "Фамилия", PropertyName = nameof(UserResponse.Lastname) },
                new ColumnDefinition { Title = "Имя", PropertyName = nameof(UserResponse.Firstname) },
                new ColumnDefinition { Title = "Номер телефона", PropertyName = nameof(UserResponse.Phone) },
                new ColumnDefinition { Title = "Адрес", PropertyName = nameof(UserResponse.Address) },
                new ColumnDefinition { Title = "Склад", PropertyName = nameof(UserResponse.StoreName) }
            };
        }
    }
    public static async Task<List<ColumnDefinition>> GetAgentColumns(this ILocalStorageService localStorage, string? localStorageKey = null)
    {
        if (localStorageKey is not null)
        {
            var columnsBase64 = await localStorage.GetItemAsStringAsync(localStorageKey) ?? string.Empty;

            if (!string.IsNullOrEmpty(columnsBase64))
            {
                var colunsStr = columnsBase64.FromBas64();

                return colunsStr.Deserialize<List<ColumnDefinition>>() ?? GetColumns();
            }
        }

        return GetColumns();

        static List<ColumnDefinition> GetColumns()
        {
            return new List<ColumnDefinition>
            {
                new ColumnDefinition { Title = "Наименования", PropertyName = nameof(AgentResponse.Name) },
                new ColumnDefinition { Title = "Номер телефона", PropertyName = nameof(AgentResponse.Phone), Sortable=false },
                new ColumnDefinition { Title = "Адрес", PropertyName = nameof(AgentResponse.Address), Sortable=false },
                new ColumnDefinition { Title = "Склад", PropertyName = nameof(AgentResponse.StoreName), Sortable=false },
                new ColumnDefinition { Title = "Менеджер", PropertyName = nameof(AgentResponse.ManagerName), Sortable=false },
                new ColumnDefinition { Title = "Коментария", PropertyName = nameof(AgentResponse.Comment), Visible = false, Sortable=false }
            };
        }
    }
    
    public static async Task<List<ColumnDefinition>> GetProductColumns(this ILocalStorageService localStorage, string? localStorageKey = null)
    {
        if (localStorageKey is not null)
        {
            var columnsBase64 = await localStorage.GetItemAsStringAsync(localStorageKey) ?? string.Empty;

            if (!string.IsNullOrEmpty(columnsBase64))
            {
                var colunsStr = columnsBase64.FromBas64();

                return colunsStr.Deserialize<List<ColumnDefinition>>() ?? GetColumns();
            }
        }

        return GetColumns();

        static List<ColumnDefinition> GetColumns()
        {
            return new List<ColumnDefinition>
            {
                new ColumnDefinition { Title = "Наименование", PropertyName = nameof(ProductResponse.Name) },
                new ColumnDefinition { Title = "Производитель", PropertyName = nameof(ProductResponse.Manufacturer) },
                new ColumnDefinition { Title = "Ед. изм.", PropertyName = nameof(ProductResponse.Unit) },
                new ColumnDefinition { Title = "Цена закупки", PropertyName = nameof(ProductResponse.BuyPrice) },
                new ColumnDefinition { Title = "Цена продажи", PropertyName = nameof(ProductResponse.SellPrice) },
                new ColumnDefinition { Title = "Лимит", PropertyName = nameof(ProductResponse.LimitRemain) }
            };
        }
    }
    public static async Task<List<ColumnDefinition>> GetProductRemainsColumns(this ILocalStorageService localStorage, string? localStorageKey = null)
    {
        if (localStorageKey is not null)
        {
            var columnsBase64 = await localStorage.GetItemAsStringAsync(localStorageKey) ?? string.Empty;

            if (!string.IsNullOrEmpty(columnsBase64))
            {
                var colunsStr = columnsBase64.FromBas64();

                return colunsStr.Deserialize<List<ColumnDefinition>>() ?? GetColumns();
            }
        }

        return GetColumns();

        static List<ColumnDefinition> GetColumns()
        {
            return new List<ColumnDefinition>
            {
                new ColumnDefinition { Title = "Наименование", PropertyName = nameof(ProductResponse.Name) },
                new ColumnDefinition { Title = "Производитель", PropertyName = nameof(ProductResponse.Manufacturer) },
                new ColumnDefinition { Title = "Цена продажи", PropertyName = nameof(ProductResponse.SellPrice) },
                new ColumnDefinition { Title = "Лимит", PropertyName = nameof(ProductResponse.LimitRemain) }
            };
        }
    }
    public static async Task<List<ColumnDefinition>> GetHistoryColumns(this ILocalStorageService localStorage, string? localStorageKey = null)
    {
        if (localStorageKey is not null)
        {
            var columnsBase64 = await localStorage.GetItemAsStringAsync(localStorageKey) ?? string.Empty;

            if (!string.IsNullOrEmpty(columnsBase64))
            {
                var colunsStr = columnsBase64.FromBas64();

                return colunsStr.Deserialize<List<ColumnDefinition>>() ?? GetColumns();
            }
        }

        return GetColumns();

        static List<ColumnDefinition> GetColumns()
        {
            return new List<ColumnDefinition>
            {
                new ColumnDefinition { Title = "Склад", PropertyName = nameof(HistoryResponse.StoreName) },
                new ColumnDefinition { Title = "Пользователь", PropertyName = nameof(HistoryResponse.UserName) },
                new ColumnDefinition { Title = "Объект", PropertyName = nameof(HistoryResponse.ObjName) },
                new ColumnDefinition { Title = "Дата", PropertyName = nameof(HistoryResponse.CreatedDate), StringFormat = "dd.MM.yyyy HH:mm" },
                new ColumnDefinition { Title = "Операция", PropertyName = nameof(HistoryResponse.Method) }
            };
        }
    }
    public static async Task<List<ColumnDefinition>> GetOrderColumns(this ILocalStorageService localStorage, string? localStorageKey = null)
    {
        if (localStorageKey is not null)
        {
            var columnsBase64 = await localStorage.GetItemAsStringAsync(localStorageKey) ?? string.Empty;

            if (!string.IsNullOrEmpty(columnsBase64))
            {
                var colunsStr = columnsBase64.FromBas64();

                return colunsStr.Deserialize<List<ColumnDefinition>>() ?? GetColumns();
            }
        }

        return GetColumns();

        static List<ColumnDefinition> GetColumns()
        {
            return new List<ColumnDefinition>
            {
                new ColumnDefinition { Title = "Код", PropertyName = nameof(OrderResponse.Code) },
                new ColumnDefinition { Title = "Дата время", PropertyName = nameof(OrderResponse.Date), StringFormat = "dd.MM.yyyy HH:mm" },
                new ColumnDefinition { Title = "Склад", PropertyName = nameof(OrderResponse.StoreName) },
                new ColumnDefinition { Title = "Контрагент", PropertyName = nameof(OrderResponse.AgentName) },
                new ColumnDefinition { Title = "Сумма", PropertyName = nameof(OrderResponse.Amount) }
            };
        }
    }
    public static async Task<List<ColumnDefinition>> GetOperationColumns(this ILocalStorageService localStorage, OperationType type, string? localStorageKey = null)
    {
        if (localStorageKey is not null)
        {
            var columnsBase64 = await localStorage.GetItemAsStringAsync(localStorageKey) ?? string.Empty;

            if (!string.IsNullOrEmpty(columnsBase64))
            {
                var colunsStr = columnsBase64.FromBas64();

                return colunsStr.Deserialize<List<ColumnDefinition>>() ?? GetColumns(type);
            }
        }

        return GetColumns(type);

        static List<ColumnDefinition> GetColumns(OperationType type)
        {
            if (type == OperationType.Send || type == OperationType.Receive)
                return new List<ColumnDefinition>
                {
                    new ColumnDefinition { Title = "Код", PropertyName = nameof(OperationResponse.Code) },
                    new ColumnDefinition { Title = "Дата время", PropertyName = nameof(OperationResponse.Date), StringFormat = "dd.MM.yyyy HH:mm" },
                    new ColumnDefinition { Title = "Со склада", PropertyName = nameof(OperationResponse.StoreName) },
                    new ColumnDefinition { Title = "На склад", PropertyName = nameof(OperationResponse.ToStoreName) },
                    new ColumnDefinition { Title = "Сумма", PropertyName = nameof(OperationResponse.Amount) }
                };

            return new List<ColumnDefinition>
            {
                new ColumnDefinition { Title = "Код", PropertyName = nameof(OperationResponse.Code) },
                new ColumnDefinition { Title = "Дата время", PropertyName = nameof(OperationResponse.Date), StringFormat = "dd.MM.yyyy HH:mm" },
                new ColumnDefinition { Title = "Склад", PropertyName = nameof(OperationResponse.StoreName) },
                new ColumnDefinition { Title = "Контрагент", PropertyName = nameof(OperationResponse.AgentName) },
                new ColumnDefinition { Title = "Сумма", PropertyName = nameof(OperationResponse.Amount) },
                new ColumnDefinition { Title = "Скидка", PropertyName = nameof(OperationResponse.Discount) },
                new ColumnDefinition { Title = "Сумма к оплате", PropertyName = nameof(OperationResponse.ToPay) }
            };
        }
    }
    public static async Task<List<ColumnDefinition>> GetAuditProductsColumns(this ILocalStorageService localStorage, string? localStorageKey = null)
    {
        if (localStorageKey is not null)
        {
            var columnsBase64 = await localStorage.GetItemAsStringAsync(localStorageKey) ?? string.Empty;

            if (!string.IsNullOrEmpty(columnsBase64))
            {
                var colunsStr = columnsBase64.FromBas64();

                return colunsStr.Deserialize<List<ColumnDefinition>>() ?? GetColumns();
            }
        }

        return GetColumns();

        static List<ColumnDefinition> GetColumns()
        {
            return new List<ColumnDefinition>
            {
                new ColumnDefinition { Title = "Код", PropertyName = nameof(AuditProductResponse.Code) },
                new ColumnDefinition { Title = "Дата время", PropertyName = nameof(AuditProductResponse.Date), StringFormat = "dd.MM.yyyy HH:mm" },
                new ColumnDefinition { Title = "Склад", PropertyName = nameof(AuditProductResponse.StoreName) },
                new ColumnDefinition { Title = "Количество", PropertyName = nameof(AuditProductResponse.ShortageCount) },
                new ColumnDefinition { Title = "Сумма", PropertyName = nameof(AuditProductResponse.ShortageAmount) },
                new ColumnDefinition { Title = "Количество", PropertyName = nameof(AuditProductResponse.SurplusCount) },
                new ColumnDefinition { Title = "Сумма", PropertyName = nameof(AuditProductResponse.SurplusAmount) },
            };
        }
    }
}
