using System.Windows.Input;
using ReactiveUI;

namespace LapisItemEditor.ViewModels.Main
{
    public class MenuViewModel : ReactiveObject
    {
        public MenuViewModel()
        {

        }

        public ICommand CloseGameData { get; set; }
        public ICommand ExitProgram { get; set; }
    }
}