/*
 * Description:             模板数据抽象类
 * Author:                  tanghuan
 * Create Date:             2018/09/03
 */

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace XbufferExcelToData
{
    /// <summary>
    /// 模板数据抽象类
    /// </summary>
    public class TTemplate
    {
        /// <summary> 整个模板内容 /// </summary>
        private string mContent;

        /// <summary> 循环模板内容 /// </summary>
        private string mLoopTemplateContent;

        /// <summary> 循环模板替换单次循环时的内容 /// </summary>
        private string mSingleLoopContent;

        /// <summary> 循环模板替换时用于临时存储所有循环内容综合的变量 /// </summary>
        private string mLoopAllContent;

        /// <summary> 是否处于循环模板替换中 /// </summary>
        private bool mIsLooping;

        /// <summary> 多次替换模板占位符 /// </summary>
        private const string LOOP_HOLDER = "#LOOP_HOLDER#";

        /// <summary> 占位符匹配正则 /// </summary>
        private const string TagPattern = "{0}(\\s*(\\S|\\s)*)\\s*{0}";

        public TTemplate()
        {
            mContent = string.Empty;
            mLoopTemplateContent = string.Empty;
            mSingleLoopContent = string.Empty;
            mLoopAllContent = string.Empty;
            mIsLooping = false;
        }

        public TTemplate(string content)
        {
            mContent = content;
            mLoopTemplateContent = string.Empty;
            mSingleLoopContent = string.Empty;
            mLoopAllContent = string.Empty;
            mIsLooping = false;
        }


        /// <summary>
        /// 重置模板内容
        /// </summary>
        /// <param name="newcontent"></param>
        public void resetContent(string newcontent)
        {
            mContent = newcontent;
            mLoopTemplateContent = string.Empty;
            mSingleLoopContent = string.Empty;
            mLoopAllContent = string.Empty;
            mIsLooping = false;
        }

        /// <summary>
        /// 开始循环模板替换
        /// </summary>
        /// <param name="looptag">循环模板标签</param>
        public void beginLoop(string looptag)
        {
            if(!Regex.IsMatch(mContent, string.Format(TagPattern, looptag)))
            {
                Console.WriteLine(string.Format("找不到匹配的循环模板标签 : {0}", looptag));
                return;
            }

            mIsLooping = true;

            // 替换循环模板部分为循环占位符，后续循环内容填充完毕后再替换回来
            mContent = Regex.Replace(mContent, string.Format(TagPattern, looptag), (match) =>
            {
                // 记录下最新的循环模板内容(已排除循环模板标签)
                mLoopTemplateContent = match.Groups[1].Value;
                mSingleLoopContent = mLoopTemplateContent;
                return LOOP_HOLDER;
            });
        }

        /// <summary>
        /// 下一步循环(循环模板内容填充完毕，填充下一次内容)
        /// </summary>
        public void nextLoop()
        {
            // 存储前一次循环模板的数据
            mLoopAllContent += mSingleLoopContent;

            // 重置循环模板内容
            mSingleLoopContent = mLoopTemplateContent;
        }

        /// <summary>
        /// 结束循环模板替换
        /// </summary>
        public void endLoop()
        {
            mIsLooping = false;
            // 替换循环模板临时占位符为最终内容数据，并重置循环模板相关数据状态
            mContent = mContent.Replace(LOOP_HOLDER, mLoopAllContent);
            mLoopAllContent = string.Empty;
            mSingleLoopContent = string.Empty;
            mLoopTemplateContent = string.Empty;
        }

        /// <summary>
        /// 设置指定标签的值
        /// </summary>
        /// <param name="tag">标签</param>
        /// <param name="value">替换值</param>
        public void setValue(string tag, string value)
        {
            if(mIsLooping)
            {
                // 循环替换模式下，实际替换的是基于循环替换模板所存储的替换后内容
                mSingleLoopContent = mSingleLoopContent.Replace(tag, value);
            }
            else
            {
                // 单次替换模式直接替换即可
                mContent = mContent.Replace(tag, value);
            }
        }

        /// <summary>
        /// 获取最新内容
        /// </summary>
        /// <returns></returns>
        public string getContent()
        {
            return mContent;
        }
    }
}
