using System.Text;
using UIXTool.Formats.Uix;
using UIXTool.IO;
using UIXTool.Utilities;

namespace UIXTool.Formats.Xpr
{
    //https://xboxdevwiki.net/XPR

    /// <summary>
    /// Xbox packed resource.
    /// </summary>
    public class Xpr
    {
        public const int StructSize = 12;

        public UixItem? Parent { get; private set; }
        public long StreamPosition { get; private set; }
        public string? Path { get; private set; }

        public string Magic { get; private set; }
        public int TotalSize {  get; private set; }
        public int HeaderSize { get; private set; } // including header data

        public List<XprResource> Resources { get; private set; } = new();
        public string Name
        {
            get
            {
                return string.IsNullOrWhiteSpace(Path) ? "Xpr" : System.IO.Path.GetFileName(Path);
            }
        }

        public string ToolTip
        {
            get
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendFormat("Path: {0}\r\n", Path);
                sb.AppendFormat("Resource Count: {0}\r\n", Resources.Count);
                sb.AppendFormat("Stream Position: 0x{0}\r\n", StreamPosition.ToString("X"));
                sb.AppendFormat("Size: 0x{0}\r\n", TotalSize.ToString("X"));
                return sb.ToString();
            }
        }

        public Xpr(UixItem? parent, string? path, EndianStream stream, long? position = null)
        {
            Parent = parent;
            Path = path;
            if (position != null) stream.Position = position.Value;
            StreamPosition = stream.Position;

            Magic = stream.ReadAscii(4);
            Assert.LogDebug(Magic.Equals("XPR0"), "Invalid XPR header magic.");    // TODO: there's other XPR formats (XPR1 etc.) as well
            TotalSize = stream.Read<int>();
            HeaderSize = stream.Read<int>();
            Assert.LogDebug(HeaderSize < TotalSize, "{0} must be less than {1}.", nameof(HeaderSize), nameof(TotalSize));
            Assert.LogDebug(HeaderSize % 1024 == 0, "{0} must be aligned on 1024-byte boundaries.", nameof(HeaderSize));    // TODO: confirm, might be 2048
            Assert.LogDebug(TotalSize % 1024 == 0, "{0} must be aligned on 1024-byte boundaries.", nameof(TotalSize));    // TODO: confirm, might be 2048

            while (stream.Peek<int>() != -1 && stream.Position < (StreamPosition + HeaderSize))
            {
                Resources.Add(new XprResource(this, stream));
            }
        }
    }
}
