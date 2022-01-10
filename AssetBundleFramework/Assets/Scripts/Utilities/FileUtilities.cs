/*
 * Description:             FileUtilities.cs
 * Author:                  TONYTANG
 * Create Date:             2021//12/26
 */

using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;

/// <summary>
/// FileUtilities.cs
/// 文件静态工具类
/// </summary>
public static class FileUtilities 
{
    /// <summary>
    /// 缓存的StringBuilder
    /// </summary>
    private static StringBuilder mCacheStringBuilder = new StringBuilder();

    /// <summary>
    /// 获取指定文件的MD5值(文件不能存在返回null)
    /// </summary>
    /// <param name="filePath">文件路径</param>
    /// <param name="md5Hash">MD5算法</param>
    /// <returns></returns>
    public static string GetFileMD5(string filePath, MD5 md5Hash = null)
    {
        if(!File.Exists(filePath))
        {
            Debug.LogError($"文件路径:{filePath}不存在，获取MD5失败，请检查代码!");
            return null;
        }
        if(md5Hash == null)
        {
            md5Hash = MD5.Create();
        }
        mCacheStringBuilder.Clear();
        using (var fileFS = File.OpenRead(filePath))
        {
            var md5value = md5Hash.ComputeHash(fileFS);
            foreach (var md5byte in md5value)
            {
                mCacheStringBuilder.Append(md5byte.ToString("x2"));
            }
        }
        return mCacheStringBuilder.ToString();
    }

    /// <summary>
    /// 确保文件删除
    /// </summary>
    /// <param name="filePath"></param>
    /// <returns></returns>
    public static void DeleteFile(string filePath)
    {
        if (File.Exists(filePath))
        {
            File.Delete(filePath);
        }
    }

    /// <summary>
    /// 复制文件到指定目录
    /// </summary>
    /// <param name="filePath"></param>
    /// <param name="targetFolderPath"></param>
    /// <returns></returns>
    public static bool CopyFileToFolder(string filePath, string targetFolderPath)
    {
        if(!File.Exists(filePath))
        {
            Debug.LogError($"文件:{filePath}不存在,复制到目标目录:{targetFolderPath}失败!");
            return false;
        }
        if(string.IsNullOrEmpty(targetFolderPath))
        {
            Debug.LogError($"无法复制文件:{filePath}空目录,请传递有效目录!");
            return false;
        }
        FolderUtilities.CheckAndCreateSpecificFolder(targetFolderPath);
        var fileName = Path.GetFileName(filePath);
        var targetFilePath = Path.Combine(targetFolderPath, fileName);
        File.Copy(filePath, targetFilePath, true);
        return true;
    }


    /// <summary>
    /// 复制指定目录到指定目录
    /// </summary>
    /// <param name="sourceFolderPath">源目录</param>
    /// <param name="targetFolderPath">目标目录</param>
    /// <param name="filePostFixBlackList">文件后缀黑名单(不参与拷贝的后缀文件名列表)</param>
    /// <returns></returns>
    public static bool CopyFolderToFolder(string sourceFolderPath, string targetFolderPath, List<string> filePostFixBlackList = null)
    {
        if (!Directory.Exists(sourceFolderPath))
        {
            Debug.LogError($"原目录:{sourceFolderPath}不存在,复制到目标目录:{targetFolderPath}失败!");
            return false;
        }
        if (string.IsNullOrEmpty(targetFolderPath))
        {
            Debug.LogError($"无法复制文件:{sourceFolderPath}空目录,请传递有效目录!");
            return false;
        }
        var sourceFolderInfo = new DirectoryInfo(sourceFolderPath);
        var targetFolderInfo = new DirectoryInfo(targetFolderPath);
        CopyFilesRecursively(sourceFolderInfo, targetFolderInfo, filePostFixBlackList);
        return true;
    }

    /// <summary>
    /// 复制指定目录信息到指定目录信息
    /// </summary>
    /// <param name="source">源目录信息</param>
    /// <param name="target">目标目录信息</param>
    /// <param name="filePostFixBlackList">文件后缀黑名单(不参与拷贝的后缀文件名列表)</param>
    public static void CopyFilesRecursively(DirectoryInfo source, DirectoryInfo target, List<string> filePostFixBlackList = null)
    {
        foreach (DirectoryInfo dir in source.GetDirectories())
        {
            CopyFilesRecursively(dir, target.CreateSubdirectory(dir.Name), filePostFixBlackList);
        }
        foreach (FileInfo file in source.GetFiles())
        {
            if(filePostFixBlackList == null || (filePostFixBlackList != null && !filePostFixBlackList.Contains(file.Extension)))
            {
                file.CopyTo(Path.Combine(target.FullName, file.Name));
            }
        }
    }
}