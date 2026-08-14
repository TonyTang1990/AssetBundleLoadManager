/*
 * Description:             AB游戏入口
 * Author:                  tanghuan
 * Create Date:             2021/10/13
 */

using Data;
using System;
using System.Collections;
using System.Collections.Generic;
using TUI;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using UnityEngine.Video;

namespace TResource
{
    /// <summary>
    /// AB游戏入口(AB加载启动入口)
    /// </summary>
    public class GameLauncher : MonoBehaviour
    {
        /// <summary>
        /// UI挂在节点
        /// </summary>
        public GameObject UIRootCanvas;

        /// <summary>
        /// UI根节点
        /// </summary>
        public GameObject UIRoot;

        /// <summary>
        /// 参数1
        /// </summary>
        public InputField InputParam1;

        /// <summary>
        /// 参数2
        /// </summary>
        public InputField InputParam2;

        /// <summary>
        /// 原生消息数据显示文本
        /// </summary>
        public Text TxtNativeOutput;

        /// <summary>
        /// 测试背景TImage访问资源
        /// </summary>
        public TImage TImgBG;

        /// <summary>
        /// 测试背景TImage2访问资源
        /// </summary>
        public TImage TImgBG2;

        /// <summary>
        /// 测试背景TRawImage访问资源
        /// </summary>
        public TRawImage TRawImgBG;

        /// <summary>
        /// 测试TButton
        /// </summary>
        public TButton DIYButton;

        /// <summary>
        /// 视屏播放组件
        /// </summary>
        public VideoPlayer VideoPlayerComponent;

        /// <summary>
        /// 视频播放RawImage显示组件
        /// </summary>
        public TRawImage VideoRawImage;

        /// <summary>
        /// 窗口实例对象
        /// </summary>
        private GameObject mMainWindow;

        /// <summary>
        /// 角色实例对象
        /// </summary>
        private GameObject mActorInstance;

        /// <summary>
        /// 角色实例对象2
        /// </summary>
        private GameObject mActorInstance2;

        /// <summary>
        /// 音效临时实例对象
        /// </summary>
        private GameObject mSFXInstance;

        /// <summary>
        /// 资源管理单例对象(快速访问)
        /// </summary>
        private ResourceModuleManager mRMM;

        /// <summary>
        /// 背景音乐音效组件
        /// </summary>
        private AudioSource mBGMAudioSource;

        /// <summary>
        /// 当前播放视频名(含后缀)
        /// </summary>
        private string mCurrentPlayVideoName = null;
        
        /// <summary>
        /// GameLuancher生命周期的资源计数释放+请求打断管理器
        /// </summary>
        private ResourceScope mResourceScope = new ResourceScope();

        /// <summary>
        /// 挂载的Mono单例列表
        /// </summary>
        private List<Action> mSingletonMonoList;
        
        private void Awake()
        {
            mSingletonMonoList = new List<Action>();
            DontDestroyOnLoad(this);
            DontDestroyOnLoad(UIRoot);

            InitSingletonMonos();
            InitSingletons();
            AddMonoComponents();
            NativeInitilization();
            Initilization();
        }

        private void Start()
        {
            AddListeners();
        }

        /// <summary>
        /// 添加监听
        /// </summary>
        private void AddListeners()
        {
            DIYButton.LongTimePressedClick = onTButtonListenerClick;
        }

        private void Update()
        {
            var deltaTime = Time.deltaTime;
            ResourceModuleManager.Singleton.Update(deltaTime);
            TimerManager.Singleton.Update(deltaTime);
            UpdateManager.Singleton.Update(deltaTime);
        }

        private void FixedUpdate()
        {
            var fixedDeltaTime = Time.fixedDeltaTime;
            TimerManager.Singleton.FixedUpdate(fixedDeltaTime);
            UpdateManager.Singleton.FixedUpdate(fixedDeltaTime);
        }

        private void OnDestroy()
        {
            mResourceScope.Clear();
            SingletonManager.ShutdownAll();
            ReleaseAllSingletonMonoBehaviour();
        }

        /// <summary>
        /// 初始化Mono单例
        /// </summary>
        private void InitSingletonMonos()
        {
            //快速UI工具
            AddSingletonMono<FastUIEntry>();
            //携程管理类
            AddSingletonMono<CoroutineManager>();
            //原生消息处理器
            AddSingletonMono<NativeMessageHandler>();
        }

