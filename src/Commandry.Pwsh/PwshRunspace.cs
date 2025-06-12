using Microsoft.Extensions.Logging;
using Microsoft.PowerShell;
using System.Management.Automation.Runspaces;
using System.Threading;

namespace Commandry
{
    public class PwshRunspace
    {
        private readonly Runspace _runspace;

        public PwshRunspace(ApartmentState apartmentState = ApartmentState.MTA)
        {
            InitialSessionState initialSessionState = InitialSessionState.CreateDefault();
            initialSessionState.ExecutionPolicy = ExecutionPolicy.RemoteSigned;
            initialSessionState.ThreadOptions = PSThreadOptions.UseCurrentThread;
            initialSessionState.ApartmentState = apartmentState;

            _runspace = RunspaceFactory.CreateRunspace(initialSessionState);
            _runspace.Open();
        }

        public void SetVariable(string name, object value)
        {
            _runspace.SessionStateProxy.SetVariable(name, value);
        }

        public Pwsh CreatePwsh(ILogger? logger = default)
        {
            return new(_runspace, logger);
        }
    }
}
