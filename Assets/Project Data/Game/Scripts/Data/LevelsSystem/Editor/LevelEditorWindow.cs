#pragma warning disable 649

using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using System;
using Watermelon;
using System.Text;
using Watermelon.HoleMarket3D;
using Watermelon.List;

public class LevelEditorWindow : LevelEditorBase
{

    
    //Path variables need to be changed ----------------------------------------
    private const string GAME_SCENE_PATH = "Assets/Project Data/Game/Scenes/Game.unity";
    private const string EDITOR_SCENE_PATH = "Assets/Project Data/Game/Scenes/LevelEditor.unity";
    private static string EDITOR_SCENE_NAME = "LevelEditor";

    //Window configuration
    private const string TITLE = "Level Editor";
    private const float WINDOW_MIN_WIDTH = 600;
    private const float WINDOW_MIN_HEIGHT = 560;
    private const float WINDOW_MAX_WIDTH = 800;
    private const float WINDOW_MAX_HEIGHT = 700;

    //Level database fields
    private const string LEVELS_PROPERTY_NAME = "levels";
    private const string PROPS_PROPERTY_NAME = "props";
    private const string THEMES_PROPERTY_NAME = "themes";
    private SerializedProperty levelsSerializedProperty;
    private SerializedProperty propsSerializedProperty;
    private SerializedProperty themesSerializedProperty;

    //EnumObjectsList 
    private const string TYPE_PROPERTY_PATH = "type";
    private const string PREFAB_PROPERTY_PATH = "prefab";
    private const string SPAWN_POSITION_PROPERTY_PATH = "spawnPosition";
    private bool enumCompiling;

    //TabHandler
    private TabHandler tabHandler;
    private const string LEVELS_TAB_NAME = "Levels";
    private const string PROPS_TAB_NAME = "Props";
    private const string THEMES_TAB_NAME = "Themes";

    //sidebar
    private LevelsHandler levelsHandler;
    private LevelRepresentation selectedLevelRepresentation;
    private const int SIDEBAR_WIDTH = 240;
    private const string OPEN_GAME_SCENE_LABEL = "Open \"Game\" scene";

    private const string OPEN_GAME_SCENE_WARNING = "Please make sure you saved changes before swiching scene. Are you ready to proceed?";
    private const string REMOVE_SELECTION = "Remove selection";
    private const string SELECT_SPAWNER_TOOL = "Select spawner tool";

    //ItemSave
    private const string POSITION_PROPERTY_PATH = "position";
    private const string ROTATION_PROPERTY_PATH = "rotation";
    private const string PROP_ID_PROPERTY_PATH = "propId";

    //General
    private const string YES = "Yes";
    private const string CANCEL = "Cancel";
    private const string WARNING_TITLE = "Warning";
    private const string BRACKET = "\"";
    private const string QUESTION_MARK = "?";
    private SerializedProperty tempProperty;
    private UnityEngine.Object tempObjectRef;
    private string tempPropertyLabel;

    //rest of levels tab
    private const string ITEMS_LABEL = "Spawn items:";
    private const string FILE = "file:";
    private const string OBJECT_MANAGEMENT = "Object management:";
    private const string CLEAR_SCENE = "Clear scene";
    private const string SAVE = "Save";
    private const string LOAD = "Load";
    private const string REMOVE = "Remove";
    private const string COMPILING = "Compiling...";
    private const string ITEM_UNASSIGNED_ERROR = "Please assign prefab to this item in \"Items\"  tab.";
    private const string ITEM_ASSIGNED = "This buttton spawns item.";
    private const string SELECT_LEVEL = "Set this level as current level";
    private const string UNKNOWN = "???";
    private const string SEPARATOR = " | ";


    private const float ITEMS_BUTTON_MAX_WIDTH = 120;
    private const float ITEMS_BUTTON_SPACE = 8;
    private const float ITEMS_BUTTON_WIDTH = 80;
    private const float ITEMS_BUTTON_HEIGHT = 80;