        /// <summary>
        /// 初始化单例对象
        /// </summary>
        private void InitSingletons()
        {
            SingletonManager.Register(new ResourceModuleManager());
            SingletonManager.Register(new GameConfigModuleManager());
            SingletonManager.Register(new HotUpdateModuleManager());
            SingletonManager.Register(new GameSceneManager());
            SingletonManager.Register(new AtlasManager());
        }

        /// <summary>
        /// 添加Mono相关的组件
        /// </summary>
        private void AddMonoComponents()
        {
            NativeMessageHandler.GetInstance().TxtNativeOutput = TxtNativeOutput;
        }

        /// <summary>
        /// 原生初始化
        /// </summary>
        private void NativeInitilization()
        {
            NativeManager.Singleton.init();
        }

        /// <summary>
        /// 初始化
        /// </summary>
        private void Initilization()
        {
            mRMM = ResourceModuleManager.Singleton;
            mRMM.Init();

            //初始化游戏配置信息
            GameConfigModuleManager.Singleton.InitGameConfigData();

            //初始化版本信息
            VersionConfigModuleManager.Singleton.InitVerisonConfigData();

            //热更模块初始化
            HotUpdateModuleManager.Singleton.Init();

            // 预加载Shader
            //ResourceManager.Singleton.loadAllShader("shaderlist", () =>
            //{

            //},
            //ResourceLoadType.PermanentLoad);

            //初始化表格数据读取
            GameDataManager.Singleton.loadAll();

            mBGMAudioSource = GetComponent<AudioSource>();
        }

        /// <summary>
        /// 加载窗口预制件
        /// </summary>
        public void onLoadWindowPrefab()
        {
            DIYLog.Log("onLoadWindowPrefab()");
            ResourceManager.Singleton.GetPrefabInstance(
                "MainWindow.prefab",
                (prefabInstance, assetRequestHandle) =>
                {
                    mMainWindow = prefabInstance;
                    mMainWindow.transform.SetParent(UIRootCanvas.transform, false);
                },
                mResourceScope
            );
        }

        /// <summary>
        /// 销毁窗口实例对象
        /// </summary>
        public void onDestroyWindowInstance()
        {
            DIYLog.Log("onDestroyWindowInstance()");
            if(mMainWindow != null)
            {
                GameObject.Destroy(mMainWindow);
                mMainWindow = null;
            }
        }

        /// <summary>
        /// 加载Image Sprite
        /// </summary>
        public void onLoadImageSprite()
        {
            DIYLog.Log("onLoadImageSprite()");
            var param1 = InputParam1.text;
            DIYLog.Log("Param1 = " + param1);
            var param2 = InputParam2.text;
            DIYLog.Log("Param2 = " + param2);
            var image = mMainWindow.transform.Find("imgBG").GetComponent<Image>();
            AtlasManager.Singleton.SetImageSingleSprite(image, param1, mResourceScope);
        }

        /// <summary>
        /// 加载TImage Sprite
        /// </summary>
        public void onLoadTImageSprite()
        {
            DIYLog.Log("onLoadTImageSprite()");
            var param1 = InputParam1.text;
            DIYLog.Log("Param1 = " + param1);
            var param2 = InputParam2.text;
            DIYLog.Log("Param2 = " + param2);
            TImgBG.SetSingleSprite(param1);
        }

        /// <summary>
        /// 加载TImage Sprite Atlas
        /// </summary>
        public void onLoadTImageSpriteAtlas()
        {
            DIYLog.Log("onLoadTImageSpriteAtlas()");
            var param1 = InputParam1.text;
            DIYLog.Log("Param1 = " + param1);
            var param2 = InputParam2.text;
            DIYLog.Log("Param2 = " + param2);
            TImgBG.SetSpriteAtlasSprite(param1, param2);
        }

        /// <summary>
        /// 加载TImage Sub Sprite(MultipleSprite那种)
        /// </summary>
        public void onLoadTImageSubSprite()
        {
            DIYLog.Log("onLoadTImageSubSprite()");
            var param1 = InputParam1.text;
            DIYLog.Log("Param1 = " + param1);
            var param2 = InputParam2.text;
            DIYLog.Log("Param2 = " + param2);
            TImgBG.SetSubSprite(param1, param2);
        }

