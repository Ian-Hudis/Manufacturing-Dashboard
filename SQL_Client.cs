using Azure;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Radzen;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Data.SqlClient;
using System.Security.AccessControl;
using System.Text;
using System.Windows;
using System.Xml;
using static MTConnectDashboard.MSR1;
using static System.Runtime.InteropServices.JavaScript.JSType;

// This is for storing data into SQL.

namespace MTConnectDashboard
{

    public class SQL_Client
    {
     //! write to the sql server
        public struct SQL_Logger
        {
            public DateTime TimeMark;
            public uint Log_num;
            
            public string Plant;
            public string WorkCenter;
            public string Machine;
            public string MaterialNumber;
            public string ProductionOrder;
            public string Operator;
            public string ProdSupervisor;
            
            public string Event;
            public int Value;
           
            public string prev_Event;
            public int prev_Value;

            public MSR1.Watchlist prevdata;
            public PLCserver.PLC_Data prevPLCdata;


            public string LinkAddress;
            public string comment;

            public MSR1.Watchlist MTConnect_data;
            public string shift; // for getting the shift to log correctly in sql

            public TimeSpan loglimiter;
            public DateTime lastlogged;
        }

        public static SQL_Logger Update(SQL_Logger Data, PLC_Client.PLC_Data Blackbox, Watchlist DashboardData, string series, string tag, string shifttype, bool barload, bool Logmode)  // detects when an event happens in update and send query
        {
            Data.LinkAddress = Blackbox.linkaddress;
            Data.comment = Blackbox.comment;

            if (Logmode) // for release mode only
            {
                Data.loglimiter = DateTime.Now - Data.lastlogged;

                // back up the data every 30 seconds
                if (Data.loglimiter > new TimeSpan(0, 0, 30))//if (Data.Event != Data.prev_Event || Data.Value != Data.prev_Value) // saves the data on an event change
                {
                    SaveData(Data, DashboardData);
                    Data.lastlogged = DateTime.Now;
                }


                Data.TimeMark = DateTime.Now; // set the time

                // auto post the machine status when a shift ends so that excel can regulate the divide in shifts
                /*
                DateTime Start1stShift;
                DateTime Start2ndShift;
                DateTime Start3rdShift;
                DateTime StartOverNight;
                DateTime StartOverNight2;
                */
                Data.MTConnect_data = DashboardData;  // update the column info

                // log shift event
                /*
                switch (shifttype)
                {
                    case "mach":
                        Start1stShift = new(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 5, 0, 1); // hours, minutes,seconds (military time -> 5:00 am)
                        Start2ndShift = new(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 15, 30, 1); // hours, minutes,seconds (military time -> 3:30 pm)
                        StartOverNight = new(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 2, 0, 1);  // hours, minutes,seconds (military time - >2:00 am)

                        Start3rdShift = new(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 5, 0, 1); // hours, minutes,seconds (military time -> 5:00 am)
                        StartOverNight2 = new(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 17, 30, 1);  // hours, minutes,seconds (military time - >5:30 pm)

                        if (DateTime.Today.DayOfWeek != DayOfWeek.Friday && DateTime.Today.DayOfWeek != DayOfWeek.Saturday && DateTime.Today.DayOfWeek != DayOfWeek.Sunday) // the weekday
                        { // the weekday
                            if (Math.Round((Start1stShift - DateTime.Now).TotalSeconds) == 0 && Data.MTConnect_data.Wshift != "1st") // if the time difference is less than 2 seconds and 1st shift has not been declared yet
                            {
                                Data.Event = "BeginShift";
                                Data.Value = 1;
                                Data.MTConnect_data.Wshift = "1st";

                                Data = Export_To_SQL(Data, series, tag); // send query
                                Data.shift = Data.MTConnect_data.Wshift;
                            }
                            if (Math.Round((Start2ndShift - DateTime.Now).TotalSeconds) == 0 && Data.MTConnect_data.Wshift != "2nd")
                            {
                                Data.Event = "BeginShift";
                                Data.Value = 2;
                                Data.MTConnect_data.Wshift = "2nd";
                                Data = Export_To_SQL(Data, series, tag); // send query
                                Data.shift = Data.MTConnect_data.Wshift;
                            }
                            if (Math.Round((StartOverNight - DateTime.Now).TotalSeconds) == 0 && Data.MTConnect_data.Wshift != "Overnight")
                            {
                                Data.Event = "BeginShift";
                                Data.Value = 0;
                                Data.MTConnect_data.Wshift = "Overnight";
                                Data = Export_To_SQL(Data, series, tag); // send query
                                Data.shift = Data.MTConnect_data.Wshift;
                            }
                        }
                        else // the weekend
                        {

                            if (Math.Round((Start3rdShift - DateTime.Now).TotalSeconds) == 0 && Data.MTConnect_data.Wshift != "3rd")
                            {
                                Data.Event = "BeginShift";
                                Data.Value = 3;
                                Data.MTConnect_data.Wshift = "3rd";
                                Data = Export_To_SQL(Data, series, tag); // send query
                                Data.shift = Data.MTConnect_data.Wshift;
                            }
                            if (Math.Round((StartOverNight2 - DateTime.Now).TotalSeconds) == 0 && Data.MTConnect_data.Wshift != "Overnight2") // when 3rd shift ends
                            {
                                Data.Event = "BeginShift";
                                Data.Value = 0;
                                Data.MTConnect_data.Wshift = "Overnight2";
                                Data = Export_To_SQL(Data, series, tag); // send query
                                Data.shift = Data.MTConnect_data.Wshift;
                            }
                        }
                        break;
                    case "line": // the weekday
                        Start1stShift = new(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 6, 0, 1); // hours, minutes,seconds (military time -> 6:00 am)
                        Start2ndShift = new(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 14, 30, 1); // hours, minutes,seconds (military time -> 2:30 pm)
                        StartOverNight = new(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 1, 0, 1);  // hours, minutes,seconds (military time - >1:00 am)

                        Start3rdShift = new(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 5, 0, 1); // hours, minutes,seconds (military time -> 5:00 am)
                        StartOverNight2 = new(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 17, 30, 1);  // hours, minutes,seconds (military time - >5:30 pm)

                        if (DateTime.Today.DayOfWeek != DayOfWeek.Saturday && DateTime.Today.DayOfWeek != DayOfWeek.Sunday) // the weekday
                        { // the weekday
                            if (Math.Round((Start1stShift - DateTime.Now).TotalSeconds) == 0 && Data.MTConnect_data.Wshift != "1st") // if the time difference is less than 2 seconds and 1st shift has not been declared yet
                            {
                                Data.Event = "BeginShift";
                                Data.Value = 1;
                                Data.MTConnect_data.Wshift = "1st";
                                Data = Export_To_SQL(Data, series, tag); // send query
                                Data.shift = Data.MTConnect_data.Wshift;

                            }
                            if (Math.Round((Start2ndShift - DateTime.Now).TotalSeconds) == 0 && Data.MTConnect_data.Wshift != "2nd")
                            {
                                Data.Event = "BeginShift";
                                Data.Value = 2;
                                Data.MTConnect_data.Wshift = "2nd";
                                Data = Export_To_SQL(Data, series, tag); // send query
                                Data.shift = Data.MTConnect_data.Wshift;
                            }
                            if (Math.Round((StartOverNight - DateTime.Now).TotalSeconds) == 0 && Data.MTConnect_data.Wshift != "Overnight")
                            {
                                Data.Event = "BeginShift";
                                Data.Value = 0;
                                Data.MTConnect_data.Wshift = "Overnight";
                                Data = Export_To_SQL(Data, series, tag); // send query
                                Data.shift = Data.MTConnect_data.Wshift;
                            }
                        }
                        else // the weekend
                        {

                            if (Math.Round((Start3rdShift - DateTime.Now).TotalSeconds) == 0 && Data.MTConnect_data.Wshift != "3rd")
                            {
                                Data.Event = "BeginShift";
                                Data.Value = 3;
                                Data.MTConnect_data.Wshift = "3rd";
                                Data = Export_To_SQL(Data, series, tag); // send query
                                Data.shift = Data.MTConnect_data.Wshift;
                            }
                            if (Math.Round((StartOverNight2 - DateTime.Now).TotalSeconds) == 0 && Data.MTConnect_data.Wshift != "Overnight2") // when 3rd shift ends
                            {
                                Data.Event = "BeginShift";
                                Data.Value = 0;
                                Data.MTConnect_data.Wshift = "Overnight2";
                                Data = Export_To_SQL(Data, series, tag); // send query
                                Data.shift = Data.MTConnect_data.Wshift;
                            }
                        }
                        break;
                }
                */
                if (Data.shift != null)
                {
                    Data.MTConnect_data.Wshift = Data.shift; // makes it so the shift gets logged to every row
                }

                if (Data.prevPLCdata.Sevent==null)
                {
                    //  Data.Event = "ProgramMod";
                    //  Data.MTConnect_data.Kiosk_State="";
                    //  Data = Export_To_SQL(Data, series, tag); // send query
                    // Data.Value = 0; // plc doesnt really come with values
                    Data.prevPLCdata.Sevent = Blackbox.Event;
                }

                //! Blackbox data
                if (Blackbox.Material != Data.prevPLCdata.Material) // partnumber
                {
                    Data.MaterialNumber = Blackbox.Material;
                    Data.prevPLCdata.Material = Blackbox.Material;
                }
                if (Blackbox.ProductionOrder != Data.prevPLCdata.ProductionOrder) // Production Order
                {
                    Data.ProductionOrder = Blackbox.ProductionOrder;
                    Data.prevPLCdata.ProductionOrder = Blackbox.ProductionOrder;
                }
                if (Blackbox.Operator != Data.prevPLCdata.Operator) // Operator
                {
                    Data.Operator = Blackbox.Operator;
                    Data.prevPLCdata.Operator = Blackbox.Operator;
                }
                if (Blackbox.Supervisor != Data.prevPLCdata.Supervisor) // Supervisor
                {
                    Data.ProdSupervisor = Blackbox.Supervisor;
                    Data.prevPLCdata.Supervisor = Blackbox.Supervisor;
                }

                if (Blackbox.Event != Data.prevPLCdata.Sevent && Blackbox.Event != "") // the Blackbox.Event != "" is to prevent program from logging a black event when the kiosk server refreshes
                {
                    Data.Event = Blackbox.Event;
                    Data.Value = 0; // plc doesnt really come with values
                    Data = Export_To_SQL(Data, series, tag, true); // send query
                    
                    Data.prevPLCdata.Sevent = Blackbox.Event;
                }
                //! Blackbox data

                //! Machine State
                if (DashboardData.DisplayMachineState != Data.prevdata.DisplayMachineState && barload == false && Data.prevPLCdata.Sevent!=null)
                {
                    Data.Event = DashboardData.DisplayMachineState;
                    Data.Value = 0;
                    Data = Export_To_SQL(Data, series, tag, false); // send query

                    Data.prevdata.DisplayMachineState = DashboardData.DisplayMachineState;
                }
                else if (DashboardData.DisplayMachineState != Data.prevdata.DisplayMachineState && barload == true && Data.prevPLCdata.Sevent!=null)
                {
                    if (DashboardData.MachineState.Contains("LOADING"))
                    {
                        Data.Event = DashboardData.DisplayMachineState;
                        Data.Value = 0;
                        Data = Export_To_SQL(Data, series, tag, false); // send query

                        Data.prevdata.DisplayMachineState = DashboardData.DisplayMachineState;
                    }
                }
                //! Machine State

                //! Machine State 2
                if (DashboardData.DisplayMachineState2 != Data.prevdata.DisplayMachineState2 && barload == false && Data.prevPLCdata.Sevent!=null && DashboardData.MachineState2!="N/A")
                {
                    Data.Event = DashboardData.DisplayMachineState2 + "2";// show head 2 as an event
                    Data.Value = 0;
                    Data = Export_To_SQL(Data, series, tag, false); // send query

                    Data.prevdata.DisplayMachineState2 = DashboardData.DisplayMachineState2;
                }
                else if (DashboardData.DisplayMachineState2 != Data.prevdata.DisplayMachineState2 && barload == true && Data.prevPLCdata.Sevent!=null)
                {
                    if (DashboardData.MachineState2.Contains("LOADING"))
                    {
                        Data.Event = DashboardData.DisplayMachineState2 + "2";
                        Data.Value = 0;
                        Data = Export_To_SQL(Data, series, tag, false); // send query

                        Data.prevdata.DisplayMachineState2 = DashboardData.DisplayMachineState2;
                    }
                }
                //! Machine State 2

                //! part count
                if (DashboardData.PartCount != Data.prevdata.PartCount && DashboardData.var_amada.machinestatus!="OFF")
                {
                    Data.Event = "PART";
                    try
                    {
                        Data.Value = int.Parse(DashboardData.PartCount);
                    }
                    catch
                    {
                        Data.Value = 0;
                        Console.WriteLine("Failed to parse PartCount.");
                    }

                    Data = Export_To_SQL(Data, series, tag, false); // send query
                    Data.prevdata.PartCount = DashboardData.PartCount;
                }
                //! part count
                //! part count 2
                if (DashboardData.PartCount2 != Data.prevdata.PartCount2)
                {
                    Data.Event = "PART2";
                    try
                    {
                        Data.Value = int.Parse(DashboardData.PartCount2);
                    }
                    catch
                    {
                        Data.Value = 0;
                        Console.WriteLine("Failed to parse PartCount.");
                    }

                    Data = Export_To_SQL(Data, series, tag, false); // send query
                    Data.prevdata.PartCount2 = DashboardData.PartCount2;
                }
                //! part count2

                //! OverRides
                // Head 1 Rapid
                if (DashboardData.Rapid != Data.prevdata.Rapid)
                {
                    Data.Event = "H1_RAP";
                    try
                    {
                        Data.Value = Int32.Parse(DashboardData.Rapid);
                    }
                    catch
                    {
                        Console.WriteLine("Failed to parse H1_Rap.");
                    }

                    Data = Export_To_SQL(Data, series, tag, false); // send query
                    Data.prevdata.Rapid = DashboardData.Rapid;
                }
                // Head 2 Rapid
                if (DashboardData.Rapid2 != Data.prevdata.Rapid2)
                {
                    Data.Event = "H2_RAP";
                    try
                    {
                        Data.Value = Int32.Parse(DashboardData.Rapid2);
                    }
                    catch
                    {
                        Console.WriteLine("Failed to parse H2_Rap.");
                    }
                    Data = Export_To_SQL(Data, series, tag, false); // send query
                    Data.prevdata.Rapid2 = DashboardData.Rapid2;
                }
                // Head 1 Feed
                if (DashboardData.Feed != Data.prevdata.Feed)
                {
                    Data.Event = "H1_FEED";
                    try
                    {
                        Data.Value = Int32.Parse(DashboardData.Feed);
                    }
                    catch
                    {
                        Console.WriteLine("Failed to parse H1_Feed");
                    }
                    Data = Export_To_SQL(Data, series, tag, false); // send query
                    Data.prevdata.Feed = DashboardData.Feed;
                }
                // Head 2 Feed
                if (DashboardData.Feed2 != Data.prevdata.Feed2)
                {
                    Data.Event = "H2_FEED";
                    try
                    {
                        Data.Value = Int32.Parse(DashboardData.Feed2);
                    }
                    catch
                    {
                        Console.WriteLine("Failed to parse H2_Feed");
                    }
                    Data = Export_To_SQL(Data, series, tag, false); // send query
                    Data.prevdata.Feed2 = DashboardData.Feed2;
                }
                // Head 1 Spindle
                if (DashboardData.Spindle != Data.prevdata.Spindle)
                {
                    Data.Event = "H1_SPIN";
                    try
                    {
                        Data.Value = Int32.Parse(DashboardData.Spindle);
                    }
                    catch
                    {
                        Console.WriteLine("Failed to parse H1_Spin");
                    }
                    Data = Export_To_SQL(Data, series, tag, false); // send query
                    Data.prevdata.Spindle = DashboardData.Spindle;
                }
                // Head 2 Spindle
                if (DashboardData.Spindle2 != Data.prevdata.Spindle2)
                {
                    Data.Event = "H2_SPIN";
                    try
                    {
                        Data.Value = Int32.Parse(DashboardData.Spindle2);
                    }
                    catch
                    {
                        Console.WriteLine("Failed to parse H2_Spin");
                    }
                    Data = Export_To_SQL(Data, series, tag, false); // send query
                    Data.prevdata.Spindle2 = DashboardData.Spindle2;
                }
                // Gantry
                if (DashboardData.Gantry != Data.prevdata.Gantry)
                {
                    Data.Event = "GANTRY";
                    try
                    {
                        Data.Value = Int32.Parse(DashboardData.Gantry);
                    }
                    catch
                    {
                        Console.WriteLine("Failed to parse Gantry");
                    }
                    Data = Export_To_SQL(Data, series, tag, false); // send query
                    Data.prevdata.Gantry = DashboardData.Gantry;
                }
                //! OverRides

                if (Data.Event != Data.prev_Event || Data.Value != Data.prev_Value)
                {
                    Data.prevdata = DashboardData;
                }
            }
            else 
            { 
                if (Data.Event != Data.prev_Event || Data.Value != Data.prev_Value)
                {
                    Data.prevdata = DashboardData;
                }
            }

            return Data;
        }

