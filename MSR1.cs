using Microsoft.IdentityModel.Tokens;
using MTConnectDashboard.MtMach;
using MTConnectDashboard.Pages;
using System;
using System.Net.NetworkInformation;
using System.Text;
using System.Text.RegularExpressions;
using System.Timers;
using static MTConnectDashboard.MSR1;

// This is for displaying the data onto the dashboard.

namespace MTConnectDashboard
{
    public class MSR1_column
    {
        public string? MachineID { get; set; }
        // This is the STATE column of the dashboard -> the time in state is only for the mtconnect changes since its for calculating cycletime
        public string? WorkCenterState { get; set; }
        public string? ProductionMonitorState { get; set; } // MTConnect

        // MTConnect
        public string? MachineMode { get; set; } // machine mode (head 1)
            public string? TimeInState {get; set;} // time the machine has been in that mode (head 1)
        public string? MachineMode2 { get; set; } // machine mode of head 2
            public string? TimeInState2 { get; set; } // time the machine has been in that mode for head 2
        public string? PartCount { get; set; } // part count head 1
        public string? PartCount2 { get;set; } //part count head2
        public string? DisplayPartCount { get; set; } // part count shown on the main dashboard
        
        public string? Cycletime { get;set;} // actual cycletime part to part
        public string? MachinCycle{ get; set; } //  the machine cycletime from running to part
        public string? Loadtime { get; set; } // actual loadtime from part to running
        public string? Setuptime { get; set; }

        // time calculations
        public string? PercentSpeedDifference { get;set; }
        public string? PercentCompletion { get; set; }
        public string? EstimatedJobTime { get; set; }
        public string? TimeLeftTillCompletion { get; set; }

        // SAP
        public string? ProductionOrder { get; set; }
        public string? Material { get; set; }
        public string? Confirmation { get; set; }
        public string? OP_Order { get; set; } // this is the operation from the sap order

        public string? IdealCycleTime { get; set; } // info 1
        public string? IdealLoadTime { get; set; } // info 2 IIOT
        public string? BaseQuantity { get; set; }
        public string? SAPSetupTime { get; set; } // sap setuptime in minutes
        public string? MachineCycletime { get ; set; }
        public string? PartCountTotal { get; set; } // order quantity in a job

        // kiosk monitoring
        public string? OP_ID { get; set; }
        public string? OP_SUP { get; set; }

        //public string? Operation { get; set; } // this is the event of the kiosk not the operation from sap
        public string? KioskState { get; set; } // this is the actual state of the kiosk derived from the kiosk events
            public string? TimeInKioskState { get; set; } // this is the time Duration for the Kiosk state
        public string? Kiosk_MTConnectState { get; set; }
        public string? Kiosk_MTConnectState2 { get; set; }

        public string? Kiosk_partcount { get; set; }
        public string? Kiosk_partcount2 { get; set; }

        // override
        public bool Override { get; set; }

        // shift
        public string? Wshift { get; set; }

        //MISC -> overrides mostly
        public string? StrMiscLabel1 { get; set; }
        public string? StrMiscLabel2 { get; set; }

        //override Display
        public string? Rapid { get; set; }
        public string? Feed { get; set; }
        public string? Spindle { get; set; }
        public string? Rapid2 { get; set; }
        public string? Feed2 { get; set; }
        public string? Spindle2 { get; set; }
        public string? Gantry { get; set; }


        // predictionary
        public string? Starttime { get; set; }
        public string? Expected_Finishtime { get; set; }
        public string? Prediction_Finishtime { get; set; }
        public string? EndSetupTime { get; set; }
        public bool SetupEnded { get; set; }
        public string? HoursLeft { get; set; }
    }

    public class MSR1_Service
    {
        #pragma warning disable CA2211 // Non-constant fields should not be visible
        public static List<MSR1_column> MSR1_columns = new()
        #pragma warning restore CA2211 // Non-constant fields should not be visible
        {
            //new MSR1_column() { MachineID = "SQUAG"},
            new MSR1_column() { MachineID = "2102L HAAS"},
            new MSR1_column() { MachineID = "2102M HAAS"},
            new MSR1_column() { MachineID = "2105 HAAS"},
            new MSR1_column() { MachineID = "2107 DOOSAN"},
            new MSR1_column() { MachineID = "2111 MAZAK"}, 
            new MSR1_column() { MachineID = "2112 MAZAK"}, 
            new MSR1_column() { MachineID = "2260 NEXUS"},
            new MSR1_column() { MachineID = "2271 DOOSAN"},
            new MSR1_column() { MachineID = "2272 MAZAK"},
            new MSR1_column() { MachineID = "2280 MAZAK"},
            new MSR1_column() { MachineID = "2281 MAZAK"},
            new MSR1_column() { MachineID = "2282 MAZAK"},
            new MSR1_column() { MachineID = "2283 MAZAK"},
            new MSR1_column() { MachineID = "2321 GRINDER"},
            new MSR1_column() { MachineID = "3111 SPARTAN"},
            new MSR1_column() { MachineID = "3112 DYNASAW"},
            new MSR1_column() { MachineID = "3321 BDTRON"}
        };

