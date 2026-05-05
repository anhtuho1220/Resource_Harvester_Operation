using UnityEngine;
using UnityEngine.UIElements;
using System.Collections.Generic;

[RequireComponent(typeof(UIDocument))]
public class GameUIController : MonoBehaviour
{
    private VisualElement root;
    private VisualElement mainLayout;
    
    // Left side
    private Label inventoryLabel;
    private Label selectedUnitsLabel;
    private VisualElement selectedUnitsList;

    // Right side
    private Button btnBuildDrone;
    private Button btnBuildTruck;
    private Button btnPing;
    private VisualElement buildProgressContainer;

    // Pause Menu
    private VisualElement pauseMenu;

    private void OnEnable() {
        UIDocument uiDoc = GetComponent<UIDocument>();
        if (uiDoc == null) return;
        root = uiDoc.rootVisualElement;
        
        mainLayout = new VisualElement();
        mainLayout.style.flexGrow = 1;
        mainLayout.style.flexDirection = FlexDirection.Row;
        mainLayout.style.justifyContent = Justify.SpaceBetween;
        mainLayout.style.paddingTop = 20;
        mainLayout.style.paddingLeft = 20;
        mainLayout.style.paddingRight = 20;
        mainLayout.style.paddingBottom = 20;
        mainLayout.pickingMode = PickingMode.Ignore;
        root.Add(mainLayout);

        VisualElement leftCol = new VisualElement();
        leftCol.style.width = 300;
        leftCol.pickingMode = PickingMode.Ignore;
        mainLayout.Add(leftCol);

        VisualElement leftContainer = CreateStyledContainer();
        inventoryLabel = new Label();
        inventoryLabel.style.fontSize = 18;
        inventoryLabel.style.color = new Color(0, 0.2f, 0);
        leftContainer.Add(inventoryLabel);
        leftCol.Add(leftContainer);

        VisualElement selContainer = CreateStyledContainer();
        selectedUnitsLabel = new Label();
        selectedUnitsLabel.style.fontSize = 18;
        selectedUnitsLabel.style.color = new Color(0, 0.2f, 0);
        selectedUnitsLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
        selContainer.Add(selectedUnitsLabel);

        selectedUnitsList = new VisualElement();
        selContainer.Add(selectedUnitsList);
        leftCol.Add(selContainer);

        VisualElement rightCol = new VisualElement();
        rightCol.style.width = 300;
        rightCol.pickingMode = PickingMode.Ignore;
        mainLayout.Add(rightCol);

        VisualElement rightContainer = CreateStyledContainer();
        Label opsLabel = new Label("Base Operations");
        opsLabel.style.fontSize = 20;
        opsLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
        opsLabel.style.color = new Color(0, 0.2f, 0);
        opsLabel.style.marginBottom = 10;
        rightContainer.Add(opsLabel);

        btnBuildDrone = CreateButton("Build Drone (500M 200C 1000F) - 30s", () => SceneManager.Instance.BuyDrone());
        btnBuildTruck = CreateButton("Build Truck (300M 100C 500F) - 20s", () => SceneManager.Instance.BuyTruck());
        btnPing = CreateButton("Resource Ping", () => {
            var b = FindAnyObjectByType<BaseUnit>();
            if (b != null) b.Ping();
        });
        
        rightContainer.Add(btnBuildDrone);
        rightContainer.Add(btnBuildTruck);
        rightContainer.Add(btnPing);

        buildProgressContainer = new VisualElement();
        buildProgressContainer.style.marginTop = 10;
        rightContainer.Add(buildProgressContainer);

        rightCol.Add(rightContainer);

        pauseMenu = new VisualElement();
        pauseMenu.style.position = Position.Absolute;
        pauseMenu.style.top = 0;
        pauseMenu.style.bottom = 0;
        pauseMenu.style.left = 0;
        pauseMenu.style.right = 0;
        pauseMenu.style.backgroundColor = new Color(0, 0, 0, 0.8f);
        pauseMenu.style.justifyContent = Justify.Center;
        pauseMenu.style.alignItems = Align.Center;
        pauseMenu.style.display = DisplayStyle.None;

        VisualElement pauseBox = CreateStyledContainer();
        pauseBox.style.width = 300;
        pauseBox.style.alignItems = Align.Center;

        Label pauseTitle = new Label("PAUSED");
        pauseTitle.style.fontSize = 30;
        pauseTitle.style.unityFontStyleAndWeight = FontStyle.Bold;
        pauseTitle.style.color = new Color(0, 0.2f, 0);
        pauseTitle.style.marginBottom = 20;
        pauseBox.Add(pauseTitle);

        Button resumeBtn = CreateButton("Resume", () => { if(SceneManager.Instance != null) SceneManager.Instance.TogglePause(); });
        resumeBtn.style.width = 200;
        Button quitBtn = CreateButton("Quit", () => {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        });
        quitBtn.style.width = 200;

        pauseBox.Add(resumeBtn);
        pauseBox.Add(quitBtn);
        pauseMenu.Add(pauseBox);

        root.Add(pauseMenu);
    }

