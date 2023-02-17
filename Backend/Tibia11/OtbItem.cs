using System.Diagnostics.CodeAnalysis;

namespace Backend
{
    using ItemCategory = Tibia.Protobuf.Shared.ITEM_CATEGORY;

    public class OtbItem
    {
        public OtbItem()
        {
            StackOrder = StackOrder.None;
        }

        public ServerItemGroup ServerItemGroup { get; set; }

        public ItemTypeFlag Flags { get; set; }
        public ItemType_t ItemType { get; set; }
        public StackOrder StackOrder { get; set; }

        public ushort ServerId { get; set; }
        public ushort ClientId { get; set; }

        public string? Name { get; set; }
        public string? Article { get; set; }
        public string? Description { get; set; }

        public ItemCategory ItemCategory { get; set; } = ItemCategory.Others;

        public ushort? LightLevel { get; set; }
        public ushort? LightColor { get; set; }
        public ushort? WareId { get; set; }

        public byte? UpgradeClassification { get; set; }

        public ushort? MaxTextLen { get; set; }
        public ushort? MaxTextLenOnce { get; set; }
        public ushort? Speed { get; set; }
        public ushort? MinimapColor { get; set; }

        public byte[]? SpriteHash { get; set; }

        [MemberNotNullWhen(returnValue: true, member: nameof(LightLevel))]
        public bool HasLightLevel => LightLevel != null;


        [MemberNotNullWhen(returnValue: true, member: nameof(LightColor))]
        public bool HasLightColor => LightColor != null;


        [MemberNotNullWhen(returnValue: true, member: nameof(WareId))]
        public bool HasWareId => WareId != null;


        [MemberNotNullWhen(returnValue: true, member: nameof(UpgradeClassification))]
        public bool HasUpgradeClassification => UpgradeClassification != null;


        [MemberNotNullWhen(returnValue: true, member: nameof(Name))]
        public bool HasName => Name != null;


        [MemberNotNullWhen(returnValue: true, member: nameof(Article))]
        public bool HasArticle => Article != null;


        [MemberNotNullWhen(returnValue: true, member: nameof(Description))]
        public bool HasDescription => Description != null;


        [MemberNotNullWhen(returnValue: true, member: nameof(MaxTextLen))]
        public bool HasMaxTextLen => MaxTextLen != null;


        [MemberNotNullWhen(returnValue: true, member: nameof(MaxTextLenOnce))]
        public bool HasMaxTextLenOnce => MaxTextLenOnce != null;


        [MemberNotNullWhen(returnValue: true, member: nameof(Speed))]
        public bool HasSpeed => Speed != null;


        [MemberNotNullWhen(returnValue: true, member: nameof(StackOrder))]
        public bool HasStackOrder => StackOrder != StackOrder.None;


        [MemberNotNullWhen(returnValue: true, member: nameof(MinimapColor))]
        public bool HasMinimapColor => MinimapColor != null;


        [MemberNotNullWhen(returnValue: true, member: nameof(SpriteHash))]
        public bool HasSpriteHash => SpriteHash != null;
    }

}