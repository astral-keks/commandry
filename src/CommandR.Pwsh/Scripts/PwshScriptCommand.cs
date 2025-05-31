using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Management.Automation.Language;
using System.Management.Automation;
using System.Management.Automation.Runspaces;
using System.Threading;
using System.Threading.Tasks;
using System.Management.Automation.Internal;

namespace CommandR.Scripts
{
    internal class PwshScriptCommand(Runspace runspace, FileInfo script) : Command
    {
        public override string Name { get; } = Path.GetFileNameWithoutExtension(script.Name);

        public override async Task ExecuteAsync(CancellationToken cancellation)
        {
            using Pwsh pwsh = new(runspace, Logger);

            List<object?> results = [];
            await foreach (var result in pwsh.RunScriptAsync(script, Parameters))
                results.Add(result);

            Result = new()
            {
                Records = results
            };
        }

        public override async Task<CommandMetadata> DescribeAsync(CancellationToken cancellation)
        {
            using Pwsh pwsh = new(runspace, Logger);

            CommandMetadata commandMetadata = new()
            {
                Name = Name
            };

            ExternalScriptInfo? scriptInfo = await pwsh.DescribeScriptAsync(script);
            CommentHelpInfo commentHelpInfo = (scriptInfo?.ScriptBlock.Ast as ScriptBlockAst)?.GetHelpContent() ?? new();

            HashSet<string> commonParameters = [.. typeof(CommonParameters).GetProperties().Select(property => property.Name)];
            commandMetadata.Schema = new()
            {
                Parameters = [..
                    scriptInfo?.Parameters?.Values
                        .Where(parameter => !commonParameters.Contains(parameter.Name) || scriptInfo.ScriptContents.Contains($"${parameter.Name}"))
                        .Select(parameter => new CommandSchema.ParameterSchema
                        {
                            Name = parameter.Name,
                            Type = parameter.ParameterType != typeof(SwitchParameter) ? parameter.ParameterType : typeof(bool),
                            IsOptional = parameter.Attributes.OfType<ParameterAttribute>().FirstOrDefault()?.Mandatory != true,
                            Description = parameter.Attributes.OfType<ParameterAttribute>().FirstOrDefault()?.HelpMessage ?? string.Empty,
                        }) ?? []
                ]
            };

            commandMetadata.Title = commentHelpInfo.Synopsis;
            
            commandMetadata.Description = commentHelpInfo.Description;

            if (!string.IsNullOrWhiteSpace(commentHelpInfo.Notes))
            {
                foreach (var commandMetadataEntry in commentHelpInfo.Notes.ParseDictionary())
                    commandMetadata.SetProperty(commandMetadataEntry.Key, commandMetadataEntry.Value);
            }

            return commandMetadata;
        }
    }
}
