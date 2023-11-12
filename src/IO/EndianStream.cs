using System.Numerics;
using System.Runtime.InteropServices;
using System.Text;

namespace UIXTool.IO
{
    // TODO: ReadAscii/PeekAscii (no length specifier, read until null termination)
    // TODO: WriteAscii, PokeAscii (optional bool to write null terminator)

    /// <summary>
    /// A stream that can control which endianness to interpret data as.
    /// </summary>
    public class EndianStream : Stream
    {
        /// <summary>
        /// The byte order endian type.
        /// </summary>
        public enum ByteOrder
        {
            Little,
            Big
        }

        /// <summary>
        /// The base stream.
        /// </summary>
        public Stream BaseStream { get; private set; }

        /// <summary>
        /// The desired endianness.
        /// </summary>
        public ByteOrder Order { get; private set; }

        /// <summary>
        /// Wraps a stream with the specified byte order.
        /// </summary>
        /// <param name="stream">The stream to wrap.</param>
        /// <param name="order">The byte order. Default to little endian.</param>
        /// <exception cref="ArgumentNullException"></exception>
        public EndianStream(Stream stream, ByteOrder order = ByteOrder.Little)
        {
            BaseStream = stream ?? throw new ArgumentNullException(nameof(stream));
            Order = order;
        }

        /// <summary>
        /// Reads a number of type T from the stream.
        /// </summary>
        /// <typeparam name="T">The number type.</typeparam>
        /// <returns>Returns the number of type T.</returns>
        public T Read<T>() where T : struct, INumber<T>
        {
            // read the number as bytes
            byte[] buffer = new byte[Marshal.SizeOf(typeof(T))];
            ReadExactly(buffer);

            // reverse the byte order for an endian mismatch
            if (BitConverter.IsLittleEndian ^ Order == ByteOrder.Little)
            {
                Array.Reverse(buffer);
            }

            // return the value
            return MemoryMarshal.Cast<byte, T>(buffer)[0];
        }

        /// <summary>
        /// Reads a number of type T from the specified stream position.
        /// </summary>
        /// <typeparam name="T">The number type.</typeparam>
        /// <param name="position">The stream position.</param>
        /// <returns>Returns the number of type T.</returns>
        public T Read<T>(long position) where T : struct, INumber<T>
        {
            Position = position;
            return Read<T>();
        }

        /// <summary>
        /// Reads a number of type T from the stream without advancing the position.
        /// </summary>
        /// <typeparam name="T">The number type.</typeparam>
        /// <returns>Returns the number of type T.</returns>
        public T Peek<T>() where T : struct, INumber<T>
        {
            long originalPosition = Position;
            T value = Read<T>();
            Position = originalPosition;
            return value;
        }

        /// <summary>
        /// Reads a number of type T from the specified stream position while preserving the original position.
        /// </summary>
        /// <typeparam name="T">The number type.</typeparam>
        /// <param name="position">The stream position.</param>
        /// <returns>Returns the number of type T.</returns>
        public T Peek<T>(long position) where T : struct, INumber<T>
        {
            long originalPosition = Position;
            T value = Read<T>(position);
            Position = originalPosition;
            return value;
        }

        /// <summary>
        /// Reads a number of bytes from the stream.
        /// </summary>
        /// <param name="count">The number of bytes to read.</param>
        /// <returns>Returns the bytes read.</returns>
        public byte[] ReadBytes(int count)
        {
            byte[] data = new byte[count];
            ReadExactly(data);
            return data;
        }

        /// <summary>
        /// Reads a number of bytes from the stream without advancing the position.
        /// </summary>
        /// <param name="count">The number of bytes to read.</param>
        /// <returns>Returns the bytes read.</returns>
        public byte[] PeekBytes(int count)
        {
            long originalPosition = Position;
            byte[] data = ReadBytes(count);
            Position = originalPosition;
            return data;
        }

        /// <summary>
        /// Reads a number of bytes from the specified stream position.
        /// </summary>
        /// <param name="position">The stream position.</param>
        /// <param name="count">The number of bytes to read.</param>
        /// <returns>Returns the bytes read.</returns>
        public byte[] ReadBytes(long position, int count)
        {
            Position = position;
            return ReadBytes(count);
        }

        /// <summary>
        /// Reads a number of bytes from the specified stream position while preserving the original position.
        /// </summary>
        /// <param name="position">The stream position.</param>
        /// <param name="count">The number of bytes to read.</param>
        /// <returns>Returns the bytes read.</returns>
        public byte[] PeekBytes(long position, int count)
        {
            long originalPosition = Position;
            byte[] data = ReadBytes(position, count);
            Position = originalPosition;
            return data;
        }

