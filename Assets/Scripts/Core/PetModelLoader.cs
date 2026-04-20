using System.Collections.Generic;
using UnityEngine;

namespace MoonlightMagicHouse
{
    // Swap the visible pet mesh when species or stage changes.
    // Assign FBX prefabs via the Inspector arrays — order must match PetSpecies enum.
    public class PetModelLoader : MonoBehaviour
    {
        [System.Serializable]
        public struct StageModels
        {
            public GameObject egg;
            public GameObject baby;
            public GameObject child;
            public GameObject teen;
            public GameObject adult;

            public GameObject ForStage(EvolutionStage s) => s switch
            {
                EvolutionStage.Egg   => egg,
                EvolutionStage.Baby  => baby,
                EvolutionStage.Child => child,
                EvolutionStage.Teen  => teen,
                EvolutionStage.Adult => adult,
                _                   => egg
            };
        }

        [SerializeField] StageModels[] speciesModels; // index matches PetSpecies enum
        [SerializeField] Transform modelRoot;

        MoonlightPet _pet;
        GameObject _current;

        void Start()
        {
            _pet = GetComponentInParent<MoonlightPet>();
            if (_pet == null) return;
            _pet.onEvolution.AddListener(_ => Refresh());
            Refresh();
        }

        public void Refresh()
        {
            if (_pet == null) return;
            int idx = (int)_pet.species;
            if (idx >= speciesModels.Length) return;

            var prefab = speciesModels[idx].ForStage(_pet.stage);
            if (prefab == null) return;

            if (_current != null) Destroy(_current);
            _current = Instantiate(prefab, modelRoot);
        }
    }
}