    private Button CreateButton(string text, System.Action onClick) {
        Button b = new Button(onClick);
        b.text = text;
        b.style.fontSize = 14;
        b.style.unityFontStyleAndWeight = FontStyle.Bold;
        b.style.color = new Color(0, 0.2f, 0);
        b.style.marginBottom = 5;
        b.style.paddingTop = 5;
        b.style.paddingBottom = 5;
        b.style.backgroundColor = new Color(0.8f, 0.8f, 0.8f);
        return b;
    }

    private VisualElement CreateStyledContainer() {
        VisualElement container = new VisualElement();
        container.style.backgroundColor = new Color(84f/255f, 150f/255f, 183f/255f, 0.8f);
        container.style.borderTopLeftRadius = 20;
        container.style.borderTopRightRadius = 20;
        container.style.borderBottomLeftRadius = 20;
        container.style.borderBottomRightRadius = 20;
        container.style.borderTopColor = new Color(0.8f, 0.8f, 0.8f);
        container.style.borderBottomColor = new Color(0.8f, 0.8f, 0.8f);
        container.style.borderLeftColor = new Color(0.8f, 0.8f, 0.8f);
        container.style.borderRightColor = new Color(0.8f, 0.8f, 0.8f);
        container.style.borderTopWidth = 2;
        container.style.borderBottomWidth = 2;
        container.style.borderLeftWidth = 2;
        container.style.borderRightWidth = 2;
        container.style.paddingLeft = 15;
        container.style.paddingRight = 15;
        container.style.paddingTop = 15;
        container.style.paddingBottom = 15;
        container.style.marginBottom = 10;
        return container;
    }

