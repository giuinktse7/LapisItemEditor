
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using Backend.Otb;
using System.Linq;

namespace Backend
{
    using Proto = Tibia.Protobuf;

    public enum OtbItemAttribute
    {
        First = 0x10,
        ServerId = First,
        ClientId,
        Name,
        Description,
        Speed,
        Slot,
        ContainerSize, // Other software calls this MaxItems
        Weight,
        Weapon,
        Ammunition,
        Armor,
        MagicLevel,
        MagicFieldType,
        Writable,
        RotateTo,
        Decay,
        SpriteHash,
        MinimapColor,
        MaxTextLength,
        MaxTextLengthOnce,
        Light,

        //1-byte aligned
        Decay2, //deprecated
        Weapon2, //deprecated
        Ammunition2, //deprecated
        Armor2, //deprecated
        Writable2, //deprecated
        Light2, // TFS uses this one (Light2=42) for light data, not the other one (Light=36) (last checked 2021-09-24)
        TopOrder,
        Writable3, //deprecated

        WareId,
        UpgradeClassification,


        Last
    };


    enum NodeType : byte
    {
        Escape = 0xfd,
        Start = 0xfe,
        End = 0xff,
        OTBM_RootV1 = 0x01
    }

    public class OtbVersion
    {
        public uint MajorVersion { get; set; }
        public uint MinorVersion { get; set; }
        public uint BuildNumber { get; set; }
    }

    public class OtbData
    {
        public Dictionary<uint, uint> ServerIdToClientId { get; } = new Dictionary<uint, uint>();

        /// <summary>
        /// Size of OTB header
        /// <para>4 bytes majorVersion</para>
        /// <para>4 bytes minorVersion</para>
        /// <para>4 bytes buildNumber</para>
        /// <para>128 bytes CSDVersion (unused)</para>
        /// 4 + 4 + 4 + 128 = 140
        /// </summary>
        const ushort HeaderLength = 140;

        private OtbVersion version = new OtbVersion();
        public OtbVersion Version { get => version; set { version = value; } }

        public int? ClientVersion { get; set; }

        public uint LastClientId { get; private set; }
        public uint LastServerId { get; private set; }

        /// <summary>Returns a copy of the array, but without trailing zeros</summary>
        public static byte[] RemoveTrailingZeros(byte[] array)
        {
            int lastIndex = Array.FindLastIndex(array, b => b != 0);

            Array.Resize(ref array, lastIndex + 1);

            return array;
        }

        private OtbData(uint majorVersion, uint minorVersion, uint buildNumber)
        {
            version.MajorVersion = majorVersion;
            version.MinorVersion = minorVersion;
            version.BuildNumber = buildNumber;
        }

        public static OtbData Load(string path, GameData gameData, bool ignoreAttributes = false)
        {
            if (!File.Exists(path))
            {
                throw new FileNotFoundException($"File not found: {path}", path);
            }

            var buffer = File.ReadAllBytes(path);
            var reader = new OtbReader(buffer);
            // Read version, always 0
            uint value = reader.NextU32();
            if (value != 0)
            {
                throw new InvalidDataException("Expected 4 empty bytes.");
            }

            // Read root node start
            byte start = reader.NextU8();
            if (start != (byte)NodeType.Start)
            {
                throw new InvalidDataException("Expected StartNode (0xfe).");
            }

            var rootNodeType = reader.NextU8();  // Root node type
            if (rootNodeType != (byte)ServerItemGroup.None)
            {
                throw new InvalidDataException($"Invalid root node type, expected {(byte)ServerItemGroup.None}.");
            }
            reader.NextU32(); // Root flags, unused

            // Root Header Version
            byte rootAttribute = reader.NextU8();
            if (rootAttribute != (byte)NodeType.OTBM_RootV1)
            {
                throw new InvalidDataException("Unsupported RootHeaderVersion.");
            }

            ushort headerLength = reader.NextU16();
            if (headerLength != HeaderLength)
            {
                throw new InvalidDataException($"Invalid header size hint (expected {HeaderLength}).");
            }

            uint majorVersion = reader.NextU32(); // major, file version
            uint minorVersion = reader.NextU32(); // minor, client version
            uint buildNumber = reader.NextU32();  // build number, revision

            byte[] otbDescriptionBuffer = RemoveTrailingZeros(reader.NextBytes(128));
            string otbDescription = System.Text.Encoding.Default.GetString(otbDescriptionBuffer);
            // Use this instead if you want to skip reading OTB description string
            // reader.SkipBytes(128);

            var otbData = new OtbData(majorVersion, minorVersion, buildNumber);

            int clientVersion = ParseClientVersion(otbDescription);
            if (clientVersion != -1)
            {
                otbData.ClientVersion = clientVersion;
            }

            otbData.ReadNodes(reader, gameData, ignoreAttributes);
            return otbData;
        }

