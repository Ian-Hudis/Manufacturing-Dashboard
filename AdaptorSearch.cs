using System;
using System.Diagnostics;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Xml;
using static MTConnectDashboard.AdapterCheck;
using static MTConnectDashboard.AdaptorSearch;

namespace MTConnectDashboard
{
    public class AdapterCheck
    {

        public struct Adapterdata
        {
            public string ID;
            public string URL;
            public bool status;
        }

        // initialize the adaptor
        public static Adapterdata[] InitAdapt(Adapterdata[] adaptorarray,int AAA, Adapterdata adaptor, string tag, string ipPort)
        {
            // put in the adaptor info
            adaptor.ID = tag; 
            adaptor.URL = "http://" + ipPort;
            adaptor.status = true;

            try
            {
                adaptorarray[AAA] = adaptor;// put the adaptor on the list
            }
            catch
            {
                Console.WriteLine("Failed to parse, AdaptorSearch: line 33");
            }
            return adaptorarray;
        }

        public static Adapterdata[] Adaptercheck(Adapterdata[] adapterarray)
        {
           // Console.WriteLine(Process.GetCurrentProcess().Threads.Count);

            if (Process.GetCurrentProcess().Threads.Count <180)
            {
                AdaptorSearch adaptsearch = new(adapterarray);
                Thread searchthread = new(new ThreadStart(Search));
                searchthread.Start();
            }
            

            if (AdaptorArray!=null)
            {
               adapterarray = AdaptorArray;
            }
            return adapterarray;
        }
    }


    public class AdaptorSearch
    {
        // State information used in the task.

        public static Adapterdata[]? AdaptorArray;

        public static Adapterdata[]? prevAdapterArray;

        // The constructor obtains the state information.
        public AdaptorSearch(Adapterdata[] adaptorarray)
        {
            AdaptorArray = adaptorarray;

            
        }


        public static void Search()  // adaptor searching thread
        {

            if (AdaptorArray!=null)
            {
                for (int i = 0; i<AdaptorArray.Length; i++)
                {
                    AdaptorArray[i].status= Test(AdaptorArray[i].URL);
                    //AdaptorList[i].status = Test(AdaptorList[i].URL);
                    //Console.WriteLine(Status);
                }
                /*
                if(AdaptorArray != prevAdapterArray)
                {
                    prevAdapterArray = AdaptorArray;
                    Thread.Sleep(250);
                }
                */

            }

        }

        private static bool Test(string url)
        {
            bool site_exists;

            Uri urlCheck = new(url);

            #pragma warning disable SYSLIB0014 // Type or member is obsolete
            WebRequest request = WebRequest.Create(urlCheck);
            #pragma warning restore SYSLIB0014 // Type or member is obsolete
            // request.Timeout = 7000; // 7 seconds
            request.Method = "GET";
            WebResponse response;

            try
            {
                response = request.GetResponse();
                site_exists = true;
            }
            catch
            {
               // Console.WriteLine(url);
                site_exists = false; //url does not exist
            }
            return site_exists;
                
        }

    }
}