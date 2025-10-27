using UnityEngine;
using UnityEngine.UI;
using System;
using Watermelon.HoleMarket3D;
using TMPro;

namespace Watermelon
{
    public class UIComplete : UIPage
    {
        [SerializeField] UIFade backgroundFade;

        [Space]
        [SerializeField] UIScalableObject levelCompleteLabel;

        [Space]
        [SerializeField] UIScalableObject rewardLabel;
        [SerializeField] Image rewardCurrencyImage;
        [SerializeField] TextMeshProUGUI rewardAmountText;

        [Space]
        [SerializeField] UIFade multiplyRewardButton;
        [SerializeField] UIFade noThanksButtonText;
        [SerializeField] Button noThanksButton;
        [SerializeField] UIFade continueButtonFade;
        [SerializeField] Button continueButton;

        public static float HideDuration => 0.25f;

        private TweenCase noThanksAppearTween;

        private Currency.Type rewardCurrency;
        private int rewardAmount;

        private bool rewardGranted = false;

        public override void Initialise()
        {

        }

        public void SetReward(Currency.Type currencyType, int rewardAmount)
        {
            rewardCurrency = currencyType;
            this.rewardAmount = rewardAmount;

            rewardCurrencyImage.sprite = CurrenciesController.GetCurrency(rewardCurrency).Icon;
        }

        #region Show/Hide
        public override void PlayShowAnimation()
        {
            rewardLabel.Hide(immediately: true);
            multiplyRewardButton.Hide(immediately: true);
            noThanksButtonText.Hide(immediately: true);
            noThanksButton.interactable = false;
            continueButtonFade.Hide(immediately: true);
            continueButton.gameObject.SetActive(false);

            backgroundFade.Show(duration: 0.3f);
            levelCompleteLabel.Show();

            ShowRewardLabel(rewardAmount, false, 0.3f, delegate // update reward here
            {
                rewardLabel.RectTransform.DOPushScale(Vector3.one * 1.1f, Vector3.one, 0.2f, 0.2f).OnComplete(delegate
                {
                    multiplyRewardButton.Show();

                    noThanksAppearTween = Tween.DelayedCall(1.5f, delegate
                    {
                        noThanksButtonText.Show();
                        noThanksButton.interactable = true;
                    });
                });
            });

            rewardGranted = false;

            UIController.OnPageOpened(this);
        }

        public override void PlayHideAnimation()
        {
            backgroundFade.Hide(HideDuration, false);

            Tween.DelayedCall(HideDuration, delegate
            {
                canvas.enabled = false;
                isPageDisplayed = false;

                UIController.OnPageClosed(this);
            });
        }
        #endregion

        #region RewardLabel
        public void ShowRewardLabel(float rewardAmount, bool immediately = false, float duration = 0.3f, Action onComplted = null)
        {
            rewardLabel.Show(immediately);

            if (immediately)
            {
                rewardAmountText.text = "+" + rewardAmount;
                onComplted?.Invoke();

                return;
            }

            rewardAmountText.text = "+" + 0;

            Tween.DoFloat(0, rewardAmount, duration, (float value) =>
            {

                rewardAmountText.text = "+" + (int)value;
            }).OnComplete(delegate
            {

                onComplted?.Invoke();
            });
        }

        #endregion

        #region Buttons

        public void MultiplyRewardButton()
        {
            AudioController.PlaySound(AudioController.Sounds.buttonSound);

            if (noThanksAppearTween != null && noThanksAppearTween.isActive)
            {
                noThanksAppearTween.Kill();
            }

            AdsManager.ShowRewardBasedVideo((bool success) =>
            {
                if (success)
                {
                    rewardGranted = true;

                    int rewardMult = 3;

                    noThanksButton.interactable = false;
                    noThanksButtonText.Hide(immediately: true);
                    multiplyRewardButton.Hide(immediately: true);

                    ShowRewardLabel(rewardAmount * rewardMult, false, 0.3f, delegate // update reward here
                    {
                        noThanksButton.interactable = true;
                        continueButton.gameObject.SetActive(true);
                        continueButtonFade.Show();
                    });
                }
                else
                {
                    NoThanksButton();
                }
            });
        }

        public void NoThanksButton()
        {
            AudioController.PlaySound(AudioController.Sounds.buttonSound);

            if (rewardCurrency == Currency.Type.Money)
            {
                CurrenciesController.Add(Currency.Type.Money, CurrenciesController.Get(Currency.Type.TempMoney) * (rewardGranted ? 3 : 1));

                LevelController.ReplayLevel();
            }
            else
            {
                CurrenciesController.Add(Currency.Type.Gems, 3 * (rewardGranted ? 3 : 1));

                GameController.NextLevel();
            }

            if (!rewardGranted) AdsManager.ShowInterstitial((bool watched) => { });

            CurrenciesController.Set(Currency.Type.TempMoney, 0);
        }

        public void HomeButton()
        {
            AudioController.PlaySound(AudioController.Sounds.buttonSound);

            if (rewardCurrency == Currency.Type.Money)
            {
                CurrenciesController.Add(Currency.Type.Money, CurrenciesController.Get(Currency.Type.TempMoney) * (rewardGranted ? 3 : 1));

                LevelController.ReplayLevel();
            }
            else
            {
                CurrenciesController.Add(Currency.Type.Gems, 3 * (rewardGranted ? 3 : 1));

                GameController.NextLevel();
            }

            if (!rewardGranted) AdsManager.ShowInterstitial((bool watched) => { });

            CurrenciesController.Set(Currency.Type.TempMoney, 0);
        }

        #endregion
    }
}
