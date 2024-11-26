using RG35XX.Core.Drawing;
using RG35XX.Core.Fonts;
using RG35XX.Core.GamePads;
using RG35XX.Core.Interfaces;
using RG35XX.Libraries;
using RG35XX.Libraries.Controls;
using RG35XX.Libraries.Pages;
using shiTTY.Pages;
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

            try
            {
                _frameBuffer = _renderer.FrameBuffer;

                _terminal.CharactersReceived += Terminal_CharacterReceived;

                _application = new(_frameBuffer);

                TerminalPage terimalPage = new(_application, _renderer, _terminal);
               
                _terminalPictureBox = new()
                {
                    Bounds = new Bounds(0, 0, 1, 1)
                };

                terimalPage.AddControl(_terminalPictureBox);
                _application.Execute();
                _application.OpenPage(terimalPage);
                _terminal.Start();

                Task tTask = _terminal.WaitForExit();

                await Task.WhenAny(_application.WaitForClose(), tTask);
            }
            catch (Exception ex)
            {
                _renderer.WriteLine(ex.Message, Color.Red);
                _renderer.WriteLine(ex.StackTrace, Color.Red);
                _renderer.WriteLine("Exiting in 15 seconds...", Color.Green);
                _renderer.Flush();

                System.Threading.Thread.Sleep(15000);
                System.Environment.Exit(1);
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