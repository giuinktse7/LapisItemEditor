
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Backend.IO;

namespace Backend.Tibia7
{
    using Proto = Tibia.Protobuf;

    public class DatIO
    {
        public ClientFeatures ClientFeatures { get; private set; }
        public Tibia7VersionData Version { get; private set; }
        public string Path { get; private set; }


        public ushort ItemTypeCount { get; private set; }
        public ushort MissileCount { get; private set; }
        public ushort EffectCount { get; private set; }
        public ushort OutfitCount { get; private set; }

        public bool IsBusy { get; private set; } = false;
        public bool Changed { get; private set; } = false;
        public string? FilePath { get; private set; }

        private bool usePatternZ;
        private bool useExtended;
        private bool useFrameDurations;
        private bool useFrameGroups;

        public DatIO(string path, Tibia7VersionData version, ClientFeatures features)
        {
            this.Path = path;
            this.Version = version;
            this.ClientFeatures = features;

            usePatternZ = ClientFeatures.HasFlag(ClientFeatures.PatternZ);
            useExtended = ClientFeatures.HasFlag(ClientFeatures.Extended);
            useFrameDurations = ClientFeatures.HasFlag(ClientFeatures.FrameDurations);
            useFrameGroups = ClientFeatures.HasFlag(ClientFeatures.FrameGroups);
        }

        public static DatIO Load(string path, Tibia7VersionData version, ClientFeatures features, AppearanceData appearanceData)
        {
            if (string.IsNullOrEmpty(path))
            {
                throw new ArgumentNullException(nameof(path));
            }

            if (!File.Exists(path))
            {
                throw new FileNotFoundException($"File not found: {path}", path);
            }

            var datData = new DatIO(path, version, features);
            datData.Read(appearanceData);

            return datData;
        }

        private bool Read(AppearanceData data)
        {
            if (this.IsBusy)
            {
                System.Diagnostics.Trace.WriteLine("_Load: IsBusy, can not load.");
                return false;
            }

            using var stream = new FileStream(Path, FileMode.Open);
            using var reader = new BinaryReader(stream);

            uint signature = reader.ReadUInt32();
            if (signature != Version.DatSignature)
            {
                throw new Exception($"Invalid DAT signature. Expected {Version.DatSignature} but found {signature}.");
            }

            ItemTypeCount = reader.ReadUInt16();
            OutfitCount = reader.ReadUInt16();
            EffectCount = reader.ReadUInt16();
            MissileCount = reader.ReadUInt16();

            int total = ItemTypeCount + OutfitCount + EffectCount + MissileCount;

            // Items
            data.Objects.SetItemCount(ItemTypeCount);
            for (uint clientId = 100; clientId <= ItemTypeCount; ++clientId)
            {
                var appearance = new Appearance(clientId);
                appearance.ThingCategory = ThingCategory.Item;

                ReadProperties(appearance, reader);
                ReadTexturePatterns(appearance, reader);

                data.Objects.Add(clientId, appearance);
            }

            // Outfits
            data.Outfits.SetItemCount(OutfitCount);
            for (uint id = 1; id <= OutfitCount; ++id)
            {
                var appearance = new Appearance(id);
                appearance.ThingCategory = ThingCategory.Outfit;


                ReadProperties(appearance, reader);
                ReadTexturePatterns(appearance, reader);

                data.Outfits.Add(id, appearance);
            }

            // Effects
            data.Effects.SetItemCount(EffectCount);
            for (uint id = 1; id <= EffectCount; ++id)
            {
                var appearance = new Appearance(id);
                appearance.ThingCategory = ThingCategory.Effect;

                ReadProperties(appearance, reader);
                ReadTexturePatterns(appearance, reader);

                data.Effects.Add(id, appearance);
            }

            // Missiles
            data.Missiles.SetItemCount(MissileCount);
            for (uint id = 1; id <= MissileCount; ++id)
            {
                var appearance = new Appearance(id);
                appearance.ThingCategory = ThingCategory.Missile;

                ReadProperties(appearance, reader);
                ReadTexturePatterns(appearance, reader);

                data.Missiles.Add(id, appearance);
            }

            return true;
        }

        // private bool Read(ItemData itemData)
        // {
        //     if (this.IsBusy)
        //     {
        //         System.Diagnostics.Trace.WriteLine("_Load: IsBusy, can not load.");
        //         return false;
        //     }

        //     using (var stream = new FileStream(Path, FileMode.Open))
        //     {
        //         var reader = new BinaryReader(stream);

        //         uint signature = reader.ReadUInt32();
        //         if (signature != Version.DatSignature)
        //         {
        //             throw new Exception($"Invalid DAT signature. Expected {Version.DatSignature} but found {signature}.");
        //         }

        //         ItemTypeCount = reader.ReadUInt16();
        //         OutfitCount = reader.ReadUInt16();
        //         EffectCount = reader.ReadUInt16();
        //         MissileCount = reader.ReadUInt16();

        //         int total = ItemTypeCount + OutfitCount + EffectCount + MissileCount;

        //         // Items
        //         itemData.SetItemCount(ItemTypeCount);
        //         for (uint clientId = 100; clientId <= ItemTypeCount; ++clientId)
        //         {
        //             var thingType = new ThingType(clientId, ThingCategory.Item);
        //             ReadProperties(thingType, reader);
        //             ReadTexturePatterns(thingType, reader);

        //             // itemData.ItemTypes.Add(clientId, thingType);
        //             itemData.AddItemType(clientId, thingType);
        //         }

        //         // Outfits
        //         for (uint id = 1; id <= OutfitCount; ++id)
        //         {
        //             var thingType = new ThingType(id, ThingCategory.Outfit);
        //             ReadProperties(thingType, reader);
        //             ReadTexturePatterns(thingType, reader);

        //             itemData.Outfits.Add(id, thingType);
        //         }

        //         // Effects
        //         for (uint id = 1; id <= EffectCount; ++id)
        //         {
        //             var thingType = new ThingType(id, ThingCategory.Effect);
        //             ReadProperties(thingType, reader);
        //             ReadTexturePatterns(thingType, reader);

        //             itemData.Effects.Add(id, thingType);
        //         }

        //         // Missiles
        //         for (uint id = 1; id <= MissileCount; ++id)
        //         {
        //             var thingType = new ThingType(id, ThingCategory.Missile);
        //             ReadProperties(thingType, reader);
        //             ReadTexturePatterns(thingType, reader);

        //             itemData.Missiles.Add(id, thingType);
        //         }
        //     }

        //     return true;
        // }

        // public void Save(string filePath, VersionData version, ItemData itemData)
        // {
        //     Save(filePath, version, this.ClientFeatures, itemData);
        // }

