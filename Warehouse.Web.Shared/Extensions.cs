using static System.Runtime.InteropServices.JavaScript.JSType;
using System.Text.RegularExpressions;

namespace Warehouse.Web.Shared;

public static class Extensions
{
    public static string GetHistoryObjectName(this string operation, string data)
    {
        if (operation.EndsWith("Manager"))
            return "Менеджер";
        else if (operation.EndsWith("User"))
            return "Пользователь";
        else if (operation.EndsWith("Store"))
            return "Склад";
        else if (operation.EndsWith("Agent"))
            return "Агент";
        else if (operation.EndsWith("Product"))
            return "Товар";
        else if (operation.EndsWith("GoodsAudit"))
            return "Ревизия товара";
        else if (operation.EndsWith("Operation"))
        {
            string pattern = "\"Type\":\\s*(\\d+)";
            Match match = Regex.Match(data, pattern);

            if (match.Success)
            {
                string typeValue = match.Groups[1].Value;
                if (typeValue == "0")
                    return "Отправка перемещение";
                else if (typeValue == "1")
                    return "Прием перемещение";
                else if (typeValue == "2")
                    return "Приемка товара";
                else if (typeValue == "3")
                    return "Возврат поставщику";
                else if (typeValue == "4")
                    return "Отгрузка";
                else if (typeValue == "5")
                    return "Возврат покупателю";
                else if (typeValue == "6")
                    return "Ревизия";
            }

            return "";
        }
        else if (operation.EndsWith("SendOrReceive"))
        {
            string pattern = "\"Type\":\\s*(\\d+)";
            Match match = Regex.Match(data, pattern);

            if (match.Success)
            {
                string typeValue = match.Groups[1].Value;
                if (typeValue == "0")
                    return "Отправка перемещение";
                else if (typeValue == "1")
                    return "Прием перемещение";
            }

            return "";
        }
        else if (operation.EndsWith("Order"))
        {
            string pattern = "\"Type\":\\s*(\\d+)";
            Match match = Regex.Match(data, pattern);

            if (match.Success)
            {
                string typeValue = match.Groups[1].Value;
                if (typeValue == "0")
                    return "Приходный ордер";
                else if (typeValue == "1")
                    return "Расходный ордер";
            }
        }

        return "";
    }

    public static string GetOperationName(this string operation, string typeValue)
    {
        if (operation.EndsWith("Manager"))
            return "Менеджер";
        else if (operation.EndsWith("User"))
            return "Пользователь";
        else if (operation.EndsWith("Store"))
            return "Склад";
        else if (operation.EndsWith("Agent"))
            return "Агент";
        else if (operation.EndsWith("Product"))
            return "Товар";
        else if (operation.EndsWith("GoodsAudit"))
            return "Ревизия товара";
        else if (operation.EndsWith("Operation"))
        {
            if (typeValue == "0")
                return "Отправка перемещение";
            else if (typeValue == "1")
                return "Прием перемещение";
            else if (typeValue == "2")
                return "Приемка товара";
            else if (typeValue == "3")
                return "Возврат поставщику";
            else if (typeValue == "4")
                return "Отгрузка";
            else if (typeValue == "5")
                return "Возврат покупателю";
            else if (typeValue == "6")
                return "Ревизия";

            return "";
        }
        else if (operation.EndsWith("SendOrReceive"))
        {
            if (typeValue == "0")
                return "Отправка перемещение";
            else if (typeValue == "1")
                return "Прием перемещение";

            return "";
        }
        else if (operation.EndsWith("Order"))
        {
            if (typeValue == "0")
                return "Приходный ордер";
            else if (typeValue == "1")
                return "Расходный ордер";
        }

        return "";
    }
}
