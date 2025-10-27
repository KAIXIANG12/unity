using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Watermelon.HoleMarket3D;
using Watermelon.Store;

namespace Watermelon
{
    public class UIMainMenu : UIPage
    {
        public readonly float HIDDEN_PAGE_DELAY = 0.55F;
        public readonly float STORE_AD_RIGHT_OFFSET_X = 300F;

        [Space]
        [SerializeField] RectTransform tapToPlayRect;

        [Header("Coins Label")]
        [SerializeField] UIScalableObject coinsLabel;
        [SerializeField] TextMeshProUGUI coinsAmountsText;

        [SerializeField] UIMainMenuButton storeButtonRect;
        [SerializeField] UIMainMenuButton noAdsButtonRect;

        [Space]
        [SerializeField] CurrencyUIPanelSimple coinsUIPanel;
        [SerializeField] CurrencyUIPanelSimple gemsUIPanel;

        [Space]
        [SerializeField] RectTransform upgradesPanel;
        [SerializeField] List<UpgradeButtonBehaviour> upgradeButtons;

        private TweenCase tapToPlayPingPong;
        private TweenCase showHideStoreAdButtonDelayTweenCase;

        private void OnEnable()
        {
            IAPManager.OnPurchaseComplete += OnAdPurchased;
        }

        private void OnDisable()
        {
            IAPManager.OnPurchaseComplete -= OnAdPurchased;
        }

        public override void Initialise() // Called in the Start method
        {
            coinsUIPanel.Initialise();
            gemsUIPanel.Initialise();

            UpdateCashLabel();

            storeButtonRect.Init(STORE_AD_RIGHT_OFFSET_X);
            noAdsButtonRect.Init(STORE_AD_RIGHT_OFFSET_X);

            for(int i = 0; i < upgradeButtons.Count; i++)
            {
                upgradeButtons[i].Initialise(this);
            }
        }

        #region Show/Hide

        public override void PlayShowAnimation()
        {
            // KILL, RESET ANIMATED OBJECT

            showHideStoreAdButtonDelayTweenCase?.Kill();

            HideAdButton(true);

            ShowTapToPlay(false);

            coinsLabel.Show(false);
            storeButtonRect.Show(false);
            UILevelNumberText.Show(false);

            showHideStoreAdButtonDelayTweenCase = Tween.DelayedCall(0.12f, delegate
            {
                ShowAdButton(immediately: false);
            });

            upgradesPanel.anchoredPosition = Vector2.down * 500;
            upgradesPanel.DOAnchoredPosition(Vector2.up * 200, 0.3f).SetEasing(Ease.Type.SineOut);

            SettingsPanel.ShowPanel(false);

            UIController.OnPageOpened(this);

            for(int i = 0; i < upgradeButtons.Count; i++)
            {
                upgradeButtons[i].UpgradeUI();
            }
        }

        public override void PlayHideAnimation()
        {
            // KILL, RESET

            showHideStoreAdButtonDelayTweenCase?.Kill();

            HideTapToPlayText(false);

            coinsLabel.Hide(false);

            HideAdButton(immediately: false);

            showHideStoreAdButtonDelayTweenCase = Tween.DelayedCall(0.1f, delegate
            {
                storeButtonRect.Hide(immediately: false);
            });

            upgradesPanel.DOAnchoredPosition(Vector2.down * 500, HIDDEN_PAGE_DELAY).SetEasing(Ease.Type.SineIn);

            SettingsPanel.HidePanel(false);

            Tween.DelayedCall(HIDDEN_PAGE_DELAY, delegate
            {
                UIController.OnPageClosed(this);
            });
        }

        #endregion

        #region Tap To Play Label

        public void ShowTapToPlay(bool immediately = true)
        {
            if (tapToPlayPingPong != null && tapToPlayPingPong.isActive)
                tapToPlayPingPong.Kill();

            if (immediately)
            {
                tapToPlayRect.localScale = Vector3.one;

                tapToPlayPingPong = tapToPlayRect.transform.DOPingPongScale(1.0f, 1.05f, 0.9f, Ease.Type.QuadIn, Ease.Type.QuadOut, unscaledTime: true);

                return;
            }

            // RESET
            tapToPlayRect.localScale = Vector3.zero;

            tapToPlayRect.DOPushScale(Vector3.one * 1.2f, Vector3.one, 0.35f, 0.2f, Ease.Type.CubicOut, Ease.Type.CubicIn).OnComplete(delegate
            {

                tapToPlayPingPong = tapToPlayRect.transform.DOPingPongScale(1.0f, 1.05f, 0.9f, Ease.Type.QuadIn, Ease.Type.QuadOut, unscaledTime: true);

            });

        }

        public void HideTapToPlayText(bool immediately = true)
        {
            if (tapToPlayPingPong != null && tapToPlayPingPong.isActive)
                tapToPlayPingPong.Kill();

            if (immediately)
            {
                tapToPlayRect.localScale = Vector3.zero;

                return;
            }

            tapToPlayRect.DOPushScale(Vector3.one * 1.2f, Vector3.zero, 0.2f, 0.35f, Ease.Type.CubicOut, Ease.Type.CubicIn);
        }

        #endregion

        #region Coins Label      

        public void UpdateCashLabel()
        {
            Debug.Log("[UI Module] Initialize coins amount here");
            coinsAmountsText.text = "XXXX";
        }

        #endregion

        #region Ad Button Label

        private void ShowAdButton(bool immediately = false)
        {
            if (AdsManager.IsForcedAdEnabled())
            {
                noAdsButtonRect.Show(immediately);
            }
            else
            {
                noAdsButtonRect.Hide(immediately: true);
            }
        }

        private void HideAdButton(bool immediately = false)
        {
            noAdsButtonRect.Hide(immediately);
        }

        private void OnAdPurchased(ProductKeyType productKeyType)
        {
            if (productKeyType == ProductKeyType.NoAds)
            {
                HideAdButton(immediately: true);
            }
        }

        #endregion

        #region Buttons

        public void StoreButton()
        {
            UIController.HidePage<UIMainMenu>();

            UIMainMenu uiMainMenu = UIController.GetPage<UIMainMenu>();
            UILevelNumberText.Hide(false);

            Tween.DelayedCall(uiMainMenu.HIDDEN_PAGE_DELAY, delegate
            {
                //Debug.Log("[UI Module] Show store page here");

                StoreController.OpenStore();
                
            });

            AudioController.PlaySound(AudioController.Sounds.buttonSound);
            Vibration.Vibrate(10);
        }


        public void NoAdButton()
        {
            IAPManager.BuyProduct(ProductKeyType.NoAds);
            AudioController.PlaySound(AudioController.Sounds.buttonSound);
            Vibration.Vibrate(10);
        }

        #endregion
    }


}