        // function for updating the values on the datagrid
        public void UpdateList(string machId, string MISC1, string MISC2, Watchlist DataOutput)
        {
            var result = from r in MSR1_columns where r.MachineID == machId select r;

			switch (DataOutput.ThyHOLYDashboardState)
            {
                case "timeout":
                    result.First().ProductionMonitorState = "Timeout";
                    break;
                case "schedmaint":
                    result.First().ProductionMonitorState = "SchedMaint";
                    break;
                case "unschedmaint":
                    result.First().ProductionMonitorState = "UnschedMaint";
                    break; 
                case "nojob":
                    result.First().ProductionMonitorState = "No Job/Op";
                    break;
                case "ready":
                    result.First().ProductionMonitorState = "Ready";
                    break;
                case "setup":
                    result.First().ProductionMonitorState = "Setup";
                    break;
                case "inspect":
                    result.First().ProductionMonitorState = "Insp/Tlch";
                    break;
                default:
                    result.First().ProductionMonitorState = DataOutput.ThyHOLYDashboardState; // the work center STATE
                    break;
            }

            if (DataOutput.TimeInWorkCenterState.TotalHours < 24)
            {
                result.First().WorkCenterState  = DataOutput.TimeInWorkCenterState.ToString().Remove(8); // time since the work center state has changed
            }
            else
            {
                result.First().WorkCenterState  = Math.Round(DataOutput.TimeInWorkCenterState.TotalHours, 2).ToString() + " hours";// time since the work center state has changed
            }

          // MTConnect Head 1 Machine State
            result.First().MachineMode = DataOutput.DisplayMachineState; // the mtconnect state  
                if (DataOutput.TimeInState.TotalHours < 24)
                {
                    result.First().TimeInState = DataOutput.TimeInState.ToString().Remove(8); // time since mt connect state changed.
                }
                else
                {
                    result.First().TimeInState = Math.Round(DataOutput.TimeInState.TotalHours, 2).ToString() + " hours";
                }
          //MTConnect Head 2 Machine State 
            result.First().MachineMode2 = DataOutput.DisplayMachineState2; // the mtconnect state
                if (DataOutput.TimeInState2.TotalHours < 24)
                {
                    result.First().TimeInState2 = DataOutput.TimeInState2.ToString().Remove(8); // time since mt connect state changed.
                }
                else
                {
                    result.First().TimeInState2 = Math.Round(DataOutput.TimeInState2.TotalHours, 2).ToString() + " hours";
                }
                
            if (DataOutput.PartCount2 == null)
            {
                DataOutput.PartCount2 = "N/A";
            }
            if(DataOutput.PartCount == "UNAVAILABLE")
            {
                DataOutput.PartCount = "0";
            }
            if(DataOutput.PartCount2 == "UNAVAILABLE")
            {
                DataOutput.PartCount2 = "0";
            }
             
            result.First().PartCount = DataOutput.PartCount; 
            result.First().PartCount2 = DataOutput.PartCount2;

            if (DataOutput.PartCount2 != "N/A" && DataOutput.PartCount2 != "0")
            {
                result.First().DisplayPartCount = DataOutput.PartCount2;
            }
            else
            {
                result.First().DisplayPartCount = DataOutput.PartCount;
            }

            // sap
            result.First().PartCountTotal = DataOutput.PartCountTotal;

            result.First().Cycletime = DataOutput.ActualCycletime.ToString().Remove(8);// cycletime on dashboard
            result.First().MachinCycle = DataOutput.MachinCycle.ToString().Remove(8); 
            result.First().Loadtime = DataOutput.Loadtime.ToString().Remove(8);
            result.First().Setuptime = DataOutput.ActualSetupTime.ToString().Remove(8); 

            result.First().IdealCycleTime = DataOutput.idealCycletime.ToString().Remove(8);
            result.First().IdealLoadTime = DataOutput.idealLoadTime.ToString().Remove(8);
            result.First().BaseQuantity = DataOutput.baseQuantity.ToString();
            result.First().SAPSetupTime = DataOutput.setuptime.ToString().Remove(8);
            result.First().MachineCycletime = DataOutput.MachineCycleTime.ToString().Remove(8); 

            // calculated value insertion
            if ((DataOutput.MachineState == "RUNNING" || DataOutput.MachineState == "LOADING" || DataOutput.MachineState2 == "RUNNING" || DataOutput.MachineState2 == "LOADING")
            && (DataOutput.idealCycletime != TimeSpan.Zero))
            {
                result.First().PercentSpeedDifference = (DataOutput.Percent_SpeedDifference + 100).ToString() + "%";
            }
            else
            {
                result.First().PercentSpeedDifference = "0%";
            }

            result.First().PercentCompletion = DataOutput.Percent_Completion.ToString() + "%";

            if (DataOutput.Estimated_Job_Time.TotalHours < 24)
            {
                result.First().EstimatedJobTime = DataOutput.Estimated_Job_Time.ToString().Remove(8);
            }
            else
            {
                result.First().EstimatedJobTime = Math.Round(DataOutput.Estimated_Job_Time.TotalHours,2).ToString() + " hours";
            }

            if (DataOutput.Time_Left_till_Completion.TotalHours < 24)
            {
                result.First().TimeLeftTillCompletion = DataOutput.Time_Left_till_Completion.ToString().Remove(8);
            }
            else
            {
                result.First().TimeLeftTillCompletion = Math.Round(DataOutput.Time_Left_till_Completion.TotalHours, 2).ToString() + " hours";
            }

            // SAP again
            result.First().Confirmation = DataOutput.Conf_Numb;
            result.First().ProductionOrder = DataOutput.Prod_Order; // sap prod order
            result.First().OP_Order = DataOutput.Operation_Number; // sap op
            result.First().Material = DataOutput.material;
            result.First().KioskState = DataOutput.KioskState; // kiosk state
            result.First().OP_ID = DataOutput.OP_ID;
            result.First().OP_SUP = DataOutput.SUP_ID;
 
            // kiosk monitoring
            result.First().Kiosk_MTConnectState = DataOutput.Kiosk_MTConnectState;
            result.First().Kiosk_MTConnectState2 = DataOutput.Kiosk_MTConnectState2;
            result.First().Kiosk_partcount = DataOutput.Kiosk_PartCount;
            result.First().Kiosk_partcount2 = DataOutput.Kiosk_PartCount2;

            // shift
            result.First().Wshift = DataOutput.Wshift;

            //override
            result.First().Override = DataOutput.KioskOveride;

            result.First().StrMiscLabel1 = MISC1;
            result.First().StrMiscLabel2 = MISC2;
          
            result.First().Rapid = DataOutput.Rapid;
            result.First().Rapid2 = DataOutput.Rapid2;
            result.First().Feed = DataOutput.Feed;
            result.First().Feed2 = DataOutput.Feed2;
            result.First().Spindle = DataOutput.Spindle;
            result.First().Spindle2 = DataOutput.Spindle2;
            result.First().Gantry = DataOutput.Gantry;

            // predictions
            if (DataOutput.JobStarttime!=DateTime.MinValue)
            {
                result.First().Starttime = DataOutput.JobStarttime.ToString("g");
                result.First().Expected_Finishtime = DataOutput.SAPJobEndtime.ToString("g");
                result.First().Prediction_Finishtime = DataOutput.PredictedEndtime.ToString("g");
                result.First().EndSetupTime = DataOutput.EndSetuptime.ToString("g");
                if (DataOutput.HoursLeft.TotalHours < 24)
                {
                    result.First().HoursLeft = DataOutput.HoursLeft.ToString().Remove(8);
                }
                else
                {
                    result.First().HoursLeft = Math.Round(DataOutput.HoursLeft.TotalHours, 2).ToString() + " hours";
                }
            }
            else
            {
                result.First().Starttime = "N/A";
                result.First().Expected_Finishtime = "N/A";
                result.First().Prediction_Finishtime = "N/A";
                result.First().EndSetupTime = "N/A";
            }
            result.First().SetupEnded = DataOutput.EndSetupTrigger;

        }

