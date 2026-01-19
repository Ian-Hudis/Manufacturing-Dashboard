using System;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Text;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using static MTConnectDashboard.PLCserver;
using Microsoft.AspNetCore.Server.IIS.Core;

namespace MTConnectDashboard
{

    public class PLC_Thread
    {
        // State information used in the task.
        private PLC_Data plc_data;
        // The constructor obtains the state information.
        public PLC_Thread(PLC_Data Input)
        {
            plc_data = Input;
        }

        // The thread procedure performs the task
        public void PLC_Listen()
        {
            // add plc ip addresses
            TcpClient tcpClient = plc_data.RAWdata.tcpListener.AcceptTcpClient();
            NetworkStream stream = tcpClient.GetStream();
            // StreamReader sr = new StreamReader(client.GetStream());
            StreamWriter sw = new (tcpClient.GetStream());

            string PlumbMessage;

            try
            {
                
                byte[] buffer = new byte[2048];
                stream.Read(buffer, 0, buffer.Length);
                int recv = 0;
                foreach (byte b in buffer)
                {
                    if (b!=0)
                    {
                        recv++;
                    }
                }
                string request = Encoding.UTF8.GetString(buffer, 1, recv);
                #pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
                string subs = Convert.ToString(tcpClient.Client.RemoteEndPoint);
                #pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.
                #pragma warning disable CS8602 // Dereference of a possibly null reference.
                string[] s1 = subs.Split(':');
#               pragma warning restore CS8602 // Dereference of a possibly null reference.
                string ID = s1[0];

                string[] Plumb = request.Split('>');
                PlumbMessage = Plumb[0];

                if (plc_data.RAWdata.IPuserList.Contains(ID))
                {
                    plc_data.RAWdata.Raw_Line_ID = ID;
                    plc_data.RAWdata.Raw_Line_Message = PlumbMessage;
                    Console.WriteLine(ID + ": " + PlumbMessage); //for debug
                    // Console.WriteLine(PlumbMessage);
                }
                else
                {
                    Console.WriteLine("Unrecognized Entity Detected.");
                }
                
                sw.Flush(); // clears buffer
            }
            catch (Exception ex)
            {
                Console.WriteLine("PLCserver.Listen: Something went wrong.");
                Console.WriteLine(ex.ToString());
            }

        }

    }


    public class PLCserver // tcp server for getting data input from HMI-PLC Combnivis things
    {
        public struct PLCrawInput
        {
            public List<string> IPuserList;

            public TcpListener tcpListener;

            public string Raw_Line_ID;
            public string Raw_Line_Message;

            public string[] data;
        }       
        public struct PLC_Data
        {
            public PLCrawInput RAWdata;

            public string Supervisor;
            public string Operator;
            public string ProductionOrder;
            public string Operation;
            public string Material;
            public string Status;
            public string Sevent;
        }

        public static PLC_Data InitializePLC(PLC_Data Data, int Port, string BlackBox_Ip)
        {
            Data.RAWdata.IPuserList = new List<string>();
            Data.RAWdata.IPuserList.Add(BlackBox_Ip); // add plc ip addresses
            //IPuserList.Add(MyTestPLC); // my plc
            Data.RAWdata.tcpListener = new TcpListener(IPAddress.Any, Port); // port is 4545 for 2282
            Data.RAWdata.tcpListener.Start();
            return Data;
        }

        public static PLC_Data BlackBoxRead(PLC_Data plc_data, string BlackBox_IP)
        {
            PLC_Thread plcThreaddata = new(plc_data); // put the plc data into the thread

            Thread listener = new Thread(new ThreadStart(plcThreaddata.PLC_Listen));
            listener.Start();
            
            if (BlackBox_IP == plc_data.RAWdata.Raw_Line_ID) // see if plc address from the message listened  to matches the ip adresss of the function
             {
                    plc_data.RAWdata.data =  plc_data.RAWdata.Raw_Line_Message.Split(":");
                    // detect when a variable changes
                    
                    if (plc_data.Supervisor != plc_data.RAWdata.data[0] || plc_data.Operator != plc_data.RAWdata.data[1] || plc_data.ProductionOrder != plc_data.RAWdata.data[2] ||
                        plc_data.Operation != plc_data.RAWdata.data[3] || plc_data.Material != plc_data.RAWdata.data[4] || plc_data.Status != plc_data.RAWdata.data[5] || 
                        plc_data.Sevent != plc_data.RAWdata.data[6])
                    {
                        plc_data.Supervisor = plc_data.RAWdata.data[0];
                        plc_data.Operator = plc_data.RAWdata.data[1];
                        plc_data.ProductionOrder = plc_data.RAWdata.data[2];
                        plc_data.Operation = plc_data.RAWdata.data[3];
                        plc_data.Material = plc_data.RAWdata.data[4];
                        plc_data.Status = plc_data.RAWdata.data[5];
                        plc_data.Sevent = plc_data.RAWdata.data[6];
                        //Console.WriteLine("Blackbox " + plcIP + " Updated");
                    }
             }
             
            return plc_data;
        }  // for grabbing plc data

    }
}