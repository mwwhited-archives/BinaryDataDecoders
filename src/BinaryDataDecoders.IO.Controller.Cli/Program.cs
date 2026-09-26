using BinaryDataDecoders.Quarta.RadexOne;
using BinaryDataDecoders.Velleman.K8055;
using System;
using System.ComponentModel;
using System.Composition.Hosting;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace BinaryDataDecoders.IO.Controller.Cli;

class Program
{
    static async Task Main(string[] args)
    {
        var configuration = new ContainerConfiguration();
        var files = Directory.GetFiles(".", "*.dll");
        var assemblies = from p in files
                         select System.Reflection.Assembly.LoadFile(Path.GetFullPath(p));

        configuration.WithAssemblies(assemblies);
        var container = configuration.CreateContainer();
        var devices = container.GetExports<IDeviceDefinition>().ToList();

        Console.WriteLine("=== Select Device ===");
        foreach (var item in devices.Select((device, index) => (device, index)))
        {
            var name = item.device.GetType().GetCustomAttribute<DescriptionAttribute>()?.Description ??
                       item.device.GetType().Name;

            Console.WriteLine($"\t{item.index + 1}) {name}");
        }


        //var definition = new K8055Definition();
        //await new DeviceConsole(
        //    minimumTrasmissionDelay: 10,
        //    testCommandDelay: 10
        //    ).Execute(definition, mid => (mid % 10) switch
        //{
        //    _ => new K8055Request
        //    {
        //        Command = Commands.SetAnalogDigital,
        //        Outputs = (DigitalOutputs)(mid % 256),
        //        Analog1 = (byte)(mid % 256),
        //        Analog2 = (byte)(255 - (mid % 256)),
        //    }
        //});


        //Fencing electronic scoring machines.
        // var definition = new SgStateDefinition();
        //await ew DeviceConsole().Execute(definition);
        // var definition = new FaveroDefinition();
        //await new DeviceConsole().Execute(definition);

        //Quarta Radex One
        var definition = new RadexOneDefinition();
        await new DeviceConsole(
            minimumTrasmissionDelay: 10,
            testCommandDelay: 1000
            ).Execute(definition, x => (x % 10) switch
        {
            1 => (IRadexObject)new ReadSerialNumberRequest((uint)x),
            // 2 => new ReadSerialNumberRequest((uint)x),
            // 3 => new DevicePing((uint)x),
            // 4 => new WriteSettingsRequest((uint)x, AlarmSettings.Audio, 30),
            7 => new ReadSettingsRequest((uint)x),
            //8 => new ResetAccumulatedRequest((uint)x),
            _ => new ReadValuesRequest((uint)x)
        });

        //Zoom H4N
        //var definition = new H4nDefinition();
        //await new DeviceConsole().Execute(definition, id => (id % 10) switch
        //    {
        //        2 => (IH4nMessage)new H4nRequest(H4nRequests.Channel1),
        //        4 => (IH4nMessage)new H4nRequest(H4nRequests.Channel2),
        //        6 => (IH4nMessage)new H4nRequest(H4nRequests.Channel2),
        //        7 => (IH4nMessage)new H4nRequest(H4nRequests.Mic),
        //        _ => (IH4nMessage)new H4nRequest(H4nRequests.Poll)
        //    });

        //GPS
        // var definition = new Nema0183Definition();
        //await new DeviceConsole().Execute(definition);

        Console.WriteLine("done");
    }
}