    private bool prefabAssigned;
    private GUIContent itemContent;
    private SerializedProperty currentLevelItemProperty;
    private Vector2 levelItemsScrollVector;
    private float itemPosX;
    private float itemPosY;
    private Rect itemsRect;
    private Rect selectedLevelFieldRect;
    private Rect itemRect;
    private int itemsPerRow;
    private int rowCount;
    private string[] themesNames;
    private CustomList propsList;
    private string elementNamePart1;
    private string elementNamePart2;
    private Rect workRect;

    protected override WindowConfiguration SetUpWindowConfiguration(WindowConfiguration.Builder builder)
    {
        builder.KeepWindowOpenOnScriptReload(true);
        builder.SetWindowMinSize(new Vector2(WINDOW_MIN_WIDTH, WINDOW_MIN_HEIGHT));
        builder.SetContentMaxSize(new Vector2(WINDOW_MAX_WIDTH, WINDOW_MAX_HEIGHT));
        builder.SetWindowMaxSize(new Vector2(WINDOW_MAX_WIDTH, WINDOW_MAX_HEIGHT));
        return builder.Build();
    }

    protected override Type GetLevelsDatabaseType()
    {
        return typeof(LevelDatabase);
    }

    public override Type GetLevelType()
    {
        return typeof(Level);
    }

    protected override void ReadLevelDatabaseFields()
    {
        levelsSerializedProperty = levelsDatabaseSerializedObject.FindProperty(LEVELS_PROPERTY_NAME);
        propsSerializedProperty = levelsDatabaseSerializedObject.FindProperty(PROPS_PROPERTY_NAME);
        themesSerializedProperty = levelsDatabaseSerializedObject.FindProperty(THEMES_PROPERTY_NAME);
    }

    

    protected override void InitialiseVariables()
    {
        enumCompiling = false;
        UpdateThemes();
        levelsHandler = new LevelsHandler(levelsDatabaseSerializedObject, levelsSerializedProperty);
        InitPropsList();
        tabHandler = new TabHandler();
        tabHandler.AddTab(new TabHandler.Tab(LEVELS_TAB_NAME, DisplayLevelsTab, UpdateThemes));
        tabHandler.AddTab(new TabHandler.Tab(PROPS_TAB_NAME, DisplayPropsTab, UpdateThemes));
        tabHandler.AddTab(new TabHandler.Tab(THEMES_TAB_NAME, DisplayThemesTab));
        PrefsSettings.InitEditor();
    }



    private void UpdateThemes()
    {
        themesNames = new string[themesSerializedProperty.arraySize];

        for (int i = 0; i < themesNames.Length; i++)
        {
            themesNames[i] = themesSerializedProperty.GetArrayElementAtIndex(i).stringValue;
        }
    }

    protected override void Styles()
    {
        if (tabHandler != null)
        {
            tabHandler.SetDefaultToolbarStyle();
        }
    }

    public override void OpenLevel(UnityEngine.Object levelObject, int index)
    {
        SaveLevelIfPosssible();
        selectedLevelRepresentation = new LevelRepresentation(levelObject);
        levelsHandler.UpdateCurrentLevelLabel(GetLevelLabel(levelObject, index));
        LoadLevelItems();
    }

    public override string GetLevelLabel(UnityEngine.Object levelObject, int index)
    {
        LevelRepresentation levelRepresentation = new LevelRepresentation(levelObject);
        return levelRepresentation.GetLevelLabel(index, stringBuilder);
    }

    public override void ClearLevel(UnityEngine.Object levelObject)
    {
        LevelRepresentation levelRepresentation = new LevelRepresentation(levelObject);
        levelRepresentation.Clear();
    }

    protected override void DrawContent()
    {
        if (EditorSceneManager.GetActiveScene().name != EDITOR_SCENE_NAME)
        {
            DrawOpenEditorScene();
            return;
        }

        if (enumCompiling)
        {
            EditorGUILayout.LabelField(COMPILING,EditorStylesExtended.label_large_bold);
            return;
        }

        tabHandler.DisplayTab();
    }