        public async Task<List<MSR1_column>> ProductList()
        {
            return await Task.FromResult(MSR1_columns);
        }

    }

    public class MSR1
    {
        public struct Watchlist // everything that can be directly taken from the mtconnect data 
        {
            //! http direct
            public TimeTracker direct_timez; // this the timez subsitute for httpdirect;
            public TimeTracker direct_timez2; // for head 2
            //! http direct

            //basic
            public string controllerMode; // automatic and Manual
            public string controllerMode2;
            public string Execution; // Active, Ready,Stopped, Wait, FeedHold
            public string Execution2;

            public string MachineState; // Running, Idle, Down, Manual
            public string MachineState2;

            public string DisplayMachineState; // for showing loading
            public string DisplayMachineState2;

            // state time recorder
            public DateTime MachineTime;
            public TimeSpan MachineTimeOffset;
            public DateTime MachineTime2;
            public TimeSpan MachineTimeOffset2;

            public DateTime PrevStatetime;
            public TimeSpan TimeInState;

            public DateTime PrevStatetime2;
            public TimeSpan TimeInState2;

            // part count and cycle timer
            public string PartCount; // machine part count head 1
            public string PartCount2;// machine part count head 2
            public string PartCountTotal; // sap quantity
           
            public TimeSpan Setuptimecounting; // grab the timespan while in setup
            public bool exitsetup; // trigger that only is active one cycle after setup is exited
            public TimeSpan ActualSetupTime; // the actual time it to setup to setup a job: Adds the time from in kiosk setup mode and then zeros when a new confirmation number is added

            public bool loadingindicator1; // tells if the machine is loading
            public bool timeoutindicator1; // tells us the kiosk went out of ready and prevents the cycletimes from being wrong
            public bool loadingindicator2; // tells if the machine is loading
            public bool timeoutindicator2; // tells us the kiosk went out of ready and prevents the cycletimes from being wrong

            public string partCountRepeat; // for grabbing previous part count number
            public DateTime prevtimez; // for grabbing previous time at partcount
            public TimeSpan Cycletime;       // calculated with part count part to part
                public TimeSpan MachCycle1; // cycletime of machine runing to part completion
                    public DateTime BeginCycle1;
                public TimeSpan LoadCycle1; // loading to running


            public string partCountRepeat2; // for grabbing previous part count number
            public DateTime prevtimez2; // for grabbing previous time at partcount
            public TimeSpan Cycletime2;       // calculated with part count part to part
               public TimeSpan MachCycle2; // cycletime of machine runing to part completion
                public DateTime BeginCycle2;
               public TimeSpan LoadCycle2; // loading to running head 2

            public TimeSpan MachinCycle; // time the machine is actually making the part.
            public TimeSpan Loadtime; // time in loading
            public TimeSpan ActualCycletime; // the cycletime we care about reporting
            
            public double Percent_SpeedDifference; // The percent difference between part compltion ideally vs actual. 
            public double Percent_Completion; //  percent completion = (partcount/SAP_Quantity) * 100
            public TimeSpan Estimated_Job_Time; // the total machine hours the job will take
            public TimeSpan Time_Left_till_Completion; // Estimated_Job_Time - Percent_Completion * Estimated job time

            // idle timer 
            public DateTime prevIdleTime;
            public TimeSpan IdleTimer;     // for machines that having loading between parts like 2260 and 2111
            public bool IdleTimerStatus; 
            public string IdleTimerDisplay; // for showing the idle state on the dashboard

            // idle timer2 
            public DateTime prevIdleTime2;
            public TimeSpan IdleTimer2;     // for machines that having loading between parts like 2260 and 2111
            public bool IdleTimerStatus2;
            public string IdleTimerDisplay2; // for showing the idle state on the dashboard

            // Program
            public string Active_Program;
            public string Active_Program2;

            public bool ProgActive;
            public bool ProgActive2;

            public string Main_Program;
            public string Main_Program2;

            //SAP confirmation number
            public string Conf_Numb;
            public string prev_Conf_Numb; // for detecting a change in job
            //SAP operation
            public string Operation_Number;
            //SAP Production Order
            public string Prod_Order;
            // SAP info 1
            public TimeSpan idealCycletime;
            // SAP info 2 
            public TimeSpan idealLoadTime;
            //sap base quantity
            public int baseQuantity;
            // sap setup time
            public TimeSpan setuptime;
            // machine cycle time
            public TimeSpan MachineCycleTime;