        private static int ParseClientVersion(string otbDescription)
        {
            var parts = otbDescription.Split("-");
            if (parts.Length == 1) return -1;

            var clientVersionParts = parts[1].Split(".");

            if (UInt32.TryParse(clientVersionParts[0], out var majorVersion))
            {
                if (UInt32.TryParse(clientVersionParts[1], out var minorVersion))
                {
                    return (int)(majorVersion * 100 + minorVersion);
                }
            }

            return -1;
        }

        private void ReadNodes(OtbReader reader, GameData gameData, bool ignoreAttributes)
        {
            HashSet<uint> serverIds = new HashSet<uint>();
            uint maxClientId = 0;
            uint maxServerId = 0;

            while (reader.PeekByte() != (byte)NodeType.End)
            {
                byte start = reader.NextU8();
                Debug.Assert(start == (byte)NodeType.Start);

                byte itemGroup = reader.NextU8();
                ItemTypeFlag flags = (ItemTypeFlag)reader.NextU32();

                ushort? serverId = null;
                ushort? clientId = null;
                ushort? lightLevel = null;
                ushort? lightColor = null;
                ushort? wareId = null;
                byte? upgradeClassification = null;
                string? name = null;
                string? description = null;
                ushort? maxTextLen = null;
                ushort? maxTextLenOnce = null;
                ushort? speed = null;
                byte? stackOrder = null;
                ushort? minimapColor = null;
                byte[]? spriteHash = null;

                while (reader.PeekByte() != (byte)NodeType.End)
                {
                    byte attributeType = reader.NextU8();
                    ushort attributeSize = reader.NextU16();

                    switch ((OtbItemAttribute)attributeType)
                    {
                        case OtbItemAttribute.ServerId:
                            Debug.Assert(attributeSize == 2);
                            serverId = reader.NextU16();
                            break;

                        case OtbItemAttribute.ClientId:
                            Debug.Assert(attributeSize == 2);
                            clientId = reader.NextU16();
                            break;

                        case OtbItemAttribute.Speed:
                            Debug.Assert(attributeSize == 2);
                            speed = reader.NextU16();
                            break;

                        case OtbItemAttribute.MinimapColor:
                            Debug.Assert(attributeSize == 2);
                            minimapColor = reader.NextU16();
                            break;

                        case OtbItemAttribute.Light2:
                            Debug.Assert(attributeSize == 4);
                            lightLevel = reader.NextU16();
                            lightColor = reader.NextU16();
                            break;

                        case OtbItemAttribute.MaxTextLength:
                            Debug.Assert(attributeSize == 2);
                            maxTextLen = reader.NextU16();
                            break;

                        case OtbItemAttribute.MaxTextLengthOnce:
                            Debug.Assert(attributeSize == 2);
                            maxTextLenOnce = reader.NextU16();
                            break;

                        case OtbItemAttribute.Name:
                            name = reader.NextString(attributeSize);
                            break;

                        case OtbItemAttribute.Description:
                            description = reader.NextString(attributeSize);
                            break;

                        case OtbItemAttribute.TopOrder:
                            Debug.Assert(attributeSize == 1);
                            stackOrder = reader.NextU8();
                            break;

                        case OtbItemAttribute.SpriteHash:
                            spriteHash = reader.NextBytes(attributeSize);
                            break;

                        case OtbItemAttribute.WareId:
                            Debug.Assert(attributeSize == 2);
                            wareId = reader.NextU16();
                            break;

                        case OtbItemAttribute.UpgradeClassification:
                            Debug.Assert(attributeSize == 1);
                            upgradeClassification = reader.NextU8();
                            break;

                        default:
                            // Skip unknown attributes
                            reader.SkipBytes(attributeSize);
                            break;
                    }
                }

                // Ignore nodes with clientId 0, these are probably items from older versions that have since been removed.
                if (clientId != 0 && clientId != null)
                {
                    maxServerId = Math.Max(maxServerId, (uint)serverId);
                    maxClientId = Math.Max(maxClientId, (uint)clientId);

                    serverIds.Add((uint)serverId);

                    ServerIdToClientId.Add((uint)serverId, (uint)clientId);

                    Appearance appearance = gameData.GetOrCreateItemTypeByClientId((uint)clientId);
                    if (appearance.Data.Flags == null)
                    {
                        appearance.Data.Flags = new Proto.Appearances.AppearanceFlags();
                    }

                    appearance.OtbServerItemGroup = (ServerItemGroup)itemGroup;

                    switch ((ServerItemGroup)itemGroup)
                    {
                        case ServerItemGroup.Container:
                            appearance.ItemType = ItemType_t.Container;
                            break;
                        case ServerItemGroup.Door:
                            appearance.ItemType = ItemType_t.Door;
                            break;
                        case ServerItemGroup.MagicField:
                            appearance.ItemType = ItemType_t.MagicField;
                            break;
                        case ServerItemGroup.Teleport:
                            appearance.ItemType = ItemType_t.Teleport;
                            break;
                        case ServerItemGroup.ShowOffSocket:
                            appearance.ItemType = ItemType_t.ShowOffSocket;
                            break;
                        case ServerItemGroup.None:
                        case ServerItemGroup.Ground:
                        case ServerItemGroup.Splash:
                        case ServerItemGroup.Fluid:
                        case ServerItemGroup.Charges:
                        case ServerItemGroup.Deprecated:
                            break;
                        default:
                            throw new InvalidDataException("Invalid ItemTypeGroup.");
                    }

                    if (serverId != null)
                    {
                        appearance.ServerId = (uint)serverId;
                    }

                    if (!ignoreAttributes)
                    {
                        if (name != null)
                        {
                            appearance.Data.Name = name;
                        }

                        if (description != null)
                        {
                            appearance.Data.Description = description;
                        }

                        if (spriteHash != null)
                        {
                            appearance.SpriteHash = spriteHash;
                        }

                        appearance.OtbFlags = flags;
                        var dataFlags = appearance.Data.Flags;

                        if (maxTextLen != null)
                        {
                            appearance.setMaxTextLength((ushort)maxTextLen);
                        }

                        if (maxTextLenOnce != null)
                        {
                            appearance.setMaxTextLength((ushort)maxTextLenOnce);
                        }

                        if (minimapColor != null)
                        {
                            appearance.SetAutomapColor((ushort)minimapColor);
                        }


                        if (flags.HasFlag(ItemTypeFlag.HookEast))
                        {
                            appearance.SetHookDirection(Proto.Shared.HOOK_TYPE.East);
                        }

                        else if (flags.HasFlag(ItemTypeFlag.HookSouth))
                        {
                            appearance.SetHookDirection(Proto.Shared.HOOK_TYPE.South);
                        }

                        appearance.AllowDistRead = flags.HasFlag(ItemTypeFlag.AllowDistRead);

                        if (flags.HasFlag(ItemTypeFlag.BlockSolid))
                        {
                            dataFlags.Unpass = true;
                        }

                        if (flags.HasFlag(ItemTypeFlag.BlockProjectile))
                        {
                            dataFlags.Unsight = true;
                        }

                        if (flags.HasFlag(ItemTypeFlag.Hangable))
                        {
                            dataFlags.Hang = true;
                        }

                        if (flags.HasFlag(ItemTypeFlag.Rotatable))
                        {
                            dataFlags.Rotate = true;
                        }


                        if (flags.HasFlag(ItemTypeFlag.Readable))
                        {
                            appearance.Readable = true;
                        }


                        if (flags.HasFlag(ItemTypeFlag.IgnoreLook))
                        {
                            dataFlags.IgnoreLook = true;
                        }


                        if (flags.HasFlag(ItemTypeFlag.Animation))
                        {
                            appearance.IsAnimation = true;
                        }

                        if (flags.HasFlag(ItemTypeFlag.ForceUse))
                        {
                            dataFlags.Forceuse = true;
                        }

                        if (flags.HasFlag(ItemTypeFlag.BlockPathfind))
                        {
                            dataFlags.Avoid = true;
                        }

                        if (flags.HasFlag(ItemTypeFlag.MultiUse))
                        {
                            dataFlags.Usable = true;
                        }

                        if (flags.HasFlag(ItemTypeFlag.Pickupable))
                        {
                            dataFlags.Take = true;
                        }

                        if (flags.HasFlag(ItemTypeFlag.Stackable))
                        {
                            dataFlags.Cumulative = true;
                        }

                        if (flags.HasFlag(ItemTypeFlag.Ammo))
                        {
                            dataFlags.Ammo = true;
                        }

                        if (flags.HasFlag(ItemTypeFlag.Reportable))
                        {
                            dataFlags.Reportable = true;
                        }

                        dataFlags.Unmove = !flags.HasFlag(ItemTypeFlag.Movable);

                        if (speed != null)
                        {
                            if (appearance.HasBank)
                            {
                                dataFlags.Bank.Waypoints = (uint)speed;
                            }
                            else
                            {
                                dataFlags.Bank = new Proto.Appearances.AppearanceFlagBank() { Waypoints = (uint)speed };
                            }
                        }

                        if (lightLevel != null && lightColor != null)
                        {
                            appearance.setLight((ushort)lightLevel, (ushort)lightColor);
                        }

                        if (wareId != null)
                        {
                            var marketFlags = appearance.GetOrCreateMarketFlags();
                            marketFlags.TradeAsObjectId = (ushort)wareId;
                        }

                        if (upgradeClassification != null)
                        {
                            dataFlags.Upgradeclassification.UpgradeClassification = (uint)upgradeClassification;
                        }

                        if (stackOrder != null)
                        {
                            Debug.Assert(stackOrder >= 0 && stackOrder <= 4);
                            switch ((StackOrder)stackOrder)
                            {
                                case StackOrder.Border:
                                    dataFlags.Clip = true;
                                    break;
                                case StackOrder.Bottom:
                                    dataFlags.Bottom = true;
                                    break;
                                case StackOrder.Top:
                                    dataFlags.Top = true;
                                    break;
                                default: break;
                            }
                        }
                    }
                }

                byte endToken = reader.NextU8();
                Debug.Assert(endToken == (byte)NodeType.End);
            }

            this.LastClientId = maxClientId;
            this.LastServerId = maxServerId;
        }

