using Microsoft.Extensions.Logging;
using Microsoft.PowerShell;
using System;
using System.IO;
using System.Management.Automation.Runspaces;
using System.Threading;

namespace Commandry
{
    public class PwshRunspace
    {
        private readonly InitialSessionState _initialSessionState;
        private readonly Lazy<Runspace> _runspace;

        public PwshRunspace(ApartmentState apartmentState = ApartmentState.STA)
        {
            _initialSessionState = InitialSessionState.CreateDefault();
            _initialSessionState.ExecutionPolicy = ExecutionPolicy.RemoteSigned;
            _initialSessionState.ThreadOptions = PSThreadOptions.UseCurrentThread;
            _initialSessionState.ApartmentState = apartmentState;

            _runspace = new(() =>
            {
                Runspace runspace = RunspaceFactory.CreateRunspace(_initialSessionState);
                runspace.Open();
                return runspace;
            });
        }

        public void ImportModule(string moduleName)
        {
            VerifyNotCreated();
            _initialSessionState.ImportPSModule(moduleName);
        }

        public void ImportModule(FileInfo moduleFile)
        {
            VerifyNotCreated();
            _initialSessionState.ImportPSModule(moduleFile.FullName);
        }

        public void SetVariable(string name, object value)
        {
            _runspace.Value.SessionStateProxy.SetVariable(name, value);
        }

        public Pwsh CreatePwsh(ILogger? logger = default)
        {
            return new(_runspace.Value, logger);
        }

        private void VerifyNotCreated()
        {
            if (_runspace.IsValueCreated)
                throw new InvalidRunspaceStateException("Runspace is already created");
        }
    }
}