        public void Save(string filePath, Tibia7VersionData version, AppearanceData appearanceData)
        {
            Save(filePath, version, this.ClientFeatures, appearanceData);
        }


        public void Save(string filePath, Tibia7VersionData version, ClientFeatures features, AppearanceData appearanceData)
        {
            if (filePath == null)
            {
                throw new ArgumentNullException(nameof(filePath));
            }

            if (version == null)
            {
                throw new ArgumentNullException(nameof(version));
            }

            string? directory = System.IO.Path.GetDirectoryName(filePath);
            if (directory == null)
            {
                throw new ArgumentNullException($"Could not create directory for file path '{filePath}'");
            }

            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            bool sameVersion = this.Version.Equals(version);
            bool sameFeatures = this.ClientFeatures.Equals(features);
            bool samePath = filePath.Equals(this.FilePath);
            if (!Changed && sameVersion && sameFeatures && !samePath)
            {
                // TODO Just copy the file
            }

            string tempPath = System.IO.Path.Combine(directory, System.IO.Path.GetFileNameWithoutExtension(filePath) + ".tmp");

            using var writer = new BinaryDataWriter(new FileStream(tempPath, FileMode.Create));
            writer.Write(version.DatSignature);

            writer.WriteU16((ushort)appearanceData.Objects.Count);

            for (uint clientId = 100; clientId <= appearanceData.Objects.Count; clientId++)
            {
                Appearance? appearance = appearanceData.Objects.Get(clientId);
                if (appearance == null)
                {
                    throw new NullReferenceException($"No ItemType with clientID '{clientId}'.");
                }

                WriteProperties(appearance, version.Format, writer);
                WriteTexturePatterns(appearance, features, writer);
            }

            // TODO Not finished

            writer.WriteU16((ushort)appearanceData.Outfits.Count);
            writer.WriteU16((ushort)appearanceData.Effects.Count);
            writer.WriteU16((ushort)appearanceData.Missiles.Count);

            throw new NotImplementedException("DatIO::Save is not fully implemented.");
        }

