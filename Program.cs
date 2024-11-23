using RG35XX.Core.Drawing;
using RG35XX.Core.Fonts;
using RG35XX.Core.GamePads;
using RG35XX.Core.Interfaces;
using RG35XX.Libraries;
using RG35XX.Libraries.Controls;
using RG35XX.Libraries.Pages;
using System.Runtime.InteropServices;

namespace shiTTY
{
    internal class Program
    {
        private static readonly ConsoleRenderer _renderer = new(ConsoleFont.Px437_IBM_VGA_8x16);

        private static Application _application;

        private static IFrameBuffer _frameBuffer;

        private static TerminalEmulator _terminal;

        private static PictureBox _terminalPictureBox;

        private static async Task Main(string[] args)
        {
            _terminal = new();

            _renderer.Initialize(640, 480);
            _renderer.AutoFlush = false;

            _frameBuffer = _renderer.FrameBuffer;

            _terminal.CharactersReceived += Terminal_CharacterReceived;

            Page terimalPage = new();
            _terminalPictureBox = new()
            {
                Bounds = new Bounds(0, 0, 1, 1),
                IsSelectable = true
            };
            _terminalPictureBox.OnKeyPressed += OnTerminalKeyPressed;
            terimalPage.AddControl(_terminalPictureBox);
            _application = new(_frameBuffer);
            _application.OpenPage(terimalPage);
            _terminal.Start();

            Task aTask = _application.Execute();
            Task tTask = _terminal.WaitForExit();

            await Task.WhenAny(aTask, tTask);
        }

        private static void OnTerminalKeyPressed(object? sender, GamepadKey e)
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

        private static readonly object _lock = new();

        private static void Terminal_CharacterReceived(object? sender, DataRecievedEventArgs e)
        {
            lock (_lock)
            {
                foreach (char c in e.Data)
                {
                    _renderer.Write(c, e.IsError ? Color.Red : Color.White);
                }

                Bitmap bitmap = _renderer.Render();
                _terminalPictureBox.Image = bitmap;
            }
        }
    }
}