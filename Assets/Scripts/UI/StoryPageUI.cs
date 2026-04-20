using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace MoonlightMagicHouse
{
    public class StoryPageUI : MonoBehaviour
    {
        [SerializeField] GameObject root;
        [SerializeField] Image illustrationImage;
        [SerializeField] TMP_Text storyText;
        [SerializeField] float typeSpeed = 0.03f;
        [SerializeField] Button nextBtn;
        [SerializeField] Button closeBtn;

        LibraryRoom _library;
        Coroutine _typingCo;

        void Start()
        {
            _library = FindAnyObjectByType<LibraryRoom>();
            nextBtn.onClick.AddListener(ReadNext);
            closeBtn.onClick.AddListener(Close);
            root.SetActive(false);
        }

        public void Show(StoryPage page)
        {
            root.SetActive(true);
            if (page.illustration) illustrationImage.sprite = page.illustration;
            if (_typingCo != null) StopCoroutine(_typingCo);
            _typingCo = StartCoroutine(TypeText(page.text));
        }

        IEnumerator TypeText(string text)
        {
            storyText.text = "";
            foreach (char c in text)
            {
                storyText.text += c;
                yield return new WaitForSeconds(typeSpeed);
            }
        }

        void ReadNext() => _library?.ReadNextPage();
        void Close()    => root.SetActive(false);
    }
}
