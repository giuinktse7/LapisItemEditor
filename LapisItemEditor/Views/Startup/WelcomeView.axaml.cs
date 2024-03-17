using Avalonia.ReactiveUI;
using LapisItemEditor.ViewModels;

namespace LapisItemEditor.Views
{
    public partial class WelcomeView : ReactiveUserControl<WelcomeViewModel>
    {
        public WelcomeView()
        {
            InitializeComponent();
        }
    }
}
