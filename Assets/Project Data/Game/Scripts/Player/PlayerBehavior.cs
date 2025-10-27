using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;
using Watermelon.Store;

namespace Watermelon.HoleMarket3D
{
    public class PlayerBehavior : MonoBehaviour
    {
        private static PlayerBehavior instance;
        public static PlayerBehavior Player => instance;

        [SerializeField] PlayerGraphics graphics;

        public static Vector3 Position => instance.transform.position;

        private static float radius = 5;
        public static float Radius
        {
            get => radius; set
            {
                radius = value;
                RadiusSqr = value * value;
                OnRadiusChanged?.Invoke(radius);
            }
        }

        public static float RadiusSqr { get; private set; } = 25;

        public static bool IsMoving { get; private set; }

        private HoleSizeUpgrade holeSizeUpgrade;

        private Joystick joystick;

        public static SimpleFloatCallback OnRadiusChanged;

        private float speed;
        private float maxSpeed;
        private float acceleration;

        TweenCase radiusCase;
        private bool subscribed;

        private bool canExpand = false;
        private float expandCost;
        private float collectedCost;
        private int nextEnergyStage;

        #region Init

        private void Awake()
        {
            instance = this;
        }

        private void Start()
        {
            joystick = Joystick.Instance;
            CurrenciesController.GetCurrency(Currency.Type.TempMoney).OnCurrencyChanged += OnCurrencyAmountChanged;

            StoreController.OnProductSelected += SwapGraphics;
        }

        public void Init()
        {
            transform.position = LevelBehavior.PlayerSpawnPosition;

            if (!subscribed)
            {
                holeSizeUpgrade = UpgradesController.GetUpgrade<HoleSizeUpgrade>(UpgradeType.HoleSize);
                holeSizeUpgrade.OnUpgraded += OnRadiusChangedCallback;
            }

            var stage = holeSizeUpgrade.GetCurrentStage();

            Radius = stage.HoleRadius;
            maxSpeed = stage.MaxSpeed;
            acceleration = maxSpeed * 5;

            CameraController.SetOffsetMultiplier(stage.CameraOffsetMultiplier);

            SpawnGraphics();

            var nextStage = holeSizeUpgrade.NextStage;
            canExpand = nextStage != null;

            graphics.IsEnergyActive = canExpand;

            if (canExpand)
            {
                nextEnergyStage = holeSizeUpgrade.UpgradeLevel + 1;

                expandCost = nextStage.Price / 2;
                collectedCost = 0;
            }
        }

        #endregion

        #region Graphics

        private void SwapGraphics(TabType tab, ProductData product)
        {
            Destroy(graphics.gameObject);

            SpawnGraphics();

            var nextStage = holeSizeUpgrade.NextStage;
            canExpand = nextStage != null;

            graphics.IsEnergyActive = canExpand;
        }

        private void SpawnGraphics()
        {
            graphics = Instantiate(StoreController.GetSelectedPrefab(TabType.Skins).GetComponent<PlayerGraphics>(), transform, false);

            graphics.transform.localPosition = Vector3.zero;
            graphics.transform.localRotation = Quaternion.identity;
            graphics.transform.localScale = Vector3.one;

            graphics.Init();
            graphics.Energy = 0;
        }

        #endregion Graphics

        #region Update

        public void Update()
        {
            if (!LevelController.IsGamePlayActive) return;
            RunningUpdate();
        }

        private void RunningUpdate()
        {
            if (joystick.IsJoysticTouched && joystick.Input.sqrMagnitude > 0.1f)
            {
                if (!IsMoving)
                {
                    IsMoving = true;
                    speed = 0;
                }
            }
            else
            {
                if (IsMoving)
                {
                    IsMoving = false;
                }
            }

            float maxAlowedSpeed = joystick.FormatInput.magnitude * maxSpeed;

            if (speed > maxAlowedSpeed)
            {
                speed -= acceleration * Time.deltaTime;
                if (speed < maxAlowedSpeed)
                    speed = maxAlowedSpeed;
            }
            else
            {
                speed += acceleration * Time.deltaTime;
                if (speed > maxAlowedSpeed)
                    speed = maxAlowedSpeed;
            }

            var position = transform.position;

            position += Quaternion.Euler(0, Camera.main.transform.eulerAngles.y, 0) * joystick.FormatInput * Time.deltaTime * speed;

            var bounds = LevelBehavior.LevelBounds;

            if (position.x < bounds.minX) position.x = bounds.minX;
            if (position.x > bounds.maxX) position.x = bounds.maxX;

            if (position.z < bounds.minY - Radius - 3) position.z = bounds.minY - Radius - 3;
            if (position.z > bounds.maxY) position.z = bounds.maxY;

            transform.position = position;
        }

