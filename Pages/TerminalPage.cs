using RG35XX.Core.GamePads;
using RG35XX.Libraries;
using RG35XX.Libraries.Controls;
using RG35XX.Libraries.Pages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace shiTTY.Pages
{
    internal class TerminalPage(Application application, ConsoleRenderer renderer, TerminalEmulator terminal) : Page
    {
        private readonly Application _application = application ?? throw new ArgumentNullException(nameof(application));

        private readonly ConsoleRenderer _renderer = renderer ?? throw new ArgumentNullException(nameof(renderer));

        private readonly TerminalEmulator _terminal = terminal ?? throw new ArgumentNullException(nameof(terminal));

        public override void OnKey(GamepadKey e)
        {
            if (e is GamepadKey.MENU_DOWN)
            {
                OnScreenKeyboard keyboard = new(string.Empty);
                _application.OpenPage(keyboard);
                keyboard.OnClosing += (s, e) =>
                {
                    string? toSend = keyboard.Value;

                    if (string.IsNullOrEmpty(toSend))
                    {
                        return;
                    }

                    if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                    {
                        _renderer.WriteLine(toSend);
                        toSend += "; echo -n \"$(pwd)# \"";
                    }

                    _terminal.SendInput(toSend);
                };
            }
        }
    }
}
