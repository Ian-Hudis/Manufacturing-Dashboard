using MTConnectDashboard.MtMach;
using System;
using System.Reflection.PortableExecutable;
using System.Xml;
using static MTConnectDashboard.DataOutput;
using static MTConnectDashboard.MSR1;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace MTConnectDashboard
{
    // a simplified down version of the original http get class

    public class HttpDirect
    {
        private struct HttpData
        {
            public string timestamp;
            public string dataItemId;
            public string value;
            public string tag;
            public long seq;
        }

        public static Watchlist Get(Watchlist CurrentData, string MachAddress, string URLPath, string machtype, int Heads, bool loader)
        {
            string URLaddress;

            switch (machtype)
            {
                case "direct": // this is for when I make the mtconnect adapter and agent myself and can set it up exactly how it is needed
                    URLaddress = "http://" + MachAddress;
                    break;
                case "amada": // really its anything using bandpi not just amada
                    URLaddress = "http://" + MachAddress + "/current.xml";
                    break;
                case "haas":
                    URLaddress = "http://" + MachAddress + "/current?" + URLPath;
                    break;
                case "doosan":
                    URLaddress = "http://" + MachAddress + "/current?" + URLPath;
                    break;
                case "bdtronic":
                    URLaddress = "http://" + MachAddress + "/current";
                    break;
                default: // default is the mazak
                    URLaddress = "http://" + MachAddress + "/current?" + URLPath;
                    break;
            }

            try
            {
                CurrentData.AdapterOnline = true; // the url address is reachable

                switch(machtype)
                {
                    case "amada":
                        CurrentData = AmadaRead(CurrentData, URLaddress); 
                        break;

                    case "bdtronic":
                        CurrentData = BdtronicRead(CurrentData, URLaddress);
                        break;

                    case "direct":
                        CurrentData=ReadDirect(CurrentData, URLaddress);
                        break;

                    case "doosan":
                        CurrentData = StandardMTConnectRead(CurrentData, URLaddress, Heads, loader);
                       // CurrentData.controllerMode2 = CurrentData.controllerMode;
                        break;
                    case "haas":
                        CurrentData = StandardMTConnectRead(CurrentData, URLaddress, Heads, loader);
                        if(Heads == 1)
                        {
                            CurrentData.PartCount2 = "N/A";
                        }
                        break;
                    default: // mazak
                        CurrentData = StandardMTConnectRead(CurrentData,URLaddress, Heads, loader);
                        break;

                }

            }
            catch (Exception ex) // the url address is not reachable
            {
                /*
                if (CurrentData.AdapterOnline == true)
                {
                    Console.WriteLine(DateTime.Now.ToString()+ " " +machtype + " " + MachAddress + " is offline.");
                }
                */
                CurrentData.AdapterOnline = false; 
            }

            return CurrentData;
        }

        private static Watchlist ReadDirect(Watchlist CurrentData, string URLaddress) // for  direct mtconnect agents 
        {
            XmlTextReader reader = new(URLaddress);

            string machinestatus = "";
            string mode = "";
            string execution = "";
            string partcount = "";

            //! find the data from mtconnect agent
            while (reader.Read())
            {
                if (reader.IsStartElement())
                {
                    switch (reader.Name)
                    {
                        case "MachineStatus":
                            CurrentData.direct_timez.MStateTime = DateTime.Parse(reader.GetAttribute("timestamp").ToString());
                            machinestatus = reader.ReadElementContentAsString();
                            break;

                        case "ControllerMode":
                            CurrentData.direct_timez.Modetime = DateTime.Parse(reader.GetAttribute("timestamp").ToString());
                            mode = reader.ReadElementContentAsString();
                            break;

                        case "Execution":
                            CurrentData.direct_timez.Executiontime = DateTime.Parse(reader.GetAttribute("timestamp").ToString());
                            execution = reader.ReadElementContentAsString();
                            break;

                        case "PartCount":
                            CurrentData.direct_timez.PartTime = DateTime.Parse(reader.GetAttribute("timestamp").ToString());
                            partcount = reader.ReadElementContentAsString();
                            break;
                           
                    }                    
                }
                
            }// while
            reader.Close();
            //Console.WriteLine(machinestatus + " " + mode + " " + execution + " " + partcount);
            //! find the data from mtconnect agent

            //! reconstruct the data
            CurrentData.var_amada.machinestatus = machinestatus; // using the amada structure to save the machine machine status
            if (mode!="" && CurrentData.controllerMode!=mode)
            {
                CurrentData.controllerMode = mode;
                CurrentData.direct_timez.MStateTime = CurrentData.direct_timez.Modetime; // update the time when the mode changes as the state change
            }
            if (execution!="" && CurrentData.Execution!=execution)
            {
                CurrentData.Execution = execution;
                CurrentData.direct_timez.MStateTime = CurrentData.direct_timez.Executiontime;  // update the time when the execution changes as the state change
            }

            if (partcount != CurrentData.PartCount && partcount!= "" && partcount != " " && partcount != null)
            {
                CurrentData.PartCount = partcount;
               // CurrentData.direct_timez.PartTime = partcounttimestamp;
            }


            if (machinestatus == "ON")
            {
                CurrentData = MSR1.MtConnect_StateFinder(CurrentData, CurrentData.direct_timez.MStateTime, CurrentData.direct_timez2.MStateTime, 1, true); // there is no head2
                //Console.WriteLine(CurrentData.Execution + " " + CurrentData.MachineState + " " + CurrentData.DisplayMachineState + " " + CurrentData.IdleTimerStatus + " " + CurrentData.IdleTimer.TotalSeconds);
            }
            else // the machine is off or the adapter is down
            {
                CurrentData.MachineState = "OFFLINE";
                CurrentData.Cycletime=TimeSpan.Zero;

                if (CurrentData.MachineState != CurrentData.StateSet)
                {
                    CurrentData.PrevStatetime = CurrentData.direct_timez.MStateTime; //Timegrab(CurrentData.direct_timez.MStateTime, CurrentData.PrevStatetime);
                    CurrentData.MachineTimeOffset = DateTime.Now - CurrentData.PrevStatetime;
                    CurrentData.StateSet = CurrentData.MachineState;
                }
            }

            if (double.TryParse(CurrentData.PartCount, out double PartCount) && double.TryParse(CurrentData.PartCountTotal, out double SAP_Quantity) && SAP_Quantity!=0)
            {
                int partsperCycle;
                if (CurrentData.baseQuantity<1)
                {
                    partsperCycle = 1;
                }
                else
                {
                    partsperCycle = CurrentData.baseQuantity;
                }
                // find the percent completion
                CurrentData.Percent_Completion = Math.Round(PartCount/SAP_Quantity * 100, 2);
                // Estimated Job Time
                CurrentData.Estimated_Job_Time = CurrentData.MachineCycleTime * (SAP_Quantity/partsperCycle);
                // time till completion
                CurrentData.Time_Left_till_Completion = CurrentData.Estimated_Job_Time * 0.01 * (100 - CurrentData.Percent_Completion);
            }


            if (partcount!= "" && partcount != " " && partcount != null && partcount != "UNAVAILABLE")  //! to prevent misreads from affecting the dashboard
            {
                CurrentData.PartCount = partcount;
                CurrentData = CycletimeCalculator(CurrentData);
                // calculate the cycletime for head1
            }

            CurrentData.ActualCycletime = CurrentData.Cycletime;
            
            // cycletime percent difference
            if (CurrentData.idealCycletime != TimeSpan.Zero && CurrentData.ActualCycletime != TimeSpan.Zero && CurrentData.idealCycletime != TimeSpan.Zero)
            {
                double idealtime = CurrentData.idealCycletime.TotalSeconds + CurrentData.idealLoadTime.TotalSeconds; // info 1 plus info 2

                double average = (CurrentData.ActualCycletime.TotalSeconds + idealtime)/2; //+ CurrentData.idealCycletime.TotalSeconds)/2;
                CurrentData.Percent_SpeedDifference = Math.Round(100 * (idealtime - CurrentData.ActualCycletime.TotalSeconds)/average, 2);//(CurrentData.idealCycletime.TotalSeconds - CurrentData.Cycletime.TotalSeconds)/average, 2);
            }
            else if (CurrentData.ActualCycletime== TimeSpan.Zero || CurrentData.idealCycletime == TimeSpan.Zero)
            {
                CurrentData.Percent_SpeedDifference = 0;
            }

            if (CurrentData.PrevStatetime!=DateTime.MinValue) // ignores the default time value
            {
                CurrentData.TimeInState = StateTimeCalc(CurrentData.PrevStatetime, CurrentData.MachineTimeOffset); // find how long the machine was in a state
            }
            if (CurrentData.PrevStatetime2!=DateTime.MinValue) // ignores the default time value
            {
                CurrentData.TimeInState2 = StateTimeCalc(CurrentData.PrevStatetime2, CurrentData.MachineTimeOffset2); // find how long the machine was in a state
            }

            return CurrentData;
        }

        private static Watchlist AmadaRead(Watchlist CurrentData, string URLaddress)
        {
            XmlTextReader reader = new(URLaddress);

            string ItemId = "";

            string machinestatus = "";
            string mode = "";
            string execution = "";
            string partcount = "";

            //! find the data from mtconnect agent
            while (reader.Read())
            {
                switch (reader.NodeType)
                {
                    case XmlNodeType.Element:
                        //Console.WriteLine(reader.Name + " " + reader.Value);
                        switch (reader.Name)//notation fixxing
                        {
                            case "MachineStatus":
                                ItemId = "machinestatus";
                                break;
                            case "ControllerMode":
                                ItemId = "mode";
                                break;
                            case "Execution":
                                ItemId = "execution";
                                break;
                            case "PartCount":
                                ItemId = "PartCountAct";
                                break;
                            default:
                                ItemId = "";
                                break;
                        }
                        break;
                    case XmlNodeType.Text:
                        switch (ItemId)
                        {
                            case "machinestatus":
                                machinestatus = reader.Value;
                                break;
                            case "mode":
                                mode = reader.Value;
                                break;
                            case "execution":
                                execution = reader.Value;
                                break;
                            case "PartCountAct":
                                partcount = reader.Value;
                                break;

                        }
                        break;
                    case XmlNodeType.EndElement: //Display the end of the element.
                        break;

                    default:

                        break;

                } // switch
            }// while
            reader.Close();
            //! find the data from mtconnect agent

            //Console.WriteLine(CurrentData.PartCount +" " + CurrentData.direct_timez.PartTime +": " + CurrentData.prevtimez + ": " + CurrentData.Cycletime);

            //! reconstruct the data
            CurrentData.var_amada.machinestatus = machinestatus;
            CurrentData.controllerMode = mode;
            CurrentData.Execution = execution;


            DateTime timeZ = DateTime.Now;

            if (partcount != CurrentData.PartCount && partcount!= "" && partcount != " " && partcount != null)
            {
                CurrentData.PartCount = partcount;
                CurrentData.direct_timez.PartTime = timeZ;
            }

            CurrentData = CycletimeCalculator(CurrentData);
            CurrentData.ActualCycletime = CurrentData.Cycletime;

            if (CurrentData.var_amada.machinestatus == "OFF")
            {
                CurrentData.MachineState = "OFFLINE"; // set to down
                //CurrentData.Cycletime=TimeSpan.Zero;
                if (CurrentData.MachineState!=CurrentData.StateSet)
                {
                    CurrentData.PrevStatetime = timeZ; //Timegrab(timeZ, CurrentData.PrevStatetime);
                    CurrentData.MachineTimeOffset = DateTime.Now - CurrentData.PrevStatetime;
                    CurrentData.StateSet = CurrentData.MachineState;
                }
            }
            else
            {
                CurrentData = MtConnect_StateFinder(CurrentData, timeZ,timeZ, 1, true); // (CurrentData?, time?, heads?, idletimer?)

            }

            if (double.TryParse(CurrentData.PartCount, out double PartCount) && double.TryParse(CurrentData.PartCountTotal, out double SAP_Quantity) && SAP_Quantity!=0)
            {
                int partsperCycle;
                if (CurrentData.baseQuantity<1)
                {
                    partsperCycle = 1;
                }
                else
                {
                    partsperCycle = CurrentData.baseQuantity;
                }
                // find the percent completion
                CurrentData.Percent_Completion = Math.Round(PartCount/SAP_Quantity * 100, 2);
                // Estimated Job Time
                CurrentData.Estimated_Job_Time = CurrentData.MachineCycleTime * (SAP_Quantity/partsperCycle);
                // time till completion
                CurrentData.Time_Left_till_Completion = CurrentData.Estimated_Job_Time * 0.01 * (100 - CurrentData.Percent_Completion);
            }

            // cycletime percent difference
            if (CurrentData.idealCycletime != TimeSpan.Zero && CurrentData.Cycletime != TimeSpan.Zero && CurrentData.idealCycletime != TimeSpan.Zero)
            {
                double idealtime = CurrentData.idealCycletime.TotalSeconds + CurrentData.idealLoadTime.TotalSeconds; // info 1 plus info 2

                double average = (CurrentData.Cycletime.TotalSeconds + idealtime)/2; //+ CurrentData.idealCycletime.TotalSeconds)/2;
                CurrentData.Percent_SpeedDifference = Math.Round(100 * (idealtime - CurrentData.Cycletime.TotalSeconds)/average, 2);//(CurrentData.idealCycletime.TotalSeconds - CurrentData.Cycletime.TotalSeconds)/average, 2);
            }
            else if (CurrentData.Cycletime == TimeSpan.Zero || CurrentData.idealCycletime == TimeSpan.Zero)
            {
                CurrentData.Percent_SpeedDifference = 0;
            }

            if (CurrentData.PrevStatetime!=DateTime.MinValue) // ignores the default time value
            {
                CurrentData.TimeInState = StateTimeCalc(CurrentData.PrevStatetime, CurrentData.MachineTimeOffset); // find how long the machine was in a state
            }
            // time in state 2 is used as the prev time state value used for telling if the adaptor is down
            CurrentData.TimeInState2 = CurrentData.TimeInState; // no head 2

            /*
            if (partcount2!="" && partcount2!=" " && partcount2!=null)
            {
                CurrentData.PartCount2 = partcount2;
                // calculate the cycletime for head2
                CurrentData = CycletimeCalculator2(CurrentData);
            } //! to prevent misreads from affecting the dashboard
            */

            return CurrentData;
        }

        public struct BDtronicdata
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

            // variables for figuring out an internal partcount
            // public string prevConfNumber; // this is for getting an accurate part count
            //public int partcountInt; // for calculating the partcount
            public bool partstart;
        }

        private static Watchlist BdtronicRead(Watchlist CurrentData, string URLaddress)
        {
            XmlTextReader reader;
            string ItemId = "";
            bool offline = false;

            //! Find the data
            try
            {
                reader = new(URLaddress);
                while (reader.Read())
                {
                    switch (reader.NodeType)
                    {
                        case XmlNodeType.Element:
                            while (reader.MoveToNextAttribute())
                            {
                                if (reader.Name=="dataItemId")
                                {
                                    ItemId = reader.Value;
                                }
                            }
                            break;
                        case XmlNodeType.Text:
                            switch (ItemId)
                            {
                                case "ifHW_eStopOk":
                                    try
                                    {
                                        CurrentData.BdtronicWatchlist.estop = short.Parse(reader.Value);
                                    }
                                    catch
                                    {
                                        offline = true;
                                    }
                                    break;
                                case "Automatic":
                                    try
                                    {
                                        CurrentData.BdtronicWatchlist.Automatic = short.Parse(reader.Value);
                                    }
                                    catch
                                    {
                                        offline = true;
                                    }
                                    break;
                                case "Manual":
                                    try
                                    {
                                        CurrentData.BdtronicWatchlist.Manual = short.Parse(reader.Value);
                                    }
                                    catch
                                    {
                                        offline = true;
                                    }
                                    break;
                                case "Green":
                                    try
                                    {
                                        CurrentData.BdtronicWatchlist.Green = short.Parse(reader.Value);
                                    }
                                    catch
                                    {
                                        offline = true;
                                    }
                                    break;
                                case "Yellow":
                                    try
                                    {
                                        CurrentData.BdtronicWatchlist.Yellow = short.Parse(reader.Value);
                                    }
                                    catch
                                    {
                                        offline = true;
                                    }
                                    break;
                                case "Red":
                                    try
                                    {
                                        CurrentData.BdtronicWatchlist.Red = short.Parse(reader.Value);
                                    }
                                    catch
                                    {
                                        offline = true;
                                    }
                                    break;
                                case "twoHandPressed":
                                    try
                                    {
                                        CurrentData.BdtronicWatchlist.HandPress = short.Parse(reader.Value);
                                    }
                                    catch { }
                                    break;
                                case "actspeed":
                                    try
                                    {
                                        CurrentData.BdtronicWatchlist.ActSpeed = int.Parse(reader.Value);
                                    }
                                    catch { }
                                    break;
                                case "FinishPartIfMatNOK_H1":
                                    try
                                    {
                                        CurrentData.BdtronicWatchlist.H1PartComplete = short.Parse(reader.Value);
                                    }
                                    catch { }
                                    break;
                                case "FinishPartIfMatNOK_H2":
                                    try
                                    {
                                        CurrentData.BdtronicWatchlist.H2PartComplete = short.Parse(reader.Value);
                                    }
                                    catch { }
                                    break;
                                case "ActualPosition_H1":
                                    try
                                    {
                                        CurrentData.BdtronicWatchlist.H1Position = short.Parse(reader.Value);
                                    }
                                    catch { }
                                    break;
                                case "ActualPosition_H2":
                                    try
                                    {
                                        CurrentData.BdtronicWatchlist.H2Position = short.Parse(reader.Value);
                                    }
                                    catch { }
                                    break;
                                case "Head1_ChamberState":
                                    try
                                    {
                                        CurrentData.BdtronicWatchlist.H1ChamberState = short.Parse(reader.Value);
                                    }
                                    catch { }
                                    break;
                                case "Head2_ChamberState":
                                    try
                                    {
                                        CurrentData.BdtronicWatchlist.H2ChamberState = short.Parse(reader.Value);
                                    }
                                    catch { }
                                    break;
                            }
                            break;
                        case XmlNodeType.EndElement: //Display the end of the element.
                            break;
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message.ToString());
            }
            //! Find the data
            // Now interpret the data
            string timeZ = DateTime.Now.ToString();
            //! Machine state
            if (CurrentData.AdapterOnline == false)  // make sure the adapter is online
            {
                CurrentData.MachineState = "OFFLINE";
                //  CurrentData.PartCount = "Agent Offline";
                CurrentData.material = "";
                if (CurrentData.MachineState != CurrentData.StateSet)
                {
                    CurrentData.PrevStatetime = Timegrab(timeZ, CurrentData.PrevStatetime);
                    CurrentData.MachineTimeOffset = DateTime.Now - CurrentData.PrevStatetime;
                    CurrentData.StateSet = CurrentData.MachineState;
                }
            }
            else
            {
                // machine state
                if (CurrentData.BdtronicWatchlist.Manual == 1)
                {
                    CurrentData.MachineState = "MANUAL";
                    CurrentData.Cycletime=TimeSpan.Zero;
                    if (CurrentData.MachineState!=CurrentData.StateSet)
                    {
                        CurrentData.PrevStatetime = Timegrab(timeZ, CurrentData.PrevStatetime);
                        CurrentData.MachineTimeOffset = DateTime.Now - CurrentData.PrevStatetime;
                        CurrentData.StateSet = CurrentData.MachineState;
                    }
                }
                else if (CurrentData.BdtronicWatchlist.Red == 1 || CurrentData.BdtronicWatchlist.estop == 0)
                {
                    CurrentData.MachineState = "OFFLINE";
                    if (CurrentData.MachineState!=CurrentData.StateSet)
                    {
                        CurrentData.PrevStatetime = Timegrab(timeZ, CurrentData.PrevStatetime);
                        CurrentData.MachineTimeOffset = DateTime.Now - CurrentData.PrevStatetime;
                        CurrentData.StateSet = CurrentData.MachineState;
                    }
                }
                else if (CurrentData.BdtronicWatchlist.H1Position > 2 && CurrentData.BdtronicWatchlist.H2Position > 2) // The machine is Glueing
                {
                    CurrentData.MachineState = "RUNNING";
                    if (CurrentData.MachineState != CurrentData.StateSet)
                    {
                        CurrentData.PrevStatetime = Timegrab(timeZ, CurrentData.PrevStatetime);
                        CurrentData.MachineTimeOffset = DateTime.Now - CurrentData.PrevStatetime;
                        CurrentData.StateSet = CurrentData.MachineState;
                    }
                }
                else // Machine is either idle or being loaded
                {
                    CurrentData = MSR1.IdleTimerDisplay(CurrentData);
                    //CurrentData.MachineState = "IDLE"; // should be replaced with the idle timer function
                    
                    if (CurrentData.MachineState!= CurrentData.StateSet)
                    {
                        CurrentData.PrevStatetime = Timegrab(timeZ, CurrentData.PrevStatetime);
                        CurrentData.MachineTimeOffset = DateTime.Now - CurrentData.PrevStatetime;
                        CurrentData.StateSet = CurrentData.MachineState;
                    }
                }

                if (CurrentData.PrevStatetime!=DateTime.MinValue) // ignores the default time value
                {
                    CurrentData.TimeInState = StateTimeCalc(CurrentData.PrevStatetime, CurrentData.MachineTimeOffset); // find how long the machine was in a state
                }
                // time in state 2 is used as the prev time state value used for telling if the adaptor is down
                CurrentData.TimeInState2 = CurrentData.TimeInState;
            }
            CurrentData.MachineState2 = "N/A"; // there is no 2nd head.

            return CurrentData;
        }

        private static (DateTime, bool) StringtoDateConvert(string Date)
        {
            if (DateTime.TryParse(Date, out DateTime outputtime)) // convert the string into a usable datetime
            {
                return (outputtime, true);
            }
            else
            {
                return (DateTime.Now, false);
            }
        }

        //(mazak, doosan, haas)
        private static Watchlist StandardMTConnectRead(Watchlist CurrentData, string URLaddress, int Heads, bool loader)
        {
            XmlTextReader reader = new(URLaddress);

            string ItemId = "";

            string mode = "";
            string mode2 = "";
            string execution = "";
            string execution2 = "";
            string partcount = "";
            string partcount2 = "";
            string PartCountTarget = ""; // rarely going to be used since we get this data from SAP now as order quantity

            string? rapid = null;
            string? rapid2 = null;
            string? feed = null;
            string? feed2 = null;
            string? spindle = null;
            string? spindle2 = null;
            string? gantry = null;

            string timegrab = DateTime.MinValue.ToString();

            //! find the data from mtconnect agent
            while (reader.Read())
            {
                switch (reader.NodeType)
                {
                    case XmlNodeType.Element:
                        while (reader.MoveToNextAttribute())
                        {
                            if (reader.Name=="dataItemId") // grab the header
                            {
                                ItemId = reader.Value;
                            }
                            else if (reader.Name == "timestamp") // time grabbing off the data
                            {
                                timegrab = reader.Value;
                            }
                        }
                        break;
                    case XmlNodeType.Text:
                        //Console.WriteLine(ItemId + " " + reader.Value); // debug
                        bool a; // just a stand in for when the value doesnt matter
                               
                        switch (ItemId)
                        {
                            // machine state related
                            case "mode":
                                mode = reader.Value;
                                (CurrentData.direct_timez.Modetime, a) = StringtoDateConvert(timegrab);
                                break;
                            case "execution":// execution state updated
                                execution = reader.Value;
                                (CurrentData.direct_timez.Executiontime, a) = StringtoDateConvert(timegrab);
                                break;
                            case "exec": // execution state updated
                                execution = reader.Value;
                                (CurrentData.direct_timez.Executiontime, a) = StringtoDateConvert(timegrab);
                                break;
                            case "path1_execution": // execution state updated
                                execution = reader.Value;
                                (CurrentData.direct_timez.Executiontime, a) = StringtoDateConvert(timegrab);
                                break;

                            case "mode2":
                                mode2 = reader.Value;
                                (CurrentData.direct_timez2.Modetime, a) = StringtoDateConvert(timegrab);
                                break;
                            case "execution2": // execution state updated
                                execution2 = reader.Value;
                                (CurrentData.direct_timez2.Executiontime, a)  = StringtoDateConvert(timegrab);
                                break;
                            case "exec2": // execution state updated
                                execution2 = reader.Value;
                                (CurrentData.direct_timez2.Executiontime, a) = StringtoDateConvert(timegrab);
                                break;
                            case "path2_execution": // execution state updated
                                execution2 = reader.Value;
                                (CurrentData.direct_timez2.Executiontime, a) = StringtoDateConvert(timegrab);
                                break;

                            // part count related
                            case "PartCountAct":
                                partcount = reader.Value;
                                (CurrentData.direct_timez.PartTime, CurrentData.direct_timez.readsuccessful) = StringtoDateConvert(timegrab);
                                break;
                            case "pc":
                                partcount = reader.Value;
                                (CurrentData.direct_timez.PartTime, CurrentData.direct_timez.readsuccessful) = StringtoDateConvert(timegrab);
                                break;
                            case "path1_part_count": // doosan version
                                partcount = reader.Value;
                                (CurrentData.direct_timez.PartTime, CurrentData.direct_timez.readsuccessful) = StringtoDateConvert(timegrab);
                                //Console.WriteLine("pc1" +": " +partcount + ": " + timegrab + ": " + CurrentData.direct_timez.PartTime); // debugging timegrab for 2271
                                break;
                            case "Count1": // haas version
                                partcount = reader.Value;
                                (CurrentData.direct_timez.PartTime, CurrentData.direct_timez.readsuccessful) = StringtoDateConvert(timegrab);
                                break;

                            case "PartCountAct2":
                                partcount2 = reader.Value;
                                (CurrentData.direct_timez2.PartTime, CurrentData.direct_timez2.readsuccessful) = StringtoDateConvert(timegrab);
                                break;
                            case "pc2":
                                partcount2 = reader.Value;
                                (CurrentData.direct_timez2.PartTime, CurrentData.direct_timez2.readsuccessful)= StringtoDateConvert(timegrab);
                                break;
                            case "path2_part_count":  // doosan version
                                partcount2 = reader.Value;
                                (CurrentData.direct_timez2.PartTime, CurrentData.direct_timez2.readsuccessful) = StringtoDateConvert(timegrab);
                                //Console.WriteLine("pc2" +": " +partcount + ": " + timegrab + ": " + CurrentData.direct_timez.PartTime);
                                break;
                            case "Count2":  // haas version
                                partcount2 = reader.Value;
                                (CurrentData.direct_timez2.PartTime, CurrentData.direct_timez2.readsuccessful) = StringtoDateConvert(timegrab);
                                break;

                            case "PartCountTarget":
                                PartCountTarget = reader.Value;
                                break;

                            // overrides
                            case "Frapidovr": // update rapid override
                                rapid = reader.Value;
                                break;
                            case "pfr": // update rapid override
                                rapid = reader.Value;
                                break;
                            case "path1_rapid_override": // update rapid 1 override (doosan)
                                rapid = reader.Value;
                                break;
                            case "Rapid_Override": // update rapid 1 override (haas)
                                rapid = reader.Value;
                                break;

                            case "Frapidovr2":// update rapid override 
                                rapid2 = reader.Value;
                                break;
                            case "pfr2": // update rapid override
                                rapid2 = reader.Value;
                                break;
                            case "path2_rapid_override": // update rapid 1 override
                                rapid2 = reader.Value;
                                break;

                            case "Fovr": // update feed override
                                feed = reader.Value;
                                break;
                            case "pfo": // update feed override
                                feed = reader.Value;
                                break;
                            case "path1_feedrate_override": // update feed 1 override
                                feed = reader.Value;
                                break;
                            case "Feedrate_Override": // update feed 1 override (haas)
                                feed = reader.Value;
                                break;

                            case "Fovr2": // update feed override
                                feed2 = reader.Value;
                                break;
                            case "pfo2": // update feed override
                                feed2 = reader.Value;
                                break;
                            case "path2_feedrate_override": // update feed 2 override
                                feed2 = reader.Value;
                                break;

                            case "Sovr": // update spindle override
                                spindle = reader.Value;
                                break;
                            case "path1_spindle_override": // update spindle 1 override
                                spindle = reader.Value;
                                break;
                            case "Spindle_Override": // update spindle 1 override (haas)
                                spindle = reader.Value;
                                break;

                            case "Sovr2": // update spindle override 2
                                spindle2 = reader.Value;
                                break;
                            case "S2ovr": // update spindle override 2
                                spindle2 = reader.Value;
                                break;
                            case "path2_spindle_override": // update spindle 2 override
                                spindle2 = reader.Value;
                                break;

                            case "glovrd": // gantry
                                gantry = reader.Value;
                                break;
                        }
                        break;
                    case XmlNodeType.EndElement: //Display the end of the element.
                        break;
                    default:
                        break;


                } // switch
            }// while
            reader.Close();
                //! find the data from mtconnect agent

                 //Console.WriteLine(i + ": " + CurrentData.direct_timez.PartTime + ": " + CurrentData.prevtimez + ": " + CurrentData.Cycletime + 
                 //    ": " + CurrentData.direct_timez.readsuccessful); // debugging time capture
                // i++;

                //! reconstruct the data
                if (mode!="" && CurrentData.controllerMode!=mode)
                {
                    CurrentData.controllerMode = mode;
                    CurrentData.direct_timez.MStateTime = CurrentData.direct_timez.Modetime; // update the time when the mode changes as the state change
                }
                if (mode2!="" && CurrentData.controllerMode2!=mode2)
                {
                    CurrentData.controllerMode2 = mode2;
                    CurrentData.direct_timez2.MStateTime = CurrentData.direct_timez2.Modetime;  // update the time when the mode changes as the state change
                }
                if (execution!="" && CurrentData.Execution!=execution)
                {
                    CurrentData.Execution = execution;
                    CurrentData.direct_timez.MStateTime = CurrentData.direct_timez.Executiontime;  // update the time when the execution changes as the state change
                }
                if (execution2!="" && CurrentData.Execution2!=execution2)
                {
                    CurrentData.Execution2 = execution2;
                    CurrentData.direct_timez2.MStateTime = CurrentData.direct_timez2.Executiontime;  // update the time when the execution changes as the state change
                }
                // head 1 cycling
                if (partcount!= "" && partcount != " " && partcount != null && partcount2 != "UNAVAILABLE")  //! to prevent misreads from affecting the dashboard
                {
                    CurrentData.PartCount = partcount;
                    CurrentData = CycletimeCalculator(CurrentData); // calculate the cycletime for head1
                }
                // head 2 cycling
                if (Heads>1)
                {
                    if (partcount2!="" && partcount2!=" " && partcount2!=null && partcount2 != "UNAVAILABLE")
                    {
                        CurrentData.PartCount2 = partcount2;
                        CurrentData = CycletimeCalculator2(CurrentData); // calculate the cycletime for head2
                    } //! to prevent misreads from affecting the dashboard

                    // debugging cycletime results
                    //  Console.WriteLine(i + ": " + CurrentData.PartCount +" " + CurrentData.direct_timez.PartTime +": " + CurrentData.prevtimez + ": " + CurrentData.Cycletime + " | "+ CurrentData.PartCount2 +" " + CurrentData.direct_timez2.PartTime +": " + CurrentData.prevtimez2 + ": " + CurrentData.Cycletime2);
                    //  i++;

                }
                else
                {
                    CurrentData.Cycletime2 = TimeSpan.Zero;
                }

                //! general correctional data for consistancy
                if (CurrentData.PartCount=="" || CurrentData.PartCount==" " || CurrentData.PartCount == null)
                {
                    CurrentData.PartCount = "0";
                }
                if (Heads>1)
                {
                    if (CurrentData.PartCount2=="" || CurrentData.PartCount==" " || CurrentData.PartCount2 == null)
                    {
                        CurrentData.PartCount2 = "0";
                    }
                }
                else
                {
                    CurrentData.PartCount2 = "N/A";
                }

            
                if (Heads!=1 && CurrentData.Cycletime2 != TimeSpan.Zero && CurrentData.Cycletime2 > CurrentData.Cycletime && (CurrentData.KioskState == "ready" || CurrentData.KioskState == "setup" || CurrentData.KioskState == "inspect")) // if head2 exists and is running look at cycletime 2 
                {
                    CurrentData.ActualCycletime = CurrentData.Cycletime2;
                    CurrentData.MachinCycle = CurrentData.MachCycle2;
                    CurrentData.Loadtime = CurrentData.LoadCycle2;
                }
                else if ( (CurrentData.KioskState == "ready" || CurrentData.KioskState == "setup" || CurrentData.KioskState == "inspect")) // any other case we want cycletime 1
                {
                    CurrentData.ActualCycletime = CurrentData.Cycletime;
                    CurrentData.MachinCycle = CurrentData.MachCycle1;
                    CurrentData.Loadtime = CurrentData.LoadCycle1;
                }
                else // both head 1 and 2 have 0 in there cycletime
                {
                    CurrentData.ActualCycletime = TimeSpan.Zero;
                    CurrentData.MachinCycle = TimeSpan.Zero;
                    CurrentData.Loadtime = TimeSpan.Zero;
                }
            
                //! general correctional data for consistancy
                CurrentData.Rapid = OVERSequence(rapid);
                CurrentData.Rapid2 = OVERSequence(rapid2);
                CurrentData.Feed = OVERSequence(feed);
                CurrentData.Feed2 = OVERSequence(feed2);
                CurrentData.Spindle = OVERSequence(spindle);
                CurrentData.Spindle2 = OVERSequence(spindle2);
                CurrentData.Gantry = OVERSequence(gantry);

                if (CurrentData.AdapterOnline == true)
                {
                    CurrentData = MtConnect_StateFinder(CurrentData, CurrentData.direct_timez.MStateTime, CurrentData.direct_timez2.MStateTime, Heads, loader);
                }
                else
                {
                    CurrentData.MachineState = "OFFLINE";
                    CurrentData.MachineState2 = "OFFLINE";
                    CurrentData.Cycletime = TimeSpan.Zero;
                    CurrentData.Cycletime2 = TimeSpan.Zero;
                    // CurrentData.PartCount = "Agent Offline";
                    if (CurrentData.MachineState != CurrentData.StateSet)
                    {
                        CurrentData.PrevStatetime = CurrentData.direct_timez.MStateTime; //Timegrab(CurrentData.direct_timez.MStateTime, CurrentData.PrevStatetime);
                        CurrentData.MachineTimeOffset = DateTime.Now - CurrentData.PrevStatetime;
                        CurrentData.StateSet = CurrentData.MachineState;
                    }

                    if (CurrentData.MachineState2 != CurrentData.StateSet2)
                    {
                        CurrentData.PrevStatetime2 = CurrentData.direct_timez2.MStateTime; //Timegrab(CurrentData.direct_timez2.MStateTime, CurrentData.PrevStatetime2);
                        CurrentData.MachineTimeOffset2 = DateTime.Now - CurrentData.PrevStatetime2;
                        CurrentData.StateSet2 = CurrentData.MachineState2;
                    }
                }

                if (double.TryParse(CurrentData.PartCount2, out double PartCount2) && double.TryParse(CurrentData.PartCountTotal, out double SAP_Quantity) && SAP_Quantity!=0)
                {
                    int partsperCycle;
                    if (CurrentData.baseQuantity<1)
                    {
                        partsperCycle = 1;
                    }
                    else
                    {
                        partsperCycle = CurrentData.baseQuantity;
                    }

                    // find the percent completion
                    CurrentData.Percent_Completion = Math.Round(PartCount2/SAP_Quantity * 100, 2);

                    // Estimated Job Time
                    CurrentData.Estimated_Job_Time = CurrentData.MachineCycleTime * (SAP_Quantity/partsperCycle);
                    // time till completion
                    CurrentData.Time_Left_till_Completion = CurrentData.Estimated_Job_Time * 0.01 * (100 - CurrentData.Percent_Completion);

                    //Console.Write(CurrentData.PartCount2 + " Percent Completion: " + CurrentData.Percent_Completion + ", "); // debug
                    //Console.Write(" Estimated_Job_Time: " + CurrentData.Estimated_Job_Time + ", "); // debug
                    //Console.WriteLine(" Time_Left_till_Completion: " + CurrentData.Time_Left_till_Completion); // debug
                }

                // cycletime percent difference
                if (CurrentData.idealCycletime != TimeSpan.Zero && CurrentData.ActualCycletime != TimeSpan.Zero && CurrentData.idealCycletime != TimeSpan.Zero)
                {
                    double idealtime = CurrentData.idealCycletime.TotalSeconds + CurrentData.idealLoadTime.TotalSeconds; // info 1 plus info 2

                    double average = (CurrentData.ActualCycletime.TotalSeconds + idealtime)/2; //+ CurrentData.idealCycletime.TotalSeconds)/2;
                    CurrentData.Percent_SpeedDifference = Math.Round(100 * (idealtime - CurrentData.ActualCycletime.TotalSeconds)/average, 2);//(CurrentData.idealCycletime.TotalSeconds - CurrentData.Cycletime.TotalSeconds)/average, 2);
                }
                else if (CurrentData.ActualCycletime  == TimeSpan.Zero || CurrentData.idealCycletime == TimeSpan.Zero)
                {
                    CurrentData.Percent_SpeedDifference = 0;
                }

                // head 1
                if (CurrentData.PrevStatetime!=DateTime.MinValue) // ignores the default time value
                {
                    CurrentData.TimeInState = StateTimeCalc(CurrentData.PrevStatetime, CurrentData.MachineTimeOffset); // find how long the machine was in a state
                }
                // head 2
                if (CurrentData.PrevStatetime2!=DateTime.MinValue) // ignores the default time value
                {
                    CurrentData.TimeInState2 = StateTimeCalc(CurrentData.PrevStatetime2, CurrentData.MachineTimeOffset2); // find how long the machine was in a state
                }

                return CurrentData;
            
        }

        private static string OVERSequence(string? OVride)
        {
            if (OVride !=null)
            {
                if (OVride=="UNAVAILABLE")
                {
                    OVride   = "100";
                }
            }
            else
            {
                OVride = null; 
            }
            #pragma warning disable CS8603
            return OVride;
            #pragma warning restore CS8603
        }

        private static Watchlist CycletimeCalculator(Watchlist watchlist)
        {
            if (watchlist.KioskState == "ready")
            {
                // Machine Cycle time 
                if (watchlist.Execution.Contains("ACTIVE") && watchlist.loadingindicator1 && watchlist.prevtimez != DateTime.MinValue) // machine loaded
                {
                    watchlist.BeginCycle1 = watchlist.direct_timez.Executiontime;
                    watchlist.loadingindicator1 = false;
                }
                else if (!watchlist.Execution.Contains("ACTIVE")) // tells that the machine is done running
                {
                    watchlist.loadingindicator1 = true;
                }

                // part to part cycletime
                if (watchlist.prevtimez == DateTime.MinValue) // this is only for the startup initialization
                {
                    watchlist.prevtimez = watchlist.direct_timez.PartTime - watchlist.Cycletime; // work backwords to find the previous time for machines running while the dashboard restarts
                    watchlist.loadingindicator1 = true;
                    watchlist.BeginCycle1 = DateTime.Now;
                }
                else if (watchlist.timeoutindicator1 && watchlist.PartCount != watchlist.partCountRepeat) // resetup after a timeout
                {
                    watchlist.prevtimez = watchlist.direct_timez.PartTime;
                    watchlist.partCountRepeat = watchlist.PartCount; // reset the signal
                    watchlist.timeoutindicator1 = false;

                    // Machine Cycle time 
                    watchlist.MachCycle1 = CycleTimeCalc(watchlist.BeginCycle1, watchlist.direct_timez.PartTime); // capture the machine cycletime
                }
                else if (watchlist.PartCount != watchlist.partCountRepeat) // get the cycletime after getting a part
                {
                    watchlist.Cycletime = CycleTimeCalc(watchlist.prevtimez, watchlist.direct_timez.PartTime); // part to part cycletime
                    watchlist.prevtimez = watchlist.direct_timez.PartTime;
                    watchlist.partCountRepeat = watchlist.PartCount; // reset the signal

                    // Machine Cycle time 
                    watchlist.MachCycle1 = CycleTimeCalc(watchlist.BeginCycle1, watchlist.direct_timez.PartTime); // capture the machine cycletime
                }

                if(watchlist.MachCycle1<TimeSpan.Zero)
                {
                    watchlist.MachCycle1 = TimeSpan.Zero;
                }

                // loadtime
                if (watchlist.MachCycle1.TotalSeconds>0 && watchlist.Cycletime >= watchlist.MachCycle1)
                {
                    watchlist.LoadCycle1 = watchlist.Cycletime - watchlist.MachCycle1;
                }

                //correction for issue of random zeroing
                /*
                if(watchlist.Cycletime == TimeSpan.Zero && watchlist.MachCycle1 != TimeSpan.Zero)
                {
                    watchlist.Cycletime = watchlist.MachCycle1 + watchlist.LoadCycle1;
                }
                */

            }
            else //cycletimes are only recordered when the kiosk is in ready
            {
                watchlist.timeoutindicator1 = true; // a arbitrary value that tells the cycle system that the kiosk timed out
            }

            if (watchlist.Cycletime2> new TimeSpan(3, 0, 0)) // 3 hour + cycletimes are most definetly wrong
            {
                watchlist.Cycletime = TimeSpan.Zero;
                watchlist.MachCycle1 = TimeSpan.Zero;
                watchlist.LoadCycle1 = TimeSpan.Zero;
            }
            else if (watchlist.MachCycle1>new TimeSpan(3, 0, 0))
            {
                watchlist.MachCycle1 = TimeSpan.Zero;
            }

            return watchlist;
        }

        private static Watchlist CycletimeCalculator2(Watchlist watchlist)
        {
            if (watchlist.KioskState == "ready")
            {
                // Machine Cycle time 
                if (watchlist.Execution2.Contains("ACTIVE") && watchlist.loadingindicator2 && watchlist.prevtimez2 != DateTime.MinValue) // machine loaded
                {
                    watchlist.BeginCycle2 = watchlist.direct_timez2.Executiontime;
                    watchlist.loadingindicator2 = false;
                    watchlist.loadingindicator2 = true;
                    watchlist.BeginCycle2 = DateTime.Now;
                }
                else if (!watchlist.Execution2.Contains("ACTIVE")) // tells that the machine is done running
                {
                    watchlist.loadingindicator2 = true;
                }

                // part to part cycletime
                if (watchlist.prevtimez2 == DateTime.MinValue) // this is only for the startup initialization
                {
                    watchlist.prevtimez2 = watchlist.direct_timez2.PartTime - watchlist.Cycletime2; // work backwords to find the previous time for machines running while the dashboard restarts
                }
                else if (watchlist.timeoutindicator2 && watchlist.PartCount2 != watchlist.partCountRepeat2) // resetup after a timeout
                {
                    watchlist.prevtimez2 = watchlist.direct_timez2.PartTime;
                    watchlist.partCountRepeat2 = watchlist.PartCount2; // reset the signal
                    watchlist.timeoutindicator2 = false;

                    // Machine Cycle time 
                    watchlist.MachCycle2 = CycleTimeCalc(watchlist.BeginCycle2, watchlist.direct_timez2.PartTime); // capture the machine cycletime
                }
                else if(watchlist.PartCount2 != watchlist.partCountRepeat2) // get the cycletime after getting a part
                {
                    watchlist.Cycletime2 = CycleTimeCalc(watchlist.prevtimez2, watchlist.direct_timez2.PartTime); // part to part cycletime
                    watchlist.prevtimez2 = watchlist.direct_timez2.PartTime;
                    watchlist.partCountRepeat2 = watchlist.PartCount2; // reset the signal

                    // Machine Cycle time 
                    watchlist.MachCycle2 = CycleTimeCalc(watchlist.BeginCycle2, watchlist.direct_timez2.PartTime); // capture the machine cycletime
                }

                if (watchlist.MachCycle2<TimeSpan.Zero)
                {
                    watchlist.MachCycle2 = TimeSpan.Zero;
                }

                // loadtime
                if (watchlist.MachCycle2.TotalSeconds>0 && watchlist.Cycletime2 >= watchlist.MachCycle2)
                {
                    watchlist.LoadCycle2 = watchlist.Cycletime2 - watchlist.MachCycle2;
                }

                //correction for issue of random zeroing
                /*
                if (watchlist.Cycletime2 == TimeSpan.Zero && watchlist.MachCycle2 != TimeSpan.Zero)
                {
                    watchlist.Cycletime2 = watchlist.MachCycle2 + watchlist.LoadCycle2;
                }
                */
            }
            else //cycletimes are only recordered when the kiosk is in ready
            {
                watchlist.timeoutindicator2 = true; // a arbitrary value that tells the cycle system that the kiosk timed out
            }

            if (watchlist.Cycletime2> new TimeSpan(3, 0, 0)) // 3 hour + cycletimes are most definetly wrong
            {
                watchlist.Cycletime2 = TimeSpan.Zero;
                watchlist.MachCycle2 = TimeSpan.Zero;
                watchlist.LoadCycle2 = TimeSpan.Zero;
            }
            else if(watchlist.MachCycle2>new TimeSpan(3, 0, 0));
            {
                watchlist.MachCycle2 = TimeSpan.Zero;
            }
            return watchlist;
        }

    }

}