        /// <summary>
        /// Reads a number of ASCII characters from the stream.
        /// </summary>
        /// <param name="length">The number of characters to read.</param>
        /// <returns>Returns the ASCII string.</returns>
        public string ReadAscii(int length)
        {
            return Encoding.ASCII.GetString(ReadBytes(length));
        }

        /// <summary>
        /// Reads a number of ASCII characters from the stream without advancing the position.
        /// </summary>
        /// <param name="length">The number of characters to read.</param>
        /// <returns>Returns the ASCII string.</returns>
        public string PeekAscii(int length)
        {
            long originalPosition = Position;
            var str = ReadAscii(length);
            Position = originalPosition;
            return str;
        }

        /// <summary>
        /// Reads a number of ASCII characters from the specified stream position.
        /// </summary>
        /// <param name="position">The stream position.</param>
        /// <param name="length">The number of characters to read.</param>
        /// <returns>Returns the ASCII string.</returns>
        public string ReadAscii(long position, int length)
        {
            Position = position;
            return ReadAscii(length);
        }

        /// <summary>
        ///  Reads a number of ASCII characters from the specified stream position while preserving the original position.
        /// </summary>
        /// <param name="position">The stream position.</param>
        /// <param name="length">The number of characters to read.</param>
        /// <returns>Returns the ASCII string.</returns>
        public string PeekAscii(long position, int length)
        {
            long originalPosition = Position;
            var str = ReadAscii(position, length);
            Position = originalPosition;
            return str;
        }

        /// <summary>
        /// Reads a string of ASCII characters from the stream until a null terminator is encountered.
        /// </summary>
        /// <returns>Returns the ASCII string.</returns>
        /// <exception cref="EndOfStreamException"></exception>
        public string ReadNullTerminatedAscii()
        {
            StringBuilder sb = new StringBuilder();
            Span<byte> buffer = stackalloc byte[16];

            while (Position < Length)
            {
                // read the next chunk of data from the stream
                int bytesToRead = (int)Math.Min(Length - Position, buffer.Length);
                var slice = buffer.Slice(0, bytesToRead);
                Read(slice);

                // decode the bytes as unicode
                var str = Encoding.ASCII.GetString(slice);

                // build the string scanning for the null terminator
                foreach (var c in str)
                {
                    if (c == '\0')
                    {
                        return sb.ToString();
                    }
                    else
                    {
                        sb.Append(c);
                    }
                }
            }

            throw new EndOfStreamException();
        }

        /// <summary>
        /// Reads a string of ASCII characters from the specified stream position until a null terminator is encountered.
        /// </summary>
        /// <returns>Returns the ASCII string.</returns>
        /// <exception cref="EndOfStreamException"></exception>
        public string ReadNullTerminatedAscii(long position)
        {
            Position = position;
            return ReadNullTerminatedAscii();
        }

        /// <summary>
        /// Reads a string of ASCII characters from the stream until a null terminator is encountered while preserving the original position.
        /// </summary>
        /// <returns>Returns the ASCII string.</returns>
        public string PeekNullTerminatedAsci()
        {
            long originalPosition = Position;
            var str = ReadNullTerminatedAscii();
            Position = originalPosition;
            return str;
        }

        /// <summary>
        /// Reads a string of ASCII characters from the specified stream position until a null terminator is encountered while preserving the original position.
        /// </summary>
        /// <returns>Returns the ASCII string.</returns>
        public string PeekNullTerminatedAsci(long position)
        {
            Position = position;
            return PeekNullTerminatedAsci();
        }

        /// <summary>
        /// Reads a string of unicode characters from the stream until a null terminator is encountered.
        /// </summary>
        /// <returns>Returns the unicode string.</returns>
        /// <exception cref="EndOfStreamException"></exception>
        public string ReadNullTerminatedUnicode()
        {
            StringBuilder sb = new StringBuilder();
            Span<byte> buffer = stackalloc byte[16];

            while (Position < Length)
            {
                // read the next chunk of data from the stream
                int bytesToRead = (int)Math.Min(Length - Position, buffer.Length);
                var slice = buffer.Slice(0, bytesToRead);
                Read(slice);

                // decode the bytes as unicode
                var str = Encoding.Unicode.GetString(slice);

                // build the string scanning for the null terminator
                foreach (var c in str)
                {
                    if (c  == '\0')
                    {
                        return sb.ToString();
                    }
                    else
                    {
                        sb.Append(c);
                    }
                }
            }

            throw new EndOfStreamException();
        }

        /// <summary>
        /// Reads a string of unicode characters from the stream until a null terminator is encountered while preserving the original position.
        /// </summary>
        /// <returns>Returns the unicode string.</returns>
        /// <exception cref="EndOfStreamException"></exception>
        public string PeekNullTerminatedUnicode()
        {
            long originalPosition = Position;
            var str = ReadNullTerminatedUnicode();
            Position = originalPosition;
            return str;
        }

