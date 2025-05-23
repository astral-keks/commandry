using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;
using Microsoft.CodeAnalysis.Text;
using Polly;
using Polly.Retry;
using System;
using System.IO;
using System.Reflection;
using System.Runtime.Loader;

namespace CommandR.Scripting
{
    public sealed class Script(Assembly assembly, AssemblyLoadContext assemblyContext) : IDisposable
    {
        private readonly Assembly _assembly = assembly;
        private readonly AssemblyLoadContext _assemblyContext = assemblyContext;

        public static Script Compile(FileInfo scriptFile, ScriptSettings scriptSettings)
        {
            RetryPolicy readPolicy = Policy.Handle<IOException>()
                .WaitAndRetry(3, _ => TimeSpan.FromSeconds(1));
            byte[] scriptBytes = readPolicy.Execute(() => File.ReadAllBytes(scriptFile.FullName));
            string scriptString = scriptSettings.Encoding.GetString(scriptBytes);
            SourceText scriptText = SourceText.From(scriptString, scriptSettings.Encoding);

            ScriptOptions scriptOptions = ScriptOptions.Default
                .WithReferences(AppDomain.CurrentDomain.GetAssemblies())
                .WithImports(scriptSettings.GlobalImports)
                .WithOptimizationLevel(scriptSettings.OptimizationLevel)
                .WithFileEncoding(scriptSettings.Encoding);
            Script<object> script = CSharpScript.Create(scriptString, scriptOptions);
            
            Compilation scriptCompilation = script.GetCompilation()
                .WithAssemblyName(scriptFile.Name);

            ScriptLoadContext assemblyContext = new();
            Assembly assembly = assemblyContext.LoadFromCompilation(scriptCompilation);
            return new(assembly, assemblyContext);
        }

        public void Dispose()
        {
            _assemblyContext.Unload();
        }

        public Assembly Assembly => _assembly;
    }
}
