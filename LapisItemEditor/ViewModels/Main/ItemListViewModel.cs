
using System.Collections.ObjectModel;
using Backend;
using ReactiveUI;
using System;
using DynamicData;
using DynamicData.Binding;
using System.Reactive.Linq;

namespace LapisItemEditor.ViewModels
{
    public sealed class ItemListViewModel : ViewModelBase
    {
        private SourceList<ItemModel> _items;
        private ReadOnlyObservableCollection<ItemModel> _observableItems;

        public ReactiveCommand<ItemModel, ItemModel> ItemTypeSelected { get; }

        public ItemListViewModel()
        {
            ItemTypeSelected = ReactiveCommand.Create<ItemModel, ItemModel>(itemModel => { return itemModel; });
            Items = new SourceList<ItemModel>();

            var loader = Items
                .Connect()
                .Sort(SortExpressionComparer<ItemModel>.Ascending(x => x.ServerId))
                .ObserveOn(RxApp.MainThreadScheduler)
                .Bind(out _observableItems)
                .DisposeMany().Subscribe();
        }

        public SourceList<ItemModel> Items
        {
            get => _items;
            set => this.RaiseAndSetIfChanged(ref _items, value);
        }

        public ReadOnlyObservableCollection<ItemModel> ObservableItems => _observableItems;

        public class ItemModel : ViewModelBase
        {
            private double _height = double.NaN;
            private Avalonia.Media.Imaging.Bitmap? spriteImage = null;

            public Avalonia.Media.Imaging.Bitmap? ItemImage
            {
                get
                {
                    if (spriteImage == null)
                    {
                        var bitmap = Backend.Backend.GetItemTypeBitmap(Appearance);

                        if (bitmap != null)
                        {
                            spriteImage = ImageExtensions.ConvertToAvaloniaBitmap(bitmap);
                        }
                    }

                    return spriteImage;
                }

                set => this.RaiseAndSetIfChanged(ref spriteImage, value);
            }

            public string Name { get; set; } = "";
            public string Text { get; set; } = "";
            public uint ServerId { get; set; } = 0;
            public uint ClientId { get; set; } = 0;
            public Appearance? Appearance { get; set; }

            public double Height
            {
                get => _height;
                set => this.RaiseAndSetIfChanged(ref _height, value);
            }
        }
    }

}
