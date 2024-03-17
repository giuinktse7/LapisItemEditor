using Avalonia.ReactiveUI;
using ReactiveUI;
using LapisItemEditor.ViewModels.Main;

namespace LapisItemEditor.Views
{
    public partial class MainView : ReactiveUserControl<MainViewModel>
    {
        public MainView()
        {
            InitializeComponent();
            this.WhenActivated(disposables => { });
        }
    }
}