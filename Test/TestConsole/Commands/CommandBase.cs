using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestConsole.Commands
{
    public abstract class CommandBase
    {
        public abstract Task<bool> ParseArgumentsAsync(IEnumerable<string> args);
        public abstract Task ExecuteAsync(IEnumerable<string> args);

        public abstract void ShowArguments();

        protected bool GetValue<T>(IEnumerable<string> args, string argName, ref T value)
        {
            bool result = false;
            value = default(T);
            var arg = args.FirstOrDefault(a => a.Split('=')[0] == $"-{argName}");
            if (arg != null)
            {
                var argSplit = arg.Split('=');
                if (argSplit.Count() > 1)
                {
                    try
                    {
                        value = (T)Convert.ChangeType(argSplit[1], typeof(T));
                        result = true;
                    }
                    catch { }
                }
            }
            return result;
        }

        protected void WriteError(string message)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(message);
            Console.ResetColor();
        }

        protected void WriteWarning(string message)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine(message);
            Console.ResetColor();
        }

        protected void WriteSuccess(string message)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine(message);
            Console.ResetColor();
        }

        protected void WriteMessage(string message)
        {
            Console.WriteLine(message);
        }

        protected void WriteJson(object obj)
        {
            var json = JsonConvert.SerializeObject(obj, Formatting.Indented);
            Console.WriteLine(json);
        }
    }
}
