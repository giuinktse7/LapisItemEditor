using System.Reactive.Linq;
using System.Windows.Input;
using ReactiveUI;
using System.Diagnostics;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using System.IO;
using Backend.Tibia11;
using Backend.Tibia7;
using Backend;
using System.Runtime.Serialization;
using System.Collections.ObjectModel;
using LapisItemEditor.ViewModels.RecentItems;
using System.Text.Json;
using System.Text.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Collections;
using System.Threading.Tasks;
using System.Reactive;

namespace LapisItemEditor.ViewModels
{
    [DataContract]
    public class WelcomeViewModel : ViewModelBase, IRoutableViewModel
    {
        private class RecentItem
        {
            [JsonPropertyName("timestamp")]
            public DateTime Timestamp { get; set; }

            [JsonPropertyName("text")]
            public string Text { get; set; } = "";
        }



        private RecentFileStore recentFileStore = new RecentFileStore();

        // Reference to IScreen that owns the routable view model.
        public IScreen HostScreen { get; }

        // Unique identifier for the routable view model.
        public string UrlPathSegment { get; } = "WelcomeViewModel";

        private GameDataConfig? gameDataConfig;
        public GameDataConfig? GameDataConfig { get => gameDataConfig; set => this.RaiseAndSetIfChanged(ref gameDataConfig, value); }

        public RecentItemListViewModel recentAssetFolders { get; set; } = new();
        public RecentItemListViewModel RecentOtbFiles { get; set; } = new();

        private GameDataConfig? CreateTibia11Config(string clientFolder)
        {
            bool hasCatalogContent = File.Exists(Path.Combine(clientFolder, "catalog-content.json"));

            if (hasCatalogContent)
            {
                var version = new Tibia11VersionData(Backend.ClientVersion.V12_71);
                return new Tibia11GameDataConfig(version, clientFolder);
            }
            else
            {
                return null;
            }
        }

        private GameDataConfig? CreateTibia7Config(string clientFolder)
        {
            string datPath = Path.Combine(clientFolder, "tibia.dat");
            string sprPath = Path.Combine(clientFolder, "tibia.spr");

            bool hasDat = File.Exists(datPath);
            bool hasSpr = File.Exists(sprPath);

            if (hasDat && hasSpr)
            {
                var version = new Tibia7VersionData(Backend.ClientVersion.V10_93, "Client 12.70 (Downgraded)", 0x4A10, 0x59E48E02);
                return new Tibia7GameDataConfig(version, datPath, sprPath);
            }
            else
            {
                return null;
            }
        }

        private void GetClientFiles(string clientFolder)
        {
            var config = CreateTibia11Config(clientFolder) ?? CreateTibia7Config(clientFolder);

            if (config == null)
            {
                BadAssetPath = clientFolder;
                return;
            }

            SelectedTibiaAssetsPath = clientFolder;
            GameDataConfig = config;

            var otbPath = Path.Combine(clientFolder, "items.otb");
            SetOtbPath(otbPath);
        }

        private void SetOtbPath(string path)
        {
            if (File.Exists(path))
            {
                SelectedOtbPath = path;
            }
        }

        private void LoadRecentItems()
        {
            recentFileStore = RecentFileStore.LoadFromFile("./recent.json");

            if (recentFileStore != null)
            {
                foreach (var recentPath in recentFileStore.RecentAssetFolders)
                {
                    recentAssetFolders.Add(new RecentDirectoryFolderItem(recentPath.Path));
                }

                foreach (var recentPath in recentFileStore.RecentOtbFiles)
                {
                    RecentOtbFiles.Add(new RecentDirectoryFileItem(recentPath.Path));
                }
            }
        }

        public WelcomeViewModel()
        {
        }

        private async Task<string?> ShowSelectFolderWindow(string title)
        {
            if (Avalonia.Application.Current.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                var dialog = new OpenFolderDialog() { Title = title };
                var result = await dialog.ShowAsync(desktop.MainWindow);
                return result;
            }

            return null;
        }

        private async Task<string[]?> ShowSelectFileWindow(string title)
        {
            if (Avalonia.Application.Current.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                var dialog = new OpenFileDialog() { AllowMultiple = false, Title = title };
                var result = await dialog.ShowAsync(desktop.MainWindow);
                return result;
            }

            return null;
        }