        private void ReadProperties(Appearance appearance, BinaryReader reader)
        {
            appearance.Data.Flags = new Proto.Appearances.AppearanceFlags();
            var flags = appearance.Data.Flags;

            byte attribute;
            while ((attribute = reader.ReadByte()) != (byte)ThingTypeAttribute.LastAttribute)
            {
                if (Version.Format >= DatFormat.Format_1010)
                {
                    // In 10.10+ all attributes from 16 and up were incremented by 1 to make space for 16 as
                    // "No Movement Animation" flag.
                    if (attribute == 16)
                    {
                        attribute = (byte)ThingTypeAttribute.NoMoveAnimation;
                    }
                    else if (attribute == 254) // Usable
                    {
                        flags.Usable = true;
                        continue;
                    }
                    else if (attribute == 35) // Default Action
                    {
                        var defaultAction = (Proto.Shared.PLAYER_ACTION)reader.ReadUInt16();
                        flags.DefaultAction = new Proto.Appearances.AppearanceFlagDefaultAction()
                        {
                            Action = defaultAction
                        };

                        continue;
                    }
                    else if (attribute > 16)
                    {
                        --attribute;
                    }
                }
                else if (Version.Format >= DatFormat.Format_860)
                {
                    // Default attribute values follow the format of 8.6-9.86. No changes here.
                }
                else if (Version.Format >= DatFormat.Format_780)
                {
                    // In 7.80-8.54 all attributes from 8 and higher were incremented by 1 to make space for 8 as
                    // "Item Charges" flag.
                    if (attribute == 8)
                    {
                        appearance.HasCharges = true;
                        continue;
                    }
                    if (attribute > 8)
                    {
                        --attribute;
                    }
                }
                else if (Version.Format >= DatFormat.Format_755)
                {
                    // In 7.55-7.72 attributes 23 is "Floor Change".
                    if (attribute == 23)
                    {
                        appearance.FloorChange = true;
                        continue;
                    }
                }
                else if (Version.Format >= DatFormat.Format_740)
                {
                    // In 7.4-7.5 attribute "Ground Border" did not exist. Attributes 1-15 have to be adjusted.
                    // Several other changes in the format.
                    if (attribute > 0 && attribute <= 15)
                    {
                        attribute += 1;
                    }
                    else if (attribute == 16)
                    {
                        attribute = (byte)ThingTypeAttribute.Light;
                    }
                    else if (attribute == 17)
                    {
                        attribute = (byte)ThingTypeAttribute.FloorChange;
                    }
                    else if (attribute == 18)
                    {
                        attribute = (byte)ThingTypeAttribute.FullGround;

                    }
                    else if (attribute == 19)
                    {
                        attribute = (byte)ThingTypeAttribute.Elevation;

                    }
                    else if (attribute == 20)
                    {
                        attribute = (byte)ThingTypeAttribute.Displacement;
                    }
                    else if (attribute == 22)
                    {
                        attribute = (byte)ThingTypeAttribute.Automap;
                    }
                    else if (attribute == 23)
                    {
                        attribute = (byte)ThingTypeAttribute.Rotatable;
                    }
                    else if (attribute == 24)
                    {
                        attribute = (byte)ThingTypeAttribute.LyingCorpse;
                    }
                    else if (attribute == 25)
                    {
                        attribute = (byte)ThingTypeAttribute.Hang;
                    }
                    else if (attribute == 26)
                    {
                        attribute = (byte)ThingTypeAttribute.HookSouth;
                    }
                    else if (attribute == 27)
                    {
                        attribute = (byte)ThingTypeAttribute.HookEast;
                    }
                    else if (attribute == 28)
                    {
                        attribute = (byte)ThingTypeAttribute.AnimateAlways;
                    }


                    /* "Multi Use" and "Force Use" are swapped */
                    if (attribute == (byte)ThingTypeAttribute.MultiUse)
                    {
                        attribute = (byte)ThingTypeAttribute.ForceUse;
                    }
                    else if (attribute == (byte)ThingTypeAttribute.ForceUse)
                    {
                        attribute = (byte)ThingTypeAttribute.MultiUse;
                    }
                }

                switch ((ThingTypeAttribute)attribute)
                {
                    case ThingTypeAttribute.Ground:
                        ushort speed = reader.ReadUInt16();
                        flags.Bank = new Proto.Appearances.AppearanceFlagBank() { Waypoints = speed };
                        break;

                    case ThingTypeAttribute.GroundBorder:
                        flags.Clip = true;
                        break;

                    case ThingTypeAttribute.OnBottom:
                        flags.Bottom = true;
                        break;

                    case ThingTypeAttribute.OnTop:
                        flags.Top = true;
                        break;

                    case ThingTypeAttribute.Container:
                        flags.Container = true;
                        break;

                    case ThingTypeAttribute.Stackable:
                        flags.Cumulative = true;
                        break;

                    case ThingTypeAttribute.ForceUse:
                        flags.Forceuse = true;
                        break;

                    case ThingTypeAttribute.MultiUse:
                        flags.Multiuse = true;
                        break;

                    case ThingTypeAttribute.Writable:
                        var maxTextLength = reader.ReadUInt16();
                        flags.Write = new Proto.Appearances.AppearanceFlagWrite()
                        {
                            MaxTextLength = maxTextLength
                        };
                        break;

                    case ThingTypeAttribute.WritableOnce:
                        var maxTextLengthOnce = reader.ReadUInt16();
                        flags.WriteOnce = new Proto.Appearances.AppearanceFlagWriteOnce()
                        {
                            MaxTextLengthOnce = maxTextLengthOnce
                        };
                        break;

                    case ThingTypeAttribute.FluidContainer:
                        flags.Liquidcontainer = true;
                        break;

                    case ThingTypeAttribute.LiquidPool:
                        flags.Liquidpool = true;
                        break;

                    case ThingTypeAttribute.UnPass:
                        flags.Unpass = true;
                        break;

                    case ThingTypeAttribute.UnMove:
                        flags.Unmove = true;
                        break;

                    case ThingTypeAttribute.UnSight:
                        flags.Unsight = true;
                        break;

                    case ThingTypeAttribute.Avoid:
                        flags.Avoid = true;
                        break;

                    case ThingTypeAttribute.NoMoveAnimation: // 0x10
                        flags.NoMovementAnimation = true;
                        break;

                    case ThingTypeAttribute.Take:
                        flags.Take = true;
                        break;

                    case ThingTypeAttribute.Hang:
                        flags.Hang = true;
                        break;

                    case ThingTypeAttribute.HookSouth:
                        flags.Hook = new Proto.Appearances.AppearanceFlagHook()
                        {
                            Direction = Proto.Shared.HOOK_TYPE.South
                        };
                        break;

                    case ThingTypeAttribute.HookEast:
                        flags.Hook = new Proto.Appearances.AppearanceFlagHook()
                        {
                            Direction = Proto.Shared.HOOK_TYPE.East
                        };
                        break;

                    case ThingTypeAttribute.Rotatable:
                        flags.Rotate = true;
                        break;

                    case ThingTypeAttribute.Light:
                        {
                            var brightness = reader.ReadUInt16();
                            var color = reader.ReadUInt16();
                            flags.Light = new Proto.Appearances.AppearanceFlagLight()
                            {
                                Brightness = brightness,
                                Color = color
                            };
                            break;
                        }

                    case ThingTypeAttribute.DontHide:
                        flags.DontHide = true;
                        break;

                    case ThingTypeAttribute.Translucent:
                        flags.Translucent = true;
                        break;

                    case ThingTypeAttribute.Displacement:
                        {
                            var shiftX = reader.ReadUInt16();
                            var shiftY = reader.ReadUInt16();
                            flags.Shift = new Proto.Appearances.AppearanceFlagShift()
                            {
                                X = shiftX,
                                Y = shiftY
                            };

                            break;
                        }

                    case ThingTypeAttribute.Elevation:
                        var elevation = reader.ReadUInt16();
                        flags.Height = new Proto.Appearances.AppearanceFlagHeight()
                        {
                            Elevation = elevation
                        };

                        break;

                    case ThingTypeAttribute.LyingCorpse:
                        flags.LyingObject = true;
                        break;

                    case ThingTypeAttribute.Automap:
                        var automapColor = reader.ReadUInt16();
                        flags.Automap = new Proto.Appearances.AppearanceFlagAutomap()
                        {
                            Color = automapColor
                        };
                        break;

                    case ThingTypeAttribute.AnimateAlways:
                        flags.AnimateAlways = true;
                        break;

                    case ThingTypeAttribute.LensHelp:
                        var lensHelpId = reader.ReadUInt16();
                        flags.Lenshelp = new Proto.Appearances.AppearanceFlagLenshelp()
                        {
                            Id = lensHelpId
                        };
                        break;

                    case ThingTypeAttribute.FullGround:
                        flags.Fullbank = true;
                        break;

                    case ThingTypeAttribute.IgnoreLook:
                        flags.IgnoreLook = true;
                        break;

                    case ThingTypeAttribute.Cloth:
                        var slot = reader.ReadUInt16();
                        flags.Clothes = new Proto.Appearances.AppearanceFlagClothes()
                        {
                            Slot = slot
                        };

                        break;

                    case ThingTypeAttribute.Market:
                        {
                            var marketCategory = (Proto.Shared.ITEM_CATEGORY)reader.ReadUInt16();
                            var tradeAsObjectId = reader.ReadUInt16();
                            var showAsObjectId = reader.ReadUInt16();

                            flags.Market = new Proto.Appearances.AppearanceFlagMarket()
                            {
                                Category = marketCategory,
                                TradeAsObjectId = tradeAsObjectId,
                                ShowAsObjectId = showAsObjectId
                            };

                            ushort nameLength = reader.ReadUInt16();
                            byte[] buffer = reader.ReadBytes(nameLength);

                            appearance.MarketName = Encoding.Default.GetString(buffer, 0, buffer.Length);
                            appearance.MarketRestrictVocation = reader.ReadUInt16();
                            appearance.MarketRestrictLevel = reader.ReadUInt16();
                            break;
                        }

                    case ThingTypeAttribute.DefaultAction:
                        var defaultAction = (Proto.Shared.PLAYER_ACTION)reader.ReadUInt16();
                        flags.DefaultAction = new Proto.Appearances.AppearanceFlagDefaultAction()
                        {
                            Action = defaultAction
                        };
                        break;

                    case ThingTypeAttribute.Wrap:
                        flags.Wrap = true;
                        break;

                    case ThingTypeAttribute.Unwrap:
                        flags.Unwrap = true;
                        break;

                    case ThingTypeAttribute.TopEffect:
                        flags.Topeffect = true;
                        break;

                    case ThingTypeAttribute.Usable:
                        flags.Usable = true;
                        break;

                    default:
                        throw new Exception(string.Format("Error while parsing, unknown flag 0x{0:X} at object id {1}.", attribute, appearance.ClientId));
                }
            }
        }

