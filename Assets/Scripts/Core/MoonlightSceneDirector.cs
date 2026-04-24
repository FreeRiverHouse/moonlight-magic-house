using System.Collections;
using UnityEngine;

namespace MoonlightMagicHouse
{
    public class MoonlightSceneDirector : MonoBehaviour
    {
        public static MoonlightSceneDirector Instance { get; private set; }

        Transform _moonlight;
        MoonlightKidAnimator _kid;
        MeshRenderer _backdrop;
        Texture _roomTexture;
        Texture _meadowTexture;
        Color _roomBackdropColor = Color.white;

        Camera _camera;
        Vector3 _cameraPos;
        Quaternion _cameraRot;
        float _cameraFov;

        GameObject _doorRoot;
        Transform _doorPivot;
        GameObject _doorGlow;
        GameObject _bedRestRoot;
        GameObject[] _roomOnlyProps;
        Coroutine _active;
        Coroutine _cameraMove;
        Vector3 _home;
        Quaternion _homeRotation = Quaternion.identity;
        Vector3 _homeScale = Vector3.one;
        bool _outside;

        void Awake()
        {
            Instance = this;
        }

        IEnumerator Start()
        {
            yield return null;
            BindScene();
            CreateDoorPortal();
            CreateBedRestHelpers();
            CacheRoomOnlyProps();
        }

        public bool PlayAction(string action)
        {
            BindScene();
            if (_moonlight == null || _kid == null) return false;

            if (_active != null) StopCoroutine(_active);
            if (_cameraMove != null) StopCoroutine(_cameraMove);
            _active = StartCoroutine(PlayActionRoutine(action));
            return true;
        }

        void BindScene()
        {
            if (_moonlight == null)
            {
                var moonlightGo = GameObject.Find("Moonlight");
                if (moonlightGo != null)
                {
                    _moonlight = moonlightGo.transform;
                    _home = _moonlight.position;
                    _homeRotation = _moonlight.rotation;
                    _homeScale = _moonlight.localScale;
                }
            }

            if (_kid == null && _moonlight != null)
                _kid = _moonlight.GetComponentInChildren<MoonlightKidAnimator>(true);

            if (_backdrop == null)
            {
                var backdropGo = GameObject.Find("PhotorealRoomBackdrop");
                if (backdropGo != null)
                {
                    _backdrop = backdropGo.GetComponent<MeshRenderer>();
                    if (_backdrop != null && _backdrop.material != null)
                    {
                        _roomTexture = _backdrop.material.mainTexture;
                        _roomBackdropColor = _backdrop.material.color;
                    }
                }
            }

            if (_meadowTexture == null)
                _meadowTexture = Resources.Load<Texture2D>("Photoreal/meadow-generated");

            if (_camera == null)
            {
                _camera = Camera.main;
                if (_camera != null)
                {
                    _cameraPos = _camera.transform.position;
                    _cameraRot = _camera.transform.rotation;
                    _cameraFov = _camera.fieldOfView;
                }
            }
        }

        IEnumerator PlayActionRoutine(string action)
        {
            if (action != "Play")
                RestoreRoomInstant(_outside);

            switch (action)
            {
                case "Nap":
                    yield return NapRoutine();
                    break;
                case "Play":
                    yield return OutdoorRunRoutine();
                    break;
                case "Snack":
                    yield return MoveAndGesture(_home + new Vector3(-0.18f, 0f, 0.02f), "Snack", 0.42f, 0f, false);
                    break;
                case "Hug":
                    yield return MoveAndGesture(_home + new Vector3(0.02f, 0f, 0.04f), "Hug", 0.36f, 0f, false);
                    break;
                case "Bath":
                    yield return MoveAndGesture(_home + new Vector3(0.44f, 0f, 0.12f), "Bath", 0.46f, -8f, false);
                    break;
                case "Dance":
                    yield return MoveAndGesture(_home + new Vector3(0.18f, 0f, -0.08f), "Dance", 0.42f, 0f, false);
                    break;
                default:
                    _kid.PlayGesture(action);
                    break;
            }
        }

        IEnumerator MoveAndGesture(Vector3 target, string gesture, float moveDuration, float yaw, bool run)
        {
            StartCameraMove(_cameraPos, ForwardLook(), _cameraFov, 0.35f);
            yield return MoveMoonlight(target, moveDuration, yaw, run);
            _kid.SetWalking(false);
            _kid.SetFacingYaw(0f);
            _kid.PlayGesture(gesture);
        }

