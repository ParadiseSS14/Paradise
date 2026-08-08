using Content.Client.UserInterface.Controls;
using Content.Shared.Modsuits;
using Content.Shared.Modsuits.Components;
using Content.Shared.Modsuits.Events;
using JetBrains.Annotations;
using Robust.Shared.Collections;
using Robust.Client.GameObjects;
using Robust.Client.UserInterface;
using Robust.Shared.Utility;
namespace Content.Client.Modsuits;

[UsedImplicitly]
public sealed partial class ModsuitMenuBoundUserInterface : BoundUserInterface
{
    private SimpleRadialMenu? _menu;
    public ModsuitMenuBoundUserInterface(EntityUid owner, Enum uiKey) : base(owner, uiKey)
    {
    }
    protected override void Open()
    {
        base.Open();

        if (!EntMan.TryGetComponent<ModsuitComponent>(Owner, out var component))
            return;

        _menu = this.CreateWindow<SimpleRadialMenu>();
        _menu.Track(Owner);

        var icons = GetIcons(component);

        _menu.SetButtons(ConvertToButtons(component, icons));
        _menu.OpenOverMouseScreenPosition();
    }
    private Dictionary<ModsuitPartType, (string Tooltip, SpriteSpecifier Sprite)> GetIcons(ModsuitComponent modsuit)
    {
        if (!modsuit.SpawnedParts.TryGetValue(ModsuitPartType.Helmet, out var helmet))
            return new();

        if (!EntMan.TryGetComponent<SpriteComponent>(helmet, out var sprite))
            return new();

        if (sprite.BaseRSI == null)
            return new();


        return new Dictionary<ModsuitPartType, (string Tooltip, SpriteSpecifier Sprite)>
        {
            [ModsuitPartType.Helmet] = ("modsuit-part-helmet", new SpriteSpecifier.Rsi(sprite.BaseRSI!.Path, "icon-helmet")),
            [ModsuitPartType.Chest] = ("modsuit-part-chest", new SpriteSpecifier.Rsi(sprite.BaseRSI!.Path, "icon-chestplate")),
            [ModsuitPartType.Gloves] = ("modsuit-part-gloves", new SpriteSpecifier.Rsi(sprite.BaseRSI!.Path, "icon-gloves")),
            [ModsuitPartType.Boots] = ("modsuit-part-boots", new SpriteSpecifier.Rsi(sprite.BaseRSI!.Path, "icon-boots")),
        };
    }
    private IEnumerable<RadialMenuOptionBase> ConvertToButtons(ModsuitComponent modsuit, Dictionary<ModsuitPartType, (string Tooltip, SpriteSpecifier Sprite)> icons)
    {
        var buttons = new List<RadialMenuOptionBase>();

        foreach (var part in Enum.GetValues<ModsuitPartType>())
        {
            buttons.Add(new RadialMenuActionOption<ModsuitPartType>(HandleMenuOptionClick, part)
            {
                IconSpecifier = RadialMenuIconSpecifier.With(icons[part].Sprite),
                ToolTip = icons[part].Tooltip
            });
        }

        return buttons;
    }
    private void HandleMenuOptionClick(ModsuitPartType part)
    {
        SendMessage(new ModsuitSystemMessage(part));
    }
}
