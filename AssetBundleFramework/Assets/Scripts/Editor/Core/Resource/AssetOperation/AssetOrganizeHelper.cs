/*
 * Description:             普通类
 * Author:                  tanghuan
 * Create Date:             2018/02/26
 */

using System.IO;

using UnityEngine;
using UnityEditor;

/// <summary>
/// Asset整理辅助工具类
/// </summary>
public static class AssetOrganizeHelper 
{
    private const string MetaPosFix = ".meta";

    /// <summary>
    /// 复制文件以及Meta文件
    /// </summary>
    /// <param name="assetpath">asset路径(相对Asset路径)</param>
    /// <param name="destfolderpath">文件目的地目录路径(相对Asset路径)</param>
    public static void moveFileAndMeta(string assetpath, string destfolderpath)
    {
        if(!checkFileExist(assetpath))
        {
            Debug.Log(string.Format("文件不存在:{0}无法移动!", assetpath));
        }
        else
        {
            var assetfullpath = Path.GetFullPath(assetpath);
            var assetfilename = Path.GetFileName(assetpath);
            var destfullpath = Path.GetFullPath(destfolderpath) + "\\" + assetfilename;
            checkAndCreateFolder(destfolderpath);
            Debug.Log("moveFileAndMeta();");
            Debug.Log(string.Format("assetfullpath : {0}", assetfullpath));
            Debug.Log(string.Format("destfullpath : {0}", destfullpath));
            File.Move(assetfullpath, destfullpath);
            var metafullpath = assetfullpath + MetaPosFix;
            var metadestfullpath = destfullpath + MetaPosFix;
            File.Move(metafullpath, metadestfullpath);
        }
    }

    /// <summary>
    /// 删除文件以及meta文件
    /// </summary>
    /// <param name="assetpath">asset路径(相对Asset路径)</param>
    public static void deleteFileAndMeta(string assetpath)
    {
        if (!checkFileExist(assetpath))
        {
            Debug.Log(string.Format("文件不存在:{0}无法删除!", assetpath));
        }
        else
        {
            var assetfullpath = Path.GetFullPath(assetpath);
            Debug.Log("deleteFileAndMeta();");
            Debug.Log(string.Format("assetfullpath : {0}", assetfullpath));
            var metafullpath = assetfullpath + MetaPosFix;
            File.Delete(assetfullpath);
            File.Delete(metafullpath);
        }
    }

    /// <summary>
    /// 检查导出目录是否存在，不存在就创建一个
    /// <param name="folderpath">目录路径</param>
    /// </summary>
    public static void checkAndCreateFolder(string folderpath)
    {
        if (!Directory.Exists(folderpath))
        {
            Directory.CreateDirectory(folderpath);
        }
    }

    /// <summary>
    /// 检查文件是否存在
    /// </summary>
    /// <param name="filepath">文件路径</param>
    /// <returns></returns>
    public static bool checkFileExist(string filepath)
    {
        return File.Exists(filepath);
    }
}
