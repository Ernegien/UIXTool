using System.Diagnostics;
using System.Text;
using UIXTool.IO;
using UIXTool.Utilities;

namespace UIXTool.Formats.Uix
{

    [DebuggerDisplay("{Name}")]
    public class UixItem
    {
        public const int StructSize = 20;
        public long StreamPosition { get; private set; }

        public ushort Type { get; private set; }
        public UixLanguageCode Language { get; private set; }
        public ushort Unk2 { get; private set; }    // another type?
        public ushort MetaCount { get; private set; }
        public uint DataOffset { get; private set; }
        public int EntryMetaDataSize {  get; private set; }
        public int DataSize { get; private set; }
        public byte[] Data { get; private set; }

        public string Name
        {
            get
            {
                // TODO: proper type enum
                string typeName = "Type " + Type.ToString("X2");
                if (Type == 1 || Type == 3)
                {
                    typeName = "Strings";
                }
                else if (Type == 2)
                {
                    typeName = "Icons";
                }

                return "UixItem - " + typeName + " (" + Language.ToString() + ")";
            }
        }

        public string ToolTip
        {
            get
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendFormat("Type: {0}\r\n", Type.ToString("X"));
                sb.AppendFormat("Language: {0}\r\n", Language.ToString());
                sb.AppendFormat("Unk: {0}\r\n", Unk2.ToString("X4"));
                sb.AppendFormat("Meta Count: {0}\r\n", MetaCount);
                sb.AppendFormat("Stream Position: 0x{0}\r\n", StreamPosition.ToString("X"));
                sb.AppendFormat("Data Position: 0x{0}\r\n", (StreamPosition + DataOffset).ToString("X"));
                sb.AppendFormat("Data Size: 0x{0}\r\n", DataSize.ToString("X"));
                return sb.ToString();
            }
        }

        public List<UixItemMeta> Metas { get; private set; } = new();

        public Dictionary<int, string> StringIds {  get; private set; } = new();
        public Xpr.Xpr? Xpr { get; private set; }
        public Uix? Parent { get; private set; }

        public UixItem(Uix parent, EndianStream stream, long? position = null)
        {
            Parent = parent;
            if (position != null) stream.Position = position.Value;
            StreamPosition = stream.Position;

            // parse main item info
            Type = stream.Read<ushort>();
            Language = (UixLanguageCode)stream.Read<ushort>();
            Unk2 = stream.Read<ushort>();
            MetaCount = stream.Read<ushort>();
            DataOffset = stream.Read<uint>();
            EntryMetaDataSize = stream.Read<int>();
            DataSize = stream.Read<int>();
            long dataStreamPosition = parent.StreamPosition + DataOffset;
            Data = stream.PeekBytes(dataStreamPosition, DataSize);

            // parse the main meta info entries struct
            stream.Position = dataStreamPosition;
            for (int i = 0; i < MetaCount; i++)
            {
                Metas.Add(new UixItemMeta(stream));
            }

            // store the meta data base offset for later 
            long metaDataBaseOffset = dataStreamPosition + MetaCount * UixItemMeta.StructSize;

            // parse the xpr if available
            if (EntryMetaDataSize != -1)
            {
                Xpr = new Xpr.Xpr(this, parent.Path, metaDataBaseOffset + EntryMetaDataSize);
            }

            // parse the meta data
            switch (Type)
            {
                // string table
                case 1:
                case 3:
                    {   
                        foreach (var meta in Metas)
                        {
                            var str = stream.PeekNullTerminatedUnicode(metaDataBaseOffset + meta.Offset);
                            meta.String = str;
                            StringIds[meta.Id] = str;
                        }
                    }
                    break;

                // icon textures inside xpr
                case 2:
                    {
                        // meta here just seems to be a quick way to look up textures in an xpr by ID and doesn't really contain anything else useful?

                        // TODO: assert EntryMetaDataSize == 0
                        // TODO: save texture index instead? let upstream logic pick through xpr if needed?

                        if (Xpr?.Resources.Count != MetaCount)
                        {
                            // TODO: warn
                        }

                        // meta data offset relative to xpr start + 12-byte xpr header
                        foreach (var meta in Metas)
                        {
                            // find the matching resource entry using the entry info offset relative to the end of the xpr header
                            var resource = Xpr?.Resources.Find(x => x.EntryInfoOffset ==  meta.Offset);
                            if (resource != null)
                            {
                                // link meta with it, mainly the id?
                                meta.Resource = resource;
                            }
                        }
                    }
                    break;

                // external metadata
                case 0x10:  // single texture?
                case 0x11:  // single texture?
                case 0x12:  // single texture?
                case 0x14:  // single texture?
                case 0x15:  // single texture?
                case 0x16:  // single optional texture?
                case 0x17:  // multiple textures
                case 0x18:  // two textures?
                case 0x19:  // two textures?
                case 0x1A:  // single texture?
                    {
                        Assert.LogDebug(EntryMetaDataSize > 0, "No meta data available.");
    
                        int dataCount = 0;
                        foreach (var meta in Metas)
                        {
                            // some meta entries don't have data associated with them!
                            if (meta.Offset == -1)
                                continue;

                            meta.Data = new UixItemMetaData(stream, metaDataBaseOffset + meta.Offset);
                            dataCount++;
                        }

                        // if the data size is specified, make sure it matches the entries with associated data size
                        if (EntryMetaDataSize > 0)
                        {
                            Assert.LogDebug(dataCount * UixItemMetaData.StructSize == EntryMetaDataSize, "Unexpected meta data encountered.");
                        }
                     }
                    break;
                default:
                    // TODO: unseen type, requires further review
                    break;
            }
        }

        //public void Export(string directory)
        //{
        //    Directory.CreateDirectory(directory);

        //    // save item data
        //    File.WriteAllBytes(Path.Combine(directory, Name + ".bin"), Data);

        //    // save json representation of meta info
        //    JsonSerializerOptions options = new()
        //    {
        //        WriteIndented = true
        //    };
        //    File.WriteAllText(Path.Combine(directory, Name + ".json"), JsonSerializer.Serialize(this, options));
        //}
    }
}
