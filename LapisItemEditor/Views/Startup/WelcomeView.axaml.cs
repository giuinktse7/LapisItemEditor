using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using LapisItemEditor.ViewModels;
using ReactiveUI;

namespace LapisItemEditor.Views
{
    public partial class WelcomeView : ReactiveUserControl<WelcomeViewModel>
    {

        public WelcomeView()
        {
            InitializeComponent();

            this.WhenActivated(disposables =>
            {
            });
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

    }
}
