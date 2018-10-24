/*
 * file CoroutineManager.cs
 *
 * author: Pengmian
 * date:   2014/10/9
 */

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CoroutineManager : MonoBehaviour
{
    /// <summary>
    /// 内部辅助类
    /// </summary>
    private class CoroutineTask
    {
        public long Id { get; set; }
        public bool Running { get; set; }
        public bool Paused { get; set; }

        public CoroutineTask(long id)
        {
            Id = id;
            Running = true;
            Paused = false;
        }

        public IEnumerator coroutineWrapper(IEnumerator co)
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

    private static Dictionary<long, CoroutineTask> mCoroutines;
    public static CoroutineManager Singleton { get; private set; }

    void Awake()
    {
        Singleton = this;
        mCoroutines = new Dictionary<long, CoroutineTask>();
    }

    private long mNowId = 0;
    private long getNewId()
    {
        mNowId++;
        while(mCoroutines.ContainsKey(mNowId))
            mNowId++;
        return mNowId;
    }

    /// <summary>
    /// 启动一个协程
    /// </summary>
    /// <param name="co"></param>
    /// <returns></returns>
    public Int64 startCoroutine(IEnumerator co)
    {

        if (this.gameObject.activeSelf)
        {
            CoroutineTask task = new CoroutineTask(getNewId());
            mCoroutines.Add(task.Id, task);
            StartCoroutine(task.coroutineWrapper(co));
            return task.Id;
        }
        return -1;
    }

    /// <summary>
    /// 停止一个协程
    /// </summary>
    /// <param name="id"></param>
    public void stopCoroutine(long id)
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
    public void pauseCoroutine(long id)
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
    public void resumeCoroutine(long id)
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

    public long delayedCall(float delayedTime, Action callback)
    {
        return startCoroutine(delayedCallImpl(delayedTime, callback));
    }

    private IEnumerator delayedCallImpl(float delayedTime, Action callback)
    {
        if (delayedTime >= 0)
            yield return new WaitForSeconds(delayedTime);
        callback();
    }


    public long delayedCall(float delayedTime, Action<object> callback, object param)
    {
        return startCoroutine(delayedCallImpl(delayedTime, callback, param));
    }

    private IEnumerator delayedCallImpl(float delayedTime, Action<object> callback, object param)
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