        private void ReadProperties(ThingType thingType, BinaryReader reader)
        {
            byte attribute;
            while ((attribute = reader.ReadByte()) != (byte)ThingTypeAttribute.LastAttribute)
            {
                if (Version.Format >= DatFormat.Format_1010)
                {
                    // In 10.10+ all attributes from 16 and up were incremented by 1 to make space for 16 as
                    // "No Movement Animation" flag.
                    if (attribute == 16)
                    {
                        attribute = (byte)ThingTypeAttribute.NoMoveAnimation;
                    }
                    else if (attribute == 254) // Usable
                    {
                        thingType.Usable = true;
                        continue;
                    }
                    else if (attribute == 35) // Default Action
                    {
                        thingType.HasAction = true;
                        thingType.DefaultAction = (DefaultAction)reader.ReadUInt16();
                        continue;
                    }
                    else if (attribute > 16)
                    {
                        --attribute;
                    }
                }
                else if (Version.Format >= DatFormat.Format_860)
                {
                    // Default attribute values follow the format of 8.6-9.86. No changes here.
                }
                else if (Version.Format >= DatFormat.Format_780)
                {
                    // In 7.80-8.54 all attributes from 8 and higher were incremented by 1 to make space for 8 as
                    // "Item Charges" flag.
                    if (attribute == 8)
                    {
                        thingType.HasCharges = true;
                        continue;
                    }
                    if (attribute > 8)
                    {
                        --attribute;
                    }
                }
                else if (Version.Format >= DatFormat.Format_755)
                {
                    // In 7.55-7.72 attributes 23 is "Floor Change".
                    if (attribute == 23)
                    {
                        thingType.FloorChange = true;
                        continue;
                    }
                }
                else if (Version.Format >= DatFormat.Format_740)
                {
                    // In 7.4-7.5 attribute "Ground Border" did not exist. Attributes 1-15 have to be adjusted.
                    // Several other changes in the format.
                    if (attribute > 0 && attribute <= 15)
                    {
                        attribute += 1;
                    }
                    else if (attribute == 16)
                    {
                        attribute = (byte)ThingTypeAttribute.Light;
                    }
                    else if (attribute == 17)
                    {
                        attribute = (byte)ThingTypeAttribute.FloorChange;
                    }
                    else if (attribute == 18)
                    {
                        attribute = (byte)ThingTypeAttribute.FullGround;

                    }
                    else if (attribute == 19)
                    {
                        attribute = (byte)ThingTypeAttribute.Elevation;

                    }
                    else if (attribute == 20)
                    {
                        attribute = (byte)ThingTypeAttribute.Displacement;
                    }
                    else if (attribute == 22)
                    {
                        attribute = (byte)ThingTypeAttribute.Automap;
                    }
                    else if (attribute == 23)
                    {
                        attribute = (byte)ThingTypeAttribute.Rotatable;
                    }
                    else if (attribute == 24)
                    {
                        attribute = (byte)ThingTypeAttribute.LyingCorpse;
                    }
                    else if (attribute == 25)
                    {
                        attribute = (byte)ThingTypeAttribute.Hang;
                    }
                    else if (attribute == 26)
                    {
                        attribute = (byte)ThingTypeAttribute.HookSouth;
                    }
                    else if (attribute == 27)
                    {
                        attribute = (byte)ThingTypeAttribute.HookEast;
                    }
                    else if (attribute == 28)
                    {
                        attribute = (byte)ThingTypeAttribute.AnimateAlways;
                    }


                    /* "Multi Use" and "Force Use" are swapped */
                    if (attribute == (byte)ThingTypeAttribute.MultiUse)
                    {
                        attribute = (byte)ThingTypeAttribute.ForceUse;
                    }
                    else if (attribute == (byte)ThingTypeAttribute.ForceUse)
                    {
                        attribute = (byte)ThingTypeAttribute.MultiUse;
                    }
                }

                switch ((ThingTypeAttribute)attribute)
                {
                    case ThingTypeAttribute.GroundBorder:
                        thingType.StackOrder = StackOrder.Border;
                        break;

                    case ThingTypeAttribute.OnBottom:
                        thingType.StackOrder = StackOrder.Bottom;
                        break;

                    case ThingTypeAttribute.OnTop:
                        thingType.StackOrder = StackOrder.Top;
                        break;

                    case ThingTypeAttribute.Container:
                        thingType.IsContainer = true;
                        break;

                    case ThingTypeAttribute.Stackable:
                        thingType.Stackable = true;
                        break;

                    case ThingTypeAttribute.ForceUse:
                        thingType.ForceUse = true;
                        break;

                    case ThingTypeAttribute.MultiUse:
                        thingType.MultiUse = true;
                        break;

                    case ThingTypeAttribute.Writable:
                        thingType.Writable = true;
                        thingType.MaxTextLength = reader.ReadUInt16();
                        break;

                    case ThingTypeAttribute.WritableOnce:
                        thingType.WritableOnce = true;
                        thingType.MaxTextLength = reader.ReadUInt16();
                        break;

                    case ThingTypeAttribute.FluidContainer:
                        thingType.IsFluidContainer = true;
                        break;

                    case ThingTypeAttribute.LiquidPool:
                        thingType.IsLiquidPool = true;
                        break;

                    case ThingTypeAttribute.UnPass:
                        thingType.Unpassable = true;
                        break;

                    case ThingTypeAttribute.UnMove:
                        thingType.Unmovable = true;
                        break;

                    case ThingTypeAttribute.UnSight:
                        thingType.BlockMissiles = true;
                        break;

                    case ThingTypeAttribute.Avoid:
                        thingType.BlockPathfinder = true;
                        break;

                    case ThingTypeAttribute.NoMoveAnimation: // 0x10
                        thingType.NoMoveAnimation = true;
                        break;

                    case ThingTypeAttribute.Take:
                        thingType.Pickupable = true;
                        break;

                    case ThingTypeAttribute.Hang:
                        thingType.Hangable = true;
                        break;

                    case ThingTypeAttribute.HookSouth:
                        thingType.HookSouth = true;
                        break;

                    case ThingTypeAttribute.HookEast:
                        thingType.HookEast = true;
                        break;

                    case ThingTypeAttribute.Rotatable:
                        thingType.Rotatable = true;
                        break;

                    case ThingTypeAttribute.Light:
                        thingType.LightLevel = reader.ReadUInt16();
                        thingType.LightColor = reader.ReadUInt16();
                        break;

                    case ThingTypeAttribute.DontHide:
                        thingType.DontHide = true;
                        break;

                    case ThingTypeAttribute.Translucent:
                        thingType.Translucent = true;
                        break;

                    case ThingTypeAttribute.Displacement:
                        thingType.HasOffset = true;
                        thingType.OffsetX = reader.ReadUInt16();
                        thingType.OffsetY = reader.ReadUInt16();
                        break;

                    case ThingTypeAttribute.Elevation:
                        thingType.HasElevation = true;
                        thingType.Elevation = reader.ReadUInt16();
                        break;

                    case ThingTypeAttribute.LyingCorpse:
                        thingType.LyingObject = true;
                        break;

                    case ThingTypeAttribute.Automap:
                        thingType.Minimap = true;
                        thingType.MinimapColor = reader.ReadUInt16();
                        break;

                    case ThingTypeAttribute.AnimateAlways:
                        thingType.AnimateAlways = true;
                        break;

                    case ThingTypeAttribute.LensHelp:
                        thingType.IsLensHelp = true;
                        thingType.LensHelp = reader.ReadUInt16();
                        break;

                    case ThingTypeAttribute.FullGround:
                        thingType.FullGround = true;
                        break;

                    case ThingTypeAttribute.IgnoreLook:
                        thingType.IgnoreLook = true;
                        break;

                    case ThingTypeAttribute.Cloth:
                        thingType.IsCloth = true;
                        thingType.ClothSlot = (ClothSlot)reader.ReadUInt16();
                        break;

                    case ThingTypeAttribute.Market:
                        thingType.IsMarketItem = true;
                        thingType.MarketCategory = (MarketCategory)reader.ReadUInt16();
                        thingType.WareId = reader.ReadUInt16();
                        thingType.MarketShowAs = reader.ReadUInt16();

                        ushort nameLength = reader.ReadUInt16();
                        byte[] buffer = reader.ReadBytes(nameLength);
                        thingType.MarketName = Encoding.Default.GetString(buffer, 0, buffer.Length);
                        thingType.MarketRestrictVocation = reader.ReadUInt16();
                        thingType.MarketRestrictLevel = reader.ReadUInt16();
                        break;

                    case ThingTypeAttribute.DefaultAction:
                        thingType.HasAction = true;
                        thingType.DefaultAction = (DefaultAction)reader.ReadUInt16();
                        break;

                    case ThingTypeAttribute.Wrap:
                        thingType.Wrappable = true;
                        break;

                    case ThingTypeAttribute.Unwrap:
                        thingType.Unwrappable = true;
                        break;

                    case ThingTypeAttribute.TopEffect:
                        thingType.IsTopEffect = true;
                        break;

                    case ThingTypeAttribute.Usable:
                        thingType.Usable = true;
                        break;

                    default:
                        throw new Exception(string.Format("Error while parsing, unknown flag 0x{0:X} at object id {1}, category {2}.", attribute, thingType.ClientId, thingType.Category));
                }
            }
        }