            //material or part label
            public string material;
            public string material2;

            // kiosk ids
            public string Operation; // this is the kiosk event
            public string KioskState; // these are the kiosk events that are state changes

            public string PrevKioskState; // for the timeout state

            public string ThyHOLYDashboardState; // this is the display for the main dashboard (Professional name pending)
           
            // Work center time in state
            public string prevThyHOLYDashboardState;
            public DateTime PrevWorkCenterTime;
            public TimeSpan TimeInWorkCenterState;

            // operator and supervisor id from kiosk
            public string OP_ID;
            public string SUP_ID;

            // kiosk debugs
            public string Kiosk_MTConnectState; //kiosk MtConnect read
            public string Kiosk_MTConnectState2; //kiosk MtConnect read for head 2
            // machine state according to the kiosk
            public string Kiosk_PartCount; // machine count accoring to the kiosk
            public string Kiosk_PartCount2; // machine count accoring to the kiosk

            // Overrides 
            public bool KioskOveride;
            public string Rapid; //(range is 0 to 100)
            public string Rapid2; //(range is 0 to 100)
            public string Feed;
            public string Feed2;
            public string Spindle;
            public string Spindle2;
            public string Gantry;

            // prediction elements
            public bool jobstarted;
            public string NewJob;
            public DateTime JobStarttime;
            public DateTime SAPJobEndtime;
            public DateTime PredictedEndtime;
            public TimeSpan HoursLeft;
            public string Pred_prev_state;
            public TimeSpan Pred_prev_timespan;
            public DateTime EndSetuptime;
            public bool EndSetupTrigger;

            // BDTronic only variables
            public Bdtronic.BDtronicKit BdtronicWatchlist;

            // amada only variables
            public Amada.Var_amada var_amada;

            // other 
            public bool AdapterOnline;
            public bool update; // signal for an update value
            public string StateSet; // this is for keeping track of time in state.
            public string StateSet2; // this is for keeping track of time in state.
            public string Wshift; // the shift the machine is in

        }

        public struct TimeTracker
        {
            public DateTime Modetime;

            public DateTime Executiontime;

            public DateTime MStateTime;

            public DateTime PartTime;

            public bool readsuccessful;
        }

        // this is the idle timer to get a cycletime while a machine is being loaded between cycles
        public static Watchlist IdleTimerDisplay(Watchlist watchlist) // occures when idle is detected
        { 
            // idle timer
            if(watchlist.MachineState == "RUNNING" ) // machine was running so the idle timer is turned on
            {
               // watchlist.prevIdleTime = DateTime.Now; // set a reference for the idle timer
                watchlist.IdleTimerStatus = true;
            }
            else if (watchlist.MachineState != "RUNNING") // This is when the machine was either down or in manual before going idle
            {
                watchlist.MachineState = "IDLE"; // set to Idle 
                //watchlist.Cycletime=TimeSpan.Zero; // reset the cycletime           
                watchlist.IdleTimer = TimeSpan.Zero; // reset the idle timer
                watchlist.IdleTimerDisplay="...";
            }

            return watchlist;
        }

        public static Watchlist IdleTimerDisplayH2(Watchlist watchlist)
        {
            // exexution is ready or wait and mode is AUTOMATIC
            if (watchlist.MachineState2 == "RUNNING") // machine was running so the idle timer is turned on
            {
                //watchlist.prevIdleTime2 = DateTime.Now; // set a reference for the idle timer
                watchlist.IdleTimerStatus2 = true;
            }
            else if (watchlist.MachineState2 != "RUNNING") // This is when the machine was either down or in manual before going idle
            {
                watchlist.MachineState2 = "IDLE"; // set to Idle 
                //watchlist.Cycletime2 = TimeSpan.Zero; // reset the cycletime
                watchlist.IdleTimer2 = TimeSpan.Zero; // reset the idle timer
                watchlist.IdleTimerDisplay2 = "...";
            }

            return watchlist;
        }

        // idle timer for loading parts 
        public static Watchlist IdleTimer(Watchlist DashboardData, TimeSpan TimeLimit)
        {
            if (DashboardData.IdleTimerStatus) // idle timer is on
            {
                if (DashboardData.Execution != "ACTIVE" && TimeLimit>TimeSpan.Zero)
                {
                    if (DashboardData.DisplayMachineState != "LOADING")
                    {
                        DashboardData.prevIdleTime = DateTime.Now;
                    }
                    DashboardData.DisplayMachineState = "LOADING"; // Show the dashboard that the machine is loading without messing up the cycltime calculation.
                }

                DashboardData.IdleTimer = DateTime.Now - DashboardData.prevIdleTime; // increment the idle timer
                //Console.WriteLine(DashboardData.IdleTimer.ToString());

                DashboardData.IdleTimerDisplay = " Loading " + Math.Round(TimeLimit.TotalSeconds - DashboardData.IdleTimer.TotalSeconds); // shows when the idle time is running
                // if time runs out
                if (TimeSpan.Compare(DashboardData.IdleTimer, TimeLimit) >= 0 || TimeLimit<=TimeSpan.Zero) // compater(1,b) ->  -1 if a is shorter, 0  if equal, and 1 if b is shorter
                {
                    DashboardData.IdleTimerStatus = false; // turn off the timer after time runs out
                    DashboardData.IdleTimerDisplay = "Inactive";

                    DashboardData.TimeInState = TimeLimit; //sets the time in state to the amount of time the machine was accually idle

                    DashboardData.PrevStatetime = DateTime.Now - TimeLimit; // timestate wont go to zero it will go to the timelimit
                    DashboardData.MachineTimeOffset = TimeSpan.Zero; // since I am not using The MTConnect Timestamp I am just assuming this value is zero
                    DashboardData.StateSet = DashboardData.MachineState;

                    DashboardData.MachineState = "IDLE"; // set to Idle after the idle timer finishes
                    
                    DashboardData.IdleTimer = TimeSpan.Zero; // reset the idle timer

                    // reset the cycletime
                    DashboardData.Cycletime = TimeSpan.Zero;
                    DashboardData.prevtimez = DateTime.MinValue;
                }
            }
            else // idle timer is off
            {
                /*
                if (DashboardData.DisplayMachineState != DashboardData.MachineState)
                {
                    DashboardData.Loadtime = DashboardData.IdleTimer; // record the time it took to load the part 
                    //Console.WriteLine(DashboardData.DisplayMachineState+ " "+ DateTime.Now + " : " + DashboardData.Loadtime);
                }
                */
                DashboardData.DisplayMachineState = DashboardData.MachineState; // Show the dashboard that the machine is loading without messing up the cycltime calculation.
                DashboardData.IdleTimer = TimeSpan.Zero; // reset the idle timer
            }

            // reset the idle timer if the execution changes 
            if ((DashboardData.Execution != "READY" &&  DashboardData.Execution != "WAIT") || DashboardData.controllerMode == "MANUAL")
            {
                DashboardData.IdleTimerStatus = false; // turn off before the timer runs out
                DashboardData.IdleTimerDisplay = "...";
            }
            return DashboardData;
        }

