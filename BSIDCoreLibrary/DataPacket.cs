using Microsoft.Extensions.Logging;
using System.ComponentModel;
using System.Linq;

namespace BSIDCoreLibrary
{
    public class DataPacket
    {
        internal ILogger<BSIDDevice> _logger;
        public byte[] Packet { get; set; }

        public DataPacketType Type { get; }

        public DataPacket(ILogger<BSIDDevice> logger, byte[] packet)
        {
            _logger = logger;
            Packet = packet;

            Type = (DataPacketType)Packet[0];
        }
    }

    public class PresetNamePacket : DataPacket
    {
        public PresetPacketSubtype Subtype { get; }
        public byte PresetNo { get; }
        public string Name { get; }

        public PresetNamePacket(ILogger<BSIDDevice> logger, byte[] packet) : base(logger, packet)
        {
            Subtype = (PresetPacketSubtype)Packet[1];

            PresetNo = Packet[2];

            Name = packet.Skip(5).Take(20).ToString();
        }
    }

    public class PresetChangePacket : DataPacket
    {
        public PresetPacketSubtype Subtype { get; }
        public byte PresetNumber { get; }

        public PresetChangePacket(ILogger<BSIDDevice> logger, byte[] packet) : base(logger, packet)
        {
            Subtype = (PresetPacketSubtype)Packet[1];

            PresetNumber = Packet[2];
        }
    }

    public class PresetSettingsPacket : DataPacket
    {
        public PresetPacketSubtype Subtype { get; }
        public byte PresetNumber { get; }
        public BSIDAmpPreset Settings { get; }

        public PresetSettingsPacket(ILogger<BSIDDevice> logger, byte[] packet) : base(logger, packet)
        {
            PresetNumber = packet[0];

            Subtype = (PresetPacketSubtype)Packet[1];

            Settings = BSIDAmpPreset.FromPacket(Packet);
        }
    }

    public class ControlChangePacket : DataPacket
    {
        public ControlPacketSubtype Subtype { get; }
        public Controls Control { get; }
        public byte Value { get; }

        public ControlChangePacket(ILogger<BSIDDevice> logger, byte[] packet) : base(logger, packet)
        {
            Subtype = (ControlPacketSubtype)Packet[3];

            var controlId = Packet[1];
            try
            {
                Control = (Controls)controlId;
            }
            catch (InvalidEnumArgumentException)
            {
                _logger.LogError("Unrecognised control ID: 0x", string.Format("X2", controlId));
            }

            Value = Packet[4];
        }
    }

    public class DelayTimeChangePacket : DataPacket
    {
        public ControlPacketSubtype Subtype { get; }
        public int Value { get; set; }

        public DelayTimeChangePacket(ILogger<BSIDDevice> logger, byte[] packet) : base(logger, packet)
        {
            Subtype = (ControlPacketSubtype)Packet[3];
            Value = Packet[4] + 256 * Packet[5];
        }
    }

    public class DelayTypeChangePacket : DataPacket
    {
        public ControlPacketSubtype Subtype { get; }
        public int DelayType { get; }
        public int Feedback { get; }

        public DelayTypeChangePacket(ILogger<BSIDDevice> logger, byte[] packet) : base(logger, packet)
        {
            DelayType = Packet[4];
            Feedback = Packet[5];
        }
    }

    public class ReverbTypeChangePacket : DataPacket
    {
        public ControlPacketSubtype Subtype { get; }
        public int ReverbType { get; }
        public int Size { get; }

        public ReverbTypeChangePacket(ILogger<BSIDDevice> logger, byte[] packet) : base(logger, packet)
        {
            ReverbType = Packet[4];
            Size = Packet[5];
        }
    }

    public class ModulationTypeChangePacket : DataPacket
    {
        public ControlPacketSubtype Subtype { get; }
        public int ModulationType { get; }
        public int Feedback { get; }

        public ModulationTypeChangePacket(ILogger<BSIDDevice> logger, byte[] packet) : base(logger, packet)
        {
            ModulationType = Packet[4];
            Feedback = Packet[5];
        }
    }

    public enum DataPacketType
    {
        Preset = 0x02,
        Control = 0x03,
        Startup = 0x07,
        Mode = 0x08,
        Tuner = 0x09
    }

    public enum PresetPacketSubtype
    {
        PresetName = 0x04,
        PresetChange = 0x06,
        PresetSettings = 0x05
    }

    public enum ControlPacketSubtype
    {
        ControlChange = 0x01,
        EffectChange = 0x02,
        ControlSettings = 0x2a
    }

    public enum ModePacketSubtype
    {
        Startup = 0x01,
        ManualMode = 0x03,
        TunerMode = 0x11
    }
}