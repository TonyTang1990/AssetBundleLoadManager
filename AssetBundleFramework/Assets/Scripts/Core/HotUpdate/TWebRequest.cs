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

    /// <summary>
    /// 当前进度
    /// 当前进度的计算方式：
    /// (完成的任务个数 + 进行时的任务进度) / 总的任务个数
    /// Note:
    /// 任务未开始没有进度信息
    /// </summary>
    public float CurrentProgress
    {
        get
        {
            return (mTotalWebRequestNumber - mWebRequestTaskQueue.Count + (1 - (mCurrentInProgressWebRequest != null ? mCurrentInProgressWebRequest.downloadProgress : 0.0f))) / mTotalWebRequestNumber;
        }
    }

    /// <summary>
    /// 当前正在执行的Web请求任务
    /// </summary>
    private UnityWebRequest mCurrentInProgressWebRequest;

    /// <summary>
    /// 总的任务个数
    /// </summary>
    private int mTotalWebRequestNumber;

    public TWebRequest()
    {
        mWebRequestTaskQueue = new Queue<WebRequestTaskInfo>();
        TWRequestStatus = TWebRequestStatus.TW_Wait_Start;
        mCurrentInProgressWebRequest = null;
        mTotalWebRequestNumber = 0;
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
                Debug.LogError("URL和completecallback都不能为空！添加任务失败！");
            }
        }
        else
        {
            Debug.LogError("已经在请求中，无法添加任务！");
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
                mTotalWebRequestNumber = mWebRequestTaskQueue.Count;
                CoroutineManager.Singleton.startCoroutine(requestCoroutine());
            }
            else
            {
                Debug.LogWarning("没有任务信息，无法开始请求！");
            }
        }
        else
        {
            Debug.LogWarning("已经在请求中，无法开始请求！");
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
    /// 继续资源请求任务
    /// </summary>
    public void resumeRequest()
    {
        TWRequestStatus = TWebRequestStatus.TW_In_Progress;
    }

    /// <summary>
    /// 重置请求
    /// </summary>
    public void resetRequest()
    {
        mWebRequestTaskQueue.Clear();
        TWRequestStatus = TWebRequestStatus.TW_Wait_Start;
        mCurrentInProgressWebRequest = null;
        mTotalWebRequestNumber = 0;
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
            Debug.Log(string.Format("下载资源 : {0}", task.URL));
            mCurrentInProgressWebRequest = UnityWebRequest.Get(task.URL);
            mCurrentInProgressWebRequest.timeout = task.TimeOut;
            yield return mCurrentInProgressWebRequest.SendWebRequest();
            if (mCurrentInProgressWebRequest.isNetworkError)
            {
                Debug.LogError(string.Format("{0}资源下载出错!", task.URL));
                Debug.LogError(mCurrentInProgressWebRequest.error);
                if(mCurrentInProgressWebRequest.isHttpError)
                {
                    Debug.LogError(string.Format("responseCode : ", mCurrentInProgressWebRequest.responseCode));
                }
                task.CompleteCallback(task.URL, mCurrentInProgressWebRequest.downloadHandler, WebRequestTaskInfo.WebTaskRequestStatus.WT_Faield);
            }
            else
            {
                Debug.Log(string.Format("{0} webrequest.isDone:{1}!", task.URL, mCurrentInProgressWebRequest.isDone));
                Debug.Log(string.Format("{0}资源下载完成!", task.URL));
                task.CompleteCallback(task.URL, mCurrentInProgressWebRequest.downloadHandler, WebRequestTaskInfo.WebTaskRequestStatus.WT_Complete);
            }
        }

        if(mWebRequestTaskQueue.Count == 0)
        {
            TWRequestStatus = TWebRequestStatus.TW_Comlete;
            mCurrentInProgressWebRequest = null;
        }
    }
}