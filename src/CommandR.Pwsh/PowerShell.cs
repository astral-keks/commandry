using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Management.Automation;
using System.Management.Automation.Runspaces;
using System.Threading;
using System.Threading.Tasks;

namespace CommandR
{
    internal class PowerShell : IDisposable
    {
        private readonly System.Management.Automation.Runspaces.Runspace _runspace;
        private readonly System.Management.Automation.PowerShell _pwsh;
        private readonly Microsoft.Extensions.Logging.ILogger? _logger;

        public PowerShell(Runspace runspace, ILogger? logger)
        {
            _runspace = runspace;
            _logger = logger;

            _pwsh = System.Management.Automation.PowerShell.Create(runspace);
            _pwsh.Streams.Verbose.DataAdding += OnVerboseMessage;
            _pwsh.Streams.Debug.DataAdding += OnDebugMessage;
            _pwsh.Streams.Information.DataAdding += OnInformationMessage;
            _pwsh.Streams.Progress.DataAdding += OnProgressMessage;
            _pwsh.Streams.Warning.DataAdding += OnWarningMessage;
            _pwsh.Streams.Error.DataAdding += OnErrorMessage;
        }

        public void Dispose()
        {
            _pwsh.Streams.Verbose.DataAdding -= OnVerboseMessage;
            _pwsh.Streams.Debug.DataAdding -= OnDebugMessage;
            _pwsh.Streams.Information.DataAdding -= OnInformationMessage;
            _pwsh.Streams.Progress.DataAdding -= OnProgressMessage;
            _pwsh.Streams.Warning.DataAdding -= OnWarningMessage;
            _pwsh.Streams.Error.DataAdding -= OnErrorMessage;

            _pwsh.Dispose();
        }

        public async IAsyncEnumerable<object?> RunScriptAsync(FileInfo script, IDictionary<object, object?> parameters)
        {
            bool locked = false;
            try
            {
                Monitor.Enter(_runspace, ref locked);

                _pwsh.AddCommand(script.FullName);
                foreach (var parameter in parameters?.AsEnumerable() ?? [])
                    _pwsh.AddParameter(parameter.Key.ToString(), parameter.Value);

                using PSDataCollection<PSObject> results = await _pwsh.InvokeAsync();
                if (_pwsh.HadErrors)
                    throw new PowerShellCommandException { Errors = [.. _pwsh.Streams.Error] };

                foreach (var result in results.Select(result => result.BaseObject))
                    yield return result;
            }
            finally
            {
                if (locked)
                    Monitor.Exit(_runspace);
            }
        }

        public async Task<ExternalScriptInfo?> DescribeScriptAsync(FileInfo script)
        {
            bool locked = false;
            try
            {
                Monitor.Enter(_runspace, ref locked);

                using PSDataCollection<PSObject> results = await _pwsh
                    .AddCommand($"Get-Command").AddArgument(script.FullName)
                    .InvokeAsync();
                if (_pwsh.HadErrors)
                    throw new PowerShellCommandException { Errors = [.. _pwsh.Streams.Error] };

                return results
                    .Select(result => result.BaseObject)
                    .OfType<ExternalScriptInfo>()
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
            if (_logger is not null)
            {
                if (itemAdded is Exception exception)
                    _logger.Log(level, 0, exception, "Unexpected error");
                else
                    _logger.Log(level, 0, default, itemAdded.ToString());
            }
        }
    }
}
