using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace Warehouse.Web.Client.Helpers;

public class MyIconButton : MudIconButton
{
    [Parameter]
    public ExtendedSize ExtendedSize { get; set; } = ExtendedSize.Medium;

    protected override void OnParametersSet()
    {
        base.OnParametersSet();

        switch (ExtendedSize)
        {
            case ExtendedSize.ExtraSmall:
                Class += " extra-small";
                break;
            case ExtendedSize.Small:
                Size = Size.Small;
                break;
            case ExtendedSize.Medium:
                Size = Size.Medium;
                break;
            case ExtendedSize.Large:
                Size = Size.Large;
                break;
        }
    }
}