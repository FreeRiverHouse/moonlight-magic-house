using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace MoonlightMagicHouse
{
    public class OnboardingFlow : MonoBehaviour
    {
        [Header("Screens")]
        [SerializeField] GameObject screenWelcome;
        [SerializeField] GameObject screenNamePicker;
        [SerializeField] GameObject screenSpeciesPicker;
        [SerializeField] GameObject screenEggReveal;

        [Header("Name input")]
        [SerializeField] TMP_InputField nameInput;
        [SerializeField] Button confirmNameBtn;
        [SerializeField] TMP_Text nameErrorLabel;

        [Header("Species grid")]
        [SerializeField] Transform speciesGrid;
        [SerializeField] GameObject speciesButtonPrefab;

        [Header("Egg reveal")]
        [SerializeField] Animator eggAnimator;
        [SerializeField] TMP_Text eggRevealLabel;

        [Header("Narration")]
        [SerializeField] NPCBubble luninaBubble;

        static readonly string[] WelcomeLines =
        {
            "Welcome to the Moonlight Magic House!",
            "A magical creature is waiting for you...",
            "First, let's give your pet a name."
        };

        PetSpecies _chosen;
        string _petName;

        void Start()
        {
            ShowScreen(screenWelcome);
            StartCoroutine(WelcomeSequence());
            confirmNameBtn.onClick.AddListener(OnNameConfirmed);
            BuildSpeciesGrid();
        }

        IEnumerator WelcomeSequence()
        {
            foreach (var line in WelcomeLines)
            {
                luninaBubble?.Show("Lunina", line, 3f);
                yield return new WaitForSeconds(3.5f);
            }
            ShowScreen(screenNamePicker);
        }

        void OnNameConfirmed()
        {
            string n = nameInput.text.Trim();
            if (n.Length < 2 || n.Length > 16)
            {
                nameErrorLabel.text = "Name must be 2–16 characters";
                nameErrorLabel.gameObject.SetActive(true);
                return;
            }
            _petName = n;
            nameErrorLabel.gameObject.SetActive(false);
            ShowScreen(screenSpeciesPicker);
            luninaBubble?.Show("Lunina", $"Wonderful name, {_petName}! Now pick your creature.", 4f);
        }

        void BuildSpeciesGrid()
        {
            var species = System.Enum.GetValues(typeof(PetSpecies));
            foreach (PetSpecies sp in species)
            {
                var go  = Instantiate(speciesButtonPrefab, speciesGrid);
                var btn = go.GetComponent<Button>();
                go.GetComponentInChildren<TMP_Text>().text = sp.ToString();
                var captured = sp;
                btn.onClick.AddListener(() => OnSpeciesChosen(captured));
            }
        }

        void OnSpeciesChosen(PetSpecies sp)
        {
            _chosen = sp;
            ShowScreen(screenEggReveal);
            StartCoroutine(EggReveal());
        }

        IEnumerator EggReveal()
        {
            eggRevealLabel.text = $"Your {_chosen} egg is hatching...";
            if (eggAnimator) eggAnimator.SetTrigger("Shake");
            AudioManager.Instance?.Play("egg_shake");
            yield return new WaitForSeconds(2f);
            if (eggAnimator) eggAnimator.SetTrigger("Crack");
            AudioManager.Instance?.Play("egg_crack");
            yield return new WaitForSeconds(1.5f);
            AudioManager.Instance?.Play("evolution");
            luninaBubble?.Show("Lunina", $"Meet {_petName} the {_chosen}! Take good care of them!", 5f);
            yield return new WaitForSeconds(1f);
            ApplyAndStart();
        }

        void ApplyAndStart()
        {
            var pet = GameManager.Instance.pet;
            pet.petName = _petName;
            pet.species = _chosen;
            pet.stage   = EvolutionStage.Egg;
            SaveManager.Instance.Save(pet);
            GameManager.Instance.ui.Refresh(pet);
            gameObject.SetActive(false);
        }

        void ShowScreen(GameObject target)
        {
            screenWelcome.SetActive(target == screenWelcome);
            screenNamePicker.SetActive(target == screenNamePicker);
            screenSpeciesPicker.SetActive(target == screenSpeciesPicker);
            screenEggReveal.SetActive(target == screenEggReveal);
        }
    }
}
