using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using Watermelon.HoleMarket3D;

namespace Watermelon
{
    public class Timer : MonoBehaviour
    {
        private static Timer instance;
        public static Timer GetTimer() => instance;

        [SerializeField] RectTransform rect;

        [Space]
        [SerializeField] TMP_Text timerText;

        [Space]
        [SerializeField] float timerScaleStart;
        [SerializeField] AnimationCurve timerScaleCurve;
        [SerializeField] AnimationCurve timerCurveMultiplier;

        [Space]
        [SerializeField] Gradient timerColorGradient;

        public event SimpleCallback OnTimerCompleted;

        private TimerUpgrade timerUpgrade;
        private TimerUpgrade.TimerUpgradeStage timerUpgradeStage;

        private Coroutine timerScaleCoroutine;

        public float Time { get; private set; }
        public bool IsPaused { get; private set; }

        private void Awake()
        {
            instance = this;
        }

        public void Initialise()
        {
            timerUpgrade = UpgradesController.GetUpgrade<TimerUpgrade>(UpgradeType.Timer);

            timerUpgrade.OnUpgraded += OnTimerUpgraded;

            timerUpgradeStage = timerUpgrade.GetCurrentStage();

            Time = timerUpgradeStage.Seconds;

            timerText.text = string.Format("{0:ss}s", TimeSpan.FromSeconds(Time));

            IsPaused = true;
        }

        private void OnTimerUpgraded()
        {
            timerUpgradeStage = timerUpgrade.GetCurrentStage();

            Time = timerUpgradeStage.Seconds;

            timerText.text = string.Format("{0:ss}s", TimeSpan.FromSeconds(Time));
        }

        private void Update()
        {
            if (LevelController.IsGamePlayActive && !IsPaused)
            {
                Time -= UnityEngine.Time.deltaTime;

                if(Time < 0)
                {
                    IsPaused = true;

                    if(timerScaleCoroutine != null)
                    {
                        StopCoroutine(timerScaleCoroutine);
                        timerScaleCoroutine = null;
                    }

                    timerText.text = string.Format("{0:ss}s", TimeSpan.FromSeconds(0));

                    timerText.transform.DOScale(1.5f, 0.5f).SetEasing(Ease.Type.SineInOut);

                    OnTimerCompleted?.Invoke();
                } else
                {
                    UpdateTimerText();
                }
            }
        }

        public void UpdateTimerText()
        {
            timerText.text = string.Format("{0:ss}s", TimeSpan.FromSeconds(Time + 1));

            if (Time <= timerScaleStart)
            {
                if (timerScaleCoroutine == null) timerScaleCoroutine = StartCoroutine(TimerScaleCoroutine());

                timerText.color = timerColorGradient.Evaluate((timerScaleStart - Time) / timerScaleStart);
            }
        }

        private IEnumerator TimerScaleCoroutine()
        {
            float time = 0;
            float duration = 1;

            while (true)
            {
                yield return null;
                time += UnityEngine.Time.deltaTime;
                time %= duration;

                var t = time / duration;

                timerText.transform.localScale = Vector3.one * Mathf.Lerp(timerScaleCurve.Evaluate(t), 1, timerCurveMultiplier.Evaluate(Time / timerScaleStart));
            }
        }

        public void ResetTimer(bool withUpgrade = false)
        {
            if (withUpgrade) timerUpgrade.ResetUpgrade();

            Time = timerUpgradeStage.Seconds;
            IsPaused = true;
        }

        public void AddTime(float value)
        {
            Time += value;
        }

        public void Pause()
        {
            IsPaused = true;
        }

        public void Play()
        {
            IsPaused = false;

            if (timerScaleCoroutine == null) timerText.transform.localScale = Vector3.one;

            if (Time <= timerScaleStart)
            { 
                timerText.color = timerColorGradient.Evaluate((timerScaleStart - Time) / timerScaleStart);
            } else
            {
                timerText.color = timerColorGradient.Evaluate(0);
            }
        }

        public void Show()
        {
            rect.anchoredPosition = new Vector2(-150, -400);
            rect.DOAnchoredPosition(new Vector2(60, -400), 0.2f).SetEasing(Ease.Type.SineOut);

            if (timerScaleCoroutine == null) timerText.transform.localScale = Vector3.one;
            if (Time <= timerScaleStart)
            {
                timerText.color = timerColorGradient.Evaluate((timerScaleStart - Time) / timerScaleStart);
            }
            else
            {
                timerText.color = timerColorGradient.Evaluate(0);
            }

            UpdateTimerText();
        }

        public void Hide()
        {
            rect.DOAnchoredPosition(new Vector2(-150, -400), 0.2f).SetEasing(Ease.Type.SineIn);
        }
        
    }
}