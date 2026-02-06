using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

/// <summary>
/// 개별 타워 선택 카드 UI
/// PvZ 스타일 드래그 앤 드롭 배치 지원
/// </summary>
public class TowerCard : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [Header("UI Elements")]
    [SerializeField] private Image backgroundImage;
    [SerializeField] private Image iconImage;
    [SerializeField] private Text nameText;
    [SerializeField] private Text costText;
    [SerializeField] private Text hotkeyText;
    [SerializeField] private Button selectButton;

    [Header("Colors")]
    [SerializeField] private Color normalColor = new Color(0.3f, 0.3f, 0.3f, 1f);
    [SerializeField] private Color selectedColor = new Color(0.2f, 0.6f, 0.2f, 1f);
    [SerializeField] private Color disabledColor = new Color(0.5f, 0.5f, 0.5f, 0.5f);

    private int cardIndex;
    private TowerBase tower;
    private GameUI gameUI;

    public int CardIndex => cardIndex;
    public TowerBase Tower => tower;

    private void Awake()
    {
        // 자동으로 UI 요소 찾기
        if (backgroundImage == null)
            backgroundImage = GetComponent<Image>();
        if (selectButton == null)
            selectButton = GetComponent<Button>();
        if (nameText == null)
        {
            var t = transform.Find("TowerName");
            if (t != null) nameText = t.GetComponent<Text>();
        }
        if (costText == null)
        {
            var t = transform.Find("Cost");
            if (t != null) costText = t.GetComponent<Text>();
        }
        if (hotkeyText == null)
        {
            var t = transform.Find("Hotkey");
            if (t != null) hotkeyText = t.GetComponent<Text>();
        }
    }

    public void Initialize(int index, TowerBase towerPrefab, GameUI ui)
    {
        cardIndex = index;
        tower = towerPrefab;
        gameUI = ui;

        // 타워 정보 표시
        if (nameText != null) nameText.text = tower.UnitName;
        if (costText != null) costText.text = $"{tower.EnergyCost}";
        if (hotkeyText != null) hotkeyText.text = $"[{index + 1}]";

        // 버튼 클릭 이벤트
        if (selectButton != null)
            selectButton.onClick.AddListener(OnCardClicked);
    }

    private void OnCardClicked()
    {
        if (gameUI != null)
            gameUI.SelectTower(cardIndex);
    }

    public void SetSelected(bool selected)
    {
        if (backgroundImage != null)
            backgroundImage.color = selected ? selectedColor : normalColor;
    }

    public void UpdateAffordability(int currentEnergy)
    {
        bool canAfford = currentEnergy >= tower.EnergyCost;

        if (selectButton != null)
            selectButton.interactable = canAfford;

        if (backgroundImage != null && !canAfford)
            backgroundImage.color = disabledColor;
    }

    #region Drag Handlers

    public void OnBeginDrag(PointerEventData eventData)
    {
        // 에너지 부족 시 드래그 시작 불가
        if (gameUI == null || tower == null) return;
        if (!gameUI.CanAffordTower(tower))
        {
            eventData.pointerDrag = null; // 드래그 취소
            return;
        }

        gameUI.StartDragPlacement(this, eventData);
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (gameUI == null) return;
        gameUI.UpdateDragPlacement(eventData);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (gameUI == null) return;
        gameUI.EndDragPlacement(eventData);
    }

    #endregion
}
