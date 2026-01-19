using static MTConnectDashboard.DataOutput;
using static MTConnectDashboard.MSR1;
using static MTConnectDashboard.MtMach.Mazak;
using static MTConnectDashboard.PLC_Client;

namespace MTConnectDashboard.MtMach
{
    public class Mazak
    {
        // for the  Mazatrol SMOOTHG (2283, 2282, and 2281) 
        public static Watchlist Mazatrol_SMOOTHG_Extract(bool AdaptorStatus, int focus, string iport, int dataLimit, DeviceInter Mach, Watchlist DashboardData, MSR1_Service mSR1service, PLC_Data plcdata, string table_address)
        {
            string URLPath = "&path=//MTConnectDevices/Devices/Device/Components/Controller";
            /*
            string URLPath = "&path=//DataItem[@id='glovrd']//MTConnectDevices/Devices/Device/Components/Controller/Components/Path[@id='path1' or @id='path2']/DataItems" +
                "/DataItem[@id='mode' or @id='mode2' or @id='execution' or @id='execution2' or @id='PartCountAct' or @id='PartCountAct2' or @id='Fovr' " +
                "or @id='Sovr' or @id='Frapidovr' or @id='Fovr2' or @id='S2ovr' or @id='Frapidovr2' or @id='glovrd']";
            */
            /*
            if (Mach.start != true) { 
               
                switch (focus)
                {
                    case 0: // partcount
                        URLPath += "/DataItem[@id='activeprogram_cmt' or @id='program_cmt' or @id='PartCountAct' or @id='activeprogram_cmt2' or @id='program_cmt2' or @id='PartCountAct2']";
                        break;
                    case 1: // state
                        URLPath += "/DataItem[@id='mode' or @id = 'execution'  or @id='program' or @id='activeprog']";
                        break;
                    case 2:  //overrides
                        URLPath += "/DataItem[@id='Fovr' or @id='Sovr' or @id='Frapidovr' or @id='Fovr2' or @id='S2ovr' or @id='Frapidovr2']";
                        break;
                    case 3: // head 2
                        URLPath += "/DataItem[@id='mode2' or @id = 'execution2'  or @id='program2' or @id='activeprog2']";
                        break;
                    default: // gantry
                        if (iport != ipPort2282 && iport != ipPort2281) // making an exception for 2282 and for now because their gantry location is in a different spot
                        {
                            URLPath += "/DataItem[@id='glovrd']";
                        }
                        else
                        {
                            URLPath = "&path=//MTConnectDevices/Devices/Device/Components/Controller/DataItems/DataItem[@id='glovrd']";
                        }
                        break;
                }
                
            }
            */

            // get the xml data from MTConnect
            if (AdaptorStatus)
            {
                // Mach.Input =  Httpget.HttpGet(iport, URLPath, dataLimit, Mach.start, "mazak");
                DashboardData = HttpDirect.Get(DashboardData, iport, URLPath, "mazak", 2, true);

                //Mach = HttpDirect.DataStore(Mach,DashboardData);
            }
            else
            {
                DashboardData.MachineState = "OFFLINE";
                DashboardData.MachineState2 = "OFFLINE";
                DashboardData.Prod_Order = "";
                DashboardData.material = "";
                DashboardData.PartCount = "0";
                DashboardData.PartCount2 = "0";
                DashboardData.Cycletime = TimeSpan.Zero;
                DashboardData.Cycletime2 = TimeSpan.Zero;
                DashboardData.ActualCycletime = TimeSpan.Zero;
            }

            // tells if the machine is offline
            Mach.fail  = Httpget.ConFail.MachineOffline;
            Mach.failmessage = Httpget.ConFail.Prob;

            // send adaptor info to dashboard
            if (Mach.fail==false && DashboardData.AdapterOnline == false)
            {
                DashboardData.AdapterOnline = true;
                Console.WriteLine(iport + " Online;");
            }
            else if (Mach.fail==true && DashboardData.AdapterOnline == true)
            {
                DashboardData.AdapterOnline = false;
                Console.WriteLine(iport + " Adaptor Down;");
                //DashboardData.PartCount = "Agent Offline";
                BailoutRecover(DashboardData, "smooth", Mach);
            }

			//! Idle Timer
			if (double.TryParse(plcdata.idealLoadTime, out double timelimit_in_minutes))
			{
				TimeSpan timelimit = TimeSpan.FromMinutes(timelimit_in_minutes);
				DashboardData = IdleTimer(DashboardData, timelimit);
			}
			else // the plc data isnt available
			{
				DashboardData.DisplayMachineState = DashboardData.MachineState; // this is for dealling with the loading display issue -> 5/8/2024
			}
			//! Idle Timer
			//! Idle Timer 2
			if (double.TryParse(plcdata.idealLoadTime, out double timelimit_in_minutes2))
			{
				TimeSpan timelimit = TimeSpan.FromMinutes(timelimit_in_minutes2);
				DashboardData = IdleTimerH2(DashboardData, timelimit);
			}
			else // the plc data isnt available
			{
				DashboardData.DisplayMachineState2 = DashboardData.MachineState2; // this is for dealling with the loading display issue -> 5/8/2024
			}
			//! Idle Timer 2

			// extract the relevant plc data and show it on the dashboard
			DashboardData = PLC_Interpret(DashboardData, plcdata);

            // Show the dashboard that the machine is loading without messing up the cycltime calculation.
            //DashboardData.DisplayMachineState = DashboardData.MachineState;
           // DashboardData.DisplayMachineState2 = DashboardData.MachineState2;

            // the part that actually updates the dashboard
            if (DashboardData.idealLoadTime != TimeSpan.Zero && DashboardData.idealLoadTime != null)
            {
                mSR1service.UpdateList(table_address, "Overrides", DashboardData.IdleTimerDisplay, DashboardData); // handloading
            }
            else
            {
                mSR1service.UpdateList(table_address, "Overrides", ""/*"Gantry Override"*/, DashboardData); // using the gantry
            }

            //if (Mach.post != true)
            //{
                DashboardData = TimePass(DashboardData); // makes time in state keep going even when the board doesnt have an update (makes it look good)
            //}
            DashboardData = MakePredictions(DashboardData);

            // mt connect stream setting adjustment
            if (Mach.start==true && Mach.fail == false)
            {
                Mach.start = false;
               // Console.WriteLine(iport + " Connecting...");
            }
            return DashboardData;
        }

