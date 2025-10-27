using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Watermelon.HoleMarket3D;

namespace Watermelon
{
    public class UIGame : UIPage
    {
        [SerializeField] SlicedFilledImage levelProgress;
        [SerializeField] UIScalableObject coinsLabel;
        [SerializeField] TextMeshProUGUI coinsAmountText;
        [SerializeField] CurrencyUIPanelSimple gemsUIPanel;

        [Space]
        [SerializeField] RevivePanel revivePanel;

        [Space]
        [SerializeField] TMP_Text collectedEverythingText;
        [SerializeField] TMP_Text timesOutText;

        TweenCase levelProgressCase;

        public override void Initialise()
        {
            gemsUIPanel.Initialise();
        }

        #region Show/Hide

        public override void PlayHideAnimation()
        {
            UILevelNumberText.Hide(false);

            coinsLabel.Hide(false, scaleMultiplier: 1.05f, onCompleted: () =>
            {
                UIController.OnPageClosed(this);
            });

            collectedEverythingText.gameObject.SetActive(false);
            timesOutText.gameObject.SetActive(false);
        }

        public override void PlayShowAnimation()
        {
            gemsUIPanel.Redraw();

            coinsLabel.Show(false, scaleMultiplier: 1.05f);
            UILevelNumberText.Show(false);

            UIController.OnPageOpened(this);
        }

        #endregion

        #region Cash Label

        public void UpdateCashLabel(int cashAmounts)
        {
            coinsAmountText.text = cashAmounts.ToString();
        }

        #endregion

        public void ShowRevivePanel()
        {
            revivePanel.Show();
        }

        public void UpdateLevelProgress(float value)
        {
            levelProgressCase.KillActive();

            levelProgressCase = Tween.DoFloat(levelProgress.fillAmount, value, 0.2f, (value) => levelProgress.fillAmount = value);
        }

        public void ShowCollectedEverythingText()
        {
            collectedEverythingText.gameObject.SetActive(true);

            collectedEverythingText.transform.localScale = Vector3.one * 0.5f;
            collectedEverythingText.DOScale(1, 0.3f).SetEasing(Ease.Type.SineOut);

            collectedEverythingText.alpha = 0;
            collectedEverythingText.DOFade(1, 0.3f).SetEasing(Ease.Type.SineOut);
        }

        public void ShowTimesOutText()
        {
            timesOutText.gameObject.SetActive(true);

            timesOutText.transform.localScale = Vector3.one * 0.5f;
            timesOutText.DOScale(1, 0.5f).SetEasing(Ease.Type.SineOut);

            timesOutText.alpha = 0;
            timesOutText.DOFade(1, 0.5f).SetEasing(Ease.Type.SineOut);
        }
    }
}