        IEnumerator NapRoutine()
        {
            Vector3 bedApproach = _home + new Vector3(0.82f, 0f, 0.08f);
            Vector3 bedRestSpot = _home + new Vector3(1.58f, 0.99f, 0.08f);
            StartCameraMove(new Vector3(0.10f, 1.27f, -4.60f), _home + new Vector3(1.22f, 1.10f, 0.08f), 35f, 0.65f);
            yield return MoveMoonlight(bedApproach, 0.52f, -12f, false);
            _kid.SetWalking(false);
            _kid.SetFacingYaw(0f);
            _kid.PlayGesture("Nap");
            ShowBedRestHelpers(true);
            yield return SettleIntoBed(bedRestSpot, 0.35f);
            yield return new WaitForSeconds(2.2f);
            ShowBedRestHelpers(false);
            RecoverMoonlightFromBed(false);
            StartCameraMove(_cameraPos, ForwardLook(), _cameraFov, 0.65f);
        }

        IEnumerator SettleIntoBed(Vector3 bedRestSpot, float duration)
        {
            if (_moonlight == null) yield break;

            Vector3 fromPos = _moonlight.position;
            Quaternion fromRot = _moonlight.rotation;
            Vector3 fromScale = _moonlight.localScale;
            Quaternion bedRot = _homeRotation * Quaternion.Euler(0f, -4f, 78f);
            Vector3 bedScale = _homeScale * 0.78f;

            float t = 0f;
            while (t < duration)
            {
                t += Time.deltaTime;
                float k = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(t / Mathf.Max(0.01f, duration)));
                _moonlight.position = Vector3.Lerp(fromPos, bedRestSpot, k);
                _moonlight.rotation = Quaternion.Slerp(fromRot, bedRot, k);
                _moonlight.localScale = Vector3.Lerp(fromScale, bedScale, k);
                yield return null;
            }
            _moonlight.position = bedRestSpot;
            _moonlight.rotation = bedRot;
            _moonlight.localScale = bedScale;
        }

        IEnumerator OutdoorRunRoutine()
        {
            RestoreRoomInstant(false);
            if (_doorRoot != null) _doorRoot.SetActive(true);

            Vector3 doorSpot = _home + new Vector3(0.98f, 0f, 0.05f);
            StartCameraMove(new Vector3(0.08f, 1.16f, -4.52f), doorSpot + new Vector3(0f, 0.72f, 0.06f), 38f, 0.45f);
            yield return MoveMoonlight(doorSpot, 0.82f, -16f, false);
            _kid.SetWalking(false);
            _kid.SetFacingYaw(-18f);

            yield return OpenDoor(true, 0.52f);
            yield return MoveMoonlight(doorSpot + new Vector3(0.28f, 0f, 0.06f), 0.34f, -24f, false);
            SetOutdoorBackdrop();
            if (_doorRoot != null) _doorRoot.SetActive(false);

            Vector3 meadowStart = _home + new Vector3(-0.36f, 0f, -0.04f);
            Vector3 meadowEnd = _home + new Vector3(0.64f, 0f, -0.10f);
            _moonlight.position = meadowStart;
            _kid.SetFacingYaw(10f);
            _kid.PlayGesture("Play");
            StartCameraMove(new Vector3(0f, 1.10f, -4.45f), _home + new Vector3(0.18f, 0.74f, -0.10f), 40f, 0.28f);
            yield return MoveMoonlight(meadowEnd, 1.35f, 10f, true);
            _kid.SetWalking(false);
            _kid.SetFacingYaw(0f);
            _kid.PlayGesture("Dance");
            yield return new WaitForSeconds(2.0f);

            RestoreRoomInstant(true);
            StartCameraMove(_cameraPos, ForwardLook(), _cameraFov, 0.75f);
        }

        IEnumerator MoveMoonlight(Vector3 target, float duration, float yaw, bool run)
        {
            if (_moonlight == null || _kid == null) yield break;

            Vector3 from = _moonlight.position;
            target.y = _home.y;
            _kid.SetWalking(true, run);
            _kid.SetFacingYaw(yaw);

            float t = 0f;
            while (t < duration)
            {
                t += Time.deltaTime;
                float k = Mathf.Clamp01(t / Mathf.Max(0.01f, duration));
                float ease = Mathf.SmoothStep(0f, 1f, k);
                _moonlight.position = Vector3.Lerp(from, target, ease);
                yield return null;
            }
            _moonlight.position = target;
        }

