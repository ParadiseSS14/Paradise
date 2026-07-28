using Content.Client.Stylesheets;
using Robust.Client.Graphics;
using Robust.Client.UserInterface.Controls;
using Robust.Shared.Utility;

namespace Content.Client.UIControls;

public sealed class CloseContainerButton : ContainerForkedButton
{
    private StyleBoxIconBox? styleBoxRefl;

    public CloseContainerButton()
    {
        if(GetStyleBox() is StyleBoxIconBox styleBoxIconBox)
        {
            styleBoxRefl = styleBoxIconBox;
        }

        StyleBoxOverridden += UpdateStyleBoxRef;
    }

    private void UpdateStyleBoxRef()
    {
        if(GetStyleBox() is StyleBoxIconBox styleBoxIconBox)
        {
            styleBoxRefl = styleBoxIconBox;
        }
    }


    protected override void MouseEntered()
    {
        base.MouseEntered();
        SetHoverState(true);
    }

    protected override void MouseExited()
    {
        base.MouseExited();
        SetHoverState(false);
    }

    public void SetHoverState(bool state)
    {
        styleBoxRefl?.SetHovered(state);
    }
}
