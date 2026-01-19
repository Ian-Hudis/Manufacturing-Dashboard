using Org.BouncyCastle.Asn1.Cms;
using System;
using System.Diagnostics;
using System.Net;
using System.Xml;
using System.Xml.Serialization;

// Discontinued as of 9/22/2025

namespace MTConnectDashboard
{
    
    public class Httpget // query thread
    {

        public struct HttpData
        {
            public string timestamp;
            public string dataItemId;
            public string value;
            public string tag;
            public long seq;

        }

        //sequance filtering
        public static long[] SequenceData = new long[DataOutput.sequenceshift+1];
        public static long[] PastSequenceData = new long[DataOutput.sequenceshift+1];
        private static bool newvalue = new();
        private static int i = 0;
        private static long repeatdata = new();


        //public string type;
        private static long Sequancestart = 0;
        public static int OutputRow = 0;
        /*
        public static HttpData[] HttpGet(string MachAddress, string URLPath, int DataLimit, bool start, string machtype)
        {
            HttpData[] output = new HttpData[DataLimit+1];
            string URLaddress;

            //URLaddress = "http://" + MachAddress + "/sample?from" + Sequancestart +"&count=" +  DataOutput.sequenceshift
            //                     + "&path=//MTConnectDevices/Devices/Device/Components/Controller";

            // URLaddress = "http://" + MachAddress + "/current?" + "&path=//MTConnectDevices/Devices/Device/Components/Controller";

            switch(machtype)
            {
                case "direct": // this is for when I make the mtconnect adapter and agent myself and can set it up exactly how it is needed.

                    URLaddress = "http://" + MachAddress;
                    
                    break;
                case "amada": // really its anything using bandpi no just amada

                    URLaddress = "http://" + MachAddress + "/current.xml";

                    break;
                case "haas":

                       URLaddress = "http://" + MachAddress + "/current?" + URLPath;
                  
                    break;
                case "doosan":
                    
                       URLaddress = "http://" + MachAddress + "/current?" + URLPath;
                   // URLaddress ="http://" + MachAddress + "/sample?from" + Sequancestart +"&count=" +  DataOutput.sequenceshift + URLPath;

                    break;
                case "bdtronic":
                        URLaddress = "http://" + MachAddress + "/current";
                    break;
                default: // default is the mazak
                    if (start)
                    {  
                        URLaddress = "http://" + MachAddress + "/current?" + URLPath;
                    }
                    else
                    {
                        URLaddress = "http://" + MachAddress + "/current?" + URLPath;
                        //URLaddress ="http://" + MachAddress + "/sample?from" + Sequancestart +"&count=" +  DataOutput.sequenceshift + URLPath;
                    }
                    break;
            }
            XmlTextReader reader;
            try
            {
                ConFail.MachineOffline = false;
                reader = new(URLaddress);


                while (reader.Read()) { 

                    MachineParseData OutputData = Parsing(reader); // grab a line in the query
                    
                    if (PastSequenceData.Contains(OutputData.sequence2) || OutputData.sequence2==0)
                    {
                        newvalue=false; // the sequence was previously issued
                    }
                    else
                    {
                        newvalue=true; // the sequence is new

                        SequenceData[i] = OutputData.sequence2; // add the sequence value to the index
                        i++;
                        if (i>=DataOutput.sequenceshift)
                        {
                            i=0;
                        }

                    }

                    if (OutputData.sequence2 != repeatdata && newvalue)
                    {
                        //  Console.WriteLine(OutputData.sequence2 + " | " + OutputData.timestamp2 + " | " + OutputData.tag2 + " | "
                        //  + OutputData.dataItemId2 + " | " + OutputData.value);

                        //! Output the data
                        if (OutputRow < DataLimit)   // build the table
                        {

                            output[OutputRow].timestamp = OutputData.timestamp2;
                            output[OutputRow].dataItemId = OutputData.dataItemId2;
                            output[OutputRow].value = OutputData.value;
                            output[OutputRow].tag = OutputData.tag2;
                            output[OutputRow].seq = OutputData.sequence2;

                            OutputRow++;
                        }
                        else  // table reaches max size and starts shifting so the new entries replace the older entries
                        {
                            for (int n = 0; n<DataLimit-1; n++)  // shift the array to the left
                            {
                                output[n]= output[n+1];
                            }

                            //! Output the data into the last row
                            output[DataLimit-1].timestamp = OutputData.timestamp2;
                            output[DataLimit-1].dataItemId = OutputData.dataItemId2;
                            output[DataLimit-1].value = OutputData.value;
                            output[DataLimit-1].tag = OutputData.tag2;
                            output[DataLimit-1].seq = OutputData.sequence2;
                            //! Output the data

                        }

                    }
                    repeatdata = OutputData.sequence2;
                    // Thread.Sleep(delay1);
                }
            }
            catch (Exception ex)
            {
                // Console.WriteLine("\n"+ URLaddress+ " is not streaming. \n"+ ex);
                ConFail.MachineOffline = true;
                ConFail.Prob = ex.Message;
            }

            PastSequenceData = SequenceData; // replace the sequance data profile

            return output;
        }
        */

