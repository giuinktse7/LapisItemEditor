
using System.Collections.ObjectModel;
using Backend;
using ReactiveUI;
using System;
using DynamicData;
using DynamicData.Binding;
using System.Reactive.Linq;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Windows.Input;

namespace LapisItemEditor.ViewModels

{


    public sealed class ItemListViewModel : ViewModelBase
    {
        private SourceList<ItemModel> _items;
        private ReadOnlyObservableCollection<ItemModel> _observableItems;

        private bool searchForServerId;
        private string searchQuery;
        public ReactiveCommand<ItemModel, ItemModel> ItemTypeSelected { get; }

        public ICommand Search { get; }

        public string SearchQuery { get => searchQuery; set => this.RaiseAndSetIfChanged(ref searchQuery, value); }
        public bool SearchForServerId { get => searchForServerId; set => this.RaiseAndSetIfChanged(ref searchForServerId, value); }


        public class ItemModelComparer<T> : IEqualityComparer<ItemModel>
        {
            Func<ItemModel, T> f;
            public ItemModelComparer(Func<ItemModel, T> f)
            {
                this.f = f;
            }

            public bool Equals(ItemModel? x, ItemModel? y)
            {
                return f(x).Equals(f(y));
            }

            public int GetHashCode([DisallowNull] ItemModel obj)
            {
                return (int)obj.ClientId;
            }
        }

        public int getIndex(uint id, bool clientId = true)
        {
            var item = new ItemModel();
            Func<ItemModel, uint> f;
            if (clientId)
            {
                f = (a) => a.ClientId;
                item.ClientId = id;
            }
            else
            {
                f = (a) => a.ServerId;
                item.ServerId = id;
            }

            return Items.AsObservableList().Items.IndexOf(item, new ItemModelComparer<uint>(f));
        }


        public ItemListViewModel()
        {
            ItemTypeSelected = ReactiveCommand.Create<ItemModel, ItemModel>(itemModel => { return itemModel; });
            Items = new SourceList<ItemModel>();

            var searchFilter = this.WhenValueChanged(x => x.SearchQuery)
            .Select(SearchNamePredicate);

            var sorter = this.WhenValueChanged(x => x.SearchQuery)
            .Select(SortComparer);

            var loader = Items
                .Connect()
                .Filter(searchFilter)
                .Sort(sorter)
                .ObserveOn(RxApp.MainThreadScheduler)
                .Bind(out _observableItems)
                .DisposeMany().Subscribe();
        }

        private Func<ItemModel, bool> SearchNamePredicate(string query)
        {
            bool isInt = int.TryParse(query, out var result);
            if (isInt || query == null || query.Length < 3)
            {
                return model => true;
            }

            return model => FuzzySharp.Fuzz.Ratio(query, model.Name) >= 0.8;
        }

        private IComparer<ItemModel> SortComparer(string query)
        {
            bool isInt = int.TryParse(query, out var result);
            if (isInt || query == null || query.Length < 3)
            {
                return SortExpressionComparer<ItemModel>.Ascending(x => x.ServerId);
            }
            else
            {
                return SortExpressionComparer<ItemModel>.Descending(model => FuzzySharp.Fuzz.Ratio(query, model.Name));
            }
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