        private void ReadTexturePatterns(Appearance appearance, BinaryReader reader)
        {
            byte groupCount = 1;
            var category = appearance.ThingCategory;

            if (useFrameGroups && category == ThingCategory.Outfit)
            {
                groupCount = reader.ReadByte();
            }

            for (int i = 0; i < groupCount; ++i)
            {
                var groupType = FrameGroupType.Default;
                if (useFrameGroups && category == ThingCategory.Outfit)
                {
                    groupType = (FrameGroupType)reader.ReadByte();
                }

                var frameGroup = new Proto.Appearances.FrameGroup();
                frameGroup.SpriteInfo = new Proto.Appearances.SpriteInfo();
                var spriteInfo = frameGroup.SpriteInfo;
                appearance.SpriteTileWidth = reader.ReadByte();
                appearance.SpriteTileHeight = reader.ReadByte();

                if (appearance.SpriteTileWidth > 1 || appearance.SpriteTileHeight > 1)
                {
                    appearance.ExactSize = reader.ReadByte();
                }
                else
                {
                    appearance.ExactSize = Sprite.DefaultSize;
                }

                spriteInfo.Layers = reader.ReadByte();
                spriteInfo.PatternWidth = reader.ReadByte();
                spriteInfo.PatternHeight = reader.ReadByte();
                spriteInfo.PatternDepth = usePatternZ ? reader.ReadByte() : (byte)1;
                var frameCount = reader.ReadByte(); // Amount of frames, same as length of spriteInfo.SpritePhase

                if (useFrameDurations && frameCount > 1)
                {
                    spriteInfo.Animation = new Proto.Appearances.SpriteAnimation();
                    var mode = (AnimationMode)reader.ReadByte();

                    spriteInfo.Animation.Synchronized = mode == AnimationMode.Synchronous;
                    spriteInfo.Animation.LoopCount = (uint)reader.ReadInt32();
                    spriteInfo.Animation.DefaultStartPhase = (uint)reader.ReadSByte();

                    for (int frame = 0; frame < frameCount; ++frame)
                    {
                        var phase = new Proto.Appearances.SpritePhase();
                        phase.DurationMin = reader.ReadUInt32();
                        phase.DurationMax = reader.ReadUInt32();
                        spriteInfo.Animation.SpritePhase.Add(phase);
                    }
                }

                var patternSize = spriteInfo.PatternWidth * spriteInfo.PatternHeight * spriteInfo.PatternDepth;
                int totalSprites = (int)(appearance.SpriteTileWidth * appearance.SpriteTileHeight * patternSize * frameCount * spriteInfo.Layers);

                if (totalSprites > 4096)
                {
                    throw new Exception("A ThingType has more than 4096 sprites.");
                }

                if (useExtended)
                {
                    for (int index = 0; index < totalSprites; index++)
                    {
                        uint spriteId = reader.ReadUInt32();
                        spriteInfo.SpriteId.Add(spriteId);
                    }
                }
                else
                {
                    for (int index = 0; index < totalSprites; index++)
                    {
                        uint spriteId = reader.ReadUInt16();
                        spriteInfo.SpriteId.Add(spriteId);
                    }
                }

                appearance.Data.FrameGroup.Add(frameGroup);
            }
        }


        private void ReadTexturePatterns(ThingType thingType, BinaryReader reader)
        {
            byte groupCount = 1;
            if (useFrameGroups && thingType.Category == ThingCategory.Outfit)
            {
                groupCount = reader.ReadByte();
            }

            for (int i = 0; i < groupCount; ++i)
            {
                var groupType = FrameGroupType.Default;
                if (useFrameGroups && thingType.Category == ThingCategory.Outfit)
                {
                    groupType = (FrameGroupType)reader.ReadByte();
                }

                var frameGroup = new FrameGroup();

                frameGroup.Width = reader.ReadByte();
                frameGroup.Height = reader.ReadByte();

                if (frameGroup.Width > 1 || frameGroup.Height > 1)
                {
                    frameGroup.ExactSize = reader.ReadByte();
                }
                else
                {
                    frameGroup.ExactSize = Sprite.DefaultSize;
                }

                frameGroup.Layers = reader.ReadByte();
                frameGroup.PatternX = reader.ReadByte();
                frameGroup.PatternY = reader.ReadByte();
                frameGroup.PatternZ = usePatternZ ? reader.ReadByte() : (byte)1;
                frameGroup.Frames = reader.ReadByte();

                if (useFrameDurations && frameGroup.Frames > 1)
                {
                    frameGroup.IsAnimation = true;
                    frameGroup.AnimationMode = (AnimationMode)reader.ReadByte();
                    frameGroup.LoopCount = reader.ReadInt32();
                    frameGroup.StartFrame = reader.ReadSByte();
                    frameGroup.FrameDurations = new FrameDuration[frameGroup.Frames];

                    for (int frame = 0; frame < frameGroup.Frames; ++frame)
                    {
                        uint minimum = reader.ReadUInt32();
                        uint maximum = reader.ReadUInt32();
                        frameGroup.FrameDurations[frame] = new FrameDuration(minimum, maximum);
                    }

                }

                int spriteCount = frameGroup.GetTotalSprites();
                if (spriteCount > 4096)
                {
                    throw new Exception("A ThingType has more than 4096 sprites.");
                }

                frameGroup.SpriteIDs = new uint[spriteCount];

                if (useExtended)
                {
                    for (int index = 0; index < spriteCount; index++)
                    {
                        frameGroup.SpriteIDs[index] = reader.ReadUInt32();
                    }
                }
                else
                {
                    for (int index = 0; index < spriteCount; index++)
                    {
                        frameGroup.SpriteIDs[index] = reader.ReadUInt16();
                    }
                }

                thingType.SetFrameGroup(groupType, frameGroup);
            }
        }


