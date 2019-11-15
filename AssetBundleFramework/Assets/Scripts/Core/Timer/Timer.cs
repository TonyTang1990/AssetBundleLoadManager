/*
 * Description:             Timer.cs
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
public class Timer
{
    /// <summary>
    /// 单例对象(相比SingletonTemplate,可以避免默认构造函数是public)
    /// </summary>
    public readonly static Timer Singleton = new Timer();

    /// <summary>
    /// Timer数据
    /// </summary>
    private class TimerData : IRecycle
    {
        /// <summary>
        /// 唯一ID
        /// </summary>
        public long UID
        {
            get;
            set;
        }

        /// <summary>
        /// 是否暂停单个定时器
        /// </summary>
        public bool IsPause
        {
            get;
            set;
        }

        /// <summary>
        /// 需要触发的回调
        /// </summary>
        public Action CallBack
        {
            get;
            set;
        }

        /// <summary>
        /// 延时时间
        /// </summary>
        public float DelayTime
        {
            get;
            set;
        }

        /// <summary>
        /// 自定义触发条件
        /// </summary>
        public Func<bool> TriggerCondition
        {
            get;
            set;
        }

        /// <summary>
        /// 经过的时间
        /// </summary>
        public float TimePassed
        {
            get;
            set;
        }

        public TimerData()
        {
            UID = 0;
            CallBack = null;
            DelayTime = 0f;
            TriggerCondition = null;
            TimePassed = 0f;
        }

        public TimerData(long uid, Action callback, float delaytime = 0, Func<bool> triggeercondition = null)
        {
            UID = uid;
            CallBack = callback;
            DelayTime = delaytime;
            TriggerCondition = triggeercondition;
            TimePassed = 0f;
        }

        /// <summary>
        /// 更新TimerData
        /// </summary>
        /// <param name="timepassed"></param>
        /// <returns></returns>
        public bool update(float timepassed)
        {
            if(!IsPause)
            {
                TimePassed += timepassed;
                if(TimePassed >= DelayTime)
                {
                    if(TriggerCondition != null)
                    {
                        if(TriggerCondition.Invoke())
                        {
                            CallBack.Invoke();
                            return true;
                        }
                    }
                    else
                    {
                        CallBack.Invoke();
                        return true;
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// 对象池弹出创建时
        /// </summary>
        public void onCreate()
        {
            UID = 0;
            CallBack = null;
            DelayTime = 0f;
            TriggerCondition = null;
            TimePassed = 0f;
        }

        /// <summary>
        /// 对象池回收时
        /// </summary>
        public void onDispose()
        {
            UID = 0;
            CallBack = null;
            DelayTime = 0f;
            TriggerCondition = null;
            TimePassed = 0f;
        }
    }

    /// <summary>
    /// 是否暂停所有计时器
    /// </summary>
    public bool IsPause
    {
        get;
        set;
    }

    /// <summary>
    /// Timer的唯一ID
    /// </summary>
    private long mTimerUID;

    /// <summary>
    /// 需要判定的计时数据
    /// </summary>
    private Dictionary<long, TimerData> TimerDataMap;

    /// <summary>
    /// 需要清除的定时器列表
    /// </summary>
    private List<long> mClearTimerList;

    /// <summary>
    /// 延迟一帧添加的Timer定时数据(为了避免Update的时候也在添加Timer造成foreach无法完成遍历)
    /// </summary>
    private Dictionary<long, TimerData> mLaterAddedTimerMap;

    private Timer()
    {
        mTimerUID = 0;
        TimerDataMap = new Dictionary<long, TimerData>();
        mClearTimerList = new List<long>();
        mLaterAddedTimerMap = new Dictionary<long, TimerData>();
    }

    /// <summary>
    /// 添加定时器
    /// </summary>
    /// <param name="callback"></param>
    /// <param name="delaytime"></param>
    /// <param name="triggeercondition"></param>
    /// <returns></returns>
    public long addTimer(Action callback, float delaytime = 0, Func<bool> triggeercondition = null)
    {
        var newtimeruid = getNewTimerUID();
        //DIYLog.Log(string.Format("添加UID定时器:{0}", newtimeruid));
        var timerdata = ObjectPool.Singleton.pop<TimerData>();
        timerdata.UID = newtimeruid;
        timerdata.CallBack = callback;
        timerdata.DelayTime = delaytime;
        timerdata.TriggerCondition = triggeercondition;
        timerdata.TimePassed = 0;
        mLaterAddedTimerMap.Add(newtimeruid, timerdata);
        return newtimeruid;
    }

    /// <summary>
    /// 移除定时器
    /// </summary>
    /// <param name="uid"></param>
    /// <returns></returns>
    public bool removeTimer(long uid)
    {
        if (TimerDataMap.ContainsKey(uid))
        {
            //DIYLog.Log(string.Format("移除UID定时器:{0}", uid));
            var timerdata = TimerDataMap[uid];
            ObjectPool.Singleton.push<TimerData>(timerdata);
            TimerDataMap.Remove(uid);
            return true;
        }
        else if(mLaterAddedTimerMap.ContainsKey(uid))
        {
            //DIYLog.Log(string.Format("移除UID待添加定时器:{0}", uid));
            mLaterAddedTimerMap.Remove(uid);
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
        if(TimerDataMap.ContainsKey(uid))
        {
            //DIYLog.Log(string.Format("暂停UID定时器:{0}", uid));
            TimerDataMap[uid].IsPause = true;
            return true;
        }
        else if (mLaterAddedTimerMap.ContainsKey(uid))
        {
            //DIYLog.Log(string.Format("暂停待添加UID定时器:{0}", uid));
            mLaterAddedTimerMap[uid].IsPause = true;
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
        if (TimerDataMap.ContainsKey(uid))
        {
            //DIYLog.Log(string.Format("继续UID定时器:{0}", uid));
            TimerDataMap[uid].IsPause = false;
            return true;
        }
        else if (mLaterAddedTimerMap.ContainsKey(uid))
        {
            //DIYLog.Log(string.Format("继续待添加UID定时器:{0}", uid));
            mLaterAddedTimerMap[uid].IsPause = false;
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
        TimerDataMap.Clear();
        mLaterAddedTimerMap.Clear();
    }

    /// <summary>
    /// 更新所有定时器
    /// </summary>
	public void Update () {
		if(!IsPause)
        {
            //延时一帧添加(为了避免Update的时候也在添加Timer造成foreach无法完成遍历)
            if (mLaterAddedTimerMap.Count > 0)
            {
                foreach (var addedtimer in mLaterAddedTimerMap)
                {
                    TimerDataMap.Add(addedtimer.Key, addedtimer.Value);
                }
                mLaterAddedTimerMap.Clear();
            }
            mClearTimerList.Clear();
            foreach (var timerdata in TimerDataMap.Values)
            {
                if(timerdata.update(Time.deltaTime))
                {
                    mClearTimerList.Add(timerdata.UID);
                }
            }
            foreach(var cleartimeruid in mClearTimerList)
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
        return ++mTimerUID;
    }
}
