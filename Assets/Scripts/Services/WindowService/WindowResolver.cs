using System;
using Windows;

namespace Services.WindowService
{
    public class WindowResolver
    {
        private readonly WindowsService _windowsService;

        public WindowResolver(WindowsService windowsService)
        {
            _windowsService = windowsService;
        }

        public PausePopup.Model GetPlayConfirmWindowModel(Action onClick)
        {
            return new(onClick, _windowsService);
        }
    }
}