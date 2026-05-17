using UnityEngine;

namespace DLSample.Shared
{
    /// <summary>
    /// 通过JSON序列化/反序列化实现深度拷贝的工具类
    /// </summary>
    public class DeepCopyHelper
    {
        /// <summary>
        /// 对对象进行深度拷贝
        /// </summary>
        /// <typeparam name="T">对象类型</typeparam>
        /// <param name="original">原始对象</param>
        /// <returns>拷贝后的新对象</returns>
        public static T Clone<T>(T original)
        {
            string json = JsonUtility.ToJson(original);
            return JsonUtility.FromJson<T>(json);
        }
    }
}
