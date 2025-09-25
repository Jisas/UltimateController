using UltimateFramework.LocomotionSystem;
using System.Collections.Generic;
using UltimateFramework.Editor;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEditor;

public class LocomotionMapsManagerWindow : EditorWindow
{
    private VisualTreeAsset m_UXML;
    private VisualElement base_Root;
    private VisualTreeAsset MainMapStructTemplate;
    private VisualTreeAsset OverrideMapStructTemplate;
    private VisualTreeAsset LocomotionGeneralsRightTemplate;
    private VisualTreeAsset LocomotionMovementRightTemplate;
    private VisualTreeAsset LocomotionGeneralsOverrideRightTemplate;
    private VisualTreeAsset IdleBreakElement;
    private LocomotionMaster locomotionMaster;
    private Button saveButton;

    private List<VisualElement> m_MapElementTemplates = new();
    private List<VisualElement> m_OverrideLayerTemplates = new();
    private Dictionary<int, AnimationClip> m_AnimationClipDic = new();

    // preview
    private PreviewRenderUtility previewUtility;
    private IMGUIContainer m_IMGUIContainer;
    private Animator previewAnimator;
    private GameObject previewModel;
    private bool isPreviewOpen = false;
    private Vector3 DefaultCameraPosition = new(0, 0.8f, -8);
    private Vector3 _targetPosition;
    private float _distance = 8.0f;
    private float _xSpeed = 5.0f;
    private float _ySpeed = 5.0f;
    private float _yMinLimit = -90f;
    private float _yMaxLimit = 90f;
    private float _distanceMin = 0.5f;
    private float _distanceMax = 15f;
    private float _x = 0.0f;
    private float _y = 0.0f;

    enum MovementStruct
    {
        General = 0,
        Walk = 1,
        Jog = 2,
        Crouch = 3
    }
    Color startColor = new(0, 0, 0, 0);
    Color endColor = new(1, 1, 0.4823529f, 1);


    [MenuItem("Ultimate Framework/Windows/Locomotion Maps Manager")]
    public static void ShowMyEditor()
    {
        EditorWindow wnd = GetWindow<LocomotionMapsManagerWindow>();
        Texture2D icon = Resources.Load<Texture2D>("Img/Locomotion_Maps_Window_Icon");
        wnd.titleContent = new GUIContent("Locomotion Maps Manager", icon);
    }
    void LoadResources()
    {
        m_UXML = Resources.Load<VisualTreeAsset>("UIToolkit/UXML/Windows/LocomotionMapsManagerWindow");
        locomotionMaster = Resources.Load<LocomotionMaster>("Data/Locomotion/LocomotionMasterMap");
        MainMapStructTemplate = Resources.Load<VisualTreeAsset>("UIToolkit/UXML/Windows/Templates/Map-Struct-Template");
        OverrideMapStructTemplate = Resources.Load<VisualTreeAsset>("UIToolkit/UXML/Windows/Templates/Override-Map-Struct-Template");
        LocomotionGeneralsRightTemplate = Resources.Load<VisualTreeAsset>("UIToolkit/UXML/Windows/Templates/Locomotion-Generals-Right-Template");
        LocomotionMovementRightTemplate = Resources.Load<VisualTreeAsset>("UIToolkit/UXML/Windows/Templates/Locomotion-Movement-Right-Template");
        LocomotionGeneralsOverrideRightTemplate = Resources.Load<VisualTreeAsset>("UIToolkit/UXML/Windows/Templates/Locomotion-Generals-Override-Right-Template");
        IdleBreakElement = Resources.Load<VisualTreeAsset>("UIToolkit/UXML/Windows/Templates/IdleBreakElement");
    }

    private void CreateGUI()
    {
        LoadResources();
        base_Root = rootVisualElement;
        m_UXML.CloneTree(base_Root);

        var searchField = FindElementInRoot<ToolbarSearchField>("search-field");
        searchField.RegisterValueChangedCallback((evt) => Search(evt));

        saveButton = FindElementInRoot<Button>("save-button");
        saveButton.clickable.clicked += () => SaveData();

        ScrollView mainMapsContainer = FindElementInRoot<ScrollView>("maps-container-scroll-view");
        Button addNewMapButton = FindElementInRoot<Button>("add-new-map-button");
        addNewMapButton.clickable.clicked += () => CreateMap(mainMapsContainer, new LocomotionMap("NewMap"));

        foreach (var map in locomotionMaster.locomotionMaps)
        {
            CreateMap(mainMapsContainer, map);
        }

        ToolbarButton previsualizerButton = FindElementInRoot<ToolbarButton>("pre-visualizer-button");
        VisualElement previsulizerContainer = FindElementInRoot<VisualElement>("pre-visualizer-container");
        previsulizerContainer.style.display = DisplayStyle.None;
        previsualizerButton.clickable.clicked += () => SetPreviewDisplay(previsulizerContainer);
    }
    private void OnGUI()
    {
        if (isPreviewOpen && m_IMGUIContainer != null)
        {          
            RenderPreview();
        }
    }
    private void OnDisable()
    {
        previewUtility?.Cleanup();
        if (previewModel != null) Object.DestroyImmediate(previewModel);
    }

    private void Search(ChangeEvent<string> evt)
    {
        string searchQuery = evt.newValue.ToLower();
        bool hasSearchQuery = !string.IsNullOrEmpty(searchQuery);

        SetElementDisplay(base_Root, "Map-Struct-Template", hasSearchQuery ? DisplayStyle.None : DisplayStyle.Flex);

        // Búsqueda y actualización de la visualización de los elementos
        foreach (var element in m_MapElementTemplates)
        {
            var mapName = FindElementInRoot<TextField>(element, "map-name");
            if (element != null)
                element.style.display = mapName.value.ToLower().Contains(searchQuery) ? DisplayStyle.Flex : DisplayStyle.None;
        }
    }

    private void CreateMap(ScrollView container, LocomotionMap locomotionMap)
    {
        var instance = MainMapStructTemplate.CloneTree();
        m_MapElementTemplates.Add(instance);
        container.Add(instance);

        if (locomotionMap.name.Contains("NewMap")) locomotionMaster.locomotionMaps.Add(locomotionMap);

        // root
        ScrollView rootRightContentContainer = FindElementInRoot<ScrollView>("right-content-container");
        VisualElement rootPreVisualizerContainer = FindElementInRoot<VisualElement>("pre-visualizer-container");

        // main map
        TextField mapName = FindElementInRoot<TextField>(instance, "map-name");
        VisualElement rootArrow = FindElementInRoot<VisualElement>(instance, "arrow-image");
        ToolbarButton selfButton = FindElementInRoot<ToolbarButton>(instance, "root-header-button");
        Button removeButton = FindElementInRoot<Button>(instance, "remove-map-button");
        VisualElement rootContentContainer = FindElementInRoot<VisualElement>(instance, "root-content-container");

        mapName.value = locomotionMap.name;
        mapName.RegisterValueChangedCallback((evt) =>
        {;
             locomotionMap.name = evt.newValue;
        });

        removeButton.clickable.clicked += () =>
        {
            RemoveMap(container, instance, locomotionMap);
            SetEndColorToSaveButton();
        };

        rootContentContainer.style.display = DisplayStyle.None;
        selfButton.clickable.clicked += () => SetElementDisplay(rootContentContainer, rootArrow);

        // general
        ToolbarButton generalButton = FindElementInRoot<ToolbarButton>(instance, "general-self-button");
        generalButton.clickable.clicked += () => InstanceRightContentElement(rootRightContentContainer, MovementStruct.General, locomotionMap);

        // movement
        ToolbarButton movementButton = FindElementInRoot<ToolbarButton>(instance, "movement-self-button");
        VisualElement movementContentcontainer = FindElementInRoot<VisualElement>(instance, "movement-content-container");
        VisualElement movementStructArrow = FindElementInRoot<VisualElement>(movementButton, "arrow-image");
        movementContentcontainer.style.display = DisplayStyle.None;
        movementButton.clickable.clicked += () => SetElementDisplay(movementContentcontainer, movementStructArrow);

        // walk
        ToolbarButton walkSelfButton = FindElementInRoot<ToolbarButton>(instance, "walk-self-button");
        VisualElement walkStructArrow = FindElementInRoot<VisualElement>(walkSelfButton, "arrow-image");
        walkSelfButton.clickable.clicked += () => InstanceRightContentElement(rootRightContentContainer, MovementStruct.Walk, locomotionMap);

        // jog
        ToolbarButton jogSelfButton = FindElementInRoot<ToolbarButton>(instance, "jog-self-button");
        VisualElement jogStructArrow = FindElementInRoot<VisualElement>(jogSelfButton, "arrow-image");
        jogSelfButton.clickable.clicked += () => InstanceRightContentElement(rootRightContentContainer, MovementStruct.Jog, locomotionMap);

        // crouch
        ToolbarButton crouchSelfButton = FindElementInRoot<ToolbarButton>(instance, "crouch-self-button");
        VisualElement crouchStructArrow = FindElementInRoot<VisualElement>(crouchSelfButton, "arrow-image");
        crouchSelfButton.clickable.clicked += () => InstanceRightContentElement(rootRightContentContainer, MovementStruct.Crouch, locomotionMap);

        // override layers
        ToolbarButton overrideLayerSelfButton = FindElementInRoot<ToolbarButton>(instance, "override-layers-self-button");
        VisualElement overrideLayersContentContainer = FindElementInRoot<VisualElement>(instance, "override-layers-content-container");
        VisualElement overrideLayersArrow = FindElementInRoot<VisualElement>(overrideLayerSelfButton, "arrow-image");
        Button addOverrideLayerButton = FindElementInRoot<Button>(instance, "add-override-layer-button");

        overrideLayersContentContainer.style.display = DisplayStyle.None;
        overrideLayerSelfButton.clickable.clicked += () => SetElementDisplay(overrideLayersContentContainer, overrideLayersArrow);
        addOverrideLayerButton.clickable.clicked += () => CreateOverrideLayer(overrideLayersContentContainer, locomotionMap, new LocomotionOverrideLayer("NewOverrideLayer"));

        foreach (var overrideLayer in locomotionMap.movement.overrideLayers)
        {
            CreateOverrideLayer(overrideLayersContentContainer, locomotionMap, overrideLayer);
        }
    }
    private void RemoveMap(ScrollView container, VisualElement mapInstance, LocomotionMap locomotionMap)
    {
        container.Remove(mapInstance);
        m_MapElementTemplates.Remove(mapInstance);
        locomotionMaster.locomotionMaps.Remove(locomotionMap);
    }

    private void CreateOverrideLayer(VisualElement container, LocomotionMap locomotionMap, LocomotionOverrideLayer overrideLayer)
    {
        var instance = OverrideMapStructTemplate.CloneTree();
        m_OverrideLayerTemplates.Add(instance);
        container.Add(instance);

        if (overrideLayer.name.Contains("NewOverrideLayer")) locomotionMap.movement.overrideLayers.Add(overrideLayer);

        // root
        ScrollView rootRightContentContainer = FindElementInRoot<ScrollView>("right-content-container");
        VisualElement rootPreVisualizerContainer = FindElementInRoot<VisualElement>("pre-visualizer-container");

        // main map
        TextField mapName = FindElementInRoot<TextField>(instance, "map-name");
        VisualElement rootArrow = FindElementInRoot<VisualElement>(instance, "arrow-image");
        ToolbarButton selfButton = FindElementInRoot<ToolbarButton>(instance, "root-header");
        Label maskLabel = FindElementInRoot<Label>(instance, "mask-name-label");
        Button removeButton = FindElementInRoot<Button>(instance, "remove-override-layer-button");
        VisualElement rootContentContainer = FindElementInRoot<VisualElement>(instance, "override-layer-content-container");

        mapName.value = overrideLayer.name;
        mapName.RegisterValueChangedCallback((evt) =>
        {
            var structValue = overrideLayer;
            structValue.name = evt.newValue;
            SetEndColorToSaveButton();
        });

        removeButton.clickable.clicked += () =>
        {
            RemoveOverrideLayer(container, instance, locomotionMap, overrideLayer);
            SetEndColorToSaveButton();
        };

        maskLabel.text = overrideLayer.movement.motionMask;
        rootContentContainer.style.display = DisplayStyle.None;
        selfButton.clickable.clicked += () => SetElementDisplay(rootContentContainer, rootArrow);

        // general
        ToolbarButton generalButton = FindElementInRoot<ToolbarButton>(instance, "general-self-button");
        generalButton.clickable.clicked += () => InstanceRightContentElement(rootRightContentContainer, MovementStruct.General, locomotionMap, true, overrideLayer);

        // movement
        ToolbarButton movementButton = FindElementInRoot<ToolbarButton>(instance, "movement-self-button");
        VisualElement movementContentcontainer = FindElementInRoot<VisualElement>(instance, "movement-content-container");
        VisualElement movementStructArrow = FindElementInRoot<VisualElement>(movementButton, "arrow-image");

        movementContentcontainer.style.display = DisplayStyle.None;
        movementButton.clickable.clicked += () => SetElementDisplay(movementContentcontainer, movementStructArrow);

        // walk
        ToolbarButton walkSelfButton = FindElementInRoot<ToolbarButton>(instance, "walk-self-button");
        VisualElement walkStructArrow = FindElementInRoot<VisualElement>(walkSelfButton, "arrow-image");
        walkSelfButton.clickable.clicked += () => InstanceRightContentElement(rootRightContentContainer, MovementStruct.Walk, locomotionMap, true, overrideLayer);

        // jog
        ToolbarButton jogSelfButton = FindElementInRoot<ToolbarButton>(instance, "jog-self-button");
        VisualElement jogStructArrow = FindElementInRoot<VisualElement>(jogSelfButton, "arrow-image");
        jogSelfButton.clickable.clicked += () => InstanceRightContentElement(rootRightContentContainer, MovementStruct.Jog, locomotionMap, true, overrideLayer);
    }
    private void RemoveOverrideLayer(VisualElement container, VisualElement overrideLayerInstance, LocomotionMap locomotionMap, LocomotionOverrideLayer overrideLayer)
    {
        container.Remove(overrideLayerInstance);
        m_OverrideLayerTemplates.Remove(overrideLayerInstance);
        locomotionMap.movement.overrideLayers.Remove(overrideLayer);
    }

