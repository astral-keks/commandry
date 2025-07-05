namespace System.Management.Automation.DependencyInjection
{
    public static class ServiceProviderPSVariable
    {
        private static readonly string _variableName = "ServiceProvider";

        public static void SetServiceProvider(this PowerShell powerShell, IServiceProvider serviceProvider)
        {
            try
            {
                powerShell
                    .AddCommand("Set-Variable").AddArgument(_variableName).AddArgument(serviceProvider)
                    .Invoke();
            }
            finally
            {
                powerShell.Commands.Clear();
            }
        }

        public static IServiceProvider GetServiceProvider(this PSCmdlet cmdlet)
        {
            return (cmdlet.GetVariableValue(_variableName) as IServiceProvider)
                 ?? throw new InvalidOperationException($"${_variableName} is undefined or null");
        }
    }
}
