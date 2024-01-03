using ConsoleApp1;
using Riss.Devices;

public class DeviceComEty
{
    public DeviceConnection DeviceConnection { get; set; }

    public Device Device { get; set; }
}

public class DownloadDataViewModel
{
    public int SN { get; set; }
    public string Type { get; set; }
    public string mode { get; set; }
    public string DN { get; set; }
    public string DIN { get; set; }
    public string Clock { get; set; }
}

public class Global
{
    private Global() { }

    /// <summary>
    /// Busy flag, this flag is set to 1 whenever the device is busy doing other process
    /// </summary>
    public const long DeviceBusy = 1;

    /// <summary>
    /// Idle flag, this flag is set to 1 whenever the device is in idle state
    /// </summary>
    public const long DeviceIdle = 0;
}

public class DeviceUtil
{
    public Device device;
    public DeviceConnection deviceConnection;
    public DeviceComEty deviceEty;
    public List<DownloadDataViewModel> ListDownloadData { get; set; } = new List<DownloadDataViewModel>();

    public String ConnectToDevice()
    {
        try
        {

            device = new Device
            {
                DN = 1,
                Model = "ZDC2911",
                ConnectionModel = 1,

                IpAddress = "192.168.1.7",
                IpPort = 5005,
                CommunicationType = CommunicationType.Tcp
            };

            deviceConnection = DeviceConnection.CreateConnection(ref device);
            System.Diagnostics.Debug.WriteLine(deviceConnection.Open());

            if (deviceConnection.Open() > 0)
            {
                deviceEty = new DeviceComEty
                {
                    Device = device,
                    DeviceConnection = deviceConnection
                };

                return "Connected";
            }
            else
            {
                return "Connection Failed";
            }
        }
        catch (Exception ex)
        {
            throw;
        }
    }

    public void DownloadData()
    {
        device = new Device
        {
            DN = 1,
            Model = "ZDC2911",
            ConnectionModel = 1,
            IpAddress = "192.168.1.7",
            IpPort = 5005,
            CommunicationType = CommunicationType.Tcp,
            Password = "0"
        };
        deviceConnection = DeviceConnection.CreateConnection(ref device);
        System.Diagnostics.Debug.WriteLine(deviceConnection.Open());
        deviceEty = new DeviceComEty
        {
            Device = device,
            DeviceConnection = deviceConnection
        };

        object extraProperty = new object();
        object extraData = new object();
        extraData = Global.DeviceBusy;
        try
        {
            List<DateTime> dtList = new List<DateTime>();
            bool result = deviceConnection.SetProperty(DeviceProperty.Enable, extraProperty, device, extraData);

            dtList.Add(Convert.ToDateTime("01/01/1991"));
            dtList.Add(DateTime.Now);

            extraProperty = false;
            extraData = dtList;
            result = deviceConnection.GetProperty(DeviceProperty.AttRecordsCount, extraProperty, ref device,
                ref extraData);
            if (false == result)
            {
                Console.WriteLine("Download failed");
            }

            int recordCount = (int)BitConverter.ToInt64((byte[])extraData, 0);
            Console.WriteLine($"Record count: {recordCount}");
            if (0 == recordCount)
            {//When 0, it means there are no new log records.
                ListDownloadData.Clear();
            }

            List<bool> boolList = new List<bool>
            {
                // true to clean after retrieval
                false,
                false
            };

            extraProperty = false;  // boolList ||  set boolean value if not works;
            extraData = (object)dtList;
            result = deviceConnection.GetProperty(DeviceProperty.AttRecords, extraProperty, ref device, ref extraData);
            if (result)
            {
                int i = 1;
                int y = 0;
                List<Record> recordList = (List<Record>)extraData;
                ListDownloadData.Clear();
                foreach (Record record in recordList)
                {
                    ListDownloadData.Add(new DownloadDataViewModel
                    {
                        SN = i,
                        DN = record.DN.ToString(),
                        DIN = record.DIN.ToString(),
                        Type = ConvertObject.GLogType(record.Verify),
                        mode = ConvertObject.IOMode(record.Action),
                        Clock = record.Clock.ToString("yyyy-MM-dd HH:mm:ss")
                    });
                    i++;
                }
            }
            else
            {
                Console.WriteLine("Download failed");
            }
        }
        catch (Exception)
        {
            Console.WriteLine("Download failed");
        }

        Console.WriteLine(ListDownloadData.Count);
    }
}
class Program
{
    static void Main()
    {
        var dev = new DeviceUtil();
        //bool isConnected = dev.ConnectToDevice() == "Connected";
        //Console.WriteLine("Is Connected {0}", isConnected);
        //if (isConnected)
        //{
        dev.DownloadData();
        //}
    }
}
