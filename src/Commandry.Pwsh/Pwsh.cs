using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Management.Automation;
using System.Management.Automation.Runspaces;
using System.Threading;

namespace Commandry
{
    public class Pwsh : IDisposable
    {
        private readonly Runspace _runspace;
        private readonly PowerShell _powerShell;
        private readonly PwshTracker? _tracker;
        private readonly ILogger? _logger;

        public Pwsh(Runspace runspace, PwshTracker? tracker, ILogger? logger)
        {
            _runspace = runspace;
            _tracker = tracker;
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

        public PSModuleInfo? ImportModule(string moduleNameOrPath)
        {
            PSModuleInfo? result = default;

            Invoke(() =>
            {
                result = _powerShell
                    .AddCommand("Import-Module").AddArgument(moduleNameOrPath).AddParameter("PassThru")
                    .Invoke()
                    .Select(result => result.BaseObject)
                    .OfType<PSModuleInfo>()
                    .FirstOrDefault();
            });

            return result;
        }

        public void RemoveModule(string moduleName)
        {
            Invoke(() =>
            {
                _powerShell
                    .AddCommand("Remove-Module").AddArgument(moduleName)
                    .Invoke();
            });
        }

        public List<PSModuleInfo> GetModules()
        {
            List<PSModuleInfo> results = [];

            Invoke(() =>
            {
                results.AddRange(
                    _powerShell
                        .AddCommand("Get-Module")
                        .Invoke()
                        .Select(result => result.BaseObject)
                        .OfType<PSModuleInfo>());
            });

            return results;
        }

        public void SetVariable(object variableName, object? variableValue)
        {
            Invoke(() =>
            {
                _powerShell
                    .AddCommand("Set-Variable").AddArgument(variableName).AddArgument(variableValue)
                    .Invoke();
            });
        }

        public List<object?> InvokeCommand(string command, IDictionary<object, object?> parameters)
        {
            List<object?> results = [];

            Invoke(() =>
            {
                _powerShell.AddCommand(command);
                foreach (var parameter in parameters?.AsEnumerable() ?? [])
                    _powerShell.AddParameter(parameter.Key.ToString(), parameter.Value);

                Collection<PSObject> records = _powerShell.Invoke();
                if (_powerShell.HadErrors)
                    throw new PwshException { Errors = [.. _powerShell.Streams.Error] };

                foreach (var record in records)
                    results.Add(record?.BaseObject);
            });

            return results;
        }

        public TCommandInfo? GetCommand<TCommandInfo>(string command)
            where TCommandInfo : CommandInfo
        {
            TCommandInfo? result = default;

            Invoke(() =>
            {
                Collection<PSObject> results = _powerShell
                    .AddCommand($"Get-Command").AddArgument(command)
                    .Invoke();
                if (_powerShell.HadErrors)
                    throw new PwshException { Errors = [.. _powerShell.Streams.Error] };

                result = results
                    .Select(result => result.BaseObject)
                    .OfType<TCommandInfo>()
                    .FirstOrDefault();
            });

            return result;
        }

        public TResult WithRunspace<TResult>(Func<TResult> operation)
        {
            Runspace currentRunspace = Runspace.DefaultRunspace;

            try
            {
                Runspace.DefaultRunspace = _runspace;
                return operation();
            }
            finally
            {
                Runspace.DefaultRunspace = currentRunspace;
            }
        }

        private void Invoke(Action action)
        {
            bool locked = false;
            try
            {
                Monitor.Enter(_runspace, ref locked);
                action();
            }
            finally
            {
                _powerShell.Commands.Clear();
                if (locked)
                    Monitor.Exit(_runspace);
            }
        }

        private void OnVerboseMessage(object? sender, DataAddingEventArgs e) => OnLogMessage(LogLevel.Trace, e.ItemAdded);
        private void OnDebugMessage(object? sender, DataAddingEventArgs e) => OnLogMessage(LogLevel.Debug, e.ItemAdded);
        private void OnInformationMessage(object? sender, DataAddingEventArgs e) => OnLogMessage(LogLevel.Information, e.ItemAdded);
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

        private void OnProgressMessage(object? sender, DataAddingEventArgs e)
        {
            if (_tracker is not null && e.ItemAdded is ProgressRecord progress)
            {
                _tracker(progress);
            }
            OnLogMessage(LogLevel.Information, e.ItemAdded);
        }
    }
}
