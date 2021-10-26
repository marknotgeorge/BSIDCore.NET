using Device.Net;
using Hid.Net;
using SerialPort.Net.Windows;
using Hid.Net.Windows;
using Usb.Net.Windows;
using Device.Net.Windows;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;
using System.ComponentModel;

namespace BSIDCoreLibrary
{
    public class BSIDDevice : IDisposable, INotifyPropertyChanged
    {
        #region Fields

        private const int _pollIntervalInMs = 6000;

        private const int _usagePage = 0xff00;
        private const int _usage = 0x01;
        private const int _vendorId = 0x27d4;

        public static readonly List<FilterDeviceDefinition> HidDeviceDefinitions = new List<FilterDeviceDefinition>
        {
            new FilterDeviceDefinition(vendorId: _vendorId, usagePage: _usagePage)
        };

        public static readonly List<FilterDeviceDefinition> UsbDeviceDefinitions = new List<FilterDeviceDefinition>
        {
            new FilterDeviceDefinition(_vendorId, 0x0001),
            new FilterDeviceDefinition(_vendorId, 0x0010),
            new FilterDeviceDefinition(_vendorId, 0x0012)
        };

        private ILogger _logger;

        private IDeviceFactory _deviceFactory;
        private IDevice _amplifierDevice;
        private DeviceListener _deviceListener;

        private bool _polling;
        private Task _pollingTask;
        private CancellationTokenSource _pollingTaskCts;

        private bool _disposed;

        #endregion Fields

        #region Properties

        private bool _amplifierConnected;

        public bool AmplifierConnected
        {
            get => _amplifierConnected;
            private set
            {
                if (_amplifierConnected != value)
                {
                    _amplifierConnected = value;
                    OnPropertyChanged(nameof(AmplifierConnected));
                    OnPropertyChanged(nameof(AmplifierDisconnected));
                }
            }
        }

        public bool AmplifierDisconnected
        {
            get => !_amplifierConnected;
        }

        public ConnectedDeviceDefinition DeviceDefinition
        {
            get; private set;
        }

        #endregion Properties

        #region Controls

        [Range(0, 5)]
        public int Voice { get; set; }

        [Range(0, 127)]
        public int Gain { get; set; }

        [Range(0, 127)]
        public int Volume { get; set; }

        [Range(0, 127)]
        public int Bass { get; set; }

        [Range(0, 127)]
        public int Middle { get; set; }

        [Range(0, 127)]
        public int Treble { get; set; }

        [Range(0, 127)]
        public int ISF { get; set; }

        [Range(0, 5)]
        public int TVPValve { get; set; }

        [Range(0, 127)]
        public int Resonance { get; set; }

        [Range(0, 127)]
        public int Presence { get; set; }

        [Range(0, 127)]
        public int MasterVolume { get; set; }

        public bool TVPSwitch { get; set; }

        public bool ModulationSwitch { get; set; }

        public bool DelaySwitch { get; set; }

        public bool ReverbSwitch { get; set; }

        [Range(0, 3)]
        public int ModulationType { get; set; }

        [Range(0, 31)]
        public int ModulationSegVal { get; set; }

        [Range(0, 127)]
        public int ModulationLevel { get; set; }

        [Range(0, 127)]
        public int ModulationSpeed { get; set; }

        [Range(0, 127)]
        public int ModulationManual { get; set; }

        [Range(0, 3)]
        public int DelayType { get; set; }

        [Range(0, 31)]
        public int DelayFeedback { get; set; }

        [Range(0, 127)]
        public int DelayLevel { get; set; }

        [Range(100, 2000)]
        public int DelayTime { get; set; }

        [Range(0, 7)]
        public int DelayTimeCoarse { get; set; }

        [Range(0, 3)]
        public int ReverbType { get; set; }

        [Range(0, 31)]
        public int ReverbSize { get; set; }

        [Range(0, 127)]
        public int ReverbLevel { get; set; }

        [Range(1, 3)]
        public int FxFocus { get; set; }

        public string[] TunerNote = { "E", "F", "F#", "G", "G#", "A", "A#", "B", "C", "C#", "D", "D#" };

        #endregion Controls

        #region Constructor

        public BSIDDevice(ILoggerFactory loggerFactory)
        {
            // This is Windows-specific.
            // TODO: Add Android-specific code later.
            var usbFactory = UsbDeviceDefinitions.CreateWindowsUsbDeviceFactory(loggerFactory);
            var hidFactory = HidDeviceDefinitions.CreateWindowsHidDeviceFactory(loggerFactory);

            _deviceFactory = usbFactory.Aggregate(hidFactory);

            _deviceListener = new DeviceListener(_deviceFactory, _pollIntervalInMs, loggerFactory);

            _deviceListener.DeviceInitialized += DeviceListener_DeviceInitialized;
            _deviceListener.DeviceDisconnected += DeviceListener_DeviceDisconnected;

            _logger = loggerFactory.CreateLogger<BSIDDevice>();
        }

