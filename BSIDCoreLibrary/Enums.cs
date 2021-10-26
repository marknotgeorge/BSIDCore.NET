namespace BSIDCoreLibrary
{
    public enum AmpModel
    {
        IdTVP = 0x0001,
        IdCore = 0x0010
    }

    public enum Controls
    {
        Voice = 0x01,
        Gain = 0x02,
        Volume = 0x03,
        Bass = 0x04,
        Middle = 0x05,
        Treble = 0x06,
        ISF = 0x07,
        TVPValve = 0x08,
        Resonance = 0x0b,
        Presence = 0x0c,
        MasterVolume = 0x0d,
        TVPSwitch = 0x0e,
        ModulationSwitch = 0x0f,
        DelaySwitch = 0x10,
        ReverbSwitch = 0x11,
        ModulationType = 0x12,
        ModulationSegVal = 0x13,
        ModulationManual = 0x14,    // Flanger only
        ModulationLevel = 0x15,
        ModulationSpeed = 0x16,
        DelayType = 0x17,
        DelayFeedback = 0x18,       // Segment value
        DelayLevel = 0x1a,
        DelayTime = 0x1b,
        DelayTimeCoarse = 0x1c,
        ReverbType = 0x1d,
        ReverbSize = 0x1e,
        ReverbLevel = 0x20,
        FxFocus = 0x24
    }

    public enum ModulationType
    {
        Mix = 0,
        Flanger,
        Feedback,
        Frequency
    }
}