        // for the Mazatrol Matrix 2 (2280)
        public static Watchlist Mazatrol_Matrix_2(bool AdaptorStatus, int focus, string iport, int dataLimit, DeviceInter Mach, Watchlist DashboardData, MSR1_Service mSR1service, PLC_Data plcdata, string table_address)
        {
            /*
            string URLPath = "&path=//MTConnectDevices/Devices/Device/Components/Controller/Components/Path[@id='path1' or @id='path2']/DataItems/"
                                + "DataItem[@id='pc' or @id='exec' or @id = 'mode' or @id = 'pcmt' or @id= 'pfr' or @id='pfo' or @id='Sovr']" //or @id='pgm' "//or @id='spgm' "//or"
                                //+"@id='pc2' or @id='exec2' or @id = 'mode2' or @id = 'pcmt2' or @id= 'pfr2' or @id='pfo2' or @id='S2ovr' or @id='pgm2' or @id='spgm2'"
                                ;
            */
            string URLPath = "&path=//MTConnectDevices/Devices/Device/Components/Controller/Components/Path[@id='path1' or @id='path2']/DataItems";
            /*
            #pragma warning disable IDE0066 // Convert switch statement to expression
            if (Mach.start != true)
            {
                switch (focus)
                {
                    case 0: // partcount
                        URLPath += "/DataItem[@id='spcmt' or @id='pcmt' or @id='pc' or @id='pc2']";

                        break;
                    case 1: // state
                        URLPath += "/DataItem[@id='mode' or @id = 'exec'  or @id='pgm' or @id='spgm']";
                        break;
                    case 2:  //overrides
                        URLPath += "/DataItem[@id='pfo' or @id='Sovr' or @id='pfr']";
                        break;
                    case 3:
                        URLPath += "/DataItem[@id='mode' or @id = 'exec'  or @id='pgm' or @id='spgm']";
                        break;
                    default:
                        URLPath += "/DataItem[@id='pfo2' or @id='S2ovr' or @id='pfr2' or @id='mode2' or @id='exec2']";
                        break;
                }
            }
            #pragma warning restore IDE0066 // Convert switch statement to expression
            */
            //Mach.start = false;
            // get the xml data from MTConnect
            if (AdaptorStatus)
            {
                //Mach.Input =  Httpget.HttpGet(iport, URLPath, dataLimit, Mach.start, "mazak");
                DashboardData = HttpDirect.Get(DashboardData, iport, URLPath, "mazak", 2, true);
            }
            else
            {
                DashboardData.MachineState = "OFFLINE";
                DashboardData.MachineState2 = "OFFLINE";
                DashboardData.Cycletime = TimeSpan.Zero;
                DashboardData.Cycletime2 = TimeSpan.Zero;
                DashboardData.ActualCycletime = TimeSpan.Zero;
                // DashboardData.PartCount = "Agent Offline";
            }

            // tells if the machine is offline
            Mach.fail  = Httpget.ConFail.MachineOffline;
            Mach.failmessage = Httpget.ConFail.Prob;

            // send adaptor info to dashboard
            if (Mach.fail==false && DashboardData.AdapterOnline == false)
            {
                DashboardData.AdapterOnline = true;
                Console.WriteLine(iport + " Online;");
            }
            else if (Mach.fail==true && DashboardData.AdapterOnline == true)
            {
                DashboardData.AdapterOnline = false;
                Console.WriteLine(iport + " Adaptor Down;");
                BailoutRecover(DashboardData, "matrix2", Mach);
            }

			//! Idle Timer
			if (double.TryParse(plcdata.idealLoadTime, out double timelimit_in_minutes))
			{
				TimeSpan timelimit = TimeSpan.FromMinutes(timelimit_in_minutes);
				DashboardData = IdleTimer(DashboardData, timelimit);
			}
			else // the plc data isnt available
			{
                DashboardData.DisplayMachineState = DashboardData.MachineState; // this is for dealling with the loading display issue -> 5/8/2024
			}
			//! Idle Timer
			//! Idle Timer 2
			if (double.TryParse(plcdata.idealLoadTime, out double timelimit_in_minutes2))
			{
				TimeSpan timelimit = TimeSpan.FromMinutes(timelimit_in_minutes2);
				DashboardData = IdleTimerH2(DashboardData, timelimit);
			}
			else // the plc data isnt available
			{
                DashboardData.DisplayMachineState2 = DashboardData.MachineState2; // this is for dealling with the loading display issue -> 5/8/2024
			}
			//! Idle Timer 2

			// extract the relevant plc data and show it on the dashboard
			DashboardData = PLC_Interpret(DashboardData, plcdata);
            /*
            //Extraction of Data
            for (int i = 0; i < DataLimit+1; i++)
            {
                Sort(Mach, i);
                FilterForLatest(iport, Mach);

                if (Mach.post== true)
                {
                    // update the dashboard
                    DashboardData = Dash_Mazak.SortStoreMatrix2(
                           Mach.Output[Mach.rowhelper].timestamp,
                           Mach.Output[Mach.rowhelper].dataItemId,
                           Mach.Output[Mach.rowhelper].value,
                           DashboardData);
                }
                else
                {
                    DashboardData=TimePass(DashboardData); // makes time in state keep going even when the board doesnt have an update (makes it look good)
                }

                // the part that actually updates the dashboard
                if (DashboardData.idealLoadTime != TimeSpan.Zero && DashboardData.idealLoadTime != null)
                {
                    mSR1service.UpdateList(table_address, "Overrides", DashboardData.IdleTimerDisplay, DashboardData); // handloading
                }
                else
                {
                    mSR1service.UpdateList(table_address, "Overrides", "", DashboardData); // using the gantry
                }

                DashboardData.DisplayMachineState = DashboardData.MachineState; // Show the dashboard that the machine is loading without messing up the cycltime calculation.
                DashboardData.DisplayMachineState2 = DashboardData.MachineState2;

                Thread.Sleep(delay1);
            }
            */

            DashboardData = MakePredictions(DashboardData);


            DashboardData=TimePass(DashboardData); // makes time in state keep going even when the board doesnt have an update (makes it look good)


            // the part that actually updates the dashboard
            if (DashboardData.idealLoadTime != TimeSpan.Zero && DashboardData.idealLoadTime != null)
            {
                mSR1service.UpdateList(table_address, "Overrides", DashboardData.IdleTimerDisplay, DashboardData); // handloading
            }
            else
            {
                mSR1service.UpdateList(table_address, "Overrides", "", DashboardData); // using the gantry
            }

            DashboardData.DisplayMachineState = DashboardData.MachineState; // Show the dashboard that the machine is loading without messing up the cycltime calculation.
            DashboardData.DisplayMachineState2 = DashboardData.MachineState2;


            if (Mach.start==true && Mach.fail == false)
            {
                // Mach.start = false;
              //  Console.WriteLine(iport + " Connecting...");
            }

            return DashboardData;
        }