        private void WriteProperties(Appearance appearance, DatFormat format, BinaryDataWriter writer)
        {
            var dataFlags = appearance.Data.Flags;
            if (appearance.ThingCategory == ThingCategory.Item)
            {
                if (dataFlags.Fullbank)
                {
                    writer.WriteByte(ThingTypeAttribute.Ground);
                    writer.WriteU16((ushort)dataFlags.Bank.Waypoints);
                }
                if (dataFlags.Clip)
                {
                    writer.WriteByte(ThingTypeAttribute.GroundBorder);
                }
                if (dataFlags.Bottom)
                {
                    writer.WriteByte(ThingTypeAttribute.OnBottom);
                }
                if (dataFlags.Top)
                {
                    writer.WriteByte(ThingTypeAttribute.OnTop);
                }

                if (dataFlags.Container)
                {
                    writer.WriteByte(ThingTypeAttribute.Container);
                }

                if (dataFlags.Cumulative)
                {
                    writer.WriteByte(ThingTypeAttribute.Stackable);
                }

                if (dataFlags.Forceuse)
                {
                    writer.WriteByte(ThingTypeAttribute.ForceUse);
                }

                if (dataFlags.Multiuse)
                {
                    writer.WriteByte(ThingTypeAttribute.MultiUse);
                }

                if (dataFlags.Write.HasMaxTextLength)
                {
                    writer.WriteByte(ThingTypeAttribute.Writable);
                    writer.WriteU16((ushort)dataFlags.Write.MaxTextLength);
                }

                if (dataFlags.WriteOnce.HasMaxTextLengthOnce)
                {
                    writer.WriteByte(ThingTypeAttribute.WritableOnce);
                    writer.WriteU16((ushort)dataFlags.WriteOnce.MaxTextLengthOnce);
                }

                if (dataFlags.Liquidcontainer)
                {
                    writer.WriteByte(ThingTypeAttribute.FluidContainer);
                }

                if (dataFlags.Liquidpool)
                {
                    writer.WriteByte(ThingTypeAttribute.LiquidPool);
                }

                if (dataFlags.Unpass)
                {
                    writer.WriteByte(ThingTypeAttribute.UnPass);
                }

                if (dataFlags.Unmove)
                {
                    writer.WriteByte(ThingTypeAttribute.UnMove);
                }

                if (dataFlags.Unsight)
                {
                    writer.WriteByte(ThingTypeAttribute.UnSight);
                }

                if (dataFlags.Avoid)
                {
                    writer.WriteByte(ThingTypeAttribute.Avoid);
                }

                if (dataFlags.NoMovementAnimation)
                {
                    writer.WriteByte(ThingTypeAttribute.NoMoveAnimation);
                }

                if (dataFlags.Take)
                {
                    writer.WriteByte(ThingTypeAttribute.Take);
                }

                if (dataFlags.Hang)
                {
                    writer.WriteByte(ThingTypeAttribute.Hang);
                }

                if (dataFlags.HasHang && dataFlags.Hook.HasDirection)
                {
                    if (dataFlags.Hook.Direction == Proto.Shared.HOOK_TYPE.East)
                    {
                        writer.WriteByte(ThingTypeAttribute.HookEast);
                    }
                    else if (dataFlags.Hook.Direction == Proto.Shared.HOOK_TYPE.South)
                    {
                        writer.WriteByte(ThingTypeAttribute.HookSouth);
                    }
                    else
                    {
                        throw new InvalidDataException($"Unknown hook type: {dataFlags.Hook.Direction}");
                    }
                }

                if (dataFlags.Rotate)
                {
                    writer.WriteByte(ThingTypeAttribute.Rotatable);
                }

                if (dataFlags.DontHide)
                {
                    writer.WriteByte(ThingTypeAttribute.DontHide);
                }

                if (dataFlags.Translucent)
                {
                    writer.WriteByte(ThingTypeAttribute.Translucent);
                }

                if (dataFlags.Height.HasElevation)
                {
                    writer.WriteByte(ThingTypeAttribute.Elevation);
                    writer.WriteU16((ushort)dataFlags.Height.Elevation);

                }
                if (dataFlags.LyingObject)
                {
                    writer.WriteByte(ThingTypeAttribute.LyingCorpse);
                }

                if (dataFlags.Automap.HasColor)
                {
                    writer.WriteByte(ThingTypeAttribute.Automap);
                    writer.Write(dataFlags.Automap.Color);
                }

                if (dataFlags.Lenshelp.HasId)
                {
                    writer.WriteByte(ThingTypeAttribute.LensHelp);
                    writer.WriteU16((ushort)dataFlags.Lenshelp.Id);
                }

                if (dataFlags.Fullbank)
                {
                    writer.WriteByte(ThingTypeAttribute.FullGround);
                }

                if (dataFlags.IgnoreLook)
                {
                    writer.WriteByte(ThingTypeAttribute.IgnoreLook);
                }

                if (dataFlags.Clothes.HasSlot)
                {
                    writer.WriteByte(ThingTypeAttribute.Cloth);
                    writer.Write((ushort)dataFlags.Clothes.Slot);
                }

                if (dataFlags.Market.HasCategory)
                {
                    writer.WriteByte(ThingTypeAttribute.Market);
                    writer.WriteU16((ushort)dataFlags.Market.Category);
                    writer.WriteU16((ushort)dataFlags.Market.TradeAsObjectId);
                    writer.WriteU16((ushort)dataFlags.Market.ShowAsObjectId);
                    writer.WriteU16((ushort)appearance.MarketName.Length);
                    writer.WriteBytes(Encoding.Default.GetBytes(appearance.MarketName));
                    writer.WriteU16(appearance.MarketRestrictVocation);
                    writer.WriteU16(appearance.MarketRestrictLevel);
                }

                if (dataFlags.DefaultAction.HasAction)
                {
                    writer.WriteByte(ThingTypeAttribute.DefaultAction);
                    writer.WriteU16((ushort)dataFlags.DefaultAction.Action);
                }

                if (format >= DatFormat.Format_1092)
                {
                    if (dataFlags.Wrap)
                    {
                        writer.WriteByte(ThingTypeAttribute.Wrap);
                    }

                    if (dataFlags.Unwrap)
                    {
                        writer.WriteByte(ThingTypeAttribute.Unwrap);
                    }
                }

                if (dataFlags.Usable)
                {
                    writer.WriteByte(ThingTypeAttribute.Usable);
                }
            }


            if (dataFlags.AnimateAlways)
            {
                writer.WriteByte(ThingTypeAttribute.AnimateAlways);
            }

            if (dataFlags.Light.HasBrightness)
            {
                writer.WriteByte(ThingTypeAttribute.Light);
                writer.WriteU16((ushort)dataFlags.Light.Brightness);
                writer.WriteU16((ushort)dataFlags.Light.Color);
            }

            if (dataFlags.Shift.HasX)
            {
                writer.WriteByte(ThingTypeAttribute.Displacement);
                writer.Write(dataFlags.Shift.X);
                writer.Write(dataFlags.Shift.Y);
            }

            if (dataFlags.Topeffect && format >= DatFormat.Format_1093)
            {
                writer.WriteByte(ThingTypeAttribute.TopEffect);
            }

            writer.WriteByte(ThingTypeAttribute.LastAttribute);
        }


