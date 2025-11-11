using Commandry.Schemas;
using System.Management.Automation;
using System.Management.Automation.Language;

namespace Commandry
{
    public abstract class PwshCommand(PwshRunspace runspace) : Command
    {
        protected Task ExecuteAsync(string command, CancellationToken cancellation)
        {
            using Pwsh pwsh = CreatePwsh();

            pwsh.SetServiceProvider(Services);

            List<object?> results = [];
            foreach (var result in pwsh.InvokeCommand(command, Parameters))
                results.Add(result);

            Result = new()
            {
                Records = results
            };

            return Task.CompletedTask;
        }

        protected Task<CommandMetadata> DescribeAsync(IEnumerable<ParameterMetadata> parameters, IEnumerable<PSTypeName> outputs, CommentHelpInfo comment,
            CancellationToken cancellation)
        {
            CommandMetadata commandMetadata = new()
            {
                Name = Name
            };

            commandMetadata.Schema = new PwshCommandSchema(runspace)
            {
                Parameters = [..
                    parameters
                        .Where(parameter => parameter.Attributes.OfType<ParameterAttribute>().FirstOrDefault()?.DontShow != true)
                        .Select(parameter => new CommandParameterSchema
                        {
                            Name = parameter.Name,
                            Type = parameter.ParameterType != typeof(SwitchParameter) ? parameter.ParameterType : typeof(bool),
                            IsOptional = parameter.Attributes.OfType<ParameterAttribute>().FirstOrDefault()?.Mandatory != true,
                            Description = parameter.Attributes.OfType<ParameterAttribute>().FirstOrDefault()?.HelpMessage ?? string.Empty,
                        }) ?? []
                ],
                Results = [..
                    outputs
                        .Select(output => new CommandResultSchema
                        {
                            Type = output.Type,
                            Description = comment.Outputs
                                .Select(PwshHelp.ParseDictionary)
                                .Where(outputMetadata =>
                                    outputMetadata.TryGetValue(nameof(CommandResultSchema.Type), out string? type) &&
                                    type == output.Type.Name)
                                .Select(outputMetadata => outputMetadata.TryGetValue(nameof(CommandResultSchema.Description), out string? description)
                                    ? description
                                    : string.Empty)
                                .FirstOrDefault() ?? string.Empty
                        }) ?? []
                ]
            };

            commandMetadata.Title = comment.Synopsis?.Trim() ?? string.Empty;

            commandMetadata.Description = comment.Description?.Trim() ?? string.Empty;

            commandMetadata.SetProperty(nameof(comment.Role), comment.Role?.Trim() ?? string.Empty);

            if (comment.Links is not null)
            {
                foreach (var link in comment.Links)
                    commandMetadata.AddProperty("Link", link);
            }

            if (!string.IsNullOrWhiteSpace(comment.Notes))
            {
                foreach (var commandMetadataEntry in PwshHelp.ParseDictionary(comment.Notes))
                    commandMetadata.SetProperty(commandMetadataEntry.Key, commandMetadataEntry.Value);
            }

            return Task.FromResult(commandMetadata);
        }

        protected void ReportProgress(ProgressRecord progress)
        {
            switch (progress.RecordType)
            {
                case ProgressRecordType.Processing:
                    Progress?.Report(progress.PercentComplete, $"{Name}: {progress.StatusDescription} {progress.PercentComplete}%");
                    break;
                case ProgressRecordType.Completed:
                    Progress?.Report(100, $"{Name}: {progress.StatusDescription}");
                    break;
            }
        }

        protected Pwsh CreatePwsh()
        {
            return runspace.CreatePwsh(ReportProgress, Logger);
        }
    }
}
