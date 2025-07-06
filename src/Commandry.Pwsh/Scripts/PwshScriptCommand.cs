using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Management.Automation;
using System.Management.Automation.Language;
using System.Threading;
using System.Threading.Tasks;

namespace Commandry.Scripts
{
    internal class PwshScriptCommand(PwshRunspace runspace, FileInfo script) : PwshCommand(runspace)
    {
        public override string Name { get; } = Path.GetFileNameWithoutExtension(script.Name);

        public override async Task ExecuteAsync(CancellationToken cancellation)
        {
            await ExecuteAsync(script.FullName, cancellation);
        }

        public override async Task<CommandMetadata> DescribeAsync(CancellationToken cancellation)
        {
            using Pwsh pwsh = CreatePwsh();

            ExternalScriptInfo? scriptInfo = pwsh.GetCommand<ExternalScriptInfo>(script.FullName);

            IEnumerable<ParameterMetadata> parametersMetadata = scriptInfo?.Parameters?.Values
                .Where(parameter => !parameter.IsCommon() || scriptInfo.ScriptContents.Contains($"${parameter.Name}"))
                ?? [];
            CommentHelpInfo commentHelpInfo = (scriptInfo?.ScriptBlock.Ast as ScriptBlockAst)?.GetHelpContent() ?? new();

            return await DescribeAsync(parametersMetadata, commentHelpInfo, cancellation);
        }
    }
}
