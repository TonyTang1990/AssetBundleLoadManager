/*
 * Description:             ResourceModuleEnum.cs
 * Author:                  TONYTANG
 * Create Date:             2019//01/21
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// ��Դ���ع�������ö�ٶ���

/// <summary>
/// AB��Դ���ط�ʽ
/// </summary>
public enum ABLoadMethod
{
    Sync = 1,          // ͬ��
    Async = 2          // �첽
}

/// <summary>
/// AB��Դ��������
/// Note:
/// �Ѽ��ص���Դ���������������ģ���ֻ�����ӵ����߱�(NormalLoad -> Preload -> PermanentLoad)���������Ӹ�����(PermanentLoad -> Preload -> NormalLoad)
/// </summary>
public enum ABLoadType
{
    NormalLoad = 1,         // ��������(��ͨ��Tick�����ж�����ж��)
    Preload = 2,            // Ԥ����(�г����Ż�ж��)
    PermanentLoad = 3,      // ���ü���(��פ�ڴ�����ж��)
}

/// <summary>
/// ��дABLoadType�Ƚ����ؽӿں���������ABLoadType��ΪDictionary Keyʱ��
/// �ײ�����Ĭ��Equals(object obj)��DefaultCompare.GetHashCode()���¶����Ķ��ڴ�����
/// �ο�:
/// http://gad.qq.com/program/translateview/7194373
/// </summary>
public class ABLoadTypeComparer : IEqualityComparer<ABLoadType>
{
    public bool Equals(ABLoadType x, ABLoadType y)
    {
        return x == y;
    }

    public int GetHashCode(ABLoadType x)
    {
        return (int)x;
    }
}

/// <summary>
/// AB��������״̬
/// </summary>
public enum ABLoadState
{
    None = 1,             // δ����״̬
    Loading = 2,          // ������״̬
    Complete = 3,         // ��������״̬
    Error = 4             // ����״̬
}
