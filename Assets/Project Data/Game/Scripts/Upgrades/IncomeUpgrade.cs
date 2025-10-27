using UnityEngine;

namespace Watermelon.HoleMarket3D
{
    using Upgrades;

    [CreateAssetMenu(menuName = "Content/Upgrades/Income Upgrade", fileName = "Income Upgrade")]
    public class IncomeUpgrade : Upgrade<IncomeUpgrade.IncomeUpgradeStage>
    {
        private void Awake()
        {
            upgradeType = UpgradeType.Income;
        }

        public override void Initialise()
        {

        }

        [System.Serializable]
        public class IncomeUpgradeStage : BaseUpgradeStage
        {
            [SerializeField] int additionalIncome;
            public int AdditionalIncome => additionalIncome;
        }
    }
}