    private void DrawOpenEditorScene()
    {
        EditorGUILayout.BeginVertical();
        EditorGUILayout.HelpBox(EDITOR_SCENE_NAME + " scene required for level editor.", MessageType.Error, true);

        if (GUILayout.Button("Open \""+ EDITOR_SCENE_NAME + "\" scene"))
        {
            OpenScene(EDITOR_SCENE_PATH);
        }

        EditorGUILayout.EndVertical();
    }

    private void DisplayLevelsTab()
    {
        EditorGUILayout.BeginHorizontal();
        //sidebar 
        EditorGUILayout.BeginVertical(GUI.skin.box, GUILayout.MaxWidth(SIDEBAR_WIDTH));
        levelsHandler.DisplayReordableList();
        DisplaySidebarButtons();
        EditorGUILayout.EndVertical();

        GUILayout.Space(8);

        //level content
        EditorGUILayout.BeginVertical(GUI.skin.box);
        DisplaySelectedLevel();
        EditorGUILayout.EndVertical();
        
        EditorGUILayout.EndHorizontal();
    }

    private void DisplaySidebarButtons()
    {
        if (GUILayout.Button("Rename Levels", EditorStylesExtended.button_01))
        {
            SaveLevelIfPosssible();
            levelsHandler.RenameLevels();
        }

        if (GUILayout.Button(OPEN_GAME_SCENE_LABEL, EditorStylesExtended.button_01))
        {
            if (EditorUtility.DisplayDialog(WARNING_TITLE, OPEN_GAME_SCENE_WARNING, YES, CANCEL))
            {
                SaveLevelIfPosssible();
                OpenScene(GAME_SCENE_PATH);
            }
        }

        if (GUILayout.Button(REMOVE_SELECTION, EditorStylesExtended.button_01))
        {
            SaveLevelIfPosssible();
            levelsHandler.ClearSelection();
            ClearScene();
        }
    }

    private static void ClearScene()
    {
        EditorSceneController.Instance.Clear();
    }

    private void SetAsCurrentLevel()
    {
        //PrefsSettings.SetInt(PrefsSettings.Key.LevelId, levelsHandler.SelectedLevelIndex); 

        EditorSetLevel(levelsHandler.SelectedLevelIndex);
        throw new NotImplementedException();
    }

    public static void EditorSetLevel(int levelIndex)
    {
        //GlobalSave tempSave = SaveController.GetGlobalSave();

        //SimpleIntSave levelIndexSave = tempSave.GetSaveObject<SimpleIntSave>("Level Number");
        //levelIndexSave.Value = levelIndex;

        //SaveController.ForceSave();
    }

    private void DisplaySelectedLevel()
    {
        if (levelsHandler.SelectedLevelIndex == -1)
        {
            return;
        }

        //handle level file field
        EditorGUI.BeginChangeCheck();
        //easy way to know width of available space
        selectedLevelFieldRect = EditorGUILayout.BeginVertical(GUILayout.ExpandWidth(true));
        EditorGUILayout.PropertyField(levelsHandler.SelectedLevelProperty, new GUIContent(FILE));
        EditorGUILayout.EndVertical();


        if (EditorGUI.EndChangeCheck())
        {
            levelsHandler.ReopenLevel();
        }

        if (selectedLevelRepresentation.NullLevel)
        {
            return;
        }

        EditorGUILayout.Space();

        if (GUILayout.Button(SELECT_LEVEL, EditorStylesExtended.button_01))
        {
            SetAsCurrentLevel();
        }

        if (GUILayout.Button("test level", EditorStylesExtended.button_01))
        {
            SaveLevelItems();
            SetAsCurrentLevel();
            OpenScene(GAME_SCENE_PATH);
            EditorApplication.ExecuteMenuItem("Edit/Play");
        }

        DisplayItemFields();
        EditorGUILayout.Space();
        DisplayItemsListSection();
        EditorGUILayout.Space();
    }