    private void InstanceRightContentElement(VisualElement rightContainer, MovementStruct movementStructType, LocomotionMap locomotionMap, bool isOverride = false, LocomotionOverrideLayer overrideLayer = default)
    {
        var rightInstance = movementStructType == MovementStruct.General ? LocomotionGeneralsRightTemplate.CloneTree() : LocomotionMovementRightTemplate.CloneTree();

        VisualElement LoopFour_Element = null;
        VisualElement LoopEight_Element = null;

        if (movementStructType != MovementStruct.General)
        {
            #region Fields

            // Generals
            VisualElement Start_ContentElement = FindElementInRoot<VisualElement>(rightInstance, "start-cardinals-content");
            VisualElement Stop_ContentElement = FindElementInRoot<VisualElement>(rightInstance, "stop-cardinals-content");
            VisualElement LoopFour_ContentElement = FindElementInRoot<VisualElement>(rightInstance, "four-loop-cardinals-content");
            VisualElement LoopEight_ContentElement = FindElementInRoot<VisualElement>(rightInstance, "eight-loop-cardinals-content");
            VisualElement Pivots_ContentElement = FindElementInRoot<VisualElement>(rightInstance, "pivot-cardinals-content");
            VisualElement TurnInPlace_ContentElement = FindElementInRoot<VisualElement>(rightInstance, "turn-in-place-content");
            VisualElement JumpMotion_ContentElement = FindElementInRoot<VisualElement>(rightInstance, "jump-motion-content");
            VisualElement JumpConfig_ContentElement = FindElementInRoot<VisualElement>(rightInstance, "jump-config-content");

            LoopFour_Element = FindElementInRoot<VisualElement>(rightInstance, "four-loop-cardinals-container");
            LoopEight_Element = FindElementInRoot<VisualElement>(rightInstance, "eight-loop-cardinals-container");
            ShowCorrectLoopMotionElement(locomotionMap.useEightDirectional, LoopEight_Element, LoopFour_Element);

            VisualElement Pivots_Element = FindElementInRoot<VisualElement>(rightInstance, "pivot-cardinals-container");
            VisualElement TurnInPlace_Element = FindElementInRoot<VisualElement>(rightInstance, "turn-in-place-container");
            VisualElement JumpMotion_Element = FindElementInRoot<VisualElement>(rightInstance, "jump-motion-container");
            VisualElement JumpConfig_Element = FindElementInRoot<VisualElement>(rightInstance, "jump-config-container");

            // Motion Speed 
            FloatField motionspeed = FindElementInRoot<FloatField>(rightInstance, "motion-speed-field");

            // Start
            ObjectField Start_Forward = FindElementInRoot<ObjectField>(Start_ContentElement, "forward-motion-field");
            ObjectField Start_Backward = FindElementInRoot<ObjectField>(Start_ContentElement, "backward-motion-field");
            ObjectField Start_Left = FindElementInRoot<ObjectField>(Start_ContentElement, "left-motion-field");
            ObjectField Start_Right = FindElementInRoot<ObjectField>(Start_ContentElement, "right-motion-field");
            AddObjectFieldManipulators(Start_Forward);
            AddObjectFieldManipulators(Start_Backward);
            AddObjectFieldManipulators(Start_Left);
            AddObjectFieldManipulators(Start_Right);

            // Stop
            ObjectField Stop_Forward = FindElementInRoot<ObjectField>(Stop_ContentElement, "forward-motion-field");
            ObjectField Stop_Backward = FindElementInRoot<ObjectField>(Stop_ContentElement, "backward-motion-field");
            ObjectField Stop_Left = FindElementInRoot<ObjectField>(Stop_ContentElement, "left-motion-field");
            ObjectField Stop_Right = FindElementInRoot<ObjectField>(Stop_ContentElement, "right-motion-field");
            AddObjectFieldManipulators(Stop_Forward);
            AddObjectFieldManipulators(Stop_Backward);
            AddObjectFieldManipulators(Stop_Left);
            AddObjectFieldManipulators(Stop_Right);

            // Loop Four
            ObjectField LoopFour_Forward = FindElementInRoot<ObjectField>(LoopFour_ContentElement, "forward-motion-field");
            ObjectField LoopFour_Backward = FindElementInRoot<ObjectField>(LoopFour_ContentElement, "backward-motion-field");
            ObjectField LoopFour_Left = FindElementInRoot<ObjectField>(LoopFour_ContentElement, "left-motion-field");
            ObjectField LoopFour_Right = FindElementInRoot<ObjectField>(LoopFour_ContentElement, "right-motion-field");
            AddObjectFieldManipulators(LoopFour_Forward);
            AddObjectFieldManipulators(LoopFour_Backward);
            AddObjectFieldManipulators(LoopFour_Left);
            AddObjectFieldManipulators(LoopFour_Right);

            // Loop Eight
            ObjectField LoopEight_Forward = FindElementInRoot<ObjectField>(LoopEight_ContentElement, "forward-motion-field");
            ObjectField LoopEight_Backward = FindElementInRoot<ObjectField>(LoopEight_ContentElement, "backward-motion-field");
            ObjectField LoopEight_Left = FindElementInRoot<ObjectField>(LoopEight_ContentElement, "left-motion-field");
            ObjectField LoopEight_Right = FindElementInRoot<ObjectField>(LoopEight_ContentElement, "right-motion-field");
            ObjectField LoopEight_Forward_Right = FindElementInRoot<ObjectField>(LoopEight_ContentElement, "forward-right-motion-field");
            ObjectField LoopEight_Forward_Left = FindElementInRoot<ObjectField>(LoopEight_ContentElement, "forward-left-motion-field");
            ObjectField LoopEight_Backward_Right = FindElementInRoot<ObjectField>(LoopEight_ContentElement, "backward-right-motion-field");
            ObjectField LoopEight_Backward_Left = FindElementInRoot<ObjectField>(LoopEight_ContentElement, "backward-left-motion-field");
            AddObjectFieldManipulators(LoopEight_Forward);
            AddObjectFieldManipulators(LoopEight_Backward);
            AddObjectFieldManipulators(LoopEight_Left);
            AddObjectFieldManipulators(LoopEight_Right);
            AddObjectFieldManipulators(LoopEight_Forward_Right);
            AddObjectFieldManipulators(LoopEight_Forward_Left);
            AddObjectFieldManipulators(LoopEight_Backward_Right);
            AddObjectFieldManipulators(LoopEight_Backward_Left);

            // Pivots
            ObjectField Pivot_Forward = FindElementInRoot<ObjectField>(Pivots_ContentElement, "forward-motion-field");
            ObjectField Pivot_Backward = FindElementInRoot<ObjectField>(Pivots_ContentElement, "backward-motion-field");
            ObjectField Pivot_Left = FindElementInRoot<ObjectField>(Pivots_ContentElement, "left-motion-field");
            ObjectField Pivot_Right = FindElementInRoot<ObjectField>(Pivots_ContentElement, "right-motion-field");
            AddObjectFieldManipulators(Pivot_Forward);
            AddObjectFieldManipulators(Pivot_Backward);
            AddObjectFieldManipulators(Pivot_Left);
            AddObjectFieldManipulators(Pivot_Right);

            // Turn In Place
            ObjectField Turn45_Right = FindElementInRoot<ObjectField>(TurnInPlace_ContentElement, "right-45-motion-field");
            ObjectField Turn90_Right = FindElementInRoot<ObjectField>(TurnInPlace_ContentElement, "right-90-motion-field");
            ObjectField Turn135_Right = FindElementInRoot<ObjectField>(TurnInPlace_ContentElement, "right-135-motion-field");
            ObjectField Turn180_Right = FindElementInRoot<ObjectField>(TurnInPlace_ContentElement, "right-180-motion-field");
            ObjectField Turn45_Left = FindElementInRoot<ObjectField>(TurnInPlace_ContentElement, "left-45-motion-field");
            ObjectField Turn90_Left = FindElementInRoot<ObjectField>(TurnInPlace_ContentElement, "left-90-motion-field");
            ObjectField Turn135_Left = FindElementInRoot<ObjectField>(TurnInPlace_ContentElement, "left-135-motion-field");
            ObjectField Turn180_Left = FindElementInRoot<ObjectField>(TurnInPlace_ContentElement, "left-180-motion-field");
            AddObjectFieldManipulators(Turn45_Right);
            AddObjectFieldManipulators(Turn90_Right);
            AddObjectFieldManipulators(Turn135_Right);
            AddObjectFieldManipulators(Turn180_Right);
            AddObjectFieldManipulators(Turn45_Left);
            AddObjectFieldManipulators(Turn90_Left);
            AddObjectFieldManipulators(Turn135_Left);
            AddObjectFieldManipulators(Turn180_Left);

            // Jump Motion
            ObjectField Jump_Start = FindElementInRoot<ObjectField>(JumpMotion_ContentElement, "jump-start-motion-field");
            ObjectField Jump_Loop = FindElementInRoot<ObjectField>(JumpMotion_ContentElement, "jump-loop-motion-field");
            ObjectField Jump_Land = FindElementInRoot<ObjectField>(JumpMotion_ContentElement, "jump-land-motion-field");
            AddObjectFieldManipulators(Jump_Start);
            AddObjectFieldManipulators(Jump_Loop);
            AddObjectFieldManipulators(Jump_Land);


            // Jump Config
            FloatField Jump_Height = FindElementInRoot<FloatField>(JumpConfig_ContentElement, "jump-height-field");
            FloatField Jump_Gravity = FindElementInRoot<FloatField>(JumpConfig_ContentElement, "jump-gravity-field");
            FloatField Jump_Timeout = FindElementInRoot<FloatField>(JumpConfig_ContentElement, "jump-timeout-field");
            FloatField Jump_Fall_Timeout = FindElementInRoot<FloatField>(JumpConfig_ContentElement, "jump-fall-timeout-field");
            #endregion

            if (!isOverride)
            {
                SetElementDisplay(Pivots_Element, true);
                SetElementDisplay(TurnInPlace_Element, true);
                SetElementDisplay(JumpMotion_Element, true);
                SetElementDisplay(JumpConfig_Element, true);

                switch (movementStructType)
                {
                    case MovementStruct.Walk:
                        #region General
                        motionspeed.value = locomotionMap.movement.walk.motionSpeed;
                        motionspeed.RegisterValueChangedCallback((evt) =>
                        {
                            var structure = locomotionMap.movement.walk;
                            structure.motionSpeed = evt.newValue;
                            locomotionMap.movement.walk = structure;
                            SetEndColorToSaveButton();
                        });
                        #endregion

                        #region Start
                        Start_Forward.value = locomotionMap.movement.walk.startCardinals.forward;
                        Start_Forward.RegisterValueChangedCallback((evt) =>
                        {
                            var structure = locomotionMap.movement.walk.startCardinals.forward;
                            structure = evt.newValue as AnimationClip;
                            locomotionMap.movement.walk.startCardinals.forward = structure;
                            SetEndColorToSaveButton();
                        });

                        Start_Backward.value = locomotionMap.movement.walk.startCardinals.backward;
                        Start_Backward.RegisterValueChangedCallback((evt) =>
                        {
                            var structure = locomotionMap.movement.walk.startCardinals.backward;
                            structure = evt.newValue as AnimationClip;
                            locomotionMap.movement.walk.startCardinals.backward = structure;
                            SetEndColorToSaveButton();
                        });

                        Start_Left.value = locomotionMap.movement.walk.startCardinals.left;
                        Start_Left.RegisterValueChangedCallback((evt) =>
                        {
                            var structure = locomotionMap.movement.walk.startCardinals.left;
                            structure = evt.newValue as AnimationClip;
                            locomotionMap.movement.walk.startCardinals.left = structure;
                            SetEndColorToSaveButton();
                        });

                        Start_Right.value = locomotionMap.movement.walk.startCardinals.right;
                        Start_Right.RegisterValueChangedCallback((evt) =>
                        {
                            var structure = locomotionMap.movement.walk.startCardinals.right;
                            structure = evt.newValue as AnimationClip;
                            locomotionMap.movement.walk.startCardinals.right = structure;
                            SetEndColorToSaveButton();
                        });
                        #endregion

                        #region Stop
                        Stop_Forward.value = locomotionMap.movement.walk.stopCardinals.forward;
                        Stop_Forward.RegisterValueChangedCallback((evt) =>
                        {
                            var structure = locomotionMap.movement.walk.stopCardinals.forward;
                            structure = evt.newValue as AnimationClip;
                            locomotionMap.movement.walk.stopCardinals.forward = structure;
                            SetEndColorToSaveButton();
                        });

                        Stop_Backward.value = locomotionMap.movement.walk.stopCardinals.backward;
                        Stop_Backward.RegisterValueChangedCallback((evt) =>
                        {
                            var structure = locomotionMap.movement.walk.stopCardinals.backward;
                            structure = evt.newValue as AnimationClip;
                            locomotionMap.movement.walk.stopCardinals.backward = structure;
                            SetEndColorToSaveButton();
                        });

                        Stop_Left.value = locomotionMap.movement.walk.stopCardinals.left;
                        Stop_Left.RegisterValueChangedCallback((evt) =>
                        {
                            var structure = locomotionMap.movement.walk.stopCardinals.left;
                            structure = evt.newValue as AnimationClip;
                            locomotionMap.movement.walk.stopCardinals.left = structure;
                            SetEndColorToSaveButton();
                        });

                        Stop_Right.value = locomotionMap.movement.walk.stopCardinals.right;
                        Stop_Right.RegisterValueChangedCallback((evt) =>
                        {
                            var structure = locomotionMap.movement.walk.stopCardinals.right;
                            structure = evt.newValue as AnimationClip;
                            locomotionMap.movement.walk.stopCardinals.right = structure;
                            SetEndColorToSaveButton();
                        });
                        #endregion

                        #region Loop Four
                        LoopFour_Forward.value = locomotionMap.movement.walk.loopCardinalsFour.forward;
                        LoopFour_Forward.RegisterValueChangedCallback((evt) =>
                        {
                            var structure = locomotionMap.movement.walk.loopCardinalsFour.forward;
                            structure = evt.newValue as AnimationClip;
                            locomotionMap.movement.walk.loopCardinalsFour.forward = structure;
                            SetEndColorToSaveButton();
                        });

                        LoopFour_Backward.value = locomotionMap.movement.walk.loopCardinalsFour.backward;
                        LoopFour_Backward.RegisterValueChangedCallback((evt) =>
                        {
                            var structure = locomotionMap.movement.walk.loopCardinalsFour.backward;
                            structure = evt.newValue as AnimationClip;
                            locomotionMap.movement.walk.loopCardinalsFour.backward = structure;
                            SetEndColorToSaveButton();
                        });

                        LoopFour_Left.value = locomotionMap.movement.walk.loopCardinalsFour.left;
                        LoopFour_Left.RegisterValueChangedCallback((evt) =>
                        {
                            var structure = locomotionMap.movement.walk.loopCardinalsFour.left;
                            structure = evt.newValue as AnimationClip;
                            locomotionMap.movement.walk.loopCardinalsFour.left = structure;
                            SetEndColorToSaveButton();
                        });

                        LoopFour_Right.value = locomotionMap.movement.walk.loopCardinalsFour.right;
                        LoopFour_Right.RegisterValueChangedCallback((evt) =>
                        {
                            var structure = locomotionMap.movement.walk.loopCardinalsFour.right;
                            structure = evt.newValue as AnimationClip;
                            locomotionMap.movement.walk.loopCardinalsFour.right = structure;
                            SetEndColorToSaveButton();
                        });
                        #endregion

                        #region Loop Eight
                        LoopEight_Forward.value = locomotionMap.movement.walk.loopCardinalsEight.forward;
                        LoopEight_Forward.RegisterValueChangedCallback((evt) =>
                        {
                            var structure = locomotionMap.movement.walk.loopCardinalsEight.forward;
                            structure = evt.newValue as AnimationClip;
                            locomotionMap.movement.walk.loopCardinalsEight.forward = structure;
                            SetEndColorToSaveButton();
                        });

                        LoopEight_Backward.value = locomotionMap.movement.walk.loopCardinalsEight.backward;
                        LoopEight_Backward.RegisterValueChangedCallback((evt) =>
                        {
                            var structure = locomotionMap.movement.walk.loopCardinalsEight.backward;
                            structure = evt.newValue as AnimationClip;
                            locomotionMap.movement.walk.loopCardinalsEight.backward = structure;
                            SetEndColorToSaveButton();
                        });

                        LoopEight_Left.value = locomotionMap.movement.walk.loopCardinalsEight.left;
                        LoopEight_Left.RegisterValueChangedCallback((evt) =>
                        {
                            var structure = locomotionMap.movement.walk.loopCardinalsEight.left;
                            structure = evt.newValue as AnimationClip;
                            locomotionMap.movement.walk.loopCardinalsEight.left = structure;
                            SetEndColorToSaveButton();
                        });

                        LoopEight_Right.value = locomotionMap.movement.walk.loopCardinalsEight.right;
                        LoopEight_Right.RegisterValueChangedCallback((evt) =>
                        {
                            var structure = locomotionMap.movement.walk.loopCardinalsEight.right;
                            structure = evt.newValue as AnimationClip;
                            locomotionMap.movement.walk.loopCardinalsEight.right = structure;
                            SetEndColorToSaveButton();
                        });

                        LoopEight_Forward_Right.value = locomotionMap.movement.walk.loopCardinalsEight.forwardRight;
                        LoopEight_Forward_Right.RegisterValueChangedCallback((evt) =>
                        {
                            var structure = locomotionMap.movement.walk.loopCardinalsEight.forwardRight;
                            structure = evt.newValue as AnimationClip;
                            locomotionMap.movement.walk.loopCardinalsEight.forwardRight = structure;
                            SetEndColorToSaveButton();
                        });

                        LoopEight_Forward_Left.value = locomotionMap.movement.walk.loopCardinalsEight.forwardLeft;
                        LoopEight_Forward_Left.RegisterValueChangedCallback((evt) =>
                        {
                            var structure = locomotionMap.movement.walk.loopCardinalsEight.forwardLeft;
                            structure = evt.newValue as AnimationClip;
                            locomotionMap.movement.walk.loopCardinalsEight.forwardLeft = structure;
                            SetEndColorToSaveButton();
                        });

                        LoopEight_Backward_Right.value = locomotionMap.movement.walk.loopCardinalsEight.backwardRight;
                        LoopEight_Backward_Right.RegisterValueChangedCallback((evt) =>
                        {
                            var structure = locomotionMap.movement.walk.loopCardinalsEight.backwardRight;
                            structure = evt.newValue as AnimationClip;
                            locomotionMap.movement.walk.loopCardinalsEight.backwardRight = structure;
                            SetEndColorToSaveButton();
                        });

                        LoopEight_Backward_Left.value = locomotionMap.movement.walk.loopCardinalsEight.backwardLeft;
                        LoopEight_Backward_Left.RegisterValueChangedCallback((evt) =>
                        {
                            var structure = locomotionMap.movement.walk.loopCardinalsEight.backwardLeft;
                            structure = evt.newValue as AnimationClip;
                            locomotionMap.movement.walk.loopCardinalsEight.backwardLeft = structure;
                            SetEndColorToSaveButton();
                        });
                        #endregion

                        #region Pivots
                        Pivot_Forward.value = locomotionMap.movement.walk.pivotCardinals.forward;
                        Pivot_Forward.RegisterValueChangedCallback((evt) =>
                        {
                            var structure = locomotionMap.movement.walk.pivotCardinals.forward;
                            structure = evt.newValue as AnimationClip;
                            locomotionMap.movement.walk.pivotCardinals.forward = structure;
                            SetEndColorToSaveButton();
                        });

                        Pivot_Backward.value = locomotionMap.movement.walk.pivotCardinals.backward;
                        LoopFour_Backward.RegisterValueChangedCallback((evt) =>
                        {
                            var structure = locomotionMap.movement.walk.pivotCardinals.backward;
                            structure = evt.newValue as AnimationClip;
                            locomotionMap.movement.walk.pivotCardinals.backward = structure;
                            SetEndColorToSaveButton();
                        });

                        Pivot_Left.value = locomotionMap.movement.walk.pivotCardinals.left;
                        Pivot_Left.RegisterValueChangedCallback((evt) =>
                        {
                            var structure = locomotionMap.movement.walk.pivotCardinals.left;
                            structure = evt.newValue as AnimationClip;
                            locomotionMap.movement.walk.pivotCardinals.left = structure;
                            SetEndColorToSaveButton();
                        });

                        Pivot_Right.value = locomotionMap.movement.walk.pivotCardinals.right;
                        Pivot_Right.RegisterValueChangedCallback((evt) =>
                        {
                            var structure = locomotionMap.movement.walk.pivotCardinals.right;
                            structure = evt.newValue as AnimationClip;
                            locomotionMap.movement.walk.pivotCardinals.right = structure;
                            SetEndColorToSaveButton();
                        });
                        #endregion

                        #region Turn In Place
                        Turn45_Right.value = locomotionMap.movement.walk.turnInPlace.right45;
                        Turn45_Right.RegisterValueChangedCallback((evt) =>
                        {
                            var structure = locomotionMap.movement.walk.turnInPlace.right45;
                            structure = evt.newValue as AnimationClip;
                            locomotionMap.movement.walk.turnInPlace.right45 = structure;
                            SetEndColorToSaveButton();
                        });

                        Turn90_Right.value = locomotionMap.movement.walk.turnInPlace.right90;
                        Turn90_Right.RegisterValueChangedCallback((evt) =>
                        {
                            var structure = locomotionMap.movement.walk.turnInPlace.right90;
                            structure = evt.newValue as AnimationClip;
                            locomotionMap.movement.walk.turnInPlace.right90 = structure;
                            SetEndColorToSaveButton();
                        });

                        Turn135_Right.value = locomotionMap.movement.walk.turnInPlace.right135;
                        Turn135_Right.RegisterValueChangedCallback((evt) =>
                        {
                            var structure = locomotionMap.movement.walk.turnInPlace.right135;
                            structure = evt.newValue as AnimationClip;
                            locomotionMap.movement.walk.turnInPlace.right135 = structure;
                            SetEndColorToSaveButton();
                        });

                        Turn180_Right.value = locomotionMap.movement.walk.turnInPlace.right180;
                        Turn180_Right.RegisterValueChangedCallback((evt) =>
                        {
                            var structure = locomotionMap.movement.walk.turnInPlace.right180;
                            structure = evt.newValue as AnimationClip;
                            locomotionMap.movement.walk.turnInPlace.right180 = structure;
                            SetEndColorToSaveButton();
                        });

                        Turn45_Left.value = locomotionMap.movement.walk.turnInPlace.left45;
                        Turn45_Left.RegisterValueChangedCallback((evt) =>
                        {
                            var structure = locomotionMap.movement.walk.turnInPlace.left45;
                            structure = evt.newValue as AnimationClip;
                            locomotionMap.movement.walk.turnInPlace.left45 = structure;
                            SetEndColorToSaveButton();
                        });

                        Turn90_Left.value = locomotionMap.movement.walk.turnInPlace.left90;
                        Turn90_Left.RegisterValueChangedCallback((evt) =>
                        {
                            var structure = locomotionMap.movement.walk.turnInPlace.left90;
                            structure = evt.newValue as AnimationClip;
                            locomotionMap.movement.walk.turnInPlace.left90 = structure;
                            SetEndColorToSaveButton();
                        });

                        Turn135_Left.value = locomotionMap.movement.walk.turnInPlace.left135;
                        Turn135_Left.RegisterValueChangedCallback((evt) =>
                        {
                            var structure = locomotionMap.movement.walk.turnInPlace.left135;
                            structure = evt.newValue as AnimationClip;
                            locomotionMap.movement.walk.turnInPlace.left135 = structure;
                            SetEndColorToSaveButton();
                        });

                        Turn180_Left.value = locomotionMap.movement.walk.turnInPlace.left180;
                        Turn180_Left.RegisterValueChangedCallback((evt) =>
                        {
                            var structure = locomotionMap.movement.walk.turnInPlace.left180;
                            structure = evt.newValue as AnimationClip;
                            locomotionMap.movement.walk.turnInPlace.left180 = structure;
                            SetEndColorToSaveButton();
                        });
                        #endregion

                        #region Jump Motion
                        Jump_Start.value = locomotionMap.movement.walk.jump.start;
                        Jump_Start.RegisterValueChangedCallback((evt) =>
                        {
                            var structure = locomotionMap.movement.walk.jump.start;
                            structure = evt.newValue as AnimationClip;
                            locomotionMap.movement.walk.jump.start = structure;
                            SetEndColorToSaveButton();
                        });

                        Jump_Loop.value = locomotionMap.movement.walk.jump.loop;
                        Jump_Loop.RegisterValueChangedCallback((evt) =>
                        {
                            var structure = locomotionMap.movement.walk.jump.loop;
                            structure = evt.newValue as AnimationClip;
                            locomotionMap.movement.walk.jump.loop = structure;
                            SetEndColorToSaveButton();
                        });

                        Jump_Land.value = locomotionMap.movement.walk.jump.end;
                        Jump_Land.RegisterValueChangedCallback((evt) =>
                        {
                            var structure = locomotionMap.movement.walk.jump.end;
                            structure = evt.newValue as AnimationClip;
                            locomotionMap.movement.walk.jump.end = structure;
                            SetEndColorToSaveButton();
                        });
                        #endregion

                        #region Jump Config
                        Jump_Height.value = locomotionMap.movement.walk.jumpConfig.jumpHeight;
                        Jump_Height.RegisterValueChangedCallback((evt) =>
                        {
                            var structure = locomotionMap.movement.walk.jumpConfig.jumpHeight;
                            structure = evt.newValue;
                            locomotionMap.movement.walk.jumpConfig.jumpHeight = structure;
                            SetEndColorToSaveButton();
                        });

                        Jump_Gravity.value = locomotionMap.movement.walk.jumpConfig.gravity;
                        Jump_Gravity.RegisterValueChangedCallback((evt) =>
                        {
                            var structure = locomotionMap.movement.walk.jumpConfig.gravity;
                            structure = evt.newValue;
                            locomotionMap.movement.walk.jumpConfig.gravity = structure;
                            SetEndColorToSaveButton();
                        });

                        Jump_Timeout.value = locomotionMap.movement.walk.jumpConfig.jumpTimeout;
                        Jump_Timeout.RegisterValueChangedCallback((evt) =>
                        {
                            var structure = locomotionMap.movement.walk.jumpConfig.jumpTimeout;
                            structure = evt.newValue;
                            locomotionMap.movement.walk.jumpConfig.jumpTimeout = structure;
                            SetEndColorToSaveButton();
                        });

                        Jump_Fall_Timeout.value = locomotionMap.movement.walk.jumpConfig.fallTimeout;
                        Jump_Fall_Timeout.RegisterValueChangedCallback((evt) =>
                        {
                            var structure = locomotionMap.movement.walk.jumpConfig.fallTimeout;
                            structure = evt.newValue;
                            locomotionMap.movement.walk.jumpConfig.fallTimeout = structure;
                            SetEndColorToSaveButton();
                        });
                        #endregion
                        break;

                    case MovementStruct.Jog:
                        #region General
                        motionspeed.value = locomotionMap.movement.jog.motionSpeed;
                        motionspeed.RegisterValueChangedCallback((evt) =>
                        {
                            var structure = locomotionMap.movement.jog;
                            structure.motionSpeed = evt.newValue;
                            locomotionMap.movement.jog = structure;
                            SetEndColorToSaveButton();
                        });
                        #endregion

                        #region Start
                        Start_Forward.value = locomotionMap.movement.jog.startCardinals.forward;
                        Start_Forward.RegisterValueChangedCallback((evt) =>
                        {
                            var structure = locomotionMap.movement.jog.startCardinals.forward;
                            structure = evt.newValue as AnimationClip;
                            locomotionMap.movement.jog.startCardinals.forward = structure;
                            SetEndColorToSaveButton();
                        });

                        Start_Backward.value = locomotionMap.movement.jog.startCardinals.backward;
                        Start_Backward.RegisterValueChangedCallback((evt) =>
                        {
                            var structure = locomotionMap.movement.jog.startCardinals.backward;
                            structure = evt.newValue as AnimationClip;
                            locomotionMap.movement.jog.startCardinals.backward = structure;
                            SetEndColorToSaveButton();
                        });

                        Start_Left.value = locomotionMap.movement.jog.startCardinals.left;
                        Start_Left.RegisterValueChangedCallback((evt) =>
                        {
                            var structure = locomotionMap.movement.jog.startCardinals.left;
                            structure = evt.newValue as AnimationClip;
                            locomotionMap.movement.jog.startCardinals.left = structure;
                            SetEndColorToSaveButton();
                        });

                        Start_Right.value = locomotionMap.movement.jog.startCardinals.right;
                        Start_Right.RegisterValueChangedCallback((evt) =>
                        {
                            var structure = locomotionMap.movement.jog.startCardinals.right;
                            structure = evt.newValue as AnimationClip;
                            locomotionMap.movement.jog.startCardinals.right = structure;
                            SetEndColorToSaveButton();
                        });
                        #endregion

                        #region Stop
                        Stop_Forward.value = locomotionMap.movement.jog.stopCardinals.forward;
                        Stop_Forward.RegisterValueChangedCallback((evt) =>
                        {
                            var structure = locomotionMap.movement.jog.stopCardinals.forward;
                            structure = evt.newValue as AnimationClip;
                            locomotionMap.movement.jog.stopCardinals.forward = structure;
                            SetEndColorToSaveButton();
                        });

                        Stop_Backward.value = locomotionMap.movement.jog.stopCardinals.backward;
                        Stop_Backward.RegisterValueChangedCallback((evt) =>
                        {
                            var structure = locomotionMap.movement.jog.stopCardinals.backward;
                            structure = evt.newValue as AnimationClip;
                            locomotionMap.movement.jog.stopCardinals.backward = structure;
                            SetEndColorToSaveButton();
                        });

                        Stop_Left.value = locomotionMap.movement.jog.stopCardinals.left;
                        Stop_Left.RegisterValueChangedCallback((evt) =>
                        {
                            var structure = locomotionMap.movement.jog.stopCardinals.left;
                            structure = evt.newValue as AnimationClip;
                            locomotionMap.movement.jog.stopCardinals.left = structure;
                            SetEndColorToSaveButton();
                        });

                        Stop_Right.value = locomotionMap.movement.jog.stopCardinals.right;
                        Stop_Right.RegisterValueChangedCallback((evt) =>
                        {
                            var structure = locomotionMap.movement.jog.stopCardinals.right;
                            structure = evt.newValue as AnimationClip;
                            locomotionMap.movement.jog.stopCardinals.right = structure;
                            SetEndColorToSaveButton();
                        });
                        #endregion

                        #region Loop Four
                        LoopFour_Forward.value = locomotionMap.movement.jog.loopCardinalsFour.forward;
                        LoopFour_Forward.RegisterValueChangedCallback((evt) =>
                        {
                            var structure = locomotionMap.movement.jog.loopCardinalsFour.forward;
                            structure = evt.newValue as AnimationClip;
                            locomotionMap.movement.jog.loopCardinalsFour.forward = structure;
                            SetEndColorToSaveButton();
                        });

                        LoopFour_Backward.value = locomotionMap.movement.jog.loopCardinalsFour.backward;
                        LoopFour_Backward.RegisterValueChangedCallback((evt) =>
                        {
                            var structure = locomotionMap.movement.jog.loopCardinalsFour.backward;
                            structure = evt.newValue as AnimationClip;
                            locomotionMap.movement.jog.loopCardinalsFour.backward = structure;
                            SetEndColorToSaveButton();
                        });

                        LoopFour_Left.value = locomotionMap.movement.jog.loopCardinalsFour.left;
                        LoopFour_Left.RegisterValueChangedCallback((evt) =>
                        {
                            var structure = locomotionMap.movement.jog.loopCardinalsFour.left;
                            structure = evt.newValue as AnimationClip;
                            locomotionMap.movement.jog.loopCardinalsFour.left = structure;
                            SetEndColorToSaveButton();
                        });

                        LoopFour_Right.value = locomotionMap.movement.jog.loopCardinalsFour.right;
                        LoopFour_Right.RegisterValueChangedCallback((evt) =>
                        {
                            var structure = locomotionMap.movement.jog.loopCardinalsFour.right;
                            structure = evt.newValue as AnimationClip;
                            locomotionMap.movement.jog.loopCardinalsFour.right = structure;
                            SetEndColorToSaveButton();
                        });
                        #endregion

                        #region Loop Eight
                        LoopEight_Forward.value = locomotionMap.movement.jog.loopCardinalsEight.forward;
                        LoopEight_Forward.RegisterValueChangedCallback((evt) =>
                        {
                            var structure = locomotionMap.movement.jog.loopCardinalsEight.forward;
                            structure = evt.newValue as AnimationClip;
                            locomotionMap.movement.jog.loopCardinalsEight.forward = structure;
                            SetEndColorToSaveButton();
                        });

                        LoopEight_Backward.value = locomotionMap.movement.jog.loopCardinalsEight.backward;
                        LoopEight_Backward.RegisterValueChangedCallback((evt) =>
                        {
                            var structure = locomotionMap.movement.jog.loopCardinalsEight.backward;
                            structure = evt.newValue as AnimationClip;
                            locomotionMap.movement.jog.loopCardinalsEight.backward = structure;
                            SetEndColorToSaveButton();
                        });

                        LoopEight_Left.value = locomotionMap.movement.jog.loopCardinalsEight.left;
                        LoopEight_Left.RegisterValueChangedCallback((evt) =>
                        {
                            var structure = locomotionMap.movement.jog.loopCardinalsEight.left;
                            structure = evt.newValue as AnimationClip;
                            locomotionMap.movement.jog.loopCardinalsEight.left = structure;
                            SetEndColorToSaveButton();
                        });

                        LoopEight_Right.value = locomotionMap.movement.jog.loopCardinalsEight.right;
                        LoopEight_Right.RegisterValueChangedCallback((evt) =>
                        {
                            var structure = locomotionMap.movement.jog.loopCardinalsEight.right;
                            structure = evt.newValue as AnimationClip;
                            locomotionMap.movement.jog.loopCardinalsEight.right = structure;
                            SetEndColorToSaveButton();
                        });

                        LoopEight_Forward_Right.value = locomotionMap.movement.jog.loopCardinalsEight.forwardRight;
                        LoopEight_Forward_Right.RegisterValueChangedCallback((evt) =>
                        {
                            var structure = locomotionMap.movement.jog.loopCardinalsEight.forwardRight;
                            structure = evt.newValue as AnimationClip;
                            locomotionMap.movement.jog.loopCardinalsEight.forwardRight = structure;
                            SetEndColorToSaveButton();
                        });

                        LoopEight_Forward_Left.value = locomotionMap.movement.jog.loopCardinalsEight.forwardLeft;
                        LoopEight_Forward_Left.RegisterValueChangedCallback((evt) =>
                        {
                            var structure = locomotionMap.movement.jog.loopCardinalsEight.forwardLeft;
                            structure = evt.newValue as AnimationClip;
                            locomotionMap.movement.jog.loopCardinalsEight.forwardLeft = structure;
                            SetEndColorToSaveButton();
                        });

                        LoopEight_Backward_Right.value = locomotionMap.movement.jog.loopCardinalsEight.backwardRight;
                        LoopEight_Backward_Right.RegisterValueChangedCallback((evt) =>
                        {
                            var structure = locomotionMap.movement.jog.loopCardinalsEight.backwardRight;
                            structure = evt.newValue as AnimationClip;
                            locomotionMap.movement.jog.loopCardinalsEight.backwardRight = structure;
                            SetEndColorToSaveButton();
                        });

                        LoopEight_Backward_Left.value = locomotionMap.movement.jog.loopCardinalsEight.backwardLeft;
                        LoopEight_Backward_Left.RegisterValueChangedCallback((evt) =>
                        {
                            var structure = locomotionMap.movement.jog.loopCardinalsEight.backwardLeft;
                            structure = evt.newValue as AnimationClip;
                            locomotionMap.movement.jog.loopCardinalsEight.backwardLeft = structure;
                            SetEndColorToSaveButton();
                        });
                        #endregion

                        #region Pivots
                        Pivot_Forward.value = locomotionMap.movement.jog.pivotCardinals.forward;
                        Pivot_Forward.RegisterValueChangedCallback((evt) =>
                        {
                            var structure = locomotionMap.movement.jog.pivotCardinals.forward;
                            structure = evt.newValue as AnimationClip;
                            locomotionMap.movement.jog.pivotCardinals.forward = structure;
                            SetEndColorToSaveButton();
                        });

                        Pivot_Backward.value = locomotionMap.movement.jog.pivotCardinals.backward;
                        LoopFour_Backward.RegisterValueChangedCallback((evt) =>
                        {
                            var structure = locomotionMap.movement.jog.pivotCardinals.backward;
                            structure = evt.newValue as AnimationClip;
                            locomotionMap.movement.jog.pivotCardinals.backward = structure;
                            SetEndColorToSaveButton();
                        });

                        Pivot_Left.value = locomotionMap.movement.jog.pivotCardinals.left;
                        Pivot_Left.RegisterValueChangedCallback((evt) =>
                        {
                            var structure = locomotionMap.movement.jog.pivotCardinals.left;
                            structure = evt.newValue as AnimationClip;
                            locomotionMap.movement.jog.pivotCardinals.left = structure;
                            SetEndColorToSaveButton();
                        });

                        Pivot_Right.value = locomotionMap.movement.jog.pivotCardinals.right;
                        Pivot_Right.RegisterValueChangedCallback((evt) =>
                        {
                            var structure = locomotionMap.movement.jog.pivotCardinals.right;
                            structure = evt.newValue as AnimationClip;
                            locomotionMap.movement.jog.pivotCardinals.right = structure;
                            SetEndColorToSaveButton();
                        });
                        #endregion

                        #region Turn In Place
                        Turn45_Right.value = locomotionMap.movement.jog.turnInPlace.right45;
                        Turn45_Right.RegisterValueChangedCallback((evt) =>
                        {
                            var structure = locomotionMap.movement.jog.turnInPlace.right45;
                            structure = evt.newValue as AnimationClip;
                            locomotionMap.movement.jog.turnInPlace.right45 = structure;
                            SetEndColorToSaveButton();
                        });

                        Turn90_Right.value = locomotionMap.movement.jog.turnInPlace.right90;
                        Turn90_Right.RegisterValueChangedCallback((evt) =>
                        {
                            var structure = locomotionMap.movement.jog.turnInPlace.right90;
                            structure = evt.newValue as AnimationClip;
                            locomotionMap.movement.jog.turnInPlace.right90 = structure;
                            SetEndColorToSaveButton();
                        });

                        Turn135_Right.value = locomotionMap.movement.jog.turnInPlace.right135;
                        Turn135_Right.RegisterValueChangedCallback((evt) =>
                        {
                            var structure = locomotionMap.movement.jog.turnInPlace.right135;
                            structure = evt.newValue as AnimationClip;
                            locomotionMap.movement.jog.turnInPlace.right135 = structure;
                            SetEndColorToSaveButton();
                        });

                        Turn180_Right.value = locomotionMap.movement.jog.turnInPlace.right180;
                        Turn180_Right.RegisterValueChangedCallback((evt) =>
                        {
                            var structure = locomotionMap.movement.jog.turnInPlace.right180;
                            structure = evt.newValue as AnimationClip;
                            locomotionMap.movement.jog.turnInPlace.right180 = structure;
                            SetEndColorToSaveButton();
                        });

                        Turn45_Left.value = locomotionMap.movement.jog.turnInPlace.left45;
                        Turn45_Left.RegisterValueChangedCallback((evt) =>
                        {
                            var structure = locomotionMap.movement.jog.turnInPlace.left45;
                            structure = evt.newValue as AnimationClip;
                            locomotionMap.movement.jog.turnInPlace.left45 = structure;
                            SetEndColorToSaveButton();
                        });

                        Turn90_Left.value = locomotionMap.movement.jog.turnInPlace.left90;
                        Turn90_Left.RegisterValueChangedCallback((evt) =>
                        {
                            var structure = locomotionMap.movement.jog.turnInPlace.left90;
                            structure = evt.newValue as AnimationClip;
                            locomotionMap.movement.jog.turnInPlace.left90 = structure;
                            SetEndColorToSaveButton();
                        });

                        Turn135_Left.value = locomotionMap.movement.jog.turnInPlace.left135;
                        Turn135_Left.RegisterValueChangedCallback((evt) =>
                        {
                            var structure = locomotionMap.movement.jog.turnInPlace.left135;
                            structure = evt.newValue as AnimationClip;
                            locomotionMap.movement.jog.turnInPlace.left135 = structure;
                            SetEndColorToSaveButton();
                        });

                        Turn180_Left.value = locomotionMap.movement.jog.turnInPlace.left180;
                        Turn180_Left.RegisterValueChangedCallback((evt) =>
                        {
                            var structure = locomotionMap.movement.jog.turnInPlace.left180;
                            structure = evt.newValue as AnimationClip;
                            locomotionMap.movement.jog.turnInPlace.left180 = structure;
                            SetEndColorToSaveButton();
                        });
                        #endregion

                        #region Jump Motion
                        Jump_Start.value = locomotionMap.movement.jog.jump.start;
                        Jump_Start.RegisterValueChangedCallback((evt) =>
                        {
                            var structure = locomotionMap.movement.jog.jump.start;
                            structure = evt.newValue as AnimationClip;
                            locomotionMap.movement.jog.jump.start = structure;
                            SetEndColorToSaveButton();
                        });

                        Jump_Loop.value = locomotionMap.movement.jog.jump.loop;
                        Jump_Loop.RegisterValueChangedCallback((evt) =>
                        {
                            var structure = locomotionMap.movement.jog.jump.loop;
                            structure = evt.newValue as AnimationClip;
                            locomotionMap.movement.jog.jump.loop = structure;
                            SetEndColorToSaveButton();
                        });

                        Jump_Land.value = locomotionMap.movement.jog.jump.end;
                        Jump_Land.RegisterValueChangedCallback((evt) =>
                        {
                            var structure = locomotionMap.movement.jog.jump.end;
                            structure = evt.newValue as AnimationClip;
                            locomotionMap.movement.jog.jump.end = structure;
                            SetEndColorToSaveButton();
                        });
                        #endregion

                        #region Jump Config
                        Jump_Height.value = locomotionMap.movement.jog.jumpConfig.jumpHeight;
                        Jump_Height.RegisterValueChangedCallback((evt) =>
                        {
                            var structure = locomotionMap.movement.jog.jumpConfig.jumpHeight;
                            structure = evt.newValue;
                            locomotionMap.movement.jog.jumpConfig.jumpHeight = structure;
                            SetEndColorToSaveButton();
                        });

                        Jump_Gravity.value = locomotionMap.movement.jog.jumpConfig.gravity;
                        Jump_Gravity.RegisterValueChangedCallback((evt) =>
                        {
                            var structure = locomotionMap.movement.jog.jumpConfig.gravity;
                            structure = evt.newValue;
                            locomotionMap.movement.jog.jumpConfig.gravity = structure;
                            SetEndColorToSaveButton();
                        });

                        Jump_Timeout.value = locomotionMap.movement.jog.jumpConfig.jumpTimeout;
                        Jump_Timeout.RegisterValueChangedCallback((evt) =>
                        {
                            var structure = locomotionMap.movement.jog.jumpConfig.jumpTimeout;
                            structure = evt.newValue;
                            locomotionMap.movement.jog.jumpConfig.jumpTimeout = structure;
                            SetEndColorToSaveButton();
                        });

                        Jump_Fall_Timeout.value = locomotionMap.movement.jog.jumpConfig.fallTimeout;
                        Jump_Fall_Timeout.RegisterValueChangedCallback((evt) =>
                        {
                            var structure = locomotionMap.movement.jog.jumpConfig.fallTimeout;
                            structure = evt.newValue;
                            locomotionMap.movement.jog.jumpConfig.fallTimeout = structure;
                            SetEndColorToSaveButton();
                        });
                        #endregion
                        break;

                    case MovementStruct.Crouch:
                        SetElementDisplay(JumpMotion_Element, false);
                        SetElementDisplay(JumpConfig_Element, false);

                        #region Start
                        Start_Forward.value = locomotionMap.movement.crouch.startCardinals.forward;
                        Start_Forward.RegisterValueChangedCallback((evt) =>
                        {
                            var structure = locomotionMap.movement.crouch.startCardinals.forward;
                            structure = evt.newValue as AnimationClip;
                            locomotionMap.movement.crouch.startCardinals.forward = structure;
                            SetEndColorToSaveButton();
                        });

                        Start_Backward.value = locomotionMap.movement.crouch.startCardinals.backward;
                        Start_Backward.RegisterValueChangedCallback((evt) =>
                        {
                            var structure = locomotionMap.movement.crouch.startCardinals.backward;
                            structure = evt.newValue as AnimationClip;
                            locomotionMap.movement.crouch.startCardinals.backward = structure;
                            SetEndColorToSaveButton();
                        });

                        Start_Left.value = locomotionMap.movement.crouch.startCardinals.left;
                        Start_Left.RegisterValueChangedCallback((evt) =>
                        {
                            var structure = locomotionMap.movement.crouch.startCardinals.left;
                            structure = evt.newValue as AnimationClip;
                            locomotionMap.movement.crouch.startCardinals.left = structure;
                            SetEndColorToSaveButton();
                        });

                        Start_Right.value = locomotionMap.movement.crouch.startCardinals.right;
                        Start_Right.RegisterValueChangedCallback((evt) =>
                        {
                            var structure = locomotionMap.movement.crouch.startCardinals.right;
                            structure = evt.newValue as AnimationClip;
                            locomotionMap.movement.crouch.startCardinals.right = structure;
                            SetEndColorToSaveButton();
                        });
                        #endregion

                        #region Stop
                        Stop_Forward.value = locomotionMap.movement.crouch.stopCardinals.forward;
                        Stop_Forward.RegisterValueChangedCallback((evt) =>
                        {
                            var structure = locomotionMap.movement.crouch.stopCardinals.forward;
                            structure = evt.newValue as AnimationClip;
                            locomotionMap.movement.crouch.stopCardinals.forward = structure;
                            SetEndColorToSaveButton();
                        });

                        Stop_Backward.value = locomotionMap.movement.crouch.stopCardinals.backward;
                        Stop_Backward.RegisterValueChangedCallback((evt) =>
                        {
                            var structure = locomotionMap.movement.crouch.stopCardinals.backward;
                            structure = evt.newValue as AnimationClip;
                            locomotionMap.movement.crouch.stopCardinals.backward = structure;
                            SetEndColorToSaveButton();
                        });

                        Stop_Left.value = locomotionMap.movement.crouch.stopCardinals.left;
                        Stop_Left.RegisterValueChangedCallback((evt) =>
                        {
                            var structure = locomotionMap.movement.crouch.stopCardinals.left;
                            structure = evt.newValue as AnimationClip;
                            locomotionMap.movement.crouch.stopCardinals.left = structure;
                            SetEndColorToSaveButton();
                        });

                        Stop_Right.value = locomotionMap.movement.crouch.stopCardinals.right;
                        Stop_Right.RegisterValueChangedCallback((evt) =>
                        {
                            var structure = locomotionMap.movement.crouch.stopCardinals.right;
                            structure = evt.newValue as AnimationClip;
                            locomotionMap.movement.crouch.stopCardinals.right = structure;
                            SetEndColorToSaveButton();
                        });
                        #endregion

                        #region Loop Four
                        LoopFour_Forward.value = locomotionMap.movement.crouch.loopCardinalsFour.forward;
                        LoopFour_Forward.RegisterValueChangedCallback((evt) =>
                        {
                            var structure = locomotionMap.movement.crouch.loopCardinalsFour.forward;
                            structure = evt.newValue as AnimationClip;
                            locomotionMap.movement.crouch.loopCardinalsFour.forward = structure;
                            SetEndColorToSaveButton();
                        });

                        LoopFour_Backward.value = locomotionMap.movement.crouch.loopCardinalsFour.backward;
                        LoopFour_Backward.RegisterValueChangedCallback((evt) =>
                        {
                            var structure = locomotionMap.movement.crouch.loopCardinalsFour.backward;
                            structure = evt.newValue as AnimationClip;
                            locomotionMap.movement.crouch.loopCardinalsFour.backward = structure;
                            SetEndColorToSaveButton();
                        });

                        LoopFour_Left.value = locomotionMap.movement.crouch.loopCardinalsFour.left;
                        LoopFour_Left.RegisterValueChangedCallback((evt) =>
                        {
                            var structure = locomotionMap.movement.crouch.loopCardinalsFour.left;
                            structure = evt.newValue as AnimationClip;
                            locomotionMap.movement.crouch.loopCardinalsFour.left = structure;
                            SetEndColorToSaveButton();
                        });

                        LoopFour_Right.value = locomotionMap.movement.jog.loopCardinalsFour.right;
                        LoopFour_Right.RegisterValueChangedCallback((evt) =>
                        {
                            var structure = locomotionMap.movement.jog.loopCardinalsFour.right;
                            structure = evt.newValue as AnimationClip;
                            locomotionMap.movement.jog.loopCardinalsFour.right = structure;
                            SetEndColorToSaveButton();
                        });
                        #endregion

                        #region Loop Eight
                        LoopEight_Forward.value = locomotionMap.movement.crouch.loopCardinalsEight.forward;
                        LoopEight_Forward.RegisterValueChangedCallback((evt) =>
                        {
                            var structure = locomotionMap.movement.crouch.loopCardinalsEight.forward;
                            structure = evt.newValue as AnimationClip;
                            locomotionMap.movement.crouch.loopCardinalsEight.forward = structure;
                            SetEndColorToSaveButton();
                        });

                        LoopEight_Backward.value = locomotionMap.movement.crouch.loopCardinalsEight.backward;
                        LoopEight_Backward.RegisterValueChangedCallback((evt) =>
                        {
                            var structure = locomotionMap.movement.crouch.loopCardinalsEight.backward;
                            structure = evt.newValue as AnimationClip;
                            locomotionMap.movement.crouch.loopCardinalsEight.backward = structure;
                            SetEndColorToSaveButton();
                        });

                        LoopEight_Left.value = locomotionMap.movement.crouch.loopCardinalsEight.left;
                        LoopEight_Left.RegisterValueChangedCallback((evt) =>
                        {
                            var structure = locomotionMap.movement.crouch.loopCardinalsEight.left;
                            structure = evt.newValue as AnimationClip;
                            locomotionMap.movement.crouch.loopCardinalsEight.left = structure;
                            SetEndColorToSaveButton();
                        });

                        LoopEight_Right.value = locomotionMap.movement.crouch.loopCardinalsEight.right;
                        LoopEight_Right.RegisterValueChangedCallback((evt) =>
                        {
                            var structure = locomotionMap.movement.crouch.loopCardinalsEight.right;
                            structure = evt.newValue as AnimationClip;
                            locomotionMap.movement.crouch.loopCardinalsEight.right = structure;
                            SetEndColorToSaveButton();
                        });

                        LoopEight_Forward_Right.value = locomotionMap.movement.crouch.loopCardinalsEight.forwardRight;
                        LoopEight_Forward_Right.RegisterValueChangedCallback((evt) =>
                        {
                            var structure = locomotionMap.movement.crouch.loopCardinalsEight.forwardRight;
                            structure = evt.newValue as AnimationClip;
                            locomotionMap.movement.crouch.loopCardinalsEight.forwardRight = structure;
                            SetEndColorToSaveButton();
                        });

                        LoopEight_Forward_Left.value = locomotionMap.movement.crouch.loopCardinalsEight.forwardLeft;
                        LoopEight_Forward_Left.RegisterValueChangedCallback((evt) =>
                        {
                            var structure = locomotionMap.movement.crouch.loopCardinalsEight.forwardLeft;
                            structure = evt.newValue as AnimationClip;
                            locomotionMap.movement.crouch.loopCardinalsEight.forwardLeft = structure;
                            SetEndColorToSaveButton();
                        });

                        LoopEight_Backward_Right.value = locomotionMap.movement.crouch.loopCardinalsEight.backwardRight;
                        LoopEight_Backward_Right.RegisterValueChangedCallback((evt) =>
                        {
                            var structure = locomotionMap.movement.crouch.loopCardinalsEight.backwardRight;
                            structure = evt.newValue as AnimationClip;
                            locomotionMap.movement.crouch.loopCardinalsEight.backwardRight = structure;
                            SetEndColorToSaveButton();
                        });

                        LoopEight_Backward_Left.value = locomotionMap.movement.crouch.loopCardinalsEight.backwardLeft;
                        LoopEight_Backward_Left.RegisterValueChangedCallback((evt) =>
                        {
                            var structure = locomotionMap.movement.crouch.loopCardinalsEight.backwardLeft;
                            structure = evt.newValue as AnimationClip;
                            locomotionMap.movement.crouch.loopCardinalsEight.backwardLeft = structure;
                            SetEndColorToSaveButton();
                        });
                        #endregion

                        #region Pivots
                        Pivot_Forward.value = locomotionMap.movement.crouch.pivotCardinals.forward;
                        Pivot_Forward.RegisterValueChangedCallback((evt) =>
                        {
                            var structure = locomotionMap.movement.crouch.pivotCardinals.forward;
                            structure = evt.newValue as AnimationClip;
                            locomotionMap.movement.crouch.pivotCardinals.forward = structure;
                            SetEndColorToSaveButton();
                        });

                        Pivot_Backward.value = locomotionMap.movement.crouch.pivotCardinals.backward;
                        LoopFour_Backward.RegisterValueChangedCallback((evt) =>
                        {
                            var structure = locomotionMap.movement.crouch.pivotCardinals.backward;
                            structure = evt.newValue as AnimationClip;
                            locomotionMap.movement.crouch.pivotCardinals.backward = structure;
                            SetEndColorToSaveButton();
                        });

                        Pivot_Left.value = locomotionMap.movement.crouch.pivotCardinals.left;
                        Pivot_Left.RegisterValueChangedCallback((evt) =>
                        {
                            var structure = locomotionMap.movement.crouch.pivotCardinals.left;
                            structure = evt.newValue as AnimationClip;
                            locomotionMap.movement.crouch.pivotCardinals.left = structure;
                            SetEndColorToSaveButton();
                        });

                        Pivot_Right.value = locomotionMap.movement.crouch.pivotCardinals.right;
                        Pivot_Right.RegisterValueChangedCallback((evt) =>
                        {
                            var structure = locomotionMap.movement.crouch.pivotCardinals.right;
                            structure = evt.newValue as AnimationClip;
                            locomotionMap.movement.crouch.pivotCardinals.right = structure;
                            SetEndColorToSaveButton();
                        });
                        #endregion

                        #region Turn In Place
                        Turn45_Right.value = locomotionMap.movement.crouch.turnInPlace.right45;
                        Turn45_Right.RegisterValueChangedCallback((evt) =>
                        {
                            var structure = locomotionMap.movement.crouch.turnInPlace.right45;
                            structure = evt.newValue as AnimationClip;
                            locomotionMap.movement.crouch.turnInPlace.right45 = structure;
                            SetEndColorToSaveButton();
                        });

                        Turn90_Right.value = locomotionMap.movement.crouch.turnInPlace.right90;
                        Turn90_Right.RegisterValueChangedCallback((evt) =>
                        {
                            var structure = locomotionMap.movement.crouch.turnInPlace.right90;
                            structure = evt.newValue as AnimationClip;
                            locomotionMap.movement.crouch.turnInPlace.right90 = structure;
                            SetEndColorToSaveButton();
                        });

                        Turn135_Right.value = locomotionMap.movement.crouch.turnInPlace.right135;
                        Turn135_Right.RegisterValueChangedCallback((evt) =>
                        {
                            var structure = locomotionMap.movement.crouch.turnInPlace.right135;
                            structure = evt.newValue as AnimationClip;
                            locomotionMap.movement.crouch.turnInPlace.right135 = structure;
                            SetEndColorToSaveButton();
                        });

                        Turn180_Right.value = locomotionMap.movement.crouch.turnInPlace.right180;
                        Turn180_Right.RegisterValueChangedCallback((evt) =>
                        {
                            var structure = locomotionMap.movement.crouch.turnInPlace.right180;
                            structure = evt.newValue as AnimationClip;
                            locomotionMap.movement.crouch.turnInPlace.right180 = structure;
                            SetEndColorToSaveButton();
                        });

                        Turn45_Left.value = locomotionMap.movement.crouch.turnInPlace.left45;
                        Turn45_Left.RegisterValueChangedCallback((evt) =>
                        {
                            var structure = locomotionMap.movement.crouch.turnInPlace.left45;
                            structure = evt.newValue as AnimationClip;
                            locomotionMap.movement.crouch.turnInPlace.left45 = structure;
                            SetEndColorToSaveButton();
                        });

                        Turn90_Left.value = locomotionMap.movement.crouch.turnInPlace.left90;
                        Turn90_Left.RegisterValueChangedCallback((evt) =>
                        {
                            var structure = locomotionMap.movement.crouch.turnInPlace.left90;
                            structure = evt.newValue as AnimationClip;
                            locomotionMap.movement.crouch.turnInPlace.left90 = structure;
                            SetEndColorToSaveButton();
                        });

                        Turn135_Left.value = locomotionMap.movement.crouch.turnInPlace.left135;
                        Turn135_Left.RegisterValueChangedCallback((evt) =>
                        {
                            var structure = locomotionMap.movement.crouch.turnInPlace.left135;
                            structure = evt.newValue as AnimationClip;
                            locomotionMap.movement.crouch.turnInPlace.left135 = structure;
                            SetEndColorToSaveButton();
                        });

                        Turn180_Left.value = locomotionMap.movement.crouch.turnInPlace.left180;
                        Turn180_Left.RegisterValueChangedCallback((evt) =>
                        {
                            var structure = locomotionMap.movement.crouch.turnInPlace.left180;
                            structure = evt.newValue as AnimationClip;
                            locomotionMap.movement.crouch.turnInPlace.left180 = structure;
                            SetEndColorToSaveButton();
                        });
                        #endregion
                        break;
                }
            }
            else
            {
                if (overrideLayer != null)
                {
                    SetElementDisplay(Pivots_Element, false);
                    SetElementDisplay(TurnInPlace_Element, false);
                    SetElementDisplay(JumpMotion_Element, false);
                    SetElementDisplay(JumpConfig_Element, false);

                    switch (movementStructType)
                    {
                        case MovementStruct.Walk:
                            #region General
                            motionspeed.value = overrideLayer.movement.walk.motionSpeed;
                            motionspeed.RegisterValueChangedCallback((evt) =>
                            {
                                var structure = overrideLayer.movement.walk;
                                structure.motionSpeed = evt.newValue;
                                overrideLayer.movement.walk = structure;
                                SetEndColorToSaveButton();
                            });
                            #endregion

                            #region Start
                            Start_Forward.value = overrideLayer.movement.walk.startCardinals.forward;
                            Start_Forward.RegisterValueChangedCallback((evt) =>
                            {
                                var structure = overrideLayer.movement.walk.startCardinals.forward;
                                structure = evt.newValue as AnimationClip;
                                overrideLayer.movement.walk.startCardinals.forward = structure;
                                SetEndColorToSaveButton();
                            });

                            Start_Backward.value = overrideLayer.movement.walk.startCardinals.backward;
                            Start_Backward.RegisterValueChangedCallback((evt) =>
                            {
                                var structure = overrideLayer.movement.walk.startCardinals.backward;
                                structure = evt.newValue as AnimationClip;
                                overrideLayer.movement.walk.startCardinals.backward = structure;
                                SetEndColorToSaveButton();
                            });

                            Start_Left.value = overrideLayer.movement.walk.startCardinals.left;
                            Start_Left.RegisterValueChangedCallback((evt) =>
                            {
                                var structure = overrideLayer.movement.walk.startCardinals.left;
                                structure = evt.newValue as AnimationClip;
                                overrideLayer.movement.walk.startCardinals.left = structure;
                                SetEndColorToSaveButton();
                            });

                            Start_Right.value = overrideLayer.movement.walk.startCardinals.right;
                            Start_Right.RegisterValueChangedCallback((evt) =>
                            {
                                var structure = overrideLayer.movement.walk.startCardinals.right;
                                structure = evt.newValue as AnimationClip;
                                overrideLayer.movement.walk.startCardinals.right = structure;
                                SetEndColorToSaveButton();
                            });
                            #endregion

                            #region Stop
                            Stop_Forward.value = overrideLayer.movement.walk.stopCardinals.forward;
                            Stop_Forward.RegisterValueChangedCallback((evt) =>
                            {
                                var structure = overrideLayer.movement.walk.stopCardinals.forward;
                                structure = evt.newValue as AnimationClip;
                                overrideLayer.movement.walk.stopCardinals.forward = structure;
                                SetEndColorToSaveButton();
                            });

                            Stop_Backward.value = overrideLayer.movement.walk.stopCardinals.backward;
                            Stop_Backward.RegisterValueChangedCallback((evt) =>
                            {
                                var structure = overrideLayer.movement.walk.stopCardinals.backward;
                                structure = evt.newValue as AnimationClip;
                                overrideLayer.movement.walk.stopCardinals.backward = structure;
                                SetEndColorToSaveButton();
                            });

                            Stop_Left.value = overrideLayer.movement.walk.stopCardinals.left;
                            Stop_Left.RegisterValueChangedCallback((evt) =>
                            {
                                var structure = overrideLayer.movement.walk.stopCardinals.left;
                                structure = evt.newValue as AnimationClip;
                                overrideLayer.movement.walk.stopCardinals.left = structure;
                                SetEndColorToSaveButton();
                            });

                            Stop_Right.value = overrideLayer.movement.walk.stopCardinals.right;
                            Stop_Right.RegisterValueChangedCallback((evt) =>
                            {
                                var structure = overrideLayer.movement.walk.stopCardinals.right;
                                structure = evt.newValue as AnimationClip;
                                overrideLayer.movement.walk.stopCardinals.right = structure;
                                SetEndColorToSaveButton();
                            });
                            #endregion

                            #region Loop Four
                            LoopFour_Forward.value = overrideLayer.movement.walk.loopCardinalsFour.forward;
                            LoopFour_Forward.RegisterValueChangedCallback((evt) =>
                            {
                                var structure = overrideLayer.movement.walk.loopCardinalsFour.forward;
                                structure = evt.newValue as AnimationClip;
                                overrideLayer.movement.walk.loopCardinalsFour.forward = structure;
                                SetEndColorToSaveButton();
                            });

                            LoopFour_Backward.value = overrideLayer.movement.walk.loopCardinalsFour.backward;
                            LoopFour_Backward.RegisterValueChangedCallback((evt) =>
                            {
                                var structure = overrideLayer.movement.walk.loopCardinalsFour.backward;
                                structure = evt.newValue as AnimationClip;
                                overrideLayer.movement.walk.loopCardinalsFour.backward = structure;
                                SetEndColorToSaveButton();
                            });

                            LoopFour_Left.value = overrideLayer.movement.walk.loopCardinalsFour.left;
                            LoopFour_Left.RegisterValueChangedCallback((evt) =>
                            {
                                var structure = overrideLayer.movement.walk.loopCardinalsFour.left;
                                structure = evt.newValue as AnimationClip;
                                overrideLayer.movement.walk.loopCardinalsFour.left = structure;
                                SetEndColorToSaveButton();
                            });

                            LoopFour_Right.value = overrideLayer.movement.walk.loopCardinalsFour.right;
                            LoopFour_Right.RegisterValueChangedCallback((evt) =>
                            {
                                var structure = overrideLayer.movement.walk.loopCardinalsFour.right;
                                structure = evt.newValue as AnimationClip;
                                overrideLayer.movement.walk.loopCardinalsFour.right = structure;
                                SetEndColorToSaveButton();
                            });
                            #endregion

                            #region Loop Eight
                            LoopEight_Forward.value = overrideLayer.movement.walk.loopCardinalsEight.forward;
                            LoopEight_Forward.RegisterValueChangedCallback((evt) =>
                            {
                                var structure = overrideLayer.movement.walk.loopCardinalsEight.forward;
                                structure = evt.newValue as AnimationClip;
                                overrideLayer.movement.walk.loopCardinalsEight.forward = structure;
                                SetEndColorToSaveButton();
                            });

                            LoopEight_Backward.value = overrideLayer.movement.walk.loopCardinalsEight.backward;
                            LoopEight_Backward.RegisterValueChangedCallback((evt) =>
                            {
                                var structure = overrideLayer.movement.walk.loopCardinalsEight.backward;
                                structure = evt.newValue as AnimationClip;
                                overrideLayer.movement.walk.loopCardinalsEight.backward = structure;
                                SetEndColorToSaveButton();
                            });

                            LoopEight_Left.value = overrideLayer.movement.walk.loopCardinalsEight.left;
                            LoopEight_Left.RegisterValueChangedCallback((evt) =>
                            {
                                var structure = overrideLayer.movement.walk.loopCardinalsEight.left;
                                structure = evt.newValue as AnimationClip;
                                overrideLayer.movement.walk.loopCardinalsEight.left = structure;
                                SetEndColorToSaveButton();
                            });

                            LoopEight_Right.value = overrideLayer.movement.walk.loopCardinalsEight.right;
                            LoopEight_Right.RegisterValueChangedCallback((evt) =>
                            {
                                var structure = overrideLayer.movement.walk.loopCardinalsEight.right;
                                structure = evt.newValue as AnimationClip;
                                overrideLayer.movement.walk.loopCardinalsEight.right = structure;
                                SetEndColorToSaveButton();
                            });

                            LoopEight_Forward_Right.value = overrideLayer.movement.walk.loopCardinalsEight.forwardRight;
                            LoopEight_Forward_Right.RegisterValueChangedCallback((evt) =>
                            {
                                var structure = overrideLayer.movement.walk.loopCardinalsEight.forwardRight;
                                structure = evt.newValue as AnimationClip;
                                overrideLayer.movement.walk.loopCardinalsEight.forwardRight = structure;
                                SetEndColorToSaveButton();
                            });

                            LoopEight_Forward_Left.value = overrideLayer.movement.walk.loopCardinalsEight.forwardLeft;
                            LoopEight_Forward_Left.RegisterValueChangedCallback((evt) =>
                            {
                                var structure = overrideLayer.movement.walk.loopCardinalsEight.forwardLeft;
                                structure = evt.newValue as AnimationClip;
                                overrideLayer.movement.walk.loopCardinalsEight.forwardLeft = structure;
                                SetEndColorToSaveButton();
                            });

                            LoopEight_Backward_Right.value = overrideLayer.movement.walk.loopCardinalsEight.backwardRight;
                            LoopEight_Backward_Right.RegisterValueChangedCallback((evt) =>
                            {
                                var structure = overrideLayer.movement.walk.loopCardinalsEight.backwardRight;
                                structure = evt.newValue as AnimationClip;
                                overrideLayer.movement.walk.loopCardinalsEight.backwardRight = structure;
                                SetEndColorToSaveButton();
                            });

                            LoopEight_Backward_Left.value = overrideLayer.movement.walk.loopCardinalsEight.backwardLeft;
                            LoopEight_Backward_Left.RegisterValueChangedCallback((evt) =>
                            {
                                var structure = overrideLayer.movement.walk.loopCardinalsEight.backwardLeft;
                                structure = evt.newValue as AnimationClip;
                                overrideLayer.movement.walk.loopCardinalsEight.backwardLeft = structure;
                                SetEndColorToSaveButton();
                            });
                            #endregion
                            break;

                        case MovementStruct.Jog:
                            #region General
                            motionspeed.value = overrideLayer.movement.jog.motionSpeed;
                            motionspeed.RegisterValueChangedCallback((evt) =>
                            {
                                var structure = overrideLayer.movement.jog;
                                structure.motionSpeed = evt.newValue;
                                overrideLayer.movement.jog = structure;
                                SetEndColorToSaveButton();
                            });
                            #endregion

                            #region Start
                            Start_Forward.value = overrideLayer.movement.jog.startCardinals.forward;
                            Start_Forward.RegisterValueChangedCallback((evt) =>
                            {
                                var structure = overrideLayer.movement.jog.startCardinals.forward;
                                structure = evt.newValue as AnimationClip;
                                overrideLayer.movement.jog.startCardinals.forward = structure;
                                SetEndColorToSaveButton();
                            });

                            Start_Backward.value = overrideLayer.movement.jog.startCardinals.backward;
                            Start_Backward.RegisterValueChangedCallback((evt) =>
                            {
                                var structure = overrideLayer.movement.jog.startCardinals.backward;
                                structure = evt.newValue as AnimationClip;
                                overrideLayer.movement.jog.startCardinals.backward = structure;
                                SetEndColorToSaveButton();
                            });

                            Start_Left.value = overrideLayer.movement.jog.startCardinals.left;
                            Start_Left.RegisterValueChangedCallback((evt) =>
                            {
                                var structure = overrideLayer.movement.jog.startCardinals.left;
                                structure = evt.newValue as AnimationClip;
                                overrideLayer.movement.jog.startCardinals.left = structure;
                                SetEndColorToSaveButton();
                            });

                            Start_Right.value = overrideLayer.movement.jog.startCardinals.right;
                            Start_Right.RegisterValueChangedCallback((evt) =>
                            {
                                var structure = overrideLayer.movement.jog.startCardinals.right;
                                structure = evt.newValue as AnimationClip;
                                overrideLayer.movement.jog.startCardinals.right = structure;
                                SetEndColorToSaveButton();
                            });
                            #endregion

                            #region Stop
                            Stop_Forward.value = overrideLayer.movement.jog.stopCardinals.forward;
                            Stop_Forward.RegisterValueChangedCallback((evt) =>
                            {
                                var structure = overrideLayer.movement.jog.stopCardinals.forward;
                                structure = evt.newValue as AnimationClip;
                                overrideLayer.movement.jog.stopCardinals.forward = structure;
                                SetEndColorToSaveButton();
                            });

                            Stop_Backward.value = overrideLayer.movement.jog.stopCardinals.backward;
                            Stop_Backward.RegisterValueChangedCallback((evt) =>
                            {
                                var structure = overrideLayer.movement.jog.stopCardinals.backward;
                                structure = evt.newValue as AnimationClip;
                                overrideLayer.movement.jog.stopCardinals.backward = structure;
                                SetEndColorToSaveButton();
                            });

                            Stop_Left.value = overrideLayer.movement.jog.stopCardinals.left;
                            Stop_Left.RegisterValueChangedCallback((evt) =>
                            {
                                var structure = overrideLayer.movement.jog.stopCardinals.left;
                                structure = evt.newValue as AnimationClip;
                                overrideLayer.movement.jog.stopCardinals.left = structure;
                                SetEndColorToSaveButton();
                            });

                            Stop_Right.value = overrideLayer.movement.jog.stopCardinals.right;
                            Stop_Right.RegisterValueChangedCallback((evt) =>
                            {
                                var structure = overrideLayer.movement.jog.stopCardinals.right;
                                structure = evt.newValue as AnimationClip;
                                overrideLayer.movement.jog.stopCardinals.right = structure;
                                SetEndColorToSaveButton();
                            });
                            #endregion

                            #region Loop Four
                            LoopFour_Forward.value = overrideLayer.movement.jog.loopCardinalsFour.forward;
                            LoopFour_Forward.RegisterValueChangedCallback((evt) =>
                            {
                                var structure = overrideLayer.movement.jog.loopCardinalsFour.forward;
                                structure = evt.newValue as AnimationClip;
                                overrideLayer.movement.jog.loopCardinalsFour.forward = structure;
                                SetEndColorToSaveButton();
                            });

                            LoopFour_Backward.value = overrideLayer.movement.jog.loopCardinalsFour.backward;
                            LoopFour_Backward.RegisterValueChangedCallback((evt) =>
                            {
                                var structure = overrideLayer.movement.jog.loopCardinalsFour.backward;
                                structure = evt.newValue as AnimationClip;
                                overrideLayer.movement.jog.loopCardinalsFour.backward = structure;
                                SetEndColorToSaveButton();
                            });

                            LoopFour_Left.value = overrideLayer.movement.jog.loopCardinalsFour.left;
                            LoopFour_Left.RegisterValueChangedCallback((evt) =>
                            {
                                var structure = overrideLayer.movement.jog.loopCardinalsFour.left;
                                structure = evt.newValue as AnimationClip;
                                overrideLayer.movement.jog.loopCardinalsFour.left = structure;
                                SetEndColorToSaveButton();
                            });

                            LoopFour_Right.value = overrideLayer.movement.jog.loopCardinalsFour.right;
                            LoopFour_Right.RegisterValueChangedCallback((evt) =>
                            {
                                var structure = overrideLayer.movement.jog.loopCardinalsFour.right;
                                structure = evt.newValue as AnimationClip;
                                overrideLayer.movement.jog.loopCardinalsFour.right = structure;
                                SetEndColorToSaveButton();
                            });
                            #endregion

                            #region Loop Eight
                            LoopEight_Forward.value = overrideLayer.movement.jog.loopCardinalsEight.forward;
                            LoopEight_Forward.RegisterValueChangedCallback((evt) =>
                            {
                                var structure = overrideLayer.movement.jog.loopCardinalsEight.forward;
                                structure = evt.newValue as AnimationClip;
                                overrideLayer.movement.jog.loopCardinalsEight.forward = structure;
                                SetEndColorToSaveButton();
                            });

                            LoopEight_Backward.value = overrideLayer.movement.jog.loopCardinalsEight.backward;
                            LoopEight_Backward.RegisterValueChangedCallback((evt) =>
                            {
                                var structure = overrideLayer.movement.jog.loopCardinalsEight.backward;
                                structure = evt.newValue as AnimationClip;
                                overrideLayer.movement.jog.loopCardinalsEight.backward = structure;
                                SetEndColorToSaveButton();
                            });

                            LoopEight_Left.value = overrideLayer.movement.jog.loopCardinalsEight.left;
                            LoopEight_Left.RegisterValueChangedCallback((evt) =>
                            {
                                var structure = overrideLayer.movement.jog.loopCardinalsEight.left;
                                structure = evt.newValue as AnimationClip;
                                overrideLayer.movement.jog.loopCardinalsEight.left = structure;
                                SetEndColorToSaveButton();
                            });

                            LoopEight_Right.value = overrideLayer.movement.jog.loopCardinalsEight.right;
                            LoopEight_Right.RegisterValueChangedCallback((evt) =>
                            {
                                var structure = overrideLayer.movement.jog.loopCardinalsEight.right;
                                structure = evt.newValue as AnimationClip;
                                overrideLayer.movement.jog.loopCardinalsEight.right = structure;
                                SetEndColorToSaveButton();
                            });

                            LoopEight_Forward_Right.value = overrideLayer.movement.jog.loopCardinalsEight.forwardRight;
                            LoopEight_Forward_Right.RegisterValueChangedCallback((evt) =>
                            {
                                var structure = overrideLayer.movement.jog.loopCardinalsEight.forwardRight;
                                structure = evt.newValue as AnimationClip;
                                overrideLayer.movement.jog.loopCardinalsEight.forwardRight = structure;
                                SetEndColorToSaveButton();
                            });

                            LoopEight_Forward_Left.value = overrideLayer.movement.jog.loopCardinalsEight.forwardLeft;
                            LoopEight_Forward_Left.RegisterValueChangedCallback((evt) =>
                            {
                                var structure = overrideLayer.movement.jog.loopCardinalsEight.forwardLeft;
                                structure = evt.newValue as AnimationClip;
                                overrideLayer.movement.jog.loopCardinalsEight.forwardLeft = structure;
                                SetEndColorToSaveButton();
                            });

                            LoopEight_Backward_Right.value = overrideLayer.movement.jog.loopCardinalsEight.backwardRight;
                            LoopEight_Backward_Right.RegisterValueChangedCallback((evt) =>
                            {
                                var structure = overrideLayer.movement.jog.loopCardinalsEight.backwardRight;
                                structure = evt.newValue as AnimationClip;
                                overrideLayer.movement.jog.loopCardinalsEight.backwardRight = structure;
                                SetEndColorToSaveButton();
                            });

                            LoopEight_Backward_Left.value = overrideLayer.movement.jog.loopCardinalsEight.backwardLeft;
                            LoopEight_Backward_Left.RegisterValueChangedCallback((evt) =>
                            {
                                var structure = overrideLayer.movement.jog.loopCardinalsEight.backwardLeft;
                                structure = evt.newValue as AnimationClip;
                                overrideLayer.movement.jog.loopCardinalsEight.backwardLeft = structure;
                                SetEndColorToSaveButton();
                            });
                            #endregion
                            break;
                    }
                }
            }
        }
        else
        {
            if (!isOverride)
            {
                #region Fields
                Button useEightDirectionalButton = UFEditorUtils.FindElementInRoot<Button>(rightInstance, "use-eight-directional-button");
                Button useOverrideAtStarupButton = UFEditorUtils.FindElementInRoot<Button>(rightInstance, "use-override-at-starup-button");
                ObjectField idleMotionField = UFEditorUtils.FindElementInRoot<ObjectField>(rightInstance, "idle-motion-field");
                ObjectField crouchEntryMotionField = UFEditorUtils.FindElementInRoot<ObjectField>(rightInstance, "crouch-entry-motion-field");
                ObjectField crouchExitMotionField = UFEditorUtils.FindElementInRoot<ObjectField>(rightInstance, "crouch-exit-motion-field");
                FloatField walkSpeedField = UFEditorUtils.FindElementInRoot<FloatField>(rightInstance, "walk-speed-field");
                FloatField jodSpeedField = UFEditorUtils.FindElementInRoot<FloatField>(rightInstance, "jog-speed-field");
                Slider rotationSmoothField = UFEditorUtils.FindElementInRoot<Slider>(rightInstance, "rotation-smooth-slider");
                FloatField speedChangeRateField = UFEditorUtils.FindElementInRoot<FloatField>(rightInstance, "speed-change-rate-field");
                ScrollView idleBreakContent = UFEditorUtils.FindElementInRoot<ScrollView>(rightInstance, "idle-break-scroll-view");
                Button addIdleBreakElement = UFEditorUtils.FindElementInRoot<Button>(rightInstance, "add-idle-break-button");
                #endregion

                AddObjectFieldManipulators(idleMotionField);
                AddObjectFieldManipulators(crouchEntryMotionField);
                AddObjectFieldManipulators(crouchExitMotionField);

                SetSwitch(locomotionMap.useEightDirectional, useEightDirectionalButton);
                useEightDirectionalButton.clickable.clicked += () =>
                {
                    locomotionMap.useEightDirectional = !locomotionMap.useEightDirectional;
                    ShowCorrectLoopMotionElement(locomotionMap.useEightDirectional, LoopEight_Element, LoopFour_Element);
                    SetSwitch(locomotionMap.useEightDirectional, useEightDirectionalButton);
                    SetEndColorToSaveButton();
                };

                SetSwitch(locomotionMap.useOverrideAtStartup, useOverrideAtStarupButton);
                useOverrideAtStarupButton.clickable.clicked += () =>
                {
                    locomotionMap.useOverrideAtStartup = !locomotionMap.useOverrideAtStartup;
                    SetSwitch(locomotionMap.useOverrideAtStartup, useOverrideAtStarupButton);
                    SetEndColorToSaveButton();
                };

                idleMotionField.value = locomotionMap.general.idle;
                idleMotionField.RegisterValueChangedCallback((evt) =>
                {
                    var structure = locomotionMap.general;
                    structure.idle = evt.newValue as AnimationClip;
                    locomotionMap.general = structure;
                    SetEndColorToSaveButton();
                });

                crouchEntryMotionField.value = locomotionMap.general.crouch.crouchEntry;
                crouchEntryMotionField.RegisterValueChangedCallback((evt) =>
                {
                    var structure = locomotionMap.general;
                    structure.crouch.crouchEntry = evt.newValue as AnimationClip;
                    locomotionMap.general = structure;
                    SetEndColorToSaveButton();
                });

                crouchExitMotionField.value = locomotionMap.general.crouch.crouchExit;
                crouchExitMotionField.RegisterValueChangedCallback((evt) =>
                {
                    var structure = locomotionMap.general;
                    structure.crouch.crouchExit = evt.newValue as AnimationClip;
                    locomotionMap.general = structure;
                    SetEndColorToSaveButton();
                });

                walkSpeedField.value = locomotionMap.general.movementConfig.walkSpeed;
                walkSpeedField.RegisterValueChangedCallback((evt) =>
                {
                    var structure = locomotionMap.general;
                    structure.movementConfig.walkSpeed = evt.newValue;
                    locomotionMap.general = structure;
                    SetEndColorToSaveButton();
                });

                jodSpeedField.value = locomotionMap.general.movementConfig.jogSpeed;
                jodSpeedField.RegisterValueChangedCallback((evt) =>
                {
                    var structure = locomotionMap.general;
                    structure.movementConfig.jogSpeed = evt.newValue;
                    locomotionMap.general = structure;
                    SetEndColorToSaveButton();
                });

                rotationSmoothField.value = locomotionMap.general.movementConfig.rotationSmoothTime;
                rotationSmoothField.RegisterValueChangedCallback((evt) =>
                {
                    var structure = locomotionMap.general;
                    structure.movementConfig.rotationSmoothTime = evt.newValue;
                    locomotionMap.general = structure;
                    SetEndColorToSaveButton();
                });

                speedChangeRateField.value = locomotionMap.general.movementConfig.speedChangeRate;
                speedChangeRateField.RegisterValueChangedCallback((evt) =>
                {
                    var structure = locomotionMap.general;
                    structure.movementConfig.speedChangeRate = evt.newValue;
                    locomotionMap.general = structure;
                    SetEndColorToSaveButton();
                });

                foreach (var idleBreak in locomotionMap.general.idleBreaks)
                {
                    AddIdleBreaks(idleBreakContent, idleBreak, locomotionMap);
                }
                addIdleBreakElement.RegisterCallback<ClickEvent>(evt =>
                {
                    var newMotion = new AnimationClip();
                    locomotionMap.general.idleBreaks.Add(newMotion);
                    AddIdleBreaks(idleBreakContent, newMotion, locomotionMap);
                });
            }
            else
            {
                rightInstance = LocomotionGeneralsOverrideRightTemplate.CloneTree();

                #region Fields
                ObjectField globalPose_MotionField = FindElementInRoot<ObjectField>(rightInstance, "global-pose-motion-field");
                TextField globalPose_LayerMaskField = FindElementInRoot<TextField>(rightInstance, "global-pose-layer-mask-field");
                ObjectField idle_MotionField = FindElementInRoot<ObjectField>(rightInstance, "idle-motion-field");
                TextField general_LayerMaskField = FindElementInRoot<TextField>(rightInstance, "general-layer-mask-field");
                #endregion

                AddObjectFieldManipulators(globalPose_MotionField);
                AddObjectFieldManipulators(idle_MotionField);

                globalPose_MotionField.value = overrideLayer.globalPose.motion;
                globalPose_MotionField.RegisterValueChangedCallback((evt) =>
                {
                    var structure = overrideLayer.globalPose.motion;
                    structure = evt.newValue as AnimationClip;
                    overrideLayer.globalPose.motion = structure;
                    SetEndColorToSaveButton();
                });

                globalPose_LayerMaskField.value = overrideLayer.globalPose.mask;
                globalPose_LayerMaskField.RegisterValueChangedCallback((evt) =>
                {
                    var structure = overrideLayer.globalPose.mask;
                    structure = evt.newValue;
                    overrideLayer.globalPose.mask = structure;
                    SetEndColorToSaveButton();
                });

                idle_MotionField.value = overrideLayer.movement.idle;
                idle_MotionField.RegisterValueChangedCallback((evt) =>
                {
                    var structure = overrideLayer.movement.idle;
                    structure = evt.newValue as AnimationClip;
                    overrideLayer.movement.idle = structure;
                    SetEndColorToSaveButton();
                });

                general_LayerMaskField.value = overrideLayer.movement.motionMask;
                general_LayerMaskField.RegisterValueChangedCallback((evt) =>
                {
                    var structure = overrideLayer.movement.motionMask;
                    structure = evt.newValue;
                    overrideLayer.movement.motionMask = structure;
                    SetEndColorToSaveButton();
                });
            }
        }

        rightContainer.Clear();
        rightContainer.Add(rightInstance);
    }

