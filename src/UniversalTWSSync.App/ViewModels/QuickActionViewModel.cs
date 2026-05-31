using System.Windows.Input;

namespace UniversalTWSSync.App.ViewModels
{
    public sealed class QuickActionViewModel
    {
        public string Title { get; set; }

        public string Subtitle { get; set; }

        public string Glyph { get; set; }

        public ICommand Command { get; set; }
    }
}
