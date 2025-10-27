using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Watermelon.HoleMarket3D
{
    public class PropBehavior : MonoBehaviour
    {
        public Prop Prop { get; private set; }
        public PropPlacement Data { get; private set; }

        private Rigidbody Rigidbody { get; set; }

        private List<Collider> colliders = new List<Collider>();

        public bool IsActive { get; private set; }
        public bool IsEaten { get; private set; }

        [SerializeField] MeshRenderer meshRenderer;

        public delegate void PropBehaviorDelegate(PropBehavior prop);
        private PropBehaviorDelegate OnEatenCallback;

        private static float lastVibrated = 0;

        private void Awake()
        {
            Rigidbody = GetComponent<Rigidbody>();

            colliders = new List<Collider>(GetComponentsInChildren<Collider>(false));
            
            if(TryGetComponent<Collider>(out var ownCollider))
            {
                colliders.Add(ownCollider);
            }
        }

        public void Init(Prop prop, PropPlacement data)
        {
            Prop = prop;
            Data = data;

            ResetPosition();
        }

        public void SetOnEatenCallback(PropBehaviorDelegate callback)
        {
            OnEatenCallback = callback;
        }

        public void ResetPosition()
        {
            transform.position = Data.Position;
            transform.eulerAngles = Data.Rotation;

            Rigidbody.isKinematic = true;
            Rigidbody.Sleep();
            
            for(int i = 0; i < colliders.Count; i++)
            {
                colliders[i].enabled = false;
            }

            IsActive = false;
            IsEaten = false;
        }

        public void Shrink()
        {
            var size = meshRenderer.bounds.size;

            var biggerSize = size.x;
            if(size.y > biggerSize) biggerSize = size.y;
            if(size.z > biggerSize) biggerSize = size.z;

            transform.localScale = Vector3.one / biggerSize;
        }

        private void Update()
        {
            if (IsEaten) return;
            if(!LevelController.IsGamePlayActive) return;

            if(!IsActive)
            {
                if (PlayerBehavior.DistanceSqr(transform) < PlayerBehavior.RadiusSqr * 1.1f)
                {
                    IsActive = true;

                    for (int i = 0; i < colliders.Count; i++)
                    {
                        colliders[i].enabled = true;
                    }

                    Rigidbody.isKinematic = false;
                    Rigidbody.WakeUp();
                }
            } else
            {
                if(transform.position.y < -0.75f)
                {
                    StartCoroutine(EatenCoroutine());
                }
            }
        }

        private IEnumerator EatenCoroutine()
        {
            colliders.ForEach((collider) => collider.enabled = false);
            OnEatenCallback?.Invoke(this);

            IsEaten = true;

            if (Time.time + 0.02f > lastVibrated)
            {
                lastVibrated = Time.time;
                Vibration.Vibrate(10);

                AudioController.PlaySound(AudioController.Sounds.bounce, 1, 1.8f);
            }

            yield return new WaitForSeconds(1f);

            gameObject.SetActive(false);
            colliders.ForEach((collider) => collider.enabled = true);
        }
    }
}