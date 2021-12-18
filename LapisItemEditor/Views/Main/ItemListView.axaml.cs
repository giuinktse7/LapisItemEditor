using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using LapisItemEditor.ViewModels;
using static LapisItemEditor.ViewModels.ItemListViewModel;

namespace LapisItemEditor.Views
{
    public partial class ItemListView : ReactiveUserControl<WelcomeViewModel>
    {
        public ItemListView()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
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
    }
}
