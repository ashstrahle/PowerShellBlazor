using System.Management.Automation;

namespace PowerShellBlazor.Services
{
    public class PowerShellService : IPowerShellService
    {
        private IWebHostEnvironment _WebHostEnvironment;

        public PowerShellService(IWebHostEnvironment webHostEnvironment)
        {
            _WebHostEnvironment = webHostEnvironment;
        }

        public List<Message> Output { get; set; } = new();

        private Message prevMsg = new();

        private void AddOutput(PSStream stream, string message)
        {
            Message msg = new()
            {
                PSStream = stream,
                Data = message
            };

            if (msg == prevMsg)
            {
                Output.Last().Data += ".";
            }
            else
            {
                Output.Add(msg);
                prevMsg = msg;
            }
            OutputChanged.Invoke(this, Output);
        }

        public event EventHandler<List<Message>> OutputChanged;

        public async Task RunScript(string script)
        {
            string pscommand = Path.Combine(_WebHostEnvironment.ContentRootPath, "Scripts/" + script);
            if (!File.Exists(pscommand))
            {
                AddOutput(PSStream.Error, "File not found: " + pscommand);
            }
            else
            {
                PowerShell shell = PowerShell.Create();
                shell.AddCommand(pscommand);

                string fontstr = "";

                AddOutput(PSStream.Output, "<b>Executing: </b>" + shell.Commands.Commands[0].ToString());
                string prevmsg = "";
                string msg = "";

                AddOutput(PSStream.Output, "<b>BEGIN</b>");
                AddOutput(PSStream.Output, "_________________________________________________________________________");

                var psOutput = new PSDataCollection<PSObject>();

                psOutput.DataAdded += delegate (object? sender, DataAddedEventArgs e)
                {
                    AddOutput(PSStream.Output, psOutput[e.Index].ToString());
                };

                shell.Streams.Information.DataAdded += delegate (object? sender, DataAddedEventArgs e)
                {
                    AddOutput(PSStream.Information, shell.Streams.Information[e.Index].ToString());
                };

                shell.Streams.Progress.DataAdded += delegate (object? sender, DataAddedEventArgs e)
                {
                    AddOutput(PSStream.Progress, shell.Streams.Progress[e.Index].Activity.ToString());
                };

                shell.Streams.Warning.DataAdded += delegate (object? sender, DataAddedEventArgs e)
                {
                    AddOutput(PSStream.Warning, shell.Streams.Warning[e.Index].ToString());
                };

                shell.Streams.Error.DataAdded += delegate (object? sender, DataAddedEventArgs e)
                {
                    AddOutput(PSStream.Error, shell.Streams.Error[e.Index].ToString());
                };

                // Execute powershell command
                IAsyncResult IasyncResult = shell.BeginInvoke<PSObject, PSObject>(null, psOutput);

                // Wait for powershell command to finish
                IasyncResult.AsyncWaitHandle.WaitOne();

                AddOutput(PSStream.Output, "_________________________________________________________________________");
                AddOutput(PSStream.Output, "<b>EXECUTION COMPLETE</b>");
            }
        }
    }
}

