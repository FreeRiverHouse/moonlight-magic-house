using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace MoonlightMagicHouse
{
    public class TricksUI : MonoBehaviour
    {
        [SerializeField] GameObject root;
        [SerializeField] Transform  grid;
        [SerializeField] GameObject trickBtnPrefab;
        [SerializeField] Button     closeBtn;

        void Start()
        {
            closeBtn.onClick.AddListener(() => root.SetActive(false));
            MoonlightTricksSystem.Instance?.onTrickLearned.AddListener(_ => RebuildIfOpen());
        }

        public void Open()
        {
            root.SetActive(true);
            Rebuild();
        }

        void RebuildIfOpen() { if (root.activeSelf) Rebuild(); }

        void Rebuild()
        {
            foreach (Transform t in grid) Destroy(t.gameObject);
            var learned = MoonlightTricksSystem.Instance?.LearnedTricks() ?? new List<MoonlightTrick>();
            foreach (var trick in learned)
            {
                var btn = Instantiate(trickBtnPrefab, grid);
                btn.GetComponentInChildren<TMP_Text>().text = $"{trick.displayName}\n+{trick.coinReward}⭐";
                var captured = trick.id;
                btn.GetComponent<Button>().onClick.AddListener(() =>
                    MoonlightTricksSystem.Instance?.Perform(captured));
            }
        }
    }
}
