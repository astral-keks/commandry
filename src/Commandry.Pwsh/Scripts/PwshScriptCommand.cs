using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Management.Automation;
using System.Management.Automation.Language;
using System.Threading;
using System.Threading.Tasks;

namespace Commandry.Scripts
{
    internal class PwshScriptCommand(PwshRunspace runspace, FileInfo script) : PwshCommand
    {
        public override string Name { get; } = Path.GetFileNameWithoutExtension(script.Name);

        public override async Task ExecuteAsync(CancellationToken cancellation)
        {
            using Pwsh pwsh = runspace.CreatePwsh(ReportProgress, Logger);

            foreach (var kv in Parameters)
                pwsh.SetVariable(kv.Key, kv.Value);

            List<object?> results = [];
            foreach (var result in pwsh.InvokeCommand(script.FullName, Parameters))
                results.Add(result);

            Result = new()
            {
                Records = results
            };
        }

        public override async Task<CommandMetadata> DescribeAsync(CancellationToken cancellation)
        {
            using Pwsh pwsh = runspace.CreatePwsh(ReportProgress, Logger);

            CommandMetadata commandMetadata = new()
            {
                Name = Name
            };

            ExternalScriptInfo? scriptInfo = pwsh.GetCommand<ExternalScriptInfo>(script.FullName);

            commandMetadata.Schema = new()
            {
                Parameters = [..
                    scriptInfo?.Parameters?.Values
                        .Where(parameter =>
                            (!parameter.IsCommon() || scriptInfo.ScriptContents.Contains($"${parameter.Name}")) &&
                            parameter.Attributes.OfType<ParameterAttribute>().FirstOrDefault()?.DontShow != true)
                        .Select(parameter => new CommandSchema.ParameterSchema
                        {
                            Name = parameter.Name,
                            Type = parameter.ParameterType != typeof(SwitchParameter) ? parameter.ParameterType : typeof(bool),
                            IsOptional = parameter.Attributes.OfType<ParameterAttribute>().FirstOrDefault()?.Mandatory != true,
                            Description = parameter.Attributes.OfType<ParameterAttribute>().FirstOrDefault()?.HelpMessage ?? string.Empty,
                        }) ?? []
                ]
            };
            
            CommentHelpInfo commentHelpInfo = (scriptInfo?.ScriptBlock.Ast as ScriptBlockAst)?.GetHelpContent() ?? new();

            commandMetadata.Title = commentHelpInfo.Synopsis;

            commandMetadata.Description = commentHelpInfo.Description;

            commandMetadata.SetProperty(nameof(commentHelpInfo.Role), commentHelpInfo.Role);

            if (commentHelpInfo.Links is not null)
            {
                foreach (var link in commentHelpInfo.Links)
                    commandMetadata.AddProperty("Link", link);
            }

            if (!string.IsNullOrWhiteSpace(commentHelpInfo.Notes))
            {
                foreach (var commandMetadataEntry in PwshHelp.ParseDictionary(commentHelpInfo.Notes))
                    commandMetadata.SetProperty(commandMetadataEntry.Key, commandMetadataEntry.Value);
            }

            return commandMetadata;
        }
    }
}
