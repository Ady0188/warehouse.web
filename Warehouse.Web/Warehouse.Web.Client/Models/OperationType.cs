namespace Warehouse.Web.Client.Models;

//public enum OperationType
//{
//    Send, // Отправка перемещение
//    Receive, // Прием перемещение
//    Receiving, // Приемка товара
//    ReturnsToSuplier, // Возврат поставщику
//    Shipments, // Отгрузка
//    BuyerReturn, // Возврат покупателю
//    AgentBeginRemains00, // Остаток на начало (агент)
//    GoodsBeginRemains, // Остаток на начало (товар)
//    Revision, // Ревизия
//    SendOrReceive, // Отправка или Прием перемещение
//}

public enum OperationType
{
    Send, // Отправка перемещение -
    Receive, // Прием перемещение
    Receiving, // Приемка товара 
    ReturnsToSuplier, // Возврат поставщику -
    Shipments, // Отгрузка -
    BuyerReturn, // Возврат покупателю
    Audit, // Ревизия склада
    SendOrReceive, // Отправка или Прием перемещение -
}