        private static SQL_Logger Export_To_SQL(SQL_Logger Data, string series, string tag, bool kioskeventchange)
        {
            var cs = @"Server=SQL_Server_Name\IN01;Database=ProductionMonitor; User ID=SQL_ID;Password=SQL_Password";

            var con = new SqlConnection(cs);
            try
            {
                con.Open();

                Data.Log_num++;

                var query = "INSERT INTO [ProductionMonitor].[dbo].["+series+tag+"_ProdMonitor]" +
                    "(plant, WorkCenter, machine, ControllerTime, Material, ProductionOrder, Operator, ProdSupervisor, Event, Value," + // 1
                    " SAP_Cycletime, SAP_Loadtime, Actual_Cycletime, Actual_MachCycle, Actual_Loadtime, SAP_SerialNumber, Serial_Internal, Result, WShift," + //2
                    " MT_state, MT_State2,  MT_pc1, MT_pc2, MT_H1Rapid, MT_H1Feed, MT_H1Spindle, MT_H2Rapid, MT_H2Feed, MT_H2Spindle, MT_Gantry," + //3
                    " Kiosk_Override, SAP_Quantity, DAY_OF_WEEK, Loader_Type, " + // 4
                    " Kiosk_Op, Kiosk_CN, Kiosk_Setuptime, SAP_Setuptime, SAP_Machine_Cycletime," + //5
                    " SAP_Base_Quantity, Kiosk_State, LinkAddress)" + // 6
                    " VALUES(@plant, @WorkCenter, @machine, @ControllerTime, @Material, @ProductionOrder, @Operator, @ProdSupervisor, @Event, @Value," + // 1
                    " @SAP_Cycletime, @SAP_Loadtime, @Actual_Cycletime, @Actual_MachCycle, @Actual_Loadtime, @SAP_SerialNumber, @Serial_Internal, @Result, @WShift,"+ //  2
                    " @MT_State, @MT_State2, @MT_pc1, @MT_pc2, @MT_H1Rapid, @MT_H1Feed, @MT_H1Spindle, @MT_H2Rapid, @MT_H2Feed, @MT_H2Spindle, @MT_Gantry,"+ //3
                    " @Kiosk_Override, @SAP_Quantity, @DAY_OF_WEEK, @Loader_Type,"+ //4
                    " @Kiosk_Op, @Kiosk_CN, @Kiosk_Setuptime, @SAP_SetupTime, @SAP_Machine_Cycletime," +//5
                    " @SAP_Base_Quantity, @Kiosk_State, @linkaddress)"; //6

                using var cmd = new SqlCommand(query, con);

                cmd.Parameters.Add(new SqlParameter("@plant", SqlDbType.Char, 4)).Value = Data.Plant;
                cmd.Parameters.Add(new SqlParameter("@WorkCenter", SqlDbType.Char, 5)).Value = Data.WorkCenter;
                cmd.Parameters.Add(new SqlParameter("@machine", SqlDbType.Char, 32)).Value = Data.Machine;
                cmd.Parameters.Add("@ControllerTime", SqlDbType.DateTime).Value = Data.TimeMark;
                cmd.Parameters.Add(new SqlParameter("@Material", SqlDbType.Char, 12)).Value = Data.MaterialNumber;  // larger string
                cmd.Parameters.Add(new SqlParameter("@ProductionOrder", SqlDbType.Char, 8)).Value = Data.ProductionOrder;  // larger string
                cmd.Parameters.Add(new SqlParameter("@Operator", SqlDbType.NVarChar, 50)).Value = Data.Operator;
                cmd.Parameters.Add(new SqlParameter("@ProdSupervisor", SqlDbType.NVarChar, 50)).Value = Data.ProdSupervisor;
                cmd.Parameters.Add(new SqlParameter("@Event", SqlDbType.Char, 16)).Value = Data.Event;
                cmd.Parameters.Add(new SqlParameter("@Value", SqlDbType.Int)).Value = Data.Value;
                //skip servertime
                cmd.Parameters.Add(new SqlParameter("@SAP_Cycletime", SqlDbType.BigInt)).Value = (long)Math.Round(Data.MTConnect_data.idealCycletime.TotalSeconds); // info field 1
                cmd.Parameters.Add(new SqlParameter("@SAP_Loadtime", SqlDbType.BigInt)).Value = (long)Math.Round(Data.MTConnect_data.idealLoadTime.TotalSeconds); // info field 2 IIOT
                cmd.Parameters.Add(new SqlParameter("@Actual_Cycletime", SqlDbType.BigInt)).Value = (long)Math.Round(Data.MTConnect_data.ActualCycletime.TotalSeconds);  // Dashboard Cycletime part to part
                cmd.Parameters.Add(new SqlParameter("@Actual_MachCycle", SqlDbType.BigInt)).Value = (long)Math.Round(Data.MTConnect_data.MachinCycle.TotalSeconds);  // Machine Cycletime
                cmd.Parameters.Add(new SqlParameter("@Actual_Loadtime", SqlDbType.BigInt)).Value = (long)Math.Round(Data.MTConnect_data.Loadtime.TotalSeconds);  // Dashboard loadtime
                cmd.Parameters.Add(new SqlParameter("@SAP_SerialNumber", SqlDbType.Int)).Value = 0; // definition pending
                cmd.Parameters.Add(new SqlParameter("@Serial_Internal", SqlDbType.Int)).Value = 0; // definition pending
                cmd.Parameters.Add(new SqlParameter("@Result", SqlDbType.Bit)).Value = true;
                if (Data.MTConnect_data.Wshift == null) { Data.MTConnect_data.Wshift = ""; }
                cmd.Parameters.Add(new SqlParameter("@WShift", SqlDbType.Char, 10)).Value = Data.MTConnect_data.Wshift;
                // MTConnect
                if (Data.MTConnect_data.MachineState == null) { Data.MTConnect_data.MachineState = ""; }
                cmd.Parameters.Add(new SqlParameter("@MT_State", SqlDbType.Char, 10)).Value = Data.MTConnect_data.MachineState;
                cmd.Parameters.Add(new SqlParameter("@MT_State2", SqlDbType.Char, 10)).Value = Data.MTConnect_data.MachineState2;
                int Default1 = 0;
                if (Data.MTConnect_data.PartCount != null && int.TryParse(Data.MTConnect_data.PartCount, out Default1))
                {
                    cmd.Parameters.Add(new SqlParameter("@MT_pc1", SqlDbType.Int)).Value = int.Parse(Data.MTConnect_data.PartCount);
                }
                else
                {
                    cmd.Parameters.Add(new SqlParameter("@MT_pc1", SqlDbType.Int)).Value = Default1;
                }

                if (Data.MTConnect_data.PartCount2 != null && int.TryParse(Data.MTConnect_data.PartCount2, out Default1))
                {
                    cmd.Parameters.Add(new SqlParameter("@MT_pc2", SqlDbType.Int)).Value = int.Parse(Data.MTConnect_data.PartCount2);
                }
                else
                {
                    cmd.Parameters.Add(new SqlParameter("@MT_pc2", SqlDbType.Int)).Value = Default1;
                }

                // MTConnect Overrides
                short Default2 = 0;
                if (Data.MTConnect_data.Rapid != null && short.TryParse(Data.MTConnect_data.Rapid, out Default2))
                {
                    cmd.Parameters.Add(new SqlParameter("@MT_H1Rapid", SqlDbType.SmallInt)).Value = short.Parse(Data.MTConnect_data.Rapid);
                }
                else
                {
                    cmd.Parameters.Add(new SqlParameter("@MT_H1Rapid", SqlDbType.SmallInt)).Value = Default2;
                }
                if (Data.MTConnect_data.Feed != null && short.TryParse(Data.MTConnect_data.Feed, out Default2))
                {
                    cmd.Parameters.Add(new SqlParameter("@MT_H1Feed", SqlDbType.SmallInt)).Value = short.Parse(Data.MTConnect_data.Feed);
                }
                else
                {
                    cmd.Parameters.Add(new SqlParameter("@MT_H1Feed", SqlDbType.SmallInt)).Value = Default2;
                }
                if (Data.MTConnect_data.Spindle != null && short.TryParse(Data.MTConnect_data.Spindle, out Default2))
                {
                    cmd.Parameters.Add(new SqlParameter("@MT_H1Spindle", SqlDbType.SmallInt)).Value = short.Parse(Data.MTConnect_data.Spindle);
                }
                else
                {
                    cmd.Parameters.Add(new SqlParameter("@MT_H1Spindle", SqlDbType.SmallInt)).Value = Default2;
                }
                if (Data.MTConnect_data.Rapid2 != null && short.TryParse(Data.MTConnect_data.Rapid2, out Default2))
                {
                    cmd.Parameters.Add(new SqlParameter("@MT_H2Rapid", SqlDbType.SmallInt)).Value = short.Parse(Data.MTConnect_data.Rapid2);
                }
                else
                {
                    cmd.Parameters.Add(new SqlParameter("@MT_H2Rapid", SqlDbType.SmallInt)).Value = Default2;
                }
                if (Data.MTConnect_data.Feed2 != null && short.TryParse(Data.MTConnect_data.Feed2, out Default2))
                {
                    cmd.Parameters.Add(new SqlParameter("@MT_H2Feed", SqlDbType.SmallInt)).Value = short.Parse(Data.MTConnect_data.Feed2);
                }
                else
                {
                    cmd.Parameters.Add(new SqlParameter("@MT_H2Feed", SqlDbType.SmallInt)).Value = Default2;
                }
                if (Data.MTConnect_data.Spindle2 != null && short.TryParse(Data.MTConnect_data.Spindle2, out Default2))
                {
                    cmd.Parameters.Add(new SqlParameter("@MT_H2Spindle", SqlDbType.SmallInt)).Value = short.Parse(Data.MTConnect_data.Spindle2);
                }
                else
                {
                    cmd.Parameters.Add(new SqlParameter("@MT_H2Spindle", SqlDbType.SmallInt)).Value = Default2;
                }
                if (Data.MTConnect_data.Gantry != null && short.TryParse(Data.MTConnect_data.Gantry, out Default2))
                {
                    cmd.Parameters.Add(new SqlParameter("@MT_Gantry", SqlDbType.SmallInt)).Value = short.Parse(Data.MTConnect_data.Gantry);
                }
                else
                {
                    cmd.Parameters.Add(new SqlParameter("@MT_Gantry", SqlDbType.SmallInt)).Value = Default2;
                }
                cmd.Parameters.Add(new SqlParameter("@Kiosk_Override", SqlDbType.Bit)).Value = Data.MTConnect_data.KioskOveride; // kiosk Override -> Goes from bool to bit
                                                                                                                                 //
                if (Data.MTConnect_data.PartCountTotal != null && int.TryParse(Data.MTConnect_data.PartCountTotal, out Default1))
                {
                    cmd.Parameters.Add(new SqlParameter("@SAP_Quantity", SqlDbType.Int)).Value = int.Parse(Data.MTConnect_data.PartCountTotal);
                }
                else
                {
                    cmd.Parameters.Add(new SqlParameter("@SAP_Quantity", SqlDbType.Int)).Value = Default1;
                }
                cmd.Parameters.Add(new SqlParameter("@DAY_OF_WEEK", SqlDbType.Char, 4)).Value = Data.TimeMark.DayOfWeek.ToString()[..3];
                cmd.Parameters.Add(new SqlParameter("@Loader_Type", SqlDbType.Char, 10)).Value = "";
                if (Data.MTConnect_data.Operation_Number == null) { Data.MTConnect_data.Operation_Number = ""; }
                cmd.Parameters.Add(new SqlParameter("@Kiosk_Op", SqlDbType.Char, 5)).Value = Data.MTConnect_data.Operation_Number; //SAP operation
                if (Data.MTConnect_data.Conf_Numb == null) { Data.MTConnect_data.Conf_Numb = ""; }
                cmd.Parameters.Add(new SqlParameter("@Kiosk_CN", SqlDbType.Char, 10)).Value = Data.MTConnect_data.Conf_Numb; //Confirmation Number
                cmd.Parameters.Add(new SqlParameter("@Kiosk_Setuptime", SqlDbType.BigInt)).Value = (long)Math.Round(Data.MTConnect_data.ActualSetupTime.TotalSeconds); // log the time spent setting up the machine according to the kiosk
                cmd.Parameters.Add(new SqlParameter("@SAP_Setuptime", SqlDbType.BigInt)).Value = (long)Math.Round(Data.MTConnect_data.setuptime.TotalSeconds); //SAP setuptime
                cmd.Parameters.Add(new SqlParameter("@SAP_Machine_Cycletime", SqlDbType.BigInt)).Value = (long)Math.Round(Data.MTConnect_data.MachineCycleTime.TotalSeconds); //SAP operation 
                cmd.Parameters.Add(new SqlParameter("@SAP_Base_Quantity", SqlDbType.Int)).Value = Data.MTConnect_data.baseQuantity; // parts done in a cycle

                if (Data.MTConnect_data.Operation == null) { Data.MTConnect_data.Operation = ""; }
                if (Data.MTConnect_data.KioskState == null) { Data.MTConnect_data.KioskState = ""; }
                cmd.Parameters.Add(new SqlParameter("@Kiosk_State", SqlDbType.Char, 10)).Value = Data.MTConnect_data.KioskState; // what is the kiosk state

                // add a link to the machine table if there is a comment for a particular event.
                if (Data.comment != "" && Data.comment != null)
                {
                    cmd.Parameters.Add(new SqlParameter("@linkaddress", SqlDbType.NVarChar, 64)).Value = Data.LinkAddress; // possible link address
                }
                else
                {
                    cmd.Parameters.Add(new SqlParameter("@linkaddress", SqlDbType.NVarChar, 64)).Value = ""; // no comment no link address 
                }


                cmd.Prepare();
                cmd.ExecuteNonQuery();
                // Console.WriteLine("Query "+ Data.Log_num + " inserted");
            }
            /*catch (Exception EX)
            {
                Console.WriteLine("Query "+ Data.Log_num + " " +  Data.WorkCenter + " "+   Data.Machine +  " Failed to Send");
                Console.WriteLine(EX);
                con.Close();
            }*/
            finally
            {
                con.Close();
            }

            if (kioskeventchange && Data.comment!=null && Data.comment !="") // log comment when commenting happens
            {
                Commenting.SQL_InsertAComment(Data.LinkAddress, tag, Data.Operator, Data.comment); //
                
            }

            return Data;
        }

