using System;
namespace PowerShellBlazor.Models
{
    public enum PSStream { Output, Information, Progress, Warning, Error }

    public class Message
    {
        public PSStream PSStream;
        public string Data;
    }
}