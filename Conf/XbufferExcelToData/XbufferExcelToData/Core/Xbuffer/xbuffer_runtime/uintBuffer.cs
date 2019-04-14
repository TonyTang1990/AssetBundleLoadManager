/*
 * File Name:               uintBuffer.cs
 *
 * Description:             基本类型处理
 * Author:                  lisiyu <576603306@qq.com>
 * Create Date:             2017/10/25
 */

using System;

namespace xbuffer
{
    public class uintBuffer
    {
        private static readonly uint size = sizeof(uint);

        public unsafe static uint deserialize(byte[] buffer, ref uint offset)
        {
            fixed (byte* ptr = buffer)
            {
                var value = *(uint*)(ptr + offset);
                offset += size;
                return BitConverter.IsLittleEndian ? value : utils.toLittleEndian(value);
            }
        }

        public unsafe static void serialize(uint value, XSteam steam)
        {
            steam.applySize(size);
            fixed (byte* ptr = steam.contents[steam.index_group])
            {
                *(uint*)(ptr + steam.index_cell) = BitConverter.IsLittleEndian ? value : utils.toLittleEndian(value);
                steam.index_cell += size;
            }
        }
    }
}