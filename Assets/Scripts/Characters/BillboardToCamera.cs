using UnityEngine;

namespace MoonlightMagicHouse
{
    // Keeps a transform facing the main camera each LateUpdate.
    public class BillboardToCamera : MonoBehaviour
    {
        Camera _cam;
        void LateUpdate()
        {
            if (_cam == null) _cam = Camera.main;
            if (_cam == null) return;
            var fwd = transform.position - _cam.transform.position;
            fwd.y = 0f;
            if (fwd.sqrMagnitude > 0.0001f)
                transform.rotation = Quaternion.LookRotation(fwd.normalized, Vector3.up);
        }
    }
}