        #endregion

        #region Callbacks

        private void OnCurrencyAmountChanged(Currency currency, int difference)
        {
            if (!LevelController.IsGamePlayActive) return;

            if (difference <= 0) return;
            if (!canExpand) return;

            collectedCost += difference;

            if (collectedCost >= expandCost)
            {
                collectedCost -= expandCost;

                var prevStage = holeSizeUpgrade.GetStage(nextEnergyStage);

                radiusCase.KillActive();
                radiusCase = DoRadius(prevStage.HoleRadius, holeSizeUpgrade.TransitionDuration).SetEasing(holeSizeUpgrade.TransitionEase);

                maxSpeed = prevStage.MaxSpeed;
                acceleration = maxSpeed * 5;

                CameraController.SetOffsetMultiplier(prevStage.CameraOffsetMultiplier, holeSizeUpgrade.TransitionDuration, holeSizeUpgrade.TransitionEase);

                nextEnergyStage++;

                var stage = holeSizeUpgrade.GetStage(nextEnergyStage);

                canExpand = stage != null;

                if (canExpand)
                {
                    expandCost = stage.Price / 2;
                    graphics.Energy = collectedCost / expandCost;
                } else
                {
                    graphics.IsEnergyActive = false;
                }
            } else
            {
                graphics.Energy = collectedCost / expandCost;
            }
        }

        private void OnRadiusChangedCallback()
        {
            var stage = holeSizeUpgrade.GetCurrentStage();
            var targetRadius = stage.HoleRadius;

            if(holeSizeUpgrade.TransitionDuration > 0)
            {
                radiusCase.KillActive();

                radiusCase = DoRadius(targetRadius, holeSizeUpgrade.TransitionDuration).SetEasing(holeSizeUpgrade.TransitionEase);
            } else
            {
                radius = targetRadius;
            }

            maxSpeed = stage.MaxSpeed;
            acceleration = maxSpeed * 5;

            CameraController.SetOffsetMultiplier(stage.CameraOffsetMultiplier, holeSizeUpgrade.TransitionDuration, holeSizeUpgrade.TransitionEase);

            var nextStage = holeSizeUpgrade.NextStage;
            canExpand = nextStage != null;

            graphics.IsEnergyActive = canExpand;

            if (canExpand)
            {
                nextEnergyStage = holeSizeUpgrade.UpgradeLevel + 1;

                expandCost = nextStage.Price / 2;
            }

            collectedCost = 0;

            Tween.NextFrame(() => transform.DOMove(LevelBehavior.PlayerSpawnPosition, 0.2f).SetEasing(Ease.Type.SineInOut));
        }

        #endregion

        #region Player Manipulations

        public void MoveToStart()
        {
            transform.DOMove(LevelBehavior.PlayerSpawnPosition, 1f).SetEasing(Ease.Type.SineInOut);
            DoRadius(2, 1f).SetEasing(Ease.Type.SineInOut);
        }

        public void ResetPlayer(bool completely = false)
        {
            if (completely) holeSizeUpgrade.ResetUpgrade();

            var stage = holeSizeUpgrade.GetCurrentStage();
            var targetRadius = stage.HoleRadius;

            maxSpeed = stage.MaxSpeed;
            acceleration = maxSpeed * 5;

            instance.DoRadius(targetRadius, 0.6f);

            CameraController.SetOffsetMultiplier(stage.CameraOffsetMultiplier, holeSizeUpgrade.TransitionDuration, holeSizeUpgrade.TransitionEase);

            var nextStage = holeSizeUpgrade.NextStage;
            canExpand = nextStage != null;

            graphics.IsEnergyActive = canExpand;
            graphics.Energy = 0;

            if (canExpand)
            {
                nextEnergyStage = holeSizeUpgrade.UpgradeLevel + 1;

                expandCost = nextStage.Price / 2;
            }

            collectedCost = 0;
        }

        #endregion

        #region Helpers

        private TweenCase DoRadius(float targetRadius, float time)
        {
            return Tween.DoFloat(radius, targetRadius, time, value => Radius = value);
        }

        public static float DistanceSqr(Transform transform) => (Position - transform.position).sqrMagnitude;

        #endregion
    }
}