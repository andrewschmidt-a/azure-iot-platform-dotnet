using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;

namespace DeviceMigration
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Starting the push of devices");
            Utils ut = new Utils();
            bool result = ut.SendTwinsToEH().GetAwaiter().GetResult();
            if (result)
            {
                Console.WriteLine("Push completed successfully, Press Enter to continue..");
            }
            else
            {
                Console.WriteLine("Push completed with failures, Press Enter to continue..");
            }
            Console.ReadLine();
        }
    }
}