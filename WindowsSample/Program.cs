using BSIDCoreLibrary;
using Device.Net;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using SerialPort.Net.Windows;
using Hid.Net.Windows;
using Usb.Net.Windows;
using Device.Net.Windows;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace WindowsSample
{
    internal class Program
    {
        #region Fields

        private static readonly ILoggerFactory _loggerFactory = LoggerFactory.Create((builder) =>
        {
            _ = builder.AddDebug();
            _ = builder.SetMinimumLevel(LogLevel.Trace);
        });

        private static ILogger logger;

        static public CancellationTokenSource cts;

        static public BSIDDevice amplifier;

        #endregion Fields

        private static async Task Main(string[] args)
        {
            cts = new CancellationTokenSource();
            var token = cts.Token;

            amplifier = new BSIDDevice(_loggerFactory);

            var connectedDevices = await amplifier.ShowConnectedDevices();

            Console.WriteLine(connectedDevices);

            amplifier.DeviceConnected += Amplifier_DeviceConnected;
            amplifier.DeviceDisconnected += Amplifier_DeviceDisconnected;

            amplifier.OpenAmplifier();

            Console.CancelKeyPress += (s, e) =>
            {
                Console.WriteLine("CTRL+C pressed...");
                cts?.Cancel();
                e.Cancel = true;
            };

            Console.WriteLine("Please connect an amp!");

            while (true)
            {
                Thread.Sleep(50);
            }
        }

        private static void Amplifier_DeviceDisconnected(object sender, EventArgs e)
        {
            Console.WriteLine("Device disconnected!");
            cts?.Cancel();
        }

        private static void Amplifier_DeviceConnected(object sender, EventArgs e)
        {
            Console.WriteLine($"Device {amplifier.DeviceDefinition.ProductName} connected!");

            _ = amplifier.CreatePollingTask(cts);
        }
    }
}