/*
 * File Name:               boolBuffer.cs
 *
 * Description:             基本类型处理
 * Author:                  lisiyu <576603306@qq.com>
 * Create Date:             2017/10/25
 */

namespace xbuffer
{
    public class boolBuffer
    {
        private static readonly uint size = sizeof(bool);

        public unsafe static bool deserialize(byte[] buffer, ref uint offset)
        {
            fixed (byte* ptr = buffer)
            {
                var value = *(bool*)(ptr + offset);
                offset += size;
                return value;
            }
        }

        public unsafe static void serialize(bool value, XSteam steam)
        {
            steam.applySize(size);
            fixed (byte* ptr = steam.contents[steam.index_group])
            {
                *(bool*)(ptr + steam.index_cell) = value;
                steam.index_cell += size;
            }
        }
    }
}