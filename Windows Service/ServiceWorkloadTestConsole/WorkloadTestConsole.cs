using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using System.Configuration;
using ServiceWorkloads;

namespace ServiceWorkloads.WorkloadTestConsole
{

    public class WorkloadTestConsole
    {
        static void Main(string[] args)
        {
            WorkloadContainerManager manager = new WorkloadContainerManager();

            manager.Start(args);
            Console.WriteLine("Press CTRL+X to Exit...");
            while (true)
            {
                var key = Console.ReadKey();
                if ((key.Key == ConsoleKey.X) && (key.Modifiers.HasFlag(ConsoleModifiers.Control)))
                {
                    manager.Stop();
                    break;
                }
            }
            return;
        }
    }
}
