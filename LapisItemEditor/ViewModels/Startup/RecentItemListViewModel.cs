using ReactiveUI;
using System.Collections.ObjectModel;

namespace LapisItemEditor.ViewModels.RecentItems
{
    public class RecentItemListViewModel : ViewModelBase
    {
        public ReactiveCommand<RecentDirectoryItem, RecentDirectoryItem> RecentItemSelected { get; }

        private ObservableCollection<RecentDirectoryItem> recentItems;
        public ObservableCollection<RecentDirectoryItem> RecentItems
        {
            get => recentItems;
            set => this.RaiseAndSetIfChanged(ref recentItems, value);
        }


        public void Add(RecentDirectoryItem item)
        {
            RecentItems.Add(item);
        }

        public RecentItemListViewModel()
        {
            recentItems = new ObservableCollection<RecentDirectoryItem>();


            RecentItemSelected = ReactiveCommand.Create<RecentDirectoryItem, RecentDirectoryItem>((context) => context);
        }
    }

    public class RecentDirectoryItem
    {
        public string Type { get; set; }
        public string Text { get; set; } = "";

        public RecentDirectoryItem() { }
        public RecentDirectoryItem(string type, string text)
        {
            this.Type = type;
            this.Text = text;
        }

    }

    public class RecentDirectoryFileItem : RecentDirectoryItem
    {
        public RecentDirectoryFileItem(string text) : base("File", text) { }
    };

    public class RecentDirectoryFolderItem : RecentDirectoryItem
    {
        public RecentDirectoryFolderItem(string text) : base("Folder", text) { }
    };
}