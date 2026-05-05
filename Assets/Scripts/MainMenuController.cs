using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class MainMenuController : MonoBehaviour
{
    private VisualElement root;
    private VisualElement container;
    
    private Button playBtn;
    private Button configBtn;
    private Button quitBtn;

    private ScrollView configPanel;

    private void OnEnable()
    {
        UIDocument uiDoc = GetComponent<UIDocument>();
        if (uiDoc == null) return;
        
        root = uiDoc.rootVisualElement;
        container = root.Q<VisualElement>("Container");

        playBtn = root.Q<Button>("Play");
        configBtn = root.Q<Button>("Config");
        quitBtn = root.Q<Button>("Quit");

        if (playBtn != null) playBtn.clicked += StartGame;
        if (configBtn != null) configBtn.clicked += ToggleConfig;
        if (quitBtn != null) quitBtn.clicked += QuitGame;

        if (container != null) {
            CreateConfigPanel();
        }
    }

    private void StartGame()
    {
        if (root != null) {
            root.style.display = DisplayStyle.None;
        }

        if (SceneManager.Instance != null) {
            SceneManager.Instance.StartGameEnvironment();
        }
    }

    private void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    private void ToggleConfig()
    {
        if (configPanel == null) return;
        
        bool isConfigOpen = configPanel.style.display == DisplayStyle.Flex;
        
        if (!isConfigOpen) {
            playBtn.style.display = DisplayStyle.None;
            configBtn.style.display = DisplayStyle.None;
            quitBtn.style.display = DisplayStyle.None;
            configPanel.style.display = DisplayStyle.Flex;
        } else {
            playBtn.style.display = DisplayStyle.Flex;
            configBtn.style.display = DisplayStyle.Flex;
            quitBtn.style.display = DisplayStyle.Flex;
            configPanel.style.display = DisplayStyle.None;
        }
    }

    private void CreateConfigPanel()
    {
        configPanel = new ScrollView();
        configPanel.style.display = DisplayStyle.None;
        configPanel.style.flexGrow = 1;
        configPanel.style.paddingLeft = 20;
        configPanel.style.paddingRight = 20;
        configPanel.style.marginTop = 20;
        configPanel.style.marginBottom = 20;

        Label header = new Label("CONFIGURATION");
        header.style.fontSize = 40;
        header.style.unityFontStyleAndWeight = FontStyle.Bold;
        header.style.color = new Color(0, 0.2f, 0);
        header.style.alignSelf = Align.Center;
        header.style.marginBottom = 20;
        configPanel.Add(header);

        configPanel.Add(CreateIntSlider("Resources per type", GameSettings.resourcesPerType, 5, 100, v => GameSettings.resourcesPerType = v));

        configPanel.Add(CreateIntSlider("Initial Metal", GameSettings.initialMetal, 0, 5000, v => GameSettings.initialMetal = v));
        configPanel.Add(CreateIntSlider("Initial Crystal", GameSettings.initialCrystal, 0, 5000, v => GameSettings.initialCrystal = v));
        configPanel.Add(CreateIntSlider("Initial Fuel", GameSettings.initialFuel, 0, 5000, v => GameSettings.initialFuel = v));

        configPanel.Add(CreateIntSlider("Initial Drones", GameSettings.initialDrones, 0, 10, v => GameSettings.initialDrones = v));
        configPanel.Add(CreateIntSlider("Initial Trucks", GameSettings.initialTrucks, 0, 10, v => GameSettings.initialTrucks = v));

        configPanel.Add(CreateFloatSlider("Drone Speed", GameSettings.droneSpeed, 5f, 50f, v => GameSettings.droneSpeed = v));
        configPanel.Add(CreateFloatSlider("Truck Speed", GameSettings.truckSpeed, 5f, 50f, v => GameSettings.truckSpeed = v));
        configPanel.Add(CreateFloatSlider("Base Speed", GameSettings.baseSpeed, 0f, 20f, v => GameSettings.baseSpeed = v));

        Button backBtn = new Button(() => ToggleConfig());
        backBtn.text = "Back to Menu";
        backBtn.style.fontSize = 30;
        backBtn.style.marginTop = 30;
        backBtn.style.unityFontStyleAndWeight = FontStyle.Bold;
        backBtn.style.color = new Color(0, 0.2f, 0);
        configPanel.Add(backBtn);

        container.Add(configPanel);
    }

    private VisualElement CreateIntSlider(string labelText, int initialValue, int min, int max, System.Action<int> onValueChanged)
    {
        SliderInt slider = new SliderInt(labelText, min, max);
        slider.value = initialValue;
        slider.style.fontSize = 20;
        slider.style.color = new Color(0, 0.2f, 0);
        slider.style.marginBottom = 10;
        slider.RegisterValueChangedCallback(evt => onValueChanged(evt.newValue));
        return slider;
    }

    private VisualElement CreateFloatSlider(string labelText, float initialValue, float min, float max, System.Action<float> onValueChanged)
    {
        Slider slider = new Slider(labelText, min, max);
        slider.value = initialValue;
        slider.style.fontSize = 20;
        slider.style.color = new Color(0, 0.2f, 0);
        slider.style.marginBottom = 10;
        slider.RegisterValueChangedCallback(evt => onValueChanged(evt.newValue));
        return slider;
    }
}