    private void DisplayItemFields()
    {
        selectedLevelRepresentation.themeIdProperty.intValue = EditorGUILayout.Popup("Theme", selectedLevelRepresentation.themeIdProperty.intValue, themesNames);

        EditorGUILayout.PropertyField(selectedLevelRepresentation.sizeProperty);
        EditorGUILayout.PropertyField(selectedLevelRepresentation.groundMaterialProperty);
        EditorGUILayout.PropertyField(selectedLevelRepresentation.voidMaterialProperty);
        EditorGUILayout.PropertyField(selectedLevelRepresentation.finishPrefabProperty);
    }

    private void DisplayItemsListSection()
    {
        EditorGUILayout.LabelField(ITEMS_LABEL);
        levelItemsScrollVector = EditorGUILayout.BeginScrollView(levelItemsScrollVector);

        itemsRect = EditorGUILayout.BeginVertical();
        itemPosX = itemsRect.x;
        itemPosY = itemsRect.y;

        //assigning space
        if (propsSerializedProperty.arraySize != 0)
        {
            itemsPerRow = Mathf.FloorToInt((Screen.width - SIDEBAR_WIDTH - 20) / (ITEMS_BUTTON_SPACE + ITEMS_BUTTON_WIDTH));
            rowCount = Mathf.CeilToInt((propsSerializedProperty.arraySize * 1f) / itemsPerRow);
            GUILayout.Space(rowCount * (ITEMS_BUTTON_SPACE + ITEMS_BUTTON_HEIGHT));
        }

        for (int i = 0; i < propsSerializedProperty.arraySize; i++)
        {
            tempProperty = propsSerializedProperty.GetArrayElementAtIndex(i);

            if(tempProperty.FindPropertyRelative("themeId").intValue != selectedLevelRepresentation.themeIdProperty.intValue)
            {
                continue;
            }


            tempObjectRef = tempProperty.FindPropertyRelative(PREFAB_PROPERTY_PATH).objectReferenceValue;
            prefabAssigned = (tempObjectRef != null);

            if (prefabAssigned)
            {
                itemContent = new GUIContent(AssetPreview.GetAssetPreview(tempObjectRef), ITEM_ASSIGNED);
            }
            else
            {
                itemContent = new GUIContent(EditorGUIUtility.whiteTexture, ITEM_UNASSIGNED_ERROR);
            }

            //check if need to start new row
            if (itemPosX + ITEMS_BUTTON_SPACE + ITEMS_BUTTON_WIDTH > selectedLevelFieldRect.width - 10)
            {
                itemPosX = itemsRect.x;
                itemPosY = itemPosY + ITEMS_BUTTON_HEIGHT + ITEMS_BUTTON_SPACE;
            }

            itemRect = new Rect(itemPosX, itemPosY, ITEMS_BUTTON_WIDTH, ITEMS_BUTTON_HEIGHT);

            EditorGUI.BeginDisabledGroup(!prefabAssigned);

            if (GUI.Button(itemRect, itemContent, EditorStylesExtended.button_01))
            {
                if (SceneView.lastActiveSceneView.in2DMode)
                {
                    EditorSceneController.Instance.Spawn((GameObject)tempProperty.FindPropertyRelative(PREFAB_PROPERTY_PATH).objectReferenceValue, tempProperty.FindPropertyRelative(SPAWN_POSITION_PROPERTY_PATH).vector3Value.SetX(SceneView.lastActiveSceneView.camera.transform.position.x), i);
                }
                else
                {
                    EditorSceneController.Instance.Spawn((GameObject)tempProperty.FindPropertyRelative(PREFAB_PROPERTY_PATH).objectReferenceValue, tempProperty.FindPropertyRelative(SPAWN_POSITION_PROPERTY_PATH).vector3Value, i);
                }
            }

            EditorGUI.EndDisabledGroup();

            itemPosX += ITEMS_BUTTON_SPACE + ITEMS_BUTTON_WIDTH;
        }


        EditorGUILayout.EndVertical();
        EditorGUILayout.EndScrollView();
    }

