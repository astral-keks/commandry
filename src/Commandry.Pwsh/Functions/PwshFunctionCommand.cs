using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Management.Automation.Language;
using System.Threading;
using System.Threading.Tasks;

namespace Commandry.Functions
{
    internal class PwshFunctionCommand(PwshRunspace runspace, FunctionInfo function) : Command
    {
        public override string Name { get; } = function.Name;

        public override async Task ExecuteAsync(CancellationToken cancellation)
        {
            using Pwsh pwsh = runspace.CreatePwsh(Logger);

            List<object?> results = [];
            await foreach (var result in pwsh.InvokeCommandAsync(function.Name, Parameters))
                results.Add(result);

            Result = new()
            {
                Records = results
            };
        }

        public override Task<CommandMetadata> DescribeAsync(CancellationToken cancellation)
        {
            using Pwsh pwsh = runspace.CreatePwsh(Logger);

            CommentHelpInfo commentHelpInfo = (function?.ScriptBlock.Ast as FunctionDefinitionAst)?.GetHelpContent() ?? new();

            CommandMetadata commandMetadata = new()
            {
                Name = Name
            };

            commandMetadata.Schema = new()
            {
                Parameters = [..
                    function?.Parameters?.Values
                        .Where(parameter => !parameter.IsCommon() || function.Definition.Contains($"${parameter.Name}"))
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
                foreach (var commandMetadataEntry in PwshHelp.ParseDictionary(commentHelpInfo.Notes))
                    commandMetadata.SetProperty(commandMetadataEntry.Key, commandMetadataEntry.Value);
            }
            commandMetadata.SetProperty(nameof(commentHelpInfo.Role), commentHelpInfo.Role);

            return Task.FromResult(commandMetadata);
        }
    }
}
