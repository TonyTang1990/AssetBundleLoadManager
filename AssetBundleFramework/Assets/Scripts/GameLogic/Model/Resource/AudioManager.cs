/*
 * Description:             AudioManager.cs
 * Author:                  TONYTANG
 * Create Date:             2018//10/20
 */

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using TResource;
using UnityEngine;

/// <summary>
/// AudioManager.cs
/// 音效单例管理类
/// </summary>
public class AudioManager : SingletonTemplate<AudioManager>
{
    /// <summary>
    /// 音效播放信息
    /// </summary>
    public class SFXAudioInfo : IRecycle
    {
        /// <summary>
        /// 资源名(含后缀)
        /// </summary>
        public string AudioResName
        {
            get;
            set;
        }

        /// <summary>
        /// 音效绑定对象
        /// </summary>
        public GameObject SFXAudioGo
        {
            get;
            set;
        }

        /// <summary>
        /// 音效组件
        /// </summary>
        public AudioSource SFXAudioSource
        {
            get;
            set;
        }

        public void OnCreate()
        {

        }

        public void OnRecycle()
        {
            AudioResName = null;
            SFXAudioGo = null;
            SFXAudioSource = null;
        }
    }

    /// <summary>
    /// 音效资源模板名
    /// </summary>
    private const string AudioGoResName = "SFXTemplate";
    
    /// <summary>
    /// 音效GameObject对象池
    /// </summary>
    private GameObjectPool mAudioGoPool;

    /// <summary>
    /// 音效实体对象模板
    /// </summary>
    private GameObject mSFXGoTemplate;

    /// <summary>
    /// 音效实体对象模板InstanceID
    /// </summary>
    private int mSFXInstanceID;

    /// <summary>
    /// 声音父节点Transform(包含所有音效+背景音乐)
    /// </summary>
    private Transform mSoundParentTransform;

    /// <summary>
    /// 背景音效组件
    /// </summary>
    private AudioSource mBGMAudioSource;

    /// <summary>
    /// 资源计数释放+请求打断管理器
    /// </summary>
    private ResourceScope mResourceScope;

    /// <summary>
    /// 当前背景音乐资源名(含后缀)
    /// </summary>
    private string mCurrentBGMResName;

    /// <summary>
    /// 是否静音所有声音(含所有音效+背景音乐)
    /// </summary>
    private bool mIsMuteAllSound = false;

    public AudioManager()
    {
        mAudioGoPool = new GameObjectPool("AudioGoPool");
        mSFXGoTemplate = new GameObject("SfxAudio");
        mSFXInstanceID = mSFXGoTemplate.GetInstanceID();
        mSFXGoTemplate.AddComponent<AudioSource>();
        mAudioGoPool.Init(mSFXGoTemplate, 5);
        mSoundParentTransform = new GameObject("SoundParent").transform;
        var bgmGo = new GameObject("BGMAudio");
        UnityEngine.Object.DontDestroyOnLoad(bgmGo);
        bgmGo.transform.SetParent(mSoundParentTransform, false);
        mBGMAudioSource = bgmGo.AddComponent<AudioSource>();
        mResourceScope = new ResourceScope();

        ObjectPool.Singleton.Initialize<SFXAudioInfo>(5);
    }