    private void InitPropsList()
    {
        workRect = new Rect();
        propsList = new CustomList(levelsDatabaseSerializedObject, propsSerializedProperty, PropsLabelCallback);
        propsList.addElementCallback = AddPropCallback;
        propsList.removeElementCallback = RemovePropCallback;
        propsList.AddSpace(4);
        propsList.AddCustomField(DrawThemeId, GetHeightThemeID);
        propsList.AddSpace(4);
        propsList.AddPropertyField("prefab");
        propsList.AddSpace(4);
        propsList.AddPropertyField("cost");
        propsList.AddSpace(4);
        propsList.AddPropertyField("spawnPosition");
    }

    

    private string PropsLabelCallback(SerializedProperty elementProperty, int elementIndex)
    {
        try
        {
            elementNamePart1 = themesNames[elementProperty.FindPropertyRelative("themeId").intValue];
        }
        catch
        {
            elementNamePart1 = UNKNOWN;
        }

        try
        {
            elementNamePart2 = elementProperty.FindPropertyRelative("prefab").objectReferenceValue.name;
        }
        catch
        {
            elementNamePart2 = UNKNOWN;
        }

        return elementNamePart1 + SEPARATOR + elementNamePart2;
    }

    private void AddPropCallback()
    {
        propsSerializedProperty.arraySize++;
        SerializedProperty newProperty = propsSerializedProperty.GetArrayElementAtIndex(propsSerializedProperty.arraySize - 1);
        propsList.ClearProperty(newProperty);
        newProperty.isExpanded = false;
    }

    private void RemovePropCallback()
    {
        propsSerializedProperty.DeleteArrayElementAtIndex(propsList.SelectedIndex);
        propsList.SelectedIndex = -1;
    }
    

    private void DrawThemeId(SerializedProperty elementProperty, Rect rect, CustomListStyle style)
    {
        SerializedProperty themeIdProperty = elementProperty.FindPropertyRelative("themeId");
        workRect.Set(rect.x, rect.y, EditorGUIUtility.labelWidth, rect.height);
        EditorGUI.LabelField(workRect, "Theme");
        workRect.xMax = rect.xMax;
        workRect.xMin = rect.xMin + EditorGUIUtility.labelWidth + 1;
        themeIdProperty.intValue = EditorGUI.Popup(workRect, themeIdProperty.intValue, themesNames);
    }

    private float GetHeightThemeID(SerializedProperty elementProperty, CustomListStyle style)
    {
        return EditorGUIUtility.singleLineHeight;
    }

    private void DisplayPropsTab()
    {
        propsList.Display();
    }

    private void DisplayThemesTab()
    {
        EditorGUILayout.PropertyField(themesSerializedProperty);
    }

    private void LoadLevelItems()
    {
        EditorSceneController.Instance.Clear();
        PropPlacement tempPropPlacement;

        for (int i = 0; i < selectedLevelRepresentation.propsProperty.arraySize; i++)
        {
            tempPropPlacement = PropertyToPropPlacement(i);
            EditorSceneController.Instance.Spawn(tempPropPlacement, (GameObject)propsSerializedProperty.GetArrayElementAtIndex(tempPropPlacement.PropId).FindPropertyRelative(PREFAB_PROPERTY_PATH).objectReferenceValue);
        }
    }

    private void SaveLevelItems()
    {
        PropPlacement[] levelItems = EditorSceneController.Instance.GetLevelItems();
        selectedLevelRepresentation.propsProperty.arraySize = levelItems.Length;

        for (int i = 0; i < levelItems.Length; i++)
        {
            PropPlacementToProperty(levelItems[i], i);
        }

        selectedLevelRepresentation.ApplyChanges();
        levelsHandler.UpdateCurrentLevelLabel(selectedLevelRepresentation.GetLevelLabel(levelsHandler.SelectedLevelIndex, stringBuilder));
        AssetDatabase.SaveAssets();
    }

    private void PropPlacementToProperty(PropPlacement levelItem, int index)
    {
        currentLevelItemProperty = selectedLevelRepresentation.propsProperty.GetArrayElementAtIndex(index);
        currentLevelItemProperty.FindPropertyRelative(PROP_ID_PROPERTY_PATH).intValue = levelItem.PropId;
        currentLevelItemProperty.FindPropertyRelative(POSITION_PROPERTY_PATH).vector3Value = levelItem.Position;
        currentLevelItemProperty.FindPropertyRelative(ROTATION_PROPERTY_PATH).vector3Value = levelItem.Rotation;
    }

