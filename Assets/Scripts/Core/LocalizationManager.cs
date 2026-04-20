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
            ["ui.hunger"]       = new() { [Language.English]="Hunger",    [Language.Italian]="Fame",      [Language.French]="Faim",    [Language.Spanish]="Hambre",    [Language.Japanese]="おなか" },
            ["ui.happiness"]    = new() { [Language.English]="Happiness", [Language.Italian]="Felicità",  [Language.French]="Bonheur", [Language.Spanish]="Felicidad", [Language.Japanese]="しあわせ" },
            ["ui.energy"]       = new() { [Language.English]="Energy",    [Language.Italian]="Energia",   [Language.French]="Énergie", [Language.Spanish]="Energía",   [Language.Japanese]="エネルギー" },
            ["ui.cleanliness"]  = new() { [Language.English]="Cleanliness",[Language.Italian]="Pulizia",  [Language.French]="Propreté",[Language.Spanish]="Limpieza",  [Language.Japanese]="きれいさ" },
            ["ui.health"]       = new() { [Language.English]="Health",    [Language.Italian]="Salute",    [Language.French]="Santé",   [Language.Spanish]="Salud",     [Language.Japanese]="けんこう" },
            ["ui.coins"]        = new() { [Language.English]="Coins",     [Language.Italian]="Stelle",    [Language.French]="Étoiles", [Language.Spanish]="Monedas",   [Language.Japanese]="コイン" },
            ["room.bedroom"]    = new() { [Language.English]="Bedroom",   [Language.Italian]="Camera",    [Language.French]="Chambre", [Language.Spanish]="Dormitorio",[Language.Japanese]="しんしつ" },
            ["room.kitchen"]    = new() { [Language.English]="Kitchen",   [Language.Italian]="Cucina",    [Language.French]="Cuisine", [Language.Spanish]="Cocina",    [Language.Japanese]="だいどころ" },
            ["room.livingroom"] = new() { [Language.English]="Living Room",[Language.Italian]="Salotto",  [Language.French]="Salon",   [Language.Spanish]="Sala",      [Language.Japanese]="リビング" },
            ["room.garden"]     = new() { [Language.English]="Garden",    [Language.Italian]="Giardino",  [Language.French]="Jardin",  [Language.Spanish]="Jardín",    [Language.Japanese]="にわ" },
            ["room.library"]    = new() { [Language.English]="Library",   [Language.Italian]="Biblioteca",[Language.French]="Bibliothèque",[Language.Spanish]="Biblioteca",[Language.Japanese]="としょかん" },
            ["onboard.welcome"] = new() { [Language.English]="Welcome to the Moonlight Magic House!", [Language.Italian]="Benvenuto nella Casa della Magia Lunare!", [Language.French]="Bienvenue dans la Maison de la Magie Lunaire!", [Language.Spanish]="¡Bienvenido a la Casa de la Magia Lunar!", [Language.Japanese]="ムーンライト マジックハウスへようこそ！" },
            ["onboard.namePrompt"] = new() { [Language.English]="Give your pet a name!", [Language.Italian]="Dai un nome al tuo amico!", [Language.French]="Donnez un nom à votre ami!", [Language.Spanish]="¡Dale un nombre a tu amigo!", [Language.Japanese]="なまえをつけてあげよう！" },
            ["stage.egg"]       = new() { [Language.English]="Egg",       [Language.Italian]="Uovo",      [Language.French]="Œuf",     [Language.Spanish]="Huevo",     [Language.Japanese]="たまご" },
            ["stage.baby"]      = new() { [Language.English]="Baby",      [Language.Italian]="Cucciolo",  [Language.French]="Bébé",    [Language.Spanish]="Bebé",      [Language.Japanese]="ベビー" },
            ["stage.child"]     = new() { [Language.English]="Child",     [Language.Italian]="Piccolo",   [Language.French]="Enfant",  [Language.Spanish]="Niño",      [Language.Japanese]="こども" },
            ["stage.teen"]      = new() { [Language.English]="Teen",      [Language.Italian]="Giovane",   [Language.French]="Ado",     [Language.Spanish]="Joven",     [Language.Japanese]="ティーン" },
            ["stage.adult"]     = new() { [Language.English]="Adult",     [Language.Italian]="Adulto",    [Language.French]="Adulte",  [Language.Spanish]="Adulto",    [Language.Japanese]="おとな" },
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
