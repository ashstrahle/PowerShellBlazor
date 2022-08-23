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
=======
        public List<string> Output { get; set; } = new();

        private void AddOutput(string str)
        {
            Output.Add(str);
            OutputChanged.Invoke(this, Output);
        }

        public event EventHandler<List<string>> OutputChanged;

        public async Task RunScript(PowerShell shell, bool varwidth)
        {
            if (shell == null)
            {
                AddOutput("Shell empty - nothing to execute");
            }
            else
            {
                string fontstr = "";
                if (varwidth != true)
                {
                    fontstr = "face='monospace' size=3";
                }

                AddOutput("<b>Executing: </b>" + shell.Commands.Commands[0].ToString());
                string prevmsg = "";
                string msg = "";

                AddOutput("<br><b>BEGIN</b>");
                AddOutput("<br>_________________________________________________________________________");

                var psOutput = new PSDataCollection<PSObject>();

                // Collect powershell OUTPUT
                psOutput.DataAdded += delegate (object? sender, DataAddedEventArgs e)
                {
                    msg = psOutput[e.Index].ToString();

                    if (msg != prevmsg)
                    {
                        AddOutput("<br><span><font color=black " + fontstr + ">" + msg + "</font></span>");
                    }
                    else
                    {
                        AddOutput(".");
                    }
                    prevmsg = msg;
                    if (sender is not null)
                    {
                        var psoutput = (PSDataCollection<PSObject>)sender;
                        Collection<PSObject> results = psoutput.ReadAll();
                    }
                };

                prevmsg = "";
                // Collect powershell PROGRESS output
                shell.Streams.Progress.DataAdded += delegate (object? sender, DataAddedEventArgs e)
                {
                    msg = shell.Streams.Progress[e.Index].Activity.ToString();
                    if (msg != prevmsg)
                    {
                        AddOutput("<br><span><font color=green " + fontstr + ">" + msg + "</font></span>");
                    }
                    else
                    {
                        AddOutput(".");
                    }
                    prevmsg = msg;
                    if (sender is not null)
                    {
                        var psprogress = (PSDataCollection<ProgressRecord>)sender;
                        Collection<ProgressRecord> results = psprogress.ReadAll();
                    }
                };

                prevmsg = "";
                // Collect powershell WARNING output
                shell.Streams.Warning.DataAdded += delegate (object? sender, DataAddedEventArgs e)
                {
                    msg = shell.Streams.Warning[e.Index].ToString();
                    if (msg != prevmsg)
                    {
                        AddOutput("<br><span><font color=orange " + fontstr + "><b>***WARNING***:</b> " + msg + "</font></span>");
                    }
                    else
                    {
                        AddOutput(".");
                    }
                    prevmsg = msg;
                    if (sender is not null)
                    {
                        var pswarning = (PSDataCollection<WarningRecord>)sender;
                        Collection<WarningRecord> results = pswarning.ReadAll();
                    }
                };

                prevmsg = "";
                // Collect powershell ERROR output
                shell.Streams.Error.DataAdded += delegate (object? sender, DataAddedEventArgs e)
                {
                    msg = shell.Streams.Error[e.Index].ToString();
                    if (msg != prevmsg)
                    {
                        AddOutput("<br><span><font color=red " + fontstr + "><b>***ERROR***:</b> " + msg + "</font></span>");
                    }
                    else
                    {
                        AddOutput(".");
                    }
                    prevmsg = msg;
                    if (sender is not null)
                    {
                        var pserror = (PSDataCollection<ErrorRecord>)sender;
                        Collection<ErrorRecord> results = pserror.ReadAll();
                    }
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

