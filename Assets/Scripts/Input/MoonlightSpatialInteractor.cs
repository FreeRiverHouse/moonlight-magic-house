using UnityEngine;

namespace MoonlightMagicHouse
{
    [RequireComponent(typeof(MoonlightCharacter))]
    public class MoonlightSpatialInteractor : MonoBehaviour
    {
        public MoonlightSpatialActionZone CurrentZone { get; private set; }
        public MoonlightSpatialActionZone NearestZone { get; private set; }
        public float CurrentDistance { get; private set; }
        public float NearestDistance { get; private set; } = float.MaxValue;
        public Vector2 NearestZoneDirectionXZ { get; private set; }
        public bool HasNavigationTarget => NearestZone != null && CurrentZone == null &&
            NearestZoneDirectionXZ.sqrMagnitude > 0.999f;

        MoonlightCharacter _moonlight;
        MoonlightSpatialActionZone[] _zones;
        float _nextScan;

        void Awake()
        {
            _moonlight = GetComponent<MoonlightCharacter>();
        }

        void Update()
        {
            if (Time.unscaledTime >= _nextScan)
            {
                _zones = FindObjectsByType<MoonlightSpatialActionZone>(FindObjectsSortMode.None);
                _nextScan = Time.unscaledTime + 0.5f;
            }

            ScanNearestZone();
        }

        public string CurrentActionLabel
        {
            get
            {
                if (CurrentZone == null) return "ACTION";
                string progress = CurrentZone.RequiredSteps > 1
                    ? $"  {CurrentZone.ProgressStep + 1}/{CurrentZone.RequiredSteps}"
                    : "";
                return CurrentZone.GetActionLabel(_moonlight) + progress;
            }
        }

        public string CurrentPrompt =>
            CurrentZone != null ? CurrentZone.GetPrompt(_moonlight) : DiscoveryPrompt;

        public string DiscoveryPrompt
        {
            get
            {
                if (NearestZone == null) return "EXPLORE THIS ROOM";
                string distance = NearestDistance < float.MaxValue ? $"  /  {NearestDistance:0.0}m" : "";
                return $"MOVE TO {NearestZone.DisplayName.ToUpperInvariant()}{distance}  /  " +
                    $"{MoonlightSpatialActionZone.GestureInstruction(NearestZone.RequiredGesture)} " +
                    NearestZone.GetActionLabel(_moonlight);
            }
        }

        public bool HasAction => CurrentZone != null;

        public void RescanNowForQA()
        {
            _zones = FindObjectsByType<MoonlightSpatialActionZone>(FindObjectsSortMode.None);
            ScanNearestZone();
        }

        public string ExecuteCurrent()
        {
            if (CurrentZone == null)
                return "Move closer to an object first.";

            var result = CurrentZone.Execute(_moonlight);
            MoonlightVisualQA.Instance?.LogContextAction(CurrentZone, transform.position, result);
            return result;
        }

        void ScanNearestZone()
        {
            MoonlightSpatialActionZone nearest = null;
            float nearestDistance = float.MaxValue;

            if (_zones == null)
            {
                CurrentZone = null;
                NearestZone = null;
                CurrentDistance = float.MaxValue;
                NearestDistance = float.MaxValue;
                NearestZoneDirectionXZ = Vector2.zero;
                return;
            }
            foreach (var zone in _zones)
            {
                if (zone == null || !zone.isActiveAndEnabled || !zone.gameObject.activeInHierarchy) continue;
                float distance = Vector2.Distance(
                    new Vector2(transform.position.x, transform.position.z),
                    new Vector2(zone.transform.position.x, zone.transform.position.z));
                if (distance < nearestDistance)
                {
                    nearest = zone;
                    nearestDistance = distance;
                }
            }

            NearestZone = nearest;
            NearestDistance = nearestDistance;
            CurrentZone = nearest != null && nearestDistance <= nearest.Radius ? nearest : null;
            CurrentDistance = CurrentZone != null ? nearestDistance : float.MaxValue;
            if (nearest != null && nearestDistance > Mathf.Epsilon)
            {
                Vector3 offset = nearest.transform.position - transform.position;
                NearestZoneDirectionXZ = new Vector2(offset.x, offset.z).normalized;
            }
            else
            {
                NearestZoneDirectionXZ = Vector2.zero;
            }
        }
    }
}
