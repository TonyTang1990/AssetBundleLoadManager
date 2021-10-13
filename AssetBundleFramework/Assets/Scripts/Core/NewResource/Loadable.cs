/*
 * Description:             Loadable.cs
 * Author:                  TONYTANG
 * Create Date:             2021/10/13
 */

// 加载器流程设计:
// 1. 

namespace TResource
{
    /// <summary>
    /// Loadable.cs
    /// 加载器基类抽象
    /// </summary>
    public abstract class Loadable : IRecycle
    {
        public virtual void onCreate()
        {

        }

        public virtual void onDispose()
        {

        }
    }
}