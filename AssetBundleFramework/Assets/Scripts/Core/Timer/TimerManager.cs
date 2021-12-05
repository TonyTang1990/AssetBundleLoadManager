/*
 * Description:             TimerManager.cs
 * Author:                  TONYTANG
 * Create Date:             2019/09/01
 */

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 不依赖于Monobehaviour的定时器(类似携程)
/// 支持延时+自定义条件的触发形式
/// </summary>
public class TimerManager
{
    /// <summary>
    /// 单例对象(相比SingletonTemplate,可以避免默认构造函数是public)
    /// </summary>
    public readonly static TimerManager Singleton = new TimerManager();

    /// <summary>
    /// 是否暂停所有计时器
    /// </summary>
    public bool IsPauseAllTimer
    {
        get;
        set;
    }

    /// <summary>
    /// 下一个有效的Timer UID
    /// </summary>
    private long mNeextValideTimerUID;

    /// <summary>
    /// 需要判定的计时数据
    /// </summary>
    private Dictionary<long, Timer> TimerMap;

    /// <summary>
    /// 需要清除的Update定时器列表
    /// </summary>
    private List<long> mClearUpdateTimerList;

    /// <summary>
    /// 需要清除的FixedUpdate定时器列表
    /// </summary>
    private List<long> mClearFixedUpdateTimerList;

    /// <summary>
    /// 延迟一帧添加的Update Timer定时数据(为了避免Update的时候也在添加Timer造成foreach无法完成遍历)
    /// </summary>
    private Dictionary<long, Timer> mLaterAddedUpdateTimerMap;

    /// <summary>
    /// 延迟一帧添加的FixedUpdate Timer定时数据(为了避免FixedUpdate的时候也在添加Timer造成foreach无法完成遍历)
    /// </summary>
    private Dictionary<long, Timer> mLaterAddedFixedUpdateTimerMap;

    private TimerManager()
    {
        mNeextValideTimerUID = 0;
        TimerMap = new Dictionary<long, Timer>();
        mClearUpdateTimerList = new List<long>();
        mClearFixedUpdateTimerList = new List<long>();
        mLaterAddedUpdateTimerMap = new Dictionary<long, Timer>();
        mLaterAddedFixedUpdateTimerMap = new Dictionary<long, Timer>();
    }

    /// <summary>
    /// 添加定时器(Update驱动)
    /// </summary>
    /// <param name="callback"></param>
    /// <param name="delaytime"></param>
    /// <param name="triggeercondition"></param>
    /// <returns></returns>
    public Timer addUpdateTimer(Action callback, float delaytime = 0, Func<bool> triggeercondition = null)
    {
        var newtimeruid = getNewTimerUID();
        //DIYLog.Log(string.Format("添加UID定时器:{0}", newtimeruid));
        var timer = ObjectPool.Singleton.pop<Timer>();
        timer.setData(newtimeruid, callback, delaytime, true, triggeercondition);
        mLaterAddedUpdateTimerMap.Add(newtimeruid, timer);
        return timer;
    }

    /// <summary>
    /// 添加定时器(FixedUpdate驱动)
    /// </summary>
    /// <param name="callback"></param>
    /// <param name="delaytime"></param>
    /// <param name="triggeercondition"></param>
    /// <returns></returns>
    public Timer addFixedUpdateTimer(Action callback, float delaytime = 0, Func<bool> triggeercondition = null)
    {
        var newtimeruid = getNewTimerUID();
        //DIYLog.Log(string.Format("添加UID定时器:{0}", newtimeruid));
        var timer = ObjectPool.Singleton.pop<Timer>();
        timer.setData(newtimeruid, callback, delaytime, false, triggeercondition);
        mLaterAddedFixedUpdateTimerMap.Add(newtimeruid, timer);
        return timer;
    }

    /// <summary>
    /// 移除定时器
    /// </summary>
    /// <param name="uid"></param>
    /// <returns></returns>
    public bool removeTimer(long uid)
    {
        if (TimerMap.ContainsKey(uid))
        {
            //DIYLog.Log(string.Format("移除UID定时器:{0}", uid));
            var timerdata = TimerMap[uid];
            ObjectPool.Singleton.push<Timer>(timerdata);
            TimerMap.Remove(uid);
            return true;
        }
        else if (mLaterAddedUpdateTimerMap.ContainsKey(uid))
        {
            //DIYLog.Log(string.Format("移除UID待添加Update定时器:{0}", uid));
            mLaterAddedUpdateTimerMap.Remove(uid);
            return true;
        }
        else if (mLaterAddedFixedUpdateTimerMap.ContainsKey(uid))
        {
            //DIYLog.Log(string.Format("移除UID待添加FixedUpdate定时器:{0}", uid));
            mLaterAddedFixedUpdateTimerMap.Remove(uid);
            return true;
        }
        else
        {
            //DIYLog.LogError(string.Format("无效的Timer UID:{0},无法移除Timer!", uid));
            return false;
        }
    }

