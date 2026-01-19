/// Ian Hudis
/// KEB America
/// release 5.2 hotfix 2
/// 12/19/2025
using Azure;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.Hosting;
using Microsoft.Identity.Client;
using MTConnectDashboard.Pages;
using System.Diagnostics;
using System.Linq.Expressions;

namespace MTConnectDashboard
{ 
    class Program // main thread
    {
        public static DateTime StartTime = new();

        // gui Script
        static void Main(string[] args)
        { // run once
            StartTime = DateTime.Now;
            // Creating and initializing threads
            Thread httpgrab = new (DataOutput.HttpGetData);
            httpgrab.Start();
            
            var builder = WebApplication.CreateBuilder(args);
            // Add services to the container.
            builder.Services.AddRazorPages();
            builder.Services.AddServerSideBlazor();
            builder.Services.AddSingleton<DeviceData>(); // streaming 2282
            builder.Services.AddSingleton<MSR1_Service>(); // the dashboard
            var app = builder.Build();
            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }
            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseRouting();
            app.MapBlazorHub();
            app.MapFallbackToPage("/_Host");
            app.Run();
        }
    }

    public class DataOutput
    {
        // Please set to true before publishing  and false when debugging on localhost!
        const bool LogMode = true; // turns on the sql logging  if true - > mostly for me debugging the system :)

        public const int delay1 = 0;    // thread sleep between line inputs (for debugging only)
        public const int delay2 = 1000; //500;    // thread sleep between queries
        public const int DataLimit = 128; // this needs to be greater than the sequence shift otherwise data is lost
        public const int sequenceshift = 127; // the amount of sequences grabbed per query

        private static int adaptnumb = 0;

        //! 3321 data
            private static readonly int Mach_Numb_3321 = adaptnumb;
            private const string P3321 = "913"; // kiosk server
            private const string Tag_3321 = "3321"; // kiosk address tag
            private const string ipPort3321 = "192.168.200.25:5000"; // the Bdtronic glue machine mtconnect address
            public static DeviceInter Mach3321 = new();
            public static MSR1.Watchlist DashboardData3321 = new(); // struct that carries the machine info  
            public static MSR1_Service mSR1service3321 = new();   // dashboard service updates the dashboard
            private static PLC_Client.PLC_Data Blackbox3321 = new(); // plc data
            private static SQL_Client.SQL_Logger SQL_3321 = new(); // format for sql storage    
		//! 3321 data
		//! 3112 data
		    private static readonly int Mach_Numb_3112 = ++adaptnumb;
		    private const string P3112 = "916"; // kiosk server
		    private const string Tag_3112 = "3112"; // kiosk address tag
            private const string ipPort3112 = "192.168.200.226:80"; // the raspberry pi mtconnect address
		    public static DeviceInter Mach3112 = new();
		    public static MSR1.Watchlist DashboardData3112 = new(); // struct that carries the machine info 
            public static MSR1_Service mSR1service3112 = new();   // dashboard service updates the dashboard
            private static PLC_Client.PLC_Data Blackbox3112 = new(); // plc data
		    private static SQL_Client.SQL_Logger SQL_3112 = new(); // format for sql storage 
		//! 3112 data
		//! 3111 data
		    private static readonly int Mach_Numb_3111 = ++adaptnumb;
            private const string P3111 = "915"; // kiosk server
            private const string Tag_3111 = "3111"; // kiosk address tag
            private const string ipPort3111 = "192.168.200.225:80"; // the raspberry pi mtconnect address
            public static DeviceInter Mach3111 = new();
            public static MSR1.Watchlist DashboardData3111 = new(); // struct that carries the machine info 
            public static MSR1_Service mSR1service3111 = new();   // dashboard service updates the dashboard
            private static PLC_Client.PLC_Data Blackbox3111 = new(); // plc data
            private static SQL_Client.SQL_Logger SQL_3111 = new(); // format for sql storage    
		//! 3111 data
		//! 2321 data
		    private static readonly int Mach_Numb_2321 = ++adaptnumb;
            private const string P2321 = "917"; // kiosk server
            private const string Tag_2321 = "2321"; // kiosk address tag
            private const string ipPort2321 = "192.168.200.25:5321"; // The MTConnect agent address for the Grinder :)
            public static DeviceInter Mach2321 = new();
            public static MSR1.Watchlist DashboardData2321 = new(); //  struct that carries that info that updates the dashboard
            public static MSR1_Service mSR1service2321 = new();  //dashboard service
            private static PLC_Client.PLC_Data Blackbox2321 = new(); // plc data
            private static SQL_Client.SQL_Logger SQL_2321 = new(); // format for sql storage
        //! 2321 data
        //! 2283 data 
            private static readonly int Mach_Numb_2283 = ++adaptnumb;
            private const string P2283 = "903";
            private const string Tag_2283 = "2283"; // 2283 blackbox address tag
            private const string ipPort2283 = "192.168.200.219:5000";  // Multiplex  Mazak smooth cnc machine
            public static DeviceInter Mach2283 = new();
            public static MSR1.Watchlist DashboardData2283 = new(); // struct that carries that info that updates the dashboard
            public static MSR1_Service mSR1service2283 = new();   // dashboard service
            private static PLC_Client.PLC_Data Blackbox2283 = new(); // plc data
            private static SQL_Client.SQL_Logger SQL_2283 = new(); // format for sql storage     
        //! 2283 data
        //! 2282 data 
            private static readonly int Mach_Numb_2282 = ++adaptnumb;
            private const string P2282 = "901";
            private const string Tag_2282 = "2282"; // 2282 blackbox address tag
            private const string ipPort2282 = "192.168.200.210:5000";  // Multiplex  Mazak smooth cnc machine
            public static DeviceInter Mach2282 = new();
            public static MSR1.Watchlist DashboardData2282 = new(); // struct that carries that info that updates the dashboard
            public static MSR1_Service mSR1service2282 = new();   // dashboard service
            private static PLC_Client.PLC_Data Blackbox2282 = new(); // plc data
            private static SQL_Client.SQL_Logger SQL_2282 = new(); // format for sql storage     
        //! 2282 data
        //! 2281 data
            private static readonly int Mach_Numb_2281 = ++adaptnumb;
            private const string P2281 = "902";
            private const string Tag_2281 = "2281"; // 2282 blackbox address tag
            private const string ipPort2281 = "192.168.200.211:5000";  // Multiplex  Mazak smooth cnc machine
            public static DeviceInter Mach2281 = new();
            public static MSR1.Watchlist DashboardData2281 = new(); // struct that carries that info that updates the dashboard
            public static MSR1_Service mSR1service2281 = new(); // dashboard service
            private static PLC_Client.PLC_Data Blackbox2281 = new(); // plc data
            private static SQL_Client.SQL_Logger SQL_2281 = new(); // format for sql storage   
        //! 2281 data
        //! 2280 data
            private static readonly int Mach_Numb_2280 = ++adaptnumb;
            private const string P2280 = "904";
            private const string Tag_2280 = "2280"; // 2282 blackbox address tag
            private const string ipPort2280 = "192.168.200.212:5280";  // Multiplex  Mazak smooth cnc machine
            public static DeviceInter Mach2280 = new();
            public static MSR1.Watchlist DashboardData2280 = new(); // struct that carries that info that updates the dashboard
            public static MSR1_Service mSR1service2280 = new();
            private static PLC_Client.PLC_Data Blackbox2280 = new(); // plc data
            private static SQL_Client.SQL_Logger SQL_2280 = new(); // format for sql storage  
        //! 2280 data
        //! 2272 data
            private static readonly int Mach_Numb_2272 = ++adaptnumb;
            private const string P2272 = "914";
            private const string Tag_2272 = "2272"; // 2272 blackbox address tag
            private const string ipPort2272 = "192.168.200.223:5000";  // Mazak smooth G cnc machine
            public static DeviceInter Mach2272 = new();
            public static MSR1.Watchlist DashboardData2272 = new(); // struct that carries that info that updates the dashboard
            public static MSR1_Service mSR1service2272 = new();
            private static PLC_Client.PLC_Data Blackbox2272 = new(); // plc data
            private static SQL_Client.SQL_Logger SQL_2272 = new(); // format for sql storage 
        //! 2272 data
        //! 2271 data
            private static readonly int Mach_Numb_2271 = ++adaptnumb;
            private const string P2271 = "908";
            private const string Tag_2271 = "2271"; // 2282 blackbox address tag
            private const string ipPort2271 = "192.168.200.25:5271";  // Multiplex  Mazak smooth cnc machine
            public static DeviceInter Mach2271 = new();
            public static MSR1.Watchlist DashboardData2271 = new(); // struct that carries that info that updates the dashboard
            public static MSR1_Service mSR1service2271 = new();
            private static PLC_Client.PLC_Data Blackbox2271 = new(); // plc data
            private static SQL_Client.SQL_Logger SQL_2271 = new(); // format for sql storage 
        //! 2271 data
        //! 2260 data
            private static readonly int Mach_Numb_2260 = ++adaptnumb;
            private const string P2260 = "905";
            private const string Tag_2260 = "2260"; // 2282 blackbox address tag
            private const string ipPort2260 = "192.168.200.213:5000";//"192.168.200.213:5000"; // Nexus510c
            public static DeviceInter Mach2260 = new();
            public static MSR1.Watchlist DashboardData2260 = new(); // struct that carries that info that updates the dashboard
            public static MSR1_Service mSR1service2260 = new();
            private static PLC_Client.PLC_Data Blackbox2260 = new(); // plc data
            private static SQL_Client.SQL_Logger SQL_2260 = new(); // format for sql storage  
        //! 2260 data
        //! 2112 data
            private static readonly int Mach_Numb_2112 = ++adaptnumb;
            private const string P2112 = "912";
            private const string Tag_2112 = "2112"; // 2112 blackbox address tag
            private const string ipPort2112 = "192.168.200.25:5112"; // 2112 Mazak EZ
            public static DeviceInter Mach2112 = new();
            public static MSR1.Watchlist DashboardData2112 = new(); // struct that carries that info that updates the dashboard
            public static MSR1_Service mSR1service2112 = new();
            private static PLC_Client.PLC_Data Blackbox2112 = new(); // plc data
            private static SQL_Client.SQL_Logger SQL_2112 = new(); // format for sql storage  
        //! 2112 data
        //! 2111 data
            private static readonly int Mach_Numb_2111 = ++adaptnumb;
            private const string P2111 = "911";
            private const string Tag_2111 = "2111"; // 2111 blackbox address tag
            private const string ipPort2111 = "192.168.200.25:5111"; // 2111 Mazak Smart 2 Friction Lining Cell
            public static DeviceInter Mach2111 = new();
            public static MSR1.Watchlist DashboardData2111 = new(); // struct that carries that info that updates the dashboard
            public static MSR1_Service mSR1service2111 = new();
            private static PLC_Client.PLC_Data Blackbox2111 = new(); // plc data
            private static SQL_Client.SQL_Logger SQL_2111 = new(); // format for sql storage  
        //! 2111 data
        //! 2107 data    -> Doosan
            private static readonly int Mach_Numb_2107 = ++adaptnumb;
            private const string P2107 = "907";
            private const string Tag_2107 = "2107"; // 2107 blackbox address tag
            private const string ipPort2107 = "192.168.200.25:5107"; // 2111 Mazak Smart 2 Friction Lining Cell
            public static DeviceInter Mach2107 = new();
            public static MSR1.Watchlist DashboardData2107 = new(); // struct that carries that info that updates the dashboard
            public static MSR1_Service mSR1service2107 = new();
            private static PLC_Client.PLC_Data Blackbox2107 = new(); // plc data
            private static SQL_Client.SQL_Logger SQL_2107 = new(); // format for sql storage  
        //! 2107 data
        //! 2105 data    -> HAAS
            private static readonly int Mach_Numb_2105 = ++adaptnumb;
            private const string P2105 = "906";
            private const string Tag_2105 = "2105"; // 2282 blackbox address tag
            private const string ipPort2105 = "192.168.200.25:5105"; // 2111 Mazak Smart 2 Friction Lining Cell
            public static DeviceInter Mach2105 = new();
            public static MSR1.Watchlist DashboardData2105 = new(); // struct that carries that info that updates the dashboard
            public static MSR1_Service mSR1service2105 = new();
            private static PLC_Client.PLC_Data Blackbox2105 = new(); // plc data
            private static SQL_Client.SQL_Logger SQL_2105 = new();   // format for sql storage  
        //! 2105 data
        //! 2103 data    -> HAAS
            private static readonly int Mach_Numb_2103 = ++adaptnumb;
            private const string P2103 = "910";
            private const string Tag_2103 = "2103"; // 2282 blackbox address tag
            private const string ipPort2103 = "192.168.200.25:5103"; // 2111 Mazak Smart 2 Friction Lining Cell
            public static DeviceInter Mach2103 = new();
            public static MSR1.Watchlist DashboardData2103 = new(); // struct that carries that info that updates the dashboard
            public static MSR1_Service mSR1service2103 = new();
            private static PLC_Client.PLC_Data Blackbox2103 = new(); // plc data
            private static SQL_Client.SQL_Logger SQL_2103 = new();   // format for sql storage  
        //! 2103 data
        //! 2102 data    -> HAAS
            private static readonly int Mach_Numb_2102 = ++adaptnumb;
            private const string P2102 = "909";
            private const string Tag_2102 = "2102"; // 2282 blackbox address tag
            private const string ipPort2102 = "192.168.200.25:5102"; // 2111 Mazak Smart 2 Friction Lining Cell
            public static DeviceInter Mach2102 = new();
            public static MSR1.Watchlist DashboardData2102 = new(); // struct that carries that info that updates the dashboard
            public static MSR1_Service mSR1service2102 = new();
            private static PLC_Client.PLC_Data Blackbox2102 = new(); // plc data
            private static SQL_Client.SQL_Logger SQL_2102 = new();   // format for sql storage  
         //! 2102 data

        //! test data
        /*
            private static readonly int Mach_Numb_1001 = ++adaptnumb;
            private const string P1001 = "900";
            private const string Tag_1001 = "1001";
            public const string ipPort1001 = "192.168.200.25:4999";
            public static DeviceInter Mach1001 = new();
            public static MSR1.Watchlist DashboardData1001 = new(); // struct that carries that info that updates the dashboard
            public static MSR1_Service mSR1service1001 = new();
            private static PLC_Client.PLC_Data Blackbox1001 = new(); // plc data
            private static SQL_Client.SQL_Logger SQL_1001 = new();   // format for sql storage  
        */
        //! test data

        //! thread 1 -> runs the http requests 
        public static void HttpGetData()  // thread
        {
            int focus = 0;
             
            //! List for checking the adaptors
            int Numb_Adaptors = adaptnumb + 1;
            AdapterCheck.Adapterdata[] AdaptorArray = new AdapterCheck.Adapterdata[Numb_Adaptors];

            AdaptorArray = AdapterCheck.InitAdapt(AdaptorArray, Mach_Numb_3321, Mach3321.adapterdata, Tag_3321, ipPort3321);
			AdaptorArray = AdapterCheck.InitAdapt(AdaptorArray, Mach_Numb_3112, Mach3112.adapterdata, Tag_3112, ipPort3112);
			AdaptorArray = AdapterCheck.InitAdapt(AdaptorArray, Mach_Numb_3111, Mach3111.adapterdata, Tag_3111, ipPort3111);
            AdaptorArray = AdapterCheck.InitAdapt(AdaptorArray, Mach_Numb_2321, Mach2321.adapterdata, Tag_2321, ipPort2321);
            AdaptorArray = AdapterCheck.InitAdapt(AdaptorArray, Mach_Numb_2283, Mach2283.adapterdata, Tag_2283, ipPort2283);
            AdaptorArray = AdapterCheck.InitAdapt(AdaptorArray, Mach_Numb_2282, Mach2282.adapterdata, Tag_2282, ipPort2282);
            AdaptorArray = AdapterCheck.InitAdapt(AdaptorArray, Mach_Numb_2281, Mach2281.adapterdata, Tag_2281, ipPort2281);
            AdaptorArray = AdapterCheck.InitAdapt(AdaptorArray, Mach_Numb_2280, Mach2280.adapterdata, Tag_2280, ipPort2280);
            AdaptorArray = AdapterCheck.InitAdapt(AdaptorArray, Mach_Numb_2272, Mach2272.adapterdata, Tag_2272, ipPort2272);
            AdaptorArray = AdapterCheck.InitAdapt(AdaptorArray, Mach_Numb_2271, Mach2271.adapterdata, Tag_2271, ipPort2271);
            AdaptorArray = AdapterCheck.InitAdapt(AdaptorArray, Mach_Numb_2260, Mach2260.adapterdata, Tag_2260, ipPort2260);
            AdaptorArray = AdapterCheck.InitAdapt(AdaptorArray, Mach_Numb_2112, Mach2112.adapterdata, Tag_2112, ipPort2112);
            AdaptorArray = AdapterCheck.InitAdapt(AdaptorArray, Mach_Numb_2111, Mach2111.adapterdata, Tag_2111, ipPort2111);     
            AdaptorArray = AdapterCheck.InitAdapt(AdaptorArray, Mach_Numb_2107, Mach2107.adapterdata, Tag_2107, ipPort2107);
            AdaptorArray = AdapterCheck.InitAdapt(AdaptorArray, Mach_Numb_2105, Mach2105.adapterdata, Tag_2105, ipPort2105);
            AdaptorArray = AdapterCheck.InitAdapt(AdaptorArray, Mach_Numb_2103, Mach2103.adapterdata, Tag_2103, ipPort2103);
            AdaptorArray = AdapterCheck.InitAdapt(AdaptorArray, Mach_Numb_2102, Mach2102.adapterdata, Tag_2102, ipPort2102);

            //AdaptorArray = AdapterCheck.InitAdapt(AdaptorArray, Mach_Numb_1001, Mach1001.adapterdata, Tag_1001, ipPort1001);

            for (int i = 0; i < Numb_Adaptors; i++)
            {
                Console.WriteLine("Connecting Adapter " + AdaptorArray[i].ID + ": " + AdaptorArray[i].URL); 
            }

            Thread.Sleep(250);

               DashboardData3321 = StandardRecovery(DashboardData3321, "", Mach3321, Tag_3321);
                    Blackbox3321 = PLC_Client.BlackBoxRead(Blackbox3321, "1" + Tag_3321, P3321, "BDTRON");  
                        SQL_3321 = SQL_Client.SQL_Init(SQL_3321, "5010", "3321", "BDT_Glue", "BDTRON", LogMode);

			   DashboardData3112 = StandardRecovery(DashboardData3112, "", Mach3112, Tag_3112);
			        Blackbox3112 = PLC_Client.BlackBoxRead(Blackbox3112, Tag_3112, P3112, "Amada");
			            SQL_3112 = SQL_Client.SQL_Init(SQL_3112, "5010", "3112", "Amada_Dynasaw", "Amada",  LogMode);

			   DashboardData3111 = StandardRecovery(DashboardData3111, "", Mach3111, Tag_3111);
                    Blackbox3111 = PLC_Client.BlackBoxRead(Blackbox3111, Tag_3111, P3111, "Amada");
                        SQL_3111 = SQL_Client.SQL_Init(SQL_3111, "5010", "3111", "Marvel_Spartan", "Amada", LogMode);

               DashboardData2321 = StandardRecovery(DashboardData2321, "", Mach2321, Tag_2321);
                    Blackbox2321 = PLC_Client.BlackBoxRead(Blackbox2321, Tag_2321, P2321, "LapMast");
                        SQL_2321 = SQL_Client.SQL_Init(SQL_2321, "5010", "2321", "Lap_Grinder", "LapMast", LogMode);

               DashboardData2283 = StandardRecovery(DashboardData2283, "TWOHEANANDGANTRY", Mach2283, Tag_2283);
                    Blackbox2283 = PLC_Client.BlackBoxRead(Blackbox2283, Tag_2283, P2283, "Mazak"); // retrieve plc blackbox data
                        SQL_2283 = SQL_Client.SQL_Init(SQL_2283, "5010", "2283", "Multiplex_W300Y","Mazak", LogMode); // set up sql data

             DashboardData2282 = StandardRecovery(DashboardData2282, "TWOHEANANDGANTRY", Mach2282, Tag_2282);
                    Blackbox2282 = PLC_Client.BlackBoxRead(Blackbox2282, Tag_2282, P2282, "Mazak"); // retrieve plc blackbox data
                        SQL_2282 = SQL_Client.SQL_Init(SQL_2282, "5010", "2282", "Multiplex_W300Y", "Mazak", LogMode); // set up sql data

               DashboardData2281 = StandardRecovery(DashboardData2281, "TWOHEANANDGANTRY", Mach2281, Tag_2281);
                    Blackbox2281 = PLC_Client.BlackBoxRead(Blackbox2281, Tag_2281, P2281, "Mazak"); // retrieve plc blackbox data
                        SQL_2281 = SQL_Client.SQL_Init(SQL_2281, "5010", "2281", "Multiplex_W200Y", "Mazak", LogMode); // set up sql data

               DashboardData2280 = StandardRecovery(DashboardData2280, "TWOHEAD", Mach2280, Tag_2280);
                    Blackbox2280 = PLC_Client.BlackBoxRead(Blackbox2280, Tag_2280, P2280, "Mazak"); // retrieve plc blackbox data
                        SQL_2280 = SQL_Client.SQL_Init(SQL_2280, "5010", "2280", "Mazatrol_Matrix2", "Mazak", LogMode); // set up sql data
            
               DashboardData2272 = StandardRecovery(DashboardData2272, "TWOHEAD", Mach2272, Tag_2272);
                    Blackbox2272 = PLC_Client.BlackBoxRead(Blackbox2272, Tag_2272, P2272, "Mazak"); // retrieve plc blackbox data
                        SQL_2272 = SQL_Client.SQL_Init(SQL_2272, "5010", "2272", "Mazatrol_HQR-250MSY", "Mazak", LogMode); // set up sql data
            
               DashboardData2271 = StandardRecovery(DashboardData2271, "TWOHEAD", Mach2271, Tag_2271);
                    Blackbox2271 = PLC_Client.BlackBoxRead(Blackbox2271,"1" + Tag_2271, P2271 , "Doosan"); // retrieve plc blackbox data
                        SQL_2271 = SQL_Client.SQL_Init(SQL_2271, "5010", "2271", "Doosan_Puma_TT21000SSY", "Doosan",  LogMode); // set up sql data

               DashboardData2260 = StandardRecovery(DashboardData2260, "ONEHEAD", Mach2260, Tag_2260);
                    Blackbox2260 = PLC_Client.BlackBoxRead(Blackbox2260, Tag_2260, P2260, "Mazak"); // retrieve plc blackbox data
                        SQL_2260 = SQL_Client.SQL_Init(SQL_2260, "5010", "2260", "Mazatrol_Matrix_Nexus", "Mazak", LogMode); // set up sql data

               DashboardData2112 = StandardRecovery(DashboardData2112, "ONEHEAD", Mach2112, Tag_2112);
                   Blackbox2112 = PLC_Client.BlackBoxRead(Blackbox2112, "1" + Tag_2112, P2112   , "Mazak"); // retrieve plc blackbox data
                       SQL_2112 = SQL_Client.SQL_Init(SQL_2112, "5010", "2112", "Mazatrol_QT_Ez_8M", "Mazak", LogMode); // set up sql data

               DashboardData2111 = StandardRecovery(DashboardData2111, "ONEHEAD", Mach2111, Tag_2111);
                    Blackbox2111 = PLC_Client.BlackBoxRead(Blackbox2111, "1" + Tag_2111, P2111, "Mazak"); // retrieve plc blackbox data
                        SQL_2111 = SQL_Client.SQL_Init(SQL_2111, "5010", "2111", "Mazatrol_Smart", "Mazak", LogMode); // set up sql data

              DashboardData2107 = StandardRecovery(DashboardData2107, "ONEHEAD", Mach2107, Tag_2107);
                   Blackbox2107 = PLC_Client.BlackBoxRead(Blackbox2107, "1" + Tag_2107, P2107, "Doosan"); // retrieve plc blackbox data
                       SQL_2107 = SQL_Client.SQL_Init(SQL_2107, "5010", "2107", "Doosan_Puma_2600LY_II", "Doosan", LogMode); // set up sql data
            
               DashboardData2105 = StandardRecovery(DashboardData2105, "ONEHEAD", Mach2105, Tag_2105);
                    Blackbox2105 = PLC_Client.BlackBoxRead(Blackbox2105, "1" + Tag_2105, P2105, "HAAS_"); // retrieve plc blackbox data
                        SQL_2105 = SQL_Client.SQL_Init(SQL_2105, "5010", "2105", "HAAS_VF-2SS", "HAAS_", LogMode); // set up sql data

               DashboardData2103 = StandardRecovery(DashboardData2103, "", Mach2103, Tag_2103);
                    Blackbox2103 = PLC_Client.BlackBoxRead(Blackbox2103, "1" + Tag_2103, P2103, "HAAS_"); // retrieve plc blackbox data
                        SQL_2103 = SQL_Client.SQL_Init(SQL_2103, "5010", "2103", "HAAS_VF-2D", "HAAS_", LogMode); // set up sql data

               DashboardData2102 = StandardRecovery(DashboardData2102, "", Mach2102, Tag_2102);
                    Blackbox2102 = PLC_Client.BlackBoxRead(Blackbox2102, "1"+ Tag_2102, P2102, "HAAS_"); // retrieve plc blackbox data
                        SQL_2102 = SQL_Client.SQL_Init(SQL_2102, "5010", "2102", "HAAS_Cell", "HAAS_", LogMode); // set up sql data

            //    DashboardData1001 = StandardRecovery(DashboardData1001, "", Mach1001);
              //       Blackbox1001 = PLC_Client.BlackBoxRead(Blackbox1001, Tag_1001, P1001); // retrieve plc blackbox data
                        // SQL_1001 = SQL_Client.SQL_Init(SQL_1001, "5010", "2102", "HAAS_Cell", "HAAS_", LogMode); // set up sql data

            while (true)    
            {
                AdaptorArray = AdapterCheck.Adaptercheck(AdaptorArray);
                
                DashboardData3321 = MtMach.Bdtronic.BeckhoffGlueMachine(AdaptorArray[Mach_Numb_3321].status, focus, ipPort3321, DataLimit, Mach3321, DashboardData3321, mSR1service3321, Blackbox3321, "3321 BDTRON");  // 2283 functionality 
                     Blackbox3321 = PLC_Client.BlackBoxRead(Blackbox3321, "1"+Tag_3321, P3321, "BDTRON"); // retrieve plc blackbox data  
                         SQL_3321 = SQL_Client.Update(SQL_3321, Blackbox3321, DashboardData3321, "BDTRON", Tag_3321, "line", false, LogMode); // detect changed data and put it into sql database;
                
                DashboardData3112 = MtMach.Amada.BandPi(AdaptorArray[Mach_Numb_3112].status, ipPort3112, DataLimit, Mach3112, DashboardData3112, mSR1service3112, Blackbox3112, "3112 DYNASAW"); // 3112 Functionality
                     Blackbox3112 =  PLC_Client.BlackBoxRead(Blackbox3112, Tag_3112, P3112, "Amada"); // retrieve plc blackbox data 
					     SQL_3112 = SQL_Client.Update(SQL_3112, Blackbox3112, DashboardData3112, "Amada", Tag_3112, "mach", false, LogMode);
                
                DashboardData3111 = MtMach.Amada.BandPi(AdaptorArray[Mach_Numb_3111].status, ipPort3111, DataLimit, Mach3111, DashboardData3111, mSR1service3111, Blackbox3111, "3111 SPARTAN"); // 3111 Functionality
                     Blackbox3111 = PLC_Client.BlackBoxRead(Blackbox3111, Tag_3111, P3111, "Amada"); // retrieve plc blackbox data 
                         SQL_3111 = SQL_Client.Update(SQL_3111, Blackbox3111, DashboardData3111, "Amada", Tag_3111, "mach", false, LogMode);
                  
                DashboardData2321 = MtMach.Direct.Direct1(AdaptorArray[Mach_Numb_2321].status, ipPort2321, DataLimit, Mach2321, DashboardData2321, mSR1service2321, Blackbox2321, "2321 GRINDER"); // 2321 Functionality
                     Blackbox2321 = PLC_Client.BlackBoxRead(Blackbox2321, Tag_2321, P2321, "LapMast"); // retrieve plc blackbox data 
                         SQL_2321 = SQL_Client.Update(SQL_2321, Blackbox2321, DashboardData2321, "LapMast", Tag_2321 , "mach", false, LogMode);
                
                DashboardData2283 = MtMach.Mazak.Mazatrol_SMOOTHG_Extract(AdaptorArray[Mach_Numb_2283].status, focus, ipPort2283, DataLimit, Mach2283, DashboardData2283, mSR1service2283, Blackbox2283, "2283 MAZAK");  // 2283 functionality 
                     Blackbox2283 = PLC_Client.BlackBoxRead(Blackbox2283, Tag_2283, P2283, "Mazak"); // retrieve plc blackbox data  
                         SQL_2283 = SQL_Client.Update(SQL_2283, Blackbox2283, DashboardData2283, "Mazak",Tag_2283, "mach", false, LogMode); // detect changed data and put it into sql database
               
                DashboardData2282 = MtMach.Mazak.Mazatrol_SMOOTHG_Extract(AdaptorArray[Mach_Numb_2282].status, focus, ipPort2282, DataLimit, Mach2282, DashboardData2282, mSR1service2282, Blackbox2282, "2282 MAZAK");  // 2282 functionality 
                     Blackbox2282 = PLC_Client.BlackBoxRead(Blackbox2282, Tag_2282, P2282, "Mazak"); // retrieve plc blackbox data
                         SQL_2282 = SQL_Client.Update(SQL_2282, Blackbox2282, DashboardData2282, "Mazak", Tag_2282, "mach", false, LogMode); // detect changed data and put it into sql database
                      
                DashboardData2281 = MtMach.Mazak.Mazatrol_SMOOTHG_Extract(AdaptorArray[Mach_Numb_2281].status, focus, ipPort2281, DataLimit, Mach2281, DashboardData2281, mSR1service2281, Blackbox2281, "2281 MAZAK"); // 2281 functionality 
                     Blackbox2281 = PLC_Client.BlackBoxRead(Blackbox2281, Tag_2281, P2281, "Mazak"); // retrieve plc blackbox data
                         SQL_2281 = SQL_Client.Update(SQL_2281, Blackbox2281, DashboardData2281, "Mazak", Tag_2281, "mach", false, LogMode); // detect changed data and put it into sql database
                
                DashboardData2280 = MtMach.Mazak.Mazatrol_Matrix_2(AdaptorArray[Mach_Numb_2280].status, focus, ipPort2280, DataLimit, Mach2280, DashboardData2280, mSR1service2280, Blackbox2280, "2280 MAZAK"); // 2280 functionality 
                     Blackbox2280 = PLC_Client.BlackBoxRead(Blackbox2280, Tag_2280, P2280, "Mazak"); // retrieve plc blackbox data
                         SQL_2280 = SQL_Client.Update(SQL_2280, Blackbox2280, DashboardData2280, "Mazak", Tag_2280, "mach", false, LogMode); // detect changed data and put it into sql database
                     
                DashboardData2272 = MtMach.Mazak.Mazatrol_HQR250M(AdaptorArray[Mach_Numb_2272].status, focus, ipPort2272, DataLimit, Mach2272, DashboardData2272, mSR1service2272, Blackbox2272, "2272 MAZAK");
                     Blackbox2272 = PLC_Client.BlackBoxRead(Blackbox2272, Tag_2272, P2272, "Mazak"); // retrieve plc blackbox data
                         SQL_2272 = SQL_Client.Update(SQL_2272, Blackbox2272, DashboardData2272, "Mazak", Tag_2272, "mach", true, LogMode); // detect changed data and put it into sql database// detect changed data and put it into sql database
                
                DashboardData2271 = MtMach.Doosan.DoosanPuma_TT21000SSY(AdaptorArray[Mach_Numb_2271].status, focus, ipPort2271, DataLimit, Mach2271, DashboardData2271, mSR1service2271, Blackbox2271, "2271 DOOSAN"); 
                     Blackbox2271 = PLC_Client.BlackBoxRead(Blackbox2271, "1"+ Tag_2271, P2271, "Doosan"); // retrieve plc blackbox data
                         SQL_2271 = SQL_Client.Update(SQL_2271, Blackbox2271, DashboardData2271, "Doosan",Tag_2271, "mach", true, LogMode); // detect changed data and put it into sql database
                 
                DashboardData2260 = MtMach.Mazak.Nexus(AdaptorArray[Mach_Numb_2260].status, focus, ipPort2260, DataLimit, Mach2260, DashboardData2260, mSR1service2260, Blackbox2260, "2260 NEXUS"); // 2260 functionality
                     Blackbox2260 = PLC_Client.BlackBoxRead(Blackbox2260, Tag_2260, P2260, "Mazak"); // retrieve plc blackbox data
                         SQL_2260 = SQL_Client.Update(SQL_2260, Blackbox2260, DashboardData2260, "Mazak", Tag_2260, "mach", false, LogMode); // detect changed data and put it into sql database
                
                DashboardData2111 = MtMach.Mazak.MazatrolSmart(AdaptorArray[Mach_Numb_2111].status, focus, ipPort2111, DataLimit, Mach2111, DashboardData2111, mSR1service2111, Blackbox2111, "2111 MAZAK"); // 2111 functionality
                     Blackbox2111 = PLC_Client.BlackBoxRead(Blackbox2111, "1" + Tag_2111, P2111, "Mazak"); // retrieve plc blackbox data
                         SQL_2111 = SQL_Client.Update(SQL_2111, Blackbox2111, DashboardData2111, "Mazak", Tag_2111, "line", false, LogMode); // detect changed data and put it into sql database
                                                                                                                               // 
                DashboardData2112 = MtMach.Mazak.MazatrolSmart(AdaptorArray[Mach_Numb_2112].status, focus, ipPort2112, DataLimit, Mach2112, DashboardData2112, mSR1service2112, Blackbox2112, "2112 MAZAK"); // 2112 functionality
                     Blackbox2112 = PLC_Client.BlackBoxRead(Blackbox2112, "1" + Tag_2112, P2112, "Mazak"); // retrieve plc blackbox data
                         SQL_2112 = SQL_Client.Update(SQL_2112, Blackbox2112, DashboardData2112, "Mazak", Tag_2112, "line", false, LogMode); // detect changed data and put it into sql database    
                
                 DashboardData2107 = MtMach.Doosan.DoosanPuma_2600LYII(AdaptorArray[Mach_Numb_2107].status, focus, ipPort2107, DataLimit, Mach2107, DashboardData2107, mSR1service2107, Blackbox2107, "2107 DOOSAN");     
                      Blackbox2107 = PLC_Client.BlackBoxRead(Blackbox2107, "1" + Tag_2107, P2107, "Doosan"); // retrieve plc blackbox data
                          SQL_2107 = SQL_Client.Update(SQL_2107, Blackbox2107, DashboardData2107, "Doosan", Tag_2107, "mach", false, LogMode); // detect changed data and put it into sql database 
                
                 DashboardData2105 = MtMach.Haas.HAASVF_2SS(AdaptorArray[Mach_Numb_2105].status, ipPort2105, DataLimit, Mach2105, DashboardData2105, mSR1service2105, Blackbox2105, "2105 HAAS");
                      Blackbox2105 = PLC_Client.BlackBoxRead(Blackbox2105, "1" + Tag_2105, P2105, "HAAS_"); // retrieve plc blackbox data
                          SQL_2105 = SQL_Client.Update(SQL_2105, Blackbox2105, DashboardData2105, "HAAS_", Tag_2105, "mach", false, LogMode); // detect changed data and put it into sql database 
                
                 DashboardData2103 = MtMach.Haas.HaasVF2D(AdaptorArray[Mach_Numb_2103].status, ipPort2103, DataLimit, Mach2103, DashboardData2103, mSR1service2103, Blackbox2103, "2102M HAAS");
                      Blackbox2103 = PLC_Client.BlackBoxRead(Blackbox2103, "1" + Tag_2103, P2103, "HAAS_"); // retrieve plc blackbox data
                          SQL_2103 = SQL_Client.Update(SQL_2103, Blackbox2103, DashboardData2103, "HAAS_", Tag_2103,"mach", false, LogMode); // detect changed data and put it into sql database 
                
                 DashboardData2102 = MtMach.Haas.Haas_69664(AdaptorArray[Mach_Numb_2102].status, ipPort2102, DataLimit, Mach2102, DashboardData2102, mSR1service2102, Blackbox2102, "2102L HAAS");
                      Blackbox2102 = PLC_Client.BlackBoxRead(Blackbox2102, "1" + Tag_2102, P2102, "HAAS_"); // retrieve plc blackbox data
                         SQL_2102 = SQL_Client.Update(SQL_2102, Blackbox2102, DashboardData2102, "HAAS_", Tag_2102, "mach", false, LogMode); // detect changed data and put it into sql database 
                
                // DashboardData1001 = MtMach.Direct.Direct1(AdaptorArray[Mach_Numb_1001].status, ipPort1001, DataLimit, Mach1001, DashboardData1001, mSR1service1001, Blackbox1001, "SQUAG"); // 2321 Functionality
                  //    Blackbox1001 = PLC_Client.BlackBoxRead(Blackbox1001, Tag_1001, P1001); // retrieve plc blackbox data
                     //SQL_1001 = SQL_Client.Update(SQL_1001, Blackbox1001, DashboardData1001, "HAAS_", Tag_2102, "mach", LogMode); // detect changed data and put it into sql database 
                 
                if (focus<4)
                {
                    focus++;
                }
                else 
                {
                    focus = 0;
                }
                Thread.Sleep(delay2);
            }
        }

        public struct SequenceUnit
        {
            public string id;
            public long seq;
            public string value;
        }
        
     // device Information class
        public class DeviceInter
        {
            public bool start;
            public AdapterCheck.Adapterdata adapterdata = new();
            public bool fail;
            public string? failmessage;
            
            //public Httpget.HttpData[] Input = new Httpget.HttpData[DataLimit+2];
            //public Httpget.HttpData[] Output = new Httpget.HttpData[DataLimit+2];
            public int rowhelper;
          
            public SequenceUnit Sequence = new();
            public List<SequenceUnit> SequenceDir = []; // this is for getting rid of repeat sequences when using a http sample query
            public bool post;   
        }

     //! Functions 
        // Initialize the values from saved data in sql
        public static MSR1.Watchlist StandardRecovery(MSR1.Watchlist DashboardData,string datatype, DeviceInter Mach, string WorkCenter)
        {          
            
            try 
            {
                DashboardData = SQL_Client.RecoverPastData(DashboardData, WorkCenter, datatype);
            }
            catch
            {
                DashboardData = BailoutRecover(DashboardData, datatype, Mach);
            }
            
            DashboardData.DisplayMachineState = DashboardData.MachineState;
            DashboardData.DisplayMachineState2 = DashboardData.MachineState2;
             
            DashboardData.AdapterOnline = true;
            Mach.fail = true;
            Mach.start = true;

            return DashboardData;
        }
        // error with standard recovery needs a bailout
        public static MSR1.Watchlist BailoutRecover(MSR1.Watchlist DashboardData, string datatype, DeviceInter Mach)
        {            
            DashboardData.MachineState= "OFFLINE";
            DashboardData.MachineState2= "OFFLINE";
            DashboardData.DisplayMachineState = DashboardData.MachineState;
            DashboardData.DisplayMachineState2 = DashboardData.MachineState2;

            DashboardData.AdapterOnline = true;
            Mach.fail = true;
            DashboardData.PartCount = "Initializing";    
            
            switch (datatype)
            {
            case "smooth":
                DashboardData.Feed="100";
                DashboardData.Feed2="100";
                DashboardData.Rapid="100";
                DashboardData.Rapid2="100";
                DashboardData.Spindle="100";
                DashboardData.Spindle2="100";
                DashboardData.Gantry="100";
                break;
            case "matrix2":
                DashboardData.Feed="100";
                DashboardData.Feed2="100";
                DashboardData.Rapid="100";
                DashboardData.Rapid2="100";
                DashboardData.Spindle="100";
                DashboardData.Spindle2="100";
                break;
            case "nexus":
                DashboardData.Feed="100";
                DashboardData.Rapid="100";
                DashboardData.Spindle="100";
                break;
            case "TWOHEANANDGANTRY":
                DashboardData.Feed="100";
                DashboardData.Feed2="100";
                DashboardData.Rapid="100";
                DashboardData.Rapid2="100";
                DashboardData.Spindle="100";
                DashboardData.Spindle2="100";
                DashboardData.Gantry="100";
                break;
            case "TWOHEAD":
                DashboardData.Feed="100";
                DashboardData.Feed2="100";
                DashboardData.Rapid="100";
                DashboardData.Rapid2="100";
                DashboardData.Spindle="100";
                DashboardData.Spindle2="100";
                break;
            case "ONEHEAD":
                DashboardData.Feed="100";
                DashboardData.Rapid="100";
                DashboardData.Spindle="100";
                break;
            default:

                break;
            }
        
            Mach.start = true;

            return DashboardData;
        }
     //! Functions 
    }
}