        /// <summary>
        /// 加载背景TImage Sprite Atlas
        /// </summary>
        public void onLoadTImageBGSpriteAtlas()
        {
            DIYLog.Log("onLoadTImageBGSpriteAtlas()");
            var param1 = InputParam1.text;
            DIYLog.Log("Param1 = " + param1);
            var param2 = InputParam2.text;
            DIYLog.Log("Param2 = " + param2);
            TImgBG.SetSpriteAtlasSprite(param1, param2);
        }

        /// <summary>
        /// 加载TRawImage
        /// </summary>
        public void onLoadTRawImageSprite()
        {
            DIYLog.Log("onLoadTRawImageSprite()");
            var param1 = InputParam1.text;
            DIYLog.Log("Param1 = " + param1);
            TRawImgBG.PrintTRawImageInfo();
            TRawImgBG.SetRawImage(param1);
            TRawImgBG.PrintTRawImageInfo();
        }

        /// <summary>
        /// 播放背景音乐
        /// </summary>
        public void onPlayBGM()
        {
            DIYLog.Log("onPlayBGM()");
            var param1 = InputParam1.text;
            DIYLog.Log("Param1 = " + param1);
            AudioManager.Singleton.PlayBGM(param1);
        }

        /// <summary>
        /// 停止背景音乐
        /// </summary>
        public void onStopBGM()
        {
            DIYLog.Log("onStopBGM()");
            AudioManager.Singleton.StopBGM();
        }

        /// <summary>
        /// 播放音效
        /// </summary>
        public void onPlaySound()
        {
            DIYLog.Log("onPlaySound()");
            var param1 = InputParam1.text;
            DIYLog.Log("Param1 = " + param1);
            AudioManager.Singleton.PlaySFXSound(param1);
        }


        /// <summary>
        /// 加载材质
        /// </summary>
        public void onLoadMaterial()
        {
            DIYLog.Log("onLoadMaterial()");
            var param1 = InputParam1.text;
            DIYLog.Log("Param1 = " + param1);
            var btnloadmat = UIRoot.transform.Find("SecondUICanvas/ButtonGroups/btnLoadMaterial");
            var image = btnloadmat.GetComponent<Image>();
            ResourceManager.Singleton.GetMaterial(
                param1,
                (material, assetRequestHandle) =>
                {
                    Material mat = material;
                    image.material = mat;
                },
                mResourceScope
            );
        }

        /// <summary>
        /// 加载角色
        /// </summary>
        public void onLoadActorPrefab()
        {
            DIYLog.Log("onLoadActorPrefab()");
            ResourceManager.Singleton.GetPrefabInstance(
                "pre_Zombunny.prefab",
                (instance, assetRequestHandle) =>
                {
                    mActorInstance = instance;
                },
                mResourceScope
            );
        }

        /// <summary>
        /// 销毁角色实例对象
        /// </summary>
        public void onDestroyActorInstance()
        {
            DIYLog.Log("onDestroyActorInstance()");
            GameObject.Destroy(mActorInstance);
        }

        /// <summary>
        /// 预加载图集资源
        /// </summary>
        public void onPreloadAtlas()
        {
            DIYLog.Log("onPreloadAtlas()");
            var param1 = InputParam1.text;
            DIYLog.Log("Param1 = " + param1);
            AtlasManager.Singleton.LoadAtlas(param1, mResourceScope, null, ResourceLoadType.PermanentLoad);
            // 如果像释放计数，需要调用assetLoader.ReleaseAsset()
        }

        /// <summary>
        /// 加载常驻Shader
        /// </summary>
        public void onLoadPermanentShaderList()
        {
            DIYLog.Log("onLoadPermanentShaderList()");
            ResourceManager.Singleton.LoadAllShader(() =>
            {


            },
            mResourceScope,
            ResourceLoadType.PermanentLoad);
        }

