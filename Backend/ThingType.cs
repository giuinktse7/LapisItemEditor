using System;
using Backend.Tibia7;

namespace Backend
{
    public enum ServerItemGroup
    {
        None,

        Ground,
        Container,
        [Obsolete("ServerItemGroup.Weapon is deprecated.")]
        Weapon,     //deprecated
        Ammunition, //deprecated
        Armor,      //deprecated
        Charges,
        Teleport,   //deprecated
        MagicField, //deprecated
        Writable,   //deprecated
        Key,        //deprecated
        Splash,
        Fluid,
        Door, //deprecated
        Deprecated,

        ShowOffSocket,

        ITEM_GROUP_LAST
    };

    public enum ItemType_t
    {
        None,
        Depot,
        Mailbox,
        TrashHolder,
        Container,
        Door,
        MagicField,
        Teleport,
        Bed,
        Key,
        Rune,
        ShowOffSocket,
        Last
    };

    [Flags]
    public enum ItemTypeFlag
    {
        BlockSolid = 1 << 0,
        BlockProjectile = 1 << 1,
        BlockPathfind = 1 << 2,
        HasElevation = 1 << 3,

        // Called FLAG_USEABLE in other software (like Remere's Map Editor). This flag corresponds to "use with" (crosshair)
        MultiUse = 1 << 4,
        Pickupable = 1 << 5,
        Movable = 1 << 6,
        Stackable = 1 << 7,
        FloorChangeDown = 1 << 8,   // unused
        FloorChangeNorth = 1 << 9,  // unused
        FloorChangeEast = 1 << 10,  // unused
        FloorChangeSouth = 1 << 11, // unused
        FloorChangeWest = 1 << 12,  // unused

        AlwaysOnTop = 1 << 13,
        Readable = 1 << 14,
        Rotatable = 1 << 15,
        Hangable = 1 << 16,
        HookEast = 1 << 17,
        HookSouth = 1 << 18,
        CanNotDecay = 1 << 19, // unused
        AllowDistRead = 1 << 20,
        ClientDuration = 1 << 21,        // Called wearout in the Tibia appearance data, added in Tibia 12.90
        ClientCharges = 1 << 22, // Called expire in the Tibia appearance data, added in Tibia 12.90
        IgnoreLook = 1 << 23,
        Animation = 1 << 24,
        FullTile = 1 << 25, // unused
        ForceUse = 1 << 26,

        Ammo = 1 << 27,
        Reportable = 1 << 28,
        Lootable = 1 << 29 // Primary use is to aid scripting of auto-loot
    };


    public enum ThingCategory : byte
    {
        Invalid = 0,
        Item = 1,
        Outfit = 2,
        Effect = 3,
        Missile = 4
    }

    public enum StackOrder : byte
    {
        None = 0,
        Border = 1,
        Bottom = 2,
        Top = 3
    }

    public enum DefaultAction : byte
    {
        None = 0,
        Look = 1,
        Use = 2,
        Open = 3,
        AutowalkHighlight = 4
    }

    public class ThingType
    {

        public ThingType(uint clientId, ThingCategory category)
        {
            ClientId = clientId;
            Category = category;
            StackOrder = StackOrder.None;
        }

        public ThingType(ThingCategory category) : this(0, category)
        {
            ////
        }

        public FrameGroup DefaultFrameGroup { get => FrameGroups[(byte)FrameGroupType.Default]; }

        public ServerItemGroup ServerItemGroup { get; set; }
        public ItemType_t ItemType { get; set; }

        public uint ClientId { get; set; }

        public uint ServerId { get; set; }

        public ThingCategory Category { get; private set; }

        public StackOrder StackOrder { get; set; }


        public ushort WareId { get; set; }

        public ushort GroundSpeed { get; set; }
        public bool HasHeight { get; set; }
        public bool Horizontal { get; set; }
        public bool Vertical { get; set; }
        public bool IsContainer { get; set; }

