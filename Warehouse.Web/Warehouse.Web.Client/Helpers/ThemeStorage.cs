using MudBlazor;

namespace Warehouse.Web.Client.Helpers;

public static class ThemeStorage
{
    static MudTheme[] themes => new MudTheme[]
    {
        SteelBlue,
        Graphite,
        Amber,
        Green,
        new MudTheme()
    };

    private static MudTheme Green = new MudTheme
    {
        PaletteLight = new PaletteLight
        {
            // Основной цвет интерфейса — “складской зелёный”
            Primary = "#2E7D32",  // Кнопки "Применить", основные действия
            Secondary = "#546E7A",  // Второстепенные кнопки, фильтры, панели
            Tertiary = "#8D6E63",  // Доп. акценты (например, статусы документов)

            Success = "#388E3C",  // Успешные операции (проведено, в наличии)
            Warning = "#F9A825",  // Низкий остаток, предупреждения
            Error = "#D32F2F",  // Ошибки, отказ, нет на складе
            Info = "#0277BD",  // Информационные сообщения

            Dark = "#263238",  // Color.Dark — тёмно-серый, не чисто чёрный
            Surface = "#FFFFFF",  // Карточки, таблицы, диалоги
            Background = "#F4F6F8",  // Общий фон рабочих форм

            TextPrimary = "#212121",
            TextSecondary = "#455A64",
        },
        PaletteDark = new PaletteDark
        {
            Primary = "#66BB6A",  // Мягкий зелёный в тёмной теме
            Secondary = "#90A4AE",  // Светло-серый/стальной
            Tertiary = "#BCAAA4",  // Тёплый акцент

            Success = "#A5D6A7",
            Warning = "#FFD54F",
            Error = "#EF9A9A",
            Info = "#4FC3F7",

            Dark = "#000000",  // Прямо чёрная кнопка Dark
            Surface = "#1E272E",  // Карточки/таблицы
            Background = "#121212",  // Общий фон

            TextPrimary = "#FFFFFF",
            TextSecondary = "#B0BEC5",
        }
    };

    private static MudTheme SteelBlue = new()
    {
        PaletteLight = new PaletteLight
        {
            Primary = "#1565C0",
            Secondary = "#455A64",
            Tertiary = "#5E35B1",

            Success = "#388E3C",
            Warning = "#F9A825",
            Error = "#D32F2F",
            Info = "#0277BD",

            Dark = "#263238",
            Surface = "#FFFFFF",
            Background = "#F5F7FA",

            TextPrimary = "#212121",
            TextSecondary = "#455A64"
        },
        PaletteDark = new PaletteDark
        {
            Primary = "#90CAF9",
            Secondary = "#B0BEC5",
            Tertiary = "#B39DDB",

            Success = "#A5D6A7",
            Warning = "#FFD54F",
            Error = "#EF9A9A",
            Info = "#81D4FA",

            Dark = "#000000",
            Surface = "#1E1E1E",
            Background = "#121212",

            TextPrimary = "#FFFFFF",
            TextSecondary = "#B0BEC5"
        }
    };

    private static MudTheme Graphite = new()
    {
        PaletteLight = new PaletteLight
        {
            Primary = "#455A64",
            Secondary = "#607D8B",
            Tertiary = "#37474F",

            Success = "#2E7D32",
            Warning = "#FBC02D",
            Error = "#C62828",
            Info = "#0277BD",

            Dark = "#263238",
            Surface = "#FFFFFF",
            Background = "#ECEFF1",

            TextPrimary = "#212121",
            TextSecondary = "#455A64"
        },
        PaletteDark = new PaletteDark
        {
            Primary = "#90A4AE",
            Secondary = "#B0BEC5",
            Tertiary = "#78909C",

            Success = "#81C784",
            Warning = "#FFD54F",
            Error = "#EF9A9A",
            Info = "#4FC3F7",

            Dark = "#000000",
            Surface = "#1E1E1E",
            Background = "#121212",

            TextPrimary = "#FFFFFF",
            TextSecondary = "#B0BEC5"
        }
    };

    private static MudTheme Amber = new()
    {
        PaletteLight = new PaletteLight
        {
            Primary = "#FF8F00",
            Secondary = "#6D4C41",
            Tertiary = "#8D6E63",

            Success = "#388E3C",
            Warning = "#FFA000",
            Error = "#D32F2F",
            Info = "#0277BD",

            Dark = "#4E342E",
            Surface = "#FFFFFF",
            Background = "#FFF8E1",

            TextPrimary = "#3E2723",
            TextSecondary = "#5D4037"
        },
        PaletteDark = new PaletteDark
        {
            Primary = "#FFCC80",
            Secondary = "#BCAAA4",
            Tertiary = "#D7CCC8",

            Success = "#A5D6A7",
            Warning = "#FFE082",
            Error = "#EF9A9A",
            Info = "#81D4FA",

            Dark = "#000000",
            Surface = "#2C2A29",
            Background = "#1C1B1A",

            TextPrimary = "#FFFFFF",
            TextSecondary = "#D7CCC8"
        }
    };

    public static MudTheme GetTheme(int index) => themes[index];
    public static MudTheme[] GetThemes() => themes;

}
