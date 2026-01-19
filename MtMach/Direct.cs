using static MTConnectDashboard.DataOutput;
using static MTConnectDashboard.MSR1;
using static MTConnectDashboard.PLC_Client;

namespace MTConnectDashboard.MtMach
{
    public class Direct // for a self made standardization 
    {
      //  private static DateTime Directtime = DateTime.Now; // initialize to the time the program starts

        public static Watchlist Direct1(bool AdaptorStatus, string iport, int dataLimit, DeviceInter Mach, Watchlist DashboardData, MSR1_Service mSR1service, PLC_Data plcdata, string table_address) 
        {
            string URLPath = "";
            
            // get the xml data from MTConnect
            if (AdaptorStatus)
            {
                DashboardData = HttpDirect.Get(DashboardData, iport, URLPath, "direct",1, true);
            }
            else
            {
                DashboardData.MachineState = "OFFLINE";
                DashboardData.PartCount = "Agent Offline";
                DashboardData.Cycletime = TimeSpan.Zero;
                DashboardData.ActualCycletime = TimeSpan.Zero;
            }
           // Console.WriteLine(DashboardData.Execution + " " + DashboardData.MachineState + " " + DashboardData.DisplayMachineState + " " + DashboardData.IdleTimerStatus + " " + DashboardData.IdleTimer.TotalSeconds);

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
            //! Idle Timer 2
            DashboardData.DisplayMachineState2 = DashboardData.MachineState2; // "N/A" -> need to change this if multiple heads

            // extract the relevant plc data and show it on the dashboard
            DashboardData = PLC_Interpret(DashboardData, plcdata);
            //plcdata.k_partcount2 = plcdata.k_partcount1;

            // the part that actually updates the dashboard
            mSR1service.UpdateList(table_address, "", DashboardData.IdleTimerDisplay, DashboardData);

            DashboardData = TimePass(DashboardData); // makes time in state keep going even when the board doesnt have an update (makes it look good)

            DashboardData = MSR1.MakePredictions(DashboardData);


            // mt connect stream setting adjustment
            if (Mach.start==true && Mach.fail == false)
            {
                Mach.start = false;
                // Console.WriteLine(iport + " Connecting...");
            }

            return DashboardData;
        }

      //  public static string prevGrinderPartCount = "0";
        
    }
}