        // idle timer for loading parts for head 2
        public static Watchlist IdleTimerH2(Watchlist DashboardData, TimeSpan TimeLimit)
        {
            if (DashboardData.IdleTimerStatus2) // idle timer is on
            {
                if (DashboardData.Execution2 != "ACTIVE" && TimeLimit>TimeSpan.Zero)
                {
                    if (DashboardData.DisplayMachineState2 != "LOADING")
                    {
                        DashboardData.prevIdleTime2 = DateTime.Now;
                    }

                    DashboardData.DisplayMachineState2 = "LOADING"; // Show the dashboard that the machine is loading without messing up the cycltime calculation.
                }
                DashboardData.IdleTimer2 = DateTime.Now - DashboardData.prevIdleTime2; // increment the idle timer
                DashboardData.IdleTimerDisplay2 = " Loading " + Math.Round(TimeLimit.TotalSeconds - DashboardData.IdleTimer2.TotalSeconds); // shows when the idle time is running
                // if time runs out
                if (TimeSpan.Compare(DashboardData.IdleTimer2, TimeLimit) >= 0 || TimeLimit<=TimeSpan.Zero) // compater(1,b) ->  -1 if a is shorter, 0  if equal, and 1 if b is shorter
                {
                    DashboardData.IdleTimerStatus2 = false; // turn off the timer after time runs out
                    DashboardData.IdleTimerDisplay2 = "Inactive";

                    DashboardData.TimeInState2 = TimeLimit; //sets the time in state to the amount of time the machine was accually idle

                    DashboardData.PrevStatetime2 = DateTime.Now - TimeLimit; // timestate wont go to zero it will go to the timelimit
                    DashboardData.MachineTimeOffset2 = TimeSpan.Zero; // since I am not using The MTConnect Timestamp I am just assuming this value is zero
                    DashboardData.StateSet2 = DashboardData.MachineState2;

                    DashboardData.MachineState2 = "IDLE"; // set to Idle after the idle timer finishes
                    DashboardData.IdleTimer2 = TimeSpan.Zero; // reset the idle timer

                    // reset the cycletime
                    DashboardData.Cycletime2 = TimeSpan.Zero;
                    DashboardData.prevtimez2 = DateTime.MinValue;
                }
            }
            else
            {
                /*
                if (DashboardData.DisplayMachineState2 != DashboardData.MachineState2)
                {
                    DashboardData.Loadtime = DashboardData.IdleTimer2; // record the time it took to load the part 
                    //Console.WriteLine(DashboardData.DisplayMachineState+ " "+ DateTime.Now + " : " + DashboardData.Loadtime);
                }
                */
                DashboardData.DisplayMachineState2 = DashboardData.MachineState2; // Show the dashboard that the machine is loading without messing up the cycltime calculation.
                DashboardData.IdleTimer2 = TimeSpan.Zero; // reset the idle timer
            }
            // reset the idle timer if the execution changes 
            if ((DashboardData.Execution2 != "READY" &&  DashboardData.Execution2 != "WAIT") || DashboardData.controllerMode2 == "MANUAL")
            {
                DashboardData.IdleTimerStatus2 = false; // turn off before the timer runs out
                DashboardData.IdleTimerDisplay2 = "...";
            }
            return DashboardData;
        }

