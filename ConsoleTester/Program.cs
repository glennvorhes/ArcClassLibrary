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
            //new comment
            //another comment
            string string_MP = "1010";
            double double_MP;

            //#1 Try to parse the string
            bool double_parse_worked = System.Double.TryParse(string_MP, out double_MP);

            //if (double_parse_worked)
            //{
            //    //#2 Create the locator on line 1
            //    Enbridge.LinearReferencing.ContLineLocatorSQL loc;

            //    //#3 run the locator
            //    loc = new Enbridge.LinearReferencing.ContLineLocatorSQL("{D4D4472B-FB1E-485B-A550-DCE76F63BC08}");

            //    double stn, meas, X, Y, Z;
            //    stn = loc.getStnFromMP(double_MP, out meas, out X, out Y, out Z);
            //    Console.WriteLine("{0} {1} {2} {3}", stn, meas, X, Y, Z);
            //}

            UpdateDLLs.update();
            Console.WriteLine("finish");
            Console.ReadLine();
        }
    }    
}
