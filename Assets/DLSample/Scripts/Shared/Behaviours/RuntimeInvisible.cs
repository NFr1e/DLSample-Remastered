using UnityEngine;

namespace DLSample.Shared
{
    /// <summary>
    /// 运行时将自身Renderer设为不可见
    /// </summary>
    [RequireComponent(typeof(Renderer))]
    public class RuntimeInvisible : MonoBehaviour
    {
        private Renderer _renderer;

        private void Awake()
        {
            _renderer = GetComponent<Renderer>();
        }

        private void Start()
        {
            _renderer.enabled = false;
        }
    }
}
