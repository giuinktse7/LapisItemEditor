using Avalonia;
using Avalonia.ReactiveUI;
using LapisItemEditor.ViewModels;
using LapisItemEditor.ViewModels.Main;
using LapisItemEditor.Views;
using ReactiveUI;
using Splat;

namespace LapisItemEditor
{
    class Program
    {
        // Initialization code. Don't use any Avalonia, third-party APIs or any
        // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
        // yet and stuff might break.
        public static void Main(string[] args) =>
            BuildAvaloniaApp()
            .StartWithClassicDesktopLifetime(args);


        // Avalonia configuration, don't remove; also used by visual designer.
        public static AppBuilder BuildAvaloniaApp()
        {
            Locator.CurrentMutable.Register(() => new MainView(), typeof(IViewFor<MainViewModel>));
            Locator.CurrentMutable.Register(() => new WelcomeView(), typeof(IViewFor<WelcomeViewModel>));

            return AppBuilder.Configure<App>()
                  .UsePlatformDetect()
                  .LogToTrace()
                  .UseReactiveUI();
        }
    }
}