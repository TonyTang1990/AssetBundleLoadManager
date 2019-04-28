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
    /// Web访问状态
    /// </summary>
    public enum WebRequestStatus
    {
        WRP_None = 1,           // 无状态
        WRP_Faield,             // 失败
        WRP_Complete            // 完成
    }

    /// <summary>
    /// Web请求任务信息抽象
    /// </summary>
    private class WebRequestTaskInfo
    {
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
        public Action<string, WebRequestStatus> CompleteCallback
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
        public WebRequestTaskInfo(string url, Action<string, WebRequestStatus> callback, int timeout)
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
    /// 是否在请求中
    /// </summary>
    private bool mIsInProgress;

    public TWebRequest()
    {
        mWebRequestTaskQueue = new Queue<WebRequestTaskInfo>();
        mIsInProgress = false;
    }

    /// <summary>
    /// 请求任务入队列
    /// </summary>
    /// <param name="url">url</param>
    /// <param name="completecallback">完成回调</param>
    /// <param name="timeout">超时时间</param>
    public void enqueue(string url, Action<string, WebRequestStatus> completecallback, int timeout = 5)
    {
        if(mIsInProgress == false)
        {
            if(!url.IsNullOrEmpty() && completecallback != null)
            {
                var newtask = new WebRequestTaskInfo(url, completecallback, timeout);
                mWebRequestTaskQueue.Equals(newtask);
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
        if(!mIsInProgress)
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
    /// 任务请求携程
    /// </summary>
    /// <returns></returns>
    private IEnumerator requestCoroutine()
    {
        mIsInProgress = true;

        while(mWebRequestTaskQueue.Count > 0)
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
                task.CompleteCallback(task.URL, WebRequestStatus.WRP_Faield);
            }
            else
            {
                DIYLog.Log(string.Format("{0} webrequest.isDone:{1}!", task.URL, webrequest.isDone));
                DIYLog.Log(string.Format("{0}资源下载完成!", task.URL));
                task.CompleteCallback(task.URL, WebRequestStatus.WRP_Faield);
            }
        }
    }
}