        private void WriteProperties(ThingType thingType, DatFormat format, BinaryDataWriter writer)
        {
            if (thingType.Category == ThingCategory.Item)
            {
                switch (thingType.StackOrder)
                {
                    case StackOrder.Border:
                        writer.WriteByte(ThingTypeAttribute.GroundBorder);
                        break;
                    case StackOrder.Bottom:
                        writer.WriteByte(ThingTypeAttribute.OnBottom);
                        break;
                    case StackOrder.Top:
                        writer.WriteByte(ThingTypeAttribute.OnTop);
                        break;
                    case StackOrder.None:
                        break;
                }

                if (thingType.IsContainer)
                {
                    writer.WriteByte(ThingTypeAttribute.Container);
                }

                if (thingType.Stackable)
                {
                    writer.WriteByte(ThingTypeAttribute.Stackable);
                }

                if (thingType.ForceUse)
                {
                    writer.WriteByte(ThingTypeAttribute.ForceUse);
                }

                if (thingType.MultiUse)
                {
                    writer.WriteByte(ThingTypeAttribute.MultiUse);
                }

                if (thingType.Writable)
                {
                    writer.WriteByte(ThingTypeAttribute.Writable);
                    writer.WriteU16(thingType.MaxTextLength);
                }

                if (thingType.WritableOnce)
                {
                    writer.WriteByte(ThingTypeAttribute.WritableOnce);
                    writer.Write(thingType.MaxTextLength);
                }

                if (thingType.IsFluidContainer)
                {
                    writer.WriteByte(ThingTypeAttribute.FluidContainer);
                }

                if (thingType.IsLiquidPool)
                {
                    writer.WriteByte(ThingTypeAttribute.LiquidPool);
                }

                if (thingType.Unpassable)
                {
                    writer.WriteByte(ThingTypeAttribute.UnPass);
                }

                if (thingType.Unmovable)
                {
                    writer.WriteByte(ThingTypeAttribute.UnMove);
                }

                if (thingType.BlockMissiles)
                {
                    writer.WriteByte(ThingTypeAttribute.UnSight);
                }

                if (thingType.BlockPathfinder)
                {
                    writer.WriteByte(ThingTypeAttribute.Avoid);
                }

                if (thingType.NoMoveAnimation)
                {
                    writer.WriteByte(ThingTypeAttribute.NoMoveAnimation);
                }

                if (thingType.Pickupable)
                {
                    writer.WriteByte(ThingTypeAttribute.Take);
                }

                if (thingType.Hangable)
                {
                    writer.WriteByte(ThingTypeAttribute.Hang);
                }

                if (thingType.HookSouth)
                {
                    writer.WriteByte(ThingTypeAttribute.HookSouth);
                }

                if (thingType.HookEast)
                {
                    writer.WriteByte(ThingTypeAttribute.HookEast);
                }

                if (thingType.Rotatable)
                {
                    writer.WriteByte(ThingTypeAttribute.Rotatable);
                }

                if (thingType.DontHide)
                {
                    writer.WriteByte(ThingTypeAttribute.DontHide);
                }

                if (thingType.Translucent)
                {
                    writer.WriteByte(ThingTypeAttribute.Translucent);
                }

                if (thingType.HasElevation)
                {
                    writer.WriteByte(ThingTypeAttribute.Elevation);
                    writer.Write(thingType.Elevation);

                }
                if (thingType.LyingObject)
                {
                    writer.WriteByte(ThingTypeAttribute.LyingCorpse);
                }

                if (thingType.Minimap)
                {
                    writer.WriteByte(ThingTypeAttribute.Automap);
                    writer.Write(thingType.MinimapColor);
                }

                if (thingType.IsLensHelp)
                {
                    writer.WriteByte(ThingTypeAttribute.LensHelp);
                    writer.Write(thingType.LensHelp);
                }

                if (thingType.FullGround)
                {
                    writer.WriteByte(ThingTypeAttribute.FullGround);
                }

                if (thingType.IgnoreLook)
                {
                    writer.WriteByte(ThingTypeAttribute.IgnoreLook);
                }

                if (thingType.IsCloth)
                {
                    writer.WriteByte(ThingTypeAttribute.Cloth);
                    writer.Write((ushort)thingType.ClothSlot);
                }

                if (thingType.IsMarketItem)
                {
                    writer.WriteByte(ThingTypeAttribute.Market);
                    writer.WriteU16((ushort)thingType.MarketCategory);
                    writer.Write(thingType.WareId);
                    writer.Write(thingType.MarketShowAs);
                    writer.WriteU16((ushort)thingType.MarketName.Length);
                    writer.WriteBytes(Encoding.Default.GetBytes(thingType.MarketName));
                    writer.Write(thingType.MarketRestrictVocation);
                    writer.Write(thingType.MarketRestrictLevel);
                }

                if (thingType.HasAction)
                {
                    writer.WriteByte(ThingTypeAttribute.DefaultAction);
                    writer.WriteU16((ushort)thingType.DefaultAction);
                }

                if (format >= DatFormat.Format_1092)
                {
                    if (thingType.Wrappable)
                    {
                        writer.WriteByte(ThingTypeAttribute.Wrap);
                    }

                    if (thingType.Unwrappable)
                    {
                        writer.WriteByte(ThingTypeAttribute.Unwrap);
                    }
                }

                if (thingType.Usable)
                {
                    writer.WriteByte(ThingTypeAttribute.Usable);
                }
            }


            if (thingType.AnimateAlways)
            {
                writer.WriteByte(ThingTypeAttribute.AnimateAlways);
            }

            if (thingType.HasLight)
            {
                writer.WriteByte(ThingTypeAttribute.Light);
                writer.Write(thingType.LightLevel);
                writer.Write(thingType.LightColor);
            }

            if (thingType.HasOffset)
            {
                writer.WriteByte(ThingTypeAttribute.Displacement);
                writer.Write(thingType.OffsetX);
                writer.Write(thingType.OffsetY);
            }

            if (thingType.IsTopEffect && format >= DatFormat.Format_1093)
            {
                writer.WriteByte(ThingTypeAttribute.TopEffect);
            }

            writer.WriteByte(ThingTypeAttribute.LastAttribute);
        }


