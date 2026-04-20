using System;
using UnityEngine;

namespace MoonlightMagicHouse
{
    public enum PetSpecies { Dragon, Cat, Panda, Fox, Penguin, Bunny, Bear, Owl }
    public enum EvolutionStage { Egg, Baby, Child, Teen, Adult }
    public enum PetMood { Sad, Tired, Hungry, Okay, Happy, Ecstatic }

    [Serializable]
    public class PetStats
    {
        [Range(0, 100)] public float hunger    = 80f;
        [Range(0, 100)] public float happiness = 80f;
        [Range(0, 100)] public float energy    = 80f;
        [Range(0, 100)] public float cleanliness = 80f;
        [Range(0, 100)] public float health    = 100f;

        public PetMood GetMood()
        {
            float avg = (hunger + happiness + energy + cleanliness + health) / 5f;
            if (health < 20f) return PetMood.Sad;
            if (energy < 20f) return PetMood.Tired;
            if (hunger < 20f) return PetMood.Hungry;
            if (avg < 40f)    return PetMood.Okay;
            if (avg < 70f)    return PetMood.Happy;
            return PetMood.Ecstatic;
        }
    }

    [Serializable]
    public class PetSaveData
    {
        public string petName;
        public PetSpecies species;
        public EvolutionStage stage;
        public PetStats stats;
        public int xp;
        public int level;
        public int coins;
        public float ageMinutes;
        public string lastSaveTime;
        public int[] unlockedOutfits;
        public int[] learnedTricks;
        public int currentOutfit;
    }
}
