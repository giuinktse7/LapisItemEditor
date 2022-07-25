namespace Backend
{
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Text.Json.Serialization;
    using Proto = Tibia.Protobuf;




    public class Appearance
    {
        public class ChangeEntry
        {
            public class Change
            {
                [JsonPropertyName("from")]
                public string From { get; set; }

                [JsonPropertyName("to")]
                public string To { get; set; }

                [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
                [JsonPropertyName("nameInAppearances")]
                public string? AppearanceName { get; set; }

                public Change(string from, string to, string appearanceName)
                {
                    this.From = from;
                    this.To = to;
                    this.AppearanceName = appearanceName;
                }

                public Change(uint from, uint to, string appearanceName)
                {
                    this.From = from.ToString();
                    this.To = to.ToString();
                    this.AppearanceName = appearanceName;
                }

                public Change(bool from, bool to, string appearanceName)
                {
                    this.From = from.ToString().ToLower();
                    this.To = to.ToString().ToLower();
                    this.AppearanceName = appearanceName;
                }

                public Change(string from, string to)
                {
                    this.From = from;
                    this.To = to;
                }

                public Change(uint from, uint to)
                {
                    this.From = from.ToString();
                    this.To = to.ToString();
                }

                public Change(bool from, bool to)
                {
                    this.From = from.ToString().ToLower();
                    this.To = to.ToString().ToLower();
                }
            }

            public uint serverId { get; set; }
            public uint clientId { get; set; }
            public string? itemName { get; set; }
            public Dictionary<string, Change> changes { get; }

            public ChangeEntry()
            {
                this.changes = new Dictionary<string, Change>();
            }

            public void AddChange(string field, string oldValue, string newValue, string appearanceName)
            {
                changes.Add(field, new Change(oldValue, newValue, appearanceName));
            }

            public void AddChange(string field, uint oldValue, uint newValue, string appearanceName)
            {
                changes.Add(field, new Change(oldValue, newValue, appearanceName));
            }

            public void AddChange(string field, bool oldValue, bool newValue, string appearanceName)
            {
                changes.Add(field, new Change(oldValue, newValue, appearanceName));
            }

            public void AddChange(string field, string oldValue, string newValue)
            {
                changes.Add(field, new Change(oldValue, newValue));
            }

            public void AddChange(string field, uint oldValue, uint newValue)
            {
                changes.Add(field, new Change(oldValue, newValue));
            }

            public void AddChange(string field, bool oldValue, bool newValue)
            {
                changes.Add(field, new Change(oldValue, newValue));
            }
        }

        #region OT Properties

        public OtbItem? otbItem { get; set; }

        public bool AllowDistRead { get; set; }
        public bool Readable { get; set; }
        public bool IsAnimation { get; set; }

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
        public uint ServerId { get => (uint)(otbItem?.ServerId ?? 0); }

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
            get => otbItem.SpriteHash; set
            {
                otbItem.SpriteHash = value;
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

        public ChangeEntry? SyncOtbWithTibia()
        {
            if (otbItem == null)
            {
                return null;
            }

            // Server item group
            if (Data.Flags.Container)
            {
                otbItem.ServerItemGroup = ServerItemGroup.Container;
            }
            else if (Data.Flags.Fullbank || Data.Flags.Bank != null)
            {
                otbItem.ServerItemGroup = ServerItemGroup.Ground;
            }
            else if (Data.Flags.Liquidpool)
            {
                otbItem.ServerItemGroup = ServerItemGroup.Splash;
            }
            else if (Data.Flags.Liquidcontainer)
            {
                otbItem.ServerItemGroup = ServerItemGroup.Fluid;
            }
            else if (Data.Flags.ShowOffSocket)
            {
                otbItem.ServerItemGroup = ServerItemGroup.ShowOffSocket;
            }
            else
            {
                // return OtbServerItemGroup ?? ServerItemGroup.None;
                otbItem.ServerItemGroup = ServerItemGroup.None;
            }

            var flags = Data.Flags;

            var change = new ChangeEntry();
            change.serverId = this.ServerId;
            change.clientId = this.ClientId;
            change.itemName = Data.HasName ? Data.Name : otbItem.Name;

            if (Data.HasName && otbItem.Name != Data.Name)
            {
                change.AddChange("name", otbItem.Name ?? "", Data.Name);
                otbItem.Name = Data.Name;
            }

            if (this.HasLight)
            {
                if (flags.Light.HasColor && otbItem.LightColor != flags.Light.Color)
                {
                    change.AddChange("lightColor", otbItem.LightColor?.ToString() ?? "", flags.Light.Color.ToString(), "Light.Color");
                    otbItem.LightColor = (ushort)Data.Flags.Light.Color;
                }

                if (flags.Light.HasBrightness && otbItem.LightLevel != flags.Light.Brightness)
                {
                    change.AddChange("lightLevel", otbItem.LightLevel?.ToString() ?? "", flags.Light.Brightness.ToString(), "Light.Brightness");
                    otbItem.LightLevel = (ushort)Data.Flags.Light.Brightness;
                }
            }


            if (this.HasMarket && flags.Market.HasTradeAsObjectId && otbItem.WareId != (ushort)flags.Market.TradeAsObjectId)
            {
                change.AddChange("wareId", otbItem.WareId?.ToString() ?? "", flags.Market.TradeAsObjectId.ToString(), "Market.TradeAsObjectId");
                otbItem.WareId = (ushort)flags.Market.TradeAsObjectId;
            }

            if (this.HasUpgradeClassification && otbItem.UpgradeClassification != flags.Upgradeclassification.UpgradeClassification)
            {
                change.AddChange("upgradeClassification", otbItem.UpgradeClassification ?? 0, flags.Upgradeclassification.UpgradeClassification, "Upgradeclassification.UpgradeClassification");
                otbItem.UpgradeClassification = (byte)flags.Upgradeclassification.UpgradeClassification;
            }


            if (Data.HasDescription && otbItem.Description != Data.Description)
            {
                change.AddChange("description", otbItem.Description ?? "", Data.Description);
                otbItem.Description = Data.Description;
            }

            if (HasWrite && flags.Write.HasMaxTextLength && otbItem.MaxTextLen != flags.Write.MaxTextLength)
            {
                change.AddChange("maxTextLength", otbItem.MaxTextLen?.ToString() ?? "", flags.Write.MaxTextLength.ToString(), "Write.MaxTextLength");
                otbItem.MaxTextLen = (ushort)flags.Write.MaxTextLength;
            }

            if (HasWriteOnce && flags.WriteOnce.HasMaxTextLengthOnce && otbItem.MaxTextLenOnce != flags.WriteOnce.MaxTextLengthOnce)
            {
                change.AddChange("maxTextLengthOnce", otbItem.MaxTextLenOnce?.ToString() ?? "", flags.WriteOnce.MaxTextLengthOnce.ToString(), "WriteOnce.MaxTextLengthOnce");
                otbItem.MaxTextLenOnce = (ushort)flags.WriteOnce.MaxTextLengthOnce;
            }



            if (this.HasBank && otbItem.Speed != flags.Bank.Waypoints)
            {
                change.AddChange("speed", otbItem.Speed?.ToString() ?? "", flags.Bank.Waypoints.ToString(), "Bank.Waypoints");
                otbItem.Speed = (ushort)flags.Bank.Waypoints;
            }

            if (flags.Clip && otbItem.StackOrder != StackOrder.Border)
            {
                change.AddChange("stackOrder", otbItem.StackOrder.ToString(), StackOrder.Border.ToString(), "Clip");
                otbItem.StackOrder = StackOrder.Border;
            }
            else if (flags.Bottom && otbItem.StackOrder != StackOrder.Bottom)
            {
                change.AddChange("stackOrder", otbItem.StackOrder.ToString(), StackOrder.Bottom.ToString(), "Bottom");
                otbItem.StackOrder = StackOrder.Bottom;
            }
            else if (flags.Top && otbItem.StackOrder != StackOrder.Top)
            {
                change.AddChange("stackOrder", otbItem.StackOrder.ToString(), StackOrder.Top.ToString(), "Top");
                otbItem.StackOrder = StackOrder.Top;

            }

            if (HasAutomap && flags.Automap.HasColor && otbItem.MinimapColor != flags.Automap.Color)
            {
                change.AddChange("minimapColor", otbItem.MinimapColor?.ToString() ?? "", flags.Automap.Color.ToString(), "Automap.Color");
                otbItem.MinimapColor = (ushort)flags.Automap.Color;
            }

            // Otb flags
            if (Data.Flags.Hook != null)
            {
                handleFlag(change, Data.Flags.Hook.Direction == Proto.Shared.HOOK_TYPE.East, ItemTypeFlag.HookEast, "Hook.Direction");
                handleFlag(change, Data.Flags.Hook.Direction == Proto.Shared.HOOK_TYPE.South, ItemTypeFlag.HookSouth, "Hook.Direction");
            }

            handleFlag(change, flags.IgnoreLook, ItemTypeFlag.IgnoreLook, "ignoreLook");
            handleFlag(change, flags.Wearout, ItemTypeFlag.ClientCharges, "wearout");
            handleFlag(change, flags.Forceuse, ItemTypeFlag.ForceUse, "forceuse");

            var hasAnimation = Data.FrameGroup.Count != 0 && Data.FrameGroup[0].SpriteInfo.Animation != null;
            handleFlag(change, hasAnimation, ItemTypeFlag.Animation);


            handleFlag(change, flags.Unpass, ItemTypeFlag.BlockSolid, "unpass");
            handleFlag(change, flags.Unsight, ItemTypeFlag.BlockProjectile, "unsight");
            handleFlag(change, flags.Avoid, ItemTypeFlag.BlockPathfind, "avoid");
            handleFlag(change, flags.Multiuse, ItemTypeFlag.MultiUse, "multiuse");
            // OTB has no corresponding flag for "Usable"
            // handleFlag(change, flags.Usable, ????, "usable");
            handleFlag(change, flags.Take, ItemTypeFlag.Pickupable, "take");
            handleFlag(change, !flags.Unmove, ItemTypeFlag.Movable, "unmove");
            handleFlag(change, flags.Cumulative, ItemTypeFlag.Stackable, "cumulative");
            handleFlag(change, flags.Ammo, ItemTypeFlag.Ammo, "ammo");
            handleFlag(change, flags.Reportable, ItemTypeFlag.Reportable, "reportable");
            handleFlag(change, flags.Hang, ItemTypeFlag.Hangable, "hang");
            handleFlag(change, flags.Rotate, ItemTypeFlag.Rotatable, "rotate");

            handleFlag(change, flags.Top || flags.Clip || flags.Bottom, ItemTypeFlag.AlwaysOnTop);

            bool readable = flags.Write != null || flags.WriteOnce != null;
            handleFlag(change, readable, ItemTypeFlag.Readable);

            if (HasHeight)
            {
                handleFlag(change, flags.Height.HasElevation, ItemTypeFlag.HasElevation, "Height.HasElevation");
            }

            // All of expire, expireStop and clockExpire map to ClientDuration (as of 12.90).
            bool hasExpiryToggle = (flags.Expire || flags.Expirestop || flags.Clockexpire);
            handleFlag(change, hasExpiryToggle, ItemTypeFlag.ClientDuration, "expire | expireStop | clockExpire");

            return change.changes.Count > 0 ? change : null;
        }

        private void handleFlag(ChangeEntry entry, bool value, ItemTypeFlag flag, string? appearanceName = null)
        {
            if (otbItem == null)
            {
                return;
            }

            if (value != otbItem.Flags.HasFlag(flag))
            {
                var name = $"ItemTypeFlag.{flag.ToString()}";

                if (appearanceName == null)
                {
                    entry.AddChange(name, otbItem.Flags.HasFlag(flag), value);
                }
                else
                {
                    entry.AddChange(name, otbItem.Flags.HasFlag(flag), value, appearanceName);
                }
                if (value)
                {
                    otbItem.Flags |= flag;
                }
                else
                {
                    otbItem.Flags &= ~flag;
                }
            }
        }


        public bool HasAutomap { get => Data.Flags.Automap != null; }
        public bool HasWrite { get => Data.Flags.Write != null; }
        public bool HasHeight { get => Data.Flags.Height != null; }
        public bool HasWriteOnce { get => Data.Flags.WriteOnce != null; }
        public bool HasLight { get => Data.Flags.Light != null; }

        public bool HasBank { get => Data.Flags.Bank != null && Data.Flags.Bank.HasWaypoints; }
        public bool HasMarket { get => Data.Flags.Market != null; }
        public bool HasUpgradeClassification { get => Data.Flags.Upgradeclassification?.HasUpgradeClassification ?? false; }

        #endregion
    }
}