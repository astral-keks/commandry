using System.Management.Automation;
using System.Management.Automation.Language;

namespace Commandry.Functions
{
    internal class PwshFunctionCommand(PwshRunspace runspace, FunctionInfo function) : PwshCommand(runspace)
    {
        public override string Name { get; } = function.Name;

        public override async Task ExecuteAsync(CancellationToken cancellation)
        {
            await ExecuteAsync(function.Name, cancellation);
        }

        public override async Task<CommandMetadata> DescribeAsync(CancellationToken cancellation)
        {
            IEnumerable<ParameterMetadata> parametersMetadata = function.Parameters?.Values
                .Where(parameter => !parameter.IsCommon() || function.Definition.Contains($"${parameter.Name}"))
                ?? [];
            IEnumerable<PSTypeName> outputsMetadata = function.OutputType;
            CommentHelpInfo commentHelpInfo = (function.ScriptBlock.Ast as FunctionDefinitionAst)?.GetHelpContent() ?? new();

            return await DescribeAsync(parametersMetadata, outputsMetadata, commentHelpInfo, cancellation);
        }
    }
}
