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

        public PausePopup.Model GetPausePopupModel(Action onClickResume, Action onClickReload, Action onClickUpgrade)
        {
            return new(onClickResume, onClickReload, onClickUpgrade, _windowsService);
        }
    }
}