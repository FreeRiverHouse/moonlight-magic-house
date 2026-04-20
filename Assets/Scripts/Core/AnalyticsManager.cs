using UnityEngine;

namespace MoonlightMagicHouse
{
    // Privacy-first analytics — no PII, no third-party SDK needed.
    // Replace body of each method with your preferred provider (GameAnalytics, Firebase, etc.)
    public static class AnalyticsManager
    {
        public static void TrackSessionStart(int level, EvolutionStage stage)
            => Log("session_start", $"level={level} stage={stage}");

        public static void TrackEvolution(PetSpecies species, EvolutionStage newStage)
            => Log("evolution", $"species={species} stage={newStage}");

        public static void TrackFeed(string foodName, int cost)
            => Log("feed", $"food={foodName} cost={cost}");

        public static void TrackPlay(string activityName)
            => Log("play", $"activity={activityName}");

        public static void TrackRoomVisit(RoomType room)
            => Log("room_visit", $"room={room}");

        public static void TrackAchievement(string id)
            => Log("achievement", $"id={id}");

        public static void TrackPurchase(string outfitName, int cost)
            => Log("purchase", $"outfit={outfitName} cost={cost}");

        public static void TrackDailyReward(int coins, int streak)
            => Log("daily_reward", $"coins={coins} streak={streak}");

        public static void TrackSessionEnd(float durationSeconds)
            => Log("session_end", $"duration={durationSeconds:F0}s");

        static void Log(string ev, string props)
        {
#if DEVELOPMENT_BUILD || UNITY_EDITOR
            Debug.Log($"[Analytics] {ev} | {props}");
#endif
            // TODO: forward to provider
        }
    }
}
