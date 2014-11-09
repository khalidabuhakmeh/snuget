using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Snuget.Job.Commands;

namespace Snuget.Job
{
    class Program
    {
        static void Main(string[] args)
        {
            new SeedDatabaseFromNuget().Execute();
            Console.ReadLine();
        }
    }
}
