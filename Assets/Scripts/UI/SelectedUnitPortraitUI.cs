using Unity.Collections;
using Unity.Entities;
using UnityEngine;
using UnityEngine.UI;

public class SelectedUnitPortraitUI : MonoBehaviour {

    [Header("Portrait UI Elements")]
    [SerializeField] private GameObject portraitPanel;
    [SerializeField] private Image portraitImage;
    [SerializeField] private Text unitNameText;
    
    private EntityManager entityManager;
    private UnitTypeListSO unitTypeListSO;

    private void Start() {
        entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
        unitTypeListSO = GameAssets.Instance.unitTypeListSO;
        
        // Subscribe to selection changes
        UnitSelectionManager.Instance.OnSelectedEntitiesChanged += OnSelectedEntitiesChanged;
        
        // Wichtig: Panel am Start deaktivieren (wie du es wolltest!)
        portraitPanel.SetActive(false);
    }

    private void OnDestroy() {
        if (UnitSelectionManager.Instance != null) {
            UnitSelectionManager.Instance.OnSelectedEntitiesChanged -= OnSelectedEntitiesChanged;
        }
    }

    private void OnSelectedEntitiesChanged(object sender, System.EventArgs e) {
        UpdatePortraitDisplay();
    }

    private void UpdatePortraitDisplay() {
        // Get selected flagbearer unit
        Entity selectedFlagbearer = GetSelectedFlagbearer();
        
        if (selectedFlagbearer == Entity.Null) {
            // Keine Flagbearer ausgewählt -> Panel AUS
            portraitPanel.SetActive(false);
            return;
        }

        // Get the unit type of the selected flagbearer
        if (entityManager.HasComponent<UnitTypeHolder>(selectedFlagbearer)) {
            UnitTypeHolder unitTypeHolder = entityManager.GetComponentData<UnitTypeHolder>(selectedFlagbearer);
            UnitTypeSO unitTypeSO = unitTypeListSO.GetUnitTypeSO(unitTypeHolder.unitType);
            
            if (unitTypeSO != null) {
                // Flagbearer ausgewählt -> Panel AN
                portraitPanel.SetActive(true);
                
                // Use portrait if available, otherwise fallback to sprite
                Sprite displaySprite = unitTypeSO.portrait != null ? unitTypeSO.portrait : unitTypeSO.sprite;
                portraitImage.sprite = displaySprite;
                
                // Update unit name
                if (unitNameText != null) {
                    unitNameText.text = unitTypeSO.unitName;
                }
            }
        }
    }

    private Entity GetSelectedFlagbearer() {
        // Query for selected entities with FlagBearer component
        EntityQuery entityQuery = new EntityQueryBuilder(Allocator.Temp)
            .WithAll<Selected, FlagBearer>()
            .Build(entityManager);

        NativeArray<Entity> selectedFlagbearers = entityQuery.ToEntityArray(Allocator.Temp);
        
        if (selectedFlagbearers.Length > 0) {
            Entity flagbearer = selectedFlagbearers[0]; // Take the first selected flagbearer
            selectedFlagbearers.Dispose();
            return flagbearer;
        }
        
        selectedFlagbearers.Dispose();
        return Entity.Null;
    }

    // Optional: Methode um das Panel manuell zu verstecken (falls du es brauchst)
    public void HidePortrait() {
        portraitPanel.SetActive(false);
    }

    // Optional: Methode um zu checken ob das Panel aktiv ist
    public bool IsPortraitVisible() {
        return portraitPanel.activeSelf;
    }
}