        public static Watchlist MtConnect_StateFinder(Watchlist watchlist, DateTime timeZ, DateTime timeZ2, int heads, bool Loader) 
        {
            //Console.WriteLine(watchlist.controllerMode + " " + watchlist.MachineState + " " + watchlist.StateSet); // debug
            // head 1
            if (watchlist.controllerMode == "AUTOMATIC"|| watchlist.controllerMode == "SEMI-AUTOMATIC" || watchlist.controllerMode == "SEMI_AUTOMATIC" || watchlist.controllerMode == "UNAVAILABLE" || watchlist.controllerMode == "EDIT")
            {
                switch (watchlist.Execution)
                {
                    case "ACTIVE":
                        watchlist.MachineState = "RUNNING"; // set to running
                        if (watchlist.MachineState != watchlist.StateSet)
                        {
                            watchlist.PrevStatetime = timeZ; //Timegrab(timeZ, watchlist.PrevStatetime);
                            watchlist.MachineTimeOffset = DateTime.Now - watchlist.PrevStatetime;
                            watchlist.StateSet = watchlist.MachineState;
                        }
                        break;
                    case "READY":
                        if (Loader)
                        {
                            watchlist = IdleTimerDisplay(watchlist);
                        }
                        else
                        {
                            watchlist.MachineState = "IDLE"; // set to Idle
                            watchlist.Cycletime=TimeSpan.Zero;
                        }

                        if (watchlist.MachineState!= watchlist.StateSet)
                        {
                            watchlist.PrevStatetime = timeZ; //Timegrab(timeZ, watchlist.PrevStatetime);
                            watchlist.MachineTimeOffset = DateTime.Now - watchlist.PrevStatetime;
                            watchlist.StateSet = watchlist.MachineState;
                        }
                        break;
                    case "WAIT":
                        if (Loader)
                        {
                            watchlist = IdleTimerDisplay(watchlist);
                        }
                        else
                        {
                            watchlist.MachineState = "IDLE"; // set to Idle
                           // watchlist.Cycletime=TimeSpan.Zero;
                        }
                        if (watchlist.MachineState!=watchlist.StateSet)
                        {
                            watchlist.PrevStatetime = timeZ;
                            watchlist.MachineTimeOffset = DateTime.Now - watchlist.PrevStatetime;
                            watchlist.StateSet = watchlist.MachineState;
                        }
                        break;
                    case "FEED_HOLD":
                        watchlist.MachineState = "FEED_HOLD"; // FEEDHOLD
                       // watchlist.Cycletime=TimeSpan.Zero;
                        if (watchlist.MachineState!=watchlist.StateSet)
                        {
                            watchlist.PrevStatetime = timeZ;
                            watchlist.MachineTimeOffset = DateTime.Now - watchlist.PrevStatetime;
                            watchlist.StateSet = watchlist.MachineState;
                        }
                        break;
                    case "INTERRUPTED":
                        watchlist.MachineState = "INTERUPT"; // set to down
                        //watchlist.Cycletime=TimeSpan.Zero;
                        if (watchlist.MachineState!=watchlist.StateSet)
                        {
                            watchlist.PrevStatetime = timeZ;
                            watchlist.MachineTimeOffset = DateTime.Now - watchlist.PrevStatetime;
                            watchlist.StateSet = watchlist.MachineState;
                        }
                        break;
                    case "STOPPED":
                        watchlist.MachineState = "PGM_STOP"; // set to down
                        watchlist.Cycletime=TimeSpan.Zero;
                        if (watchlist.MachineState!=watchlist.StateSet)
                        {
                            watchlist.PrevStatetime = timeZ;
                            watchlist.MachineTimeOffset = DateTime.Now - watchlist.PrevStatetime;
                            watchlist.StateSet = watchlist.MachineState;
                        }
                        break;
                    case "PROGRAM_STOPPED":
                        watchlist.MachineState = "PGM_STOP"; // set to down
                       // watchlist.Cycletime=TimeSpan.Zero;
                        if (watchlist.MachineState!=watchlist.StateSet)
                        {
                            watchlist.PrevStatetime =   timeZ;
                            watchlist.MachineTimeOffset = DateTime.Now - watchlist.PrevStatetime;
                            watchlist.StateSet = watchlist.MachineState;
                        }
                        break;
                    case "UNAVAILABLE":
                        watchlist.MachineState = "OFFLINE"; // set to down
                        watchlist.Cycletime=TimeSpan.Zero;
                        watchlist.PartCount="0";
                        if (watchlist.MachineState!=watchlist.StateSet)
                        {
                            watchlist.PrevStatetime = timeZ;
                            watchlist.MachineTimeOffset = DateTime.Now - watchlist.PrevStatetime;
                            watchlist.StateSet = watchlist.MachineState;
                        }
                        break;
                        /*
                    default:  // Stopped, 
                        watchlist.MachineState = "OFFLINE"; // set to down
                        watchlist.Cycletime = TimeSpan.Zero;
                        if (watchlist.MachineState!=watchlist.StateSet)
                        {
                            watchlist.PrevStatetime = Timegrab(timeZ, watchlist.PrevStatetime);
                            watchlist.MachineTimeOffset = DateTime.Now - watchlist.PrevStatetime;
                            watchlist.StateSet = watchlist.MachineState;
                        }
                        break;
                        */
                }
            }
            else if (watchlist.controllerMode == "MANUAL" || watchlist.controllerMode == "MANUAL_DATA_INPUT")
            {
                watchlist.MachineState = "MANUAL"; // set to manual
                watchlist.Cycletime=TimeSpan.Zero;
                if (watchlist.MachineState!=watchlist.StateSet)
                {
                    watchlist.PrevStatetime = timeZ ;
                    watchlist.MachineTimeOffset = DateTime.Now - watchlist.PrevStatetime;
                    watchlist.StateSet = watchlist.MachineState;
                }
            } 
            else if(!watchlist.controllerMode.IsNullOrEmpty())
            {
                watchlist.MachineState = "OFFLINE"; // set to manual
                watchlist.Cycletime=TimeSpan.Zero;
                watchlist.PartCount = "0";
                if (watchlist.MachineState!=watchlist.StateSet)
                {
                    watchlist.PrevStatetime = timeZ;
                    watchlist.MachineTimeOffset = DateTime.Now - watchlist.PrevStatetime;
                    watchlist.StateSet = watchlist.MachineState;
                }
            }
            // head 2 
            if(heads == 2) 
            {
                if (watchlist.controllerMode2 == "AUTOMATIC" || watchlist.controllerMode == "SEMI-AUTOMATIC" || watchlist.controllerMode2 == "SEMI_AUTOMATIC" || watchlist.controllerMode2 == "UNAVAILABLE" || watchlist.controllerMode2 == "EDIT")
                {
                    switch (watchlist.Execution2)
                    {
                        case "ACTIVE":
                            watchlist.MachineState2 = "RUNNING"; // set to running
                            if (watchlist.MachineState2 != watchlist.StateSet2)
                            {
                                watchlist.PrevStatetime2 = timeZ2; //Timegrab(timeZ2, watchlist.PrevStatetime2);
                                watchlist.MachineTimeOffset2 = DateTime.Now - watchlist.PrevStatetime2;
                                watchlist.StateSet2 = watchlist.MachineState2;
                            }
                            break;
                        case "READY":
                            if (Loader)
                            {
                                watchlist = IdleTimerDisplayH2(watchlist);
                            }
                            else
                            {
                                watchlist.MachineState2 = "IDLE"; // set to Idle
                                watchlist.Cycletime2=TimeSpan.Zero;
                            }
                            if (watchlist.MachineState2 != watchlist.StateSet2)
                            {
                                watchlist.PrevStatetime2 = timeZ2;
                                watchlist.MachineTimeOffset2 = DateTime.Now - watchlist.PrevStatetime2;
                                watchlist.StateSet2 = watchlist.MachineState2;
                            }
                            break;
                        case "WAIT":
                            if (Loader)
                            {
                                watchlist = IdleTimerDisplayH2(watchlist);
                            }
                            else
                            {
                                watchlist.MachineState2 = "IDLE"; // set to Idle
                                watchlist.Cycletime2=TimeSpan.Zero;
                            }
                            if (watchlist.MachineState2 != watchlist.StateSet2)
                            {
                                watchlist.PrevStatetime2 = timeZ2;
                                watchlist.MachineTimeOffset2 = DateTime.Now - watchlist.PrevStatetime2;
                                watchlist.StateSet2 = watchlist.MachineState2;
                            }
                            break;
                        case "FEED_HOLD":
                            watchlist.MachineState2 = "FEED_HOLD"; // FEEDHOLD
                                                                  // watchlist.Cycletime=TimeSpan.Zero;
                            if (watchlist.MachineState2!=watchlist.StateSet2)
                            {
                                watchlist.PrevStatetime2 =  timeZ2;
                                watchlist.MachineTimeOffset2 = DateTime.Now - watchlist.PrevStatetime2;
                                watchlist.StateSet2 = watchlist.MachineState2;
                            }
                            break;
                        case "INTERRUPTED":
                            watchlist.MachineState2 = "INTERUPT"; // set to down
                            //watchlist.Cycletime2=TimeSpan.Zero;
                            if (watchlist.MachineState2 != watchlist.StateSet2)
                            {
                                watchlist.PrevStatetime2 = timeZ2;
                                watchlist.MachineTimeOffset2 = DateTime.Now - watchlist.PrevStatetime2;
                                watchlist.StateSet2 = watchlist.MachineState2;
                            }
                            break;
                        case "STOPPED":
                            watchlist.MachineState2 = "PGM_STOP"; // set to down
                            watchlist.Cycletime2=TimeSpan.Zero;
                            if (watchlist.MachineState2!=watchlist.StateSet2)
                            {
                                watchlist.PrevStatetime2 = timeZ2;
                                watchlist.MachineTimeOffset2 = DateTime.Now - watchlist.PrevStatetime2;
                                watchlist.StateSet2 = watchlist.MachineState2;
                            }
                            break;
                        case "PROGRAM_STOPPED":
                            watchlist.MachineState2 = "PGM_STOP"; // set to down
                            watchlist.Cycletime2=TimeSpan.Zero;
                            if (watchlist.MachineState2 != watchlist.StateSet2)
                            {
                                watchlist.PrevStatetime2 =  timeZ2;
                                watchlist.MachineTimeOffset2 = DateTime.Now - watchlist.PrevStatetime2;
                                watchlist.StateSet2 = watchlist.MachineState2;
                            }
                            break;
                        case "UNAVAILABLE":
                            watchlist.MachineState2 = "OFFLINE"; // set to down
                            watchlist.Cycletime2 = TimeSpan.Zero;
                            watchlist.PartCount2 = "0";
                            if (watchlist.MachineState2 != watchlist.StateSet2)
                            {
                                watchlist.PrevStatetime2 = timeZ2;
                                watchlist.MachineTimeOffset2 = DateTime.Now - watchlist.PrevStatetime2;
                                watchlist.StateSet2 = watchlist.MachineState2;
                            }
                            break;
                            /*
                        default:  // Stopped, 
                            watchlist.MachineState = ""; // set to down
                            watchlist.Cycletime = TimeSpan.Zero;
                            if (watchlist.MachineState!=watchlist.StateSet)
                            {
                                watchlist.PrevStatetime = Timegrab(timeZ, watchlist.PrevStatetime);
                                watchlist.MachineTimeOffset = DateTime.Now - watchlist.PrevStatetime;
                                watchlist.StateSet = watchlist.MachineState;
                            }
                            break;
                            */
                    }
                }
                else if (watchlist.controllerMode2 == "MANUAL" || watchlist.controllerMode == "MANUAL_DATA_INPUT")
                {
                    watchlist.MachineState2 = "MANUAL"; // set to manual
                    watchlist.Cycletime2=TimeSpan.Zero;
                    if (watchlist.MachineState2!=watchlist.StateSet2)
                    {
                        watchlist.PrevStatetime2 = timeZ2;
                        watchlist.MachineTimeOffset2 = DateTime.Now - watchlist.PrevStatetime2;
                        watchlist.StateSet2 = watchlist.MachineState2;
                    }
                }
            }
            else
            {
                watchlist.MachineState2 = "N/A";
            }
            /*
            if (watchlist.KioskState != "ready") // if the kiosk times out show the cycletime as zero
            {
                watchlist.Cycletime = TimeSpan.Zero;
                watchlist.Cycletime2 = TimeSpan.Zero;
            }
            */
            return watchlist;
        }

