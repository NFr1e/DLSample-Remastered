using UnityEngine;

namespace DLSample.Shared
{
    /// <summary>
    /// 基础碰撞体绘制器，在Scene视图中绘制碰撞体的轮廓和填充
    /// </summary>
    [ExecuteAlways][RequireComponent(typeof(Collider))]
    public class BasicColliderDrawer : MonoBehaviour
    {
        [SerializeField] private bool drawOutline = true;
        [SerializeField] private Color outlineColor = new(0, 1, 0, 1);

        [SerializeField] private bool drawFill = false;
        [SerializeField] private Color fillColor = new(0, 1, 0, 0.5f);

        private Collider _collider;

        private void OnEnable()
        {
            _collider = GetComponent<Collider>();
        }

        private void OnDrawGizmos()
        {
            if (!enabled) return;

            if (_collider == null)
                _collider = GetComponent<Collider>();

            if (_collider == null)
                return;

            Draw(_collider);
        }

        private void Draw(Collider collider)
        {
            Matrix4x4 previousMatrix = Gizmos.matrix;
            Gizmos.matrix = transform.localToWorldMatrix;

            if (drawOutline)
            {
                DrawOutlineInternal(collider);
            }

            if (drawFill)
            {
                DrawFillInternal(collider);
            }
            Gizmos.matrix = previousMatrix;
        }

        private void DrawOutlineInternal(Collider collider)
        {
            Gizmos.color = outlineColor;

            switch (collider)
            {
                case BoxCollider box:
                    Gizmos.DrawWireCube(box.center, box.size);
                    break;
                case SphereCollider sphere:
                    Gizmos.DrawWireSphere(sphere.center, sphere.radius);
                    break;
                case MeshCollider mesh:
                    if (mesh.sharedMesh != null)
                        Gizmos.DrawWireMesh(mesh.sharedMesh);
                    break;
                default:
                    break;
            }
        }

        private void DrawFillInternal(Collider collider)
        {
            Gizmos.color = fillColor;

            switch (collider)
            {
                case BoxCollider box:
                    Gizmos.DrawCube(box.center, box.size);
                    break;
                case SphereCollider sphere:
                    Gizmos.DrawSphere(sphere.center, sphere.radius);
                    break;
                default:
                    break;
            }
        }
    }
}
