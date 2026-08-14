/*
 * Description:             携程管理器
 * Author:                  TonyTang
 * Create Date:             2014/10/09
 */

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// CoroutineManager.cs
/// 协程管理器
/// </summary>
public class CoroutineManager : SingletonMonoTemplate<CoroutineManager>
{
    /// <summary>
    /// 内部辅助类
    /// </summary>
    private class CoroutineTask
    {
        /// <summary>
        /// 协程ID
        /// </summary>
        public long Id
        {
            get;
            set;
        }

        /// <summary>
        /// 协程是否正在运行
        /// </summary>
        public bool Running
        {
            get;
            set;
        }

        /// <summary>
        /// 协程是否暂停
        /// </summary>
        public bool Paused
        {
            get;
            set;
        }

        public CoroutineTask(long id)
        {
            Id = id;
            Running = true;
            Paused = false;
        }

        /// <summary>
        /// 协程包装器
        /// </summary>
        /// <param name="co"></param>
        /// <returns></returns>
        public IEnumerator CoroutineWrapper(IEnumerator co)
        {
            IEnumerator coroutine = co;
            while (Running)
            {
                if (Paused)
                    yield return null;
                else
                {
                    if (coroutine != null && coroutine.MoveNext())
                        yield return coroutine.Current;
                    else
                        Running = false;
                }
            }
            mCoroutines.Remove(Id);
        }
    }

    /// <summary>
    /// 协程管理器的协程字典
    /// </summary>
    private static Dictionary<long, CoroutineTask> mCoroutines;

    /// <summary>
    /// 当前正在使用的携程ID
    /// </summary>
    private long mNowId = 0;

    void Awake()
    {
        mCoroutines = new Dictionary<long, CoroutineTask>();
    }

    /// <summary>
    /// 获取一个新的携程ID
    /// </summary>
    /// <returns></returns>
    private long GetNewId()
    {
        mNowId++;
        return mNowId;
    }

    /// <summary>
    /// 启动一个协程
    /// </summary>
    /// <param name="co"></param>
    /// <returns></returns>
    public new long StartCoroutine(IEnumerator co)
    {
        if (gameObject.activeSelf)
        {
            CoroutineTask task = new CoroutineTask(GetNewId());
            mCoroutines.Add(task.Id, task);
            base.StartCoroutine(task.CoroutineWrapper(co));
            return task.Id;
        }
        return -1;
    }

    /// <summary>
    /// 停止一个协程
    /// </summary>
    /// <param name="id"></param>
    public void StopCoroutine(long id)
    {
        CoroutineTask task = mCoroutines[id];
        if (task != null)
        {
            task.Running = false;
            mCoroutines.Remove(id);
        }
    }

    /// <summary>
    /// 暂停协程的运行
    /// </summary>
    /// <param name="id"></param>
    public void PauseCoroutine(long id)
    {
        CoroutineTask task = mCoroutines[id];
        if (task != null)
        {
            task.Paused = true;
        }
        else
        {
            Debug.LogError("coroutine: " + id.ToString() + " is not exist!");
        }
    }

    /// <summary>
    /// 恢复协程的运行
    /// </summary>
    /// <param name="id"></param>
    public void ResumeCoroutine(long id)
    {
        CoroutineTask task = mCoroutines[id];
        if (task != null)
        {
            task.Paused = false;
        }
        else
        {
            Debug.LogError( "coroutine: " + id.ToString() + " is not exist!" );
        }
    }

    /// <summary>
    /// 延迟调用
    /// </summary>
    /// <param name="delayedTime"></param>
    /// <param name="callback"></param>
    /// <returns></returns>
    public long DelayedCall(float delayedTime, Action callback)
    {
        return StartCoroutine(DelayedCallImpl(delayedTime, callback));
    }

    /// <summary>
    /// 延迟调用
    /// </summary>
    /// <param name="delayedTime"></param>
    /// <param name="callback"></param>
    /// <returns></returns>
    private IEnumerator DelayedCallImpl(float delayedTime, Action callback)
    {
        if (delayedTime >= 0)
            yield return new WaitForSeconds(delayedTime);
        callback();
    }

    /// <summary>
    /// 延迟调用
    /// </summary>
    /// <param name="delayedTime"></param>
    /// <param name="callback"></param>
    /// <param name="param"></param>
    /// <returns></returns>
    public long DelayedCall(float delayedTime, Action<object> callback, object param)
    {
        return StartCoroutine(DelayedCallImpl(delayedTime, callback, param));
    }

    /// <summary>
    /// 延迟调用
    /// </summary>
    /// <param name="delayedTime"></param>
    /// <param name="callback"></param>
    /// <param name="param"></param>
    /// <returns></returns>
    private IEnumerator DelayedCallImpl(float delayedTime, Action<object> callback, object param)
    {
        if (delayedTime >= 0)
            yield return new WaitForSeconds(delayedTime);
        callback(param);
    }

    void OnDestroy()
    {
        foreach (CoroutineTask task in mCoroutines.Values)
        {
            task.Running = false;
        }
        mCoroutines.Clear();
    }
}
