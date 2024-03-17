using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using LapisItemEditor.ViewModels.ItemProperties;
using ReactiveUI;

namespace LapisItemEditor.Views.ItemProperties
{
    public partial class ItemPropertiesView : ReactiveUserControl<ItemPropertiesViewModel>
    {
        public ItemPropertiesView()
        {
            InitializeComponent();
        }

    }
}
