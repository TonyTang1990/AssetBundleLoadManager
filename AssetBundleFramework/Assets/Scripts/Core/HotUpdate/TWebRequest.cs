/*
 * Description:             TWebRequest.cs
 * Author:                  TONYTANG
 * Create Date:             2019//04/21
 */

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

/// <summary>
/// TWebRequest.cs
/// Web任务访问封装
/// </summary>
public class TWebRequest {

    /// <summary>
    /// Web任务请求状态
    /// </summary>
    public enum TWebRequestStatus
    {
        TW_Wait_Start,              // 等待开始
        TW_In_Progress,             // 进行中
        TW_Stop,                    // 停止
        TW_Comlete,                 // 完成
    }

    /// <summary>
    /// Web请求任务信息抽象
    /// </summary>
    public class WebRequestTaskInfo
    {
        /// <summary>
        /// Web请求任务访问状态
        /// </summary>
        public enum WebTaskRequestStatus
        {
            WT_Faield,             // 失败
            WT_Complete            // 完成
        }
        
        /// <summary>
        /// 任务URL
        /// </summary>
        public string URL
        {
            get;
            private set;
        }

        /// <summary>
        /// 任务完成回调
        /// </summary>
        public Action<string, DownloadHandler, WebTaskRequestStatus> CompleteCallback
        {
            get;
            private set;
        }

        /// <summary>
        /// 任务超时时间
        /// </summary>
        public int TimeOut
        {
            get;
            private set;
        }

        /// <summary>
        /// Web请求任务信息构造函数
        /// </summary>
        /// <param name="url"></param>
        /// <param name="callback"></param>
        /// <param name="timeout"></param>
        public WebRequestTaskInfo(string url, Action<string, DownloadHandler, WebTaskRequestStatus> callback, int timeout)
        {
            URL = url;
            CompleteCallback = callback;
            TimeOut = timeout;
        }
    }

    /// <summary>
    /// Web访问任务队列
    /// </summary>
    private Queue<WebRequestTaskInfo> mWebRequestTaskQueue;

    /// <summary>
    /// Web任务请求状态
    /// </summary>
    public TWebRequestStatus TWRequestStatus
    {
        get;
        private set;
    }

    public TWebRequest()
    {
        mWebRequestTaskQueue = new Queue<WebRequestTaskInfo>();
        TWRequestStatus = TWebRequestStatus.TW_Wait_Start;
    }

    /// <summary>
    /// 请求任务入队列
    /// </summary>
    /// <param name="url">url</param>
    /// <param name="completecallback">完成回调</param>
    /// <param name="timeout">超时时间</param>
    public void enqueue(string url, Action<string, DownloadHandler, WebRequestTaskInfo.WebTaskRequestStatus> completecallback, int timeout = 5)
    {
        if(TWRequestStatus != TWebRequestStatus.TW_In_Progress)
        {
            if(!url.IsNullOrEmpty() && completecallback != null)
            {
                var newtask = new WebRequestTaskInfo(url, completecallback, timeout);
                mWebRequestTaskQueue.Enqueue(newtask);
            }
            else
            {
                DIYLog.LogError("URL和completecallback都不能为空！添加任务失败！");
            }
        }
        else
        {
            DIYLog.LogError("已经在请求中，无法添加任务！");
        }
    }

    /// <summary>
    /// 开始请求资源任务
    /// </summary>
    public void startRequest()
    {
        if(TWRequestStatus != TWebRequestStatus.TW_In_Progress)
        {
            if (mWebRequestTaskQueue.Count > 0)
            {
                CoroutineManager.Singleton.startCoroutine(requestCoroutine());
            }
            else
            {
                DIYLog.LogWarning("没有任务信息，无法开始请求！");
            }
        }
        else
        {
            DIYLog.LogWarning("已经在请求中，无法开始请求！");
        }
    }

    /// <summary>
    /// 停止资源请求任务
    /// </summary>
    public void stopRequest()
    {
        TWRequestStatus = TWebRequestStatus.TW_Stop;
    }

    /// <summary>
    /// 重置请求
    /// </summary>
    public void resetRequest()
    {
        mWebRequestTaskQueue.Clear();
        TWRequestStatus = TWebRequestStatus.TW_Wait_Start;
    }

    /// <summary>
    /// 任务请求携程
    /// </summary>
    /// <returns></returns>
    private IEnumerator requestCoroutine()
    {
        TWRequestStatus = TWebRequestStatus.TW_In_Progress;

        while(mWebRequestTaskQueue.Count > 0 && TWRequestStatus == TWebRequestStatus.TW_In_Progress)
        {
            var task = mWebRequestTaskQueue.Dequeue();
            DIYLog.Log(string.Format("下载资源 : {0}", task.URL));
            var webrequest = UnityWebRequest.Get(task.URL);
            webrequest.timeout = task.TimeOut;
            yield return webrequest.SendWebRequest();
            if (webrequest.isNetworkError)
            {
                DIYLog.LogError(string.Format("{0}资源下载出错!", task.URL));
                DIYLog.LogError(webrequest.error);
                if(webrequest.isHttpError)
                {
                    DIYLog.LogError(string.Format("responseCode : ", webrequest.responseCode));
                }
                task.CompleteCallback(task.URL, webrequest.downloadHandler, WebRequestTaskInfo.WebTaskRequestStatus.WT_Faield);
            }
            else
            {
                DIYLog.Log(string.Format("{0} webrequest.isDone:{1}!", task.URL, webrequest.isDone));
                DIYLog.Log(string.Format("{0}资源下载完成!", task.URL));
                task.CompleteCallback(task.URL, webrequest.downloadHandler, WebRequestTaskInfo.WebTaskRequestStatus.WT_Complete);
            }
        }

        if(mWebRequestTaskQueue.Count == 0)
        {
            TWRequestStatus = TWebRequestStatus.TW_Comlete;
        }
    }
}