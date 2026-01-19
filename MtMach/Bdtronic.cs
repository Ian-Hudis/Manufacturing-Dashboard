using static MTConnectDashboard.DataOutput;
using static MTConnectDashboard.MSR1;
using static MTConnectDashboard.PLC_Client;

namespace MTConnectDashboard.MtMach
{
    public class Bdtronic
    {
        private static bool loading;

        // 3321
        public static Watchlist BeckhoffGlueMachine(bool AdaptorStatus, int focus, string iport, int dataLimit, DeviceInter Mach, Watchlist DashboardData, MSR1_Service mSR1service, PLC_Data plcdata, string table_address)
        {
            string URLPath = "";

            // get the xml data from MTConnect
            if (AdaptorStatus)
            {
                //Mach.Input =  Httpget.HttpGet(iport, URLPath, dataLimit, Mach.start, "bdtronic");
                DashboardData = HttpDirect.Get(DashboardData, iport, URLPath, "bdtronic", 1, true);
            }
            else
            {
                DashboardData.MachineState = "OFFLINE";
                DashboardData.Prod_Order = "";
                DashboardData.material = "";
                DashboardData.PartCount = "";
                DashboardData.Cycletime = TimeSpan.Zero;
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
                BailoutRecover(DashboardData, "BDTRONIC", Mach);
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
            DashboardData.DisplayMachineState2 = DashboardData.MachineState2;

            // extract the relevant plc data and show it on the dashboard
            DashboardData = PLC_Interpret(DashboardData, plcdata);

            DateTime timeZ = DateTime.Now;

            if (plcdata.k_partcount1 != DashboardData.PartCount)
            {
                DashboardData.PartCount = plcdata.k_partcount1; // make the kiosk part count the same as the dashboard partcount
                DashboardData.direct_timez.PartTime = timeZ;
            }

            if (DashboardData.MachineState == "IDLE" && !loading) // t flip switch logic
            {
                DashboardData.BeginCycle1 = timeZ;
                loading = true;
            }
            else if(DashboardData.MachineState == "RUNNING" && loading)
            {
                DashboardData.LoadCycle1 = timeZ - DashboardData.BeginCycle1;
                DashboardData.Cycletime = DashboardData.MachCycle1 + DashboardData.LoadCycle1;
                loading = false;
            }

            if(DashboardData.LoadCycle1 > new TimeSpan(1, 0, 0)) // safe assumption that all loads will be less than an hour
            {
                DashboardData.LoadCycle1 = TimeSpan.Zero;
                DashboardData.prevtimez = DateTime.MinValue;
            }


            // cycletime
            if (DashboardData.PartCount == null)
            {
                DashboardData.PartCount="0";
            }            

			if (DashboardData.prevtimez != DateTime.MinValue && DashboardData.partCountRepeat != DashboardData.PartCount && (DashboardData.KioskState != "nojob"))
			{
				DashboardData.MachCycle1 = CycleTimeCalc(DashboardData.prevtimez, timeZ).Add(new TimeSpan(0,0,2)); // factor in 2 seconds of lag
                DashboardData.prevtimez = timeZ; // sets the previous partcount time to the next part count time
				DashboardData.partCountRepeat = DashboardData.PartCount;

                DashboardData.Cycletime = DashboardData.MachCycle1 + DashboardData.LoadCycle1;
            }
            else if (DashboardData.prevtimez == DateTime.MinValue) // first part
            {
                DashboardData.Cycletime = TimeSpan.Zero;
                DashboardData.LoadCycle1 = TimeSpan.Zero;
                DashboardData.prevtimez = timeZ; // sets the time when the 1st part occurs
                DashboardData.partCountRepeat = DashboardData.PartCount;
            }
            else if (DashboardData.KioskState == "nojob" || DashboardData.KioskState == "timeout")
            {
                DashboardData.Cycletime = TimeSpan.Zero;
                DashboardData.MachCycle1 = TimeSpan.Zero;
                DashboardData.LoadCycle1 = TimeSpan.Zero;
            }

            //Console.WriteLine(DashboardData.Cycletime);
            DashboardData.ActualCycletime = DashboardData.Cycletime;  // part to part
            DashboardData.MachinCycle = DashboardData.MachCycle1; // running to part complete
            DashboardData.Loadtime = DashboardData.LoadCycle1; // idle to running
            /*
            //Extraction of Data
            for (int i = 0; i < DataLimit+1; i++)
            {
                Sort(Mach, i); // reformat data
                FilterForLatest(iport, Mach);
                //Mach.Output = Mach.Input;

                if (Mach.post== true)              
                {
                    // update the dashboard
                    DashboardData = Dash_GlueMachine1(
                           Mach.Output[Mach.rowhelper].timestamp,
                           Mach.Output[Mach.rowhelper].dataItemId,
                           Mach.Output[Mach.rowhelper].value,
                           DashboardData); 
                }
                else
                {
                    DashboardData= TimePass(DashboardData); // makes time in state keep going even when the board doesnt have an update (makes it look good)
                   // Console.WriteLine(DashboardData.TimeInState.ToString());
                }

                // the part that actually updates the dashboard
                mSR1service.UpdateList(table_address, DashboardData.BdtronicWatchlist.ActSpeed.ToString(), DashboardData.IdleTimerDisplay, DashboardData);

                Thread.Sleep(delay1);
            }
            */

            mSR1service.UpdateList(table_address, DashboardData.BdtronicWatchlist.ActSpeed.ToString(), DashboardData.IdleTimerDisplay, DashboardData);

            DashboardData= TimePass(DashboardData); // makes time in state keep going even when the board doesnt have an update (makes it look good)

            DashboardData = MakePredictions(DashboardData);

            if (Mach.start==true && Mach.fail == false)
            {
                //  Mach.start = false;
                //  Console.WriteLine(iport + " Connecting...");
            }
            else if (Mach.fail != false)
            {
               // DashboardData.MachineState = "OFFLINE";
                // DashboardData.PartCount = "Agent Offline";
               // DashboardData.material = "";
                // watchlist.Cycletime=0;
            }

            return DashboardData;
        }

        public struct BDtronicKit // everything that can be directly taken from the mtconnect data in the Glue Machine
        {
            //MTConnect
            public short estop; // This is primarily for telling if the machine is down.

            public short Automatic;
            public short Manual;
            public short Red;
            public short Green;
            public short Yellow;

            public short H1Position;
            public short H2Position;

            public short H1PartComplete; // becomes 0 when a part is starting and becomes 1 when a part is finished
            public short H2PartComplete; // becomes 0 when a part is starting and becomes 1 when a part is finished

            public short prevH1PartComplete; // from tracking the change for a partcount
            public short prevH2PartComplete;

            public short H1ChamberState;
            public short H2ChamberState;

            public int ActSpeed; // The speed the machine is moving

            public short HandPress; // twoHandPressed is 1 when the operator is pressing it

            // assist tools for getting the proper part count
            public int partcountInt;
            public bool partstart;
            public string prevMaterialNumber;
        }

    }

    
}
