using System;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using Avalonia.Threading;
using LapisItemEditor.ViewModels;
using static LapisItemEditor.ViewModels.ItemListViewModel;

namespace LapisItemEditor.Views
{

    public partial class ItemListView : ReactiveUserControl<ItemListViewModel>
    {

        public static Action Debounce(Action f, int milliseconds = 300)
        {
            var last = 0;
            return () =>
            {
                var current = Interlocked.Increment(ref last);

                Task.Delay(milliseconds).ContinueWith(task =>
                {
                    if (current == last)
                    {
                        Dispatcher.UIThread.Post(() => f());
                    }
                    task.Dispose();
                });
            };
        }
        private ScrollViewer scrollView;
        private Action debouncedSearch;
        private Action debouncedWrapper;

        public TextBox SearchBox => this.FindControl<TextBox>("search_box");


        public ItemListView()
        {
            InitializeComponent();

            scrollView = this.FindControl<ScrollViewer>("scroller");
            debouncedWrapper = Debounce(search, 300);
        }

        private void search()
        {
            if (ViewModel == null)
            {
                return;
            }

            ViewModel.SearchQuery = SearchBox.Text;

            if (int.TryParse(SearchBox.Text, out var result))
            {
                bool useClientId = !ViewModel.SearchForServerId;
                var index = ViewModel.getIndex((uint)result, useClientId);

                if (index != -1)
                {
                    scrollView.Offset = new Avalonia.Point(0, index * 51);
                }
            }
            else
            {
                scrollView.Offset = new Avalonia.Point(0, 0);
            }
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);

            var a = SearchBox;

        }

        public void OnSelectTemplateKey(object sender, SelectTemplateEventArgs e)
        {
            var item = (ItemModel?)e.DataContext;
            if (item == null)
            {
                return;
            }

            e.TemplateKey = "defaultKey";
        }



        private void searchBoxKeyUp(object sender, KeyEventArgs e)
        {
            debouncedWrapper();
        }

    }
}
