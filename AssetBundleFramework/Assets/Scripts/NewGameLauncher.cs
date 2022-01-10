/*
 * Description:             新版AB游戏入口
 * Author:                  tanghuan
 * Create Date:             2021/10/13
 */

using Data;
using System.Collections;
using TUI;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

namespace TResource
{
    /// <summary>
    /// 新版AB游戏入口(新版AB加载启动入口)
    /// </summary>
    public class NewGameLauncher : MonoBehaviour
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
        /// 当前场景ABI，用于手动释放引用
        /// </summary>
        private AssetBundleInfo mCurrentSceneABI;

        /// <summary>
        /// 资源管理单例对象(快速访问)
        /// </summary>
        private TResource.ResourceModuleManager mRMM;

        /// <summary>
        /// 背景音乐音效组件
        /// </summary>
        private AudioSource mBGMAudioSource;

        /// <summary>
        /// 当前场景背景音乐的资源信息
        /// </summary>
        private AbstractResourceInfo mCurrentBGMARI;

        private void Awake()
        {
            DontDestroyOnLoad(this);

            DontDestroyOnLoad(UIRoot);

            initSingletons();

            addMonoComponents();

            nativeInitilization();

            initilization();
        }

        private void Start()
        {
            addListeners();
        }

        /// <summary>
        /// 添加监听
        /// </summary>
        private void addListeners()
        {
            DIYButton.LongTimePressedClick = onTButtonListenerClick;
        }

        private void Update()
        {
            ResourceModuleManager.Singleton.Update();
            TimerManager.Singleton.update(Time.deltaTime);
            UpdateManager.Singleton.update(Time.deltaTime);
        }

        private void FixedUpdate()
        {
            TimerManager.Singleton.fixedUpdate(Time.fixedDeltaTime);
            UpdateManager.Singleton.fixedUpdate(Time.fixedDeltaTime);
        }

        private void OnDestroy()
        {
            Application.logMessageReceived -= VisibleLogUtility.getInstance().HandleLog;
        }

        /// <summary>
        /// 初始化单例
        /// </summary>
        private void initSingletons()
        {
            // 因为SingletonTemplate采用的是惰性初始化(即第一次调用的时候初始化)
            // 会造成单例构造函数无法一开始被触发的问题
            AtlasManager.Singleton.startUp();
        }

        /// <summary>
        /// 添加Mono相关的组件
        /// </summary>
        private void addMonoComponents()
        {
            VisibleLogUtility visiblelog = gameObject.AddComponent<VisibleLogUtility>();
            visiblelog.setInstance(visiblelog);
            VisibleLogUtility.getInstance().mVisibleLogSwitch = FastUIEntry.LogSwitch;
            Application.logMessageReceived += VisibleLogUtility.getInstance().HandleLog;

            gameObject.AddComponent<FastUIEntry>();

            gameObject.AddComponent<CoroutineManager>();

            gameObject.AddComponent<NativeMessageHandler>();
            NativeMessageHandler.Singleton.TxtNativeOutput = TxtNativeOutput;
        }

        /// <summary>
        /// 原生初始化
        /// </summary>
        private void nativeInitilization()
        {
            NativeManager.Singleton.init();
        }

        /// <summary>
        /// 初始化
        /// </summary>
        private void initilization()
        {
            mRMM = TResource.ResourceModuleManager.Singleton;

            // 资源模块初始化
            mRMM.init();

            //热更模块初始化
            HotUpdateModuleManager.Singleton.init();

            // 预加载Shader
            //ResourceManager.Singleton.loadAllShader("shaderlist", () =>
            //{

            //},
            //ResourceLoadType.PermanentLoad);

            //初始化版本信息
            VersionConfigModuleManager.Singleton.initVerisonConfigData();

            //初始化表格数据读取
            GameDataManager.Singleton.loadAll();

            // 初始化逻辑层Manager
            GameSceneManager.Singleton.init();

            mBGMAudioSource = GetComponent<AudioSource>();
        }

