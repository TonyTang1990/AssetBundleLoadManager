/*
 * Description:             SingletonManager.cs
 * Author:                  TONYTANG
 * Create Date:             2026//08/14
 */
using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// SingletonManager.cs
/// 单例管理器
/// </summary>
public static class SingletonManager
{
    /// <summary>
    /// 单例对象字典(Key为单例类型,Value为单例对象)
    /// </summary>
    private static readonly Dictionary<Type, ISingleton> Instances = new();

    /// <summary>
    /// 注册单例对象
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="instance"></param>
    /// <exception cref="ArgumentNullException"></exception>
    /// <exception cref="InvalidOperationException"></exception>
    public static void Register<T>(T instance) where T : class, ISingleton
    {
        if (instance == null)
        {
            throw new ArgumentNullException(nameof(instance));
        }

        Type type = typeof(T);

        if (Instances.ContainsKey(type))
        {
            throw new InvalidOperationException($"{type.Name} 已经注册，不能重复创建。");
        }

        Instances.Add(type, instance);
        instance.Initialize();
    }

    /// <summary>
    /// 获取单例对象
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    public static T Get<T>() where T : class, ISingleton
    {
        if (Instances.TryGetValue(typeof(T), out ISingleton instance))
        {
            return (T)instance;
        }

        throw new InvalidOperationException($"{typeof(T).Name} 尚未初始化。");
    }

    /// <summary>
    /// 释放指定类型的单例对象
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public static bool Shautdown<T>() where T : class, ISingleton
    {
        var type = typeof(T);
        if (Instances.TryGetValue(type, out ISingleton instance))
        {
            instance.Shutdown();
            Instances.Remove(type);
            return true;
        }
        Debug.LogError($"{type.Name} 尚未注册，无法释放。");
        return false;
    }

    /// <summary>
    /// 释放所有单例对象
    /// </summary>
    public static void ShutdownAll()
    {
        var instances = new List<ISingleton>(Instances.Values);

        // 通常按照初始化的相反顺序销毁
        for (int i = instances.Count - 1; i >= 0; i--)
        {
            instances[i].Shutdown();
        }

        Instances.Clear();
    }
}