        public void CreateMissingItems(GameData gameData)
        {
            // Just for logging
            uint prevLastServerId = LastServerId;

            uint createdCount = 0;
            uint serverIdCursor = LastServerId;
            uint? prevClientId = null;

            var orderedObjects = gameData.Objects.OrderBy(x => x.ClientId);

            foreach (var appearance in orderedObjects)
            {
                uint clientId = appearance.ClientId;

                Debug.Assert(clientId != 0);

                if (appearance.ServerId == 0)
                {
                    // The client id's sometimes contain gaps. These gaps are sometimes filled in by a later version
                    // of Tibia. This delta is here to account for those gaps (to make sure we have space in this server ID range to match those client IDs)
                    uint delta = prevClientId == null ? 1 : clientId - (uint)prevClientId;
                    serverIdCursor += delta;

                    uint serverId = serverIdCursor;

                    appearance.ServerId = serverId;

                    ServerIdToClientId.Add(serverId, clientId);
                    ++createdCount;

                }

                prevClientId = clientId;
            }

            if (createdCount != 0)
            {
                Trace.WriteLine($"Created {createdCount} new items, server IDs {prevLastServerId + 1} - {serverIdCursor - 1}");
                LastServerId = serverIdCursor - 1;
            }
            else
            {
                Trace.WriteLine($"There were no missing items.");
            }
        }