        /// <summary>
        /// 加载窗口预制件
        /// </summary>
        public void onLoadWindowPrefab()
        {
            DIYLog.Log("onLoadWindowPrefab()");
#if NEW_RESOURCE
            ResourceManager.Singleton.getPrefabInstance(
                "Assets/Res/windows/MainWindow.prefab",
                (prefabInstance, requestUid) =>
                {
                    mMainWindow = prefabInstance;
                    mMainWindow.transform.SetParent(UIRootCanvas.transform, false);
                }
            );
#endif
        }

        /// <summary>
        /// 销毁窗口实例对象
        /// </summary>
        public void onDestroyWindowInstance()
        {
            DIYLog.Log("onDestroyWindowInstance()");
#if NEW_RESOURCE
            GameObject.Destroy(mMainWindow);
#endif
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
#if NEW_RESOURCE
            var image = mMainWindow.transform.Find("imgBG").GetComponent<Image>();
            AtlasManager.Singleton.setImageSingleSprite(image, param1);
#endif
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
#if NEW_RESOURCE
            AtlasManager.Singleton.setTImageSingleSprite(TImgBG, param1);
#endif
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
#if NEW_RESOURCE
            AtlasManager.Singleton.setTImageSpriteAtlas(TImgBG, param1, param2);
#endif
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
#if NEW_RESOURCE
            AtlasManager.Singleton.setTImageSpriteAtlas(TImgBG, param1, param2);
#endif
        }

        /// <summary>
        /// 加载TRawImage
        /// </summary>
        public void onLoadTRawImageSprite()
        {
            DIYLog.Log("onLoadTRawImageSprite()");
            var param1 = InputParam1.text;
            DIYLog.Log("Param1 = " + param1);
#if NEW_RESOURCE
            TRawImgBG.printTRawImageInfo();
            AtlasManager.Singleton.setRawImage(TRawImgBG, param1);
            TRawImgBG.printTRawImageInfo();
#endif
        }

        /// <summary>
        /// 播放背景音乐
        /// </summary>
        public void onPlayBGM()
        {
            DIYLog.Log("onPlayBGM()");
            var param1 = InputParam1.text;
            DIYLog.Log("Param1 = " + param1);
#if NEW_RESOURCE
            TResource.AssetLoader assetLoader;
            AudioManager.Singleton.playBGM(
                param1,
                out assetLoader
            );
#endif
        }

        /// <summary>
        /// 播放音效
        /// </summary>
        public void onPlaySound()
        {
            DIYLog.Log("onPlaySound()");
            var param1 = InputParam1.text;
            DIYLog.Log("Param1 = " + param1);
#if NEW_RESOURCE
            TResource.AssetLoader assetLoader;
            AudioManager.Singleton.playSFXSound(
                param1,
                out assetLoader
            );
#endif
        }


        /// <summary>
        /// 加载材质
        /// </summary>
        public void onLoadMaterial()
        {
            DIYLog.Log("onLoadMaterial()");
            var param1 = InputParam1.text;
            DIYLog.Log("Param1 = " + param1);
#if NEW_RESOURCE
            var btnloadmat = UIRoot.transform.Find("SecondUICanvas/ButtonGroups/btnLoadMaterial");
            var image = btnloadmat.GetComponent<Image>();
            ResourceManager.Singleton.getMaterial(
                image,
                param1,
                (material, requestUid) =>
                {
                    Material mat = material;
                    image.material = mat;
                }
            );
#endif
        }

        /// <summary>
        /// 加载角色
        /// </summary>
        public void onLoadActorPrefab()
        {
            DIYLog.Log("onLoadActorPrefab()");
#if NEW_RESOURCE
            ModelManager.Singleton.getModelInstance(
                "Assets/Res/actors/zombunny/pre_Zombunny.prefab",
                (instance, requestUid) =>
                {
                    mActorInstance = instance;
                }
            );
#endif
        }

