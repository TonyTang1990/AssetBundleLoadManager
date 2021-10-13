/*
 * Description:             LoaderManager.cs
 * Author:                  TONYTANG
 * Create Date:             2021/10/13
 */

/// <summary>
/// LoaderManager.cs
/// 加载器单例管理类
/// </summary>
public class LoaderManager : SingletonTemplate<LoaderManager>
{
    /// <summary>
    /// 下一个有效请求UID
    /// </summary>
    private int mNextRequestUID;

    /// <summary>
    /// AssetBundle请求UID Map<请求UID,AssetBundle路径>
    /// </summary>
    private Dictionary<int, string> mAssetBundleRequestUIDMap;

    public LoaderManager()
    {
        mNextRequestUID = 1;
        mAssetBundleRequestUIDMap = new Dictionary<int, string>();
    }

    /// <summary>
    /// 获取下一个有效请求UID
    /// </summary>
    /// <returns></returns>
    public int GetNextRequestUID()
    {
        return mNextRequestUID++;
    }
    
    /// <summary>
    /// 添加指定AssetBundle路径请求的UID
    /// </summary>
    /// <param name="requestUID"></param>
    /// <param name="assetBundlePath"></param>
    /// <returns></returns>
    public bool addAssetBundleRequestUID(int requestUID, string assetBundlePath)
    {
        if(!mAssetBundleRequestUIDMap.ContainKey(requestUID))
        {
            mAssetBundleRequestUIDMap.Add(requestUID, assetBundlePath);
            return true;
        }
        else
        {
            Debug.LogError($"重复添加相同请求UID:{requestUID},添加AssetBundle:{assetBundlePath}请求失败!");
            return false;
        }
    }

    /// <summary>
    /// 移除指定请求UID的AssetBundle请求
    /// </summary>
    /// <param name="requestUID"></param>
    /// <returns></returns>
    public bool removeAssetBundleRequestUID(int requestUID)
    {
        if(mAssetBundleRequestUIDMap.Remove(requestUID))
        {
            return true;
        }
        else
        {
            Debug.LogError($"未添加AssetBundle请求UID:{requestUID},移除AssetBundle请求UID失败!");
            return false;
        }
    }

    /// <summary>
    /// 取消指定请求UID的AssetBundle请求
    /// </summary>
    /// <param name="requestUID"></param>
    /// <returns></returns>
    public bool cancelAssetBundleRequest(int requestUID)
    {
        string assetBundlePath = getAssetBundleByRequestUID(requestUID);
        if(string.IsNullOrEmpty(assetBundlePath))
        {
            Debug.LogError($"未找到请求UID:{requestUID}的AssetBundle加载请求,取消请求失败!")
            return false;
        }
        var assetBundleLoader = getAssetBundleLoader(assetBundlePath);
        return assetBundleLoader != null ? assetBundleLoader.cancelRequest(requestUID) : false;
    }

    /// <summary>
    /// 获取指定AssetBundle路径的加载器
    /// </summary>
    /// <param name="assetBundlePath"></param>
    /// <returns></returns>
    public AssetBundleLoader getAssetBundleLoader(string assetBundlePath)
    {
        AssetBundleLoader abLoader;
        //if(AllAssetBundleLoaderMap.TryGetValue(assetBundlePath, out abLoader))
        //{
        //    return abLoader;
        //}
        return null;
    }

    /// <summary>
    /// 获取指定请求UID的AssetBundle路径
    /// </summary>
    /// <param name="requestUID"></param>
    /// <returns></returns>
    private string getAssetBundleByRequestUID(int requestUID)
    {
        string assetBundlePath;
        if(mAssetBundleRequestUIDMap.TryGetValue(requestUID, out assetBundlePath))
        {
            return assetBundlePath;
        }
        else
        {
            Debug.LogError($"AssetBundle未添加请求UID:{requestUID}的请求,获取AssetBundle路径失败!");
            return null;
        }
    }
}