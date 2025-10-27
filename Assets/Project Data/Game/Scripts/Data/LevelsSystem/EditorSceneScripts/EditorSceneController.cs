#pragma warning disable 649

using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Watermelon.HoleMarket3D;

namespace Watermelon
{
    public class EditorSceneController : MonoBehaviour
    {

#if UNITY_EDITOR
        private static EditorSceneController instance;
        public static EditorSceneController Instance { get => instance; }

        [SerializeField] private GameObject container;

        public EditorSceneController()
        {
            instance = this;
        }

        //used when user spawns objects by clicking on object name in level editor
        public void Spawn(GameObject prefab, Vector3 defaultPosition, int propId)
        {
            GameObject gameObject = Instantiate(prefab, defaultPosition, Quaternion.identity, container.transform);
            gameObject.name = prefab.name + " ( Child # " + container.transform.childCount + ")";
            gameObject.AddComponent<SavableItem>().PropId = propId;
            SelectGameObject(gameObject);
        }

        //used when level loads in level editor
        public void Spawn(PropPlacement propPlacement, GameObject prefab)
        {
            GameObject gameObject = Instantiate(prefab, propPlacement.Position, Quaternion.Euler(propPlacement.Rotation), container.transform);
            gameObject.name = prefab.name + "(el # " + container.transform.childCount + ")";
            gameObject.AddComponent<SavableItem>().PropId = propPlacement.PropId;
            SelectGameObject(gameObject);
        }

        public void SelectGameObject(GameObject selectedGameObject)
        {
            Selection.activeGameObject = selectedGameObject;
        }

        public void Clear()
        {
            for (int i = container.transform.childCount - 1; i >= 0; i--)
            {
                DestroyImmediate(container.transform.GetChild(i).gameObject);
            }
        }

        public PropPlacement[] GetLevelItems()
        {
            SavableItem[] savableItems = FindObjectsOfType<SavableItem>();
            List<PropPlacement> result = new List<PropPlacement>();

            for (int i = 0; i < savableItems.Length; i++)
            {
                result.Add(HandleParse(savableItems[i]));
            }

            return result.ToArray();
        }

        private PropPlacement HandleParse(SavableItem savableItem)
        {
            return new PropPlacement(savableItem.PropId, savableItem.gameObject.transform.position, savableItem.gameObject.transform.rotation.eulerAngles);
        }
#endif
    }
}