        /// <summary>
        /// 销毁角色实例对象
        /// </summary>
        public void onDestroyActorInstance()
        {
            DIYLog.Log("onDestroyActorInstance()");
#if NEW_RESOURCE
            GameObject.Destroy(mActorInstance);
#endif
        }


        /// <summary>
        /// 预加载图集资源
        /// </summary>
        public void onPreloadAtlas()
        {
            DIYLog.Log("onPreloadAtlas()");
            var param1 = InputParam1.text;
            DIYLog.Log("Param1 = " + param1);
#if NEW_RESOURCE
            AtlasManager.Singleton.loadAtlas(param1, null, ResourceLoadType.PermanentLoad);
#endif
        }


        /// <summary>
        /// 加载常驻Shader
        /// </summary>
        public void onLoadPermanentShaderList()
        {
            DIYLog.Log("onLoadPermanentShaderList()");
#if NEW_RESOURCE
            ResourceManager.Singleton.loadAllShader("shaderlist", () =>
            {


            },
            ResourceLoadType.PermanentLoad);
#endif
        }

        /// <summary>
        /// 预加载Shader变体
        /// </summary>
        public void onPreloadShaderVariants()
        {
            DIYLog.Log("onPreloadShaderVariants()");
#if NEW_RESOURCE
            // Shader通过预加载ShaderVariantsCollection里指定的Shader来进行预编译
            TResource.AssetLoader assetLoader;
            TResource.ResourceModuleManager.Singleton.requstAssetSync<ShaderVariantCollection>(
                "Assets/Res/shadervariants/DIYShaderVariantsCollection.shadervariants",
                out assetLoader,
                (loader, requestUid) =>
                {
                    var svc = loader.getAsset<ShaderVariantCollection>();
                    // Shader通过预加载ShaderVariantsCollection里指定的Shader来进行预编译
                    svc.WarmUp();
                },
                ResourceLoadType.PermanentLoad
            );
#endif
        }


        /// <summary>
        /// 异步加载窗口
        /// </summary>
        public void onAsynLoadWindowPrefab()
        {
            DIYLog.Log("onAsynLoadWindowPrefab()");
#if NEW_RESOURCE
            if (mMainWindow == null)
            {
                onDestroyWindowInstance();
            }
            AssetLoader assetLoader;
            ResourceManager.Singleton.getPrefabInstanceAsync(
                "Assets/Res/windows/MainWindow.prefab",
                out assetLoader,
                (prefabInstance, requestUid) =>
                {
                    mMainWindow = prefabInstance;
                    mMainWindow.transform.SetParent(UIRootCanvas.transform, false);
                }
            );
#endif
        }

        /// <summary>
        /// 测试异步转同步窗口加载
        /// </summary>
        public void onAsynToSyncLoadWindow()
        {
            DIYLog.Log("onAsynToSyncLoadWindow()");
#if NEW_RESOURCE
            if (mMainWindow == null)
            {
                onDestroyWindowInstance();
            }
            AssetLoader assetLoader;
            var requestUID = ResourceManager.Singleton.getPrefabInstanceAsync(
                "Assets/Res/windows/MainWindow.prefab",
                out assetLoader,
                (prefabInstance, requestUid) =>
                {
                    mMainWindow = prefabInstance;
                    mMainWindow.transform.SetParent(UIRootCanvas.transform, false);
                }
            );
            // 将异步转同步加载
            assetLoader.loadImmediately();
#endif
        }

