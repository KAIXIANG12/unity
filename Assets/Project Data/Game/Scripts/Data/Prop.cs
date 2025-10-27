using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Watermelon.HoleMarket3D
{
    [System.Serializable]
    public class Prop
    {
        [SerializeField] int themeId;
        public int ThemeId => themeId;

        [SerializeField] GameObject prefab;
        public GameObject Prefab => prefab;

        [SerializeField] int cost;
        public int Cost => cost;

        [SerializeField] private Vector3 spawnPosition; // default spawn position in level editor
    }

    [System.Serializable]
    public class PropPlacement
    {
        [SerializeField] int propId;
        public int PropId => propId;

        [SerializeField] Vector3 position;
        public Vector3 Position => position;

        [SerializeField] Vector3 rotation;
        public Vector3 Rotation => rotation;

        public PropPlacement()
        {
        }

        public PropPlacement(int propId, Vector3 position, Vector3 rotation)
        {
            this.propId = propId;
            this.position = position;
            this.rotation = rotation;
        }
    }
}