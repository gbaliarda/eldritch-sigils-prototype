using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BuildingUI : Singleton<BuildingUI>
{
    [SerializeField] private TextMeshProUGUI _title;
    [SerializeField] private TextMeshProUGUI _time;
    [SerializeField] private TextMeshProUGUI _performance;
    [SerializeField] private UnityEngine.UI.Slider _overclock;
    [SerializeField] private Toggle _on;
    public FactoryBuilding OpenBuilding { get; private set; }
    void Start()
    {
        //EventManager.Instance.OnOpenBuildingUI += OnOpenBuildingUI;

        OnCloseBuildingUI();
    }


    private void OnOpenBuildingUI(FactoryBuilding building)
    {
        OpenBuilding = building;
        RefreshOpenBuildingStats();
        gameObject.SetActive(true);
    }

    public void RefreshOpenBuildingStats()
    {
        _title.text = "Shotgun Bullet Building";
        _time.text = $"{OpenBuilding.RealTimeInterval:F2} sec";
        _performance.text = $"{(OpenBuilding.Performance):F1}%";
        _overclock.value = OpenBuilding.Overclock;
        _on.isOn = OpenBuilding.IsOn;
    }

    public void OnCloseBuildingUI()
    {
        gameObject.SetActive(false);
    }

    public void OnOvercloakValueChange()
    {
        OpenBuilding.SetOverclock(_overclock.value);
        RefreshOpenBuildingStats();
    }
    
    public void OnIsOnValueChange()
    {
        OpenBuilding.SetIsOn(_on.isOn);
        RefreshOpenBuildingStats();
    }
}
