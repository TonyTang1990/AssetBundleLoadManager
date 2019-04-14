/*
 * File Name:               floatBuffer.cs
 *
 * Description:             基本类型处理
 * Author:                  lisiyu <576603306@qq.com>
 * Create Date:             2017/10/25
 */

using System;

namespace xbuffer
{
    public class floatBuffer
    {
        private static readonly uint size = sizeof(float);

        //-------------------------------------------------
        // float在ARM机器上dereferencing浮点数(float,double等)强制要求内存4字节对齐，不然会报空
        // 参考链接:
        // https://stackoverflow.com/questions/28436327/monotouch-floating-point-pointer-throws-nullreferenceexception-when-not-4-byte-a
        //-------------------------------------------------

        // 修复方案，定义一个全局的4字节byte数组，用于不满足内存4字节对齐时赋值用于解析float
        private static readonly byte[] fourByteAlginedArray = new byte[4];

        public unsafe static float deserialize(byte[] buffer, ref uint offset)
        {
            fixed (byte* ptr = buffer)
            {
                float value;
                if ((int)(ptr + offset) % 4 == 0)
                {
                    value = *(float*)(ptr + offset);
                }
                else
                {
                    for (int i = 0; i < 4; i++)
                    {
                        fourByteAlginedArray[i] = (ptr + offset)[i];
                    }
                    fixed (byte* ptr2 = fourByteAlginedArray)
                    {
                        value = *(float*)(ptr2);
                    }
                }
                offset += size;
                return BitConverter.IsLittleEndian ? value : utils.toLittleEndian((uint)value);
            }
        }

        public unsafe static void serialize(float value, XSteam steam)
        {
            steam.applySize(size);
            fixed (byte* ptr = steam.contents[steam.index_group])
            {
                *(float*)(ptr + steam.index_cell) = BitConverter.IsLittleEndian ? value : utils.toLittleEndian((uint)value);
                steam.index_cell += size;
            }
        }
    }
}