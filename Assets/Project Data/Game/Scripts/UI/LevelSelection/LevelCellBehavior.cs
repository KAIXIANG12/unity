#pragma warning disable 0649

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Watermelon
{
    public class LevelCellBehavior : MonoBehaviour, GridItem
    {
        [SerializeField] Text levelNumber;
        [SerializeField] Image currentLevelIndicator;
        [SerializeField] Image lockImage;

        [Space]
        [SerializeField] Button button;

        private RectTransform rectTransform;

        private int levelId;
        public int LevelNumber {
            get => levelId;
            set {
                levelNumber.text = (value + 1).ToString();
                levelId = value;
            }
        }

        public bool IsSelected { set => currentLevelIndicator.enabled = value; }

        public bool IsOpened { 
            set
            {
                lockImage.enabled = !value;
                levelNumber.enabled = value;

                button.enabled = value;
            } 
        }

        private void Awake()
        {
            rectTransform = GetComponent<RectTransform>();
        }

        public RectTransform GetRectTransform()
        {
            //throw new System.NotImplementedException();

            return rectTransform;
        }

        public void InitGridItem(int id)
        {
            //throw new System.NotImplementedException();

            /*LevelNumber = id;
            IsSelected = id == GameController.CurrentLevelId;
            IsOpened = id <= GameController.MaxLevelReachedId;*/
        }

        public void OnClick()
        {
            

            UIController.HidePage<UILevelSelector>(); //LevelSelectionBehavior.Hide();

            //GameController.CurrentLevelId = LevelNumber;
            // GameController.ActualLevelId = LevelNumber;

            Tween.DelayedCall(0.4f, () => {
                //GameCanvasBehavior.LevelNumber = GameController.CurrentLevelId;
                UIController.HidePage<UIGame>();//GameCanvasBehavior.Show();

                //GameController.LoadLevel(LevelNumber);
            });
        }
    }
}