        /// <summary>
        /// Reads a string of unicode characters from the specified stream location until a null terminator is encountered.
        /// </summary>
        /// <returns>Returns the unicode string.</returns>
        /// <exception cref="EndOfStreamException"></exception>
        public string ReadNullTerminatedUnicode(long position)
        {
            Position = position;
            return ReadNullTerminatedUnicode();
        }

        /// <summary>
        /// Reads a string of unicode characters from the specified stream location until a null terminator is encountered while preserving the original position.
        /// </summary>
        /// <returns>Returns the unicode string.</returns>
        /// <exception cref="EndOfStreamException"></exception>
        public string PeekNullTerminatedUnicode(long position)
        {
            long originalPosition = Position;
            var str = ReadNullTerminatedUnicode(position);
            Position = originalPosition;
            return str;
        }

        /// <summary>
        /// Writes a number of type T to the stream.
        /// </summary>
        /// <typeparam name="T">The number type.</typeparam>
        /// <param name="number">The number.</param>
        public void Write<T>(T number) where T : struct, INumber<T>
        {
            // write the number as bytes
            byte[] buffer = new byte[Marshal.SizeOf(typeof(T))];
            MemoryMarshal.Write(buffer, ref number);

            // reverse the byte order for an endian mismatch
            if (BitConverter.IsLittleEndian ^ Order == ByteOrder.Little)
            {
                Array.Reverse(buffer);
            }

            // write the value
            Write(buffer, 0, buffer.Length);
        }

        /// <summary>
        /// Writes a number of type T to the specified stream position.
        /// </summary>
        /// <typeparam name="T">The number type.</typeparam>
        /// <param name="position">The stream position.</param>
        /// <param name="number">The number.</param>
        public void Write<T>(long position, T number) where T : struct, INumber<T>
        {
            Position = position;
            Write(number);
        }

        /// <summary>
        /// Writes data to the stream.
        /// </summary>
        /// <param name="data">The data.</param>
        public void Write(byte[] data)
        {
            Write(data, 0, data.Length);
        }

        /// <summary>
        /// Writes data to the specified stream position.
        /// </summary>
        /// <param name="position">The stream position.</param>
        /// <param name="data">The data.</param>
        public void Write(long position, byte[] data)
        {
            Position = position;
            Write(data);
        }

        /// <summary>
        /// Writes data to the stream without advancing the position.
        /// </summary>
        /// <typeparam name="T">The number type.</typeparam>
        /// <param name="number">The number.</param>
        public void Poke<T>(T number) where T : struct, INumber<T>
        {
            long originalPosition = Position;
            Write(number);
            Position = originalPosition;
        }

        /// <summary>
        /// Writes data to the specified stream position while preserving the original position.
        /// </summary>
        /// <typeparam name="T">The number type.</typeparam>
        /// <param name="position">The stream position.</param>
        /// <param name="number">The number.</param>
        public void Poke<T>(long position, T number) where T : struct, INumber<T>
        {
            long originalPosition = Position;
            Write(position, number);
            Position = originalPosition;
        }

        /// <summary>
        /// Writes data to the stream without advancing the position.
        /// </summary>
        /// <param name="data">The data.</param>
        public void Poke(byte[] data)
        {
            long originalPosition = Position;
            Write(data);
            Position = originalPosition;
        }

        /// <summary>
        /// Writes data to the specified stream position while preserving the original position.
        /// </summary>
        /// <param name="position">The stream position.</param>
        /// <param name="data">The data.</param>
        public void Poke(long position, byte[] data)
        {
            long originalPosition = Position;
            Write(position, data);
            Position = originalPosition;
        }

        ///<inheritdoc/>
        public override bool CanRead => BaseStream.CanRead;
        ///<inheritdoc/>
        public override bool CanSeek => BaseStream.CanSeek;
        ///<inheritdoc/>
        public override bool CanWrite => BaseStream.CanWrite;
        ///<inheritdoc/>
        public override long Length => BaseStream.Length;
        ///<inheritdoc/>
        public override long Position { get => BaseStream.Position; set => BaseStream.Position = value; }
        ///<inheritdoc/>
        public override void Flush() => BaseStream.Flush();
        ///<inheritdoc/>
        public override int Read(byte[] buffer, int offset, int count) => BaseStream.Read(buffer, offset, count);
        ///<inheritdoc/>
        public override long Seek(long offset, SeekOrigin origin) => BaseStream.Seek(offset, origin);
        ///<inheritdoc/>
        public override void SetLength(long value) => BaseStream.SetLength(value);
        ///<inheritdoc/>
        public override void Write(byte[] buffer, int offset, int count) => BaseStream.Write(buffer, offset, count);
    }
}
