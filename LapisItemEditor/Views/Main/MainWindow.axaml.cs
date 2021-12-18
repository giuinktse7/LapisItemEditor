using System;
using System.Diagnostics;
using System.IO;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using Backend.Tibia11;
using Backend.Tibia7;
using LapisItemEditor.ViewModels;
using ReactiveUI;

namespace LapisItemEditor.Views
{
    public class MainWindow : ReactiveWindow<MainWindowViewModel>
    {
        public void WindowPressed(object sender, PointerPressedEventArgs args)
        {
            FocusManager.Instance.Focus(null);
        }

        public MenuItem menuItemEdit => this.FindControl<MenuItem>("MenuItemEdit");

        Tibia7GameData LoadTibia7()
        {

            string datPath = @"./tibia.dat";
            string sprPath = @"./tibia.spr";
            // var version = new Backend.Tibia7.Version(1079, "Client 10.79", 0x3A71, 0x557A5E34, 0);
            var version = new Tibia7VersionData(Backend.ClientVersion.V10_93, "Client 12.70 (Downgraded)", 0x4A10, 0x59E48E02);

            return new Tibia7GameData(version, datPath, sprPath);
        }

        Tibia11GameData LoadTibia11()
        {
            string assetDirectory = @"D:/Programs/TibiaLatest/packages/TibiaExternal/assets";

            var version = new Tibia11VersionData(Backend.ClientVersion.V12_71);
            return new Tibia11GameData(version, assetDirectory);
        }

        public MainWindow()
        {
            InitializeComponent();

            this.WhenActivated(disposables => { });

            Config.LoadFromFile("./data/config.json");
        }


        private void InitializeComponent()
        {
            this.WhenActivated(disposables => { });
            AvaloniaXamlLoader.Load(this);
        }


    }
}