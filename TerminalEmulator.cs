using System.Diagnostics;
using System.Runtime.InteropServices;

namespace shiTTY
{
    public partial class TerminalEmulator
    {
        private readonly CancellationTokenSource _cts = new();

        private Process _process;

        public event EventHandler<DataRecievedEventArgs> CharactersReceived;

        public void SendInput(string input)
        {
            _process.StandardInput.WriteLine(input);
        }

        public void Start()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                _process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = "/bin/sh",
                        Arguments = "-i",
                        RedirectStandardInput = true,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        UseShellExecute = false,
                        Environment = { ["TERM"] = "xterm" }
                    }
                };
            }
            else
            {
                _process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = "cmd.exe",
                        UseShellExecute = false,
                        RedirectStandardInput = true,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true
                    }
                };
            }

            _process.Start();

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                _process.StandardInput.WriteLine("@echo on");
            }
            else
            {
                _process.StandardInput.WriteLine("PS1=");
            }

            // Start reading both streams character by character
            Task.Run(async () => await this.ReadStreamAsync(_process.StandardOutput, false, _cts.Token));
            Task.Run(async () => await this.ReadStreamAsync(_process.StandardError, true, _cts.Token));
        }

        public void Stop()
        {
            _cts.Cancel();
            _process?.Kill();
            _process?.Dispose();
            _cts.Dispose();
        }

        public async Task WaitForExit()
        {
            TaskCompletionSource<bool> tcs = new();
            _process.Exited += (sender, args) => tcs.SetResult(true);
            await tcs.Task;
        }

        private async Task ReadStreamAsync(StreamReader stream, bool isError, CancellationToken cancellationToken)
        {
            char[] buffer = new char[8096];

            while (!cancellationToken.IsCancellationRequested)
            {
                int bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);

                if (bytesRead == 0)
                {
                    break; // End of stream
                }

                CharactersReceived?.Invoke(this, new DataRecievedEventArgs(buffer.Take(bytesRead).ToArray(), isError));
            }
        }
    }
}