namespace Warehouse.Web.Operations;

public enum OperationType
{
    Send, // Отправка перемещение -
    Receive, // Прием перемещение
    Receiving, // Приемка товара 
    ReturnsToSuplier, // Возврат поставщику -
    Shipments, // Отгрузка -
    BuyerReturn, // Возврат покупателю
    Audit, // Ревизия склада
    SendOrReceive
}
