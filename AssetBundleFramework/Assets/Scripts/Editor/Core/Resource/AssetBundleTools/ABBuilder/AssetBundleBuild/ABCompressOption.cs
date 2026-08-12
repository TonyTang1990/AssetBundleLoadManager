/*
 * Description:             ABCompressOption.cs
 * Author:                  TONYTANG
 * Create Date:             2026//08/11
 */

namespace TResource
{
    /// <summary>
    /// ABCompressOption.cs
    /// AssetBundle压缩选项
    /// </summary>
    public enum ABCompressOption
    {
        Uncompressed = 0,
        StandardCompressionLZMA,
        ChunkBasedCompressionLZ4,
    }
}