    private void OpenPreview(AnimationClip currentClip)
    {
        VisualElement previsulizerContainer = FindElementInRoot<VisualElement>("pre-visualizer-container");
        m_IMGUIContainer = FindElementInRoot<IMGUIContainer>(previsulizerContainer, "preview-content-container");
        m_IMGUIContainer.onGUIHandler = () => HandleEvents();

        previsulizerContainer.style.display = DisplayStyle.Flex;
        isPreviewOpen = true;

        ObjectField previewObjectField = FindElementInRoot<ObjectField>("preview-model-field");
        previewObjectField.RegisterValueChangedCallback((evt) =>
        {
            previewModel = evt.newValue != null ? Instantiate(evt.newValue as GameObject) : Instantiate(Resources.Load<GameObject>("Preview/Zirem_Preview"));
            previewUtility?.Cleanup();
            previewUtility = new PreviewRenderUtility(true);
            SetupPreviewScene(currentClip);
        });

        previewModel = previewObjectField.value != null ? Instantiate(previewObjectField.value as GameObject) : Instantiate(Resources.Load<GameObject>("Preview/Zirem_Preview"));

        previewUtility?.Cleanup();
        previewUtility = new PreviewRenderUtility(true);
        SetupPreviewScene(currentClip);
    }
    private void SetupPreviewScene(AnimationClip currentClip)
    {       
        if (previewModel == null) previewModel = Instantiate(Resources.Load<GameObject>("Preview/Zirem_Preview"));
        previewModel.transform.SetPositionAndRotation(Vector3.zero, new Quaternion(0, 180, 0, 0));

        // Verifica que el GameObject tenga un componente Animator
        AnimatorOverrideController overrideController;
        if (!previewModel.TryGetComponent<Animator>(out previewAnimator))
        {
            // Si no tiene un componente Animator, agrega uno
            previewAnimator = previewModel.AddComponent<Animator>();
            previewAnimator.enabled = false;
        }

        previewAnimator.enabled = false;

        if (previewAnimator.runtimeAnimatorController == null)
            previewAnimator.runtimeAnimatorController = Resources.Load<RuntimeAnimatorController>("Preview/Preview_Controller");

        overrideController = new(previewAnimator.runtimeAnimatorController);
        previewAnimator.runtimeAnimatorController = overrideController;
        previewAnimator.applyRootMotion = false;

        var clipClone = Instantiate(currentClip);
        overrideController["Preview_Anim"] = clipClone;
        previewAnimator.Play("Preview", 0, 0);

        previewModel.hideFlags = HideFlags.HideAndDontSave;
        previewUtility.AddSingleGO(previewModel);

        // Camera is spawned at origin, so position is in front of the cube.
        previewUtility.camera.nearClipPlane = 0.1f;
        previewUtility.camera.farClipPlane = 100f;
        previewUtility.camera.transform.position = DefaultCameraPosition;
        Vector3 angles = previewUtility.camera.transform.eulerAngles;
        _x = angles.y;
        _y = angles.x;
    }
    private void RenderPreview()
    {
        // Render Preview
        previewUtility.BeginPreview(m_IMGUIContainer.contentRect, previewBackground: GUIStyle.none);
        previewUtility.Render(true);
        previewAnimator.Update(Time.deltaTime / 3);

        Texture texture = previewUtility.EndPreview();
        GUI.DrawTexture(m_IMGUIContainer.contentRect, texture);

        Texture2D texture2D = new(texture.width, texture.height, TextureFormat.RGBA32, false);
        RenderTexture currentRT = RenderTexture.active;
        RenderTexture renderTexture = RenderTexture.GetTemporary(texture.width, texture.height, 32);
        Graphics.Blit(texture, renderTexture);
        texture2D.ReadPixels(new Rect(0, 0, renderTexture.width, renderTexture.height), 0, 0);
        texture2D.Apply();
        RenderTexture.active = currentRT;
        RenderTexture.ReleaseTemporary(renderTexture);
        m_IMGUIContainer.style.backgroundImage = new StyleBackground(texture2D);
    }
    private void HandleEvents()
    {
        Event current = Event.current;
        if (current.type == EventType.MouseDrag)
        {
            if (current.button == 0)
            {
                _x += current.delta.x * _xSpeed * _distance * 0.02f;
                _y += current.delta.y * _ySpeed * _distance * 0.02f;
            }
            else if (current.button == 2)
            {
                _targetPosition -= 0.01f * current.delta.x * previewUtility.camera.transform.right;
                _targetPosition += 0.01f * current.delta.y * previewUtility.camera.transform.up;
            }
        }

        if (current.type == EventType.ScrollWheel)
            _distance = Mathf.Clamp(_distance + current.delta.y * 0.5f, _distanceMin, _distanceMax);

        _y = ClampAngle(_y, _yMinLimit, _yMaxLimit);
        Quaternion rotation = Quaternion.Euler(_y, _x, 0);
        Vector3 negDistance = new(0.0f, 0.0f, -_distance);
        Vector3 position = rotation * negDistance + _targetPosition;
        previewUtility.camera.transform.SetPositionAndRotation(position, rotation);
    }
    private void AddIdleBreaks(VisualElement container, AnimationClip motion, LocomotionMap locomotionMap)
    {
        var instance = IdleBreakElement.CloneTree();
        container.Add(instance);

        ObjectField motionField = UFEditorUtils.FindElementInRoot<ObjectField>(instance, "motion");
        Button removeButton = UFEditorUtils.FindElementInRoot<Button>(instance, "remove-button");

        motionField.value = motion;
        var motionIndex = locomotionMap.general.idleBreaks.IndexOf(motion);
        motionField.RegisterValueChangedCallback(evt =>
        {
            locomotionMap.general.idleBreaks[motionIndex] = (AnimationClip)evt.newValue;
        });

        removeButton.RegisterCallback<ClickEvent>(evt =>
        {
            RemoveIdleBreak(container, instance, locomotionMap, motion);
        });
    }
    private void RemoveIdleBreak(VisualElement container, TemplateContainer instance, LocomotionMap locomotionMap, AnimationClip motion)
    {
        locomotionMap.general.idleBreaks.Remove(motion);
        container.Remove(instance);
    }

