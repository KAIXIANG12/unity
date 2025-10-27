using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Watermelon
{
    partial class CameraController
    {
        private static TweenCase offsetCase;
        public static float OffsetMultiplier { get; private set; } = 1;
        public static void SetOffsetMultiplier(float value, float duration = 0, Ease.Type easing = Ease.Type.Linear)
        {
            if (duration <= 0)
            {
                ApplyOffsetMultiplier(value);
            }
            else
            {
                offsetCase.KillActive();
                offsetCase = Tween.DoFloat(OffsetMultiplier, value, duration, ApplyOffsetMultiplier).SetEasing(easing);
            }
        }

        private static void ApplyOffsetMultiplier(float value)
        {
            OffsetMultiplier = value;

            for (int i = 0; i < cameraController.virtualCameras.Length; i++)
            {
                var camera = cameraController.virtualCameras[i];
                camera.SetFollowOffsetMultiplier(value);
            }

            Physics.gravity = Vector3.down * 100 * OffsetMultiplier * OffsetMultiplier;
        }
    }

    partial class VirtualCameraCase
    {
        private CinemachineTransposer transposer;
        private Vector3 followOffset;

        public void Initialise()
        {
            transposer = VirtualCamera.GetCinemachineComponent<CinemachineTransposer>();
            followOffset = transposer.m_FollowOffset;
        }

        public void SetFollowOffsetMultiplier(float value)
        {
            transposer.m_FollowOffset = followOffset * value;
        }
    }
}
