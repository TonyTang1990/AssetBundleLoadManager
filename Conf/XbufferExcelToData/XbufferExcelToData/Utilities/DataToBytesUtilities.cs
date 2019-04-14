/*
 * Description:             转换数据到bytes数组静态工具类
 * Author:                  tanghuan
 * Create Date:             2018/09/03
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace XbufferExcelToData
{
    /// <summary>
    /// 转换数据到bytes数组静态工具类
    /// </summary>
    public static class DataToBytesUtilities
    {

        #region 数据Bytes转换
        /// <summary>
        /// 获取int数据的bytes
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static byte[] GetIntBytes(int data)
        {
            byte[] bytes = BitConverter.GetBytes(data);
            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(bytes);
            }
            return bytes;
        }        
        #endregion

    }
}