        #endregion Constructor

        #region Events

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string info) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(info));

        public event EventHandler DeviceConnected;

        public event EventHandler DeviceDisconnected;

        private void DeviceListener_DeviceDisconnected(object sender, DeviceEventArgs e)
        {
            CloseAmplifier();
            _amplifierDevice = null;
            DeviceDisconnected?.Invoke(this, new EventArgs());
        }

        private async void DeviceListener_DeviceInitialized(object sender, DeviceEventArgs e)
        {
            await ConnectNewDeviceAsync();
            DeviceConnected?.Invoke(this, new EventArgs());
        }

        #endregion Events

        #region Methods

        public void CloseAmplifier()
        {
            _pollingTaskCts?.Cancel();
            _amplifierDevice?.Close();
            AmplifierConnected = false;
            _pollingTask?.Dispose();
        }

        private async Task<bool> ConnectNewDeviceAsync()
        {
            if (!AmplifierConnected)
            {
                var deviceDefinitions = await _deviceFactory.GetConnectedDeviceDefinitionsAsync().ConfigureAwait(false);

                var firstDevice = deviceDefinitions.FirstOrDefault();

                _logger.LogDebug($"First device: {firstDevice.DeviceType} Name: {firstDevice.ProductName}");

                _amplifierDevice = await _deviceFactory.GetDeviceAsync(firstDevice).ConfigureAwait(false);

                await _amplifierDevice.InitializeAsync().ConfigureAwait(false);

                if (!(_amplifierDevice is null))
                {
                    AmplifierConnected = true;
                    DeviceDefinition = _amplifierDevice.ConnectedDeviceDefinition;
                    return true;
                }
                return false;
            }
            return false;
        }

        public bool OpenAmplifier()
        {
            _deviceListener.Start();
            return true;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    CloseAmplifier();
                    _deviceListener.DeviceDisconnected -= DeviceListener_DeviceDisconnected;
                    _deviceListener.DeviceInitialized -= DeviceListener_DeviceInitialized;
                    _deviceListener.Dispose();
                }
                _disposed = true;
            }
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        public Task CreatePollingTask(CancellationTokenSource cancellationTokenSource)
        {
            if (!_polling)
            {
                _polling = true;

                var cancellationToken = cancellationTokenSource.Token;
                _pollingTaskCts = cancellationTokenSource;

                _pollingTask = Task.Run(async () =>
                {
                    _logger.LogDebug("Polling Task started.");
                    try
                    {
                        cancellationToken.ThrowIfCancellationRequested();

                        while (!cancellationToken.IsCancellationRequested && !_disposed)
                        {
                            if (_amplifierConnected)
                            {
                                _logger.LogDebug("Reading data...");
                                var result = await _amplifierDevice?.ReadAsync(cancellationToken);

                                _logger.LogDebug($"Bytes transferred: {result.BytesTransferred}");

                                if (result.BytesTransferred > 0)
                                {
                                    // Remove the frst byte - it's the HID Report ID which we don't need
                                    byte[] dataToParse = result.Data[1..];

                                    ParseReceivedData(dataToParse);
                                }
                            }
                        }

                        _polling = false;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex.Message);
                    }
                    finally
                    {
                        _polling = false;
                    }
                });

                return _pollingTask;
            }

            return null;
        }

        private void ParseReceivedData(byte[] data)
        {
            _logger.LogDebug($"Received ${data.Length} bytes");

            _logger.LogDebug(FormatData(data));
        }

        private string FormatData(byte[] data)
        {
            // Turn bytes into hex strings
            var hexStrings = new List<string>();
            foreach (byte dataByte in data)
            {
                var dataString = dataByte.ToString("X2");
                hexStrings.Add(dataString);
            }
            var hexStringsArray = hexStrings.ToArray();

            var returnString = Environment.NewLine;
            for (var i = 1; i <= hexStringsArray.Length; i++)
            {
                returnString += hexStringsArray[i - 1];
                if (i % 16 == 0)
                {
                    returnString += Environment.NewLine;
                }
                else if (i != hexStringsArray.Length)
                {
                    returnString += " ";
                }
            }

            return returnString;
        }

        public async Task<string> ShowConnectedDevices()
        {
            var connectedDevices = await _deviceFactory.GetConnectedDeviceDefinitionsAsync().ConfigureAwait(false);
            return string.Join(Environment.NewLine,
                 connectedDevices
                     .Where(D => D.Manufacturer.Contains("Blackstar"))
                     .Select(d => $"Name:{d.ProductName} Usage: {d.Usage} UsagePage {d.UsagePage}"));
        }

        #endregion Methods
    }
}