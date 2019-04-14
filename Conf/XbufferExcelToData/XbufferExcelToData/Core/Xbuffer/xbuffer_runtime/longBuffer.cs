/*
 * File Name:               longBuffer.cs
 *
 * Description:             基本类型处理
 * Author:                  lisiyu <576603306@qq.com>
 * Create Date:             2017/10/25
 */

using System;

namespace xbuffer
{
    public class longBuffer
    {
        private static readonly uint size = sizeof(long);

        public unsafe static long deserialize(byte[] buffer, ref uint offset)
        {
            fixed (byte* ptr = buffer)
            {
                var value = *(long*)(ptr + offset);
                offset += size;
                return BitConverter.IsLittleEndian ? value : (long)utils.toLittleEndian((ulong)value);
            }
        }

        public unsafe static void serialize(long value, XSteam steam)
        {
            steam.applySize(size);
            fixed (byte* ptr = steam.contents[steam.index_group])
            {
                *(long*)(ptr + steam.index_cell) = BitConverter.IsLittleEndian ? value : (long)utils.toLittleEndian((ulong)value);
                steam.index_cell += size;
            }
        }
    }
}