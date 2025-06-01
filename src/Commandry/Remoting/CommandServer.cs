using Commandry.Hosting;
using System;
using System.IO;
using System.IO.Pipes;
using System.Threading;
using System.Threading.Tasks;

namespace Commandry.Remoting
{
    public sealed class CommandServer : IDisposable
    {
        private readonly string _name;
        private readonly CommandHost _host;
        private readonly bool _isLocal;
        private readonly Mutex _mutex;
        private readonly CommandErrorHandler? _errorHandler;
        private readonly CommandSerializer _serializer;
        private readonly CancellationTokenSource _cancellation;
        private Task? _listener;

        public CommandServer(string serverName, CommandSerializer serializer, CommandErrorHandler? errorHandler, CommandHost host)
        {
            _name = serverName;
            _serializer = serializer;
            _host = host;
            _errorHandler = errorHandler;
            _cancellation = new();
            _mutex = new Mutex(true, MutexName, out _isLocal);
        }

        public void Dispose()
        {
            if (_isLocal)
            {
                _mutex.ReleaseMutex();
                _mutex.Dispose();
            }

            _cancellation.Cancel();

            if (_listener is not null)
            {
                _listener.Wait();
                _listener.Dispose();
            }
        }

        public string Name => _name;

        public bool IsRemote => !_isLocal;

        public Command ConnectCommand(CommandIntent intent) => new CommandClient(intent, this);

        public async Task SendCommandAsync(CommandIntent intent, CancellationToken cancellation)
        {
            using NamedPipeClientStream stream = new(PipeName);
            using StreamWriter writer = new(stream, leaveOpen: true);

            await stream.ConnectAsync(1000, cancellation);

            _serializer.Serialize(writer, intent);
            await writer.FlushAsync(cancellation);
        }

        public void ReceiveCommands() => _listener = Task.Run(ReceiveCommandsAsync);

        private async Task ReceiveCommandsAsync()
        {
            try
            {
                while (!_cancellation.IsCancellationRequested)
                {
                    using NamedPipeServerStream stream = new(PipeName);

                    Task waiting = stream.WaitForConnectionAsync(_cancellation.Token);
                    while (!waiting.IsCompleted && !_cancellation.IsCancellationRequested)
                        await Task.Delay(1000, _cancellation.Token);
                    await waiting;

                    using StreamReader reader = new(stream, leaveOpen: true);
                    CommandIntent? intent = _serializer.Deserialize(reader);
                    if (intent is not null)
                    {
                        Command? command = _host.GetCommand(intent);
                        if (command is not null)
                            await command.ExecuteAsync(_cancellation.Token);
                    }
                }
            }
            catch (TaskCanceledException)
            {
            }
            catch (Exception e)
            {
                _errorHandler?.Invoke(e);
            }
        }

        private string MutexName => $"AppR.Mutex.{_name}";

        private string PipeName => $"AppR.Pipe.{_name}";
    }
}
