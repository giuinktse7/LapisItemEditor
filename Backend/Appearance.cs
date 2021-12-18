namespace Backend
{
    using Proto = Tibia.Protobuf;

    public class Appearance
    {
        #region OT Properties

        protected byte[]? spriteHash = null;

        public ServerItemGroup? OtbServerItemGroup { get; set; }

        public ServerItemGroup ServerItemGroup
        {
            get
            {
                if (Data.Flags.Container)
                {
                    return ServerItemGroup.Container;
                }
                else if (Data.Flags.Fullbank || Data.Flags.Bank != null)
                {
                    return ServerItemGroup.Ground;
                }
                else if (Data.Flags.Liquidpool)
                {
                    return ServerItemGroup.Splash;
                }
                else if (Data.Flags.Liquidcontainer)
                {
                    return ServerItemGroup.Fluid;
                }
                else if (Data.Flags.ShowOffSocket)
                {
                    return ServerItemGroup.ShowOffSocket;
                }
                else
                {
                    // return OtbServerItemGroup ?? ServerItemGroup.None;
                    return ServerItemGroup.None;
                }
            }
        }

        public ItemType_t ItemType { get; set; }
        public bool AllowDistRead { get; set; }
        public bool Readable { get; set; }
        public bool IsAnimation { get; set; }

        public ItemTypeFlag OtbFlags { get; set; }
        #endregion


        #region Tibia 7 format properties (tibia.dat & tibia.spr)

        public string? Article { get; set; }

        // Properties only present in Tibia7 format (tibia.dat)
        public bool HasCharges { get; set; }
        public bool FloorChange { get; set; }
        public string? MarketName { get; set; }
        public ushort MarketRestrictVocation { get; set; }
        public ushort MarketRestrictLevel { get; set; }
        public ThingCategory ThingCategory { get; set; }

        public uint ClientId { get => Data.Id; }
        public uint ServerId { get; set; }

        ///<summary>
        /// Amount of horizontal tiles the sprite takes.
        ///
        /// [32x32, 32x64] -> 1
        /// [64x32, 64x64] -> 2
        /// </summary>
        public byte SpriteTileWidth { get; set; }

        ///<summary>
        /// Amount of vertical tiles the sprite takes.
        ///
        /// [32x32, 64x32] -> 1
        /// [32x64, 64x64] -> 2
        /// </summary>
        public byte SpriteTileHeight { get; set; }

        public byte ExactSize { get; set; }

        #endregion

        public bool HasSprites { get => Data.FrameGroup.Count != 0; }

        public Proto.Appearances.Appearance Data { get; }
        public byte[]? SpriteHash
        {
            get => spriteHash; set
            {
                spriteHash = value;
            }
        }

        public Appearance(Proto.Appearances.Appearance appearance)
        {
            this.Data = appearance;
        }

        public Appearance(uint clientId)
        {
            this.Data = new Proto.Appearances.Appearance();
            Data.Id = clientId;
        }

        public uint GetDefaultSpriteId()
        {
            return Data.FrameGroup[0].SpriteInfo.SpriteId[0];
        }

        public void UpdateOtbFlags()
        {
            if (Data.Flags.Top || Data.Flags.Clip || Data.Flags.Bottom)
            {
                OtbFlags |= ItemTypeFlag.AlwaysOnTop;
            }

            if (Data.Flags.Cumulative)
            {
                OtbFlags |= ItemTypeFlag.Stackable;
            }

            if (Data.Flags.Unsight)
            {
                OtbFlags |= ItemTypeFlag.BlockSolid;
            }

            if (Data.Flags.Hang)
            {
                OtbFlags |= ItemTypeFlag.Hangable;
            }

            if (Data.Flags.Rotate)
            {
                OtbFlags |= ItemTypeFlag.Rotatable;
            }

            if (Data.Flags.Height != null && Data.Flags.Height.HasElevation)
            {
                OtbFlags |= ItemTypeFlag.HasElevation;
            }


            bool readable = Data.Flags.Write != null || Data.Flags.WriteOnce != null;
            if (readable)
            {
                OtbFlags |= ItemTypeFlag.Readable;
            }

            if (Data.Flags.Hook != null)
            {
                if (Data.Flags.Hook.Direction == Proto.Shared.HOOK_TYPE.East)
                {
                    OtbFlags |= ItemTypeFlag.HookEast;
                }
                else if (Data.Flags.Hook.Direction == Proto.Shared.HOOK_TYPE.South)
                {
                    OtbFlags |= ItemTypeFlag.HookSouth;
                }
            }

            if (Data.Flags.IgnoreLook)
            {
                OtbFlags |= ItemTypeFlag.IgnoreLook;
            }

            if (Data.FrameGroup.Count != 0 && Data.FrameGroup[0].SpriteInfo.Animation != null)
            {
                OtbFlags |= ItemTypeFlag.Animation;
            }

            if (Data.Flags.Forceuse)
            {
                OtbFlags |= ItemTypeFlag.ForceUse;
            }

            if (Data.Flags.Multiuse)
            {
                OtbFlags |= ItemTypeFlag.MultiUse;
            }

            if (Data.Flags.Avoid)
            {
                OtbFlags |= ItemTypeFlag.BlockPathfind;
            }

            if (Data.Flags.Take)
            {
                OtbFlags |= ItemTypeFlag.Pickupable;
            }

            if (!Data.Flags.Unmove)
            {
                OtbFlags |= ItemTypeFlag.Movable;
            }

            if (Data.Flags.Unsight)
            {
                OtbFlags |= ItemTypeFlag.BlockProjectile;
            }

            if (Data.Flags.Unpass)
            {
                OtbFlags |= ItemTypeFlag.BlockSolid;
            }

            if (Data.Flags.Ammo)
            {
                OtbFlags |= ItemTypeFlag.Ammo;
            }

            if (Data.Flags.Reportable)
            {
                OtbFlags |= ItemTypeFlag.Reportable;
            }
        }

        public ItemTypeFlag GetOtbFlags()
        {
            return OtbFlags;
        }

        #region Data setters
        public void SetAutomapColor(ushort color)
        {
            Data.Flags.Automap = new Proto.Appearances.AppearanceFlagAutomap()
            {
                Color = color
            };
        }

        public void SetHookDirection(Proto.Shared.HOOK_TYPE direction)
        {
            Data.Flags.Hook = new Proto.Appearances.AppearanceFlagHook()
            {
                Direction = direction
            };
        }

        public void setMaxTextLength(ushort length)
        {
            Data.Flags.Write = new Proto.Appearances.AppearanceFlagWrite
            {
                MaxTextLength = length
            };
        }

        public void setMaxTextLengthOnce(ushort length)
        {
            Data.Flags.WriteOnce = new Proto.Appearances.AppearanceFlagWriteOnce
            {
                MaxTextLengthOnce = length
            };
        }

        public void setLight(ushort brightness, ushort color)
        {
            Data.Flags.Light = new Proto.Appearances.AppearanceFlagLight()
            {
                Brightness = brightness,
                Color = color
            };
        }

        public Proto.Appearances.AppearanceFlagMarket GetOrCreateMarketFlags()
        {
            var flags = Data.Flags.Market;

            if (flags != null)
            {
                return flags;
            }
            else
            {
                Data.Flags.Market = new Proto.Appearances.AppearanceFlagMarket();
                return Data.Flags.Market;
            }
        }

        public bool HasAutomap { get => Data.Flags.Automap != null; }
        public bool HasWrite { get => Data.Flags.Write != null; }
        public bool HasWriteOnce { get => Data.Flags.WriteOnce != null; }
        public bool HasLight { get => Data.Flags.Light != null; }
        public bool HasBank { get => Data.Flags.Bank != null && Data.Flags.Bank.HasWaypoints; }
        public bool HasMarket { get => Data.Flags.Market != null; }
        public bool HasUpgradeClassification { get => Data.Flags.Upgradeclassification?.HasUpgradeClassification ?? false; }

        #endregion
    }
}