using Content.Shared._Paradise.PoweredClothing;
using Content.Shared.Item.ItemToggle;
using Content.Shared.PowerCell;
using Robust.Shared.Timing;

namespace Content.Server._Paradise.PhysicalParameters;

public sealed partial class PoweredClothingSystem : SharedPoweredClothingSystem
{
    [Dependency] private PowerCellSystem _cellSystem = default!;
    [Dependency] private IGameTiming _timing = default!;
    [Dependency] private ItemToggleSystem _itemToggle = default!;

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<ActivePoweredClothingComponent, PoweredClothingComponent>();

        while (query.MoveNext(out var uid, out var active, out var comp))
        {
            if (_timing.CurTime < active.TargetTime)
                continue;

            if (!_cellSystem.TryUseCharge(comp.PowerSource, comp.DrawRate))
            {
                RemComp<ActivePoweredClothingComponent>(uid);

                var ev = new PoweredClothingTurnedOffEvent();
                RaiseLocalEvent(uid, ref ev);

                _itemToggle.TryDeactivate(uid, predicted: false);

                return;
            }

            active.TargetTime += comp.DrawTime;
        }
    }
}
