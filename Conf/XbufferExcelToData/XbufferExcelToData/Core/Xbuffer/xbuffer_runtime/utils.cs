/*
 * File Name:               utils.cs
 *
 * Description:             基本工具集 用于处理大小端等逻辑
 * Author:                  lisiyu <576603306@qq.com>
 * Create Date:             2017/10/25
 */

namespace xbuffer
{
    public class utils
    {
        public static uint toLittleEndian(uint value)
        {
            return ((value & 0x000000FFU) << 24) |
                    ((value & 0x0000FF00U) << 8) |
                    ((value & 0x00FF0000U) >> 8) |
                    ((value & 0xFF000000U) >> 24);
        }

        public static ulong toLittleEndian(ulong value)
        {
            return (((value & 0x00000000000000FFUL) << 56) |
                    ((value & 0x000000000000FF00UL) << 40) |
                    ((value & 0x0000000000FF0000UL) << 24) |
                    ((value & 0x00000000FF000000UL) << 8) |
                    ((value & 0x000000FF00000000UL) >> 8) |
                    ((value & 0x0000FF0000000000UL) >> 24) |
                    ((value & 0x00FF000000000000UL) >> 40) |
                    ((value & 0xFF00000000000000UL) >> 56));
        }
    }
}