        // for the  Mazak HQR-250MSY (2272)
        public static Watchlist Mazatrol_HQR250M(bool AdaptorStatus, int focus, string iport, int dataLimit, DeviceInter Mach, Watchlist DashboardData, MSR1_Service mSR1service, PLC_Data plcdata, string table_address)
        {
            string URLPath = "&path=//MTConnectDevices/Devices/Device/Components/Controller/Components/Path[@id='path1' or @id='path2']/DataItems"+
                    "/DataItem[@id='mode' or @id='mode2' or @id='execution' or @id='execution2' or @id='PartCountAct' or @id='PartCountAct2' or @id='Fovr' " +
                    "or @id='Sovr' or @id='Frapidovr' or @id='Fovr2' or @id='S2ovr' or @id='Frapidovr2']";

            /*
            if (Mach.start != true)
            {
                switch (focus)
                {
                    case 0: // partcount
                        URLPath += "/DataItem[@id='activeprogram_cmt' or @id='program_cmt' or @id='PartCountAct' or @id='activeprogram_cmt2' or @id='program_cmt2' or @id='PartCountAct2']";
                        break;
                    case 1: // state
                        URLPath += "/DataItem[@id='mode' or @id = 'execution'  or @id='program' or @id='activeprog']";
                        break;
                    case 2:  //overrides
                        URLPath += "/DataItem[@id='Fovr' or @id='Sovr' or @id='Frapidovr' or @id='Fovr2' or @id='S2ovr' or @id='Frapidovr2']";
                        break;
                    case 3: // head 2
                        URLPath += "/DataItem[@id='mode2' or @id = 'execution2'  or @id='program2' or @id='activeprog2']";
                        break;
                    default:
                        URLPath += "/DataItem[@id='activeprogram_cmt' or @id='program_cmt' or @id='PartCountAct' or @id='activeprogram_cmt2' or @id='program_cmt2' or @id='PartCountAct2']";
                        break;
                }
            }
            */

            // get the xml data from MTConnect
            if (AdaptorStatus)
            {
                // Mach.Input =  Httpget.HttpGet(iport, URLPath, dataLimit, Mach.start, "mazak");
                DashboardData = HttpDirect.Get(DashboardData, iport, URLPath, "mazak", 2, true);
            }
            else
            {
                DashboardData.MachineState = "OFFLINE";
                DashboardData.MachineState2 = "OFFLINE";
                DashboardData.Cycletime = TimeSpan.Zero;
                DashboardData.Cycletime2 = TimeSpan.Zero;
                DashboardData.ActualCycletime = TimeSpan.Zero;
            }

            // tells if the machine is offline
            Mach.fail  = Httpget.ConFail.MachineOffline;
            Mach.failmessage = Httpget.ConFail.Prob;

            // send adaptor info to dashboard
            if (Mach.fail==false && DashboardData.AdapterOnline == false)
            {
                DashboardData.AdapterOnline = true;
                Console.WriteLine(iport + " Online;");
            }
            else if (Mach.fail==true && DashboardData.AdapterOnline == true)
            {
                DashboardData.AdapterOnline = false;
                Console.WriteLine(iport + " Adaptor Down;");
                //DashboardData.PartCount = "Agent Offline";
                BailoutRecover(DashboardData, "matrix2", Mach);
            }

            //! idle timer (Bar feeder setup)
            TimeSpan timelimit = new(0, 0, 16); // time until the machine is considered idle 
            DashboardData = IdleTimer(DashboardData, timelimit);
			DashboardData = IdleTimerH2(DashboardData, timelimit);
			//! idle timer

			// the part that actually updates the dashboard
			mSR1service.UpdateList(table_address, "Overrides", "" /*DashboardData.IdleTimerDisplay*/, DashboardData);
            
           // DashboardData.DisplayMachineState = DashboardData.MachineState; // Show the dashboard that the machine is loading without messing up the cycltime calculation.
           // DashboardData.DisplayMachineState2 =DashboardData.MachineState2; 

            // extract the relevant plc data and show it on the dashboard
            DashboardData = PLC_Interpret(DashboardData, plcdata);
            /*
            //Extraction of Data
            for (int i = 0; i < DataLimit+1; i++)
            {
                Sort(Mach, i); // reformat data
                FilterForLatest(iport, Mach);
                //Mach.Output = Mach.Input; // bypass


                if (Mach.post== true)
                {
                    // update the dashboard
                    DashboardData = Dash_Mazak.SortStoreSmoothG(
                           Mach.Output[Mach.rowhelper].timestamp,
                           Mach.Output[Mach.rowhelper].dataItemId,
                           Mach.Output[Mach.rowhelper].value,
                           DashboardData);
                }
                else
                {
                    DashboardData = TimePass(DashboardData); // makes time in state keep going even when the board doesnt have an update (makes it look good)
                }

                // Thread.Sleep(delay1);
            }
            */
            DashboardData = TimePass(DashboardData); // makes time in state keep going even when the board doesnt have an update (makes it look good)

            DashboardData = MakePredictions(DashboardData);

            // mt connect stream setting adjustment
            if (Mach.start==true && Mach.fail == false)
            {
                Mach.start = false;
                // Console.WriteLine(iport + " Connecting...");
            }

            return DashboardData;
        }

