using UnityEngine;
using Watermelon.Store;
using Watermelon.Upgrades;

namespace Watermelon.HoleMarket3D
{
    public class GameController : MonoBehaviour
    {
        private static GameController instance;

        [SerializeField] UIController uiController;

        private static LevelController levelController;
        private static UpgradesController upgradeController;
        private static CurrenciesController currenciesController;
        private static FloatingTextController floatingTextController;

        private SimpleIntSave levelIdSave;

        public static int LevelId { 
            get => instance.levelIdSave.Value; 
            private set => instance.levelIdSave.Value = value; 
        }

        private void Awake()
        {
            instance = this;

            SaveController.Initialise(true);
            levelIdSave = SaveController.GetSaveObject<SimpleIntSave>("Level Id");

            if (!TryGetComponent(out upgradeController)) Debug.LogError("Script Holder doesn't have UpgradesController script added to it");
            if (!TryGetComponent(out currenciesController)) Debug.LogError("Script Holder doesn't have CurrenciesController script added to it");
            if (!TryGetComponent(out levelController)) Debug.LogError("Script Holder doesn't have LevelController script added to it");
            if (!TryGetComponent(out floatingTextController)) Debug.LogError("Script Holder doesn't have FloatingTextController script added to it");
        }

        private void Start()
        {
            InitialiseGame();
        }

        public void InitialiseGame()
        {
            currenciesController.Initialise();
            upgradeController.Initialise();
            floatingTextController.Inititalise();

            uiController.Initialise();
            uiController.InitialisePages();

            StoreController.Init();

            levelController.Initialise();

            levelController.LoadLevel(LevelId);

            UIController.ShowPage<UIMainMenu>();

            // Move this method to the point when the game is fully loaded
            GameLoading.MarkAsReadyToHide();
        }

        public static void NextLevel()
        {
            CurrenciesController.Set(Currency.Type.Money, 0);

            LevelId++;
            levelController.LoadNextLevel(LevelId);
        }

        private void OnDestroy()
        {
#if UNITY_EDITOR
            SaveController.ForceSave();
#endif
        }

        private void OnApplicationFocus(bool focus)
        {
#if !UNITY_EDITOR
        if(!focus) SaveController.Save();
#endif
        }
    }
}