        /// <summary>
        /// 预加载Shader变体
        /// </summary>
        public void onPreloadShaderVariants()
        {
            DIYLog.Log("onPreloadShaderVariants()");
            // Shader通过预加载ShaderVariantsCollection里指定的Shader来进行预编译
            AssetLoader assetLoader;
            ResourceModuleManager.Singleton.RequstAssetSync<ShaderVariantCollection>(
                ResourceConstData.ShaderVariantsAssetName,
                out assetLoader,
                (loader, assetRequestHandle) =>
                {
                    var svc = loader.GetAsset<ShaderVariantCollection>();
                    // Shader通过预加载ShaderVariantsCollection里指定的Shader来进行预编译
                    svc.WarmUp();
                },
                ResourceLoadType.PermanentLoad
            );
        }


        /// <summary>
        /// 异步加载窗口
        /// </summary>
        public void onAsynLoadWindowPrefab()
        {
            DIYLog.Log("onAsynLoadWindowPrefab()");
            if (mMainWindow != null)
            {
                onDestroyWindowInstance();
            }
            ResourceManager.Singleton.GetPrefabInstanceAsync(
                "MainWindow.prefab",
                (prefabInstance, assetRequestHandle) =>
                {
                    mMainWindow = prefabInstance;
                    mMainWindow.transform.SetParent(UIRootCanvas.transform, false);
                },
                mResourceScope
            );
        }

        /// <summary>
        /// 测试异步转同步窗口加载
        /// </summary>
        public void onAsynToSyncLoadWindow()
        {
            DIYLog.Log("onAsynToSyncLoadWindow()");
            if (mMainWindow != null)
            {
                onDestroyWindowInstance();
            }
            var assetRequestHandle = ResourceManager.Singleton.GetPrefabInstanceAsync(
                "MainWindow.prefab",
                (prefabInstance, assetRequestHandle) =>
                {
                    mMainWindow = prefabInstance;
                    mMainWindow.transform.SetParent(UIRootCanvas.transform, false);
                },
                mResourceScope
            );
            // 未开始加载时将异步转同步加载
            assetRequestHandle.LoadImmediately();
        }

        /// <summary>
        /// 测试异步转同步窗口加载2
        /// </summary>
        public void onAsynToSyncLoadWindow2()
        {
            DIYLog.Log("onAsynToSyncLoadWindow2()");
            if (mMainWindow != null)
            {
                onDestroyWindowInstance();
            }
            var assetRequestHandle = ResourceManager.Singleton.GetPrefabInstanceAsync(
                "MainWindow.prefab",
                (prefabInstance, assetRequestHandle) =>
                {
                    mMainWindow = prefabInstance;
                    mMainWindow.transform.SetParent(UIRootCanvas.transform, false);
                },
                mResourceScope
            );
            StartCoroutine(WaitLoadCoroutine(assetRequestHandle));
        }

        /// <summary>
        /// 等待加载携程
        /// </summary>
        /// <returns></returns>
        private IEnumerator WaitLoadCoroutine(AssetRequestHandle assetRequestHandle)
        {
            yield return new WaitForEndOfFrame();
            // 开始异步加载后转同步加载
            assetRequestHandle.LoadImmediately();
        }

        /// <summary>
        /// 测试异步转同步窗口加载3
        /// </summary>
        public void onAsynToSyncLoadWindow3()
        {
            DIYLog.Log("onAsynToSyncLoadWindow3()");
            if (mMainWindow != null)
            {
                onDestroyWindowInstance();
            }
            var assetRequestHandle = ResourceManager.Singleton.GetPrefabInstanceAsync(
                "MainWindow.prefab",
                (prefabInstance, assetRequestHandle) =>
                {
                    DIYLog.Log($"ResourceManager.Singleton.getPrefabInstanceAsync()");
                    // 避免出现两个主界面窗口
                    onDestroyWindowInstance();
                    mMainWindow = prefabInstance;
                    mMainWindow.transform.SetParent(UIRootCanvas.transform, false);
                },
                mResourceScope
            );
            // 异步未开始时触发同步加载2
            ResourceManager.Singleton.GetPrefabInstance("MainWindow.prefab",
                (instance, assetRequestHandle)=>
                {
                    DIYLog.Log($"ResourceManager.Singleton.getPrefabInstance()");
                    // 避免出现两个主界面窗口
                    onDestroyWindowInstance();
                    mMainWindow = instance;
                    mMainWindow.transform.SetParent(UIRootCanvas.transform, false);
                },
                mResourceScope
            );
        }

