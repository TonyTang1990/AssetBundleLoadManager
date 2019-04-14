/*
 * File Name:               XSteam.cs
 *
 * Description:             一个简单的内存流实现 用于精准的控制内存管理
 * Author:                  lisiyu <576603306@qq.com>
 * Create Date:             2018/04/09
 */

namespace xbuffer
{
    public class XSteam
    {
        public uint index_group;            // 当前组别序号 行
        public uint index_cell;             // 当前单元序号 列

        public uint capacity_group;         // 行的数量
        public uint capacity_cell;          // 列的数量
        public byte[][] contents;           // 内容列表
        public uint[] wastes;               // 浪费列表 对应每一行

        public XSteam(uint capacity_group, uint capacity_cell)
        {
            this.capacity_group = capacity_group;
            this.capacity_cell = capacity_cell;

            contents = new byte[capacity_group][];
            for (int i = 0; i < capacity_group; i++)
            {
                contents[i] = new byte[capacity_cell];
            }
            wastes = new uint[capacity_group];
        }

        /// <summary>
        /// 申请空间
        /// 基本策略是不够了就申请一倍
        /// </summary>
        /// <param name="size"></param>
        public void applySize(uint size)
        {
            if (index_cell + size > capacity_cell)
            {
                wastes[index_group] = capacity_cell - index_cell;
                index_cell = 0;
                index_group++;
                if (index_group >= capacity_group)
                {
                    var nCapacity_Group = capacity_group + 1;

                    var nContents = new byte[nCapacity_Group][];
                    for (uint i = 0; i < nCapacity_Group; i++)
                    {
                        if (i < capacity_group)
                            nContents[i] = contents[i];
                        else
                            nContents[i] = new byte[capacity_cell];
                    }
                    contents = nContents;

                    var nWastes = new uint[nCapacity_Group];
                    for (uint i = 0; i < capacity_group; i++)
                    {
                        nWastes[i] = wastes[i];
                    }
                    wastes = nWastes;

                    capacity_group = nCapacity_Group;
                }
            }
        }

        /// <summary>
        /// 返回输出字节流
        /// </summary>
        /// <returns></returns>
        public byte[] getBytes()
        {
            var len = index_group * capacity_cell + index_cell;
            for (int i = 0; i < index_group; i++)
            {
                len -= wastes[i];
            }

            var ret = new byte[len];
            var idx = 0;
            for (int i = 0; i < index_group; i++)
            {
                for (int j = 0; j < capacity_cell - wastes[i]; j++)
                {
                    ret[idx++] = contents[i][j];
                }
            }
            for (int i = 0; i < index_cell; i++)
            {
                ret[idx++] = contents[index_group][i];
            }

            return ret;
        }
    }
}