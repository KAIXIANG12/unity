using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Watermelon.HoleMarket3D
{
    public class CustomerBehavior : MonoBehaviour
    {
        private static readonly int HAPPY_TRIGGER = Animator.StringToHash("HAPPY");
        private static readonly int SAD_TRIGGER = Animator.StringToHash("SAD");

        [SerializeField] Animator animator;
        [SerializeField] BoxCollider cartCollider;

        [Space]
        [SerializeField] CanvasGroup fillbarFade;
        [SerializeField] SlicedFilledImage fillbar;

        public int RequiredItemsAmount { get; private set; }
        public int FilledAmount { get; private set; }
        public int RealFilledAmount { get; private set; }

        public float Fill { get => fillbar.fillAmount; set => fillbar.fillAmount = value; }

        private TweenCase fillCase;
        private float pitchMultiplier = 1;
        private float prevSoundTime = 0;
        private float soundInterval = 0.02f;

        public void Spawn(int requiredAmount)
        {
            var scale = transform.localScale;
            transform.localScale = Vector3.zero;
            transform.DOScale(scale, 0.4f, Random.value * 0.2f).SetEasing(Ease.Type.SineOut);

            fillbarFade.alpha = 0;
            fillbarFade.DOFade(1, 0.4f);

            Fill = 0;

            RequiredItemsAmount = requiredAmount;

            FilledAmount = 0;
            RealFilledAmount = 0;

            pitchMultiplier = 1;
        }

        public bool AddItemToCart(PropBehavior prop)
        {
            Vector3 startPos = prop.transform.position;
            Vector3 endPos = GetRandomPosInCart();

            var key1 = startPos + Vector3.up * 5;
            var key2 = endPos + Vector3.up * 5;

            Tween.DoFloat(0, 1, 0.5f, (float t) =>
            {
                prop.transform.position = Bezier.EvaluateCubic(startPos, key1, key2, endPos, t);
            }).SetEasing(Ease.Type.SineOut).OnComplete(() =>
            {
                RealFilledAmount++;

                fillCase.KillActive();
                fillCase = Tween.DoFloat(Fill, (float)RealFilledAmount / RequiredItemsAmount, 0.1f, (value) => Fill = value);

                if (RealFilledAmount == RequiredItemsAmount) GetHappy();

            });

            Tween.DelayedCall(0.1f, () => {
                if (Time.time > prevSoundTime + soundInterval)
                {
                    prevSoundTime = Time.time;

                    pitchMultiplier *= 1.02f;
                    AudioController.PlaySound(AudioController.Sounds.bounceSpawn, 1, pitchMultiplier);

                    Vibration.Vibrate(10);
                }
            });

            FilledAmount++;

            return RequiredItemsAmount == FilledAmount;
        }

        public Vector3 GetRandomPosInCart()
        {
            return cartCollider.bounds.GetRandomPosition(cartCollider.transform.rotation, -0.9f);
        }

        public void GetHappy()
        {
            fillbarFade.DOFade(0f, 0.3f);

            animator.SetTrigger(HAPPY_TRIGGER);
        }

        public void GetSad()
        {
            fillbarFade.DOFade(0f, 0.3f);

            animator.SetTrigger(SAD_TRIGGER);
        }
    }
}