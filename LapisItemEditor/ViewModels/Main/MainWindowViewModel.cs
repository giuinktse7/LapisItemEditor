using System;
using Avalonia.Controls.ApplicationLifetimes;
using LapisItemEditor.ViewModels.Main;
using ReactiveUI;
using Avalonia.Threading;
using System.Threading.Tasks;

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
                    Progress = 0;
                    InfoMessage = "Ready.";
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

                        Dispatcher.UIThread.InvokeAsync(() => InfoMessage = "Loading game data...");
                        Task.Run(() =>
                        {
                            mainViewModel.Load(welcomeViewModel.GameDataConfig, welcomeViewModel.SelectedOtbPath);
                            Dispatcher.UIThread.InvokeAsync(() => Router.Navigate.Execute(mainViewModel));
                        });
                    }
                });

            this.WhenAnyValue(x => x.Progress)
            .Subscribe(progress =>
            {
                if (_previousProgress == 0 && progress > 0)
                {
                    IsLoading = true;
                }

                if (progress == 100)
                {
                    IsLoading = false;
                    progress = 0;
                }
                _previousProgress = progress;
            });
        }


        private string infoMessage = "Ready.";
        public string InfoMessage { get => infoMessage; set => this.RaiseAndSetIfChanged(ref infoMessage, value); }

        private int _progress = 0;
        public int Progress { get => _progress; set => this.RaiseAndSetIfChanged(ref _progress, value); }

        private bool _isLoading = false;
        public bool IsLoading { get => _isLoading; private set => this.RaiseAndSetIfChanged(ref _isLoading, value); }

        private int _previousProgress = 0;
    }
}
