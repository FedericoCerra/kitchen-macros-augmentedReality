using UnityEngine;

namespace KitchenMacros
{

    [DefaultExecutionOrder(100)]
    public class Billboard : MonoBehaviour
    {

        [SerializeField] bool keepUpright = true;

        [SerializeField] float rotationDamping = 10f;

        Camera _camera;

        void OnEnable() => _camera = Camera.main;

        void LateUpdate()
        {
            if (_camera == null)
            {
                _camera = Camera.main;
                if (_camera == null) return;
            }

            var camTransform = _camera.transform;
            var forward = camTransform.forward;
            var up = camTransform.up;

            if (keepUpright)
            {
                forward.y = 0f;

                if (forward.sqrMagnitude < 1e-6f)
                {
                    forward = -camTransform.up;
                    forward.y = 0f;
                }

                if (forward.sqrMagnitude < 1e-6f) return;

                up = Vector3.up;
            }

            var desired = Quaternion.LookRotation(forward.normalized, up);

            transform.rotation = rotationDamping <= 0f
                ? desired
                : Quaternion.Slerp(transform.rotation, desired,
                                   1f - Mathf.Exp(-rotationDamping * Time.deltaTime));
        }
    }
}