        public void Write(string path, GameData gameData)
        {
            Write(path, version.MajorVersion, version.MinorVersion, version.BuildNumber, gameData);
        }


        public void Write(string path, uint majorVersion, uint minorVersion, uint buildNumber, GameData gameData)
        {
            if (path == null)
            {
                throw new ArgumentNullException(nameof(path));
            }

            var directory = Path.GetDirectoryName(path);

            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            using var binaryWriter = new BinaryWriter(File.Open(path, FileMode.Create));
            var writer = new OtbWriter(binaryWriter);

            // Version, always 0
            writer.WriteRawU32(0);

            // Write root node
            writer.StartNode(ServerItemGroup.None);

            // Node flags, unused for root node
            writer.WriteU32(0);

            // Root Header Version
            writer.WriteOTBMRootV1();

            writer.WriteU16(HeaderLength);

            writer.WriteU32(majorVersion);
            writer.WriteU32(minorVersion);
            writer.WriteU32(buildNumber);

            var clientMajorVersion = (uint)gameData.Version.Version / 100;
            var clientMinorVersion = (uint)gameData.Version.Version % 100;

            string csdVersionString = $"OTB {majorVersion}.{minorVersion}.{buildNumber}-{clientMajorVersion}.{clientMinorVersion}";
            byte[] CSDVersion = Encoding.ASCII.GetBytes(csdVersionString);

            Array.Resize(ref CSDVersion, 128);
            writer.WriteBytesWithoutSizeHint(CSDVersion);

            WriteItemTypes(writer, gameData);

            // Close the root node
            writer.CloseNode();
        }


