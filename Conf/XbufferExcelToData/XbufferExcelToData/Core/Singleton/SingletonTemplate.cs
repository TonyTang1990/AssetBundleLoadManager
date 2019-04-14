/*
 * Description:             单例模板类
 * Author:                  tanghuan
 * Create Date:             2018/09/02
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace XbufferExcelToData
{
    /// <summary>
    /// 模板单例
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class SingletonTemplate<T> where T : class, new()
    {
        public static T Singleton
        {
            get
            {
                if (mSingleton == null)
                {
                    mSingleton = new T();
                }
                return mSingleton;
            }
        }
        protected static T mSingleton = null;

        protected SingletonTemplate()
        {

        }
    }

}
