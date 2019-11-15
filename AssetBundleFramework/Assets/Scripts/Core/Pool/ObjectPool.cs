/*
 * Description:             ObjectPool.cs
 * Author:                  TonyTang
 * Create Date:             2019/09/01
 */

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 对象可回收接口设计
/// </summary>
public interface IRecycle
{
    /// <summary>
    /// 创建时调用接口
    /// </summary>
    void onCreate();

    /// <summary>
    /// 回收时调用接口
    /// </summary>
    void onDispose();
}

/// <summary>
/// ObjectPool.cs
/// Object对象池
/// Note:
/// 用于替代ObejectFactory,相比ObjectFactory,
/// ObjectPool面向接口设计，同时把对象放在全局唯一的Pool里管理,避免创建多个类型的Factory文件
/// </summary>
public class ObjectPool
{
    /// <summary>
    /// 单例管理池
    /// </summary>
    public readonly static ObjectPool Singleton = new ObjectPool();

    /// <summary>
    /// 对象管理池
    /// </summary>
    private Dictionary<int, Stack<IRecycle>> ObjectPoolMap;

    private ObjectPool()
    {
        ObjectPoolMap = new Dictionary<int, Stack<IRecycle>>();
    }

    /// <summary>
    /// 初始化指定数量的指定对象
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="number"></param>
    public void initialize<T>(int number) where T : IRecycle
    {
        for(int i = 0; i < number; i++)
        {
            var obj = Activator.CreateInstance<T>();
            push<T>(obj);
        }
        var hashcode = typeof(T).GetHashCode();
        //DIYLog.Log(string.Format("初始化类型:{0}的剩余数量:{1}", typeof(T).Name, ObjectPoolMap[hashcode].Count));
    }

    /// <summary>
    /// 指定对象进池
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="obj"></param>
    public void push<T>(T obj) where T : IRecycle
    {
        obj.onDispose();
        var hashcode = typeof(T).GetHashCode();
        if(!ObjectPoolMap.ContainsKey(hashcode))
        {
            ObjectPoolMap.Add(hashcode, new Stack<IRecycle>());
        }
        ObjectPoolMap[hashcode].Push(obj);
        //DIYLog.Log(string.Format("类型:{0}进对象池!",typeof(T).Name));
        //DIYLog.Log(string.Format("池里类型:{0}的剩余数量:{1}", typeof(T).Name, ObjectPoolMap[hashcode].Count));
    }

    /// <summary>
    /// 弹出可用指定对象
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public T pop<T>() where T : IRecycle
    {
        var hashcode = typeof(T).GetHashCode();
        if (ObjectPoolMap.ContainsKey(hashcode))
        {
            var instance = ObjectPoolMap[hashcode].Pop();
            instance.onCreate();
            //DIYLog.Log(string.Format("类型:{0}出对象池!", typeof(T).Name));
            //DIYLog.Log(string.Format("池里类型:{0}的剩余数量:{1}", typeof(T).Name, ObjectPoolMap[hashcode].Count));
            if (ObjectPoolMap[hashcode].Count == 0)
            {
                clear<T>();
            }
            return (T)instance;
        }
        else
        {
            //DIYLog.Log(string.Format("类型:{0}构建新的对象!", typeof(T).Name));
            // 默认池里没有反射创建,尽量避免反射创建，
            // 可以考虑调用Initialize初始化一定数量进池
            return Activator.CreateInstance<T>();
        }
    }

    /// <summary>
    /// 清除指定类型的对象缓存
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public bool clear<T>() where T : IRecycle
    {
        var hashcode = typeof(T).GetHashCode();
        if(ObjectPoolMap.ContainsKey(hashcode))
        {
            //DIYLog.Log(string.Format("清除对象池里的类型:{0}", typeof(T).Name));
            ObjectPoolMap.Remove(hashcode);
            return true;
        }
        else
        {
            return false;
        }
    }

    /// <summary>
    /// 清除所有对象缓存
    /// </summary>
    public void clearAll()
    {
        ObjectPoolMap.Clear();
    }
}