using System.Text;
using UIXTool.IO;
using UIXTool.Utilities;

namespace UIXTool.Formats.Uix
{
    public class Uix
    {
        public string Magic { get; private set; }   // XSK0
        public ushort HeaderSize { get; private set; }    // TODO: confirm
        public ushort ItemCount { get; private set; }
        public string Magic2 { get; private set; }  // UIX
        public uint Unk2 { get; private set; }  // always 0?
        public uint Unk3 { get; private set; }  // always 13?

        public List<UixItem> Items { get; private set; } = new();

        public long StreamPosition { get; private set; }
        public string Path { get; private set; }

        public string Name
        {
            get
            {
                return System.IO.Path.GetFileName(Path);
            }
        }

        public string ToolTip
        {
            get
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendFormat("Item Count: {0}\r\n", ItemCount);
                sb.AppendFormat("Path: {0}\r\n", Path);
                sb.AppendFormat("Stream Position: 0x{0}\r\n", StreamPosition.ToString("X"));
                return sb.ToString();
            }
        }

        public Uix(string path)
        {
            Path = path ?? throw new ArgumentNullException(nameof(path));

            using var stream = new EndianStream(File.Open(path, FileMode.Open, FileAccess.Read, FileShare.Read));
            StreamPosition = stream.Position;

            Magic = stream.ReadAscii(4);
            Assert.Throw(Magic.Equals("XSK0"), "Invalid magic.");
            HeaderSize = stream.Read<ushort>();
            Assert.LogDebug(HeaderSize == 20, "Unexpected {0} value.", nameof(HeaderSize));
            ItemCount = stream.Read<ushort>();
            Magic2 = stream.ReadAscii(4).Trim('\0');
            Assert.Throw(Magic2.Equals("UIX"), "Invalid magic.");
            Unk2 = stream.Read<uint>();
            Assert.LogDebug(Unk2 == 0, "Unexpected {0} value.", nameof(Unk2));
            Unk3 = stream.Read<uint>();
            Assert.LogDebug(Unk3 == 13, "Unexpected {0} value.", nameof(Unk3));

            // parse the entries
            for (int i = 0; i < ItemCount; i++)
            {
                Items.Add(new UixItem(this, stream, StreamPosition + 20 + i * UixItem.StructSize));
            }
        }
    }
}
