using static MTConnectDashboard.DataOutput;
using static MTConnectDashboard.MSR1;
using static MTConnectDashboard.PLC_Client;

namespace MTConnectDashboard.MtMach
{
    public class Amada
    {
        public static Watchlist BandPi(bool AdaptorStatus, string iport, int dataLimit, DeviceInter Mach, Watchlist DashboardData, MSR1_Service mSR1service, PLC_Data plcdata, string table_address)
        {
            string URLPath = "";

            // get the xml data from MTConnect
            if (AdaptorStatus)
            {
                DashboardData = HttpDirect.Get(DashboardData, iport, URLPath, "amada",1, false);
               // Mach.Input =  Httpget.HttpGet(iport, URLPath, dataLimit, Mach.start, "amada");
            }
            else
            {
                DashboardData.MachineState = "OFFLINE";
                DashboardData.material = "Agent Offline";
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
                //Console.WriteLine(iport + " Online;");
            }
            else if (Mach.fail==true && DashboardData.AdapterOnline == true)
            {
                DashboardData.AdapterOnline = false;
                // Console.WriteLine(iport + " Adaptor Down;");
                //DashboardData.PartCount = "Agent Offline";
                BailoutRecover(DashboardData, "", Mach);
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
            plcdata.k_partcount2 = plcdata.k_partcount1;

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
                    DashboardData = Dash_BandPi(
                           Mach.Output[Mach.rowhelper].timestamp,
                           Mach.Output[Mach.rowhelper].tag,      // We use the MTConnect tag instead of the ID to avoid syncronisity issues
                           Mach.Output[Mach.rowhelper].value,
                           DashboardData);

                }
                else
                {
                    DashboardData = TimePass(DashboardData); // makes time in state keep going even when the board doesnt have an update (makes it look good)
                }

                Thread.Sleep(delay1);
            }
            */
            DashboardData = TimePass(DashboardData); // makes time in state keep going even when the board doesnt have an update (makes it look good)

            DashboardData = MakePredictions(DashboardData);

            // the part that actually updates the dashboard
            if (DashboardData.idealLoadTime != TimeSpan.Zero /*&& DashboardData.idealLoadTime != null*/)
            {
                mSR1service.UpdateList(table_address, "", DashboardData.IdleTimerDisplay, DashboardData); // handloading
            }
            else
            {
                mSR1service.UpdateList(table_address, "", "", DashboardData); // using the gantry
            }

            // mt connect stream setting adjustment
            if (Mach.start==true && Mach.fail == false)
            {
                Mach.start = false;
                // Console.WriteLine(iport + " Connecting...");
            }
            return DashboardData;
        }

        public struct Var_amada
        {
            public string machinestatus;
        }

    }

}