using System.Data;

namespace MTConnectDashboard
{

    public class DataExtraction
    {
        public long Sequence { get; set; }

        public string? TimeStamp { get; set; }

        public string? ID { get; set; }

        public string? Value { get; set; }

        public string? Tag { get; set; }
    }

    
    public class DeviceData
    {
        public Task<DataExtraction[]> GetDataAsync(int row, DataOutput.DeviceInter Mach)
        {

             return Task.FromResult(
             Enumerable.Range(1, 1).Select(index => new DataExtraction
             {      
                 /*
                 Sequence = Mach.Output[row].seq,
                 TimeStamp = Mach.Output[row].timestamp,
                 ID = Mach.Output[row].dataItemId,
                 Value = Mach.Output[row].value,
                 Tag = Mach.Output[row].tag
                 */
             }).ToArray());
        }

    }
      


}
