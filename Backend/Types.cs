
using System;

namespace Backend
{
    public enum ClientVersion : uint
    {
        Invalid = 0,

        // Tibia 7 format
        V7_10 = 710,
        V7_40 = 740,
        V7_55 = 755,
        V7_80 = 780,
        V8_60 = 860,
        V9_60 = 960,
        V10_10 = 1010,
        V10_50 = 1050,
        V10_57 = 1057,
        V10_92 = 1092,
        V10_93 = 1093,

        // Tibia 11 format
        V11_00 = 1100,
        V11_02 = 1102,
        V11_40 = 1140,
        V11_50 = 1150,
        V11_80 = 1180,
        V12_00 = 1200,
        V12_02 = 1202,
        V12_15 = 1215,
        V12_20 = 1220,
        V12_30 = 1230,
        V12_70 = 1270,
        V12_71 = 1271,
        V12_72 = 1272,
        V12_81 = 1281,
    }

    [Flags]
    public enum ClientFeatures
    {
        None = 0,
        PatternZ = 1 << 0,
        Extended = 1 << 1,
        FrameDurations = 1 << 2,
        FrameGroups = 1 << 3,
        Transparency = 1 << 4
    }

    public enum ClothSlot : ushort
    {
        None = 0,
        TwoHandWeapon = 1,
        Helmet = 2,
        Amulet = 3,
        Backpack = 4,
        Armor = 5,
        Shield = 6,
        OneHandWeapon = 7,
        Legs = 8,
        Boots = 9,
        Ring = 10,
        Arrow = 11
    }

    public enum MarketCategory : ushort
    {
        None = 0,
        Armors = 1,
        Amulets = 2,
        Boots = 3,
        Containers = 4,
        Decoration = 5,
        Food = 6,
        HelmetsAndHats = 7,
        Legs = 8,
        Others = 9,
        Potions = 10,
        Rings = 11,
        Runes = 12,
        Shields = 13,
        Tools = 14,
        Valuables = 15,
        Ammunition = 16,
        Axes = 17,
        Clubs = 18,
        DistanceWeapons = 19,
        Swords = 20,
        WandsAndRods = 21,
        PremiumScrolls = 22,
        MetaWeapons = 255
    }

}
