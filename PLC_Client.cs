using Microsoft.AspNetCore.Mvc.ModelBinding.Binders;
using Org.BouncyCastle.Asn1.Cms;
using System.Diagnostics;
using System.Diagnostics.Eventing.Reader;
using System.Xml;
using static MTConnectDashboard.MSR1;


// This if for grabbing the kiosk data.

namespace MTConnectDashboard
{
    public class PLC_Client
    {
        public struct PLC_Data
        {
            public string Supervisor;
            public string Operator;
            public string ProductionOrder;
            public string Operation; // sap operation
            public string ConfirmationNumber;
            public string Material;
            public string Status;
            public string Event;
            public string Override;
            public string TotalPartCount; // SAP Quanitity
            public string idealCycleTime; // SAP INFO 1 field
            public string idealLoadTime;   // SAP INFO 2 IIOT field
            public string BaseQuantity;
            public string SetupTime;
            public string MachineCycleTime; 

            // for kiosk debugging
            public string k_MachineState;
            public string k_MachineState2;
            public string k_partcount1;
            public string k_partcount2;

            // for comment
            public string linkaddress;
            public string comment;
        }

        private const string BlackboxServerAddress = "http://192.168.200.25:";

        public static PLC_Data BlackBoxRead(PLC_Data PLC_data, string pagetag, string port, string series)
        {
            string URLaddress = BlackboxServerAddress+ port +"/" +pagetag;
            XmlTextReader reader;
            reader = new(URLaddress);
            try
            {
                while (reader.Read())
                {
                    if (reader.NodeType == XmlNodeType.Element)
                    {
                        while (reader.MoveToNextAttribute())
                        {
                           // Console.WriteLine(" " + reader.Name + "='" + reader.Value + "'");
                            switch (reader.Name)
                            {
                                case "SUP":
                                    PLC_data.Supervisor = reader.Value;
                                    break;
                                case "Oper":
                                    PLC_data.Operator = reader.Value;
                                    break;
                                //confirmation number
                                case "CN":
                                    PLC_data.ConfirmationNumber = reader.Value;
                                    break;
                                // SAP Data
                                case "Prod_Order":
                                    PLC_data.ProductionOrder = reader.Value;
                                    break;
                                case "Operation":
                                    PLC_data.Operation = reader.Value;
                                    break;
                                case "Material":
                                    PLC_data.Material = reader.Value;
                                    break;
                                case "Status":
                                    PLC_data.Status = reader.Value;
                                    break;
                                case "Event":
                                    PLC_data.Event = reader.Value;
                                    break;
                                case "Override":
                                    PLC_data.Override = reader.Value;
                                    break;
                                // SAP part quantity
                                case "TargetPartCount":
                                    PLC_data.TotalPartCount = reader.Value;
                                    break;
                                //SAP cycle time values
                                case "CT":
                                    PLC_data.idealCycleTime = reader.Value;
                                    break;
                                case "LT":
                                    PLC_data.idealLoadTime = reader.Value;
                                    break;
                                //SAP base quantity
                                case "BQ":
                                    PLC_data.BaseQuantity = reader.Value;
                                    break;
                                //SAP  setup time 
                                case "Setup":
                                    PLC_data.SetupTime = reader.Value;
                                    break;
                                // Machine Cycletime
                                case "MCT":
                                    PLC_data.MachineCycleTime = reader.Value;
                                    break;
                                // for kiosk debugging (reiteration data)
                                case "MachineState":
                                    PLC_data.k_MachineState = reader.Value;
                                    break;
                                case "MachineState2":
                                    PLC_data.k_MachineState2 = reader.Value;
                                    break;
                                case "PartCount":
                                    PLC_data.k_partcount1 = reader.Value;
                                    break;
                                case "PartCount2":
                                    PLC_data.k_partcount2 = reader.Value;
                                    break;
                                case "Comment":
                                    PLC_data.comment = reader.Value;
                                    if (PLC_data.comment != "" && PLC_data.comment !=null)//if comment exists make a link address
                                    {
                                        string tag;
                                        if (pagetag.Length>4) // get rid of the first number of the location number if necessary
                                        {
                                            tag = pagetag.Remove(0, 1);
                                        }
                                        else
                                        {
                                            tag = pagetag;
                                        }
                                        PLC_data.linkaddress = Commenting.Find_LinkAddress(series, tag); //find the link address for the comment.
                                    }
                                    break;
                            }
                        } // Read the attributes
                    }

                }
            }
            catch/*(Exception EX)*/
            {
                //Console.WriteLine(EX);
            }
           // Console.WriteLine(PLC_data.Supervisor + ": " +PLC_data.Operator + ": "+PLC_data.ProductionOrder + ": " +PLC_data.Operation +
           // ": "+PLC_data.Material + ": " + PLC_data.Status + ": " + PLC_data.Event);

            return PLC_data;
        }