        public struct MachFail
        {
            public bool MachineOffline;
            public string Prob;
        }
        public static MachFail ConFail = new();


        //! Parse each xml entry
        private struct MachineParseData
        {
            public string tag1;
            public string tag2;
            public string dataItemId1;
            public string dataItemId2;
            public string timestamp1;
            public string timestamp2;
            public long sequence1;
            public long sequence2;
            public string type1;
            public string type2;
            public string value;
            // public long repeatdata;
            // public long sequancestart;
            // public int OutputRow;
        }
        /*
        private static MachineParseData Rift = new()
        {
            tag1="",
            dataItemId1="",
            timestamp1="",
            sequence1=1,
            type1="",
            value="",
            tag2="",
            dataItemId2="",
            timestamp2="",
            sequence2=1,
            type2=""
        };
        static MachineParseData Parsing(XmlTextReader reader)// function that grabs the xml
        {
            //read the xml entry
            switch (reader.NodeType)
            {
                case XmlNodeType.Element: // The node is an element.
                                          // Console.Write("<" );//+ reader.Name);
                    Rift.tag1 = reader.Name;
                    while (reader.MoveToNextAttribute())
                    { // Read the attributes.

                        //   Console.Write(" " + reader.Name + "='" + reader.Value + "'");
                        //  Console.Write("|");
                        Rift.type1 ="Null";


                        if (reader.Name=="lastSequence")
                        {
                            Sequancestart= long.Parse(reader.Value) - DataOutput.sequenceshift;
                            //  Console.WriteLine(Sequancestart + " to " + reader.Value);
                        }


                        if (reader.Name=="dataItemId")
                        {
                            Rift.dataItemId1 = reader.Value;
                        }
                        else if (reader.Name=="timestamp")
                        {
                            Rift.timestamp1 = reader.Value;
                        }
                        else if (reader.Name=="sequence")
                        {
                            try
                            {
                                long seq = long.Parse(reader.Value);
                                Rift.sequence1 = seq;
                            }
                            catch (FormatException)
                            {
                                Console.WriteLine($"Unable to parse last sequance");
                            }

                        }
                        else if (reader.Name=="subType") // not every entry has a subtype
                        {
                            Rift.type1 = reader.Value;
                        }
                    }
                    break;
                case XmlNodeType.Text: //Display the text in each element.
                                       //Console.WriteLine(" Value = " + reader.Value + ">");
                    Rift.value = reader.Value;
                    // update the type 2s with type 1s so the rift value will be with the right data
                    Rift.timestamp2 = Rift.timestamp1;
                    Rift.tag2 = Rift.tag1;
                    Rift.sequence2=Rift.sequence1;
                    Rift.dataItemId2 = Rift.dataItemId1;

                    break;
                case XmlNodeType.EndElement: //Display the end of the element.
                                             // Console.Write("</" + reader.Name);
                                             // Console.WriteLine(">");  
                    break;
            }

            return Rift;
        }
        //! Parse each xml entry
        */
    }
    
}