    /// <summary>
    /// 播放音效
    /// </summary>
    /// <param name="resName">资源名(含后缀)</param>
    /// <param name="callBack">回调</param>
    /// <param name="loadType">加载类型</param>
    /// <returns></returns>
    public AssetRequestHandle PlaySFXSound(string resName, Action<AudioClip, AssetRequestHandle> callBack = null,
                                           ResourceLoadType loadType = ResourceLoadType.NormalLoad)
    {
        var sfxGo = mAudioGoPool.Pop(mSFXGoTemplate);
        sfxGo.transform.SetParent(mSoundParentTransform, false);
        AssetLoader assetLoader;
        var assetRequestHandle = ResourceModuleManager.Singleton.RequstAssetSync<AudioClip>(
            resName,
            out assetLoader,
            (loader, assetRequestHandle) =>
            {
                DIYLog.Log($"PlaySFXSound加载resName:{resName}完成!");
                mResourceScope.RemoveRequest(assetRequestHandle);
                if (loader == null || !assetRequestHandle.IsComplete)
                {
                    mAudioGoPool.Push(mSFXInstanceID, sfxGo);
                    callBack?.Invoke(null, assetRequestHandle);
                    return;
                }
                var sfxAudioInfo = ObjectPool.Singleton.Pop<SFXAudioInfo>();
                var ac = mResourceScope.GetAsset<AudioClip>(loader);
                var audioSource = sfxGo.GetComponent<AudioSource>();
                sfxAudioInfo.SFXAudioGo = sfxGo;
                sfxAudioInfo.SFXAudioSource = audioSource;
                sfxAudioInfo.AudioResName = resName;
                audioSource.clip = ac;
                audioSource.mute = mIsMuteAllSound;
                audioSource.Play();
                TimerManager.Singleton.AddUpdateTimer((deltaTime) =>
                {
                    // 手动释放音效资源
                    sfxAudioInfo.SFXAudioSource.clip = null;
                    mResourceScope.ReleaseResourceByName(sfxAudioInfo.AudioResName);
                    mAudioGoPool.Push(mSFXInstanceID, sfxAudioInfo.SFXAudioGo);
                    ObjectPool.Singleton.Push<SFXAudioInfo>(sfxAudioInfo);
                }, ac.length);
                callBack?.Invoke(ac, assetRequestHandle);
            },
            loadType
        );
        mResourceScope.RecordRequest(assetRequestHandle);
        return assetRequestHandle;
    }

    /// <summary>
    /// 播放背景音乐
    /// </summary>
    /// <param name="resName">资源名(含后缀)</param>
    /// <param name="assetLoader">Asset加载器</param>
    /// <param name="loop">是否循环播放</param>
    /// <param name="callBack">回调</param>
    /// <param name="loadType">加载类型</param>
    /// <returns></returns>
    public AssetRequestHandle PlayBGM(string resName, bool loop = true,
                                      Action<AudioClip, AssetRequestHandle> callBack = null,
                                      ResourceLoadType loadType = ResourceLoadType.NormalLoad)
    {
        AssetLoader assetLoader;
        var assetRequestHandle = ResourceModuleManager.Singleton.RequstAssetSync<AudioClip>(
            resName,
            out assetLoader,
            (loader, assetRequestHandle) =>
            {
                DIYLog.Log($"PlayBGM加载resName:{resName}完成!");
                mResourceScope.RemoveRequest(assetRequestHandle);
                if (loader == null || !assetRequestHandle.IsComplete)
                {
                    callBack?.Invoke(null, assetRequestHandle);
                    return;
                }
                //背景音效是挂载DontDestroyOnLoad上会导致永远无法满足卸载条件，所以需要手动移除资源计数
                ReleaseCurrentBgmRes();
                mCurrentBGMResName = resName;
                var clip = mResourceScope.GetAsset<AudioClip>(loader);
                mBGMAudioSource.clip = clip;
                mBGMAudioSource.loop = loop;
                mBGMAudioSource.mute = mIsMuteAllSound;
                mBGMAudioSource.Play();
                callBack?.Invoke(clip, assetRequestHandle);
            },
            loadType
        );
        mResourceScope.RecordRequest(assetRequestHandle);
        return assetRequestHandle;
    }

    /// <summary>
    /// 静音或取消静音所有声音(含所有音效+背景音乐)
    /// </summary>
    /// <param name="mute"></param>
    public void MuteAllSound(bool mute)
    {
        mIsMuteAllSound = mute;
        for(int index = 0, length = mSoundParentTransform.childCount; index < length; index++)
        {
            var child = mSoundParentTransform.GetChild(index);
            var audioSource = child.GetComponent<AudioSource>();
            if(audioSource != null)
            {
                audioSource.mute = mute;
            }
        }
    }

    /// <summary>
    /// 停止播放背景音乐
    /// </summary>
    public void StopBGM()
    {
        mBGMAudioSource.Stop();
        mBGMAudioSource.clip = null;
        ReleaseCurrentBgmRes();
    }

    /// <summary>
    /// 释放当前背景音乐资源
    /// </summary>
    private void ReleaseCurrentBgmRes()
    {
        if (!string.IsNullOrEmpty(mCurrentBGMResName))
        {
            mResourceScope.ReleaseResourceByName(mCurrentBGMResName);
            mCurrentBGMResName = null;
        }
    }
}
