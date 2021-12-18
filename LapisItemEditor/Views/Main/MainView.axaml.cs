using Avalonia.ReactiveUI;
using LapisItemEditor.ViewModels;
using Avalonia.Markup.Xaml;
using ReactiveUI;
using LapisItemEditor.ViewModels.Main;

namespace LapisItemEditor.Views
{
    public class MainView : ReactiveUserControl<MainViewModel>
    {
        public MainView()
        {
            this.WhenActivated(disposables => { });
            AvaloniaXamlLoader.Load(this);
        }
    }
}