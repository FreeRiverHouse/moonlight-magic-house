using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace MoonlightMagicHouse
{
    public class SettingsUI : MonoBehaviour
    {
        [SerializeField] GameObject root;
        [SerializeField] Slider sfxSlider;
        [SerializeField] Slider musicSlider;
        [SerializeField] Toggle notifToggle;
        [SerializeField] TMP_Dropdown langDropdown;
        [SerializeField] Button resetBtn;
        [SerializeField] Button closeBtn;

        const string SFX_KEY   = "sfx_vol";
        const string MUSIC_KEY = "music_vol";

        void Start()
        {
            sfxSlider.value   = PlayerPrefs.GetFloat(SFX_KEY, 1f);
            musicSlider.value = PlayerPrefs.GetFloat(MUSIC_KEY, 0.4f);
            notifToggle.isOn  = NotificationManager.Enabled;

            sfxSlider.onValueChanged.AddListener(v =>
            {
                PlayerPrefs.SetFloat(SFX_KEY, v);
            });

            musicSlider.onValueChanged.AddListener(v =>
            {
                PlayerPrefs.SetFloat(MUSIC_KEY, v);
            });

            notifToggle.onValueChanged.AddListener(v =>
            {
                NotificationManager.Enabled = v;
            });

            langDropdown.onValueChanged.AddListener(v =>
            {
                LocalizationManager.Instance?.SetLanguage((Language)v);
            });

            resetBtn.onClick.AddListener(ConfirmReset);
            closeBtn.onClick.AddListener(() => root.SetActive(false));
        }

        void ConfirmReset()
        {
            MoonlightSave.Delete();
            UnityEngine.SceneManagement.SceneManager.LoadScene(0);
        }

        public void Open()  => root.SetActive(true);
        public void Close() => root.SetActive(false);
    }
}
