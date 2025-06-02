using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Management.Automation.Runspaces;
using System.Threading;
using System.Threading.Tasks;

namespace Commandry
{
    public class Pwsh : IDisposable
    {
        private readonly Runspace _runspace;
        private readonly PowerShell _powerShell;
        private readonly ILogger? _logger;

        public ILogger? Logger => _logger;

        public Pwsh(Runspace runspace, ILogger? logger)
        {
            _runspace = runspace;
            _logger = logger;

            _powerShell = PowerShell.Create(runspace);
            _powerShell.Streams.Verbose.DataAdding += OnVerboseMessage;
            _powerShell.Streams.Debug.DataAdding += OnDebugMessage;
            _powerShell.Streams.Information.DataAdding += OnInformationMessage;
            _powerShell.Streams.Progress.DataAdding += OnProgressMessage;
            _powerShell.Streams.Warning.DataAdding += OnWarningMessage;
            _powerShell.Streams.Error.DataAdding += OnErrorMessage;
        }

        public void Dispose()
        {
            _powerShell.Streams.Verbose.DataAdding -= OnVerboseMessage;
            _powerShell.Streams.Debug.DataAdding -= OnDebugMessage;
            _powerShell.Streams.Information.DataAdding -= OnInformationMessage;
            _powerShell.Streams.Progress.DataAdding -= OnProgressMessage;
            _powerShell.Streams.Warning.DataAdding -= OnWarningMessage;
            _powerShell.Streams.Error.DataAdding -= OnErrorMessage;

            _powerShell.Dispose();
        }

        public IEnumerable<CommandInfo> GetCommands(string namePattern, CommandTypes commandTypes)
        {
            return _runspace.SessionStateProxy.InvokeCommand.GetCommands(namePattern, commandTypes, nameIsPattern: true);
        }

        public async IAsyncEnumerable<object?> InvokeCommandAsync(string command, IDictionary<object, object?> parameters)
        {
            bool locked = false;
            try
            {
                Monitor.Enter(_runspace, ref locked);

                _powerShell.AddCommand(command);
                foreach (var parameter in parameters?.AsEnumerable() ?? [])
                    _powerShell.AddParameter(parameter.Key.ToString(), parameter.Value);

                using PSDataCollection<PSObject> results = await _powerShell.InvokeAsync();
                if (_powerShell.HadErrors)
                    throw new PwshException { Errors = [.. _powerShell.Streams.Error] };

                foreach (var result in results)
                    yield return result?.BaseObject;
            }
            finally
            {
                if (locked)
                    Monitor.Exit(_runspace);
            }
        }

        public async Task<TCommandInfo?> DescribeCommandAsync<TCommandInfo>(string command)
            where TCommandInfo : CommandInfo
        {
            bool locked = false;
            try
            {
                Monitor.Enter(_runspace, ref locked);

                using PSDataCollection<PSObject> results = await _powerShell
                    .AddCommand($"Get-Command").AddArgument(command)
                    .InvokeAsync();
                if (_powerShell.HadErrors)
                    throw new PwshException { Errors = [.. _powerShell.Streams.Error] };

                return results
                    .Select(result => result.BaseObject)
                    .OfType<TCommandInfo>()
                    .FirstOrDefault();
            }
            finally
            {
                if (locked)
                    Monitor.Exit(_runspace);
            }
        }

        private void OnVerboseMessage(object? sender, DataAddingEventArgs e) => OnLogMessage(LogLevel.Trace, e.ItemAdded);
        private void OnDebugMessage(object? sender, DataAddingEventArgs e) => OnLogMessage(LogLevel.Debug, e.ItemAdded);
        private void OnInformationMessage(object? sender, DataAddingEventArgs e) => OnLogMessage(LogLevel.Information, e.ItemAdded);
        private void OnProgressMessage(object? sender, DataAddingEventArgs e) => OnLogMessage(LogLevel.Information, e.ItemAdded);
        private void OnWarningMessage(object? sender, DataAddingEventArgs e) => OnLogMessage(LogLevel.Warning, e.ItemAdded);
        private void OnErrorMessage(object? sender, DataAddingEventArgs e) => OnLogMessage(e.ItemAdded is Exception ? LogLevel.Critical : LogLevel.Error, e.ItemAdded);
        private void OnLogMessage(LogLevel level, object itemAdded)
        {
            if (Logger is not null)
            {
                if (itemAdded is Exception exception)
                    Logger.Log(level, 0, exception, "Unexpected error");
                else
                    Logger.Log(level, 0, default, itemAdded.ToString());
            }
        }
    }
}