        public static SQL_Logger SQL_Init(SQL_Logger Data, string plant, string workcenter, string machine, string series, bool LogMode)
        {
            // dont want to read and write to sql at the same time
            Data.lastlogged = DateTime.Now; 
            Data.loglimiter = TimeSpan.Zero;
            //
            Data.Plant = plant;
            Data.WorkCenter = workcenter;
            Data.Machine = machine;
            Data.TimeMark = DateTime.Now;
            Data.MaterialNumber = "";
            Data.ProductionOrder = "";
            Data.Operator = "";
            Data.ProdSupervisor = "";
            Data.Event = "ProgramMod";
            Data.Value = 0;
            // servertime is logged in sql
            Data.MTConnect_data.idealCycletime = TimeSpan.Zero;
            Data.MTConnect_data.idealLoadTime = TimeSpan.Zero;
            Data.MTConnect_data.Cycletime = TimeSpan.Zero;

            Data.MTConnect_data.Wshift = "...";

            Data.MTConnect_data.MachineState = "...";
            Data.MTConnect_data.MachineState2 = "...";

            Data.MTConnect_data.KioskOveride = false;

            Data.MTConnect_data.setuptime = TimeSpan.Zero;
            Data.MTConnect_data.MachineCycleTime = TimeSpan.Zero;
            Data.MTConnect_data.baseQuantity = 0;
            // for display in the sql table where the dashboard was updated
            if (LogMode)
            {
                Data = Export_To_SQL(Data, series, workcenter, false);

            }

            return Data;
        }
     //! write to the sql serverautomation

