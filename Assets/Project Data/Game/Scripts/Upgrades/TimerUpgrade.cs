using UnityEngine;

namespace Watermelon.HoleMarket3D
{
    using Upgrades;

    [CreateAssetMenu(menuName = "Content/Upgrades/Timer Upgrade", fileName = "Timer Upgrade")]
    public class TimerUpgrade : Upgrade<TimerUpgrade.TimerUpgradeStage>
    {
        private void Awake()
        {
            upgradeType = UpgradeType.Timer;
        }

        public override void Initialise()
        {

        }

        [System.Serializable]
        public class TimerUpgradeStage : BaseUpgradeStage
        {
            [SerializeField] int seconds;
            public float Seconds => seconds;
        }
    }
}