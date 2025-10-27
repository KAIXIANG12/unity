using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Watermelon.HoleMarket3D
{
    public class PlayerGraphics : MonoBehaviour
    {
        [SerializeField] Gradient holeGradient;

        [SerializeField] Image energyFill;

        public float Energy { get => energyFill.fillAmount; set => energyFill.fillAmount = value; }
        public bool IsEnergyActive { get => energyFill.enabled; set => energyFill.enabled = value; }

        public void Init()
        {
            var texture = new Texture2D(1, 100);

            texture.wrapMode = TextureWrapMode.Clamp;

            Color32[] colors = new Color32[100];

            for(int i = 0; i < 100; i++)
            {
                colors[i] = holeGradient.Evaluate(i / 99f);
            }

            texture.SetPixels32(colors);
            texture.Apply();

            VoidMesh.SetTexture(texture);

            PlayerBehavior.OnRadiusChanged += OnRadiusChanged;
            OnRadiusChanged(PlayerBehavior.Radius);
        }

        private void OnRadiusChanged(float value)
        {
            transform.localScale = Vector3.one * value * 0.98f;
        }

        private void OnDestroy()
        {
            PlayerBehavior.OnRadiusChanged -= OnRadiusChanged;
        }
    }
}