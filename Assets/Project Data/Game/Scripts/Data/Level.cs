using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Watermelon.HoleMarket3D
{
    [CreateAssetMenu(fileName = "Level Database", menuName = "Content/Level")]
    [System.Serializable]
    public class Level : ScriptableObject
    {
        [SerializeField] int themeId;
        public int ThemeId => themeId;

        [SerializeField] Vector2 size;
        public Vector2 Size => size;

        [SerializeField] Material groundMaterial;
        public Material GroundMaterial => groundMaterial;

        [SerializeField] Color groundMainColor;
        [SerializeField] Color groundAdditionalColor;

        public Color GroundMainColor => groundMainColor;
        public Color GroundAdditionalColor => groundAdditionalColor;

        [SerializeField] Material voidMaterial;
        public Material VoidMaterial => voidMaterial;

        [SerializeField] GameObject finishPrefab;
        public GameObject FinishPrefab => finishPrefab;

        [SerializeField] PropPlacement[] props;
        public PropPlacement[] Props => props;
    }
}