        public bool DirectlyAboveBorder { get; set; }
        public bool Stackable { get; set; }
        public bool ForceUse { get; set; }
        public bool MultiUse { get; set; }
        public bool IsFluidContainer { get; set; }
        public bool IsLiquidPool { get; set; }
        public bool Unpassable { get; set; }
        public bool Unmovable { get; set; }
        public bool BlockSolid { get; set; }
        public bool BlockMissiles { get; set; }
        public bool BlockPathfinder { get; set; }
        public bool NoMoveAnimation { get; set; }
        public bool Pickupable { get; set; }
        public bool AllowDistRead { get; set; }
        public bool Readable { get; set; }
        public bool Hangable { get; set; }
        public bool HookEast { get; set; }
        public bool HookSouth { get; set; }
        public bool Rotatable { get; set; }
        public bool FullGround { get; set; }
        public bool IgnoreLook { get; set; }
        public bool IsAnimation { get; set; }
        public bool DontHide { get; set; }
        public bool Translucent { get; set; }
        public bool LyingObject { get; set; }
        public bool AnimateAlways { get; set; }
        public bool Usable { get; set; }
        public bool HasCharges { get; set; }
        public bool FloorChange { get; set; }
        public bool Wrappable { get; set; }
        public bool Unwrappable { get; set; }
        public bool IsTopEffect { get; set; }
        public bool Writable { get; set; }
        public bool WritableOnce { get; set; }
        public bool HasLight { get => LightLevel != 0 || LightColor != 0; }
        public bool HasOffset { get; set; }
        public bool HasElevation { get; set; }
        public bool Minimap { get; set; }
        public bool IsLensHelp { get; set; }
        public bool IsCloth { get; set; }

        public ushort RotateTo { get; set; }
        public ushort ContainerSize { get; set; }
        public uint Weight { get; set; }
        public ushort MaxTextLength { get; set; }

        public ushort LightLevel { get; set; }
        public ushort LightColor { get; set; }
        public ushort OffsetX { get; set; }
        public ushort OffsetY { get; set; }
        public ushort Elevation { get; set; }
        public ushort MinimapColor { get; set; }
        public ushort LensHelp { get; set; }

        public ClothSlot ClothSlot { get; set; }
        public bool IsMarketItem { get; set; }
        public string MarketName { get; set; } = "";

        public string Name { get; set; } = "";
        public string Description { get; set; } = "";
        public MarketCategory MarketCategory { get; set; }
        public ushort MarketShowAs { get; set; }
        public ushort MarketRestrictVocation { get; set; }
        public ushort MarketRestrictLevel { get; set; }
        public bool HasAction { get; set; }
        public DefaultAction DefaultAction { get; set; }

        public bool HasSprites { get => FrameGroups[(int)FrameGroupType.Default] != null; }

        public uint GetDefaultSpriteId()
        {
            return FrameGroups[(int)FrameGroupType.Default].SpriteIDs[0];
        }

        protected byte[]? spriteHash = null;
        public byte[]? SpriteHash
        {
            get => spriteHash; set
            {
                spriteHash = value;
            }
        }

        internal FrameGroup[] FrameGroups { get; } = new FrameGroup[2];
        public int FrameGroupCount
        {
            get
            {
                if (FrameGroups[0] == null)
                {
                    return 0;
                }
                if (FrameGroups[1] == null)
                {
                    return 1;
                }
                else
                {
                    return 2;
                }
            }
        }

        public override string ToString()
        {
            if (MarketName != null)
            {
                return ClientId.ToString() + " - " + MarketName;
            }

            return ClientId.ToString();
        }

        public FrameGroup? GetFrameGroup(FrameGroupType groupType)
        {
            return FrameGroups[(int)groupType];
        }

        public FrameGroup SetFrameGroup(FrameGroupType groupType, FrameGroup group)
        {
            if (groupType == FrameGroupType.Walking && (FrameGroups[(int)FrameGroupType.Default] != null || FrameGroupCount == 0))
            {
                FrameGroups[(int)FrameGroupType.Default] = group;
            }

            if (FrameGroups[(int)groupType] == null)
            {
                FrameGroups[(int)groupType] = group;
            }

            return group;
        }