        /// <summary>
        /// 取消异步窗口加载请求回调
        /// </summary>
        public void onCancelAsynLoadWindow()
        {
            DIYLog.Log("onCancelAsynLoadWindow()");
#if NEW_RESOURCE
            if (mMainWindow == null)
            {
                onDestroyWindowInstance();
            }
            AssetLoader assetLoader;
            var requestUID = ResourceManager.Singleton.getPrefabInstanceAsync(
                "Assets/Res/windows/MainWindow.prefab",
                out assetLoader,
                (prefabInstance, requestUid) =>
                {
                    mMainWindow = prefabInstance;
                    mMainWindow.transform.SetParent(UIRootCanvas.transform, false);
                }
            );
            // 取消异步加载请求
            assetLoader.cancelRequest(requestUID);
#endif
        }

        /// <summary>
        /// 多异步请求单个Sprite
        /// </summary>
        public void onMultipleAsyncLoadSingleTSprite()
        {
            DIYLog.Log("onMultipleAsyncLoadTSprite()");
            var param1 = InputParam1.text;
            DIYLog.Log("Param1 = " + param1);
#if NEW_RESOURCE
            AssetLoader assetLoader1;
            AtlasManager.Singleton.setTImageSingleSpriteAsync(TImgBG, param1, out assetLoader1);
            AssetLoader assetLoader2;
            AtlasManager.Singleton.setTImageSingleSpriteAsync(TImgBG2, param1, out assetLoader2);
#endif
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
#if NEW_RESOURCE
            AssetLoader assetLoader1;
            AtlasManager.Singleton.setTImageSingleSpriteAsync(TImgBG, param1, out assetLoader1);
            AssetLoader assetLoader2;
            AtlasManager.Singleton.setTImageSingleSpriteAsync(TImgBG2, param2, out assetLoader2);
#endif
        }


        /// <summary>
        /// 异步+同步加载窗口但取消异步请求
        /// </summary>
        public void onAsyncAndSyncLoadWindowButCancelAsync()
        {
            DIYLog.Log("onAsyncAndSyncLoadWindowButCancelAsync()");
#if NEW_RESOURCE
            if (mMainWindow == null)
            {
                onDestroyWindowInstance();
            }
            AssetLoader assetLoader;
            var requestUID = ResourceManager.Singleton.getPrefabInstanceAsync(
                "Assets/Res/windows/MainWindow.prefab",
                out assetLoader,
                (prefabInstance, requestUid) =>
                {
                    Debug.Log($"getPrefabInstanceAsync()");
                    mMainWindow = prefabInstance;
                    mMainWindow.transform.SetParent(UIRootCanvas.transform, false);
                }
            );
            // 取消异步加载请求后同步加载窗口
            assetLoader.cancelRequest(requestUID);
            ResourceManager.Singleton.getPrefabInstance(
                "Assets/Res/windows/MainWindow.prefab",
                (prefabInstance, requestUid) =>
                {
                    Debug.Log($"getPrefabInstance()");
                    mMainWindow = prefabInstance;
                    mMainWindow.transform.SetParent(UIRootCanvas.transform, false);
                }
            );
#endif
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

#if NEW_RESOURCE
            //切换场景前关闭所有打开窗口，测试切场景资源卸载功能
            onDestroyWindowInstance();

            GameSceneManager.Singleton.loadSceneSync(param1);
#endif
        }

        /// <summary>
        /// 打印AB依赖信息
        /// </summary>
        public void onPrintABDepInfo()
        {
            DIYLog.Log("onPrintABDepInfo()");
            if(mRMM.CurrentResourceModule is AssetBundleModule)
            {
                (mRMM.CurrentResourceModule as AssetBundleModule).printAllResourceDpInfo();
            }
        }

