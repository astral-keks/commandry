using System.Management.Automation;
using System.Management.Automation.Language;

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
            IEnumerable<PSTypeName> outputsMetadata = scriptInfo?.OutputType ?? Enumerable.Empty<PSTypeName>();
            CommentHelpInfo commentHelpInfo = (scriptInfo?.ScriptBlock.Ast as ScriptBlockAst)?.GetHelpContent() ?? new();

            return await DescribeAsync(parametersMetadata, outputsMetadata, commentHelpInfo, cancellation);
        }
    }
}
