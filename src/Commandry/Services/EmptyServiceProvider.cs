using System;

namespace Commandry.Services
{
    public class EmptyServiceProvider : IServiceProvider
    {
        public static readonly EmptyServiceProvider Instance = new();

        public object? GetService(Type serviceType) => default;
    }
}
