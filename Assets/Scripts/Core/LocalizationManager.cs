using System.Collections.Generic;
using UnityEngine;

namespace MoonlightMagicHouse
{
    public enum Language { English, Italian, French, Spanish, Japanese }

    public class LocalizationManager : MonoBehaviour
    {
        public static LocalizationManager Instance { get; private set; }

        [SerializeField] Language defaultLanguage = Language.English;

        Language _current;

        static readonly Dictionary<string, Dictionary<Language, string>> Table =
            new Dictionary<string, Dictionary<Language, string>>
        {
            // ── Stats ───────────────────────────────────────────────────────
            ["ui.hunger"]          = new() { [Language.English]="Hunger",    [Language.Italian]="Fame",          [Language.French]="Faim",        [Language.Spanish]="Hambre",    [Language.Japanese]="おなか" },
            ["ui.wonder"]          = new() { [Language.English]="Wonder",    [Language.Italian]="Meraviglia",    [Language.French]="Émerveillement",[Language.Spanish]="Asombro", [Language.Japanese]="ふしぎ" },
            ["ui.warmth"]          = new() { [Language.English]="Warmth",    [Language.Italian]="Calore",        [Language.French]="Chaleur",     [Language.Spanish]="Calidez",   [Language.Japanese]="ぬくもり" },
            ["ui.rest"]            = new() { [Language.English]="Rest",      [Language.Italian]="Riposo",        [Language.French]="Repos",       [Language.Spanish]="Descanso",  [Language.Japanese]="きゅうそく" },
            ["ui.magic"]           = new() { [Language.English]="Magic",     [Language.Italian]="Magia",         [Language.French]="Magie",       [Language.Spanish]="Magia",     [Language.Japanese]="まほう" },
            ["ui.coins"]           = new() { [Language.English]="Stars",     [Language.Italian]="Stelle",        [Language.French]="Étoiles",     [Language.Spanish]="Estrellas", [Language.Japanese]="ほし" },
            // ── Rooms ───────────────────────────────────────────────────────
            ["room.bedroom"]       = new() { [Language.English]="Bedroom",   [Language.Italian]="Camera",        [Language.French]="Chambre",     [Language.Spanish]="Dormitorio",[Language.Japanese]="しんしつ" },
            ["room.kitchen"]       = new() { [Language.English]="Kitchen",   [Language.Italian]="Cucina",        [Language.French]="Cuisine",     [Language.Spanish]="Cocina",    [Language.Japanese]="だいどころ" },
            ["room.livingroom"]    = new() { [Language.English]="Living Room",[Language.Italian]="Salotto",      [Language.French]="Salon",       [Language.Spanish]="Sala",      [Language.Japanese]="リビング" },
            ["room.garden"]        = new() { [Language.English]="Garden",    [Language.Italian]="Giardino",      [Language.French]="Jardin",      [Language.Spanish]="Jardín",    [Language.Japanese]="にわ" },
            ["room.library"]       = new() { [Language.English]="Library",   [Language.Italian]="Biblioteca",    [Language.French]="Bibliothèque",[Language.Spanish]="Biblioteca",[Language.Japanese]="としょかん" },
            // ── Welcome ─────────────────────────────────────────────────────
            ["ui.welcome"]         = new() { [Language.English]="Welcome to the Moonlight Magic House!", [Language.Italian]="Benvenuta nella Casa della Magia Lunare!", [Language.French]="Bienvenue dans la Maison de la Magie Lunaire!", [Language.Spanish]="¡Bienvenida a la Casa de la Magia Lunar!", [Language.Japanese]="ムーンライト マジックハウスへようこそ！" },
            ["ui.offline"]         = new() { [Language.English]="Moonlight missed you!", [Language.Italian]="Moonlight ti aspettava!", [Language.French]="Moonlight s'ennuyait de toi!", [Language.Spanish]="¡Moonlight te echaba de menos!", [Language.Japanese]="ムーンライトがさびしがっていたよ！" },
            // ── Moonlight stages ────────────────────────────────────────────
            ["stage.moonbud"]      = new() { [Language.English]="Moonbud",   [Language.Italian]="Germoglio",     [Language.French]="Bourgeon",    [Language.Spanish]="Brote Lunar",[Language.Japanese]="ムーンバッド" },
            ["stage.starling"]     = new() { [Language.English]="Starling",  [Language.Italian]="Stellina",      [Language.French]="Étoilée",     [Language.Spanish]="Estrellita", [Language.Japanese]="スターリング" },
            ["stage.luminary"]     = new() { [Language.English]="Luminary",  [Language.Italian]="Luminaria",     [Language.French]="Luminaire",   [Language.Spanish]="Luminaria",  [Language.Japanese]="ルミナリー" },
            ["stage.sorceress"]    = new() { [Language.English]="Sorceress", [Language.Italian]="Strega",        [Language.French]="Sorcière",    [Language.Spanish]="Hechicera",  [Language.Japanese]="ソーサレス" },
            ["stage.moonkeeper"]   = new() { [Language.English]="Moonkeeper",[Language.Italian]="Guardiana",     [Language.French]="Gardienne",   [Language.Spanish]="Guardiana",  [Language.Japanese]="ムーンキーパー" },
            // ── Actions ─────────────────────────────────────────────────────
            ["action.feed"]        = new() { [Language.English]="Feed",      [Language.Italian]="Dai da mangiare",[Language.French]="Nourrir",    [Language.Spanish]="Alimentar",  [Language.Japanese]="たべさせる" },
            ["action.cuddle"]      = new() { [Language.English]="Cuddle",    [Language.Italian]="Coccola",       [Language.French]="Câlin",       [Language.Spanish]="Abrazar",    [Language.Japanese]="だっこ" },
            ["action.sleep"]       = new() { [Language.English]="Sleep",     [Language.Italian]="Dormi",         [Language.French]="Dormir",      [Language.Spanish]="Dormir",     [Language.Japanese]="ねる" },
        };

        void Awake()
        {
            if (Instance != null) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);
            _current = (Language)PlayerPrefs.GetInt("lang", (int)defaultLanguage);
        }

        public void SetLanguage(Language lang)
        {
            _current = lang;
            PlayerPrefs.SetInt("lang", (int)lang);
        }

        public static string Get(string key)
        {
            if (Instance == null) return key;
            if (!Table.TryGetValue(key, out var langs)) return key;
            if (!langs.TryGetValue(Instance._current, out var text)) return key;
            return text;
        }
    }
}
