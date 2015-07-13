using Microsoft.Owin.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace ProtobufCacheServer.Console
{
    class Program
    {
        static void Main(string[] args)
        {
           

            string baseAddress = "http://localhost:9000/";

            // Start OWIN host 
            using (WebApp.Start<Startup>(url: baseAddress))
            {

                System.Console.ReadLine(); 
            }

        }
    }
}