    private PropPlacement PropertyToPropPlacement(int index)
    {
        currentLevelItemProperty = selectedLevelRepresentation.propsProperty.GetArrayElementAtIndex(index);
        return new PropPlacement(
            currentLevelItemProperty.FindPropertyRelative(PROP_ID_PROPERTY_PATH).intValue,
            currentLevelItemProperty.FindPropertyRelative(POSITION_PROPERTY_PATH).vector3Value,
            currentLevelItemProperty.FindPropertyRelative(ROTATION_PROPERTY_PATH).vector3Value);
    }

    private void SaveLevelIfPosssible()
    {
        if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name != EDITOR_SCENE_NAME)
        {
            return;
        }

        if (selectedLevelRepresentation == null)
        {
            return;
        }

        if (selectedLevelRepresentation.NullLevel)
        {
            return;
        }

        try
        {
            SaveLevelItems();
        }
        catch
        {

        }

        levelsHandler.SetLevelLabels();
    }

    private void OnDestroy()
    {
        SaveLevelIfPosssible();
    }

    protected class LevelRepresentation : LevelRepresentationBase
    {
        private const string THEME_ID_PROPERTY_NAME = "themeId";
        private const string SIZE_PROPERTY_NAME = "size";
        private const string GROUND_MATERIAL_PROPERTY_NAME = "groundMaterial";
        private const string VOID_MATERIAL_PROPERTY_NAME = "voidMaterial";
        private const string FINISH_PREFAB_PROPERTY_NAME = "finishPrefab";
        private const string PROPS_PROPERTY_NAME = "props";
        public SerializedProperty themeIdProperty;
        public SerializedProperty sizeProperty;
        public SerializedProperty groundMaterialProperty;
        public SerializedProperty voidMaterialProperty;
        public SerializedProperty finishPrefabProperty;
        public SerializedProperty propsProperty;

        //this empty constructor is nessesary
        public LevelRepresentation(UnityEngine.Object levelObject) : base(levelObject)
        {
        }


        protected override void ReadFields()
        {
            themeIdProperty = serializedLevelObject.FindProperty(THEME_ID_PROPERTY_NAME);
            sizeProperty = serializedLevelObject.FindProperty(SIZE_PROPERTY_NAME);
            groundMaterialProperty = serializedLevelObject.FindProperty(GROUND_MATERIAL_PROPERTY_NAME);
            voidMaterialProperty = serializedLevelObject.FindProperty(VOID_MATERIAL_PROPERTY_NAME);
            finishPrefabProperty = serializedLevelObject.FindProperty(FINISH_PREFAB_PROPERTY_NAME);
            propsProperty = serializedLevelObject.FindProperty(PROPS_PROPERTY_NAME);

        }

        public override void Clear()
        {
            if (!NullLevel)
            {
                themeIdProperty.intValue = 0;
                sizeProperty.vector2Value = new Vector2(100, 100);
                groundMaterialProperty.objectReferenceValue = null;
                voidMaterialProperty.objectReferenceValue = null;
                finishPrefabProperty.objectReferenceValue = null;
                propsProperty.arraySize = 0;
                ApplyChanges();
            }

        }

        public override string GetLevelLabel(int index, StringBuilder stringBuilder)
        {
            if (NullLevel)
            {
                return base.GetLevelLabel(index, stringBuilder);
            }
            else
            {
                return base.GetLevelLabel(index, stringBuilder) + SEPARATOR + propsProperty.arraySize;
            }            
        }
    }

    
}


// -----------------
// Scene interraction level editor V1.5
// -----------------

// Changelog
// v 1.4
// • Updated EnumObjectlist
// • Updated object preview
// v 1.4
// • Updated EnumObjectlist
// • Fixed bug with window size
// v 1.3
// • Updated EnumObjectlist
// • Added StartPointHandles script that can be added to gameobjects
// v 1.2
// • Reordered some methods
// v 1.1
// • Added spawner tool
// v 1 basic version works