        private static void WriteTexturePatterns(Appearance appearance, ClientFeatures features, BinaryDataWriter writer)
        {
            bool usePatternZ = features.HasFlag(ClientFeatures.PatternZ);
            bool useExtended = features.HasFlag(ClientFeatures.Extended);
            bool useFrameDurations = features.HasFlag(ClientFeatures.FrameDurations);
            bool useFrameGroups = features.HasFlag(ClientFeatures.FrameGroups);
            int groupCount = 1;

            if (useFrameGroups && appearance.ThingCategory == ThingCategory.Outfit)
            {
                groupCount = appearance.Data.FrameGroup.Count;
                writer.WriteByte((byte)groupCount);
            }

            for (byte frameGroupIndex = 0; frameGroupIndex < groupCount; ++frameGroupIndex)
            {
                // write frame group type.
                if (useFrameGroups && appearance.ThingCategory == ThingCategory.Outfit)
                {
                    writer.WriteByte(frameGroupIndex);
                }

                var frameGroup = appearance.Data.FrameGroup[frameGroupIndex];
                var spriteInfo = frameGroup.SpriteInfo;

                var width = appearance.SpriteTileWidth;
                var height = appearance.SpriteTileHeight;

                writer.WriteByte(width);
                writer.WriteByte(height);

                // write exact size
                if (width > 1 || height > 1)
                {
                    writer.Write(appearance.ExactSize);
                }

                writer.WriteByte((byte)spriteInfo.Layers);
                writer.WriteByte((byte)spriteInfo.PatternWidth);
                writer.WriteByte((byte)spriteInfo.PatternHeight);

                if (usePatternZ)
                {
                    writer.WriteByte((byte)spriteInfo.PatternDepth);
                }

                var frames = (byte)spriteInfo.Animation.SpritePhase.Count;
                writer.WriteByte(frames);

                if (useFrameDurations && frames > 1)
                {
                    byte animationMode = (byte)(spriteInfo.Animation.Synchronized ? AnimationMode.Synchronous : AnimationMode.Asynchronous);
                    writer.WriteByte(animationMode);
                    writer.Write((int)spriteInfo.Animation.LoopCount);
                    writer.WriteSignedByte((sbyte)spriteInfo.Animation.DefaultStartPhase);

                    var phases = spriteInfo.Animation.SpritePhase.Count;
                    for (int i = 0; i < phases; i++)
                    {
                        writer.WriteU32((uint)spriteInfo.Animation.SpritePhase[i].DurationMin);
                        writer.WriteU32((uint)spriteInfo.Animation.SpritePhase[i].DurationMax);
                    }
                }

                for (int i = 0; i < spriteInfo.SpriteId.Count; i++)
                {
                    // write sprite index
                    if (useExtended)
                    {
                        writer.WriteU32(spriteInfo.SpriteId[i]);
                    }
                    else
                    {
                        writer.WriteU16((ushort)spriteInfo.SpriteId[i]);
                    }
                }
            }
        }

        private static void WriteTexturePatterns(ThingType thing, ClientFeatures features, BinaryDataWriter writer)
        {
            bool usePatternZ = features.HasFlag(ClientFeatures.PatternZ);
            bool useExtended = features.HasFlag(ClientFeatures.Extended);
            bool useFrameDurations = features.HasFlag(ClientFeatures.FrameDurations);
            bool useFrameGroups = features.HasFlag(ClientFeatures.FrameGroups);
            int groupCount = 1;

            if (useFrameGroups && thing.Category == ThingCategory.Outfit)
            {
                groupCount = thing.FrameGroupCount;
                writer.WriteByte((byte)groupCount);
            }

            for (byte frameGroupIndex = 0; frameGroupIndex < groupCount; ++frameGroupIndex)
            {
                // write frame group type.
                if (useFrameGroups && thing.Category == ThingCategory.Outfit)
                {
                    writer.WriteByte(frameGroupIndex);
                }

                FrameGroup group = thing.GetFrameGroup((FrameGroupType)frameGroupIndex);

                writer.WriteByte(group.Width);
                writer.WriteByte(group.Height);

                // write exact size
                if (group.Width > 1 || group.Height > 1)
                {
                    writer.Write(group.ExactSize);
                }

                writer.WriteByte(group.Layers);
                writer.WriteByte(group.PatternX);
                writer.WriteByte(group.PatternY);

                if (usePatternZ)
                {
                    writer.WriteByte(group.PatternZ);
                }

                writer.WriteByte(group.Frames);

                if (useFrameDurations && group.Frames > 1)
                {
                    writer.WriteByte((byte)group.AnimationMode);
                    writer.Write(group.LoopCount);
                    writer.WriteSignedByte(group.StartFrame);

                    for (int i = 0; i < group.FrameDurations.Length; i++)
                    {
                        writer.WriteU32((uint)group.FrameDurations[i].Minimum); // write minimum duration
                        writer.WriteU32((uint)group.FrameDurations[i].Maximum); // write maximum duration
                    }
                }

                for (int i = 0; i < group.SpriteIDs.Length; i++)
                {
                    // write sprite index
                    if (useExtended)
                    {
                        writer.WriteU32(group.SpriteIDs[i]);
                    }
                    else
                    {
                        writer.WriteU16((ushort)group.SpriteIDs[i]);
                    }
                }
            }
        }

        public uint LastItemTypeClientId
        {
            get
            {
                return (uint)(ItemTypeCount + 100);
            }
        }
    }
}
