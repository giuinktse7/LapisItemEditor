
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

        Article,


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

        public OtbItem? ReadOtbItemNode(OtbReader reader)
        {
            byte start = reader.NextU8();
            Debug.Assert(start == (byte)NodeType.Start);

            byte itemGroup = reader.NextU8();
            ItemTypeFlag flags = (ItemTypeFlag)reader.NextU32();

            var otbItem = new OtbItem();
            otbItem.Flags = flags;

            while (reader.PeekByte() != (byte)NodeType.End)
            {
                byte attributeType = reader.NextU8();
                ushort attributeSize = reader.NextU16();

                switch ((OtbItemAttribute)attributeType)
                {
                    case OtbItemAttribute.ServerId:
                        Debug.Assert(attributeSize == 2);
                        otbItem.ServerId = reader.NextU16();
                        break;

                    case OtbItemAttribute.ClientId:
                        Debug.Assert(attributeSize == 2);
                        otbItem.ClientId = reader.NextU16();
                        break;

                    case OtbItemAttribute.Speed:
                        Debug.Assert(attributeSize == 2);
                        otbItem.Speed = reader.NextU16();
                        break;

                    case OtbItemAttribute.MinimapColor:
                        Debug.Assert(attributeSize == 2);
                        otbItem.MinimapColor = reader.NextU16();
                        break;

                    case OtbItemAttribute.Light2:
                        Debug.Assert(attributeSize == 4);
                        otbItem.LightLevel = reader.NextU16();
                        otbItem.LightColor = reader.NextU16();
                        break;

                    case OtbItemAttribute.MaxTextLength:
                        Debug.Assert(attributeSize == 2);
                        otbItem.MaxTextLen = reader.NextU16();
                        break;

                    case OtbItemAttribute.MaxTextLengthOnce:
                        Debug.Assert(attributeSize == 2);
                        otbItem.MaxTextLenOnce = reader.NextU16();
                        break;

                    case OtbItemAttribute.Name:
                        otbItem.Name = reader.NextString(attributeSize);
                        break;

                    case OtbItemAttribute.Article:
                        otbItem.Article = reader.NextString(attributeSize);
                        break;

                    case OtbItemAttribute.Description:
                        otbItem.Description = reader.NextString(attributeSize);
                        break;

                    case OtbItemAttribute.TopOrder:
                        Debug.Assert(attributeSize == 1);
                        otbItem.StackOrder = (StackOrder)reader.NextU8();
                        break;

                    case OtbItemAttribute.SpriteHash:
                        otbItem.SpriteHash = reader.NextBytes(attributeSize);
                        break;

                    case OtbItemAttribute.WareId:
                        Debug.Assert(attributeSize == 2);
                        otbItem.WareId = reader.NextU16();
                        break;

                    case OtbItemAttribute.UpgradeClassification:
                        Debug.Assert(attributeSize == 1);
                        otbItem.UpgradeClassification = reader.NextU8();
                        break;

                    default:
                        // Skip unknown attributes
                        reader.SkipBytes(attributeSize);
                        break;
                }
            }

            byte endToken = reader.NextU8();
            Debug.Assert(endToken == (byte)NodeType.End);


            // Ignore nodes with clientId 0, these are probably items from older versions that have since been removed.
            if (otbItem.ClientId == 0)
            {
                return null;
            }

            otbItem.ServerItemGroup = (ServerItemGroup)itemGroup;

            switch (otbItem.ServerItemGroup)
            {
                case ServerItemGroup.Container:
                    otbItem.ItemType = ItemType_t.Container;
                    break;
                case ServerItemGroup.Door:
                    otbItem.ItemType = ItemType_t.Door;
                    break;
                case ServerItemGroup.MagicField:
                    otbItem.ItemType = ItemType_t.MagicField;
                    break;
                case ServerItemGroup.Teleport:
                    otbItem.ItemType = ItemType_t.Teleport;
                    break;
                case ServerItemGroup.ShowOffSocket:
                    otbItem.ItemType = ItemType_t.ShowOffSocket;
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

            return otbItem;
        }

        private void ReadNodes(OtbReader reader, GameData gameData, bool ignoreAttributes)
        {
            HashSet<uint> serverIds = new HashSet<uint>();
            uint maxClientId = 0;
            uint maxServerId = 0;

            while (reader.PeekByte() != (byte)NodeType.End)
            {
                var otbItem = ReadOtbItemNode(reader);
                if (otbItem == null)
                {
                    continue;
                }

                var serverId = otbItem.ServerId;
                var clientId = otbItem.ClientId;

                // Ignore nodes with clientId 0, these are probably items from older versions that have since been removed.
                maxServerId = Math.Max(maxServerId, serverId);
                maxClientId = Math.Max(maxClientId, clientId);

                serverIds.Add(serverId);

                ServerIdToClientId.Add(serverId, clientId);

                Appearance appearance = gameData.GetOrCreateItemTypeByClientId(clientId);
                if (appearance.Data.Flags == null)
                {
                    appearance.Data.Flags = new Proto.Appearances.AppearanceFlags();
                }

                appearance.otbItem = otbItem;
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

                    if (appearance.otbItem == null)
                    {
                        appearance.otbItem = new OtbItem();
                    }

                    appearance.otbItem.ServerId = (ushort)serverId;
                    // appearance.SyncOtbWithTibia();

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

                var otbItem = appearance.otbItem;

                if (otbItem == null)
                {
                    Trace.WriteLine($"There is no otb item for server ID {serverId} (client ID: {clientId})");
                    continue;
                }

                ServerItemGroup itemGroup = otbItem.ServerItemGroup;

                // Write ItemType start node
                writer.StartNode(itemGroup);

                // Update OTB flags based on Appearance data
                // appearance.UpdateOtbFlags();

                // ItemTypeFlag flags = appearance.GetOtbFlags();
                ItemTypeFlag flags = otbItem.Flags;

                writer.WriteU32((uint)flags);

                writer.WriteAttributeType(OtbItemAttribute.ServerId, 2);
                writer.WriteU16((ushort)serverId);

                if (itemGroup != ServerItemGroup.Deprecated)
                {
                    writer.WriteAttributeType(OtbItemAttribute.ClientId, 2);
                    writer.WriteU16((ushort)clientId);

                    // TODO Maybe move this? Maybe to a "Generate sprite hashes" button
                    if (appearance.SpriteHash == null && appearance.Data.FrameGroup.Count != 0)
                    {
                        gameData.ComputeSpriteHash(appearance);
                    }

                    if (otbItem.HasDescription)
                    {
                        var bytes = otbItem.Description.ToCharArray();
                        writer.WriteAttributeType(OtbItemAttribute.Description, (ushort)bytes.Length);
                        writer.WriteBytesWithoutSizeHint(bytes);
                    }

                    if (otbItem.HasSpriteHash)
                    {
                        writer.WriteAttributeType(OtbItemAttribute.SpriteHash, (ushort)otbItem.SpriteHash.Length);
                        writer.WriteBytesWithoutSizeHint(otbItem.SpriteHash);
                    }

                    if (otbItem.HasMinimapColor)
                    {
                        writer.WriteAttributeType(OtbItemAttribute.MinimapColor, 2);
                        writer.WriteU16((ushort)otbItem.MinimapColor);
                    }

                    if (otbItem.HasMaxTextLen)
                    {
                        writer.WriteAttributeType(OtbItemAttribute.MaxTextLength, 2);
                        writer.WriteU16((ushort)otbItem.MaxTextLen);
                    }

                    if (otbItem.HasMaxTextLenOnce)
                    {
                        writer.WriteAttributeType(OtbItemAttribute.MaxTextLengthOnce, 2);
                        writer.WriteU16((ushort)otbItem.MaxTextLenOnce);
                    }

                    if (otbItem.HasLightLevel)
                    {
                        writer.WriteAttributeType(OtbItemAttribute.Light2, 4);
                        writer.WriteU16((ushort)otbItem.LightLevel);
                        writer.WriteU16(otbItem.LightColor ?? 0);
                    }

                    if (otbItem.HasSpeed)
                    {
                        writer.WriteAttributeType(OtbItemAttribute.Speed, 2);
                        writer.WriteU16((ushort)otbItem.Speed);
                    }

                    if (otbItem.StackOrder == StackOrder.Border)
                    {
                        writer.WriteAttributeType(OtbItemAttribute.TopOrder, 1);
                        writer.WriteU8((byte)StackOrder.Border);
                    }
                    else if (otbItem.StackOrder == StackOrder.Bottom)
                    {
                        writer.WriteAttributeType(OtbItemAttribute.TopOrder, 1);
                        writer.WriteU8((byte)StackOrder.Bottom);
                    }
                    else if (otbItem.StackOrder == StackOrder.Top)
                    {
                        writer.WriteAttributeType(OtbItemAttribute.TopOrder, 1);
                        writer.WriteU8((byte)StackOrder.Top);
                    }


                    if (otbItem.HasWareId)
                    {
                        writer.WriteAttributeType(OtbItemAttribute.WareId, 2);
                        writer.WriteU16((ushort)otbItem.WareId);
                    }

                    if (otbItem.HasUpgradeClassification)
                    {
                        writer.WriteAttributeType(OtbItemAttribute.UpgradeClassification, 1);
                        writer.WriteU8((byte)otbItem.UpgradeClassification);
                    }

                    if (otbItem.HasName)
                    {
                        var bytes = otbItem.Name.ToCharArray();
                        writer.WriteAttributeType(OtbItemAttribute.Name, (ushort)bytes.Length);
                        writer.WriteBytesWithoutSizeHint(bytes);
                    }

                    if (otbItem.HasArticle)
                    {
                        var bytes = otbItem.Article.ToCharArray();
                        writer.WriteAttributeType(OtbItemAttribute.Article, (ushort)bytes.Length);
                        writer.WriteBytesWithoutSizeHint(bytes);
                    }
                }

                // Close the ItemType node
                writer.CloseNode();
            }
        }
    }
}