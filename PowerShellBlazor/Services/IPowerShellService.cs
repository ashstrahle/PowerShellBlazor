using System;
using System.Management.Automation;

namespace PowerShellBlazor.Services
{
    public interface IPowerShellService
    {
        public List<string> Output { get; set; }
        public Task RunScript(PowerShell shell, bool varwidth);

        event EventHandler<List<string>> OutputChanged;
    }
}