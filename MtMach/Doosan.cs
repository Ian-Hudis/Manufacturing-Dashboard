using static MTConnectDashboard.DataOutput;
using static MTConnectDashboard.MSR1;
using static MTConnectDashboard.PLC_Client;

namespace MTConnectDashboard.MtMach
{
    public class Doosan
    {
        // for Doosan Puma (2107)
        public static Watchlist DoosanPuma_2600LYII(bool AdaptorStatus, int focus, string iport, int dataLimit, DeviceInter Mach, Watchlist DashboardData, MSR1_Service mSR1service, PLC_Data plcdata, string table_address)
        {
            //string URLPath = "?path=//MTConnectDevices/Devices/Device/Components/Controller" + "/Components/Path/DataItems";

            string URLPath = "?&path=//MTConnectDevices/Devices/Device/Components/Controller/Components/Path[@id='path1']/DataItems";
            // only one head
            /*
            #pragma warning disable IDE0066 // Convert switch statement to expression
            if (Mach.start != true)
            {
                switch (focus)
                {
                    case 0: // partcount and material number
                        URLPath += "/DataItem[@id='estop' or @id='path1_comment' or @id='path1_part_count' or @id='path2_part_count']";
                        break;
                    case 1: // state
                        URLPath += "/DataItem[@id = 'mode' or @id = 'path1_execution'  or @id='path1_program_number_current' or @id='path1_program_number_main']";
                        break;
                    case 2:  //overrides
                        URLPath += "/DataItem[@id='path1_feedrate_override' or @id='path1_spindle_override' or @id='path1_rapid_override']";
                        break;
                    case 3: // repeat data just to double check the state
                        URLPath += "/DataItem[@id = 'mode' or @id = 'path1_execution'  or @id='path1_program_number_current' or @id='path1_program_number_main']";
                        break;
                    default:
                        URLPath += "/DataItem[@id = 'mode' or @id = 'path1_execution'  or @id='path1_program_number_current' or @id='path1_program_number_main']";
                        break;
                }
            }
            #pragma warning restore IDE0066 // Convert switch statement to expression
            */

            // get the xml data from MTConnect
            if (AdaptorStatus)
            {
                //Mach.Input =  Httpget.HttpGet(iport, URLPath, dataLimit, Mach.start, "doosan");
                DashboardData = HttpDirect.Get(DashboardData, iport, URLPath, "doosan", 1, true);
            }
            else
            {
                DashboardData.MachineState = "OFFLINE";
                DashboardData.Cycletime = TimeSpan.Zero;
                DashboardData.ActualCycletime = TimeSpan.Zero;
                // DashboardData.PartCount = "Agent Offline";
            }

            DashboardData.Rapid = RapidConverter(DashboardData.Rapid);

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
                BailoutRecover(DashboardData, "doosan", Mach);
            }

            //! idle timer
            /*
            TimeSpan timelimit = new(0, 2, 0); // time until the machine is considered idle 
            DashboardData = IdleTimer(DashboardData, timelimit);
            */
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
            //! Idle Timer
            DashboardData.DisplayMachineState2 = DashboardData.MachineState2;

            /*
            //Extraction of Data
            for (int i = 0; i < DataLimit+1; i++)
            {
                Sort(Mach, i);
                FilterForLatest(iport, Mach);
                //Mach.Output = Mach.Input; // bypass

                if (Mach.post== true)
                {
                    // update the dashboard
                    DashboardData = Dash_Puma_2600LYII(
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

            // extract the relevant plc data and show it on the dashboard
            DashboardData = PLC_Interpret(DashboardData, plcdata);

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

        // for Doosan Puma (2271)
        public static Watchlist DoosanPuma_TT21000SSY(bool AdaptorStatus, int focus, string iport, int dataLimit, DeviceInter Mach, Watchlist DashboardData, MSR1_Service mSR1service, PLC_Data plcdata, string table_address)    
        {
            string URLPath = "?&path=//MTConnectDevices/Devices/Device/Components/Controller" + "/Components/Path/DataItems" +
                "/DataItem[@id='mode' or @id = 'path1_execution' or @id='path2_execution' or @id='path1_part_count' or @id='path2_part_count' or " +
                "@id='path1_rapid_override' or @id='path1_feedrate_override' or @id='path1_spindle_override' or @id='path2_rapid_override' or @id='path2_feedrate_override' or @id='path2_spindle_override']";
            // has 2 heads

            // get the xml data from MTConnect
            if (AdaptorStatus)
            {
               // Mach.Input =  Httpget.HttpGet(iport, URLPath, dataLimit, Mach.start, "doosan");
                DashboardData = HttpDirect.Get(DashboardData, iport, URLPath,"doosan",2,true);
                if (DashboardData.controllerMode != DashboardData.controllerMode2)
                {
                    DashboardData.controllerMode2 = DashboardData.controllerMode;
                    DashboardData.direct_timez2 = DashboardData.direct_timez;
                }
                
            }
            else
            {
                DashboardData.MachineState = "OFFLINE";
                DashboardData.MachineState2 = "OFFLINE";
                DashboardData.PartCount = "Agent Offline";
                DashboardData.Cycletime = TimeSpan.Zero;
                DashboardData.Cycletime2 = TimeSpan.Zero;
                DashboardData.ActualCycletime = TimeSpan.Zero;
            }

            DashboardData.Rapid = RapidConverter(DashboardData.Rapid);
            DashboardData.Rapid2 = RapidConverter(DashboardData.Rapid2);

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
                BailoutRecover(DashboardData, "doosan", Mach);
            }

            //! idle timer
            TimeSpan timelimit = new(0, 0, 16); // time until the machine is considered idle 
            DashboardData = IdleTimer(DashboardData, timelimit);
            DashboardData = IdleTimerH2(DashboardData, timelimit);
            //! idle timer

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
                    DashboardData = Dash_Puma_TT21000SSY(
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
                mSR1service.UpdateList(table_address, "Overrides", "", DashboardData);
                Thread.Sleep(delay1);
            }
            */

            DashboardData=TimePass(DashboardData); // makes time in state keep going even when the board doesnt have an update (makes it look good)

            DashboardData = MakePredictions(DashboardData);

            mSR1service.UpdateList(table_address, "Overrides", "", DashboardData);

            if (Mach.start==true && Mach.fail == false)
            {
                Mach.start = false;
              //  Console.WriteLine(iport + " Connecting...");
            }

            return DashboardData;
        }

        private static string RapidConverter(string value)
        {
            if (value.Contains("255"))
            {
                value = "100";
            }
            else if (value.Contains("253"))
            {
                value = "25";
            }
            else if (value.Contains("252"))
            {
                value = "0";
            }

            return value;
        } // value converter for the doosan rapids.

    }
}