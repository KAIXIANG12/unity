using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Watermelon
{
    public class UILevelSelector : UIPage
    {       
        [SerializeField] VerticalGridScrollView gridScrollView;

        public override void Initialise()
        {

        }

        #region Show/Hide

        public override void PlayShowAnimation()
        {
            UIController.OnPageOpened(this);
        }

        public override void PlayHideAnimation()
        {
            UIController.OnPageClosed(this);
        }

        #endregion

        private void InitGrid()
        {           
            //gridScrollView.InitGrid(GameController.LevelDatabase.AmountOfLevels, GameController.MaxLevelReachedId);
        }

        #region Buttons

        public void CloseButton()
        {

        }

        #endregion
    }
}
