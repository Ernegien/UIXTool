using UIXTool.Formats.Xpr;
using UIXTool.IO;

namespace UIXTool.Formats.Uix
{
    public class UixItemMeta
    {
        public const int StructSize = 8;
        public long StreamPosition { get; private set; }

        public byte Id { get; private set; }
        public byte Flags { get; private set; }
        public byte Unk1 { get; private set; }
        public byte Unk2 { get; private set; }
        public int Offset {  get; private set; }

        // set these upstream
        public XprResource? Resource { get; set; }
        public string? String { get; set; }
        public UixItemMetaData? Data { get; set; }

        public UixItemMeta(EndianStream stream, long? position = null)
        {
            if (position != null) stream.Position = position.Value;
            StreamPosition = stream.Position;

            Id = stream.Read<byte>();
            Flags = stream.Read<byte>();
            Unk1 = stream.Read<byte>();
            Unk2 = stream.Read<byte>();
            Offset = stream.Read<int>();
        }
    }
}
