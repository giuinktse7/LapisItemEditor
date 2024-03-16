using System;
using System.Diagnostics;
using System.Windows.Input;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using LapisItemEditor.ViewModels.Main;
using ReactiveUI;

namespace LapisItemEditor.ViewModels
{
    public class MainWindowViewModel : ViewModelBase, IScreen
    {
        // The Router associated with this Screen.
        // Required by the IScreen interface.
        public RoutingState Router { get; } = new RoutingState();

        MainViewModel mainViewModel;
        WelcomeViewModel welcomeViewModel;

        MenuViewModel menuViewModel { get; } = new();

        public MainWindowViewModel()
        {
            mainViewModel = new MainViewModel(this);
            CreateWelcomeView();

            if (welcomeViewModel != null)
            {
                Router.Navigate.Execute(welcomeViewModel);

                menuViewModel.CloseGameData = ReactiveCommand.Create(() =>
                {
                    Backend.Backend.Reset();

                    CreateWelcomeView();

                    Router.Navigate.Execute(welcomeViewModel);
                    mainViewModel = new MainViewModel(this);
                });

                menuViewModel.ExitProgram = ReactiveCommand.Create(() =>
                {
                    if (Avalonia.Application.Current.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
                    {
                        desktop.Shutdown();
                    }
                });
            }
        }

        private void CreateWelcomeView()
        {
            welcomeViewModel = new WelcomeViewModel(this);
            welcomeViewModel.WhenAnyValue(x => x.IsFinished)
                .Subscribe(finished =>
                {
                    if (finished)
                    {
                        var hasOtb = welcomeViewModel.SelectedOtbPath != null || welcomeViewModel.UseNewItemsOtb;
                        if (welcomeViewModel.GameDataConfig == null || !hasOtb)
                        {
                            throw new ApplicationException("Should be impossible.");
                        }

                        Router.Navigate.Execute(mainViewModel);
                        mainViewModel.Load(welcomeViewModel.GameDataConfig, welcomeViewModel.SelectedOtbPath);
                    }
                });
        }



        private string infoMessage = "Ready.";
        public string InfoMessage { get => infoMessage; set => this.RaiseAndSetIfChanged(ref infoMessage, value); }
    }
}