    private void Update() {
        if (SceneManager.Instance == null) return;

        if (!SceneManager.Instance.isGameActive) {
            mainLayout.style.display = DisplayStyle.None;
            pauseMenu.style.display = DisplayStyle.None;
            return;
        } else {
            mainLayout.style.display = DisplayStyle.Flex;
        }

        pauseMenu.style.display = SceneManager.Instance.isPaused ? DisplayStyle.Flex : DisplayStyle.None;

        string inv = "<b>Base Inventory</b>\n";
        inv += $"Max Capacity: {SceneManager.MaxBaseCapacity}\n";
        inv += $"Metal: {SceneManager.Instance.GetStock(ResourceType.Metal)}\n";
        inv += $"Crystal: {SceneManager.Instance.GetStock(ResourceType.Crystal)}\n";
        inv += $"Fuel: {SceneManager.Instance.GetStock(ResourceType.Fuel)}";
        inventoryLabel.text = inv;

        btnBuildDrone.SetEnabled(SceneManager.Instance.CanAffordDrone());
        btnBuildTruck.SetEnabled(SceneManager.Instance.CanAffordTruck());

        BaseUnit baseUnit = FindAnyObjectByType<BaseUnit>();
        if (baseUnit != null) {
            float timeSincePing = Time.time - baseUnit.lastPingTime;
            bool canPing = timeSincePing >= 120f;
            btnPing.SetEnabled(canPing);
            btnPing.text = canPing ? "Resource Ping" : $"Resource Ping ({Mathf.CeilToInt(120f - timeSincePing)}s)";
        }

        buildProgressContainer.Clear();
        if (SceneManager.Instance.activeBuilds.Count > 0) {
            Label bTitle = new Label("<b>Active Builds</b>");
            bTitle.style.color = new Color(0, 0.2f, 0);
            buildProgressContainer.Add(bTitle);

            foreach (var build in SceneManager.Instance.activeBuilds) {
                float progress = (Time.time - build.startTime) / build.duration;
                
                Label l = new Label($"Building {build.type}... ({Mathf.FloorToInt(progress * 100)}%)");
                l.style.color = new Color(0, 0.2f, 0);
                buildProgressContainer.Add(l);

                VisualElement barBg = new VisualElement();
                barBg.style.height = 10;
                barBg.style.backgroundColor = Color.gray;
                barBg.style.marginBottom = 5;

                VisualElement barFill = new VisualElement();
                barFill.style.height = 10;
                barFill.style.width = Length.Percent(Mathf.Clamp01(progress) * 100);
                barFill.style.backgroundColor = Color.green;
                
                barBg.Add(barFill);
                buildProgressContainer.Add(barBg);
            }
        }

        PlayerController pc = FindAnyObjectByType<PlayerController>();
        if (pc != null) {
            selectedUnitsList.Clear();
            var drones = pc.GetSelectedDrones();
            var trucks = pc.GetSelectedTrucks();
            bool baseSelected = pc.GetSelectedBase() != null;

            if (drones.Count == 0 && trucks.Count == 0 && !baseSelected) {
                selectedUnitsLabel.text = "No Units Selected";
            } else {
                string title = baseSelected ? "Mobile Harvester\n" : "";
                
                if (drones.Count > 0 && trucks.Count > 0) {
                    selectedUnitsLabel.text = title + $"Selected Units\nDrone x{drones.Count}\nTruck x{trucks.Count}";
                } else if (drones.Count > 0) {
                    selectedUnitsLabel.text = title + "Selected Drones";
                    for (int i = drones.Count - 1; i >= 0; i--) {
                        VisualElement row = new VisualElement();
                        row.style.flexDirection = FlexDirection.Row;
                        row.style.justifyContent = Justify.SpaceBetween;
                        
                        Label l = new Label($"Drone #{i + 1}");
                        l.style.color = new Color(0, 0.2f, 0);
                        
                        int index = i;
                        Button rx = new Button(() => pc.RemoveDrone(index));
                        rx.text = "X";
                        rx.style.width = 25;
                        
                        row.Add(l);
                        row.Add(rx);
                        selectedUnitsList.Add(row);
                    }
                } else if (trucks.Count > 0) {
                    selectedUnitsLabel.text = title + "Selected Trucks";
                    for (int i = trucks.Count - 1; i >= 0; i--) {
                        VisualElement row = new VisualElement();
                        row.style.flexDirection = FlexDirection.Row;
                        row.style.justifyContent = Justify.SpaceBetween;
                        
                        Label l = new Label($"Truck #{i + 1}");
                        l.style.color = new Color(0, 0.2f, 0);
                        
                        int index = i;
                        Button rx = new Button(() => pc.RemoveTruck(index));
                        rx.text = "X";
                        rx.style.width = 25;
                        
                        row.Add(l);
                        row.Add(rx);
                        selectedUnitsList.Add(row);
                    }
                } else if (baseSelected) {
                    selectedUnitsLabel.text = "Mobile Harvester";
                }
            }
        }
    }
}
