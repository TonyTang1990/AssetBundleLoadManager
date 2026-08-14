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

namespace TResource
{
    /// <summary>
    /// TWebRequest.cs
    /// Web任务访问封装
    /// </summary>
    public class TWebRequest
    {
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
            /// 文件Sha256用于校验
            /// </summary>
            public string FileSha256
            {
                get;
                private set;
            }

            /// <summary>
            /// 任务完成回调
            /// </summary>
            public Action<string, string, DownloadHandler, WebTaskRequestStatus, object> CompleteCallback
            {
                get;
                private set;
            }

            /// <summary>
            /// 自定义数据
            /// </summary>
            public object CustomData
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
            /// <param name="fileSha256">文件Sha256(不传表示不校验)</param>
            /// <param name="callback"></param>
            /// <param name="customData">自定义数据</param>
            /// <param name="timeout"></param>
            public WebRequestTaskInfo(string url, string fileSha256,
                                    Action<string, string, DownloadHandler, WebTaskRequestStatus, object> callback,
                                    object customData = null,
                                    int timeout = 300)
            {
                URL = url;
                FileSha256 = fileSha256;
                CompleteCallback = callback;
                CustomData = customData;
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
        /// 是否完成
        /// </summary>
        public bool IsComplete
        {
            get
            {
                return TWRequestStatus == TWebRequestStatus.TW_Comlete || TWRequestStatus == TWebRequestStatus.TW_Stop;
            }
        }

        /// <summary>
        /// 当前进度
        /// </summary>
        public float CurrentProgress
        {
            get
            {
                float progress = mTotalWebRequestNumber - mWebRequestTaskQueue.Count;
                var runningLeftProgress = 0f;
                if(mCurrentInProgressWebRequest != null && !mCurrentInProgressWebRequest.isDone)
                {
                    runningLeftProgress = (1 - mCurrentInProgressWebRequest.downloadProgress);
                    runningLeftProgress = Math.Clamp(runningLeftProgress, 0f, 1f);
                }
                progress = progress - runningLeftProgress;
                return progress / mTotalWebRequestNumber;
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
        /// <param name="fileSha256">文件Sha256</param>
        /// <param name="completecallback">完成回调</param>
        /// <param name="customData">自定义数据</param>
        /// <param name="timeout">超时时间</param>
        public void Enqueue(string url, string fileSha256,
                            Action<string, string, DownloadHandler, WebRequestTaskInfo.WebTaskRequestStatus, object> completecallback,
                            object customData = null, int timeout = 300)
        {
            if(TWRequestStatus != TWebRequestStatus.TW_In_Progress)
            {
                if(!url.IsNullOrEmpty() && completecallback != null)
                {
                    var newtask = new WebRequestTaskInfo(url, fileSha256, completecallback, customData, timeout);
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
        public void StartRequest()
        {
            if(TWRequestStatus != TWebRequestStatus.TW_In_Progress)
            {
                if (mWebRequestTaskQueue.Count > 0)
                {
                    mTotalWebRequestNumber = mWebRequestTaskQueue.Count;
                    CoroutineManager.GetInstance().StartCoroutine(RequestCoroutine());
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
        public void StopRequest()
        {
            TWRequestStatus = TWebRequestStatus.TW_Stop;
        }

        /// <summary>
        /// 继续资源请求任务
        /// </summary>
        public void ResumeRequest()
        {
            TWRequestStatus = TWebRequestStatus.TW_In_Progress;
        }

        /// <summary>
        /// 重置请求
        /// </summary>
        public void ResetRequest()
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
        private IEnumerator RequestCoroutine()
        {
            TWRequestStatus = TWebRequestStatus.TW_In_Progress;

            while (mWebRequestTaskQueue.Count > 0 && TWRequestStatus == TWebRequestStatus.TW_In_Progress)
            {
                //TODO:
                //Sha256信息做资源验证，确保资源下对以及未被修改
                var task = mWebRequestTaskQueue.Dequeue();
                Debug.Log($"下载资源:{task.URL}");
                mCurrentInProgressWebRequest = UnityWebRequest.Get(task.URL);
                mCurrentInProgressWebRequest.timeout = task.TimeOut;
                yield return mCurrentInProgressWebRequest.SendWebRequest();
                if (mCurrentInProgressWebRequest.isNetworkError || mCurrentInProgressWebRequest.isHttpError)
                {
                    Debug.LogError($"{task.URL}资源下载出错!");
                    Debug.LogError(mCurrentInProgressWebRequest.error);
                    if(mCurrentInProgressWebRequest.isHttpError)
                    {
                        Debug.LogError($"responseCode:{mCurrentInProgressWebRequest.responseCode}");
                    }
                    task.CompleteCallback(task.URL, task.FileSha256, mCurrentInProgressWebRequest.downloadHandler,
                                        WebRequestTaskInfo.WebTaskRequestStatus.WT_Faield, task.CustomData);
                }
                else
                {
                    Debug.Log($"{task.URL} webrequest.isDone:{mCurrentInProgressWebRequest.isDone}!");
                    Debug.Log($"{task.URL}资源下载完成!");
                    task.CompleteCallback(task.URL, task.FileSha256, mCurrentInProgressWebRequest.downloadHandler,
                                        WebRequestTaskInfo.WebTaskRequestStatus.WT_Complete, task.CustomData);
                }
            }

            if(mWebRequestTaskQueue.Count == 0)
            {
                TWRequestStatus = TWebRequestStatus.TW_Comlete;
                mCurrentInProgressWebRequest = null;
            }
        }
    }
}