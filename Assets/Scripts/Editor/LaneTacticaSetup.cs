using UnityEngine;
using UnityEngine.UI;
using UnityEditor;

/// <summary>
/// LaneTactica UI 프리팹 자동 생성 에디터 도구
/// </summary>
public class LaneTacticaSetup : Editor
{
    [MenuItem("LaneTactica/Create Energy Generator Prefab")]
    public static void CreateEnergyGeneratorPrefab()
    {
        CreateFolderIfNotExists("Assets/Prefabs");

        // Cylinder 오브젝트 생성
        GameObject generator = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        generator.name = "Cylinder_EnergyGenerator";
        generator.transform.position = new Vector3(0f, 0f, 0f);
        generator.transform.localScale = new Vector3(1f, 0.5f, 1f);

        // CapsuleCollider → BoxCollider 교체
        Object.DestroyImmediate(generator.GetComponent<CapsuleCollider>());
        generator.AddComponent<BoxCollider>();

        // TowerBase + EnergyGenerator 컴포넌트 추가 (컴포지션)
        var tower = generator.AddComponent<TowerBase>();
        var gen = generator.AddComponent<EnergyGenerator>();

        // TowerBase 스탯 설정
        var towerSO = new SerializedObject(tower);
        towerSO.FindProperty("unitName").stringValue = "에너지 생성기";
        towerSO.FindProperty("maxHealth").intValue = 80;
        towerSO.FindProperty("attackDamage").intValue = 0;
        towerSO.FindProperty("attackSpeed").floatValue = 0;
        towerSO.FindProperty("attackRange").floatValue = 0;
        towerSO.FindProperty("attackType").enumValueIndex = 0; // None
        towerSO.FindProperty("showHealthBar").boolValue = true;
        towerSO.FindProperty("energyCost").intValue = 50;
        towerSO.FindProperty("cooldown").floatValue = 10f;
        towerSO.ApplyModifiedPropertiesWithoutUndo();

        // EnergyGenerator 스탯 설정
        var genSO = new SerializedObject(gen);
        genSO.FindProperty("energyPerGeneration").intValue = 25;
        genSO.FindProperty("generationInterval").floatValue = 5f;
        genSO.ApplyModifiedPropertiesWithoutUndo();

        // 프리팹 저장
        string prefabPath = "Assets/Prefabs/Cylinder_EnergyGenerator.prefab";
        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(generator, prefabPath);
        DestroyImmediate(generator);

        Selection.activeObject = prefab;
        EditorGUIUtility.PingObject(prefab);

        Debug.Log($"Energy Generator prefab created: {prefabPath}");
    }

    [MenuItem("LaneTactica/Create Tower Card Prefab")]
    public static void CreateTowerCardPrefab()
    {
        // 폴더 생성
        CreateFolderIfNotExists("Assets/Prefabs");
        CreateFolderIfNotExists("Assets/Prefabs/UI");

        // Tower Card 생성
        GameObject card = new GameObject("TowerCard");

        var cardRect = card.AddComponent<RectTransform>();
        cardRect.sizeDelta = new Vector2(80, 80);

        var cardImage = card.AddComponent<Image>();
        cardImage.color = new Color(0.3f, 0.3f, 0.3f, 1f);

        var button = card.AddComponent<Button>();
        var colors = button.colors;
        colors.normalColor = new Color(0.3f, 0.3f, 0.3f, 1f);
        colors.highlightedColor = new Color(0.4f, 0.4f, 0.4f, 1f);
        colors.pressedColor = new Color(0.2f, 0.5f, 0.2f, 1f);
        colors.selectedColor = new Color(0.2f, 0.6f, 0.2f, 1f);
        button.colors = colors;

        // Hotkey Text (상단)
        GameObject hotkeyObj = CreateTextObject("Hotkey", card.transform, "[1]", 12);
        var hotkeyRect = hotkeyObj.GetComponent<RectTransform>();
        hotkeyRect.anchorMin = new Vector2(0, 1);
        hotkeyRect.anchorMax = new Vector2(1, 1);
        hotkeyRect.pivot = new Vector2(0.5f, 1);
        hotkeyRect.sizeDelta = new Vector2(0, 20);
        hotkeyRect.anchoredPosition = new Vector2(0, -2);

        // Tower Name (중앙)
        GameObject nameObj = CreateTextObject("TowerName", card.transform, "Tower", 14);
        var nameRect = nameObj.GetComponent<RectTransform>();
        nameRect.anchorMin = new Vector2(0, 0.3f);
        nameRect.anchorMax = new Vector2(1, 0.7f);
        nameRect.sizeDelta = Vector2.zero;
        nameRect.anchoredPosition = Vector2.zero;

        // Cost Text (하단)
        GameObject costObj = CreateTextObject("Cost", card.transform, "50", 12);
        var costText = costObj.GetComponent<Text>();
        costText.color = Color.yellow;
        var costRect = costObj.GetComponent<RectTransform>();
        costRect.anchorMin = new Vector2(0, 0);
        costRect.anchorMax = new Vector2(1, 0);
        costRect.pivot = new Vector2(0.5f, 0);
        costRect.sizeDelta = new Vector2(0, 20);
        costRect.anchoredPosition = new Vector2(0, 2);

        // TowerCard 컴포넌트 추가
        card.AddComponent<TowerCard>();

        // 프리팹 저장
        string prefabPath = "Assets/Prefabs/UI/TowerCard.prefab";
        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(card, prefabPath);
        DestroyImmediate(card);

        // 선택
        Selection.activeObject = prefab;
        EditorGUIUtility.PingObject(prefab);

        Debug.Log($"Tower Card prefab created: {prefabPath}");
    }

    private static GameObject CreateTextObject(string name, Transform parent, string content, int fontSize)
    {
        GameObject textObj = new GameObject(name);
        textObj.transform.SetParent(parent, false);

        var rect = textObj.AddComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.sizeDelta = Vector2.zero;
        rect.anchoredPosition = Vector2.zero;

        var text = textObj.AddComponent<Text>();
        text.text = content;
        text.fontSize = fontSize;
        text.color = Color.white;
        text.alignment = TextAnchor.MiddleCenter;
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

        return textObj;
    }

    private static void CreateFolderIfNotExists(string path)
    {
        if (!AssetDatabase.IsValidFolder(path))
        {
            string parent = System.IO.Path.GetDirectoryName(path).Replace("\\", "/");
            string folderName = System.IO.Path.GetFileName(path);
            AssetDatabase.CreateFolder(parent, folderName);
        }
    }
}