        public WelcomeViewModel(IScreen screen)
        {
            LoadRecentItems();
            HostScreen = screen;

            _hasGameDataConfig = this.WhenAnyValue(x => x.GameDataConfig)
                                        .Select(config => config != null)
                                        .ToProperty(this, x => x.HasGameDataConfig);

            _hasOtb = this.WhenAnyValue(
                    x => x.SelectedOtbPath,
                    x => x.UseNewItemsOtb,
                    (otbPath, newItemsOtb) => otbPath != null || newItemsOtb)
                .ToProperty(this, x => x.HasOtb);

            _hasBadAssetPath = this.WhenAnyValue(x => x.BadAssetPath)
                .Select(badAssetPath => badAssetPath != null)
                .ToProperty(this, x => x.HasBadAssetPath);

            _isFinished = this.WhenAnyValue(
                x => x.HasGameDataConfig,
                x => x.HasOtb,
                (hasConfig, hasOtb) => hasConfig && hasOtb)
            .ToProperty(this, x => x.IsFinished);

            recentAssetFolders.RecentItemSelected.Subscribe(context =>
            {
                GetClientFiles(context.Text);
            });

            RecentOtbFiles.RecentItemSelected.Subscribe(context =>
            {
                SetOtbPath(context.Text);
            });

            ChooseClientFolder = ReactiveCommand.Create(async () =>
            {
                BadAssetPath = null;
                var result = await ShowSelectFolderWindow("Select Tibia asset folder");
                if (result != null && Directory.Exists(result))
                {
                    GetClientFiles(result);
                }
            });

            ChooseItemsOtbFile = ReactiveCommand.Create(async () =>
            {
                var paths = await ShowSelectFileWindow("Select items.otb");
                if (paths != null)
                {
                    var path = paths[0];
                    SetOtbPath(path);
                }
            });

            UseNewItemsOtbCommand = ReactiveCommand.Create(() =>
            {
                UseNewItemsOtb = true;
            });

            OpenInFileBrowserCommand = ReactiveCommand.Create<string, Unit>((directory) =>
            {
                if (Directory.Exists(directory))
                {
                    OpenFileBrowser.AtPath(directory);
                }

                return Unit.Default;
            });

            this.WhenAnyValue(x => x.IsFinished).Subscribe(finished =>
            {
                if (finished)
                {
                    var now = DateTime.Now;

                    if (SelectedTibiaAssetsPath != null)
                    {
                        recentFileStore.AddAssetFolder(now, SelectedTibiaAssetsPath);
                    }

                    if (SelectedOtbPath != null)
                    {
                        recentFileStore.AddOtbFile(now, SelectedOtbPath);
                    }

                    recentFileStore.SaveToFile("./recent.json");
                }
            });
        }

        private readonly ObservableAsPropertyHelper<bool> _hasBadAssetPath;
        public bool HasBadAssetPath => _hasBadAssetPath?.Value ?? false;

        private string? badAssetPath;
        public string? BadAssetPath { get => badAssetPath; set => this.RaiseAndSetIfChanged(ref badAssetPath, value); }

        readonly ObservableAsPropertyHelper<bool> _isFinished;
        public bool IsFinished { get { return _isFinished?.Value ?? false; } }

        readonly ObservableAsPropertyHelper<bool> _hasGameDataConfig;
        public bool HasGameDataConfig { get { return _hasGameDataConfig?.Value ?? false; } }

        private string? selectedTibiaAssetPath;
        public string? SelectedTibiaAssetsPath { get => selectedTibiaAssetPath; set => this.RaiseAndSetIfChanged(ref selectedTibiaAssetPath, value); }

        private string? selectedOtbPath;
        public string? SelectedOtbPath { get => selectedOtbPath; set => this.RaiseAndSetIfChanged(ref selectedOtbPath, value); }

        private bool useNewItemsOtb = false;
        public bool UseNewItemsOtb { get => useNewItemsOtb; private set => this.RaiseAndSetIfChanged(ref useNewItemsOtb, value); }

        public ICommand ChooseClientFolder { get; }
        public ICommand ChooseItemsOtbFile { get; }
        public ReactiveCommand<string, Unit> OpenInFileBrowserCommand { get; }

        private readonly ObservableAsPropertyHelper<bool> _hasOtb;
        public bool HasOtb => _hasOtb?.Value ?? false;
    }


}
