/*
 * Description:             FileUtilities.cs
 * Author:                  TONYTANG
 * Create Date:             2021//12/26
 */

using System;
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
    private static StringBuilder CacheStringBuilder = new StringBuilder();
    
    /// <summary>
    /// MD5生成算法
    /// </summary>
    private static MD5 MD5 = MD5.Create();

    /// <summary>
    /// Sha256生成算法
    /// </summary>
    private static SHA256 SHA256 = SHA256.Create();

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
        md5Hash = md5Hash != null ? md5Hash : MD5;
        CacheStringBuilder.Clear();
        using (var fileFS = File.OpenRead(filePath))
        {
            var md5value = md5Hash.ComputeHash(fileFS);
            foreach (var md5byte in md5value)
            {
                CacheStringBuilder.Append(md5byte.ToString("x2"));
            }
        }
        return CacheStringBuilder.ToString();
    }

    /// <summary>
    /// 转换Sha256的Bytes值(字节数组为空返回null)
    /// </summary>
    /// <param name="bytes"></param>
    /// <returns></returns>
    public static string ConvertSha256Bytes(byte[] bytes)
    {
        if(bytes == null || bytes.Length == 0)
        {
            Debug.LogError($"字节数组为空，转换Sha256失败，请检查代码!");
            return null;
        }
        return BitConverter.ToString(bytes).Replace("-", string.Empty).ToLowerInvariant();
    }

    /// <summary>
    /// 获取指定文件的Sha256值(文件不能存在返回null)
    /// </summary>
    /// <param name="filePath">文件路径</param>
    /// <param name="sha256Hash">Sha256算法</param>
    /// <returns></returns>
    public static string GetFileSha256(string filePath, SHA256 sha256Hash = null)
    {
        if(!File.Exists(filePath))
        {
            Debug.LogError($"文件路径:{filePath}不存在，获取Sha256失败，请检查代码!");
            return null;
        }
        sha256Hash = sha256Hash != null ? sha256Hash :SHA256;
        CacheStringBuilder.Clear();
        using (var fileFS = File.OpenRead(filePath))
        {
            var hash = sha256Hash.ComputeHash(fileFS);
            return ConvertSha256Bytes(hash);
        }
    }

    /// <summary>
    /// 获取指定字节数组的Sha256值(字节数组为空返回null)
    /// </summary>
    /// <param name="bytes"></param>
    /// <param name="sha256Hash"></param>
    /// <returns></returns>
    public static string GetBytesSha256(byte[] bytes, SHA256 sha256Hash = null)
    {
        if(bytes == null || bytes.Length == 0)
        {
            Debug.LogError($"字节数组为空，获取Sha256失败，请检查代码!");
            return null;
        }
        sha256Hash = sha256Hash != null ? sha256Hash :SHA256;
        CacheStringBuilder.Clear();
        var hash = sha256Hash.ComputeHash(bytes);
        return ConvertSha256Bytes(hash);
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
    /// 复制文件到指定文件路径
    /// </summary>
    /// <param name="filePath"></param>
    /// <param name="targetFilePath"></param>
    /// <returns></returns>
    public static bool CopyFileToFile(string filePath, string targetFilePath)
    {
        if(!File.Exists(filePath))
        {
            Debug.LogError($"文件:{filePath}不存在,复制到目标文件:{targetFilePath}失败!");
            return false;
        }
        if(string.IsNullOrEmpty(targetFilePath))
        {
            Debug.LogError($"无法复制文件:{filePath}空目标文件路径,请传递有效目录!");
            return false;
        }
        var targetFolderPath = Path.GetDirectoryName(targetFilePath);
        FolderUtilities.CheckAndCreateSpecificFolder(targetFolderPath);
        File.Copy(filePath, targetFilePath, true);
        return true;
    }

    /// <summary>
    /// 复制文件到指定目录
    /// </summary>
    /// <param name="filePath"></param>
    /// <param name="targetFolderPath"></param>
    /// <returns></returns>
    public static bool CopyFileToFolder(string filePath, string targetFolderPath, out string newFilePath)
    {
        newFilePath = string.Empty;
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
        newFilePath = Path.Combine(targetFolderPath, fileName);
        File.Copy(filePath, newFilePath, true);
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

    /// <summary>
    /// 获取指定文件路径重命名后的路径
    /// </summary>
    /// <param name="oldFilePath"></param>
    /// <param name="newFileName"></param>
    /// <returns></returns>
    public static string GetFileRenameNewPath(string oldFilePath, string newFileName)
    {
        if(string.IsNullOrEmpty(oldFilePath))
        {
            Debug.LogError($"无法获取重命名空文件路径:{oldFilePath}的重命名路径!");
            return null;
        }
        string directoryPath = Path.GetDirectoryName(oldFilePath);
        string newFilePath = Path.Combine(directoryPath, newFileName);
        return newFilePath;
    }

    /// <summary>
    /// 重命名指定文件路径到指定文件路径
    /// </summary>
    /// <param name="oldFilePath"></param>
    /// <param name="newFileName"></param>
    /// <returns>返回null表示改名失败</returns>
    public static string RenameFile(string oldFilePath, string newFileName)
    {
        if(string.IsNullOrEmpty(oldFilePath))
        {
            Debug.LogError($"无法重命名空文件路径:{oldFilePath}!");
            return null;
        }
        if(!File.Exists(oldFilePath))
        {
            Debug.LogError($"文件路径:{oldFilePath}不存在,无法重命名!");
            return null;
        }
        string newFilePath = GetFileRenameNewPath(oldFilePath, newFileName);
        if(string.Equals(oldFilePath, newFilePath))
        {
            Debug.LogWarning($"文件路径:{oldFilePath}和新文件路径:{newFilePath}相同,无需重命名!");
            return newFilePath;
        }
        File.Move(oldFilePath, newFilePath);
        return newFilePath;
    }

    /// <summary>
    /// 检查指定文件的Sha256值是否与目标Sha256值一致
    /// </summary>
    /// <param name="filePath"></param>
    /// <param name="targetSha256"></param>
    /// <param name="sha256"></param>
    /// <returns></returns>
    public static (bool, string) CheckFileSha256(string filePath, string targetSha256, SHA256 sha256 = null)
    {
        var fileSha256 = GetFileSha256(filePath, sha256);
        var result = string.Equals(fileSha256, targetSha256);
        return (result, fileSha256);
    }
}