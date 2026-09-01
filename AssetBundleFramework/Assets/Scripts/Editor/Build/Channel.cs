/*
 * Description:             Channel.cs
 * Author:                  TONYTANG
 * Create Date:             2026/08/20
 */

/// <summary>
/// BuildChannel.cs
/// 打包渠道枚举
/// </summary>
public enum Channel
{
    /// <summary>
    /// 无渠道
    /// </summary>
    None = 0,
    /// <summary>
    /// 谷歌渠道
    /// </summary>
    GooglePlay,

    #region IOS渠道
    /// <summary>
    /// 苹果商店渠道
    /// </summary>
    AppStore,
    #endregion

    #region Windows渠道
    /// <summary>
    /// Steam渠道
    /// </summary>
    Steam,
    #endregion
}