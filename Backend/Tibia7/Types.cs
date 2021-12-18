namespace Backend.Tibia7
{

    public enum ThingTypeAttribute : byte
    {
        Ground = 0,
        GroundBorder = 1,
        OnBottom = 2,
        OnTop = 3,
        Container = 4,
        Stackable = 5,
        ForceUse = 6,
        MultiUse = 7,
        Writable = 8,
        WritableOnce = 9,
        FluidContainer = 10,
        LiquidPool = 11,
        UnPass = 12,
        UnMove = 13,
        UnSight = 14,
        Avoid = 15,
        Take = 16,
        Hang = 17,
        HookSouth = 18,
        HookEast = 19,
        Rotatable = 20,
        Light = 21,
        DontHide = 22,
        Translucent = 23,
        Displacement = 24,
        Elevation = 25,
        LyingCorpse = 26,
        AnimateAlways = 27,
        Automap = 28,
        LensHelp = 29,
        FullGround = 30,
        IgnoreLook = 31,
        Cloth = 32,
        Market = 33,
        Usable = 34,
        Wrap = 35,
        Unwrap = 36,
        TopEffect = 37,

        // additional
        Opacity = 100,
        NotPreWalkable = 101,

        DefaultAction = 251,

        FloorChange = 252,
        NoMoveAnimation = 253, // 10.10: real value is 16, but we need to do this for backwards compatibility
        Chargeable = 254, // deprecated
        LastAttribute = 255
    }
}