        /// <summary>
        /// 取消异步窗口加载请求回调
        /// </summary>
        public void onCancelAsynLoadWindow()
        {
            DIYLog.Log("onCancelAsynLoadWindow()");
            if (mMainWindow != null)
            {
                onDestroyWindowInstance();
            }
            var assetRequestHandle = ResourceManager.Singleton.GetPrefabInstanceAsync(
                "MainWindow.prefab",
                (prefabInstance, assetRequestHandle) =>
                {
                    mMainWindow = prefabInstance;
                    mMainWindow.transform.SetParent(UIRootCanvas.transform, false);
                },
                mResourceScope
            );
            // 取消异步加载请求
            assetRequestHandle.Cancel();
        }

        /// <summary>
        /// 多异步请求单个Sprite
        /// </summary>
        public void onMultipleAsyncLoadSingleTSprite()
        {
            DIYLog.Log("onMultipleAsyncLoadTSprite()");
            var param1 = InputParam1.text;
            DIYLog.Log("Param1 = " + param1);
            AtlasManager.Singleton.SetTImageSingleSpriteAsync(TImgBG, param1);
            AtlasManager.Singleton.SetTImageSingleSpriteAsync(TImgBG2, param1);
        }

        /// <summary>
        /// 多异步请求多个Sprite
        /// </summary>
        public void onMultipleAsyncLoadMultipleTSprite()
        {
            DIYLog.Log("onMultipleAsyncLoadTSprite()");
            var param1 = InputParam1.text;
            DIYLog.Log("Param1 = " + param1);
            var param2 = InputParam2.text;
            DIYLog.Log("Param2 = " + param2);
            AtlasManager.Singleton.SetTImageSingleSpriteAsync(TImgBG, param1);
            AtlasManager.Singleton.SetTImageSingleSpriteAsync(TImgBG2, param2);
        }


        /// <summary>
        /// 异步+同步加载窗口但取消异步请求
        /// </summary>
        public void onAsyncAndSyncLoadWindowButCancelAsync()
        {
            DIYLog.Log("onAsyncAndSyncLoadWindowButCancelAsync()");
            if (mMainWindow != null)
            {
                onDestroyWindowInstance();
            }
            var assetRequestHandle = ResourceManager.Singleton.GetPrefabInstanceAsync(
                "MainWindow.prefab",
                (prefabInstance, assetRequestHandle) =>
                {
                    // 第二次加载因为已经加载过可能出现立刻回到的情况
                    // 必须确保清理干净避免两个MainWindow出现
                    onDestroyWindowInstance();
                    Debug.Log($"getPrefabInstanceAsync()");
                    mMainWindow = prefabInstance;
                    mMainWindow.transform.SetParent(UIRootCanvas.transform, false);
                },
                mResourceScope
            );
            // 取消异步加载请求后同步加载窗口
            assetRequestHandle.Cancel();
            ResourceManager.Singleton.GetPrefabInstance(
                "MainWindow.prefab",
                (prefabInstance, assetRequestHandle) =>
                {
                    // 第二次加载因为已经加载过可能出现立刻回到的情况
                    // 必须确保清理干净避免两个MainWindow出现
                    onDestroyWindowInstance();
                    Debug.Log($"getPrefabInstance()");
                    mMainWindow = prefabInstance;
                    mMainWindow.transform.SetParent(UIRootCanvas.transform, false);
                },
                mResourceScope
            );
        }

        /// <summary>
        /// 切换场景
        /// </summary>
        public void onChangeScene()
        {
            DIYLog.Log("onChangeScene()");
            var param1 = InputParam1.text;
            DIYLog.Log("Param1 = " + param1);
            var param2 = InputParam2.text;
            DIYLog.Log("Param2 = " + param2);

            //切换场景前关闭所有打开窗口，测试切场景资源卸载功能
            onDestroyWindowInstance();

            GameSceneManager.Singleton.LoadSceneSync(param1);
        }

        /// <summary>
        /// 打印AB依赖信息
        /// </summary>
        public void onPrintABDepInfo()
        {
            DIYLog.Log("onPrintABDepInfo()");
            if(mRMM.CurrentResourceModule is AssetBundleModule)
            {
                (mRMM.CurrentResourceModule as AssetBundleModule).PrintAllResourceDpInfo();
            }
        }

