using System;
using System.Collections.Generic;
using System.IO;

namespace CommandR.Scripting
{
    public sealed class ScriptBundle : IDisposable
    {
        private readonly DirectoryInfo _scriptDirectory;
        private readonly FileSystemWatcher _scriptWatcher;
        private readonly ScriptSettings _scriptSettings;

        public ScriptBundle(DirectoryInfo scriptDirectory, ScriptSettings scriptSettings)
        {
            _scriptDirectory = scriptDirectory;
            _scriptWatcher = new(_scriptDirectory.FullName)
            {
                Filter = "*.csx",
                NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.FileName,
                IncludeSubdirectories = false,
                EnableRaisingEvents = true,
            };
            _scriptWatcher.Changed += (sender, e) => ReloadScripts();
            _scriptSettings = scriptSettings;
            ReloadScripts();
        }

        public void Dispose()
        {
            _scriptWatcher.Dispose();
        }

        public IEnumerable<Script> Scripts { get; private set; } = [];

        public IEnumerable<ScriptDiagnostic> Diagnostics { get; private set; } = [];

        public event EventHandler? Reloaded;

        private void ReloadScripts()
        {
            List<Script> scripts = [];
            List<ScriptDiagnostic> diagnostics = [];

            foreach (var scriptFile in _scriptDirectory.EnumerateFiles(_scriptWatcher.Filter))
            {
                try
                {
                    Script script = Script.Compile(scriptFile, _scriptSettings);
                    scripts.Add(script);
                }
                catch (ScriptLoadException e)
                {
                    diagnostics.AddRange(e.Diagnostics);
                }
            }

            foreach (var script in Scripts)
                script.Dispose();

            Scripts = scripts;
            Diagnostics = diagnostics;

            Reloaded?.Invoke(this, EventArgs.Empty);
        }
    }
}
