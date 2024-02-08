using Avalonia.Controls;
using Avalonia.ReactiveUI;
using LapisItemEditor.ViewModels.RecentItems;

namespace LapisItemEditor.Views.RecentItems
{
    public partial class RecentItemListView : ReactiveUserControl<RecentItemListViewModel>
    {
        public RecentItemListView()
        {
            InitializeComponent();
        }

        public void OnSelectTemplateKey(object sender, SelectTemplateEventArgs e)
        {
            var item = (RecentDirectoryItem?)e.DataContext;
            if (item == null)
            {
                return;
            }

            if (item is RecentDirectoryFileItem)
            {
                e.TemplateKey = "file";
            }
            else if (item is RecentDirectoryFolderItem)
            {
                e.TemplateKey = "folder";
            }
            else
            {
                e.TemplateKey = "defaultKey";
            }
        }
    }
}
