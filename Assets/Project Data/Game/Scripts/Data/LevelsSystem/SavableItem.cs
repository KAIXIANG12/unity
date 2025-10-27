#pragma warning disable 649
using UnityEngine;

namespace Watermelon
{
    public class SavableItem : MonoBehaviour
    {
        [SerializeField] int propId;

        public int PropId { get => propId; set => propId = value; }
    }
}