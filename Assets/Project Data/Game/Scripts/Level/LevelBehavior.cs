using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Watermelon.HoleMarket3D
{
    public class LevelBehavior : MonoBehaviour
    {
        private static readonly int FLOATING_TEXT_HASH = FloatingTextController.GetHash("Income");

        private List<PropBehavior> prop = new List<PropBehavior>();

        GroundBehavior groundBehavior;

        private IncomeUpgrade incomeUpgrade;

        private UIGame uiGame;

        private int maxPrice;
        private int collectedPrice;

        private FinishBehavior finish;

        private Material groundMaterial;

        private HoleSizeUpgrade holeUpgrade;

        public static Vector2 LevelSize { get; private set; }
        public static Vector2 LevelCenter { get; private set; }

        public static LevelBounds LevelBounds { get; private set; }

        public static Vector3 PlayerSpawnPosition { get; private set; }

        private List<PropBehavior> collectedProp;

        public void Init(Level level, Material groundMaterial, Material voidMaterial)
        {
            holeUpgrade = UpgradesController.GetUpgrade<HoleSizeUpgrade>(UpgradeType.HoleSize);
            holeUpgrade.OnUpgraded += OnHoleRadiusUpgraded;

            InitProp(level);

            incomeUpgrade = UpgradesController.GetUpgrade<IncomeUpgrade>(UpgradeType.Income);

            uiGame = UIController.GetPage<UIGame>();
            uiGame.UpdateLevelProgress(0);

            groundBehavior = new GameObject("Ground Behavior").AddComponent<GroundBehavior>();
            groundBehavior.Init(5, groundMaterial, voidMaterial);

            groundMaterial.SetColor("_BaseColor", level.GroundMainColor);
            groundMaterial.SetColor("_AdditionalColor", level.GroundAdditionalColor);

            this.groundMaterial = groundMaterial;
        }

        private void InitProp(Level level)
        {
            prop = new List<PropBehavior>();

            var minPos = Vector2.positiveInfinity;
            var maxPos = Vector2.negativeInfinity;

            maxPrice = 0;
            for (int i = 0; i < level.Props.Length; i++)
            {
                var propPlacement = level.Props[i];
                var propData = LevelController.Database.GetProp(propPlacement.PropId);

                var propObject = Instantiate(propData.Prefab);
                var propBehavior = propObject.GetComponent<PropBehavior>();
                propBehavior.Init(propData, propPlacement);

                prop.Add(propBehavior);

                maxPrice += propData.Cost;

                propBehavior.SetOnEatenCallback(OnPropEaten);

                if(propPlacement.Position.x < minPos.x) minPos.x = propPlacement.Position.x;
                if(propPlacement.Position.x > maxPos.x) maxPos.x = propPlacement.Position.x;

                if(propPlacement.Position.z < minPos.y) minPos.y = propPlacement.Position.z;
                if(propPlacement.Position.z > maxPos.y) maxPos.y = propPlacement.Position.z;
            }

            LevelCenter = (minPos + maxPos) / 2;
            LevelSize = maxPos - minPos;

            var holeRadius = holeUpgrade.GetCurrentStage().HoleRadius;

            PlayerSpawnPosition = Vector3.zero.SetZ(LevelCenter.y - LevelSize.y / 2f - holeRadius - 3);

            LevelBounds = new LevelBounds { 
                minX = minPos.x,
                minY = minPos.y,
                maxX = maxPos.x,
                maxY = maxPos.y
            };

            collectedPrice = 0;
        }

        public void ResetLevel()
        {
            for(int i = 0; i < prop.Count; i++)
            {
                var item = prop[i];

                if (item.gameObject.activeSelf)
                {
                    item.DOScale(0, 0.2f, Random.value * 0.1f).SetEasing(Ease.Type.SineIn);
                }
            }

            finish.HideAndDestroy();

            Tween.DelayedCall(0.3f, RespawnLevel);
        }

        public void NextLevel(Level newLevel)
        {
            incomeUpgrade.ResetUpgrade();

            StartCoroutine(NextLevelCoroutine(newLevel));
        }

        private IEnumerator NextLevelCoroutine(Level newLevel)
        {
            for (int i = 0; i < prop.Count; i++)
            {
                var item = prop[i];

                if (item.gameObject.activeSelf)
                {
                    item.DOScale(0, 0.2f, Random.value * 0.1f).SetEasing(Ease.Type.SineIn).OnComplete(() => Destroy(item.gameObject));
                }
                else
                {
                    Destroy(item.gameObject);
                }
            }

            finish.HideAndDestroy();

            yield return new WaitForSeconds(0.3f);

            InitProp(newLevel);

            for(int i = 0; i < prop.Count; i++)
            {
                var item = prop[i];

                item.transform.localScale = Vector3.zero;

                item.DOScale(1, 0.4f, 0.1f).SetEasing(Ease.Type.BackOut);
            }

            groundMaterial.DOColor(Shader.PropertyToID("_BaseColor"), newLevel.GroundMainColor, 0.3f);
            groundMaterial.DOColor(Shader.PropertyToID("_AdditionalColor"), newLevel.GroundAdditionalColor, 0.3f);
        }

        private void RespawnLevel()
        {
            for(int i = 0; i < prop.Count; i++)
            {
                var item = prop[i];

                item.ResetPosition();

                item.transform.localScale = Vector3.zero;
                item.gameObject.SetActive(true);

                item.DOScale(1, 0.3f, 0.2f).SetEasing(Ease.Type.BackOut);
            }

            collectedPrice = 0;
        }

        private void OnPropEaten(PropBehavior prop)
        {
            int propIncome = prop.Prop.Cost;
            int income = propIncome + incomeUpgrade.GetCurrentStage().AdditionalIncome;
            CurrenciesController.Add(Currency.Type.TempMoney, income);

            FloatingTextController.SpawnFloatingText(FLOATING_TEXT_HASH, $"{income}", (PlayerBehavior.Position + (Vector3.forward + Vector3.right).normalized * PlayerBehavior.Radius).SetY(1), CameraController.MainCamera.transform.rotation, CameraController.OffsetMultiplier);
            
            collectedPrice += propIncome;

            uiGame.UpdateLevelProgress(collectedPrice / (float) maxPrice);

            if(collectedPrice >= maxPrice)
            {
                // Have won
                LevelController.EverythingCollected();
            }
        }

        public void SpawnFinish()
        {
            StartCoroutine(SpawnFinishCoroutine());
        }

        // For optimization purposes we do not create more than 30 tweens per frame
        private IEnumerator SpawnFinishCoroutine()
        {
            collectedProp = new List<PropBehavior>();

            int yieldId = 30;
            float delay = 0.2f;

            for(int i = 0; i < prop.Count; i++)
            {
                if(i == yieldId)
                {
                    yield return null;
                    yieldId += 30;

                    delay -= Time.deltaTime;
                    if(delay < 0) delay = 0;
                }

                var item = prop[i];

                if (!item.gameObject.activeSelf)
                {
                    collectedProp.Add(item);
                }
                else
                {
                    item.DOScale(0, 0.5f, Random.value * delay).SetEasing(Ease.Type.BackIn).OnComplete(() => item.gameObject.SetActive(false));
                }
            }

            // Calculating exactly 0.7 sec from the start of the coroutine
            float initFinishDelay = 0.7f - (0.2f - delay);

            yield return new WaitForSeconds(initFinishDelay);

            finish = Instantiate(LevelController.LevelData.FinishPrefab).GetComponent<FinishBehavior>();
            finish.transform.position = PlayerSpawnPosition;
            finish.Spawn(collectedProp, prop.Count);

            UIController.ShowPage<UIFinish>();
        }

        private void OnHoleRadiusUpgraded()
        {
            var holeRadius = holeUpgrade.GetCurrentStage().HoleRadius;

            PlayerSpawnPosition = Vector3.zero.SetZ(LevelCenter.y - LevelSize.y / 2f - holeRadius - 3);
        }
    }

    public struct LevelBounds
    {
        public float minX;
        public float maxX;
        public float minY;
        public float maxY;

        public float MaxAbsCoord()
        {
            float minXAbs = Mathf.Abs(minX);
            float maxXAbs = Mathf.Abs(maxX);
            float minYAbs = Mathf.Abs(minY);
            float maxYAbs = Mathf.Abs(maxY);

            var absX = minXAbs > maxXAbs ? minXAbs : maxXAbs;
            var absY = minYAbs > maxYAbs ? minYAbs : maxYAbs;

            return absX > absY ? absX : absY;
        }
    }
}