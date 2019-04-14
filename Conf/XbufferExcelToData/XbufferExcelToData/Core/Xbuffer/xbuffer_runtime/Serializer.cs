/*
 * File Name:               Serializer.cs
 *
 * Description:             泛型接口
 * Author:                  lisiyu <576603306@qq.com>
 * Create Date:             2017/10/25
 */

using System;

namespace xbuffer
{
    public class Serializer
    {
        public static XSteam cachedSteam;

        public static void serialize<T>(T value)
        {
            if (cachedSteam == null)
                throw new NullReferenceException();

            cachedSteam.index_cell = 0;
            cachedSteam.index_group = 0;

            var bufferType = typeof(T).Assembly.GetType(string.Format("xbuffer.{0}Buffer", typeof(T).FullName));
            var method = bufferType.GetMethod("serialize", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public);
            var args = new object[] { value, cachedSteam };
            method.Invoke(null, args);
        }


        public static T deserialize<T>(byte[] buffer)
        {
            uint offset = 0;
            return deserialize<T>(buffer, ref offset);
        }

        public static T deserialize<T>(byte[] buffer, ref uint offset)
        {
            var bufferType = typeof(T).Assembly.GetType(string.Format("xbuffer.{0}Buffer", typeof(T).FullName));
            var method = bufferType.GetMethod("deserialize", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public);
            var args = new object[] { buffer, offset };
            var ret = (T)method.Invoke(null, args);
            offset = (uint)args[1];
            return ret;
        }
    }
}