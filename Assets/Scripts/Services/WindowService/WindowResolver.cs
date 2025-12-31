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

        public PausePopup.Model GetPausePopupModel(Action onClickReload, Action onClickUpgrade)
        {
            return new(onClickReload, onClickUpgrade, _windowsService);
        }
    }
}