     //! Read From the sql server
        public struct SQL_Reader
        {
            public string rawData;
        }
     //! Read From the sql server

        //! data Storage
        public static Watchlist RecoverPastData(Watchlist dashboard, string workcenter, string datatype)
        {
            string rap1 = "100";
            string rap2 = "100";
            string feed1 = "100";
            string feed2 = "100";
            string spindle1 = "100";
            string spindle2 = "100";
            string gantry = "100";

            // grab data from sql table
            var cs = @"Server=DB-USMN-001\IN01;Database=ProductionMonitor; User ID=s4automation;Password=s4automation";
            using var con = new SqlConnection(cs);
            try
            {
                con.Open();

                string sql = "SELECT [WorkCenter], [WCTimeDuration], [KioskState], " +
                    "[MTC_State_H1], [MTC_Duration_H1], [MTC_PC_H1],"+
                    "[MTC_State_H2], [MTC_Duration_H2], [MTC_PC_H2]," +
                    "[Base_Quantity], [SAP_Part_Quantity], [Cycletime], [SAP_Cycletime], [Loadtime], [SAP_Loadtime]," +
                    "[SetupTime], [SAP_Setuptime], [SAP_MachineCycletime], [Kiosk_CN], [Kiosk_OP]," +
                    "[ProductionOrder], [Material], [Operator], [ProdSupervisor], " +
                    "[MT_H1Rapid], [MT_H1Feed], [MT_H1Spindle], [MT_H2Rapid], [MT_H2Feed], [MT_H2Spindle], [MT_Gantry]," +
                    "[Prev_WC_State], [Prev_WC_Time], [Prev_StateTime_H1], [Prev_StateTime_H2], [Prev_StateOffset_H1], [Prev_StateOffset_H2], [Prev_KioskState]," +
                    "[Starttime], [Pred_JobEndtime],[Pred_SetupEnd]" +
                    "FROM [ProductionMonitor].[dbo].[Dashboard_ProdMonitor] WHERE WorkCenter = "+ workcenter +"; ";

                using var cmd = new SqlCommand(sql, con);
                using SqlDataReader rdr = cmd.ExecuteReader();

                while (rdr.Read())
                {
                    //workcenter = rdr.GetString(0);
                    dashboard.TimeInWorkCenterState = TimeSpan.FromSeconds(rdr.GetInt64(1));
                    dashboard.KioskState = (rdr.GetString(2)).TrimEnd(' ');

                    dashboard.MachineState = rdr.GetString(3).TrimEnd(' ');
                    if (dashboard.MachineState.Contains("RUNNING"))
                    {
                        dashboard.MachineState = "RUNNING";
                    }
                    else if (dashboard.MachineState.Contains("IDLE"))
                    {
                        dashboard.MachineState = "IDLE";
                    }
                    dashboard.StateSet = dashboard.MachineState;
                    dashboard.TimeInState = TimeSpan.FromSeconds(rdr.GetInt64(4));
                    dashboard.PartCount = rdr.GetString(5);

                    dashboard.MachineState2 = rdr.GetString(6).TrimEnd(' ');
                    if (dashboard.MachineState2.Contains("RUNNING"))
                    {
                        dashboard.MachineState2 = "RUNNING";
                    }
                    else if (dashboard.MachineState2.Contains("IDLE"))
                    {
                        dashboard.MachineState2 = "IDLE";
                    }
                    dashboard.StateSet2 = dashboard.MachineState2;
                    dashboard.TimeInState2 = TimeSpan.FromSeconds(rdr.GetInt64(7));
                    dashboard.PartCount2 = rdr.GetString(8);

                    dashboard.baseQuantity = rdr.GetInt32(9);
                    dashboard.PartCountTotal = rdr.GetString(10).TrimEnd(' ');
                    
                    dashboard.ActualCycletime =  TimeSpan.FromSeconds(rdr.GetInt64(11));
                    dashboard.Cycletime = dashboard.ActualCycletime;
                    dashboard.Cycletime2 = dashboard.ActualCycletime;
                    
                    dashboard.idealCycletime = TimeSpan.FromSeconds(rdr.GetInt64(12));
                    dashboard.Loadtime = TimeSpan.FromSeconds(rdr.GetInt64(13));
                    dashboard.idealLoadTime = TimeSpan.FromSeconds(rdr.GetInt64(14));

                    dashboard.ActualSetupTime = TimeSpan.FromSeconds(rdr.GetInt64(15));
                    dashboard.setuptime = TimeSpan.FromSeconds(rdr.GetInt64(16));
                    dashboard.MachineCycleTime = TimeSpan.FromSeconds(rdr.GetInt64(17));
                    dashboard.Conf_Numb = rdr.GetString(18).TrimEnd(' ');
                    dashboard.prev_Conf_Numb = dashboard.Conf_Numb;
                    dashboard.Operation_Number = rdr.GetString(19).TrimEnd(' ');

                    dashboard.Prod_Order = rdr.GetString(20).TrimEnd(' ');
                    dashboard.material = rdr.GetString(21).TrimEnd(' ');
                    dashboard.OP_ID = rdr.GetString(22).TrimEnd(' ');
                    dashboard.SUP_ID = rdr.GetString(23).TrimEnd(' ');

                    rap1 = rdr.GetString(24).TrimEnd(' ');
                    feed1 = rdr.GetString(25).TrimEnd(' ');
                    spindle1 = rdr.GetString(26).TrimEnd(' ');
                    rap2 = rdr.GetString(27).TrimEnd(' ');
                    feed2 = rdr.GetString(28).TrimEnd(' ');
                    spindle2 = rdr.GetString(29).TrimEnd(' ');
                    gantry = rdr.GetString(30).TrimEnd(' ');

                    dashboard.prevThyHOLYDashboardState = rdr.GetString(31).TrimEnd(' ');

                    dashboard.PrevWorkCenterTime = DateTime.Parse(rdr.GetString(32));
                    dashboard.PrevStatetime = DateTime.Parse(rdr.GetString(33));
                    dashboard.PrevStatetime2 = DateTime.Parse(rdr.GetString(34));
                    dashboard.MachineTimeOffset = TimeSpan.Parse(rdr.GetString(35));
                    dashboard.MachineTimeOffset2 = TimeSpan.Parse(rdr.GetString(36));

                    dashboard.PrevKioskState = (rdr.GetString(37)).TrimEnd(' '); ;
                    // prediction values
                    dashboard.JobStarttime = DateTime.Parse(rdr.GetString(38));
                    dashboard.PredictedEndtime = DateTime.Parse(rdr.GetString(39));
                    dashboard.EndSetuptime = DateTime.Parse(rdr.GetString(40));
                }

            }
            catch(Exception e) 
            {
                Console.WriteLine(e.Message.ToString());
            }
            finally
            {
                con.Close();
            }

            // data format correcting
            if(dashboard.PartCount2 == "")
            {
                dashboard.PartCount2 = "N/A";
            }

            // fix up main state
            dashboard.ThyHOLYDashboardState = PLC_Client.MachineStateFinder(dashboard.MachineState, dashboard.MachineState2, dashboard.KioskState, dashboard.prevThyHOLYDashboardState);
            dashboard.prevThyHOLYDashboardState = dashboard.ThyHOLYDashboardState;

            // fix up overrides
            switch (datatype)
            {
                case "smooth":
                    dashboard.Feed= feed1;
                    dashboard.Feed2= feed2;
                    dashboard.Rapid = rap1;
                    dashboard.Rapid2= rap2;
                    dashboard.Spindle= spindle1;
                    dashboard.Spindle2= spindle2;
                    dashboard.Gantry= gantry;
                    break;
                case "matrix2":
                    dashboard.Feed= feed1;
                    dashboard.Feed2= feed2;
                    dashboard.Rapid = rap1;
                    dashboard.Rapid2= rap2;
                    dashboard.Spindle= spindle1;
                    dashboard.Spindle2= spindle2;
                    break;
                case "nexus":
                    dashboard.Feed= feed1;
                    dashboard.Rapid = rap1;
                    dashboard.Spindle= spindle1;
                    break;
                case "TWOHEANANDGANTRY":
                    dashboard.Feed= feed1;
                    dashboard.Feed2= feed2;
                    dashboard.Rapid = rap1;
                    dashboard.Rapid2= rap2;
                    dashboard.Spindle= spindle1;
                    dashboard.Spindle2= spindle2;
                    dashboard.Gantry= gantry;
                    break;
                case "TWOHEAD":
                    dashboard.Feed= feed1;
                    dashboard.Feed2= feed2;
                    dashboard.Rapid = rap1;
                    dashboard.Rapid2= rap2;
                    dashboard.Spindle= spindle1;
                    dashboard.Spindle2= spindle2;
                    break;
                case "ONEHEAD":
                    dashboard.Feed= feed1;
                    dashboard.Rapid = rap1;
                    dashboard.Spindle= spindle1;
                    break;
                default:

                    break;
            }


            return dashboard;
        }

