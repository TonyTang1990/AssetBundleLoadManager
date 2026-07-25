/*
 * Description:             UpdateManager.cs
 * Author:                  TANGHUAN
 * Create Date:             2021/02/22
 */

using System;

/// <summary>
/// 提供统一注入和取消Update机制的单例管理类
/// </summary>
public class UpdateManager : SingletonTemplate<UpdateManager>
{
    /// <summary>
    /// 注入的Update委托
    /// </summary>
    private Action<float> mUpdateDelegates;

    /// <summary>
    /// 注入的FixedUpdate委托
    /// </summary>
    private Action<float> mFixedUpdateDelegates;

    public UpdateManager()
    {

    }

    /// <summary>
    /// 注册Update委托
    /// </summary>
    /// <param name="updatedelegate"></param>
    public void RegisterUpdate(Action<float> updatedelegate)
    {
        mUpdateDelegates += updatedelegate;
    }

    /// <summary>
    /// 取消注册Update委托
    /// </summary>
    /// <param name="updatedelegate"></param>
    public void UnregisterUpdate(Action<float> updatedelegate)
    {
        mUpdateDelegates -= updatedelegate;
    }

    /// <summary>
    /// 注册FixedUpdate委托
    /// </summary>
    /// <param name="fixedupdatedelegate"></param>
    public void RegisterFixedUpdate(Action<float> fixedupdatedelegate)
    {
        mFixedUpdateDelegates += fixedupdatedelegate;
    }

    /// <summary>
    /// 取消注册FixedUpdate委托
    /// </summary>
    /// <param name="fixedupdatedelegate"></param>
    public void UnregisterFixedUpdate(Action<float> fixedupdatedelegate)
    {
        mFixedUpdateDelegates -= fixedupdatedelegate;
    }

    /// <summary>
    /// Update
    /// </summary>
    /// <param name="deltatime"></param>
    public void Update(float deltatime)
    {
        mUpdateDelegates?.Invoke(deltatime);
    }

    /// <summary>
    /// FixedUpdate
    /// </summary>
    /// <param name="fixeddeltatime"></param>
    public void FixedUpdate(float fixeddeltatime)
    {
        mFixedUpdateDelegates?.Invoke(fixeddeltatime);
    }
}