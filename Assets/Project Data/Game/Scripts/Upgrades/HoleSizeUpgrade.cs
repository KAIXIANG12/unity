using UnityEngine;

namespace Watermelon.HoleMarket3D
{
    using Upgrades;

    [CreateAssetMenu(menuName = "Content/Upgrades/Hole Size Upgrade", fileName = "Hole Size Upgrade")]
    public class HoleSizeUpgrade : Upgrade<HoleSizeUpgrade.HoleSizeUpgradeStage>
    {

        [SerializeField] float transitionDuration;
        [SerializeField] Ease.Type transitionEase;

        public float TransitionDuration => transitionDuration;
        public Ease.Type TransitionEase => transitionEase;

        private void Awake()
        {
            upgradeType = UpgradeType.HoleSize;
        }

        public override void Initialise()
        {

        }

        [System.Serializable]
        public class HoleSizeUpgradeStage : BaseUpgradeStage
        {
            [SerializeField] float holeRadius;
            public float HoleRadius => holeRadius;

            [SerializeField] float maxSpeed;
            public float MaxSpeed => maxSpeed;

            [SerializeField] float cameraOffsetMultiplier = 1;
            public float CameraOffsetMultiplier => cameraOffsetMultiplier;
        }
    }
}