    /// <summary>
    /// 暂停指定定时器
    /// </summary>
    /// <param name="uid"></param>
    /// <returns></returns>
    public bool pauseTimer(long uid)
    {
        if (TimerMap.ContainsKey(uid))
        {
            //DIYLog.Log(string.Format("暂停UID定时器:{0}", uid));
            TimerMap[uid].pause();
            return true;
        }
        else if (mLaterAddedUpdateTimerMap.ContainsKey(uid))
        {
            //DIYLog.Log(string.Format("暂停待添加UID Update定时器:{0}", uid));
            mLaterAddedUpdateTimerMap[uid].pause();
            return true;
        }

        else if (mLaterAddedFixedUpdateTimerMap.ContainsKey(uid))
        {
            //DIYLog.Log(string.Format("暂停待添加UID FixedUpdate定时器:{0}", uid));
            mLaterAddedFixedUpdateTimerMap[uid].pause();
            return true;
        }
        else
        {
            //DIYLog.LogError(string.Format("无效的Timer UID:{0},无法暂停Timer!", uid));
            return false;
        }
    }

    /// <summary>
    /// 继续指定定时器
    /// </summary>
    /// <param name="uid"></param>
    /// <returns></returns>
    public bool resumeTimer(long uid)
    {
        if (TimerMap.ContainsKey(uid))
        {
            //DIYLog.Log(string.Format("继续UID定时器:{0}", uid));
            TimerMap[uid].resume();
            return true;
        }
        else if (mLaterAddedUpdateTimerMap.ContainsKey(uid))
        {
            //DIYLog.Log(string.Format("继续待添加UID Update定时器:{0}", uid));
            mLaterAddedUpdateTimerMap[uid].resume();
            return true;
        }
        else if (mLaterAddedFixedUpdateTimerMap.ContainsKey(uid))
        {
            //DIYLog.Log(string.Format("继续待添加UID FixedUpdate定时器:{0}", uid));
            mLaterAddedFixedUpdateTimerMap[uid].resume();
            return true;
        }
        else
        {
            //DIYLog.LogError(string.Format("无效的Timer UID:{0},无法继续Timer!", uid));
            return false;
        }
    }

    /// <summary>
    /// 清除所有定时器
    /// </summary>
    public void clearAllTimer()
    {
        //DIYLog.Log("清除所有定时器");
        TimerMap.Clear();
        mClearUpdateTimerList.Clear();
        mClearFixedUpdateTimerList.Clear();
        mLaterAddedUpdateTimerMap.Clear();
        mLaterAddedFixedUpdateTimerMap.Clear();
    }

    /// <summary>
    /// Update更新所有定时器
    /// </summary>
    /// <param name="deltatime"></param>
	public void update(float deltatime)
    {
        if (!IsPauseAllTimer)
        {
            mClearUpdateTimerList.Clear();
            //延时一帧添加(为了避免Update的时候也在添加Timer造成foreach无法完成遍历)
            if (mLaterAddedUpdateTimerMap.Count > 0)
            {
                foreach (var addedtimer in mLaterAddedUpdateTimerMap)
                {
                    if (!addedtimer.Value.isOver())
                    {
                        TimerMap.Add(addedtimer.Value.UID, addedtimer.Value);
                    }
                    else
                    {
                        mClearUpdateTimerList.Add(addedtimer.Value.UID);
                    }
                }
                mLaterAddedUpdateTimerMap.Clear();
            }
            foreach (var timer in TimerMap.Values)
            {
                if (!timer.isOver())
                {
                    timer.update(deltatime);
                }
                else
                {
                    mClearUpdateTimerList.Add(timer.UID);
                }
            }
            foreach (var cleartimeruid in mClearUpdateTimerList)
            {
                removeTimer(cleartimeruid);
            }
        }
    }

    /// <summary>
    /// fixedUpdate更新所有定时器
    /// </summary>
    /// <param name="fixeddeltatime"></param>
	public void fixedUpdate(float fixeddeltatime)
    {
        if (!IsPauseAllTimer)
        {
            mClearFixedUpdateTimerList.Clear();
            //延时一帧添加(为了避免FixedUpdate的时候也在添加Timer造成foreach无法完成遍历)
            if (mLaterAddedFixedUpdateTimerMap.Count > 0)
            {
                foreach (var addedtimer in mLaterAddedFixedUpdateTimerMap)
                {
                    if (!addedtimer.Value.isOver())
                    {
                        TimerMap.Add(addedtimer.Value.UID, addedtimer.Value);
                    }
                    else
                    {
                        mClearFixedUpdateTimerList.Add(addedtimer.Value.UID);
                    }
                }
                mLaterAddedFixedUpdateTimerMap.Clear();
            }
            foreach (var timer in TimerMap.Values)
            {
                if (!timer.isOver())
                {
                    timer.fixedUpdate(fixeddeltatime);
                }
                else
                {
                    mClearFixedUpdateTimerList.Add(timer.UID);
                }
            }
            foreach (var cleartimeruid in mClearFixedUpdateTimerList)
            {
                removeTimer(cleartimeruid);
            }
        }
    }

    /// <summary>
    /// 得到一个最新的定时器UID
    /// </summary>
    /// <returns></returns>
    private long getNewTimerUID()
    {
        return ++mNeextValideTimerUID;
    }
}
