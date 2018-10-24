/*
 * Description:             TimeCounter.cs
 * Author:                  TONYTANG
 * Create Date:             2018/08/08
 */

using UnityEngine;
using System.Collections;
using System.Diagnostics;

/// <summary>
/// 计时器
/// </summary>
public class TimeCounter : SingletonTemplate<TimeCounter>
{
    /// <summary>
    /// 计时器
    /// </summary>
    private Stopwatch mTimer;

    /// <summary>
    /// Tag名
    /// </summary>
    private string mName;

    /// <summary>
    /// 时间消耗
    /// </summary>
    public float TimeSpend
    {
        get
        {
            return mTimer.ElapsedMilliseconds;
        }
    }
    private float mTimeSpend;

    public TimeCounter()
    {
        mTimer = new Stopwatch();
        mName = "Default";
    }

    public void Start(string name)
    {
        mName = name;
        mTimer.Start();
    }

    public void Restart(string name)
    {
        mTimer.Reset();
        mTimer.Start();
        mName = name;
    }

    public void End()
    {
        mTimer.Stop();
        mTimeSpend = mTimer.ElapsedMilliseconds;
        UnityEngine.Debug.Log(string.Format("{0} 耗时 : {1} ms", mName, mTimeSpend));
    }
}