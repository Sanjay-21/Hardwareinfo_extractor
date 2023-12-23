using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using LibreHardwareMonitor;
using LibreHardwareMonitor.Hardware;

public class UpdateVisitor : IVisitor
{
    public void VisitComputer(IComputer computer)
    {
        computer.Traverse(this);
    }

    public void VisitHardware(IHardware hardware)
    {
        hardware.Update();
        foreach (IHardware subHardware in hardware.SubHardware) subHardware.Accept(this);
    }

    public void VisitSensor(ISensor sensor) { }
    public void VisitParameter(IParameter parameter) { }
}

class Program
{
    static async Task Main(string[] args)
    {
        Computer computer = new Computer
        {
            IsCpuEnabled = true,
            IsGpuEnabled = true,
            IsMemoryEnabled = true,
            IsMotherboardEnabled = true,
            IsControllerEnabled = true,
            IsNetworkEnabled = true,
            IsStorageEnabled = true
        };

        computer.Open();
        computer.Accept(new UpdateVisitor());
        float? temp = 0;
        float cpu_temp=0, cpu_power = 0, cpu_volt = 0, gpu_temp = 0, gpu_power = 0;
        string cpu="", gpu="";
        for(int i=0;i<50;i++)
        {
            foreach (IHardware hardware in computer.Hardware)
            {
                if (hardware.HardwareType == HardwareType.Cpu) // Use "CPU" instead of "Cpu"
                {
                    computer.Accept(new UpdateVisitor());
                    Console.WriteLine("Main Hardware: {0}", hardware.Name);
                    cpu = hardware.Name;
                    Console.WriteLine("\t\tSensor: {0}, value: {1}", hardware.Sensors[31].Name, hardware.Sensors[31].Value);
                    temp = hardware.Sensors[31].Value;
                    cpu_temp = (float)Math.Round(temp.Value,2);
                    //cpu_temp = Math.Round(cpu_temp * 100f) / 100f;
                    Console.WriteLine("\t\tSensor: {0}, value: {1}", hardware.Sensors[40].Name, hardware.Sensors[40].Value);
                    temp = hardware.Sensors[40].Value;
                    cpu_power = (float)Math.Round(temp.Value, 2);
                    Console.WriteLine("\t\tSensor: {0}, value: {1}", hardware.Sensors[43].Name, hardware.Sensors[43].Value);
                    temp = hardware.Sensors[43].Value;
                    cpu_volt = (float)Math.Round(temp.Value, 2);
                }
                if (hardware.HardwareType == HardwareType.GpuNvidia) // Use "CPU" instead of "Cpu"
                {
                    computer.Accept(new UpdateVisitor());
                    Console.WriteLine("Main Hardware: {0}", hardware.Name);
                    gpu = hardware.Name;
                    Console.WriteLine("\t\tSensor: {0}, value: {1}", hardware.Sensors[0].Name, hardware.Sensors[0].Value);
                    temp = hardware.Sensors[0].Value;
                    gpu_temp = (float)Math.Round(temp.Value, 2);
                    Console.WriteLine("\t\tSensor: {0}, value: {1}", hardware.Sensors[23].Name, hardware.Sensors[23].Value);
                    temp = hardware.Sensors[23].Value;
                    gpu_power = (float)Math.Round(temp.Value, 2);
                }
            }
            Console.WriteLine("Test {0},{1},{2},{3},{4}", cpu_temp, cpu_power, cpu_volt, gpu_temp, gpu_power);
            await SendDataToThingSpeak(cpu_temp, cpu_power, cpu_volt, gpu_temp, gpu_power,cpu,gpu);
            //Thread.Sleep(500);
        }

        computer.Close();
    }

    static async Task SendDataToThingSpeak(float? cpu_temp_data, float? cpu_power_data, float? cpu_volt_data, float? gpu_temp_data, float? gpu_power_data, string cpu_data, string gpu_data)
    {
        string apiKey = "BA4YQQXFI2T38UJH";
        if (cpu_temp_data.HasValue && cpu_power_data.HasValue && cpu_volt_data.HasValue && gpu_temp_data.HasValue && gpu_power_data.HasValue) // Check if cpuTemp has a value
        {
            using (HttpClient client = new HttpClient())
            {
                var content = new FormUrlEncodedContent(new[]
                {
                new KeyValuePair<string, string>("api_key", apiKey),
                new KeyValuePair<string, string>("field1", cpu_temp_data.ToString()),
                new KeyValuePair<string, string>("field2", cpu_power_data.ToString()),
                new KeyValuePair<string, string>("field3", cpu_volt_data.ToString()),
                new KeyValuePair<string, string>("field4", gpu_temp_data.ToString()),
                new KeyValuePair<string, string>("field5", gpu_power_data.ToString()),
                new KeyValuePair<string, string>("field6", cpu_data),
                new KeyValuePair<string, string>("field7", gpu_data)// Access the Value property
            });

                var response = await client.PostAsync("https://api.thingspeak.com/update", content);

                if (response.IsSuccessStatusCode)
                {
                    Console.WriteLine("Data uploaded successfully!");
                }
                else
                {
                    Console.WriteLine("Failed to upload data. Error: " + response.StatusCode);
                }
            }
        }
        else
        {
            Console.WriteLine("Data Value is null. Data not uploaded.");
        }
    }
}

