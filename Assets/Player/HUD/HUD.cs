using RTS;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class HUD : MonoBehaviour
{
    public Texture2D[] resourceHealthBars;
    public GUISkin resourceSkin, ordersSkin, selectBoxSkin;
    private Player player;
    public Texture2D activeCursor;
    public Texture2D selectCursor, leftCursor, rightCursor, upCursor, downCursor;
    public Texture2D[] moveCursors, attackCursors, harvestCursor;
    public GUISkin mouseCursorSkin;
    private CursorState activeCursorState;
    private int currentFrame = 0;
    private Dictionary<ResourceType, int> resourceValues, resourceLimits;
    private float ORDERS_BAR_WIDTH = Screen.width * 0.15f, RESOURCE_BAR_HEIGHT = Screen.width * 0.050f;
    private const int SELECTION_NAME_HEIGHT = 15;
    private const int ICON_WIDTH = 32, ICON_HEIGHT = 32, TEXT_WIDTH = 128, TEXT_HEIGHT = 32;
    public Texture2D[] resources;
    private Dictionary<ResourceType, Texture2D> resourceImages;
    public Texture2D smallButtonHover, smallButtonClick;
    private WorldObject lastSelection;
    private float sliderValue;
    public Texture2D rallyPointImage;
    public Texture2D buttonHover, buttonClick;
    private const int BUILD_IMAGE_WIDTH = 64, BUILD_IMAGE_HEIGHT = 64;
    private float buildAreaHeight = 0; /* need to check variable type */
    private const int BUTTON_SPACING = 5;
    private const int SCROLL_BAR_WIDTH = 22;
    private const int BUILD_IMAGE_PADDING = 8;
    public Texture2D buildFrame, buildMask;
    public Texture2D rallyPointCursor;
    private CursorState previousCursorState;
    public Text Food, Wood, Money, Power;
    public Canvas ResourceCanvas;
    public Texture2D healthy, damaged, critical;

    /*** Game Engine Methods ***/
    void Start()
    {
        buildAreaHeight = Screen.height - RESOURCE_BAR_HEIGHT - SELECTION_NAME_HEIGHT - 2 * BUTTON_SPACING;

        player = transform.root.GetComponent<Player>();
        ResourceManager.StoreSelectBoxItems(selectBoxSkin, healthy, damaged, critical);
        SetCursorState(CursorState.Select);
        resourceValues = new Dictionary<ResourceType, int>();
        resourceLimits = new Dictionary<ResourceType, int>();
        resourceImages = new Dictionary<ResourceType, Texture2D>();
        for (int i = 0; i < resources.Length; i++)
        {
            switch (resources[i].name)
            {
                case "Money":
                    resourceImages.Add(ResourceType.Money, resources[i]);
                    resourceValues.Add(ResourceType.Money, 0);
                    resourceLimits.Add(ResourceType.Money, 0);
                    break;
                case "Power":
                    resourceImages.Add(ResourceType.Power, resources[i]);
                    resourceValues.Add(ResourceType.Power, 0);
                    resourceLimits.Add(ResourceType.Power, 0);
                    break;
                case "Food":
                    resourceImages.Add(ResourceType.Food, resources[i]);
                    resourceValues.Add(ResourceType.Food, 0);
                    resourceLimits.Add(ResourceType.Food, 0);
                    break;
                case "Wood":
                    resourceImages.Add(ResourceType.Wood, resources[i]);
                    resourceValues.Add(ResourceType.Wood, 0);
                    resourceLimits.Add(ResourceType.Wood, 0);
                    break;

                default: break;
            }
        }
        Dictionary<ResourceType, Texture2D> resourceHealthBarTextures = new Dictionary<ResourceType, Texture2D>();
        for (int i = 0; i < resourceHealthBars.Length; i++)
        {
            switch (resourceHealthBars[i].name)
            {
                case "ore":
                    resourceHealthBarTextures.Add(ResourceType.Ore, resourceHealthBars[i]);
                    break;
                default: break;
            }
        }
        ResourceManager.SetResourceHealthBarTextures(resourceHealthBarTextures);

    }
    void DisplayResourcesCanvas()
    {
        ResourceCanvas.gameObject.SetActive(true);
        Food.text = resourceValues[ResourceType.Food].ToString() + "/" + resourceLimits[ResourceType.Food].ToString();
        Wood.text = resourceValues[ResourceType.Wood].ToString() + "/" + resourceLimits[ResourceType.Wood].ToString();
        Money.text = resourceValues[ResourceType.Money].ToString() + "/" + resourceLimits[ResourceType.Money].ToString();
    }
    public void SetResourceValues(Dictionary<ResourceType, int> resourceValues, Dictionary<ResourceType, int> resourceLimits)
    {
        this.resourceValues = resourceValues;
        this.resourceLimits = resourceLimits;
    }

    void OnGUI()
    {
        //we only want to draw a GUI for human players
        if (player.human)
        {
            DrawOrdersBar();
            DisplayResourcesCanvas();
            //DrawResourceBar();
            DrawMouseCursor();
        }
    }
    /*** Public methods for interacting with the HUD ***/
    public void SetCursorState(CursorState newState)
    {
        if (activeCursorState != newState) previousCursorState = activeCursorState;
        activeCursorState = newState;
        switch (newState)
        {
            case CursorState.Select:
                activeCursor = selectCursor;
                break;
            case CursorState.Attack:
                currentFrame = (int)Time.time % attackCursors.Length;
                activeCursor = attackCursors[currentFrame];
                break;
            case CursorState.Harvest:
                currentFrame = (int)Time.time % harvestCursor.Length;
                activeCursor = harvestCursor[currentFrame];
                break;
            case CursorState.Move:
                currentFrame = (int)Time.time % moveCursors.Length;
                activeCursor = moveCursors[currentFrame];
                break;
            case CursorState.PanLeft:
                activeCursor = leftCursor;
                break;
            case CursorState.PanRight:
                activeCursor = rightCursor;
                break;
            case CursorState.PanUp:
                activeCursor = upCursor;
                break;
            case CursorState.PanDown:
                activeCursor = downCursor;
                break;
            case CursorState.RallyPoint:
                activeCursor = rallyPointCursor;
                break;
            default: break;
        }
    }
    public bool MouseInBounds()
    {
        //Screen coordinates start in the lower-left corner of the screen
        //not the top-right of the screen like the drawing coordinates do
        Vector3 mousePos = Input.mousePosition;
        bool insideWidth = mousePos.x >= 0 && mousePos.x <= Screen.width - ORDERS_BAR_WIDTH;
        bool insideHeight = mousePos.y >= 0 && mousePos.y <= Screen.height - RESOURCE_BAR_HEIGHT;
        return insideWidth && insideHeight;
    }
    public CursorState GetPreviousCursorState()
    {
        return previousCursorState;
    }
    public Rect GetPlayingArea()
    {
        return new Rect(0, RESOURCE_BAR_HEIGHT, Screen.width - ORDERS_BAR_WIDTH, Screen.height - RESOURCE_BAR_HEIGHT);
    }
    /*** Private Worker Methods ***/
    private Rect GetCursorDrawPosition()
    {
        //set base position for custom cursor image
        float leftPos = Input.mousePosition.x;
        float topPos = Screen.height - Input.mousePosition.y; //screen draw coordinates are inverted
                                                              //adjust position base on the type of cursor being shown
        if (activeCursorState == CursorState.PanRight) leftPos = Screen.width - activeCursor.width;
        else if (activeCursorState == CursorState.PanDown) topPos = Screen.height - activeCursor.height;
        else if (activeCursorState == CursorState.Move || activeCursorState == CursorState.Select || activeCursorState == CursorState.Harvest)
        {
            topPos -= activeCursor.height / 2;
            leftPos -= activeCursor.width / 2;
        }
        else if (activeCursorState == CursorState.RallyPoint) topPos -= activeCursor.height;
        return new Rect(leftPos, topPos, activeCursor.width, activeCursor.height);
    }
    private void UpdateCursorAnimation()
    {
        //sequence animation for cursor (based on more than one image for the cursor)
        //change once per second, loops through array of images
        if (activeCursorState == CursorState.Move)
        {
            currentFrame = (int)Time.time % moveCursors.Length;
            activeCursor = moveCursors[currentFrame];
        }
        else if (activeCursorState == CursorState.Attack)
        {
            currentFrame = (int)Time.time % attackCursors.Length;
            activeCursor = attackCursors[currentFrame];
        }
        else if (activeCursorState == CursorState.Harvest)
        {
            currentFrame = (int)Time.time % harvestCursor.Length;
            activeCursor = harvestCursor[currentFrame];
        }
    }
    private void DrawMouseCursor()
    {
        bool mouseOverHud = !MouseInBounds() && activeCursorState != CursorState.PanRight && activeCursorState != CursorState.PanUp;
        if (mouseOverHud)
        {
            Cursor.visible = true;
        }
        else
        {
            Cursor.visible = false;
            if (!player.IsFindingBuildingLocation())
            {
                GUI.skin = mouseCursorSkin;
                GUI.BeginGroup(new Rect(0, 0, Screen.width, Screen.height));
                UpdateCursorAnimation();
                Rect cursorPosition = GetCursorDrawPosition();
                GUI.Label(cursorPosition, activeCursor);
                GUI.EndGroup();
            }
       
        }
    }
    private void DrawBuildQueue(string[] buildQueue, float buildPercentage)
    {
        for (int i = 0; i < buildQueue.Length; i++)
        {
            float topPos = i * BUILD_IMAGE_HEIGHT - (i + 1) * BUILD_IMAGE_PADDING;
            Rect buildPos = new(BUILD_IMAGE_PADDING, topPos, BUILD_IMAGE_WIDTH, BUILD_IMAGE_HEIGHT);
            GUI.DrawTexture(buildPos, ResourceManager.GetBuildImage(buildQueue[i]));
            GUI.DrawTexture(buildPos, buildFrame);
            topPos += BUILD_IMAGE_PADDING;
            float width = BUILD_IMAGE_WIDTH - 2 * BUILD_IMAGE_PADDING;
            float height = BUILD_IMAGE_HEIGHT - 2 * BUILD_IMAGE_PADDING;
            if (i == 0)
            {
                //shrink the build mask on the item currently being built to give an idea of progress
                topPos += height * buildPercentage;
                height *= (1 - buildPercentage);
            }
            GUI.DrawTexture(new Rect(2 * BUILD_IMAGE_PADDING, topPos, width, height), buildMask);
        }
    }
    public CursorState GetCursorState()
    {
        return activeCursorState;
    }
    private void DrawStandardBuildingOptions(Building building)
    {
        GUIStyle buttons = new();
        buttons.hover.background = smallButtonHover;
        buttons.active.background = smallButtonClick;
        GUI.skin.button = buttons;
        int leftPos = BUILD_IMAGE_WIDTH + SCROLL_BAR_WIDTH + BUTTON_SPACING;
        float topPos = buildAreaHeight - BUILD_IMAGE_HEIGHT / 2;
        int width = BUILD_IMAGE_WIDTH / 2;
        int height = BUILD_IMAGE_HEIGHT / 2;
        if (GUI.Button(new Rect(leftPos, topPos, width, height), building.sellImage))
        {
            building.Sell();
        }
        if (building.hasSpawnPoint())
        {
            leftPos += width + BUTTON_SPACING;
            if (GUI.Button(new Rect(leftPos, topPos, width, height), building.rallyPointImage))
            {
                if (activeCursorState != CursorState.RallyPoint && previousCursorState != CursorState.RallyPoint) SetCursorState(CursorState.RallyPoint);
                else
                {
                    //dirty hack to ensure toggle between RallyPoint and not works ...
                    SetCursorState(CursorState.PanRight);
                    SetCursorState(CursorState.Select);
                }
            }
        }
    }
    private void DrawOrdersBar()
    {

        /* Part 10: Drw units to create */
        if (player.SelectedObject != null)
        {
            GUI.skin = ordersSkin;



            GUI.BeginGroup(new Rect(Screen.width - ORDERS_BAR_WIDTH - BUILD_IMAGE_WIDTH, RESOURCE_BAR_HEIGHT, ORDERS_BAR_WIDTH + BUILD_IMAGE_WIDTH, Screen.height - RESOURCE_BAR_HEIGHT));
            GUI.Box(new Rect(BUILD_IMAGE_WIDTH + SCROLL_BAR_WIDTH, 0, ORDERS_BAR_WIDTH, Screen.height - RESOURCE_BAR_HEIGHT), "");
            string selectionName = "";
            if (player.SelectedObject)
            {
                selectionName = player.SelectedObject.objectName;
            }

            if (player.SelectedObject.IsOwnedBy(player))
            {
                //reset slider value if the selected object has changed
                if (lastSelection && lastSelection != player.SelectedObject) sliderValue = 0.0f;
                DrawActions(player.SelectedObject.GetActions());
                //store the current selection
                lastSelection = player.SelectedObject;
                Building selectedBuilding = lastSelection.GetComponent<Building>();
                if (selectedBuilding)
                {
                    DrawBuildQueue(selectedBuilding.getBuildQueueValues(), selectedBuilding.getBuildPercentage());
                    DrawStandardBuildingOptions(selectedBuilding);
                }
            }
            if (!selectionName.Equals(""))
            {
                int leftPos = BUILD_IMAGE_WIDTH + SCROLL_BAR_WIDTH / 2;
                float topPos = buildAreaHeight + BUTTON_SPACING;
                GUI.Label(new Rect(leftPos, topPos, ORDERS_BAR_WIDTH, SELECTION_NAME_HEIGHT), selectionName);
            }

            GUI.EndGroup();
        }

    }

    private void DrawActions(string[] actions)
    {
        GUIStyle buttons = new();
        buttons.hover.background = buttonHover;
        buttons.active.background = buttonClick;
        Debug.Log(GUI.skin.button.margin);
        GUI.skin.button = buttons;
        Debug.Log(GUI.skin.button.margin);

        int numActions = actions.Length;
        //define the area to draw the actions inside 
        //TODO:Fix Drawing Postion Replace Gui Skin with new solution 
        GUI.BeginGroup(new Rect(BUILD_IMAGE_WIDTH+10, 20, ORDERS_BAR_WIDTH, buildAreaHeight));
        //draw scroll bar for the list of actions if need be
        if (numActions >= MaxNumRows((int)buildAreaHeight)) DrawSlider((int)buildAreaHeight, numActions / 2.0f);
        //display possible actions as buttons and handle the button click for each
        for (int i = 0; i < numActions; i++)
        {
            int column = i % 2;
            int row = i / 2;
            Rect pos = GetButtonPos(row, column);
            Texture2D action = ResourceManager.GetBuildImage(actions[i]);
            if (action)
            {
                //create the button and handle the click of that button
                //GUIStyle btn = new();
                //btn.padding = new(10, 10, 10, 10);
                if (GUI.Button(pos, action))
                {
                    if (player.SelectedObject) player.SelectedObject.PerformAction(actions[i]);
                }
            }
        }
        GUI.EndGroup();
    }
    private int MaxNumRows(int areaHeight)
    {
        return areaHeight / BUILD_IMAGE_HEIGHT;
    }

    private Rect GetButtonPos(int row, int column)
    {
        int left = SCROLL_BAR_WIDTH + column * BUILD_IMAGE_WIDTH;
        float top = row * BUILD_IMAGE_HEIGHT - sliderValue * BUILD_IMAGE_HEIGHT;
        return new Rect(left, top, BUILD_IMAGE_WIDTH, BUILD_IMAGE_HEIGHT);
    }

    private void DrawSlider(int groupHeight, float numRows)
    {
        //slider goes from 0 to the number of rows that do not fit on screen
        sliderValue = GUI.VerticalSlider(GetScrollPos(groupHeight), sliderValue, 0.0f, numRows - MaxNumRows(groupHeight));
    }

    private Rect GetScrollPos(int groupHeight)
    {
        return new Rect(BUTTON_SPACING, BUTTON_SPACING, SCROLL_BAR_WIDTH, groupHeight - 2 * BUTTON_SPACING);
    }
    private void DrawResourceIcon(ResourceType type, int iconLeft, int textLeft, int topPos)
    {
        Texture2D icon = resourceImages[type];
        string text = resourceValues[type].ToString() + "/" + resourceLimits[type].ToString();
        GUI.DrawTexture(new Rect(iconLeft, topPos, ICON_WIDTH, ICON_HEIGHT), icon);
        GUI.Label(new Rect(textLeft, topPos, TEXT_WIDTH, TEXT_HEIGHT), text);
    }
    //OLD IMPLEMENTATION OF THE RESOURCES BAR MOVED TO NEW IMPLEMENTATION USING CANVAS
    private void DrawResourceBar()
    {
        GUI.skin = resourceSkin;
        GUI.BeginGroup(new Rect(0, 0, Screen.width, RESOURCE_BAR_HEIGHT));
        GUI.Box(new Rect(0, 0, Screen.width, RESOURCE_BAR_HEIGHT), "");
        int topPos = 4, iconLeft = 4, textLeft = 20;
        DrawResourceIcon(ResourceType.Money, iconLeft, textLeft, topPos);
        iconLeft += TEXT_WIDTH;
        textLeft += TEXT_WIDTH;
        DrawResourceIcon(ResourceType.Power, iconLeft, textLeft, topPos);
        iconLeft += TEXT_WIDTH;
        textLeft += TEXT_WIDTH;
        DrawResourceIcon(ResourceType.Food, iconLeft, textLeft, topPos);
        iconLeft += TEXT_WIDTH;
        textLeft += TEXT_WIDTH;
        DrawResourceIcon(ResourceType.Wood, iconLeft, textLeft, topPos);
        GUI.EndGroup();
    }
}