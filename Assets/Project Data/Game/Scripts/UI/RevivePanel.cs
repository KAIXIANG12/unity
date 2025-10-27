using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Watermelon.HoleMarket3D
{
    public class RevivePanel : MonoBehaviour
    {
        [SerializeField] Canvas canvas;
        [SerializeField] Image backgroundImage;

        [Space]
        [SerializeField] RectTransform panel;
        [SerializeField] Vector2 hiddenPos;
        [SerializeField] Vector2 shownPos;
        [SerializeField] float showDuration;

        [Space]
        [SerializeField] RectTransform heartRect;
        [SerializeField] AnimationCurve heartScaleCurve;
        [SerializeField] float heartAnimDuration;

        [Space]
        [SerializeField] TMP_Text additionalTimeText;

        [Space]
        [SerializeField] Button reviveButton;
        [SerializeField] Button tapToSkipButton;

        private void Awake()
        {
            reviveButton.onClick.AddListener(OnReviveClicked);
            tapToSkipButton.onClick.AddListener(OnSkipClicked);
        }

        [Button]
        public void Show()
        {
            canvas.enabled = true;

            panel.anchoredPosition = hiddenPos;
            panel.DOAnchoredPosition(shownPos, showDuration).SetEasing(Ease.Type.SineOut);
            backgroundImage.SetAlpha(0);
            backgroundImage.DOFade(0.6f, showDuration);

            StartCoroutine(HeartCoroutine());
        }

        [Button]
        public void Hide()
        {
            panel.DOAnchoredPosition(hiddenPos, showDuration).SetEasing(Ease.Type.SineIn);
            backgroundImage.DOFade(0, showDuration).OnComplete(() => {
                canvas.enabled = false;

                StopAllCoroutines();
            });
        }

        private IEnumerator HeartCoroutine()
        {
            float time = 0;
            while (true)
            {
                yield return null;
                time += Time.deltaTime;
                time %= heartAnimDuration;

                var t = time / heartAnimDuration;

                heartRect.localScale = Vector3.one * heartScaleCurve.Evaluate(t);
            }
            
        }

        private void OnReviveClicked()
        {
            AdsManager.ShowRewardBasedVideo(OnRewardWatched);
        }

        private void OnRewardWatched(bool success)
        {
            if(success)
            {
                LevelController.ReviveSuccess();
            } else
            {
                LevelController.ReviveFailed();
            }
            Hide();
        }

        private void OnSkipClicked()
        {
            LevelController.ReviveFailed();
            Hide();
        }
    }
}