        // for the nexus (2260)
        public static Watchlist Nexus(bool AdaptorStatus, int focus, string iport, int dataLimit, DeviceInter Mach, Watchlist DashboardData, MSR1_Service mSR1service, PLC_Data plcdata, string table_address)
        {
            /*
            string URLPath = "&path=//MTConnectDevices/Devices/Device/Components/Controller/Components/Path[@id='path1' or @id='path2']/DataItems/"
                                + "DataItem[@id='pc' or @id='exec' or @id = 'mode' or @id = 'pcmt' or @id= 'pfr' or @id='pfo' or @id='Sovr']" //or @id='pgm' "//or @id='spgm' "//or"
                                //+"@id='pc2' or @id='exec2' or @id = 'mode2' or @id = 'pcmt2' or @id= 'pfr2' or @id='pfo2' or @id='S2ovr' or @id='pgm2' or @id='spgm2'"
                                ;
            */
            string URLPath = "&path=//MTConnectDevices/Devices/Device/Components/Controller/Components/Path[@id='path1']/DataItems";
            /*
            #pragma warning disable IDE0066 // Convert switch statement to expression
            if (Mach.start != true)
            {
                switch (focus)
                {
                    case 0: // partcount
                        URLPath += "/DataItem[@id='spcmt' or @id='pcmt' or @id='pc']";
                        break;
                    case 1: // state
                        URLPath += "/DataItem[@id='mode' or @id = 'exec'  or @id='pgm' or @id='spgm']";
                        break;
                    case 2:  //overrides
                        URLPath += "/DataItem[@id='pfo' or @id='Sovr' or @id='pfr']";
                        break;
                    case 3:
                        URLPath += "/DataItem[@id='mode' or @id = 'exec'  or @id='pgm' or @id='pc']";
                        break;
                    default:
                        URLPath += "/DataItem[@id='pgm' or @id='pc']";
                        break;
                }
            }
            #pragma warning restore IDE0066 // Convert switch statement to expression
            */
            //Mach.start = false;
            // get the xml data from MTConnect
            if (AdaptorStatus)
            {
                //Mach.Input =  Httpget.HttpGet(iport, URLPath, dataLimit, Mach.start, "mazak");
                DashboardData = HttpDirect.Get(DashboardData, iport, URLPath, "mazak", 1, true);
            }
            else
            {
                DashboardData.MachineState = "OFFLINE";
                DashboardData.Cycletime = TimeSpan.Zero;
                DashboardData.ActualCycletime = TimeSpan.Zero;
                //  DashboardData.PartCount = "Agent Offline";
            }

            // tells if the machine is offline
            Mach.fail  = Httpget.ConFail.MachineOffline;
            Mach.failmessage = Httpget.ConFail.Prob;

            // send adaptor info to dashboard
            if (Mach.fail==false && DashboardData.AdapterOnline == false)
            {
                DashboardData.AdapterOnline = true;
                Console.WriteLine(iport + " Online;");
            }
            else if (Mach.fail==true && DashboardData.AdapterOnline == true)
            {
                DashboardData.AdapterOnline = false;
                Console.WriteLine(iport + " Adaptor Down;");
                BailoutRecover(DashboardData, "nexus", Mach);
            }
            
            //! idle timer
                //TimeSpan timelimit = new(0, 6, 30); // time until the machine is considered idle (2 minutes)
                //DashboardData = IdleTimer(DashboardData, timelimit);
                //! idle timer
                //! Idle Timer
                if (double.TryParse(plcdata.idealLoadTime, out double timelimit_in_minutes))
                {
                    TimeSpan timelimit = TimeSpan.FromMinutes(timelimit_in_minutes);
                    DashboardData = IdleTimer(DashboardData, timelimit);
                }
                else
                {
                    DashboardData.DisplayMachineState = DashboardData.MachineState; // Show the dashboard that the machine is loading without messing up the cycltime calculation.
                }
                DashboardData.DisplayMachineState2 = DashboardData.MachineState2;
            //! Idle Timer

            // extract the relevant plc data and show it on the dashboard
            DashboardData = PLC_Interpret(DashboardData, plcdata);
            /*
            //Extraction of Data
            for (int i = 0; i < DataLimit+1; i++)
            {
                Sort(Mach, i);
                FilterForLatest(iport, Mach);
               // Mach.Output = Mach.Input;  // bypass for the sample filter

                if (Mach.post== true)
                {
                    // update the dashboard
                    DashboardData = Dash_Mazak.SortStoreNexus(
                           Mach.Output[Mach.rowhelper].timestamp,
                           Mach.Output[Mach.rowhelper].dataItemId,
                           Mach.Output[Mach.rowhelper].value,
                           DashboardData);
                }
                else
                {
                    DashboardData = TimePass(DashboardData); // makes time in state keep going even when the board doesnt have an update (makes it look good)
                }                
                   if (DashboardData.update)
                    {
                    }
                
                Thread.Sleep(delay1);
              }
            */

            DashboardData = TimePass(DashboardData); // makes time in state keep going even when the board doesnt have an update (makes it look good)
            DashboardData = MakePredictions(DashboardData);

            // the part that actually updates the dashboard
            mSR1service.UpdateList(table_address, "Overrides", DashboardData.IdleTimerDisplay, DashboardData);


            if (Mach.start==true && Mach.fail == false)
            {
                Mach.start = false;
              //  Console.WriteLine(iport + " Connecting...");
            }

            return DashboardData;
        }

