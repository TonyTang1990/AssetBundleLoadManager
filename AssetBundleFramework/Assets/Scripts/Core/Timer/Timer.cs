/*
 * Description:             Timer.cs
 * Author:                  TANGHUAN
 * Create Date:             2021/02/07
 */

using System;

// TODO:
// 支持每隔一段时间触发调用的Timer

/// <summary>
/// Timer数据
/// </summary>
public class Timer : IRecycle
{
    /// <summary>
    /// 唯一ID
    /// </summary>
    public long UID
    {
        get;
        protected set;
    }

    /// <summary>
    /// 是否暂停单个定时器
    /// </summary>
    protected bool mIsPause;

    /// <summary>
    /// 需要触发的回调
    /// </summary>
    protected Action mCallBack;

    /// <summary>
    /// 延时时间
    /// </summary>
    protected float mDelayTime;

    /// <summary>
    /// 是否是Update Timer反之为FixedUpdate Timer
    /// </summary>
    protected bool mIsUpdate;

    /// <summary>
    /// 自定义触发条件
    /// </summary>
    protected Func<bool> mTriggerCondition;

    /// <summary>
    /// 经过的时间
    /// </summary>
    protected float mTimePassed;

    /// <summary>
    /// 是否结束
    /// </summary>
    protected bool mIsOver;

    public Timer()
    {
        UID = 0;
        mCallBack = null;
        mDelayTime = 0f;
        mIsUpdate = true;
        mTriggerCondition = null;
        mTimePassed = 0f;
        mIsPause = false;
        mIsOver = false;
    }

    /// <summary>
    /// 设置Timer数据
    /// </summary>
    /// <param name="uid"></param>
    /// <param name="callback"></param>
    /// <param name="delaytime"></param>
    /// <param name="useupdate"></param>
    /// <param name="triggeercondition"></param>
    public void setData(long uid, Action callback, float delaytime = 0, bool useupdate = true, Func<bool> triggeercondition = null)
    {
        UID = uid;
        mCallBack = callback;
        mDelayTime = delaytime;
        mIsUpdate = useupdate;
        mTriggerCondition = triggeercondition;
        mTimePassed = 0f;
        mIsPause = false;
        mIsOver = false;
    }

    /// <summary>
    /// 暂停Timer
    /// </summary>
    public void pause()
    {
        mIsPause = true;
    }

    /// <summary>
    /// 继续Timer
    /// </summary>
    public void resume()
    {
        mIsPause = false;
    }

    /// <summary>
    /// 停止计时器
    /// </summary>
    public void stop()
    {
        mIsOver = true;
    }

    /// <summary>
    /// Update更新TimerData
    /// </summary>
    /// <param name="timepassed"></param>
    /// <returns></returns>
    public void update(float timepassed)
    {
        if(mIsUpdate)
        {
            if (!mIsPause)
            {
                mTimePassed += timepassed;
                if (mTimePassed >= mDelayTime)
                {
                    if (mTriggerCondition != null)
                    {
                        if (mTriggerCondition.Invoke())
                        {
                            mIsOver = true;
                            mCallBack.Invoke();
                            return;
                        }
                    }
                    else
                    {
                        mIsOver = true;
                        mCallBack.Invoke();
                        return;
                    }
                }
            }
        }
    }

    /// <summary>
    /// FixedUpdate更新TimerData
    /// </summary>
    /// <param name="timepassed"></param>
    /// <returns></returns>
    public void fixedUpdate(float timepassed)
    {
        if(!mIsUpdate)
        {
            if (!mIsPause)
            {
                mTimePassed += timepassed;
                if (mTimePassed >= mDelayTime)
                {
                    if (mTriggerCondition != null)
                    {
                        if (mTriggerCondition.Invoke())
                        {
                            mIsOver = true;
                            mCallBack.Invoke();
                            return;
                        }
                    }
                    else
                    {
                        mIsOver = true;
                        mCallBack.Invoke();
                        return;
                    }
                }
            }
        }
    }

    /// <summary>
    /// 对象池弹出创建时
    /// </summary>
    public void onCreate()
    {
        UID = 0;
        mCallBack = null;
        mDelayTime = 0f;
        mIsUpdate = true;
        mTriggerCondition = null;
        mTimePassed = 0f;
        mIsPause = false;
        mIsOver = false;
    }

    /// <summary>
    /// 是否暂停
    /// </summary>
    /// <returns></returns>
    public bool isPaused()
    {
        return mIsPause;
    }

    /// <summary>
    /// 是否结束
    /// </summary>
    /// <returns></returns>
    public bool isOver()
    {
        return mIsOver;
    }

    /// <summary>
    /// 对象池回收时
    /// </summary>
    public void onDispose()
    {
        UID = 0;
        mCallBack = null;
        mDelayTime = 0f;
        mIsUpdate = true;
        mTriggerCondition = null;
        mTimePassed = 0f;
        mIsPause = false;
        mIsOver = false;
    }
}