        /// <summary>
        /// 打印已加载资源信息
        /// </summary>
        public void onPrintLoadedResourceInfo()
        {
            DIYLog.Log("onPrintLoadedResourceInfo()");
            mRMM.CurrentResourceModule.PrintAllLoadedResourceOwnersAndRefCount();
        }

        /// <summary>
        /// 卸载不再使用的Asset
        /// </summary>
        public void onUnloadUnsedAssets()
        {
            DIYLog.Log("onUnloadUnsedAssets()");
            Resources.UnloadUnusedAssets();
        }

        /// <summary>
        /// 切换资源Log开关
        /// </summary>
        public void onChangeResourceLogSwitch()
        {
            DIYLog.Log("onChangeResourceLogSwitch()");
            ResourceLogger.LogSwitch = !ResourceLogger.LogSwitch;
        }

        /// <summary>
        /// 打印版本信息
        /// </summary>
        public void onPrintVersionInfo()
        {
            DIYLog.Log("onPrintVersionInfo()");
            VersionConfigModuleManager.Singleton.InitVerisonConfigData();
            if (HotUpdateModuleManager.Singleton.ServerVersionConfig != null)
            {
                DIYLog.Log($"服务器版本信息:VersionCode:{HotUpdateModuleManager.Singleton.ServerVersionConfig.VersionCode} ResourceVersionCode : {HotUpdateModuleManager.Singleton.ServerVersionConfig.ResourceVersionCode}");
            }
            else
            {
                DIYLog.LogError("未获取服务器的版本信息!");
            }
        }

        /// <summary>
        /// 打印所有表格数据
        /// </summary>
        public void onPrintAllExcellData()
        {
            DIYLog.Log("onPrintAllExcellData()");
            var languageList = GameDataManager.Singleton.Gett_language_cnList();
            foreach (var language in languageList)
            {
                DIYLog.Log("----------------------------------------------");
                DIYLog.Log(string.Format("language Key : {0}", language.Key));
                DIYLog.Log(string.Format("language Value : {0}", language.Value));
            }
            var authorList = GameDataManager.Singleton.Gett_author_InfoList();
            foreach (var author in authorList)
            {
                DIYLog.Log("----------------------------------------------");
                DIYLog.Log(string.Format("author id : {0}", author.id));
                DIYLog.Log(string.Format("author author : {0}", author.author));
                DIYLog.Log(string.Format("author age : {0}", author.age));
                DIYLog.Log(string.Format("author national : {0}", author.national));
                DIYLog.Log(string.Format("author sex : {0}", author.sex));
            }
            var globalSList = GameDataManager.Singleton.Gett_global_sList();
            foreach (var global in globalSList)
            {
                DIYLog.Log("----------------------------------------------");
                DIYLog.Log(string.Format("global Key : {0}", global.Key));
                DIYLog.Log(string.Format("global stringvalue : {0}", global.Value));
            }
        }

        /// <summary>
        /// 打印所有AB路径信息
        /// </summary>
        public void onPrintAllABPath()
        {
            DIYLog.Log("onPrintAllABPath()");
            ResourcePath.PrintAllPathInfo();
        }

        /// <summary>
        /// 调用原生方法
        /// </summary>
        public void onCallNativeMethodClick()
        {
            Debug.Log("onCallNativeMethodClick()");
            NativeManager.Singleton.callNativeMethod();
        }

        /// <summary>
        /// 获取服务器版本信息
        /// </summary>
        public void onObtainServerVersionConfig()
        {
            DIYLog.Log("onObtainServerVersionConfig()");
            HotUpdateModuleManager.Singleton.DoObtainServerVersionConfig(serverVersionConfigHotUpdateCompleteCallBack);
        }

        /// <summary>
        /// 版本强更测试
        /// </summary>
        public void onTestVersionwHotUpdate()
        {
            DIYLog.Log("onTestVersionwHotUpdate()");
            if (HotUpdateModuleManager.Singleton.ServerVersionConfig != null)
            {
                if (HotUpdateModuleManager.Singleton.CheckVersionHotUpdate(HotUpdateModuleManager.Singleton.ServerVersionConfig.VersionCode))
                {
                    HotUpdateModuleManager.Singleton.DoNewVersionHotUpdate(HotUpdateModuleManager.Singleton.ServerVersionConfig.VersionCode, versionHotUpdateCompleteCallBack);
                }
            }
            else
            {
                DIYLog.LogError("版本强更前，请先获取服务器版本信息!");
            }
        }

