using Avalonia.ReactiveUI;
using Avalonia.Markup.Xaml;
using ReactiveUI;
using LapisItemEditor.ViewModels.Main;

namespace LapisItemEditor.Views
{
    public class MenuView : ReactiveUserControl<MenuViewModel>
    {
        public MenuView()
        {
            this.WhenActivated(disposables => { });
            AvaloniaXamlLoader.Load(this);
        }
    }
}