using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using Watermelon.HoleMarket3D;

namespace Watermelon
{
    public class UIPersistent : UIPage
    {
        [Header("Timer")]
        [SerializeField] Timer timer;
        [SerializeField] Joystick joystick;

        public override void Initialise()
        {
            timer.Initialise();

            joystick.Initialise(canvas);
        }

        public override void PlayHideAnimation()
        {
            
        }

        public override void PlayShowAnimation()
        {
            ShowTimer();
        }

        public void ShowTimer()
        {
            timer.Show();
        }

        public void HideTimer()
        {
            timer.Hide();
        }

        public void UpdateTimer()
        {

        }
    }
}