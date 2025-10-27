using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Watermelon.HoleMarket3D
{
    public class UIFinish : UIPage, IPointerDownHandler, IPointerUpHandler
    {
        private FinishBehavior finish;

        [SerializeField] SlicedFilledImage progressFill;

        TweenCase progressCase;
        public float Progress { get => progressFill.fillAmount; 
            set
            {
                progressCase.KillActive();
                progressCase = progressFill.DOFillAmount(value, 0.1f);
            }
        }

        public override void Initialise()
        {
            
        }

        public void InjectFinish(FinishBehavior finish)
        {
            this.finish = finish;
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            finish.StartFilling();
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            finish.StopFilling();
        }

        public override void PlayHideAnimation()
        {
            UIController.OnPageClosed(this);
        }

        public override void PlayShowAnimation()
        {
            UIController.OnPageOpened(this);
        }
    }

}
