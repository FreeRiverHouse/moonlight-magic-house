using UnityEngine;

namespace MoonlightMagicHouse
{
    // Gentle idle animation: head tilt, arm sway, subtle body squash.
    // Acts on named child transforms inside the Visual rig.
    public class IdleMicroMotion : MonoBehaviour
    {
        [SerializeField] string headName = "Head";
        [SerializeField] string armLName = "ArmL";
        [SerializeField] string armRName = "ArmR";
        [SerializeField] string bodyName = "Body";

        Transform _head, _armL, _armR, _body;
        Quaternion _headBase, _armLBase, _armRBase;
        Vector3    _bodyBase;

        void Start()
        {
            _head = transform.Find(headName);
            _armL = transform.Find(armLName);
            _armR = transform.Find(armRName);
            _body = transform.Find(bodyName);
            if (_head != null) _headBase = _head.localRotation;
            if (_armL != null) _armLBase = _armL.localRotation;
            if (_armR != null) _armRBase = _armR.localRotation;
            if (_body != null) _bodyBase = _body.localScale;
        }

        void Update()
        {
            float t = Time.time;

            if (_head != null)
            {
                float tilt = Mathf.Sin(t * 1.1f) * 3.5f;
                float nod  = Mathf.Sin(t * 0.9f + 0.7f) * 2.0f;
                _head.localRotation = _headBase * Quaternion.Euler(nod, tilt, 0f);
            }

            if (_armL != null)
            {
                float sway = Mathf.Sin(t * 1.3f) * 6f;
                _armL.localRotation = _armLBase * Quaternion.Euler(0f, 0f, sway);
            }
            if (_armR != null)
            {
                float sway = Mathf.Sin(t * 1.3f + Mathf.PI) * 6f;
                _armR.localRotation = _armRBase * Quaternion.Euler(0f, 0f, sway);
            }

            if (_body != null)
            {
                float s = 1f + Mathf.Sin(t * 2.0f) * 0.018f;
                _body.localScale = new Vector3(_bodyBase.x * s, _bodyBase.y * (1f / s), _bodyBase.z * s);
            }
        }
    }
}
