using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Watermelon
{
    public class UIGameOver : UIPage
    {
        [Header("Settings")]
        [SerializeField] float noThanksDelay;

        [SerializeField] UIScalableObject levelFailed;

        [SerializeField] UIFade backgroundFade;

        [SerializeField] UIScalableObject continueButton;

        [Header("No Thanks Label")]
        [SerializeField] Button noThanksButton;
        [SerializeField] TextMeshProUGUI noThanksText;

        private TweenCase continuePingPongCase;

        [NonSerialized]
        public float HiddenPageDelay = 0f;

        public override void Initialise()
        {

        }

        #region Show/Hide

        public override void PlayShowAnimation()
        {
            // RESET

            levelFailed.Hide(immediately: true);
            continueButton.Hide(immediately: true);
            HideNoThanksButton();

            //

            float fadeDuration = 0.3f;
            backgroundFade.Show(fadeDuration, false);

            Tween.DelayedCall(fadeDuration * 0.8f, delegate { 
            
                levelFailed.Show(false, scaleMultiplier: 1.1f);
                
                ShowNoThanksButton(noThanksDelay, immediately: false);

                continueButton.Show(false, scaleMultiplier: 1.05f);

                continuePingPongCase = continueButton.RectTransform.DOPingPongScale(1.0f, 1.05f, 0.9f, Ease.Type.QuadIn, Ease.Type.QuadOut, unscaledTime: true);

                UIController.OnPageOpened(this);
            });

        }

        public override void PlayHideAnimation()
        {
            HiddenPageDelay = 0.3f;

            backgroundFade.Hide(0.3f, false);

            Tween.DelayedCall(0.3f, delegate {

                if (continuePingPongCase != null && continuePingPongCase.isActive) continuePingPongCase.Kill();

                UIController.OnPageClosed(this);
            });
        }

        #endregion

        #region NoThanks Block

        public void ShowNoThanksButton(float delayToShow = 0.3f, bool immediately = true)
        {
            if (immediately)
            {
                noThanksButton.gameObject.SetActive(true);
                noThanksText.gameObject.SetActive(true);

                return;
            }

            Tween.DelayedCall(delayToShow, delegate { 

                noThanksButton.gameObject.SetActive(true);
                noThanksText.gameObject.SetActive(true);

            });
        }

        public void HideNoThanksButton()
        {
            noThanksButton.gameObject.SetActive(false);
            noThanksText.gameObject.SetActive(false);
        }

        #endregion

        #region Buttons 

        public void ContinueButton()
        {
            AudioController.PlaySound(AudioController.Sounds.buttonSound);

        }

        public void NoThanksButton()
        {
            AudioController.PlaySound(AudioController.Sounds.buttonSound);

        }

        #endregion
    }
}