        private void WriteItemTypes(OtbWriter writer, GameData gameData)
        {
            foreach (var entry in ServerIdToClientId)
            {
                var serverId = entry.Key;
                var clientId = entry.Value;

                var appearance = gameData.GetItemTypeByClientId(clientId);
                if (appearance == null)
                {
                    throw new NullReferenceException($"There is no ItemType for client ID {clientId}");
                }

                ServerItemGroup itemGroup = appearance.ServerItemGroup;

                // Write ItemType start node
                writer.StartNode(itemGroup);

                // Update OTB flags based on Appearance data
                appearance.UpdateOtbFlags();

                ItemTypeFlag flags = appearance.GetOtbFlags();

                writer.WriteU32((uint)flags);

                writer.WriteAttributeType(OtbItemAttribute.ServerId, 2);
                writer.WriteU16((ushort)serverId);

                var dataFlags = appearance.Data.Flags;

                if (itemGroup != ServerItemGroup.Deprecated)
                {
                    writer.WriteAttributeType(OtbItemAttribute.ClientId, 2);
                    writer.WriteU16((ushort)clientId);

                    // TODO Maybe move this? Maybe to a "Generate sprite hashes" button
                    if (appearance.SpriteHash == null && appearance.Data.FrameGroup.Count != 0)
                    {
                        gameData.ComputeSpriteHash(appearance);
                    }

                    if (appearance.Data.HasDescription)
                    {
                        var bytes = appearance.Data.Description.ToCharArray();
                        writer.WriteAttributeType(OtbItemAttribute.Description, (ushort)bytes.Length);
                        writer.WriteBytesWithoutSizeHint(bytes);
                    }

                    if (appearance.SpriteHash != null)
                    {
                        writer.WriteAttributeType(OtbItemAttribute.SpriteHash, (ushort)appearance.SpriteHash.Length);
                        writer.WriteBytesWithoutSizeHint(appearance.SpriteHash);
                    }

                    if (appearance.HasAutomap && dataFlags.Automap.Color != 0)
                    {
                        writer.WriteAttributeType(OtbItemAttribute.MinimapColor, 2);
                        writer.WriteU16((ushort)dataFlags.Automap.Color);
                    }

                    if (appearance.HasWrite && dataFlags.Write.MaxTextLength != 0)
                    {
                        writer.WriteAttributeType(OtbItemAttribute.MaxTextLength, 2);
                        writer.WriteU16((ushort)dataFlags.Write.MaxTextLength);
                    }

                    if (appearance.HasWriteOnce && dataFlags.WriteOnce.MaxTextLengthOnce != 0)
                    {
                        writer.WriteAttributeType(OtbItemAttribute.MaxTextLengthOnce, 2);
                        writer.WriteU16((ushort)dataFlags.WriteOnce.MaxTextLengthOnce);
                    }

                    if (appearance.HasLight && dataFlags.Light.HasColor)
                    {
                        writer.WriteAttributeType(OtbItemAttribute.Light2, 4);
                        writer.WriteU16((ushort)dataFlags.Light.Brightness);
                        writer.WriteU16((ushort)dataFlags.Light.Color);
                    }

                    if (appearance.HasBank)
                    {
                        writer.WriteAttributeType(OtbItemAttribute.Speed, 2);
                        writer.WriteU16((ushort)dataFlags.Bank.Waypoints);
                    }

                    if (dataFlags.Clip)
                    {
                        writer.WriteAttributeType(OtbItemAttribute.TopOrder, 1);
                        writer.WriteU8((byte)StackOrder.Border);
                    }
                    else if (dataFlags.Bottom)
                    {
                        writer.WriteAttributeType(OtbItemAttribute.TopOrder, 1);
                        writer.WriteU8((byte)StackOrder.Bottom);
                    }
                    else if (dataFlags.Top)
                    {
                        writer.WriteAttributeType(OtbItemAttribute.TopOrder, 1);
                        writer.WriteU8((byte)StackOrder.Top);
                    }


                    if (appearance.HasMarket && dataFlags.Market.ShowAsObjectId != 0)
                    {
                        writer.WriteAttributeType(OtbItemAttribute.WareId, 2);
                        writer.WriteU16((ushort)dataFlags.Market.ShowAsObjectId);
                    }

                    if (appearance.HasUpgradeClassification && dataFlags.Upgradeclassification.UpgradeClassification != 0)
                    {
                        writer.WriteAttributeType(OtbItemAttribute.UpgradeClassification, 1);
                        writer.WriteU8((byte)dataFlags.Upgradeclassification.UpgradeClassification);
                    }

                    if (!string.IsNullOrEmpty(appearance.Data.Name))
                    {
                        var bytes = appearance.Data.Name.ToCharArray();
                        writer.WriteAttributeType(OtbItemAttribute.Name, (ushort)bytes.Length);
                        writer.WriteBytesWithoutSizeHint(bytes);
                    }

                }

                // Close the ItemType node
                writer.CloseNode();
            }
        }
    }
}