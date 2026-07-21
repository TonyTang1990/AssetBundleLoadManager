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
        /// Asset加载器
        /// </summary>
        public AssetLoader Loader
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

        public void OnDispose()
        {
            Loader = null;
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
    /// 背景音效组件
    /// </summary>
    private AudioSource mBGMAudioSource;

    /// <summary>
    /// 当前背景音乐的Asset加载器
    /// </summary>
    private AssetLoader mCurrentBGMAssetLoader;

    public AudioManager()
    {
        mAudioGoPool = new GameObjectPool("AudioGoPool");
        mSFXGoTemplate = new GameObject("SfxAudio");
        mSFXInstanceID = mSFXGoTemplate.GetInstanceID();
        mSFXGoTemplate.AddComponent<AudioSource>();
        mAudioGoPool.Init(mSFXGoTemplate, 5);
        var bgmgo = new GameObject("BGMAudio");
        UnityEngine.Object.DontDestroyOnLoad(bgmgo);
        mBGMAudioSource = bgmgo.AddComponent<AudioSource>();

        ObjectPool.Singleton.Initialize<SFXAudioInfo>(5);
    }

    /// <summary>
    /// 播放音效
    /// </summary>
    /// <param name="resName">资源名(含后缀)</param>
    /// <param name="assetLoader">Asset加载器</param>
    /// <param name="callBack">回调</param>
    /// <param name="loadType">加载类型</param>
    /// <returns></returns>
    public AssetRequestHandle PlaySFXSound(string resName, out AssetLoader assetLoader,
                            Action<AudioClip, AssetRequestHandle> callBack = null,
                            ResourceLoadType loadType = ResourceLoadType.NormalLoad)
    {
        var sfxgo = mAudioGoPool.Pop(mSFXGoTemplate);
        return ResourceModuleManager.Singleton.RequstAssetSync<AudioClip>(
            resName,
            out assetLoader,
            (loader, assetRequestHandle) =>
            {
                var sfxaudioinfo = ObjectPool.Singleton.Pop<SFXAudioInfo>();
                var ac = loader.BindAsset<AudioClip>(sfxgo);
                var audiosource = sfxgo.GetComponent<AudioSource>();
                sfxaudioinfo.SFXAudioGo = sfxgo;
                sfxaudioinfo.SFXAudioSource = audiosource;
                sfxaudioinfo.Loader = loader;
                audiosource.clip = ac;
                audiosource.Play();
                TimerManager.Singleton.addUpdateTimer(() =>
                {
                    // 手动释放音效资源绑定，因为音效绑定对象会进池会导致无法满足释放条件
                    sfxaudioinfo.SFXAudioSource.clip = null;
                    sfxaudioinfo.Loader.ReleaseOwner(sfxaudioinfo.SFXAudioGo);
                    mAudioGoPool.Push(mSFXInstanceID, sfxaudioinfo.SFXAudioGo);
                    ObjectPool.Singleton.Push<SFXAudioInfo>(sfxaudioinfo);
                }, ac.length);
                callBack?.Invoke(ac, assetRequestHandle);
            },
            loadType
        );
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
    public AssetRequestHandle PlayBGM(string resName, out AssetLoader assetLoader,
                                      bool loop = true, Action<AudioClip, AssetRequestHandle> callBack = null,
                                      ResourceLoadType loadType = ResourceLoadType.NormalLoad)
    {
        //背景音效是挂载DontDestroyOnLoad上会导致永远无法满足卸载条件，所以需要手动移除对象绑定
        if (mCurrentBGMAssetLoader != null)
        {
            mCurrentBGMAssetLoader.ReleaseOwner(mBGMAudioSource);
            mCurrentBGMAssetLoader = null;
        }

        return ResourceModuleManager.Singleton.RequstAssetSync<AudioClip>(
            resName,
            out assetLoader,
            (loader, assetRequestHandle) =>
            {
                mCurrentBGMAssetLoader = loader;
                var clip = loader.BindAsset<AudioClip>(mBGMAudioSource);
                mBGMAudioSource.clip = clip;
                mBGMAudioSource.loop = loop;
                mBGMAudioSource.Play();
                callBack?.Invoke(clip, assetRequestHandle);
            },
            loadType
        );
    }
}
