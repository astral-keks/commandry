using Commandry.Schemas;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Management.Automation.Language;
using System.Threading;
using System.Threading.Tasks;

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
            IEnumerable<ParameterMetadata> parametersMetadata = function?.Parameters?.Values
                .Where(parameter => !parameter.IsCommon() || function.Definition.Contains($"${parameter.Name}"))
                ?? [];
            CommentHelpInfo commentHelpInfo = (function?.ScriptBlock.Ast as FunctionDefinitionAst)?.GetHelpContent() ?? new();
            return await DescribeAsync(parametersMetadata, commentHelpInfo, cancellation);
        }
    }
}
