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
    internal class PowerShell(Runspace runspace) : IDisposable
    {
        private readonly System.Management.Automation.PowerShell _pwsh = System.Management.Automation.PowerShell.Create(runspace);

        public void Dispose() => _pwsh.Dispose();

        public async IAsyncEnumerable<object?> RunScriptAsync(FileInfo script, IDictionary<object, object?> parameters)
        {
            bool locked = false;
            try
            {
                Monitor.Enter(runspace, ref locked);

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
                    Monitor.Exit(runspace);
            }
        }

        public async Task<ExternalScriptInfo?> DescribeScriptAsync(FileInfo script)
        {
            bool locked = false;
            try
            {
                Monitor.Enter(runspace, ref locked);

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
                    Monitor.Exit(runspace);
            }
        }
    }
}
