/*
 * File Name:               byteBuffer.cs
 *
 * Description:             基本类型处理
 * Author:                  lisiyu <576603306@qq.com>
 * Create Date:             2017/11/09
 */

namespace xbuffer
{
    public class byteBuffer
    {
        private static readonly uint size = sizeof(byte);

        public unsafe static byte deserialize(byte[] buffer, ref uint offset)
        {
            var value = buffer[offset];
            offset += size;
            return value;
        }

        public unsafe static void serialize(byte value, XSteam steam)
        {
            steam.applySize(size);
            steam.contents[steam.index_group][steam.index_cell] = value;
            steam.index_cell += size;
        }
    }
}