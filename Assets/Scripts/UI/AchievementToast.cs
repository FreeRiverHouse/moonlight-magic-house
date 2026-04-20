using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace MoonlightMagicHouse
{
    public class AchievementToast : MonoBehaviour
    {
        [SerializeField] GameObject root;
        [SerializeField] Image iconImage;
        [SerializeField] TMP_Text titleLabel;
        [SerializeField] TMP_Text descLabel;
        [SerializeField] float displayDuration = 3.5f;
        [SerializeField] float slideInTime = 0.4f;

        RectTransform _rect;
        Vector2 _shownPos;
        Vector2 _hiddenPos;
        Coroutine _co;

        void Awake()
        {
            _rect = root.GetComponent<RectTransform>();
            _shownPos  = _rect.anchoredPosition;
            _hiddenPos = _shownPos + new Vector2(500f, 0f);
            _rect.anchoredPosition = _hiddenPos;
            root.SetActive(false);

            if (AchievementSystem.Instance)
                AchievementSystem.Instance.onUnlocked.AddListener(Show);
        }

        public void Show(Achievement ach)
        {
            if (_co != null) StopCoroutine(_co);
            _co = StartCoroutine(Animate(ach));
        }

        IEnumerator Animate(Achievement ach)
        {
            iconImage.sprite   = ach.icon;
            titleLabel.text    = ach.title;
            descLabel.text     = ach.description;
            root.SetActive(true);

            float t = 0;
            while (t < slideInTime)
            {
                t += Time.deltaTime;
                _rect.anchoredPosition = Vector2.Lerp(_hiddenPos, _shownPos, t / slideInTime);
                yield return null;
            }

            yield return new WaitForSeconds(displayDuration);

            t = 0;
            while (t < slideInTime)
            {
                t += Time.deltaTime;
                _rect.anchoredPosition = Vector2.Lerp(_shownPos, _hiddenPos, t / slideInTime);
                yield return null;
            }

            root.SetActive(false);
        }
    }
}
