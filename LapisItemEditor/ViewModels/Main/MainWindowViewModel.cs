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

        public MainWindowViewModel()
        {
            mainViewModel = new MainViewModel(this);
            CreateWelcomeView();

            if (welcomeViewModel != null)
            {
                Router.Navigate.Execute(welcomeViewModel);

                CloseGameData = ReactiveCommand.Create(() =>
                {
                    Backend.Backend.Reset();

                    CreateWelcomeView();

                    Router.Navigate.Execute(welcomeViewModel);
                    mainViewModel = new MainViewModel(this);
                });

                ExitProgram = ReactiveCommand.Create(() =>
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
                        if (welcomeViewModel.GameDataConfig == null || welcomeViewModel.SelectedOtbPath == null)
                        {
                            throw new ApplicationException("Should be impossible.");
                        }

                        Router.Navigate.Execute(mainViewModel);
                        mainViewModel.Load(welcomeViewModel.GameDataConfig, welcomeViewModel.SelectedOtbPath);
                    }
                });
        }

        public ICommand CloseGameData { get; }
        public ICommand ExitProgram { get; }

        private string infoMessage = "Ready.";
        public string InfoMessage { get => infoMessage; set => this.RaiseAndSetIfChanged(ref infoMessage, value); }
    }
}
