using UIXTool.IO;

namespace UIXTool.Formats.Uix
{
    public class UixItemMetaData
    {
        public const int StructSize = 44;
        public long StreamPosition { get; private set; }

        // TODO: other possibles - padding, margin, offsets, font size etc.
        public ushort X { get; private set; }
        public ushort Y { get; private set; }
        public ushort Width { get; private set; }
        public ushort Height { get; private set; }
        public uint Unk5 { get; private set; }
        public uint Unk6 { get; private set; }
        public uint Unk7 { get; private set; }
        public uint Unk8 { get; private set; }
        public uint Unk9 { get; private set; }
        public uint Unk10 { get; private set; }
        public uint Unk11 { get; private set; }
        public uint Unk12 { get; private set; }
        public uint Unk13 { get; private set; }

        public UixItemMetaData(EndianStream stream, long? position = null)
        {
            if (position != null) stream.Position = position.Value;
            StreamPosition = stream.Position;

            X = stream.Read<ushort>();
            Y = stream.Read<ushort>();
            Width = stream.Read<ushort>();
            Height = stream.Read<ushort>();
            Unk5 = stream.Read<uint>();
            Unk6 = stream.Read<uint>();
            Unk7 = stream.Read<uint>();
            Unk8 = stream.Read<uint>();
            Unk9 = stream.Read<uint>();
            Unk10 = stream.Read<uint>();
            Unk11 = stream.Read<uint>();
            Unk12 = stream.Read<uint>();
            Unk13 = stream.Read<uint>();
        }
    }
}
