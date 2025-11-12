using Commandry.Hosting;
using Microsoft.Extensions.Logging;
using ModelContextProtocol.Protocol;
using ModelContextProtocol.Server;

namespace Commandry.Mcp.Resources
{
    internal class McpResourcesController : IDisposable
    {
        private readonly CommandHost _commandHost;
        private readonly IMcpServer _mcpServer;
        private readonly ILoggerProvider _loggerProvider;
        private readonly ILogger _logger;

        public McpResourcesController(CommandHost commandHost, IMcpServer mcpServer, ILoggerProvider loggerProvider)
        {
            _commandHost = commandHost;
            _mcpServer = mcpServer;
            _loggerProvider = loggerProvider;
            _logger = _loggerProvider.CreateLogger(nameof(McpResourcesController));
        }

        public void Dispose()
        {
            _loggerProvider.Dispose();
        }

        public async ValueTask<ListResourcesResult> ListResourcesAsync(CancellationToken cancellation)
        {
            ListResourcesResult result = new();

            foreach (var command in _commandHost.GetCommands())
            {
                try
                {
                    CommandMetadata commandMetadata = await command.DescribeAsync(cancellation);
                    if (commandMetadata.IsResourceList())
                    {
                        command.Logger = _logger;

                        await command.ExecuteAsync(cancellation);

                        IEnumerable<Resource> resources = command.Inspect().Records
                            .Where(record => record is not null)
                            .Select(record => record.ToResource());
                        foreach (var resource in resources)
                            result.Resources.Add(resource);
                    }
                }
                catch (Exception e)
                {
                    _logger.LogError(e, "Unexpected error while getting resources");
                }
            }

            return result;
        }

        public async ValueTask<ListResourceTemplatesResult> ListResourceTemplatesAsync(CancellationToken cancellation)
        {
            ListResourceTemplatesResult result = new();

            foreach (var command in _commandHost.GetCommands())
            {
                try
                {
                    CommandMetadata commandMetadata = await command.DescribeAsync(cancellation);
                    if (commandMetadata.IsResourceTemplateList())
                    {
                        command.Logger = _logger;

                        await command.ExecuteAsync(cancellation);

                        IEnumerable<ResourceTemplate> resourceTemlpates = command.Inspect().Records
                            .Where(record => record is not null)
                            .Select(record => record.ToResourceTemplate());
                        foreach (var resourceTemplate in resourceTemlpates)
                            result.ResourceTemplates.Add(resourceTemplate);
                    }
                }
                catch (Exception e)
                {
                    _logger.LogError(e, "Unexpected error while getting resource templates");
                }
            }

            return result;
        }

        public async ValueTask<ReadResourceResult> ReadResourceAsync(ReadResourceRequestParams? request, CancellationToken cancellation)
        {
            ReadResourceResult result = new();

            if (request is not null)
            {
                foreach (var command in _commandHost.GetCommands())
                {
                    try
                    {
                        CommandMetadata commandMetadata = await command.DescribeAsync(cancellation);
                        if (commandMetadata.IsResourceContent(request.Uri))
                        {
                            command.Logger = _logger;
                            command.Parameters = new()
                            {
                                { "Uri", request.Uri },
                            };

                            await command.ExecuteAsync(cancellation);

                            IEnumerable<ResourceContents> resourcesContents = command.Inspect().Records
                                .Where(record => record is not null)
                                .Select(record => record.ToResourceContents(request.Uri));
                            foreach (var resourceContents in resourcesContents)
                                result.Contents.Add(resourceContents);
                        }
                    }
                    catch (Exception e)
                    {
                        _logger.LogError(e, "Unexpected error while reading resource");
                        throw;
                    }
                }
            }

            return result;
        }

        public async ValueTask SubscribeToResourceAsync(SubscribeRequestParams? request, CancellationToken cancellation)
        {
            if (request?.Uri is not null)
            {
                foreach (var command in _commandHost.GetCommands())
                {
                    try
                    {
                        CommandMetadata commandMetadata = await command.DescribeAsync(cancellation);
                        if (commandMetadata.IsResourceSubscription(request.Uri))
                        {
                            command.Logger = _logger;
                            command.Parameters = new()
                            {
                                { "Uri", request.Uri },
                                { "Observer", _mcpServer.AsResourceObserver(_logger, cancellation) }
                            };

                            await command.ExecuteAsync(cancellation);

                            command.Inspect();
                        }
                    }
                    catch (Exception e)
                    {
                        _logger.LogError(e, "Unexpected error while subscribing to resource");
                    }
                }
            }
        }
    }
}
