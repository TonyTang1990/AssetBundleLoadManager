/*
 * Description:             TButton.cs
 * Author:                  TONYTANG
 * Create Date:             2020//10/08
 */

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace TUI
{
    /// <summary>
    /// TButton.cs
    /// 重写Button组件，方便提供一些额外功能(比如按钮统一点击音效，按钮缩放表现等)
    /// </summary>
    [RequireComponent(typeof(TImage))]
    public class TButton : Button, IPointerDownHandler, IPointerUpHandler
    {
        /// <summary>
        /// 长按点击开关
        /// </summary>
        [Header("长按点击开关")]
        public bool EnableLongtimePress = false;

        /// <summary>
        /// 长按点击是否只有一次(反之无数次)
        /// </summary>
        [Header("长按点击是否只有一次(反之无数次)")]
        public bool IsLongtimePressOnlyOnce = true;

        /// <summary>
        /// 有效长按时长间隔
        /// </summary>
        [Header("有效长按时长间隔")]
        public float LongtimePressTimeInterval = 1.0f;

        /// <summary>
        /// 长按点击回调
        /// </summary>
        public Action LongTimePressedClick
        {
            get;
            set;
        }

        /// <summary>
        /// 长按点击时长
        /// </summary>
        private float mLongTimeClickPressedTimePassed;

        /// <summary>
        /// 长按点击响应次数
        /// </summary>
        private int mLongTimePressedCalledTimes;

        /// <summary>
        /// 是否点击
        /// </summary>
        private bool mIsPressed;

        public override void OnPointerDown(PointerEventData pointerEventData)
        {
            base.OnPointerDown(pointerEventData);
            mIsPressed = true;
            if (EnableLongtimePress)
            {
                mLongTimePressedCalledTimes = 0;
                mLongTimeClickPressedTimePassed = 0f;
            }
            //TODO: 统一播放音效和做动画表现
        }

        private void Update()
        {
            if(mIsPressed && EnableLongtimePress)
            {
                mLongTimeClickPressedTimePassed += Time.deltaTime;
                if (mLongTimeClickPressedTimePassed >= LongtimePressTimeInterval)
                {
                    if (mLongTimePressedCalledTimes == 0 && IsLongtimePressOnlyOnce)
                    {
                        LongTimePressedClick?.Invoke();
                    }
                    else
                    {
                        LongTimePressedClick?.Invoke();
                    }
                    mLongTimePressedCalledTimes++;
                    mLongTimeClickPressedTimePassed = 0f;
                    Debug.Log($"长按点击次数:{mLongTimePressedCalledTimes}");
                }
                Debug.Log($"长按时长:{mLongTimeClickPressedTimePassed}");
            }
        }

        public override void OnPointerUp(PointerEventData pointerEventData)
        {
            base.OnPointerUp(pointerEventData);
            if (EnableLongtimePress)
            {
                mLongTimeClickPressedTimePassed = 0f;
                mLongTimePressedCalledTimes = 0;
            }
            mIsPressed = false;
            //TODO: 统一做动画表现
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            LongTimePressedClick = null;
        }
    }
}