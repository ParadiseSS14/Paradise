namespace Content.Shared.Modsuits;

public static class ModsuitContainers
{
    private static readonly Dictionary<ModsuitPartType, string> PartContainers = new()
    {
        { ModsuitPartType.Helmet, "helmet" },
        { ModsuitPartType.Chest, "chest" },
        { ModsuitPartType.Gloves, "gloves" },
        { ModsuitPartType.Boots, "boots" },
    };
    private static readonly Dictionary<ModsuitPartType, string> StorageContainers = new()
    {
        { ModsuitPartType.Gloves, "storedGloves" },
        { ModsuitPartType.Boots, "storedBoots" },
    };
    private static readonly Dictionary<ModsuitPartType, string> InventorySlots = new()
    {
        { ModsuitPartType.Helmet, "head" },
        { ModsuitPartType.Chest, "outerClothing" },
        { ModsuitPartType.Gloves, "gloves" },
        { ModsuitPartType.Boots, "shoes" },
    };

    private static readonly Dictionary<ModsuitPartType, string> PartNames = new()
    {
        { ModsuitPartType.Helmet, "helmet" },
        { ModsuitPartType.Chest, "chestplate" },
        { ModsuitPartType.Gloves, "gloves" },
        { ModsuitPartType.Boots, "boots" },
    };
    public static readonly Dictionary<ModsuitPartType, string> ProtectionSlots = new()
    {
        { ModsuitPartType.Helmet, "helmet" },
        { ModsuitPartType.Chest, "chestplate" },
    };
    public static string GetPartContainer(ModsuitPartType part)
        => PartContainers[part];
    public static string GetInventorySlot(ModsuitPartType part)
        => InventorySlots[part];
    public static bool TryGetStorageContainer(ModsuitPartType part, out string container)
        => StorageContainers.TryGetValue(part, out container!);
    public static string GetPartName(ModsuitPartType part)
        => PartNames[part];
}