        IEnumerator OpenDoor(bool open, float duration)
        {
            if (_doorPivot == null) yield break;

            if (_doorGlow != null) _doorGlow.SetActive(true);
            Quaternion from = _doorPivot.localRotation;
            Quaternion to = Quaternion.Euler(0f, open ? -72f : 0f, 0f);
            float t = 0f;
            while (t < duration)
            {
                t += Time.deltaTime;
                float k = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(t / Mathf.Max(0.01f, duration)));
                _doorPivot.localRotation = Quaternion.Slerp(from, to, k);
                yield return null;
            }
            _doorPivot.localRotation = to;
        }

        void SetOutdoorBackdrop()
        {
            if (_backdrop != null && _meadowTexture != null)
            {
                _backdrop.material.mainTexture = _meadowTexture;
                _backdrop.material.color = Color.white;
            }
            RenderSettings.ambientLight = new Color(0.72f, 0.66f, 0.48f);
            SetRoomOnlyProps(false);
            _outside = true;
        }

        void RestoreRoomInstant(bool resetMoonlight)
        {
            if (_backdrop != null && _roomTexture != null)
            {
                _backdrop.material.mainTexture = _roomTexture;
                _backdrop.material.color = _roomBackdropColor;
            }
            RenderSettings.ambientLight = new Color(0.56f, 0.43f, 0.36f);
            SetRoomOnlyProps(true);
            if (_doorRoot != null) _doorRoot.SetActive(false);
            if (_doorPivot != null) _doorPivot.localRotation = Quaternion.identity;
            if (_doorGlow != null) _doorGlow.SetActive(false);
            ShowBedRestHelpers(false);
            RecoverMoonlightFromBed(resetMoonlight);
            if (_kid != null)
            {
                _kid.SetWalking(false);
                _kid.SetFacingYaw(0f);
            }
            _outside = false;
        }

        void RecoverMoonlightFromBed(bool resetPosition)
        {
            if (_moonlight == null) return;

            _moonlight.rotation = _homeRotation;
            _moonlight.localScale = _homeScale;
            if (resetPosition)
            {
                _moonlight.position = _home;
            }
            else
            {
                var p = _moonlight.position;
                p.y = _home.y;
                _moonlight.position = p;
            }
        }

        void StartCameraMove(Vector3 pos, Vector3 lookAt, float fov, float duration)
        {
            if (_camera == null) return;
            if (_cameraMove != null) StopCoroutine(_cameraMove);
            _cameraMove = StartCoroutine(MoveCamera(pos, lookAt, fov, duration));
        }

        IEnumerator MoveCamera(Vector3 pos, Vector3 lookAt, float fov, float duration)
        {
            Vector3 fromPos = _camera.transform.position;
            Quaternion fromRot = _camera.transform.rotation;
            float fromFov = _camera.fieldOfView;
            Quaternion toRot = Quaternion.LookRotation((lookAt - pos).normalized, Vector3.up);

            float t = 0f;
            while (t < duration)
            {
                t += Time.deltaTime;
                float k = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(t / Mathf.Max(0.01f, duration)));
                _camera.transform.position = Vector3.Lerp(fromPos, pos, k);
                _camera.transform.rotation = Quaternion.Slerp(fromRot, toRot, k);
                _camera.fieldOfView = Mathf.Lerp(fromFov, fov, k);
                yield return null;
            }
            _camera.transform.position = pos;
            _camera.transform.rotation = toRot;
            _camera.fieldOfView = fov;
        }

        Vector3 ForwardLook()
        {
            return _cameraPos + _cameraRot * Vector3.forward * 4.7f;
        }

        void CreateBedRestHelpers()
        {
            if (_bedRestRoot != null || _moonlight == null) return;

            _bedRestRoot = new GameObject("MoonBedRestOcclusion");
            _bedRestRoot.transform.position = _home + new Vector3(1.60f, 0.94f, -0.16f);
            _bedRestRoot.transform.rotation = Quaternion.identity;

            var shadowMat = MakeSpriteMat(new Color(0.12f, 0.055f, 0.05f, 0.26f), MoonlightHouseSetup.MakeSoftCircleTex(128));
            MakeQuad("MoonBedRestSoftShadow", _bedRestRoot.transform, new Vector3(-0.04f, -0.12f, 0.02f), new Vector3(0.90f, 0.24f, 1f), shadowMat);

            var blanketMat = MakeSpriteMat(new Color(0.95f, 0.47f, 0.44f, 0.34f), MakeSoftDuvetTex(192, 96));
            MakeQuad("MoonBedRestBlanketOcclusion", _bedRestRoot.transform, new Vector3(-0.02f, 0.02f, -0.03f), new Vector3(0.72f, 0.26f, 1f), blanketMat);

            var rimMat = MakeSpriteMat(new Color(1f, 0.82f, 0.62f, 0.10f), MoonlightHouseSetup.MakeSoftCircleTex(96));
            MakeQuad("MoonBedRestWarmRim", _bedRestRoot.transform, new Vector3(0.02f, 0.11f, -0.04f), new Vector3(0.48f, 0.12f, 1f), rimMat);

            _bedRestRoot.SetActive(false);
        }

        void ShowBedRestHelpers(bool show)
        {
            if (_bedRestRoot == null) return;
            _bedRestRoot.SetActive(show);
        }

        void CacheRoomOnlyProps()
        {
            if (_roomOnlyProps != null) return;
            _roomOnlyProps = new[]
            {
                GameObject.Find("StrawberryMacaron"),
                GameObject.Find("LemonMacaron"),
                GameObject.Find("BlueberryMacaron")
            };
        }

        void SetRoomOnlyProps(bool show)
        {
            CacheRoomOnlyProps();
            if (_roomOnlyProps == null) return;
            for (int i = 0; i < _roomOnlyProps.Length; i++)
            {
                if (_roomOnlyProps[i] != null)
                    _roomOnlyProps[i].SetActive(show);
            }
        }

        void CreateDoorPortal()
        {
            if (_doorRoot != null) return;

            var doorMat = MakeSpriteMat(new Color(1.0f, 0.52f, 0.66f, 0.72f), Texture2D.whiteTexture);
            var frameMat = MakeStandard(new Color(1.0f, 0.76f, 0.48f), 0.42f, true);
            var knobMat = MakeStandard(new Color(1.0f, 0.86f, 0.38f), 0.62f);

            _doorRoot = new GameObject("MoonDoorPortal");
            _doorRoot.transform.position = _home + new Vector3(1.22f, 0.56f, 0.16f);
            _doorRoot.transform.rotation = Quaternion.Euler(0f, -2f, 0f);

            var auraMat = MakeSpriteMat(new Color(1f, 0.70f, 0.36f, 0.28f), MoonlightHouseSetup.MakeSoftCircleTex(128));
            MakeQuad("MoonDoorOuterAura", _doorRoot.transform, new Vector3(0f, 0.02f, 0.040f), new Vector3(0.98f, 1.30f, 1f), auraMat);

            MakeCube("MoonDoorFrameTop", _doorRoot.transform, new Vector3(0f, 0.50f, 0f), new Vector3(0.60f, 0.040f, 0.042f), frameMat);
            MakeCube("MoonDoorFrameLeft", _doorRoot.transform, new Vector3(-0.31f, 0f, 0f), new Vector3(0.040f, 1.01f, 0.042f), frameMat);
            MakeCube("MoonDoorFrameRight", _doorRoot.transform, new Vector3(0.31f, 0f, 0f), new Vector3(0.040f, 1.01f, 0.042f), frameMat);

            for (int i = 0; i <= 10; i++)
            {
                float t = i / 10f;
                float x = Mathf.Lerp(-0.31f, 0.31f, t);
                float y = 0.48f + Mathf.Sin(t * Mathf.PI) * 0.15f;
                Color c = Color.Lerp(new Color(1f, 0.56f, 0.82f), new Color(1f, 0.86f, 0.40f), t);
                MakePortalSparkle(_doorRoot.transform, new Vector3(x, y, -0.040f), 0.030f, c);
            }

            _doorGlow = GameObject.CreatePrimitive(PrimitiveType.Quad);
            _doorGlow.name = "MoonDoorMeadowGlow";
            _doorGlow.transform.SetParent(_doorRoot.transform, false);
            _doorGlow.transform.localPosition = new Vector3(0f, 0f, 0.025f);
            _doorGlow.transform.localScale = new Vector3(0.52f, 0.92f, 1f);
            Object.Destroy(_doorGlow.GetComponent<Collider>());
            var glowMat = new Material(Shader.Find("Sprites/Default"));
            glowMat.mainTexture = _meadowTexture;
            glowMat.color = new Color(1f, 1f, 1f, 0.82f);
            _doorGlow.GetComponent<MeshRenderer>().material = glowMat;
            _doorGlow.SetActive(false);

            var pivot = new GameObject("MoonDoorPivot");
            _doorPivot = pivot.transform;
            _doorPivot.SetParent(_doorRoot.transform, false);
            _doorPivot.localPosition = new Vector3(-0.25f, 0f, -0.01f);

            MakeQuad("MoonDoorPanel", _doorPivot, new Vector3(0.25f, 0f, -0.006f), new Vector3(0.50f, 0.92f, 1f), doorMat);
            MakeQuad("MoonDoorWindow", _doorPivot, new Vector3(0.25f, 0.24f, -0.014f), new Vector3(0.22f, 0.20f, 1f), MakeSpriteMat(new Color(1f, 0.90f, 0.56f, 0.72f), MoonlightHouseSetup.MakeSoftCircleTex(64)));
            MakeCube("MoonDoorKnob", _doorPivot, new Vector3(0.42f, -0.06f, -0.038f), Vector3.one * 0.040f, knobMat);
            _doorRoot.SetActive(false);
        }

        static GameObject MakeQuad(string name, Transform parent, Vector3 localPos, Vector3 scale, Material material)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Quad);
            go.name = name;
            go.transform.SetParent(parent, false);
            go.transform.localPosition = localPos;
            go.transform.localScale = scale;
            Object.Destroy(go.GetComponent<Collider>());
            go.GetComponent<MeshRenderer>().material = material;
            return go;
        }

        static GameObject MakePortalSparkle(Transform parent, Vector3 localPos, float scale, Color color)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            go.name = "MoonDoorFairyPearl";
            go.transform.SetParent(parent, false);
            go.transform.localPosition = localPos;
            go.transform.localScale = Vector3.one * scale;
            Object.Destroy(go.GetComponent<Collider>());
            go.GetComponent<MeshRenderer>().material = MakeStandard(color, 0.72f, true);
            return go;
        }

        static GameObject MakeCube(string name, Transform parent, Vector3 localPos, Vector3 scale, Material material)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = name;
            go.transform.SetParent(parent, false);
            go.transform.localPosition = localPos;
            go.transform.localScale = scale;
            Object.Destroy(go.GetComponent<Collider>());
            go.GetComponent<MeshRenderer>().material = material;
            return go;
        }

        static Material MakeStandard(Color color, float gloss, bool emission = false)
        {
            var mat = new Material(Shader.Find("Standard"));
            mat.color = color;
            mat.SetFloat("_Glossiness", gloss);
            mat.SetFloat("_Metallic", 0f);
            if (emission)
            {
                mat.EnableKeyword("_EMISSION");
                mat.SetColor("_EmissionColor", color * 1.5f);
            }
            return mat;
        }

        static Material MakeSpriteMat(Color color, Texture texture)
        {
            var mat = new Material(Shader.Find("Sprites/Default"));
            mat.mainTexture = texture != null ? texture : Texture2D.whiteTexture;
            mat.color = color;
            mat.renderQueue = 3000;
            return mat;
        }

        static Texture2D MakeSoftDuvetTex(int width, int height)
        {
            var tex = new Texture2D(width, height, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Bilinear;
            tex.wrapMode = TextureWrapMode.Clamp;
            float cx = (width - 1) * 0.5f;
            float cy = (height - 1) * 0.42f;
            for (int y = 0; y < height; y++)
            for (int x = 0; x < width; x++)
            {
                float nx = Mathf.Abs((x - cx) / cx);
                float ny = Mathf.Abs((y - cy) / Mathf.Max(1f, height * 0.48f));
                float rounded = Mathf.Clamp01(1f - Mathf.Max(nx * 0.82f, ny));
                float edge = Mathf.SmoothStep(0f, 1f, rounded);
                float weave = 0.82f + Mathf.Sin(x * 0.18f + y * 0.07f) * 0.08f + Mathf.Sin(y * 0.31f) * 0.04f;
                tex.SetPixel(x, y, new Color(1f, 1f, 1f, Mathf.Clamp01(edge * weave)));
            }
            tex.Apply();
            return tex;
        }
    }
}