        // for Mazatrol Smart (2111 and 2112)
        public static Watchlist MazatrolSmart(bool AdaptorStatus, int focus, string iport, int dataLimit, DeviceInter Mach, Watchlist DashboardData, MSR1_Service mSR1service, PLC_Data plcdata, string table_address)
        {
            string URLPath = "&path=//MTConnectDevices/Devices/Device/Components/Controller/Components/Path[@id='path1']/DataItems";
            /*
            #pragma warning disable IDE0066 // Convert switch statement to expression
            
            if (Mach.start != true)
            {
                switch (focus)
                {
                    case 0: // partcount
                        URLPath += "/DataItem[@id='activeprogram_cmt' or @id='program_cmt']";
                        break;
                    case 1: // state
                        URLPath += "/DataItem[ @id = 'execution']";
                        break;
                    case 2:  //overrides
                        URLPath += "/DataItem[@id='Fovr' or @id='Sovr' or @id='Frapidovr']";
                        break;
                    case 3:
                        URLPath += "/DataItem[@id='PartCountAct']";
                        break;
                    default: // gantry
                        URLPath += "/DataItem[@id='mode' or @id='program' or @id='activeprog']";
                        break;
                }
            }
            #pragma warning restore IDE0066 // Convert switch statement to expression
            */

            // get the xml data from MTConnect
            if (AdaptorStatus)
            {
                //Mach.Input =  Httpget.HttpGet(iport, URLPath, dataLimit, true, "mazak");
                DashboardData = HttpDirect.Get(DashboardData, iport, URLPath, "mazak", 1, true);
            }
            else
            {
                DashboardData.MachineState = "OFFLINE";
                DashboardData.Cycletime = TimeSpan.Zero;
                DashboardData.ActualCycletime = TimeSpan.Zero;
                //     DashboardData.PartCount = "Agent Offline";
            }

            // tells if the machine is offline
            Mach.fail  = Httpget.ConFail.MachineOffline;
            Mach.failmessage = Httpget.ConFail.Prob;

            // send adaptor info to dashboard
            if (Mach.fail==false && DashboardData.AdapterOnline == false)
            {
                DashboardData.AdapterOnline = true;
                Console.WriteLine(iport + " Online;");
            }
            else if (Mach.fail==true && DashboardData.AdapterOnline == true)
            {
                DashboardData.AdapterOnline = false;
                Console.WriteLine(iport + " Adaptor Down;");
                BailoutRecover(DashboardData, "smart", Mach);
            }

            //! Idle Timer
            if(double.TryParse(plcdata.idealLoadTime, out double timelimit_in_minutes))
            {
                TimeSpan timelimit = TimeSpan.FromMinutes(timelimit_in_minutes);
                DashboardData = IdleTimer(DashboardData, timelimit);
            }
            else // the plc data isnt available
            {
                DashboardData.DisplayMachineState = DashboardData.MachineState; // this is for dealling with the loading display issue -> 5/8/2024
            }
            //! Idle Timer
            DashboardData.DisplayMachineState2 = DashboardData.MachineState2;

            // extract the relevant plc data and show it on the dashboard
            DashboardData = PLC_Interpret(DashboardData, plcdata);

            /*
            //Extraction of Data
            for (int i = 0; i < DataLimit+1; i++)
            {
                Sort(Mach, i); // reformat data
                FilterForLatest(iport, Mach);
               // Mach.Output = Mach.Input; // bypass for the sample filter 

                if (Mach.post== true)
                {
                    // update the dashboard
                    DashboardData = Dash_Mazak.SortStoreMatrixSmart(
                           Mach.Output[Mach.rowhelper].timestamp,
                           Mach.Output[Mach.rowhelper].dataItemId,
                           Mach.Output[Mach.rowhelper].value,
                           DashboardData);
                }
                else
                {
                    DashboardData= TimePass(DashboardData); // makes time in state keep going even when the board doesnt have an update (makes it look good)
                }
                
                   if (DashboardData.update)
                    {

                    }
                

            Thread.Sleep(delay1);
            }
            */

            DashboardData= TimePass(DashboardData); // makes time in state keep going even when the board doesnt have an update (makes it look good)

            DashboardData = MakePredictions(DashboardData);

            // the part that actually updates the dashboard
            mSR1service.UpdateList(table_address, "Overrides", DashboardData.IdleTimerDisplay, DashboardData);

            if (Mach.start==true && Mach.fail == false)
            {
              //  Mach.start = false;
              //  Console.WriteLine(iport + " Connecting...");
            }
            else if (Mach.fail != false)
            {
                DashboardData.MachineState = "OFFLINE";
               // DashboardData.PartCount = "Agent Offline";
                DashboardData.material = "";
                // watchlist.Cycletime=0;
            }

            return DashboardData;
        }

    }
}