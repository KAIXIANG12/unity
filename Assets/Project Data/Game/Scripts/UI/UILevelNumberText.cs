using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Watermelon.HoleMarket3D;

namespace Watermelon
{
    [RequireComponent(typeof(TextMeshProUGUI))]
    public class UILevelNumberText : MonoBehaviour
    {
        private const string LEVEL_LABEL = "LEVEL {0}";
        private static UILevelNumberText instance;

        [SerializeField] UIScalableObject uIScalableObject;

        private static UIScalableObject UIScalableObject => instance.uIScalableObject;
        private static TextMeshProUGUI levelNumberText;

        private static bool IsDisplayed = false;

        private void Awake()
        {
            instance = this;
            levelNumberText = GetComponent<TextMeshProUGUI>();
        }

        private void Start()
        {
            UpdateLevelNumber();
        }

        public static void Show(bool immediately = true)
        {
            if (IsDisplayed) return;

            IsDisplayed = true;

            levelNumberText.enabled = true;
            UIScalableObject.Show(immediately, scaleMultiplier: 1.05f);
        }

        public static void Hide(bool immediately = true)
        {
            if (!IsDisplayed) return;

            if (immediately) IsDisplayed = false;

            UIScalableObject.Hide(immediately, scaleMultiplier: 1.05f, onCompleted: delegate {

                IsDisplayed = false;
                levelNumberText.enabled = false;
            });
        }

        public static void UpdateLevelNumber()
        {
            levelNumberText.text = string.Format(LEVEL_LABEL, GameController.LevelId + 1);
        }

    }
}
