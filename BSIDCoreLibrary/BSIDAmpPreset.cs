namespace BSIDCoreLibrary
{
    public class BSIDAmpPreset
    {
        private byte[] _packet;

        public static BSIDAmpPreset FromPacket(byte[] packet)
        {
            return new BSIDAmpPreset(packet);
        }

        private BSIDAmpPreset(byte[] packet)
        {
            _packet = packet;

            //TODO: Implement the rest of the constructor
        }
    }
}