        // for converting material number is askey
        public static string AskeyHelper(string input, char[] materialarray,int i){
            // set up the char array
            Array.Resize(ref materialarray, 16);
            //make the input a whole number
            input = input.Split('.')[0];
            //convert to askey
                materialarray[i]  = (char)Int32.Parse(input);
            // insert askey value into the string
            string Output = new (materialarray);
            return Output;
        }

        // for passing time
        public static Watchlist TimePass(Watchlist watchlist)
        {
            watchlist.TimeInState = StateTimeCalc(watchlist.PrevStatetime, watchlist.MachineTimeOffset); // find how long the machine was in a state 
            watchlist.TimeInState2 = StateTimeCalc(watchlist.PrevStatetime2, watchlist.MachineTimeOffset2); // head 2 
            return watchlist;
        }

        // grabs the time
        public static DateTime Timegrab(string timez, DateTime prev)
        {
            
            DateTime MachineTime;
            try
            {
                MachineTime = DateTime.Parse(timez);
            }
            catch
            {
                MachineTime = prev;
            }
            return MachineTime;
        }

        // calculate time
        public static TimeSpan StateTimeCalc(DateTime prevTime, TimeSpan MachineTimeOffset)
        {
            TimeSpan StateTime;
                
            if (prevTime != DateTime.MinValue)
            {
                StateTime = DateTime.Now - (prevTime+MachineTimeOffset);  // find how long the machine was in a state
            }
            else
            {
                StateTime = DateTime.Now - Program.StartTime;
            }
 
            return StateTime;
        }

