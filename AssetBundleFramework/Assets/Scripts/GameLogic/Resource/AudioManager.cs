/*
 * Description:             AudioManager.cs
 * Author:                  TONYTANG
 * Create Date:             2018//10/20
 */

using System.Collections;
using System.Collections.Generic;
using System.IO;
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
        /// 资源加载信息
        /// </summary>
        public AbstractResourceInfo ABI
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

        public void onCreate()
        {

        }

        public void onDispose()
        {
            ABI = null;
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
    /// 当前场景背景音乐的资源信息
    /// </summary>
    private AbstractResourceInfo mCurrentBGMARI;

    public AudioManager()
    {
        mAudioGoPool = new GameObjectPool("AudioGoPool");
        mSFXGoTemplate = new GameObject("SfxAudio");
        mSFXInstanceID = mSFXGoTemplate.GetInstanceID();
        mSFXGoTemplate.AddComponent<AudioSource>();
        mAudioGoPool.Init(mSFXGoTemplate, 5);
        var bgmgo = new GameObject("BGMAudio");
        Object.DontDestroyOnLoad(bgmgo);
        mBGMAudioSource = bgmgo.AddComponent<AudioSource>();

        ObjectPool.Singleton.initialize<SFXAudioInfo>(5);
    }

    /// <summary>
    /// 播放音效
    /// </summary>
    /// <param name="respath">资源路径</param>
    public void playSFXSound(string respath)
    {
        var sfxgo = mAudioGoPool.Pop(mSFXGoTemplate);
        ResourceModuleManager.Singleton.requstResource(respath,
        (abi) =>
        {
            var sfxaudioinfo = ObjectPool.Singleton.pop<SFXAudioInfo>();
            var sfxname = Path.GetFileName(respath);
            var ac = abi.getAsset<AudioClip>(sfxgo, sfxname);
            var audiosource = sfxgo.GetComponent<AudioSource>();
            sfxaudioinfo.SFXAudioGo = sfxgo;
            sfxaudioinfo.SFXAudioSource = audiosource;
            sfxaudioinfo.ABI = abi;
            audiosource.clip = ac;
            audiosource.Play();
            Timer.Singleton.addTimer(() =>
            {
                // 手动释放音效资源绑定，因为音效绑定对象会进池会导致无法满足释放条件
                sfxaudioinfo.SFXAudioSource.clip = null;
                sfxaudioinfo.ABI.releaseOwner(sfxaudioinfo.SFXAudioGo);
                mAudioGoPool.Push(mSFXInstanceID, sfxaudioinfo.SFXAudioGo);
                ObjectPool.Singleton.push<SFXAudioInfo>(sfxaudioinfo);
            }, ac.length);
        });
    }

    /// <summary>
    /// 播放背景音乐
    /// </summary>
    /// <param name="respath">资源路径</param>
    /// <param name="loop">是否循环播放</param>
    public void playBGM(string respath, bool loop = true)
    {
        //背景音效是挂载DontDestroyOnLoad上会导致永远无法满足卸载条件，所以需要手动移除对象绑定
        if (mCurrentBGMARI != null)
        {
            mCurrentBGMARI.releaseOwner(mBGMAudioSource);
        }

        var assetname = Path.GetFileName(respath);
        ResourceModuleManager.Singleton.requstResource(respath,
        (ari) =>
        {
            mCurrentBGMARI = ari;
            var clip = ari.getAsset<AudioClip>(mBGMAudioSource, assetname);
            mBGMAudioSource.clip = clip;
            mBGMAudioSource.loop = loop;
            mBGMAudioSource.Play();
        });
    }
}