    private T FindElementInRoot<T>(string name) where T : VisualElement
    {
        return base_Root.Q<T>(name);
    }
    private T FindElementInRoot<T>(VisualElement root, string name) where T : VisualElement
    {
        return root.Q<T>(name);
    }
    private void SetElementDisplay(VisualElement container, bool value)
    {
        if (value) container.style.display = DisplayStyle.Flex;
        else container.style.display = DisplayStyle.None;
    }
    private void SetElementDisplay(VisualElement container, VisualElement arrow)
    {
        if (container.style.display == DisplayStyle.None)
        {
            container.style.display = DisplayStyle.Flex;
            arrow.AddToClassList("arrowmark-toggle-open");
        }
        else
        {
            container.style.display = DisplayStyle.None;
            arrow.RemoveFromClassList("arrowmark-toggle-open");
        }
    }
    private void SetElementDisplay(VisualElement root, string queryElement, DisplayStyle displayStyle)
    {
        FindElementInRoot<VisualElement>(root, queryElement).style.display = displayStyle;
    }
    private void SetSwitch(bool value, Button buton)
    {
        if (value == true)
        {
            buton.text = "ON";
            buton.RemoveFromClassList("SwitchOff");
        }
        else
        {
            buton.text = "OFF";
            buton.AddToClassList("SwitchOff");
        }
    }
    private void SetPreviewDisplay(VisualElement container)
    {
        if (container.style.display == DisplayStyle.None)
        {
            container.style.display = DisplayStyle.Flex;
            isPreviewOpen = true;
        }
        else
        {
            container.style.display = DisplayStyle.None;
            isPreviewOpen = false;
        }
    }
    private void SetStartColorToSaveButton()
    {
        saveButton.style.backgroundColor = startColor;
        saveButton.style.color = Color.white;
    }
    private void SetEndColorToSaveButton()
    {
        saveButton.style.backgroundColor = endColor;
        saveButton.style.color = Color.black;
    }
    private void ShowCorrectLoopMotionElement(bool value, params VisualElement[] containers)
    {
        foreach (var conta in containers)
        {
            if (conta == null) return;
        }

        if (value)
        {
            containers[0].style.display = DisplayStyle.Flex;
            containers[1].style.display = DisplayStyle.None;
        }
        else
        {
            containers[0].style.display = DisplayStyle.None;
            containers[1].style.display = DisplayStyle.Flex;
        }
    }
    private void AddObjectFieldManipulators(ObjectField objectField)
    {
        // Agrega un manejador para el evento de click derecho
        objectField.RegisterCallback<ContextClickEvent>(evt =>
        {
            // Crea un menú de contexto
            GenericMenu menu = new();

            // Agrega una opción para copiar el valor
            menu.AddItem(new GUIContent("Copy"), false, () =>
            {
                // Copia el valor al portapapeles
                EditorGUIUtility.systemCopyBuffer = AssetDatabase.GetAssetPath(objectField.value);
            });

            // Agrega una opción para pegar el valor
            menu.AddItem(new GUIContent("Paste"), false, () =>
            {
                // Pega el valor desde el portapapeles
                objectField.value = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(EditorGUIUtility.systemCopyBuffer);
            });

            // Agrega una opción para abirir la pre visualizacion
            menu.AddItem(new GUIContent("Preview"), false, () =>
            {             
                OpenPreview((AnimationClip)objectField.value);
            });

            // Muestra el menú de contexto
            menu.ShowAsContext();
        });
    }
    private static float ClampAngle(float angle, float min, float max)
    {
        if (angle < -360F)
            angle += 360F;
        if (angle > 360F)
            angle -= 360F;
        return Mathf.Clamp(angle, min, max);
    }

    private void SaveData()
    {
        locomotionMaster.Save();
        SetStartColorToSaveButton();
    }
}