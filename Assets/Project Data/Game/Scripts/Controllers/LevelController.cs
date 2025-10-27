using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Watermelon.HoleMarket3D
{
    public class LevelController : MonoBehaviour
    {
        public static LevelController instance;

        [SerializeField] LevelDatabase database;
        public static LevelDatabase Database => instance.database;

        [Space]
        [SerializeField] PlayerBehavior player;

        [Space]
        [SerializeField] Material groundMaterial;
        [SerializeField] Material voidMaterial;

        public static Level LevelData { get; private set; }
        public static LevelBehavior LevelBehavior { get; private set; }

        public static bool IsGamePlayActive { get; private set; }

        private static UIGame uiGame;
        private static UIPersistent uiPersistent;

        private Timer timer;

        private static bool isRevived = false;

        private void Awake()
        {
            instance = this;
        }

        public void Initialise()
        {
            timer = Timer.GetTimer();
            timer.OnTimerCompleted += OnTimerCompleted;

            uiGame = UIController.GetPage<UIGame>();
            uiPersistent = UIController.GetPage<UIPersistent>();

            LevelBehavior = new GameObject("Level Behavior").AddComponent<LevelBehavior>();
        }

        public void LoadLevel(int levelId)
        {
            LevelData = database.GetLevel(levelId);

            LevelBehavior.Init(LevelData, groundMaterial, voidMaterial);

            player.Init();

            CameraController.SetMainTarget(player.transform);

            UIController.ShowPage<UIPersistent>();
            UILevelNumberText.UpdateLevelNumber();

            Joystick.OnJoystickTouched += OnJoystickTouched;
        }

        public void LoadNextLevel(int levelId)
        {
            LevelData = database.GetLevel(levelId);

            LevelBehavior.NextLevel(LevelData);

            instance.timer.ResetTimer(true);
            instance.timer.Show();

            CameraController.EnableCamera(CameraType.Gameplay);

            Joystick.Instance.EnableControl();
            Joystick.OnJoystickTouched += instance.OnJoystickTouched;

            PlayerBehavior.Player.ResetPlayer(true);

            isRevived = false;

            UIController.HidePage<UIComplete>();
            UIController.ShowPage<UIMainMenu>();

            UILevelNumberText.UpdateLevelNumber();
            uiGame.UpdateLevelProgress(0);
        }

        private void OnJoystickTouched()
        {
            Joystick.OnJoystickTouched -= OnJoystickTouched;

            UIController.HidePage<UIMainMenu>(UIController.ShowPage<UIGame>);

            CameraController.EnableCamera(CameraType.Gameplay);

            CurrenciesController.Set(Currency.Type.TempMoney, 0);

            timer.Play();

            IsGamePlayActive = true;
        }

        private void OnTimerCompleted()
        {
            IsGamePlayActive = false;

            Joystick.Instance.DisableControl();

            if (!isRevived)
            {
                uiGame.ShowRevivePanel();
            } else
            {
                ReviveFailed();
            }
        }

        public static void ReviveFailed()
        {
            Tween.DelayedCall(0.3f, uiGame.ShowTimesOutText);
            Tween.DelayedCall(2.3f, instance.SpawnFinish);

            uiPersistent.HideTimer(); 

            Joystick.Instance.DisableControl();

            AudioController.PlaySound(AudioController.Sounds.looseSound);
        }

        private void SpawnFinish()
        {
            UIController.HidePage<UIGame>();

            LevelBehavior.SpawnFinish();

            PlayerBehavior.Player.MoveToStart();

            CameraController.SetOffsetMultiplier(1f, 0.5f);
            CameraController.EnableCamera(CameraType.Finish);
        }

        public static void ReviveSuccess()
        {
            isRevived = true;

            IsGamePlayActive = true;

            instance.timer.AddTime(10);
            instance.timer.Play();

            Joystick.Instance.EnableControl();
        }

        public static void EverythingCollected()
        {
            IsGamePlayActive = false;
            instance.timer.Pause();

            uiGame.ShowCollectedEverythingText();
            Tween.DelayedCall(2.3f, instance.SpawnFinish);

            uiPersistent.HideTimer();
            Joystick.Instance.DisableControl();

            AudioController.PlaySound(AudioController.Sounds.winSound);
        }

        public static void ReplayLevel()
        {
            LevelBehavior.ResetLevel();

            instance.timer.ResetTimer();
            instance.timer.Show();

            CameraController.EnableCamera(CameraType.Gameplay);

            UIController.HidePage<UIComplete>();
            UIController.ShowPage<UIMainMenu>();

            uiGame.UpdateLevelProgress(0);

            PlayerBehavior.Player.ResetPlayer();

            Joystick.Instance.EnableControl();
            Joystick.OnJoystickTouched += instance.OnJoystickTouched;

            isRevived = false;
        }
    }
}