        /// <summary>
        /// 资源热更测试
        /// </summary>
        public void onTestResourceHotUpdate()
        {
            DIYLog.Log("onTestResourceHotUpdate()");
            HotUpdateModuleManager.Singleton.TryResourceHotUpdate(resourceHotUpdateCompleteCallBack);
        }

        /// <summary>
        /// 获取服务器版本信息回调
        /// </summary>
        /// <param name="result"></param>
        private void serverVersionConfigHotUpdateCompleteCallBack(bool result)
        {
            DIYLog.Log($"获取服务器版本结果:{result}");
        }

        /// <summary>
        /// 版本强更完成回调
        /// </summary>
        /// <param name="result">版本强更结果</param>
        private void versionHotUpdateCompleteCallBack(bool result)
        {
            DIYLog.Log($"版本强更结果:{result}");
        }

        /// <summary>
        /// 资源热更完成回调
        /// </summary>
        /// <param name="hotUpdateResult">资源热更结果</param>
        private void resourceHotUpdateCompleteCallBack(HotUpdateResult hotUpdateResult)
        {
            DIYLog.Log($"资源热更结果:{hotUpdateResult}");
        }

        /// <summary>
        /// 测试热更新完整流程
        /// </summary>
        public void onTestHotUpdateFullWorkFlow()
        {
            DIYLog.Log("onTestHotUpdateFullWorkFlow()");
            VersionConfigModuleManager.Singleton.InitVerisonConfigData();
            //检测是否强更过版本
            HotUpdateModuleManager.Singleton.CheckHasVersionHotUpdate();
            //TODO:
            //拉去服务器列表信息(网络那一套待开发,暂时用本地默认数值测试)
            HotUpdateModuleManager.Singleton.DoObtainServerVersionConfig((result)=> {
                DIYLog.Log(string.Format("获取服务器版本结果 result : {0}", result));
                var serverVersionConfig = HotUpdateModuleManager.Singleton.ServerVersionConfig;
                if (HotUpdateModuleManager.Singleton.CheckVersionHotUpdate(serverVersionConfig.VersionCode))
                {
                    HotUpdateModuleManager.Singleton.DoNewVersionHotUpdate(
                    serverVersionConfig.VersionCode,
                    (versionhotupdateresult) =>
                    {
                        if (versionhotupdateresult)
                        {
                            DIYLog.Log("版本强更完成!触发自动安装！");
#if UNITY_ANDROID
                            (NativeManager.Singleton as AndroidNativeManager).installAPK(HotUpdateModuleManager.Singleton.VersionHotUpdateCacheFilePath);
#endif
                            return;
                        }
                        else
                        {
                            resourceHotUpdate();
                        }
                    });
                }
                else
                {
                    resourceHotUpdate();
                }
            });
        }

        private void resourceHotUpdate()
        {
            HotUpdateModuleManager.Singleton.TryResourceHotUpdate(resourceHotUpdateCompleteCallBack);
        }

        /// <summary>
        /// 清理ResourceScope所有资源
        /// </summary>
        public void onClearResourceScope()
        {
            DIYLog.Log("onClearResourceScope()");
            mResourceScope.Clear();
        }

        /// <summary>
        /// 强制卸载指定AB
        /// </summary>
        public void onForceUnloadSpecificAB()
        {
            DIYLog.Log("onForceUnloadSpecificAB()");
            if (ResourceModuleManager.Singleton.CurrentResourceModule.ResLoadMode == ResourceLoadMode.AssetBundle)
            {
                var param1 = InputParam1.text;
                DIYLog.Log("Param1 = " + param1);
                var assetBundleResourceModule = ResourceModuleManager.Singleton.CurrentResourceModule as AssetBundleModule;
                var abWithPostFix = ResourcePath.GetABPathWithPostFix(param1);
                Debug.Log($"abWithPostFix:{abWithPostFix}");
                assetBundleResourceModule.ForceUnloadAssetBundle(abWithPostFix);
            }
            else
            {
                DIYLog.Log("未处于AB状态，无法卸载指定AB!");
            }
        }