        /// <summary>
        /// 打印已加载资源信息
        /// </summary>
        public void onPrintLoadedResourceInfo()
        {
            DIYLog.Log("onPrintLoadedResourceInfo()");
            mRMM.CurrentResourceModule.printAllLoadedResourceOwnersAndRefCount();
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
            VersionConfigModuleManager.Singleton.initVerisonConfigData();
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
        /// 存储最新版本信息
        /// </summary>
        public void onSaveNewVersionInfo()
        {
            DIYLog.Log("onSaveNewVersionInfo()");
            var param1 = InputParam1.text;
            DIYLog.Log("Param1 = " + param1);
            var param2 = InputParam2.text;
            DIYLog.Log("Param2 = " + param2);
            double newversioncode = 0.0f;
            int newresourceversioncode = 0;
            if (double.TryParse(param1, out newversioncode))
            {
                if (int.TryParse(param2, out newresourceversioncode))
                {
                    VersionConfigModuleManager.Singleton.saveNewVersionCodeOuterConfig(newversioncode);
                    VersionConfigModuleManager.Singleton.saveNewResoueceCodeOuterConfig(newresourceversioncode);
                }
                else
                {
                    DIYLog.LogError("新资源版本号解析出错！");
                }
            }
            else
            {
                DIYLog.LogError("新版本号解析出错！");
            }
        }

        /// <summary>
        /// 打印所有表格数据
        /// </summary>
        public void onPrintAllExcellData()
        {
            DIYLog.Log("onPrintAllExcellData()");
            var languagelist = GameDataManager.Singleton.t_languagecontainer.getList();
            foreach (var language in languagelist)
            {
                DIYLog.Log("----------------------------------------------");
                DIYLog.Log(string.Format("language id : {0}", language.id));
                DIYLog.Log(string.Format("language content : {0}", language.content));
            }
            var authorlist = GameDataManager.Singleton.t_author_Infocontainer.getList();
            foreach (var author in authorlist)
            {
                DIYLog.Log("----------------------------------------------");
                DIYLog.Log(string.Format("author id : {0}", author.id));
                DIYLog.Log(string.Format("author author : {0}", author.author));
                DIYLog.Log(string.Format("author age : {0}", author.age));
                DIYLog.Log(string.Format("author national : {0}", author.national));
                DIYLog.Log(string.Format("author sex : {0}", author.sex));
            }
            var globallist = GameDataManager.Singleton.t_globalcontainer.getList();
            foreach (var global in globallist)
            {
                DIYLog.Log("----------------------------------------------");
                DIYLog.Log(string.Format("global id : {0}", global.id));
                DIYLog.Log(string.Format("global intvalue : {0}", global.intvalue));
                DIYLog.Log(string.Format("global stringvalue : {0}", global.stringvalue));
                DIYLog.Log(string.Format("global floatvalue : {0}", global.floatvalue));
                DIYLog.Log(string.Format("global intarrayvalue : {0}", global.intarrayvalue.ToString()));
                DIYLog.Log(string.Format("global stringarrayvalue : {0}", global.stringarrayvalue.ToString()));
            }
        }

        /// <summary>
        /// 打印所有AB路径信息
        /// </summary>
        public void onPrintAllABPath()
        {
            DIYLog.Log("onPrintAllABPath()");
            AssetBundlePath.PrintAllPathInfo();
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
            HotUpdateModuleManager.Singleton.doObtainServerVersionConfig(serverVersionConfigHotUpdateCompleteCallBack);
        }

        /// <summary>
        /// 版本强更测试
        /// </summary>
        public void onTestVersionwHotUpdate()
        {
            DIYLog.Log("onTestVersionwHotUpdate()");
            if (HotUpdateModuleManager.Singleton.ServerVersionConfig != null)
            {
                if (HotUpdateModuleManager.Singleton.checkVersionHotUpdate(HotUpdateModuleManager.Singleton.ServerVersionConfig.VersionCode))
                {
                    HotUpdateModuleManager.Singleton.doNewVersionHotUpdate(HotUpdateModuleManager.Singleton.ServerVersionConfig.VersionCode, versionHotUpdateCompleteCallBack);
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
            if (HotUpdateModuleManager.Singleton.ServerVersionConfig != null)
            {
                if (HotUpdateModuleManager.Singleton.checkResourceHotUpdate(HotUpdateModuleManager.Singleton.ServerVersionConfig.ResourceVersionCode))
                {
                    HotUpdateModuleManager.Singleton.doResourceHotUpdate(HotUpdateModuleManager.Singleton.ServerVersionConfig.ResourceVersionCode, resourceHotUpdateCompleteCallBack);
                }
            }
            else
            {
                DIYLog.LogError("资源热更新前，请先获取服务器版本信息!");
            }
        }

        /// <summary>
        /// 获取服务器版本信息回调
        /// </summary>
        /// <param name="result"></param>
        private void serverVersionConfigHotUpdateCompleteCallBack(bool result)
        {
            DIYLog.Log(string.Format("获取服务器版本结果 result : {0}", result));
        }

        /// <summary>
        /// 版本强更完成回调
        /// </summary>
        /// <param name="result">版本强更结果</param>
        private void versionHotUpdateCompleteCallBack(bool result)
        {
            DIYLog.Log(string.Format("版本强更结果 result : {0}", result));
        }

        /// <summary>
        /// 资源热更完成回调
        /// </summary>
        /// <param name="result">资源热更结果</param>
        private void resourceHotUpdateCompleteCallBack(bool result)
        {
            DIYLog.Log(string.Format("资源热更结果 result : {0}", result));
        }

        /// <summary>
        /// 测试热更新完整流程
        /// </summary>
        public void onTestHotUpdateFullWorkFlow()
        {
            DIYLog.Log("onTestHotUpdateFullWorkFlow()");
            VersionConfigModuleManager.Singleton.initVerisonConfigData();
            //检测是否强更过版本
            HotUpdateModuleManager.Singleton.checkHasVersionHotUpdate();
            //TODO:
            //拉去服务器列表信息(网络那一套待开发,暂时用本地默认数值测试)
            if (HotUpdateModuleManager.Singleton.checkVersionHotUpdate(HotUpdateModuleManager.Singleton.ServerVersionConfig.VersionCode))
            {
                HotUpdateModuleManager.Singleton.doNewVersionHotUpdate(
                    HotUpdateModuleManager.Singleton.ServerVersionConfig.VersionCode,
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
                    }
                );
            }
            else
            {
                resourceHotUpdate();
            }
        }

        private void resourceHotUpdate()
        {
            //不需要强更走后判定资源热更流程
            if (HotUpdateModuleManager.Singleton.checkResourceHotUpdate(HotUpdateModuleManager.Singleton.ServerVersionConfig.ResourceVersionCode))
            {
                //单独开启一个携程打印强更进度
                StartCoroutine(printVersionHotUpdateProgressCoroutine());
                HotUpdateModuleManager.Singleton.doResourceHotUpdate(
                    HotUpdateModuleManager.Singleton.ServerVersionConfig.ResourceVersionCode,
                    (resourcehotupdateresult) =>
                    {
                        if (resourcehotupdateresult)
                        {
                            DIYLog.Log("资源热更完成!请重进或重新触发热更流程！");
                            return;
                        }
                        else
                        {
                            DIYLog.Log("资源热更出错!");
                            return;
                        }
                    }
                );
            }
            else
            {
                DIYLog.Log("无需资源热更，可以直接进入游戏！");
            }
        }

        private IEnumerator printVersionHotUpdateProgressCoroutine()
        {
            Debug.Log("printVersionHotUpdateProgressCoroutine()");
            while (HotUpdateModuleManager.Singleton.HotVersionUpdateRequest.TWRequestStatus == TWebRequest.TWebRequestStatus.TW_In_Progress)
            {
                yield return new WaitForSeconds(1.0f);
                Debug.Log(string.Format("当前版本热更进度 : {0}", HotUpdateModuleManager.Singleton.HotResourceUpdateProgress));
            }
        }

        /// <summary>
        /// 测试URL拉去
        /// </summary>
        public void onRequestURLResource()
        {
            DIYLog.Log("onRequestURLResource()");
            var url = InputParam1.text;
            DIYLog.Log("Param1 = " + url);
            TWebRequest hotResourceUpdateRequest = new TWebRequest();
            hotResourceUpdateRequest.enqueue(url, null, resourceHotUpdateCompleteCB);
            hotResourceUpdateRequest.startRequest();
        }

        /// <summary>
        /// 单个资源热更下载完成回调
        /// </summary>
        /// <param name="url">下载地址</param>
        /// <param name="fileMd5">文件MD5</param>
        /// <param name="downloadhandler">下载结果信息</param>
        /// <param name="requeststatus">请求结果</param>
        private void resourceHotUpdateCompleteCB(string url, string fileMd5, DownloadHandler downloadhandler, TWebRequest.WebRequestTaskInfo.WebTaskRequestStatus requeststatus)
        {
            DIYLog.Log($"资源URL:{url}下载结果:{requeststatus}");
        }

        /// <summary>
        /// 尝试进游戏(验证版本强更以及资源热更相关判定)
        /// </summary>
        public void onTryEnterGame()
        {
            DIYLog.Log("onTryEnterGame()");
            VersionConfigModuleManager.Singleton.initVerisonConfigData();
            if (HotUpdateModuleManager.Singleton.HotUpdateSwitch)
            {
                if (VersionConfigModuleManager.Singleton.needVersionHotUpdate(HotUpdateModuleManager.Singleton.ServerVersionConfig.VersionCode))
                {
                    DIYLog.Log(string.Format("服务器版本号 : {0}高于本地版本号 : {1}，需要强更！", HotUpdateModuleManager.Singleton.ServerVersionConfig.VersionCode, VersionConfigModuleManager.Singleton.GameVersionConfig.VersionCode));
                    DIYLog.Log("不允许进游戏！");
                }
                else
                {
                    DIYLog.Log(string.Format("服务器版本号 : {0}小于或等于本地版本号 : {1}，不需要强更！", HotUpdateModuleManager.Singleton.ServerVersionConfig.VersionCode, VersionConfigModuleManager.Singleton.GameVersionConfig.VersionCode));
                    if (VersionConfigModuleManager.Singleton.needResourceHotUpdate(HotUpdateModuleManager.Singleton.ServerVersionConfig.ResourceVersionCode))
                    {
                        DIYLog.Log(string.Format("服务器资源版本号 : {0}大于本地资源版本号 : {1}，需要资源热更！", HotUpdateModuleManager.Singleton.ServerVersionConfig.ResourceVersionCode, VersionConfigModuleManager.Singleton.GameVersionConfig.ResourceVersionCode));
                        DIYLog.Log("不允许进游戏！");
                    }
                    else
                    {
                        DIYLog.Log(string.Format("服务器资源版本号 : {0}小于或等于本地资源版本号 : {1}，不需要资源热更！", HotUpdateModuleManager.Singleton.ServerVersionConfig.ResourceVersionCode, VersionConfigModuleManager.Singleton.GameVersionConfig.ResourceVersionCode));
                        DIYLog.Log("可以进游戏!");
                    }
                }
            }
            else
            {
                DIYLog.Log("热更开关未打开，不允许热更！");
                DIYLog.Log("可以进游戏!");
            }
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
                var assetbundleresourcemodule = ResourceModuleManager.Singleton.CurrentResourceModule as AssetBundleModule;
                assetbundleresourcemodule.forceUnloadSpecificAssetBundle(param1);

            }
            else
            {
                DIYLog.Log("未处于AB状态，无法卸载指定AB!");
            }
        }

        /// <summary>
        /// 自定义按钮点击挂载指定响应
        /// </summary>
        public void onTButtonClick()
        {
            DIYLog.Log("onTButtonClick()");
        }

        /// <summary>
        /// 自定义按钮点击监听响应
        /// </summary>
        public void onTButtonListenerClick()
        {
            DIYLog.Log("onTButtonListenerClick()");
        }
    }
}