        public static Watchlist PLC_Interpret(Watchlist DashboardData, PLC_Data plcdata)
        { 
            try
            {
                // override
                if (plcdata.Override == "Unpressed")
                {
                    DashboardData.KioskOveride = false;
                }
                else if(plcdata.Override == "")
                {
                    ; // dont make a change if the data is unknown (kiosk server was probably recycled)
                }
                else
                {
                    DashboardData.KioskOveride = true;
                }

                DashboardData.Conf_Numb = plcdata.ConfirmationNumber;

                if (DashboardData.Conf_Numb != "")
                {
                    DashboardData.material = plcdata.Material;

                    DashboardData.Operation_Number = plcdata.Operation;

                    DashboardData.Prod_Order = plcdata.ProductionOrder;

                    DashboardData.PartCountTotal = plcdata.TotalPartCount;

                    float cycletime = float.Parse(plcdata.idealCycleTime); 
                    DashboardData.idealCycletime = TimeSpan.FromMinutes((double)new decimal(cycletime)); //sap info 1

                    float loadtime = float.Parse(plcdata.idealLoadTime);
                    DashboardData.idealLoadTime = TimeSpan.FromMinutes((double)new decimal(loadtime)); //sap info 2

                    DashboardData.baseQuantity = int.Parse(plcdata.BaseQuantity); 

                    float setuptime = float.Parse(plcdata.SetupTime);
                    DashboardData.setuptime = TimeSpan.FromMinutes((double)new decimal(setuptime));

                    float MachineCycleTime = float.Parse(plcdata.MachineCycleTime);
                    DashboardData.MachineCycleTime = TimeSpan.FromMinutes((double)new decimal(MachineCycleTime));
                }
                else
                {
                    // clear the entries when the machine isnt doing a job
                    DashboardData.material = "";
                    DashboardData.Operation_Number = "";
                    DashboardData.Prod_Order = "";
                    DashboardData.PartCountTotal = "";
                    DashboardData.idealCycletime = TimeSpan.Zero;
                    DashboardData.idealLoadTime = TimeSpan.Zero;
                    DashboardData.baseQuantity = 0;
                    DashboardData.setuptime = TimeSpan.Zero;
                    DashboardData.MachineCycleTime = TimeSpan.Zero;
                }
               
                // operation of kiosk (Not the sap operation)
                if (plcdata.Event != "") // we are going to seperate the kiosk event from kiosk state because not all the events are states.
                {
                    DashboardData.Operation = plcdata.Event; // this is every event log the kiosk can log
                    // basically a filter so we dont log kiosk events as kiosk states
                    if (DashboardData.Operation != "change_user" && DashboardData.Operation != "override_on" && DashboardData.Operation != "override_off") 
                    {
                        DashboardData.KioskState = DashboardData.Operation; 
                    }
                }
                // operator id
                //if (plcdata.Operator != "")
                //{
                DashboardData.OP_ID = plcdata.Operator;
                //}
                // supervisor id
                //if (plcdata.Supervisor != "")
                //{
                DashboardData.SUP_ID = plcdata.Supervisor;
                //}


                if (plcdata.Event == "NoJob" || plcdata.Event == "TimeOut" || plcdata.Event == "SchedMaint" || plcdata.Event == "UnschedMaint") // removes the order number that is no longer relevant
                {
                    DashboardData.Prod_Order = plcdata.ProductionOrder;
                    DashboardData.material = plcdata.Material;
                    DashboardData.PartCountTotal = plcdata.TotalPartCount;
                }

                // monitoring the kiosk (reiteration data)
                DashboardData.Kiosk_MTConnectState = plcdata.k_MachineState;
                DashboardData.Kiosk_MTConnectState2 = plcdata.k_MachineState2;
                DashboardData.Kiosk_PartCount = plcdata.k_partcount1;
                DashboardData.Kiosk_PartCount2 = plcdata.k_partcount2;
            }
            catch
            { }

            // this will be what the dashboard displays under WC STATE
            DashboardData.ThyHOLYDashboardState = MachineStateFinder(DashboardData.DisplayMachineState, DashboardData.DisplayMachineState2, DashboardData.KioskState, DashboardData.PrevKioskState);
            
            if (DashboardData.ThyHOLYDashboardState != DashboardData.prevThyHOLYDashboardState && DashboardData.ThyHOLYDashboardState != "LOADING")
            {
               // Console.WriteLine("change in state " + DashboardData.prevThyHOLYDashboardState + " -> "+DashboardData.ThyHOLYDashboardState);
                DashboardData.PrevWorkCenterTime = DateTime.Now;
                DashboardData.TimeInWorkCenterState = TimeSpan.Zero;
                DashboardData.prevThyHOLYDashboardState = DashboardData.ThyHOLYDashboardState;

            }
            else
            {
                DashboardData.TimeInWorkCenterState = DateTime.Now - DashboardData.PrevWorkCenterTime; // this is the WC Time
            }

            if (DashboardData.KioskState == "setup" || DashboardData.KioskState == "inspect" || DashboardData.KioskState == "ready")
            {
                //Console.WriteLine(DashboardData.ThyHOLYDashboardState + " " + DashboardData.PrevKioskState);
                DashboardData.PrevKioskState = DashboardData.KioskState;

            }


            // find time in setup
            if (DashboardData.ThyHOLYDashboardState  == "setup")
            {
                DashboardData.Setuptimecounting = DashboardData.TimeInWorkCenterState; // gather the amount of time in setup
                DashboardData.exitsetup = false;
            }
            else if (DashboardData.ThyHOLYDashboardState != "setup" && DashboardData.exitsetup != true)
            {
                DashboardData.ActualSetupTime += DashboardData.Setuptimecounting; // add the setup time 
                DashboardData.exitsetup = true;
            }
            // the setup time goes to zero when the job changes
            if (DashboardData.prev_Conf_Numb != DashboardData.Conf_Numb && DashboardData.Conf_Numb!="") 
            {
                DashboardData.ActualSetupTime = TimeSpan.Zero;
                DashboardData.Setuptimecounting = TimeSpan.Zero;
                DashboardData.prev_Conf_Numb = DashboardData.Conf_Numb;
            }

            return DashboardData;
        }