        /// <summary>
        /// 强制卸载所有资源
        /// </summary>
        public void onForceUnloadAllResources()
        {
            DIYLog.Log("onForceUnloadAllResources()");
            ResourceModuleManager.Singleton.ForceUnloadAllResources();
            Resources.UnloadUnusedAssets();
        }

        /// <summary>
        /// 强制重新加载AB依赖信息
        /// </summary>
        public void onForceReloadABDepInfo()
        {
            DIYLog.Log("onForceReloadABDepInfo()");
            var assetbundleresourcemodule = ResourceModuleManager.Singleton.CurrentResourceModule;
            assetbundleresourcemodule.ReloadData();
        }

        /// <summary>
        /// 播放指定Video
        /// </summary>
        public void onPlayVideo()
        {
            DIYLog.Log("onPlayVideo()");
            var param1 = InputParam1.text;
            DIYLog.Log("Param1 = " + param1);
            ReleasePlayedVideoRes();
            // TOOD: 封装视频播放组件，关闭视频播放时释放资源
            var videoClip = ResourceManager.Singleton.GetVideoClip(param1, (videoClip, assetRequestHandle) =>
            {
                if(videoClip == null)
                {
                    return;
                }
                if(VideoPlayerComponent.targetTexture != null &&
                !VideoPlayerComponent.targetTexture.IsCreated())
                {
                    VideoPlayerComponent.targetTexture.Create();
                }
                VideoPlayerComponent.clip = videoClip;
                if(videoClip != null)
                {
                    VideoPlayerComponent.Play();
                    mCurrentPlayVideoName = param1;
                }
                VideoRawImage.enabled = true;
            },
            mResourceScope
            );
        }

        /// <summary>
        /// 关闭视频播放
        /// </summary>
        public void onCloseVideo()
        {
            DIYLog.Log("onCloseVideo()");
            VideoPlayerComponent.Stop();
            VideoPlayerComponent.clip = null;
            ClearVideoTexture();
            ReleasePlayedVideoRes();
            VideoRawImage.enabled = false;
        }

        /// <summary>
        /// 释放已播放的视频资源
        /// </summary>
        private void ReleasePlayedVideoRes()
        {
            if(string.IsNullOrEmpty(mCurrentPlayVideoName))
            {
                return;
            }
            mResourceScope.ReleaseResourceByName(mCurrentPlayVideoName);
            mCurrentPlayVideoName = null;
        }

        /// <summary>
        /// 清理视频播放使用的纹理，避免残留图像导致场景Game显示不正确
        /// </summary>
        private void ClearVideoTexture()
        {
            var videioRenderTexture = VideoPlayerComponent.targetTexture;
            if(videioRenderTexture != null &&
               videioRenderTexture.IsCreated())
            {
                var prev = RenderTexture.active;
                RenderTexture.active = videioRenderTexture;
                GL.Clear(true, true, new Color(0, 0, 0, 0)); // 透明
                RenderTexture.active = prev;
                videioRenderTexture.DiscardContents();
            }            
        }

        /// <summary>
        /// 自定义按钮点击监听响应
        /// </summary>
        public void onTButtonListenerClick()
        {
            DIYLog.Log("onTButtonListenerClick()");
        }

#region 单例Mono挂载部分
        /// <summary>
        /// 添加指定Mono挂载
        /// </summary>
        /// <typeparam name="T"></typeparam>
        private void AddSingletonMono<T>() where T : SingletonMonoTemplate<T>
        {
            if (gameObject.GetComponent<T>() == null)
            {
                T t = gameObject.AddComponent<T>();
                t.SetInstance(t);
                t.Init();
                mSingletonMonoList.Add(delegate () { t.Release(); });
            }
        }

        /// <summary>
        /// 释放所有单例MonoBehaviour
        /// </summary>
        private void ReleaseAllSingletonMonoBehaviour()
        {
            for (int i = 0; i < mSingletonMonoList.Count; i++)
            {
                mSingletonMonoList[i]();
            }
        }
#endregion
    }
}
