using static MTConnectDashboard.DataOutput;
using static MTConnectDashboard.MSR1;
using static MTConnectDashboard.PLC_Client;

namespace MTConnectDashboard.MtMach
{
    public class Haas
    {
        // for HAAS VF (2105)
        public static Watchlist HAASVF_2SS(bool AdaptorStatus,  string iport, int dataLimit, DeviceInter Mach, Watchlist DashboardData, MSR1_Service mSR1service, PLC_Data plcdata, string table_address)
        {
            string URLPath = "path=//MTConnectDevices/Devices/Device/Components/Controller/DataItems";

            // get the xml data from MTConnect
            if (AdaptorStatus)
            {
                // Mach.Input =  Httpget.HttpGet(iport, URLPath, dataLimit, Mach.start, "haas");
                DashboardData = HttpDirect.Get(DashboardData, iport, URLPath, "haas", 1, true);
            }
            else
            {
                DashboardData.MachineState = "OFFLINE";
                DashboardData.Cycletime = TimeSpan.Zero;
                DashboardData.ActualCycletime = TimeSpan.Zero;
                //  DashboardData.PartCount = "Agent Offline";
            }
            DashboardData.Cycletime2 = TimeSpan.Zero;

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
                BailoutRecover(DashboardData, "haas", Mach);
            }
            
            //! idle timer
            // TimeSpan timelimit = new(0, 2, 0); // time until the machine is considered idle 
            //DashboardData = IdleTimer(DashboardData, timelimit);
            //! idle timer
            if (double.TryParse(plcdata.idealLoadTime, out double timelimit_in_minutes))
            {
                TimeSpan timelimit = TimeSpan.FromMinutes(timelimit_in_minutes);
                DashboardData = IdleTimer(DashboardData, timelimit);
            }
            else
            {
                DashboardData.DisplayMachineState = DashboardData.MachineState; // Show the dashboard that the machine is loading without messing up the cycltime calculation.
            }
            DashboardData.DisplayMachineState2 = "N/A";//DashboardData.MachineState2; // No head 2 value
            //! idle timer

            // extract the relevant plc data and show it on the dashboard
            DashboardData = PLC_Interpret(DashboardData, plcdata);
            /*
            //Extraction of Data
            for (int i = 0; i < DataLimit+1; i++)
            {
                Sort(Mach, i);
                FilterForLatest(iport, Mach);
               // Mach.Output = Mach.Input; //  bypass

                if (Mach.post== true)
                {
                    // update the dashboard
                    DashboardData = Dash_VF_2SS(
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
                mSR1service.UpdateList(table_address, "Overrides", DashboardData.IdleTimerDisplay, DashboardData);

                Thread.Sleep(delay1);
            }
            */
            DashboardData=TimePass(DashboardData); // makes time in state keep going even when the board doesnt have an update (makes it look good)

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

        // data used to make sure 2105 is actually running and not just left on "run" mode
        public struct RunningCheckData
        {
            public string prevpart;
            public DateTime LastPartcompleted;
            public bool ActuallyRunning;
        }

        public static RunningCheckData RunCheck(RunningCheckData runningCheckData, string partnumber)
        {
            if (partnumber != runningCheckData.prevpart) // a part has been completed
            {
                runningCheckData.LastPartcompleted = DateTime.Now;
                runningCheckData.prevpart= partnumber;
            }

            if (DateTime.Now < runningCheckData.LastPartcompleted.AddHours(1)) // its been an hour since the last part was completed
            {
                runningCheckData.ActuallyRunning = true;
            }
            else
            {
                runningCheckData.ActuallyRunning = false;
            }

            return runningCheckData;
        }
        public static RunningCheckData Runningcheckdata = new();

            // this is for the dashboard data for the HAAS VF (2105)
            // this should work with any NGC HAAS