        public ThingType Clone()
        {
            ThingType clone = (ThingType)MemberwiseClone();

            clone.FrameGroups[0] = FrameGroups[0].Clone();
            clone.FrameGroups[1] = FrameGroups[1].Clone();

            return clone;
        }

        public static ThingType Create(ushort id, ThingCategory category)
        {
            if (category == ThingCategory.Invalid)
            {
                throw new ArgumentException("Invalid category.");
            }

            ThingType thing = new ThingType(id, category);

            if (category == ThingCategory.Outfit)
            {
                for (int i = 0; i < 2; i++)
                {
                    FrameGroup group = FrameGroup.Create();
                    group.PatternX = 4; // directions
                    group.Frames = 3;   // animations
                    group.IsAnimation = true;
                    group.SpriteIDs = new uint[group.GetTotalSprites()];
                    group.FrameDurations = new FrameDuration[group.Frames];

                    for (int f = 0; f < group.Frames; f++)
                    {
                        group.FrameDurations[f] = new FrameDuration(category);
                    }

                    thing.SetFrameGroup((FrameGroupType)i, group);
                }
            }
            else
            {
                FrameGroup group = FrameGroup.Create();

                if (category == ThingCategory.Missile)
                {
                    group.PatternX = 3;
                    group.PatternY = 3;
                    group.SpriteIDs = new uint[group.GetTotalSprites()];
                }

                thing.SetFrameGroup(FrameGroupType.Default, group);
            }

            return thing;
        }

        ///<summary>
        /// Some Tibia versions (early ones?) only use a single frame group, even for outfits. This function converts
        /// a ThingType to one that only has a single frame group.
        ///</summary>
        public static ThingType ToSingleFrameGroup(ThingType thingType)
        {
            if (thingType.Category != ThingCategory.Outfit || thingType.FrameGroupCount != 2)
            {
                return thingType;
            }

            FrameGroup? walkingFrameGroup = thingType.GetFrameGroup(FrameGroupType.Walking);
            FrameGroup? newGroup = walkingFrameGroup?.Clone();

            if (walkingFrameGroup.Frames > 1)
            {
                newGroup.Frames = (byte)(newGroup.Frames + 1);
                newGroup.SpriteIDs = new uint[newGroup.GetTotalSprites()];
                newGroup.IsAnimation = true;
                newGroup.FrameDurations = new FrameDuration[newGroup.Frames];

                for (int i = 0; i < newGroup.Frames; i++)
                {
                    if (newGroup.FrameDurations[i] != null)
                    {
                        newGroup.FrameDurations[i] = newGroup.FrameDurations[i];
                    }
                    else
                    {
                        newGroup.FrameDurations[i] = new FrameDuration(ThingCategory.Outfit);
                    }
                }
            }

            for (byte k = 0; k < thingType.FrameGroupCount; k++)
            {
                FrameGroup? group = thingType.GetFrameGroup((FrameGroupType)k);

                for (byte f = 0; f < group.Frames; f++)
                {
                    for (byte z = 0; z < group.PatternZ; z++)
                    {
                        for (byte y = 0; y < group.PatternY; y++)
                        {
                            for (byte x = 0; x < group.PatternX; x++)
                            {
                                for (byte l = 0; l < group.Layers; l++)
                                {
                                    for (byte w = 0; w < group.Width; w++)
                                    {
                                        for (byte h = 0; h < group.Height; h++)
                                        {
                                            if (k == (byte)FrameGroupType.Default && f == 0)
                                            {
                                                int i = group.GetSpriteIndex(w, h, l, x, y, z, f);
                                                int ni = newGroup.GetSpriteIndex(w, h, l, x, y, z, f);
                                                newGroup.SpriteIDs[ni] = group.SpriteIDs[i];
                                            }
                                            else if (k == (byte)FrameGroupType.Walking)
                                            {
                                                int i = group.GetSpriteIndex(w, h, l, x, y, z, f);
                                                int ni = newGroup.GetSpriteIndex(w, h, l, x, y, z, f + 1);
                                                newGroup.SpriteIDs[ni] = group.SpriteIDs[i];
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }

            thingType.FrameGroups[(int)FrameGroupType.Default] = newGroup;
            return thingType;
        }
    }
}