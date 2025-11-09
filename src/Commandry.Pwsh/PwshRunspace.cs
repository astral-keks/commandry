using Microsoft.Extensions.Logging;
using Microsoft.PowerShell;
using System.Management.Automation;
using System.Management.Automation.Runspaces;

namespace Commandry
{
    public class PwshRunspace
    {
        private readonly Runspace _runspace;

        public PwshRunspace(string[] initialModules)
        {
            InitialSessionState initialSessionState = InitialSessionState.CreateDefault();
            initialSessionState.ImportPSModule(initialModules);
            initialSessionState.ExecutionPolicy = ExecutionPolicy.RemoteSigned;
            initialSessionState.ThreadOptions = PSThreadOptions.UseCurrentThread;
            initialSessionState.ApartmentState = Thread.CurrentThread.GetApartmentState();

            _runspace = RunspaceFactory.CreateRunspace(initialSessionState);
            _runspace.Open();
        }

        public void SetVariable(string name, object value)
        {
            _runspace.SessionStateProxy.SetVariable(name, value);
        }

        public Pwsh CreatePwsh(Action<ProgressRecord>? progress = default, ILogger? logger = default)
        {
            return new(_runspace, progress, logger);
        }
    }
}