        // find cycletime
        public static TimeSpan CycleTimeCalc(DateTime inputtime1, DateTime inputtime2)
        {
            try
            {
                //DateTime Date1 = DateTime.Parse(inputtime1);
               //DateTime Date2 = DateTime.Parse(inputtime2);
                TimeSpan dif = inputtime2 - inputtime1;
                TimeSpan output = dif; //Math.Round(dif.TotalMinutes, 2);
                return output;
            }
            catch(Exception e)
            {
                Console.WriteLine(e.ToString());
                return TimeSpan.Zero;
            }
    
        }

        public static Watchlist MakePredictions(Watchlist Dashboard)
        {

            if (double.TryParse(Dashboard.PartCount, out double PartCount) && double.TryParse(Dashboard.PartCountTotal, out double SAP_Quantity) && SAP_Quantity!=0)
            {
                int partsperCycle;
                if (Dashboard.baseQuantity<1)
                {
                    partsperCycle = 1;
                }
                else
                {
                    partsperCycle = Dashboard.baseQuantity;
                }
                // find the percent completion
                Dashboard.Percent_Completion = Math.Round(PartCount/SAP_Quantity * 100, 2);
                // Estimated Job Time
                Dashboard.Estimated_Job_Time = Dashboard.MachineCycleTime * (SAP_Quantity/partsperCycle);
                // time till completion
                Dashboard.Time_Left_till_Completion = Dashboard.Estimated_Job_Time * 0.01 * (100 - Dashboard.Percent_Completion);
            }

            Dashboard.SAPJobEndtime = Dashboard.JobStarttime.Add(Dashboard.setuptime).Add(Dashboard.Estimated_Job_Time);

            if (Dashboard.KioskState == "setup" && !Dashboard.jobstarted) // ideal case
            {
                Dashboard.EndSetuptime = DateTime.MinValue; // should show up as N/A on the dashboard
                Dashboard.JobStarttime = DateTime.Now;
                Dashboard.jobstarted = true;
                Dashboard.PredictedEndtime = Dashboard.SAPJobEndtime;
            }
            else if (Dashboard.JobStarttime == DateTime.MinValue) // unideal case
            {
                Dashboard.JobStarttime = DateTime.Now; // good enough grab data
            }

            if (Dashboard.KioskState != "setup" && Dashboard.jobstarted && Dashboard.Conf_Numb != Dashboard.NewJob)
            {
                Dashboard.jobstarted = false;
                Dashboard.NewJob = Dashboard.Conf_Numb;
            }

            TimeSpan Time_left_in_job = ((100 - Dashboard.Percent_Completion)/100) * Dashboard.Estimated_Job_Time;     

            if(Dashboard.KioskState == "setup") // setup is not complete
            {
                Dashboard.EndSetupTrigger = true; // activate trigger
                Dashboard.PredictedEndtime = DateTime.Now /*Dashboard.JobStarttime*/ + Time_left_in_job + Dashboard.setuptime;       
            } 
            else
            {
                if (Dashboard.EndSetupTrigger)
                {
                    Dashboard.EndSetuptime = DateTime.Now;
                    Dashboard.EndSetupTrigger = false; // reset trigger 
                }
                Dashboard.PredictedEndtime = DateTime.Now /*Dashboard.JobStarttime*/ + Time_left_in_job /*+ Dashboard.ActualSetupTime*/;
            }

            Dashboard.HoursLeft = Dashboard.PredictedEndtime - DateTime.Now;

            /*
           else if (Dashboard.KioskState == "ready"|| Dashboard.KioskState == "inpsect") // setup is done
           {
               if (Dashboard.EndSetupTrigger)
               {
                   Dashboard.EndSetuptime = DateTime.Now;
                   Dashboard.EndSetupTrigger = false; // reset trigger 
               }
               Dashboard.PredictedEndtime = Dashboard.JobStarttime + Time_left_in_job + Dashboard.ActualSetupTime;
           }
           else  // no job/maint
           { 
               if(Dashboard.Pred_prev_state != Dashboard.ThyHOLYDashboardState)
               {
                   Dashboard.PredictedEndtime = Dashboard.PredictedEndtime.Add(Dashboard.Pred_prev_timespan);
               }
               Dashboard.Pred_prev_timespan = Dashboard.TimeInWorkCenterState; // reset timspan
               Dashboard.Pred_prev_state = Dashboard.ThyHOLYDashboardState; // reset trigger
           }
              */
            return Dashboard;
        }

    }
}