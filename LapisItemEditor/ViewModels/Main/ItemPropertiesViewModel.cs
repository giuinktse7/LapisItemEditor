using System.Diagnostics;
using Backend;
using ReactiveUI;
using System.Reactive.Linq;
using System;
using Avalonia.Data.Converters;
using System.Globalization;
using Tibia.Protobuf.Appearances;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Avalonia.Controls.Selection;

namespace LapisItemEditor.ViewModels.ItemProperties
{
    public static class ReactiveExtensions
    {
        public static IObservable<uint> WhenUInt32<TSender>(this TSender sender, Expression<Func<TSender, string>> property1)
        {
            return sender.WhenAnyValue(property1)
            .Select<string, uint?>(rawValue =>
            {
                if (UInt32.TryParse(rawValue, out uint value))
                {
                    return value;
                }
                return null;
            }).WhereNotNull()
            .Select(x => (uint)x);
        }
    }

    public class WritableTypeConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object parameter, CultureInfo culture)
        {
            switch ((WritableType)value)
            {
                case WritableType.NotWritable:
                    return "Not Writable";
                case WritableType.Writable:
                    return "Writable";
                case WritableType.WritableOnce:
                    return "Writable Once";
                default:
                    throw new InvalidProgramException();
            }
        }

        public object? ConvertBack(object? value, Type targetType, object parameter, CultureInfo culture)
        {
            return (WritableType)value;
        }
    }

    public class HangableHookTypeConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object parameter, CultureInfo culture)
        {
            Debug.Assert(value != null);
            switch ((HangableHookType)value)
            {
                case HangableHookType.None:
                    return "None";
                case HangableHookType.East:
                    return "East";
                case HangableHookType.South:
                    return "South";
                default:
                    throw new InvalidProgramException();
            }
        }

        public object? ConvertBack(object? value, Type targetType, object parameter, CultureInfo culture)
        {
            Debug.Assert(value != null);
            return (HangableHookType)value;
        }
    }

    public class ItemSlotConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object parameter, CultureInfo culture)
        {
            Debug.Assert(value != null);
            switch ((ClothSlot)value)
            {
                case ClothSlot.None:
                    return "None";
                case ClothSlot.TwoHandWeapon:
                    return "Two-Handed Weapon";
                case ClothSlot.Helmet:
                    return "Helmet";
                case ClothSlot.Amulet:
                    return "Amulet";
                case ClothSlot.Backpack:
                    return "Backpack";
                case ClothSlot.Armor:
                    return "Armor";
                case ClothSlot.Shield:
                    return "Shield";
                case ClothSlot.OneHandWeapon:
                    return "One-Handed Weapon";
                case ClothSlot.Legs:
                    return "Legs";
                case ClothSlot.Boots:
                    return "Boots";
                case ClothSlot.Ring:
                    return "Ring";
                case ClothSlot.Arrow:
                    return "Arrow";
                default:
                    throw new InvalidProgramException();
            }
        }

        public object? ConvertBack(object? value, Type targetType, object parameter, CultureInfo culture)
        {
            Debug.Assert(value != null);
            return (ClothSlot)value;
        }
    }



    public class DefaultActionConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object parameter, CultureInfo culture)
        {
            Debug.Assert(value != null);
            switch ((Tibia.Protobuf.Shared.PLAYER_ACTION)value)
            {
                case Tibia.Protobuf.Shared.PLAYER_ACTION.None:
                    return "None";
                case Tibia.Protobuf.Shared.PLAYER_ACTION.Look:
                    return "Look";
                case Tibia.Protobuf.Shared.PLAYER_ACTION.Use:
                    return "Use";
                case Tibia.Protobuf.Shared.PLAYER_ACTION.Open:
                    return "Open";
                case Tibia.Protobuf.Shared.PLAYER_ACTION.AutowalkHighlight:
                    return "Autowalk Highlight";
                default:
                    throw new InvalidProgramException();
            }
        }

        public object? ConvertBack(object? value, Type targetType, object parameter, CultureInfo culture)
        {
            Debug.Assert(value != null);
            return (Tibia.Protobuf.Shared.PLAYER_ACTION)value;
        }
    }

    public class MarketCategoryConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object parameter, CultureInfo culture)
        {
            Debug.Assert(value != null);
            switch ((Tibia.Protobuf.Shared.ITEM_CATEGORY)value)
            {
                case Tibia.Protobuf.Shared.ITEM_CATEGORY.Armors:
                    return "Armors";
                case Tibia.Protobuf.Shared.ITEM_CATEGORY.Amulets:
                    return "Amulets";
                case Tibia.Protobuf.Shared.ITEM_CATEGORY.Boots:
                    return "Boots";
                case Tibia.Protobuf.Shared.ITEM_CATEGORY.Containers:
                    return "Containers";
                case Tibia.Protobuf.Shared.ITEM_CATEGORY.Decoration:
                    return "Decoration";
                case Tibia.Protobuf.Shared.ITEM_CATEGORY.Food:
                    return "Food";
                case Tibia.Protobuf.Shared.ITEM_CATEGORY.HelmetsHats:
                    return "Helmets & Hats";
                case Tibia.Protobuf.Shared.ITEM_CATEGORY.Legs:
                    return "Legs";
                case Tibia.Protobuf.Shared.ITEM_CATEGORY.Others:
                    return "Others";
                case Tibia.Protobuf.Shared.ITEM_CATEGORY.Potions:
                    return "Potions";
                case Tibia.Protobuf.Shared.ITEM_CATEGORY.Rings:
                    return "Rings";
                case Tibia.Protobuf.Shared.ITEM_CATEGORY.Runes:
                    return "Runes";
                case Tibia.Protobuf.Shared.ITEM_CATEGORY.Shields:
                    return "Shields";
                case Tibia.Protobuf.Shared.ITEM_CATEGORY.Tools:
                    return "Tools";
                case Tibia.Protobuf.Shared.ITEM_CATEGORY.Valuables:
                    return "Valuables";
                case Tibia.Protobuf.Shared.ITEM_CATEGORY.Ammunition:
                    return "Ammunition";
                case Tibia.Protobuf.Shared.ITEM_CATEGORY.Axes:
                    return "Axes";
                case Tibia.Protobuf.Shared.ITEM_CATEGORY.Clubs:
                    return "Clubs";
                case Tibia.Protobuf.Shared.ITEM_CATEGORY.DistanceWeapons:
                    return "Distance Weapons";
                case Tibia.Protobuf.Shared.ITEM_CATEGORY.Swords:
                    return "Swords";
                case Tibia.Protobuf.Shared.ITEM_CATEGORY.WandsRods:
                    return "WandsRods";
                case Tibia.Protobuf.Shared.ITEM_CATEGORY.PremiumScrolls:
                    return "Premium Scrolls";
                case Tibia.Protobuf.Shared.ITEM_CATEGORY.TibiaCoins:
                    return "Tibia Coins";
                case Tibia.Protobuf.Shared.ITEM_CATEGORY.CreatureProducts:
                    return "Creature Products";
                default:
                    throw new InvalidProgramException();
            }
        }

        public object? ConvertBack(object? value, Type targetType, object parameter, CultureInfo culture)
        {
            Debug.Assert(value != null);
            return (Tibia.Protobuf.Shared.ITEM_CATEGORY)value;
        }
    }


    public class PlayerProfessionConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object parameter, CultureInfo culture)
        {
            Debug.Assert(value != null);
            switch ((Tibia.Protobuf.Shared.VOCATION)value)
            {
                case Tibia.Protobuf.Shared.VOCATION.Any:
                    return "Any";
                case Tibia.Protobuf.Shared.VOCATION.None:
                    return "None";
                case Tibia.Protobuf.Shared.VOCATION.Knight:
                    return "Knight";
                case Tibia.Protobuf.Shared.VOCATION.Paladin:
                    return "Paladin";
                case Tibia.Protobuf.Shared.VOCATION.Sorcerer:
                    return "Sorcerer";
                case Tibia.Protobuf.Shared.VOCATION.Druid:
                    return "Druid";
                case Tibia.Protobuf.Shared.VOCATION.Promoted:
                    return "Promoted";
                default:
                    throw new InvalidProgramException();
            }
        }

        public object? ConvertBack(object? value, Type targetType, object parameter, CultureInfo culture)
        {
            Debug.Assert(value != null);
            return (Tibia.Protobuf.Shared.VOCATION)value;
        }
    }


    public enum WritableType
    {
        NotWritable = 0,
        Writable = 1,
        WritableOnce = 2,
    }

    public enum HangableHookType
    {
        None,
        East,
        South
    }


    public class ItemPropertiesViewModel : ViewModelBase
    {
        const int BankWaypointMinimum = 0;

        // Default length of a black book (See: https://tibia.fandom.com/wiki/Book_(Black))
        const int DefaultWritableTextLength = 1023;

        const int ObjectIdMin = 100;
        private Backend.Appearance? appearance;

        private WritableType writableType;
        private HangableHookType hangableHookType;
        private ClothSlot clothSlot;
        private Tibia.Protobuf.Shared.PLAYER_ACTION selectedDefaultAction;
        private Tibia.Protobuf.Shared.ITEM_CATEGORY? marketCategory;

        // private ObjectType objectType;
        private bool bank;
        private uint bankWaypoints;

        private uint shiftX;
        private uint shiftY;

        private uint lightBrightness;
        private uint lightColor;

        private uint changedToExpireFormerObjectTypeId;
        private uint elevation;
        private uint automapColor;

        private bool clip;
        private bool bottom;
        private bool top;
        private bool container;
        private bool cumulative;
        private bool usable;
        private bool forceUse;
        private bool multiUse;
        private bool liquidPool;
        private bool liquidContainer;
        private bool unpass;
        private bool unmove;
        private bool unsight;
        private bool avoid;
        private bool take;
        private bool hang;
        private bool noMovementAnimation;
        private bool cyclopediaItem;
        private bool rotate;
        private bool light;
        private bool dontHide;
        private bool translucent;
        private bool shift;
        private bool wrap;
        private bool unwrap;
        private bool topEffect;
        private bool npcSaleData;
        private bool corpse;
        private bool playerCorpse;
        private bool height;
        private bool lyingObject;
        private bool animateAlways;
        private bool automap;
        private bool lensHelp;
        private bool fullBank;
        private bool ignoreLook;
        private bool hasClothSlot;
        private bool defaultAction;
        private bool hasMarket;
        private bool hasMarketCategory;
        private bool hasProfession;
        private bool hasTradeAs;
        private bool hasShowAs;
        private bool hasMinimumLevel;
        private bool changedToExpire;

        private uint cyclopediaType;
        private string cyclopediaTypeText;


        public ItemPropertiesViewModel()
        {
            WritableItemTypes = new();
            ((IList<WritableType>)Enum.GetValues(typeof(WritableType))).ToList().ForEach(x => WritableItemTypes.Add(x));

            HangableHookTypes = new();
            ((IList<HangableHookType>)Enum.GetValues(typeof(HangableHookType))).ToList().ForEach(x => HangableHookTypes.Add(x));

            ClothSlots = new();
            ((IList<ClothSlot>)Enum.GetValues(typeof(ClothSlot))).ToList().ForEach(x => ClothSlots.Add(x));

            DefaultActions = new();
            ((IList<Tibia.Protobuf.Shared.PLAYER_ACTION>)Enum.GetValues(typeof(Tibia.Protobuf.Shared.PLAYER_ACTION))).ToList().ForEach(x => DefaultActions.Add(x));

            MarketCategories = new();
            ((IList<Tibia.Protobuf.Shared.ITEM_CATEGORY>)Enum.GetValues(typeof(Tibia.Protobuf.Shared.ITEM_CATEGORY))).ToList().ForEach(x => MarketCategories.Add(x));

            PlayerProfessions = new();
            ((IList<Tibia.Protobuf.Shared.VOCATION>)Enum.GetValues(typeof(Tibia.Protobuf.Shared.VOCATION))).ToList().ForEach(x => PlayerProfessions.Add(x));

            MarketProfessionSelection = new SelectionModel<Tibia.Protobuf.Shared.VOCATION>();
            MarketProfessionSelection.SingleSelect = false;
            MarketProfessionSelection.SelectionChanged += MarketProfessionSelectionChanged;


            this.WhenAnyValue(x => x.Appearance)
            .Subscribe(appearance =>
            {
                if (appearance == null)
                {
                    Bank = false;
                    BankWaypoints = BankWaypointMinimum;

                    Container = false;
                    Cumulative = false;
                    Usable = false;
                    ForceUse = false;
                    MultiUse = false;
                    LiquidPool = false;
                    LiquidContainer = false;
                    Unpass = false;
                    Unmove = false;
                    Unsight = false;
                    Avoid = false;
                    Take = false;
                    Hang = false;
                    NoMovementAnimation = false;
                    CyclopediaItem = false;
                    Rotate = false;
                    Light = false;
                    DontHide = false;
                    Translucent = false;
                    Shift = false;
                    Wrap = false;
                    Unwrap = false;
                    TopEffect = false;
                    NPCSaleData = false;
                    Corpse = false;
                    PlayerCorpse = false;
                    Height = false;
                    LyingObject = false;
                    AnimateAlways = false;
                    Automap = false;
                    LensHelp = false;
                    FullBank = false;
                    IgnoreLook = false;
                    HasClothSlot = false;
                    DefaultAction = false;
                    HasMarket = false;
                    HasMarketCategory = false;
                    HasMarketProfession = false;
                    HasTradeAs = false;
                    HasShowAs = false;
                    HasMinimumLevel = false;
                    ChangedToExpire = false;

                    LightBrightness = 0;
                    LightColor = 0;
                    ShiftX = 0;
                    ShiftY = 0;
                    Elevation = 0;
                    ChangedToExpireFormerObjectTypeId = 0;
                    AutomapColor = 0;
                    clothSlot = ClothSlot.None;
                    selectedDefaultAction = Tibia.Protobuf.Shared.PLAYER_ACTION.None;

                    marketCategory = null;
                }
                else
                {
                    // _flags to not shadow the flags member
                    var _flags = appearance.Data.Flags;

                    if (_flags.Bank != null)
                    {
                        Bank = true;
                        BankWaypoints = _flags.Bank.Waypoints;
                    }
                    else
                    {
                        Bank = false;
                    }

                    Clip = _flags.Clip;
                    Bottom = _flags.Bottom;
                    Top = _flags.Top;
                    Container = _flags.Container;
                    Cumulative = _flags.Cumulative;
                    Usable = _flags.Usable;
                    ForceUse = _flags.Forceuse;
                    MultiUse = _flags.Multiuse;
                    LiquidPool = _flags.Liquidpool;
                    LiquidContainer = _flags.Liquidcontainer;
                    Unpass = _flags.Unpass;
                    Unmove = _flags.Unmove;
                    Unsight = _flags.Unsight;
                    Avoid = _flags.Avoid;
                    Take = _flags.Take;
                    Hang = _flags.Hang;
                    NoMovementAnimation = _flags.NoMovementAnimation;

                    if (_flags.Clothes != null)
                    {
                        HasClothSlot = true;
                        ClothSlot = (ClothSlot)_flags.Clothes.Slot;
                    }
                    else
                    {
                        HasClothSlot = false;
                        ClothSlot = ClothSlot.None;
                    }

                    if (_flags.DefaultAction != null)
                    {
                        DefaultAction = true;
                        SelectedDefaultAction = _flags.DefaultAction.Action;
                    }
                    else
                    {
                        DefaultAction = false;
                        SelectedDefaultAction = Tibia.Protobuf.Shared.PLAYER_ACTION.None;
                    }


                    if (_flags.Cyclopediaitem != null)
                    {
                        CyclopediaItem = true;
                        CyclopediaType = _flags.Cyclopediaitem.CyclopediaType;
                    }
                    else
                    {
                        CyclopediaItem = false;
                        CyclopediaType = ObjectIdMin;
                    }

                    Rotate = _flags.Rotate;

                    if (_flags.Light != null)
                    {
                        Light = true;
                        LightBrightness = _flags.Light.Brightness;
                        LightColor = _flags.Light.Color;
                    }
                    else
                    {
                        Light = false;
                        LightBrightness = 0;
                        LightColor = 0;
                    }

                    if (_flags.Shift != null)
                    {
                        Shift = true;
                        ShiftX = _flags.Shift.HasX ? _flags.Shift.X : 0;
                        ShiftY = _flags.Shift.HasY ? _flags.Shift.Y : 0;
                    }
                    else
                    {
                        Shift = false;
                        ShiftX = 0;
                        ShiftY = 0;
                    }

                    DontHide = _flags.DontHide;
                    Translucent = _flags.Translucent;
                    Wrap = _flags.Wrap;
                    Unwrap = _flags.Unwrap;
                    TopEffect = _flags.Topeffect;
                    NPCSaleData = _flags.Npcsaledata != null ? _flags.Npcsaledata.Count != 0 : false;
                    Corpse = _flags.Corpse;
                    PlayerCorpse = _flags.PlayerCorpse;

                    if (_flags.Height != null)
                    {
                        Height = true;
                        Elevation = _flags.Height.Elevation;
                    }
                    else
                    {
                        Height = false;
                        Elevation = 0;
                    }

                    LyingObject = _flags.LyingObject;
                    AnimateAlways = _flags.AnimateAlways;
                    if (_flags.Automap != null)
                    {
                        Automap = true;
                        AutomapColor = _flags.Automap.Color;
                    }
                    else
                    {
                        Automap = false;
                        AutomapColor = 0;
                    }

                    LensHelp = _flags.Lenshelp?.HasId ?? false;
                    FullBank = _flags.Fullbank;
                    IgnoreLook = _flags.IgnoreLook;
                    HasClothSlot = _flags.Clothes?.HasSlot ?? false;
                    DefaultAction = _flags.DefaultAction?.HasAction ?? false;

                    if (_flags.Market != null)
                    {
                        HasMarket = true;

                        if (_flags.Market.HasCategory)
                        {
                            HasMarketCategory = true;
                            MarketCategory = _flags.Market.Category;
                        }

                        foreach (var profession in _flags.Market.RestrictToVocation)
                        {
                            switch (profession)
                            {
                                case Tibia.Protobuf.Shared.VOCATION.Any:
                                    MarketProfessionSelection.Select(-1);
                                    break;
                                case Tibia.Protobuf.Shared.VOCATION.None:
                                    MarketProfessionSelection.Select(0);
                                    break;
                                case Tibia.Protobuf.Shared.VOCATION.Knight:
                                    MarketProfessionSelection.Select(1);
                                    break;
                                case Tibia.Protobuf.Shared.VOCATION.Paladin:
                                    MarketProfessionSelection.Select(2);
                                    break;
                                case Tibia.Protobuf.Shared.VOCATION.Sorcerer:
                                    MarketProfessionSelection.Select(3);
                                    break;
                                case Tibia.Protobuf.Shared.VOCATION.Druid:
                                    MarketProfessionSelection.Select(4);
                                    break;
                                case Tibia.Protobuf.Shared.VOCATION.Promoted:
                                    MarketProfessionSelection.Select(5);
                                    break;
                                default:
                                    throw new InvalidProgramException();
                            }
                        }
                    }

                    HasMarket = _flags.Market != null ? _flags.Market.HasCategory || _flags.Market.HasShowAsObjectId || _flags.Market.HasTradeAsObjectId : false;
                    HasMarketCategory = _flags.Market?.HasCategory ?? false;
                    HasMarketProfession = (_flags.Market?.RestrictToVocation?.Count ?? 0) != 0;
                    HasTradeAs = _flags.Market?.HasTradeAsObjectId ?? false;
                    HasShowAs = _flags.Market?.HasShowAsObjectId ?? false;
                    HasMinimumLevel = _flags.Market?.HasMinimumLevel ?? false;

                    if (_flags.Changedtoexpire != null)
                    {
                        ChangedToExpire = true;
                        ChangedToExpireFormerObjectTypeId = _flags.Changedtoexpire.FormerObjectTypeid;
                    }
                    else
                    {
                        ChangedToExpire = false;
                        ChangedToExpireFormerObjectTypeId = ObjectIdMin;
                    }

                    if (_flags.Write != null)
                    {
                        WritableType = WritableType.Writable;
                    }
                    else if (_flags.WriteOnce != null)
                    {
                        WritableType = WritableType.WritableOnce;
                    }
                    else
                    {
                        WritableType = WritableType.NotWritable;
                    }
                }
            });

            this.WhenAnyValue(x => x.Container).Subscribe(value => { if (flags != null) flags.Container = value; });
            this.WhenAnyValue(x => x.Cumulative).Subscribe(value => { if (flags != null) flags.Cumulative = value; });
            this.WhenAnyValue(x => x.Usable).Subscribe(value => { if (flags != null) flags.Usable = value; });

            this.WhenAnyValue(x => x.ForceUse).Subscribe(value => { if (flags != null) flags.Forceuse = value; });
            this.WhenAnyValue(x => x.MultiUse).Subscribe(value => { if (flags != null) flags.Multiuse = value; });
            this.WhenAnyValue(x => x.LiquidPool).Subscribe(value => { if (flags != null) flags.Liquidpool = value; });
            this.WhenAnyValue(x => x.LiquidContainer).Subscribe(value => { if (flags != null) flags.Liquidcontainer = value; });
            this.WhenAnyValue(x => x.Unpass).Subscribe(value => { if (flags != null) flags.Unpass = value; });
            this.WhenAnyValue(x => x.Unmove).Subscribe(value => { if (flags != null) flags.Unmove = value; });
            this.WhenAnyValue(x => x.Unsight).Subscribe(value => { if (flags != null) flags.Unsight = value; });
            this.WhenAnyValue(x => x.Avoid).Subscribe(value => { if (flags != null) flags.Avoid = value; });
            this.WhenAnyValue(x => x.Take).Subscribe(value => { if (flags != null) flags.Take = value; });
            this.WhenAnyValue(x => x.Hang).Subscribe(value => { if (flags != null) flags.Hang = value; });
            this.WhenAnyValue(x => x.NoMovementAnimation).Subscribe(value => { if (flags != null) flags.NoMovementAnimation = value; });
            this.WhenAnyValue(x => x.Rotate).Subscribe(value => { if (flags != null) flags.Rotate = value; });
            this.WhenAnyValue(x => x.Light).Subscribe(value =>
            {
                if (flags == null) return;

                if (value)
                {
                    if (flags.Light == null)
                    {
                        flags.Light = new AppearanceFlagLight() { Brightness = LightBrightness, Color = LightColor };
                    }
                }
                else
                {
                    flags.Light = null;
                }

            });
            this.WhenAnyValue(x => x.LightBrightness).Subscribe(value => { if (flags != null && flags.Light != null) flags.Light.Brightness = value; });
            this.WhenAnyValue(x => x.LightColor).Subscribe(value => { if (flags != null && flags.Light != null) flags.Light.Color = value; });
            this.WhenAnyValue(x => x.DontHide).Subscribe(value => { if (flags != null) flags.DontHide = value; });
            this.WhenAnyValue(x => x.Translucent).Subscribe(value => { if (flags != null) flags.Translucent = value; });
            this.WhenAnyValue(x => x.Shift).Subscribe(value =>
            {
                if (flags != null) flags.Shift = value ? new AppearanceFlagShift() { X = 0, Y = 0 } : null;
            });
            this.WhenAnyValue(x => x.ShiftX).Subscribe(value =>
            {
                if (flags != null && flags.Shift != null) flags.Shift.X = value;
            });
            this.WhenAnyValue(x => x.ShiftY).Subscribe(value =>
            {
                if (flags != null && flags.Shift != null) flags.Shift.Y = value;
            });

            this.WhenAnyValue(x => x.Bank).Subscribe(isBank =>
            {
                if (flags != null)
                {
                    if (isBank)
                    {
                        if (flags.Bank == null)
                        {
                            flags.Bank = new AppearanceFlagBank() { Waypoints = BankWaypointMinimum };
                        }
                    }
                }
            });

            this.WhenAnyValue(x => x.BankWaypoints).Subscribe(value =>
            {
                if (flags == null) return;

                if (flags.Bank != null)
                {
                    flags.Bank.Waypoints = value;
                }
                else
                {
                    flags.Bank = new AppearanceFlagBank() { Waypoints = value };
                }
            });

            this.WhenAnyValue(x => x.Clip).Subscribe(value => { if (flags != null) flags.Clip = value; });
            this.WhenAnyValue(x => x.Bottom).Subscribe(value => { if (flags != null) flags.Bottom = value; });
            this.WhenAnyValue(x => x.Top).Subscribe(value => { if (flags != null) flags.Top = value; });

            this.WhenAnyValue(x => x.WritableType).Subscribe(value =>
            {
                if (flags != null)
                {
                    switch (value)
                    {
                        case WritableType.NotWritable:
                            flags.Write = null;
                            flags.WriteOnce = null;
                            break;
                        case WritableType.Writable:
                            if (flags.Write == null)
                            {
                                flags.Write = new AppearanceFlagWrite() { MaxTextLength = DefaultWritableTextLength };
                            }
                            flags.WriteOnce = null;
                            break;
                        case WritableType.WritableOnce:
                            if (flags.Write == null)
                            {
                                flags.WriteOnce = new AppearanceFlagWriteOnce() { MaxTextLengthOnce = DefaultWritableTextLength };
                            }
                            flags.Write = null;
                            break;
                    }
                }
            });

            this.WhenAnyValue(x => x.ClothSlot).Subscribe(value =>
            {
                if (flags == null) { return; }

                if (flags?.Clothes != null)
                {
                    flags.Clothes.Slot = (uint)value;
                }
            });

            this.WhenAnyValue(x => x.SelectedDefaultAction).Subscribe(value =>
            {
                if (flags == null) { return; }

                if (flags?.DefaultAction != null)
                {
                    flags.DefaultAction.Action = value;
                }
            });

            this.WhenAnyValue(x => x.CyclopediaItem).Subscribe(value =>
            {
                if (flags == null) { return; }

                if (value)
                {
                    if (flags.Cyclopediaitem == null)
                    {
                        flags.Cyclopediaitem = new AppearanceFlagCyclopedia() { CyclopediaType = CyclopediaType };
                    }
                }
                else
                {
                    flags.Cyclopediaitem = null;
                }
            });

            this.WhenAnyValue(x => x.CyclopediaType).Subscribe(value =>
            {
                if (flags?.Cyclopediaitem != null)
                {
                    flags.Cyclopediaitem.CyclopediaType = value;
                }
            });

            this.WhenAnyValue(x => x.ChangedToExpire).Subscribe(value =>
            {
                if (flags == null) return;

                if (value)
                {
                    if (flags.Changedtoexpire == null)
                    {
                        flags.Changedtoexpire = new AppearanceFlagChangedToExpire() { FormerObjectTypeid = ObjectIdMin };
                    }
                }
                else
                {
                    flags.Changedtoexpire = null;
                }
            });

            this.WhenAnyValue(x => x.ChangedToExpireFormerObjectTypeId).Subscribe(value =>
            {
                if (flags?.Changedtoexpire != null)
                {
                    flags.Changedtoexpire.FormerObjectTypeid = value;
                }
            });


            this.WhenAnyValue(x => x.Height).Subscribe(value =>
            {
                if (flags == null) return;

                if (value)
                {
                    if (flags.Height == null)
                    {
                        flags.Height = new AppearanceFlagHeight() { Elevation = 0 };
                    }
                }
                else
                {
                    flags.Height = null;
                }
            });

            this.WhenAnyValue(x => x.Elevation).Subscribe(value =>
            {
                if (flags?.Height != null)
                {
                    flags.Height.Elevation = value;
                }
            });

            this.WhenAnyValue(x => x.Automap).Subscribe(value =>
            {
                if (flags == null) return;

                if (value)
                {
                    if (flags.Automap == null)
                    {
                        flags.Automap = new AppearanceFlagAutomap() { Color = 0 };
                    }
                }
                else
                {
                    flags.Automap = null;
                }
            });

            this.WhenAnyValue(x => x.AutomapColor).Subscribe(value =>
            {
                if (flags?.Automap != null)
                {
                    flags.Automap.Color = value;
                }
            });

            this.WhenAnyValue(x => x.FullBank).Subscribe(value =>
            {
                if (flags != null)
                {
                    flags.Fullbank = value;
                }
            });

            this.WhenAnyValue(x => x.IgnoreLook).Subscribe(value =>
            {
                if (flags != null)
                {
                    flags.Fullbank = value;
                }
            });

            this.WhenAnyValue(x => x.HasClothSlot).Subscribe(value =>
            {
                if (flags != null)
                {
                    if (value)
                    {
                        if (flags.Clothes == null)
                        {
                            flags.Clothes = new AppearanceFlagClothes() { Slot = (uint)ClothSlot.None };
                        }
                    }
                    else
                    {
                        flags.Clothes = null;
                    }

                }
            });

            this.WhenAnyValue(x => x.DefaultAction).Subscribe(value =>
            {
                if (flags != null)
                {
                    if (value)
                    {
                        if (flags.DefaultAction == null)
                        {
                            flags.DefaultAction = new AppearanceFlagDefaultAction() { Action = Tibia.Protobuf.Shared.PLAYER_ACTION.None };
                        }
                    }
                    else
                    {
                        flags.DefaultAction = null;
                    }
                }
            });

            this.WhenAnyValue(x => x.HasMarket).Subscribe(value =>
            {
                if (flags == null) { return; }

                if (!value)
                {
                    flags.Market = null;
                }
            });

            this.WhenAnyValue(x => x.MarketCategory).Subscribe(value =>
            {
                if (flags == null || flags.Market == null || value == null) { return; }

                flags.Market.Category = (Tibia.Protobuf.Shared.ITEM_CATEGORY)value;
            });

            _isWritable = this.WhenAnyValue(x => x.WritableType)
                        .Select(x => x != WritableType.NotWritable)
                        .ToProperty(this, x => x.IsWritable);


            this.WhenUInt32(x => x.CyclopediaTypeText).Subscribe(value => CyclopediaType = value);
        }

        public Backend.Appearance? Appearance { get => appearance; set => this.RaiseAndSetIfChanged(ref appearance, value); }
        private Tibia.Protobuf.Appearances.AppearanceFlags? flags { get => Appearance?.Data.Flags; }

        public WritableType WritableType { get => writableType; set => this.RaiseAndSetIfChanged(ref writableType, value); }
        public HangableHookType HangableHookType { get => hangableHookType; set => this.RaiseAndSetIfChanged(ref hangableHookType, value); }
        public ClothSlot ClothSlot { get => clothSlot; set => this.RaiseAndSetIfChanged(ref clothSlot, value); }
        public Tibia.Protobuf.Shared.PLAYER_ACTION SelectedDefaultAction { get => selectedDefaultAction; set => this.RaiseAndSetIfChanged(ref selectedDefaultAction, value); }
        public Tibia.Protobuf.Shared.ITEM_CATEGORY? MarketCategory { get => marketCategory; set => this.RaiseAndSetIfChanged(ref marketCategory, value); }

        public uint LightBrightness { get => lightBrightness; set => this.RaiseAndSetIfChanged(ref lightBrightness, value); }
        public uint LightColor { get => lightColor; set => this.RaiseAndSetIfChanged(ref lightColor, value); }

        public uint ShiftX { get => shiftX; set => this.RaiseAndSetIfChanged(ref shiftX, value); }
        public uint ShiftY { get => shiftY; set => this.RaiseAndSetIfChanged(ref shiftY, value); }

        public bool Bank { get => bank; set => this.RaiseAndSetIfChanged(ref bank, value); }
        public bool Clip { get => clip; set => this.RaiseAndSetIfChanged(ref clip, value); }
        public bool Bottom { get => bottom; set => this.RaiseAndSetIfChanged(ref bottom, value); }
        public bool Top { get => top; set => this.RaiseAndSetIfChanged(ref top, value); }
        public bool Container { get => container; set => this.RaiseAndSetIfChanged(ref container, value); }
        public bool Cumulative { get => cumulative; set => this.RaiseAndSetIfChanged(ref cumulative, value); }
        public bool Usable { get => usable; set => this.RaiseAndSetIfChanged(ref usable, value); }
        public bool ForceUse { get => forceUse; set => this.RaiseAndSetIfChanged(ref forceUse, value); }
        public bool MultiUse { get => multiUse; set => this.RaiseAndSetIfChanged(ref multiUse, value); }
        public bool LiquidPool { get => liquidPool; set => this.RaiseAndSetIfChanged(ref liquidPool, value); }
        public bool LiquidContainer { get => liquidContainer; set => this.RaiseAndSetIfChanged(ref liquidContainer, value); }
        public bool Unpass { get => unpass; set => this.RaiseAndSetIfChanged(ref unpass, value); }
        public bool Unmove { get => unmove; set => this.RaiseAndSetIfChanged(ref unmove, value); }
        public bool Unsight { get => unsight; set => this.RaiseAndSetIfChanged(ref unsight, value); }
        public bool Avoid { get => avoid; set => this.RaiseAndSetIfChanged(ref avoid, value); }
        public bool Take { get => take; set => this.RaiseAndSetIfChanged(ref take, value); }
        public bool Hang { get => hang; set => this.RaiseAndSetIfChanged(ref hang, value); }
        public bool NoMovementAnimation { get => noMovementAnimation; set => this.RaiseAndSetIfChanged(ref noMovementAnimation, value); }
        public bool CyclopediaItem { get => cyclopediaItem; set => this.RaiseAndSetIfChanged(ref cyclopediaItem, value); }
        public bool Rotate { get => rotate; set => this.RaiseAndSetIfChanged(ref rotate, value); }
        public bool Light { get => light; set => this.RaiseAndSetIfChanged(ref light, value); }
        public bool DontHide { get => dontHide; set => this.RaiseAndSetIfChanged(ref dontHide, value); }
        public bool Translucent { get => translucent; set => this.RaiseAndSetIfChanged(ref translucent, value); }
        public bool Shift { get => shift; set => this.RaiseAndSetIfChanged(ref shift, value); }
        public bool Wrap { get => wrap; set => this.RaiseAndSetIfChanged(ref wrap, value); }
        public bool Unwrap { get => unwrap; set => this.RaiseAndSetIfChanged(ref unwrap, value); }
        public bool TopEffect { get => topEffect; set => this.RaiseAndSetIfChanged(ref topEffect, value); }
        public bool NPCSaleData { get => npcSaleData; set => this.RaiseAndSetIfChanged(ref npcSaleData, value); }
        public bool Corpse { get => corpse; set => this.RaiseAndSetIfChanged(ref corpse, value); }
        public bool PlayerCorpse { get => playerCorpse; set => this.RaiseAndSetIfChanged(ref playerCorpse, value); }
        public bool Height { get => height; set => this.RaiseAndSetIfChanged(ref height, value); }
        public bool LyingObject { get => lyingObject; set => this.RaiseAndSetIfChanged(ref lyingObject, value); }
        public bool AnimateAlways { get => animateAlways; set => this.RaiseAndSetIfChanged(ref animateAlways, value); }
        public bool Automap { get => automap; set => this.RaiseAndSetIfChanged(ref automap, value); }
        public bool LensHelp { get => lensHelp; set => this.RaiseAndSetIfChanged(ref lensHelp, value); }
        public bool FullBank { get => fullBank; set => this.RaiseAndSetIfChanged(ref fullBank, value); }
        public bool IgnoreLook { get => ignoreLook; set => this.RaiseAndSetIfChanged(ref ignoreLook, value); }
        public bool HasClothSlot { get => hasClothSlot; set => this.RaiseAndSetIfChanged(ref hasClothSlot, value); }
        public bool DefaultAction { get => defaultAction; set => this.RaiseAndSetIfChanged(ref defaultAction, value); }

        public bool HasMarket { get => hasMarket; set => this.RaiseAndSetIfChanged(ref hasMarket, value); }
        public bool HasMarketCategory { get => hasMarketCategory; set => this.RaiseAndSetIfChanged(ref hasMarketCategory, value); }
        public bool HasMarketProfession { get => hasProfession; set => this.RaiseAndSetIfChanged(ref hasProfession, value); }
        public bool HasTradeAs { get => hasTradeAs; set => this.RaiseAndSetIfChanged(ref hasTradeAs, value); }
        public bool HasShowAs { get => hasShowAs; set => this.RaiseAndSetIfChanged(ref hasShowAs, value); }
        public bool HasMinimumLevel { get => hasMinimumLevel; set => this.RaiseAndSetIfChanged(ref hasMinimumLevel, value); }

        public bool ChangedToExpire { get => changedToExpire; set => this.RaiseAndSetIfChanged(ref changedToExpire, value); }

        public uint BankWaypoints { get => bankWaypoints; set => this.RaiseAndSetIfChanged(ref bankWaypoints, value); }

        public uint CyclopediaType { get => cyclopediaType; set => this.RaiseAndSetIfChanged(ref cyclopediaType, value); }

        public string CyclopediaTypeText { get => cyclopediaTypeText; set => this.RaiseAndSetIfChanged(ref cyclopediaTypeText, value); }

        public uint ChangedToExpireFormerObjectTypeId { get => changedToExpireFormerObjectTypeId; set => this.RaiseAndSetIfChanged(ref changedToExpireFormerObjectTypeId, value); }

        public uint Elevation { get => elevation; set => this.RaiseAndSetIfChanged(ref elevation, value); }

        public uint AutomapColor { get => automapColor; set => this.RaiseAndSetIfChanged(ref automapColor, value); }



        #region Writable
        readonly ObservableAsPropertyHelper<bool> _isWritable;
        public bool IsWritable { get { return _isWritable?.Value ?? false; } }

        private ObservableCollection<WritableType> _writableItemTypes;
        public ObservableCollection<WritableType> WritableItemTypes
        {
            get => _writableItemTypes;
            set { this.RaiseAndSetIfChanged(ref _writableItemTypes, value); }
        }

        #endregion

        private ObservableCollection<HangableHookType> _hangableHookTypes;
        public ObservableCollection<HangableHookType> HangableHookTypes
        {
            get => _hangableHookTypes;
            set { this.RaiseAndSetIfChanged(ref _hangableHookTypes, value); }
        }

        private ObservableCollection<ClothSlot> _clothSlots;
        public ObservableCollection<ClothSlot> ClothSlots
        {
            get => _clothSlots;
            set { this.RaiseAndSetIfChanged(ref _clothSlots, value); }
        }

        private ObservableCollection<Tibia.Protobuf.Shared.PLAYER_ACTION> _defaultActions;
        public ObservableCollection<Tibia.Protobuf.Shared.PLAYER_ACTION> DefaultActions
        {
            get => _defaultActions;
            set { this.RaiseAndSetIfChanged(ref _defaultActions, value); }
        }

        private ObservableCollection<Tibia.Protobuf.Shared.ITEM_CATEGORY> _marketCategories;
        public ObservableCollection<Tibia.Protobuf.Shared.ITEM_CATEGORY> MarketCategories
        {
            get => _marketCategories;
            set { this.RaiseAndSetIfChanged(ref _marketCategories, value); }
        }

        public ObservableCollection<Tibia.Protobuf.Shared.VOCATION> PlayerProfessions { get; }

        public SelectionModel<Tibia.Protobuf.Shared.VOCATION> MarketProfessionSelection { get; }


        void MarketProfessionSelectionChanged(object? sender, SelectionModelSelectionChangedEventArgs e)
        {
            // Trace.WriteLine("Selected:");
            // foreach (var x in e.SelectedItems)
            // {
            //     Trace.WriteLine(x);
            // }
            // Trace.WriteLine("Deselected:");
            // foreach (var x in e.DeselectedItems)
            // {
            //     Trace.WriteLine(x);
            // }
        }

    }
}