        // for HAAS mill (2103)
        public static Watchlist HaasVF2D(bool AdaptorStatus, string iport, int dataLimit, DeviceInter Mach, Watchlist DashboardData, MSR1_Service mSR1service, PLC_Data plcdata, string table_address)
        {
            string URLPath = "path=//MTConnectDevices/Devices/Device/Components/Controller/DataItems";

           // Console.WriteLine(iport + "  " + DashboardData.controllerMode  +"  "+DashboardData.Execution);
            // get the xml data from MTConnect
            if (AdaptorStatus)
            {
                // Mach.Input =  Httpget.HttpGet(iport, URLPath, dataLimit, Mach.start, "haas");
                DashboardData = HttpDirect.Get(DashboardData, iport, URLPath, "haas", 1, true);
            }
            else
            {
                DashboardData.MachineState = "OFFLINE";
                DashboardData.Cycletime = TimeSpan.Zero;
                DashboardData.ActualCycletime = TimeSpan.Zero;
                //   DashboardData.PartCount = "Agent Offline";
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
                BailoutRecover(DashboardData, "haas", Mach);
            }

            // haas oftens does not log the mode so if it is unavailable it really is just in automatic mode
            if (DashboardData.controllerMode =="UNAVAILABLE")
            {
                DashboardData.controllerMode = "AUTOMATIC";
              //  Console.WriteLine(DashboardData.controllerMode);
            }

            //! idle timer
           // TimeSpan timelimit = new(0, 2, 0); // time until the machine is considered idle 
            //DashboardData = IdleTimer(DashboardData, timelimit);
            //! idle timer
                if (double.TryParse(plcdata.idealLoadTime, out double timelimit_in_minutes))
                {
                    TimeSpan timelimit = TimeSpan.FromMinutes(timelimit_in_minutes);
                    DashboardData = IdleTimer(DashboardData, timelimit.Add(new TimeSpan(0, 0, 10))); // add 10 seconds for latency
                }
                else
                {
                    DashboardData.DisplayMachineState = DashboardData.MachineState; // Show the dashboard that the machine is loading without messing up the cycltime calculation.
                }
            //! idle timer
            DashboardData.DisplayMachineState2 = "N/A"; //DashboardData.MachineState2; // No head 2 value

            // extract the relevant plc data and show it on the dashboard
            DashboardData = PLC_Interpret(DashboardData, plcdata);
            /*
            //Extraction of Data
            for (int i = 0; i < DataLimit+1; i++)
            {
                Sort(Mach, i);
                FilterForLatest(iport, Mach);
                //Mach.Output = Mach.Input;

               if (Mach.post== true)
               {
                    // update the dashboard
                    DashboardData = Dash_VF2D(
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
                mSR1service.UpdateList(table_address, "", DashboardData.IdleTimerDisplay, DashboardData);

                Thread.Sleep(delay1);
            }
            */
            DashboardData=TimePass(DashboardData); // makes time in state keep going even when the board doesnt have an update (makes it look good)

            DashboardData = MakePredictions(DashboardData);

            // the part that actually updates the dashboard
            mSR1service.UpdateList(table_address, "", DashboardData.IdleTimerDisplay, DashboardData);

            if (Mach.start==true && Mach.fail == false)
            {
                Mach.start = false;
                //  Console.WriteLine(iport + " Connecting...");
            }

            return DashboardData;
        }

        // for HAAS mill (2102)
        public static Watchlist Haas_69664(bool AdaptorStatus, string iport, int dataLimit, DeviceInter Mach, MSR1.Watchlist DashboardData, MSR1_Service mSR1service, PLC_Data plcdata, string table_address)
        {
            string URLPath = "path=//MTConnectDevices/Devices/Device/Components/Controller/DataItems";

            //Console.WriteLine(iport + "  " + DashboardData.controllerMode  +"  "+DashboardData.Execution);

            // get the xml data from MTConnect
            if (AdaptorStatus)
            {
                DashboardData.controllerMode = "AUTOMATIC";
                //Mach.Input =  Httpget.HttpGet(iport, URLPath, dataLimit, Mach.start, "haas");
                DashboardData = HttpDirect.Get(DashboardData, iport, URLPath, "haas", 1, true);
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
                BailoutRecover(DashboardData, "haas", Mach);
            }

            //! 2102 idle timer
            //TimeSpan timelimit = new(0, 2, 0); // time until the machine is considered idle 
            //DashboardData = IdleTimer(DashboardData, timelimit);
            //! 2102 idle timer
            //! idle timer
            if (double.TryParse(plcdata.idealLoadTime, out double timelimit_in_minutes))
            {
                TimeSpan timelimit = TimeSpan.FromMinutes(timelimit_in_minutes);
                DashboardData = IdleTimer(DashboardData, timelimit.Add(new TimeSpan(0, 0, 10))); // add 10 seconds for latency
            }
            else
            {
                DashboardData.DisplayMachineState = DashboardData.MachineState; // Show the dashboard that the machine is loading without messing up the cycltime calculation.
            }
            //! idle timer
            DashboardData.DisplayMachineState2 = "N/A";//DashboardData.MachineState2; // No head 2 value

            // extract the relevant plc data and show it on the dashboard
            DashboardData = PLC_Interpret(DashboardData, plcdata);

            /*
            //Extraction of Data
            if (Mach.start != true)
            {
                for (int i = 0; i < DataLimit+1; i++)
                {
                    Sort(Mach, i);
                    FilterForLatest(iport, Mach);
                    //Mach.Output = Mach.Input;

                    if (Mach.post== true)
                    {
                        // update the dashboard
                        DashboardData = Dash_VF2D(
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
                    mSR1service.UpdateList(table_address, "", DashboardData.IdleTimerDisplay, DashboardData);

                    Thread.Sleep(delay1);
                }
            }
            */
            DashboardData=TimePass(DashboardData); // makes time in state keep going even when the board doesnt have an update (makes it look good)

            DashboardData = MakePredictions(DashboardData);

            // the part that actually updates the dashboard
            mSR1service.UpdateList(table_address, "", DashboardData.IdleTimerDisplay, DashboardData);

            if (Mach.start==true && Mach.fail == false)
            {
                DashboardData.MachineState = "OFFLINE";
                DashboardData.Execution = "Initializing";
                Mach.start = false;
                // Console.WriteLine(iport + " Connecting... ");
            }

            //Console.WriteLine(iport + " " + DashboardData.controllerMode +" "+ DashboardData.Execution +" "+ DashboardData.MachineState);
            return DashboardData;
        }

            // this is for the dashboard data for the Legacy HAAS (Hass 2012 and 2103)
            // this should work with non-NGC HAAS
    }
}
