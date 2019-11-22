
/*
 * file ObjectFactory.cs
 *
 * author: ***
 * date: 2014/11/11   
 */

using System.Collections.Generic;


/// <summary>
/// 工厂对象必须实现的接口
/// </summary>
public interface FactoryObj
{
    void recycle();
}


/// <summary>
/// 对象工厂模版（用于减少频繁的new操作）
/// </summary>
/// <typeparam name="T"></typeparam>
public class ObjectFactory<T> where T : FactoryObj, new()
{

    protected static Stack<T> mFreeList = new Stack<T>();

    /// <summary>
    /// 初始化
    /// </summary>
    /// <param name="size"></param>
    public static void initialize(int size)
    {
        for (int a = 0; a < size; ++a)
        {
            mFreeList.Push(new T());
        }
    }

    /// <summary>
    /// 创建对象
    /// </summary>
    /// <returns></returns>
    public static T create()
    {
        if (mFreeList.Count <= 0)
        {
            for (int a = 0; a < DefaultSize; ++a)
            {
                mFreeList.Push(new T());
            }
        }

        if (mFreeList.Count <= 0)
        {
            return default(T);
        }

        return mFreeList.Pop();
    }

    /// <summary>
    /// 回收对象
    /// </summary>
    /// <param name="obj"></param>
    public static void recycle(T obj)
    {
        obj.recycle();
        mFreeList.Push(obj);
    }

    /// <summary>
    /// 默认大小
    /// </summary>
    public static int DefaultSize = 10;

}

