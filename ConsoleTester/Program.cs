using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Collections.Generic;
using Enbridge.Examples;


namespace ConsoleTester
{
    class Program
    {
        static void Main(string[] args)
        {
            

            UpdateDLLs.update();
            Console.WriteLine("finish");
            Console.ReadLine();
        }
    }    
}