        private static void SaveData(SQL_Logger Data, MSR1.Watchlist DashboardData)
        {
            if (DashboardData.MachineState2.Contains("N/A"))
            {
                DashboardData.MachineTime2 = DateTime.Now;
                DashboardData.TimeInState2 = TimeSpan.Zero;
                DashboardData.PrevStatetime2 = DateTime.Now;
                DashboardData.MachineTimeOffset2 = TimeSpan.Zero;
            }

            var cs = @"Server=DB-USMN-001\IN01;Database=ProductionMonitor; User ID=s4automation;Password=s4automation";
            var con = new SqlConnection(cs);
            try
            {
                con.Open();
                    
                var query = "" +
                    "UPDATE [ProductionMonitor].[dbo].[Dashboard_ProdMonitor]" +
                    "SET WCTimeDuration = " + Math.Round(DashboardData.TimeInWorkCenterState.TotalSeconds) +
                    ", KioskState = '"+ DashboardData.KioskState + "'" +
                    ", MTC_State_H1 = '" + DashboardData.MachineState + "'" +
                    ", MTC_Duration_H1 = " + Math.Round(DashboardData.TimeInState.TotalSeconds) +
                    ", MTC_PC_H1 = '" + DashboardData.PartCount + "'" + 
                    ", MTC_State_H2 = '" + DashboardData.MachineState2 + "'" +
                    ", MTC_Duration_H2 = " + Math.Round(DashboardData.TimeInState2.TotalSeconds) +
                    ", MTC_PC_H2 = '" + DashboardData.PartCount2 + "'" +

                    ", Base_Quantity = " + DashboardData.baseQuantity + 
                    ", SAP_Part_Quantity = '" + DashboardData.PartCountTotal + "'" +

                    ", Cycletime = " + DashboardData.ActualCycletime.TotalSeconds +
                    ", SAP_Cycletime = " + Math.Round(DashboardData.idealCycletime.TotalSeconds) +
                    ", Loadtime = " + Math.Round(DashboardData.Loadtime.TotalSeconds) +
                    ", SAP_Loadtime = " + Math.Round(DashboardData.idealLoadTime.TotalSeconds) +
                    ", SetupTime =" + Math.Round(DashboardData.ActualSetupTime.TotalSeconds) +
                    ", SAP_SetupTime =" + Math.Round(DashboardData.setuptime.TotalSeconds) +

                    ", SAP_MachineCycletime = " + Math.Round(DashboardData.MachineCycleTime.TotalSeconds) +
                    ", Kiosk_CN = '" + DashboardData.Conf_Numb + "'" +
                    ", Kiosk_OP = '" + DashboardData.Operation_Number + "'" +
                    ", ProductionOrder = '" + DashboardData.Prod_Order + "'" +
                    ", Material = '" + DashboardData.material + "'" +
                    ", Operator = '" + DashboardData.OP_ID + "'" +
                    ", ProdSupervisor = '" + DashboardData.SUP_ID + "'" +

                    ", MT_H1Rapid = '" + DashboardData.Rapid + "'"  +
                    ", MT_H1Feed  = '" + DashboardData.Feed + "'"  +
                    ", MT_H1Spindle  = '" + DashboardData.Spindle + "'"   +
                    ", MT_H2Rapid = '" + DashboardData.Rapid2 + "'"  +
                    ", MT_H2Feed  = '" + DashboardData.Feed2 + "'"  +
                    ", MT_H2Spindle  = '" + DashboardData.Spindle2 + "'"   +
                    ", MT_Gantry  = '" + DashboardData.Gantry+ "'"   +

                    ", Prev_WC_State  = '" + DashboardData.ThyHOLYDashboardState + "'"   +
                    ", Prev_WC_Time  = '" + DashboardData.PrevWorkCenterTime + "'"   +
                    ", Prev_StateTime_H1  = '" + DashboardData.PrevStatetime + "'"   +
                    ", Prev_StateTime_H2  = '" + DashboardData.PrevStatetime2 + "'"   +
                    ", Prev_StateOffset_H1  = '" + DashboardData.MachineTimeOffset + "'"   +
                    ", Prev_StateOffset_H2  = '" + DashboardData.MachineTimeOffset2 + "'"   +
                    ", Starttime = '" + DashboardData.JobStarttime + "'" +
                    ", Pred_JobEndtime = '" + DashboardData.PredictedEndtime + "'" +
                    ", Pred_SetupEnd = '" + DashboardData.EndSetuptime + "'" +
                    // ", Starttime = " + " " +

                    "WHERE WorkCenter = "+ Data.WorkCenter +"; ";

                using var cmd = new SqlCommand(query, con);
                cmd.Prepare();
                cmd.ExecuteNonQuery();
            }
            catch(Exception e)
            {
                /*
                Console.WriteLine(Data.WorkCenter + ": " + e.Message.ToString());
                Console.WriteLine("WCTimeDuration: " + DashboardData.TimeInWorkCenterState.TotalSeconds);
                Console.WriteLine("KioskState: " + DashboardData.KioskState.Length);
                Console.WriteLine("MTC_State_H1: " + DashboardData.MachineState.Length);
                Console.WriteLine("MTC_Duration_H1: " + DashboardData.TimeInState.TotalSeconds);
                Console.WriteLine("MTC_PC_H1: " + DashboardData.PartCount.Length);
                Console.WriteLine("MTC_State_H2: " + DashboardData.MachineState2.Length);
                Console.WriteLine("MTC_Duration_H2: " + DashboardData.TimeInState2.TotalSeconds);
                Console.WriteLine("MTC_PC_H2: " + DashboardData.PartCount2.Length);
                */
            }
            finally
            {
                con.Close();
            }
            //! data Storage
        }
    }

