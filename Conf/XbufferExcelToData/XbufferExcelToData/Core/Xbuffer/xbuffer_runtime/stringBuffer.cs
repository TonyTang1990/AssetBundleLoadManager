/*
 * File Name:               stringBuffer.cs
 *
 * Description:             基本类型处理
 * Author:                  lisiyu <576603306@qq.com>
 * Create Date:             2017/10/25
 */

using System.Text;

namespace xbuffer
{
    public class stringBuffer
    {
        public unsafe static string deserialize(byte[] buffer, ref uint offset)
        {
            fixed (byte* ptr = buffer)
            {
                uint byteCount = uintBuffer.deserialize(buffer, ref offset);
                string value = Encoding.UTF8.GetString(buffer, (int)offset, (int)byteCount);
                offset += byteCount;
                return value;
            }
        }

        public unsafe static void serialize(string value, XSteam steam)
        {
            var bytes = Encoding.UTF8.GetBytes(value);
            uintBuffer.serialize((uint)bytes.Length, steam);
            for (int i = 0; i < bytes.Length; i++)
            {
                steam.applySize(1);
                steam.contents[steam.index_group][steam.index_cell] = bytes[i];
                steam.index_cell += 1;
            }
        }
    }
}