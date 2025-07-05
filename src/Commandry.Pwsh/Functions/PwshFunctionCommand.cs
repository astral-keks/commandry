using Commandry.Schemas;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Management.Automation.Language;
using System.Threading;
using System.Threading.Tasks;

namespace Commandry.Functions
{
    internal class PwshFunctionCommand(PwshRunspace runspace, FunctionInfo function) : PwshCommand
    {
        public override string Name { get; } = function.Name;

        public override async Task ExecuteAsync(CancellationToken cancellation)
        {
            using Pwsh pwsh = runspace.CreatePwsh(ReportProgress,Logger);

            pwsh.SetServiceProvider(Services);

            List<object?> results = [];
            foreach (var result in pwsh.InvokeCommand(function.Name, Parameters))
                results.Add(result);

            Result = new()
            {
                Records = results
            };
        }

        public override Task<CommandMetadata> DescribeAsync(CancellationToken cancellation)
        {
            using Pwsh pwsh = runspace.CreatePwsh(ReportProgress, Logger);

            CommandMetadata commandMetadata = new()
            {
                Name = Name
            };

            commandMetadata.Schema = new PwshCommandSchema(runspace)
            {
                Parameters = [..
                    function?.Parameters?.Values
                        .Where(parameter => 
                            (!parameter.IsCommon() || function.Definition.Contains($"${parameter.Name}")) &&
                            parameter.Attributes.OfType<ParameterAttribute>().FirstOrDefault()?.DontShow != true)
                        .Select(parameter => new CommandParameterSchema
                        {
                            Name = parameter.Name,
                            Type = parameter.ParameterType != typeof(SwitchParameter) ? parameter.ParameterType : typeof(bool),
                            IsOptional = parameter.Attributes.OfType<ParameterAttribute>().FirstOrDefault()?.Mandatory != true,
                            Description = parameter.Attributes.OfType<ParameterAttribute>().FirstOrDefault()?.HelpMessage ?? string.Empty,
                        }) ?? []
                ]
            };

            CommentHelpInfo commentHelpInfo = (function?.ScriptBlock.Ast as FunctionDefinitionAst)?.GetHelpContent() ?? new();

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

            return Task.FromResult(commandMetadata);
        }
    }
}
