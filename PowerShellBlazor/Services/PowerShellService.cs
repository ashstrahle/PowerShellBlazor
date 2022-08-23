using System;
using System.Collections.ObjectModel;
using System.Management.Automation;

namespace PowerShellBlazor.Services
{
    public class PowerShellService : IPowerShellService
    {
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


                AddOutput("<br>_________________________________________________________________________");
                AddOutput("<br><b>EXECUTION COMPLETE</b>");
            }
        }
    }
}

