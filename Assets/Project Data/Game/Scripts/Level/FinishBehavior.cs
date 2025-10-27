using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Watermelon.HoleMarket3D
{
    public class FinishBehavior : MonoBehaviour
    {
        [SerializeField] List<CustomerBehavior> customers;
        [SerializeField] List<Transform> prop;

        private List<PropBehavior> collectedProp;
        private int maxCount;

        private bool IsFilling { get; set; } = false;

        private UIFinish uiFinish;

        int index = 0;
        float lastSpawn = 0;
        float spawnInterval = 0.1f;

        int customerId = 0;

        public void Spawn(List<PropBehavior> collectedProp, int maxCost)
        {
            this.collectedProp = collectedProp;
            this.maxCount = maxCost;

            int costPerCustomer = maxCost / 3;

            for (int i = 0; i < customers.Count; i++)
            {
                if (i != customers.Count - 1)
                {
                    customers[i].Spawn(costPerCustomer);
                } else
                {
                    customers[i].Spawn(costPerCustomer + maxCost - costPerCustomer * customers.Count);
                }
            }

            for (int i = 0; i < prop.Count; i++)
            {
                var item = prop[i];

                var scale = item.transform.localScale;

                item.transform.localScale = Vector3.zero;
                item.DOScale(scale, 0.4f, Random.value * 0.2f).SetEasing(Ease.Type.SineOut);
            }

            uiFinish = UIController.GetPage<UIFinish>();
            uiFinish.InjectFinish(this);
            uiFinish.Progress = 0;

            IsFilling = false;

            customerId = 0;

            spawnInterval = collectedProp.Count < 20 ? 0.1f : collectedProp.Count < 50 ? 0.06f : 0.03f;

            if(collectedProp.Count == 0)
            {
                Tween.DelayedCall(1f, () =>
                {
                    for (int i = customerId; i < customers.Count; i++)
                    {
                        customers[i].GetSad();
                    }

                    Tween.DelayedCall(1f, () =>
                    {
                        UIController.HidePage<UIFinish>();
                        var uiComplete = UIController.GetPage<UIComplete>();

                        uiComplete.SetReward(Currency.Type.Money, 1);

                        UIController.ShowPage<UIComplete>();
                    });
                });
            }
        }

        private void Update()
        {
            if (!IsFilling) return;

            while(Time.time >= lastSpawn && index < collectedProp.Count)
            {
                lastSpawn = Time.time + spawnInterval;

                var flyingProp = collectedProp[index++];

                flyingProp.gameObject.SetActive(true);

                flyingProp.Shrink();
                flyingProp.ResetPosition();

                flyingProp.transform.rotation = Quaternion.Euler(Random.value * 360, Random.value * 360, Random.value * 360);
                flyingProp.transform.position = Vector3.down * 2;

                uiFinish.Progress = index / (float)maxCount;

                var customer = customers[customerId];

                if (customer.AddItemToCart(flyingProp))
                {
                    customerId++;
                }

                if(index == collectedProp.Count)
                {
                    for(int i = customerId; i < customers.Count; i++)
                    {
                        customers[i].GetSad();
                    }

                    Tween.DelayedCall(1f, () =>
                    {
                        UIController.HidePage<UIFinish>();
                        var uiComplete = UIController.GetPage<UIComplete>();

                        if(collectedProp.Count == maxCount)
                        {
                            uiComplete.SetReward(Currency.Type.Gems, 3);
                        } else
                        {
                            uiComplete.SetReward(Currency.Type.Money, CurrenciesController.Get(Currency.Type.TempMoney));
                        }

                        UIController.ShowPage<UIComplete>();
                    });
                }
            }
        }

        public void HideAndDestroy()
        {
            for(int i = 0; i < customers.Count; i++) 
            { 
                var customer = customers[i];

                customer.DOScale(0, 0.2f, 0.1f).SetEasing(Ease.Type.SineIn);
            }

            for(int i = 0; i < prop.Count; i++)
            {
                var item = prop[i];

                item.DOScale(0, 0.2f, 0.1f).SetEasing(Ease.Type.SineIn);
            }

            Destroy(gameObject);
        }

        public void StartFilling()
        {
            IsFilling = true;
        }

        public void StopFilling()
        {
            IsFilling = false;
        }
    }
}