        // The Machine State on the main Dashboard (Work Center State)
        public static string MachineStateFinder(string MtConnectState, string MtConnectState2, string KioskState, string prevMachineState)
        {
            string ActualMachineState;

            if(KioskState == "unschedmaint"|| KioskState == "schedmaint" || KioskState == "nojob")  // no job and maintanence 
            {
                ActualMachineState = KioskState;
            }
            else if(KioskState == "setup" || KioskState == "inspect") // inspection and setup
            {
               /* if (MtConnectState == "FEED_HOLD" || MtConnectState2 == "FEED_HOLD") // showing feedhold takes priority
                {
                    ActualMachineState = "FEED_HOLD";
                } // feedhold exception
                else*/
                if(MtConnectState == "OFFLINE" || MtConnectState2 == "OFFLINE")
                {
                    ActualMachineState = "OFFLINE";
                } // offline exception
                else
                {
                    ActualMachineState = KioskState; // will show either inspect or setup
                }
            }
            else if(KioskState == "ready") // ready mode
            {

                if (MtConnectState == "FEED_HOLD" || MtConnectState2 == "FEED_HOLD") // showing feedhold takes priority
                {
                    ActualMachineState = "FEED_HOLD";
                } // feedhold exception
                else if (MtConnectState == "RUNNING" || MtConnectState2 == "RUNNING") // running exception
                {
                    ActualMachineState = "RUNNING";
                }
                else if (MtConnectState == "LOADING" || MtConnectState2 == "LOADING") // loading exception
                {
                    ActualMachineState = "LOADING";
                }
                else if (MtConnectState == "PGM_STOP" || MtConnectState2 == "PGM_STOP") // program stop exception
                {
                    ActualMachineState = "PGM_STOP";
                }  // offline exception
                else if (MtConnectState == "OFFLINE" || MtConnectState2 == "OFFLINE")
                {
                    ActualMachineState = "OFFLINE";
                }  // offline exception
                else
                {
                    ActualMachineState = "Ready"; // show ready
                }
            }
            else if(KioskState == "timeout")
            {
                if (MtConnectState == "FEED_HOLD" || MtConnectState2 == "FEED_HOLD") // showing feedhold takes priority
                {
                    ActualMachineState = "FEED_HOLD";
                }
                else
                {
                    ActualMachineState = "timeout";
                }
            }
            else
            {
                if (MtConnectState == "FEED_HOLD" || MtConnectState2 == "FEED_HOLD") // showing feedhold takes priority
                {
                    ActualMachineState = "FEED_HOLD";
                } // feedhold exception
                else if (MtConnectState == "RUNNING" || MtConnectState2 == "RUNNING") // running exception
                {
                    ActualMachineState = "RUNNING";
                }
                else
                {
                    ActualMachineState = MtConnectState;
                }
            }

       
            /*
            switch (MtConnectState)
            {
                case "LOADING":
                    ActualMachineState = MtConnectState;
                    break;
                case "RUNNING":
                    if (KioskState == "run" || KioskState == "ready")
                    {
                        ActualMachineState = "RUNNING";
                    }
                    else if (KioskState == "setup" || KioskState == "inspect" || KioskState == "unschedmaint"|| KioskState == "schedmaint" || KioskState == "nojob")
                    {
                        ActualMachineState = KioskState;
                    }
                    else
                    {
                        ActualMachineState = MtConnectState;
                    }
                    break;
                case "IDLE":
                    if (KioskState == "run" || KioskState == "ready")
                    {
                        ActualMachineState = KioskState;
                    }
                    else if (KioskState == "setup" || KioskState == "inspect" || KioskState == "unschedmaint"|| KioskState == "schedmaint" || KioskState == "nojob" || KioskState == "timeout")
                    {
                        ActualMachineState = KioskState;
                    }
                    else
                    {
                        ActualMachineState = MtConnectState;
                    }
                    break;
                case "FEED_HOLD":
                    if (KioskState == "run" || KioskState == "ready")
                    {
                        ActualMachineState = MtConnectState;
                    }
                    else if (KioskState == "unschedmaint"|| KioskState == "schedmaint" || KioskState == "nojob")
                    {
                        ActualMachineState = KioskState;
                    }
                    else
                    {
                        ActualMachineState = MtConnectState;
                    }
                    break;
                case "INTERUPT":
                    if (KioskState == "run" || KioskState == "ready")
                    {
                        ActualMachineState = KioskState;
                    }
                    else if (KioskState == "setup" || KioskState == "inspect" || KioskState == "unschedmaint"|| KioskState == "schedmaint" || KioskState == "nojob" || KioskState == "timeout")
                    {
                        ActualMachineState = KioskState;
                    }
                    else
                    {
                        ActualMachineState = MtConnectState;
                    }
                    break;
                case "PGM_STOP":
                    if (KioskState == "run" || KioskState == "ready")
                    {
                        ActualMachineState = KioskState;
                    }
                    else if (KioskState == "setup" || KioskState == "inspect" || KioskState == "unschedmaint"|| KioskState == "schedmaint" || KioskState == "nojob" || KioskState == "timeout")
                    {
                        ActualMachineState = KioskState;
                    }
                    else
                    {
                        ActualMachineState = MtConnectState;
                    }
                    break;
                case "MANUAL":
                    if (KioskState == "run" || KioskState == "ready")
                    {
                        ActualMachineState = KioskState;
                    }
                    else if (KioskState == "setup" || KioskState == "inspect" || KioskState == "unschedmaint"|| KioskState == "schedmaint" || KioskState == "nojob" || KioskState == "timeout")
                    {
                        ActualMachineState = KioskState;
                    }
                    else
                    {
                        ActualMachineState = MtConnectState;
                    }
                    break;
                case "OFFLINE":
                    if (KioskState == "run")
                    {
                        ActualMachineState = "OFFLINE";
                    }
                    else if (KioskState == "unschedmaint"|| KioskState == "schedmaint" || KioskState == "nojob" || KioskState == "timeout")
                    {
                        ActualMachineState = KioskState;
                    }
                    else
                    {
                        ActualMachineState = MtConnectState;
                    }
                    break;
                default: // off or down
                    ActualMachineState = MtConnectState;
                    break;
            }
            */

            if(ActualMachineState == "timeout")
            {

                switch(prevMachineState)
                {
                    case "Setup":
                        ActualMachineState = "Setup Timeout";
                        break;
                    case "Inspect":
                        ActualMachineState = "Inspect Timeout";
                        break;
                    case "Ready":
                        ActualMachineState = "Ready Timeout";
                        break;
                    case "setup":
                        ActualMachineState = "Setup Timeout";
                        break;
                    case "inspect":
                        ActualMachineState = "Inspect Timeout";
                        break;
                    case "ready":
                        ActualMachineState = "Ready Timeout";
                        break;
                    default:
                        ActualMachineState = MtConnectState;
                        break;
                }
            }


            return ActualMachineState;
        }

    }
}
