using UIXTool.Utilities;

namespace UIXTool.Extensions
{
    public static class NumberExtensions
    {
        /// <summary>
        /// Extracts a bitfield value using the given bit mask range from 0 to 31.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="lowBit"></param>
        /// <param name="highBit"></param>
        /// <returns></returns>
        public static uint BitField(this uint value, int lowBit, int highBit)
        {
            Assert.Throw<ArgumentOutOfRangeException>(lowBit >= 0 && lowBit < 32, nameof(lowBit));
            Assert.Throw<ArgumentOutOfRangeException>(highBit >= 0 && highBit < 32, nameof(highBit));
            Assert.Throw<ArgumentException>(highBit > lowBit, "{0} must be greater than {1}", nameof(highBit), nameof(lowBit));

            return (value & (uint.MaxValue >> (31 - highBit)) & (uint.MaxValue << lowBit)) >> lowBit;
        }

        /// <summary>
        /// Extracts a bitfield value using the given bit mask range from 0 to 15.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="lowBit"></param>
        /// <param name="highBit"></param>
        /// <returns></returns>
        public static ushort BitField(this ushort value, int lowBit, int highBit)
        {
            Assert.Throw<ArgumentOutOfRangeException>(lowBit >= 0 && lowBit < 16, nameof(lowBit));
            Assert.Throw<ArgumentOutOfRangeException>(highBit >= 0 && highBit < 16, nameof(highBit));
            Assert.Throw<ArgumentException>(highBit > lowBit, "{0} must be greater than {1}", nameof(highBit), nameof(lowBit));

            return (ushort)((value & (ushort.MaxValue >> (15 - highBit)) & (ushort.MaxValue << lowBit)) >> lowBit);
        }

        /// <summary>
        /// Extracts a bitfield value using the given bit mask range from 0 to 7.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="lowBit"></param>
        /// <param name="highBit"></param>
        /// <returns></returns>
        public static byte BitField(this byte value, int lowBit, int highBit)
        {
            Assert.Throw<ArgumentOutOfRangeException>(lowBit >= 0 && lowBit < 8, nameof(lowBit));
            Assert.Throw<ArgumentOutOfRangeException>(highBit >= 0 && highBit < 8, nameof(highBit));
            Assert.Throw<ArgumentException>(highBit > lowBit, "{0} must be greater than {1}", nameof(highBit), nameof(lowBit));

            return (byte)((value & (byte.MaxValue >> (7 - highBit)) & (byte.MaxValue << lowBit)) >> lowBit);
        }

        /// <summary>
        /// Determines if a value is a power of 2.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static bool IsPow2(this uint value)
        {
            return (value > 0) && ((value & (value - 1)) == 0); 
        }

        /// <summary>
        /// Determines if a value is a power of 2.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static bool IsPow2(this ushort value)
        {
            return (value > 0) && ((value & (value - 1)) == 0);
        }
    }
}
