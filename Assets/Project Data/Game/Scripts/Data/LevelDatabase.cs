using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Watermelon.HoleMarket3D
{
    [CreateAssetMenu(fileName = "Level Database", menuName = "Content/Level Database")]
    public class LevelDatabase : ScriptableObject
    {
        [SerializeField] Level[] levels;

        [SerializeField] string[] themes;

        [SerializeField] Prop[] props;

        public Level GetLevel(int levelId)
        {
            return levels[levelId % levels.Length];
        }

        public Prop GetProp(int propId)
        {
            if(propId >= 0 || propId < props.Length) return props[propId];

            return props[0];
        }
    }
}