    public static class Commenting
    {
        // involves reading the machine sql data
        public static string Find_LinkAddress(string series, string tag)
        {
            string ID = "";
            // grab data from sql table
            var cs = @"Server=DB-USMN-001\IN01;Database=ProductionMonitor; User ID=s4automation;Password=s4automation";
            using var con = new SqlConnection(cs);
            try
            {
                con.Open();

                string query = "SELECT MAX(Id) FROM [ProductionMonitor].[dbo].["+series+tag+"_ProdMonitor]";
                using var cmd = new SqlCommand(query, con);

                SqlDataReader rdr = cmd.ExecuteReader();

                while (rdr.Read())
                {
                    ID = (rdr.GetInt64(0)+1).ToString();
                }

            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            Console.WriteLine(tag+ "_" + ID);

            return tag+ "_" + ID;
        }

        // insert a row into the comment table
        public static void SQL_InsertAComment(string LinkAddress, string WorkCenter, string writer, string comment)
        {
            var cs = @"Server=DB-USMN-001\IN01;Database=ProductionMonitor; User ID=s4automation;Password=s4automation";
            var con = new SqlConnection(cs);

            // check to see if the link address already exists
            bool LinkAddressExists = false;
            var con2 = new SqlConnection(cs);
            try 
            {
                con2.Open();
                string AddressSearch = "SELECT [ID]" +
                  "FROM [ProductionMonitor].[dbo].[ProdMonitor_Comments]" +
                  "WHERE EXISTS " +
                  "(SELECT LinkAddress FROM  [ProductionMonitor].[dbo].[ProdMonitor_Comments] WHERE LinkAddress = '"+ LinkAddress + "');";
                var cmd2 = new SqlCommand(AddressSearch, con2);
                SqlDataReader rdr2 = cmd2.ExecuteReader();
                while (rdr2.Read())
                {
                    int value = rdr2.GetInt32(0);
                    if (value>0) // link address already exists
                    {
                        LinkAddressExists = true;
                    }
                    else
                    {
                        LinkAddressExists = false; 
                    }
                }
                    
                rdr2.Close();
            }
            finally
            {
                con2.Close();
            }
            if (LinkAddressExists == false) // if the link address exists we can confirm this is a new comment sent by an opertator
            {
                // send a comment 
                try
                {
                    con.Open();

                    var query = "INSERT INTO [ProductionMonitor].[dbo].[ProdMonitor_Comments]" +
                        "(LinkAddress, WorkCenter, ServerTimeCommented, Writer, Operator_Comment,Comment)" +
                        "VALUES(@linkaddress, @workcenter, @servertimecommented, @writer, @operator_comment,@comment)";

                    var cmd = new SqlCommand(query, con);

                    cmd.Parameters.Add(new SqlParameter("@linkaddress", SqlDbType.NVarChar, 64)).Value = LinkAddress;
                    cmd.Parameters.Add(new SqlParameter("@workcenter", SqlDbType.Char, 5)).Value = WorkCenter;
                    cmd.Parameters.Add("@servertimecommented", SqlDbType.DateTime).Value = DateTime.Now;

                    cmd.Parameters.Add(new SqlParameter("@writer", SqlDbType.NVarChar, 64)).Value = writer;
                    cmd.Parameters.Add(new SqlParameter("@operator_comment", SqlDbType.NVarChar, 256)).Value = comment; // the comment made by the operator from the kiosk
                    cmd.Parameters.Add(new SqlParameter("@comment", SqlDbType.NVarChar, 512)).Value = ""; // this will be the comment from the manager over KEBOT //comment;

                    cmd.Prepare();
                    cmd.ExecuteNonQuery();
                }
                finally
                {
                    con.Close();
                }
            }
        }

    }
}

