using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FightUIManager : MonoBehaviour
{
    // Start is called before the first frame update
    [SerializeField] GameObject[] UIComponents;

    private GameObject Dock;
    private GameObject TL;


    public UITimeline Timeline;
    public UIDockSpell DockSpell;

    public List<Player> players;


    public GridController fightHandler;
    public FightActionsDisplay fightActionsDisplay;
    public MapBuild mapBuild;

    public bool PointerOnUI = false;

    private enum ActionType
    {
        None = 0,
        SelectSpell = 1,
        HoverTimelinePlayer = 2,
        BlurTimelinePlayer = 4,
        ClickTimelinePlayer = 3,
    }

    private void Awake()
    {
        if (UIComponents.Length == 0)
            throw new MissingComponentException("No Component UI prefabs in List");
        Dock = Instantiate(UIComponents[1], transform);
        DockSpell = Dock.GetComponent<UIDockSpell>();

        fightHandler = GetComponent<GridController>();
        fightActionsDisplay = GetComponent<FightActionsDisplay>();
        mapBuild = GetComponent<MapBuild>();


        TL = Instantiate(UIComponents[0], transform);
        Timeline = TL.GetComponent<UITimeline>();
    }


    public void Setup(List<Player> players, Observable<int> currPlayer)
    {
        this.players = players;
        Timeline.Setup(players);
        currPlayer.OnChanged += Timeline.NextTurn; // manage the all timeline actions automated

        DockSpell.Display(false);
        Timeline.Display(false);
    }

    public void Display(bool show)
    {
        DockSpell.Display(show);
        Timeline.Display(show);
    }

    public void onHover()
    {
        PointerOnUI = true;
    }

    public void onBlur()
    {
        PointerOnUI = false;
    }

    public void UIAction(int actionType, int value)
    {
        ActionType action = (ActionType)actionType;

        switch (action)
        {
            case ActionType.SelectSpell:
                fightActionsDisplay.SetSelectedSpell(value);
                break;
            case ActionType.HoverTimelinePlayer:
                if (fightActionsDisplay.isLookingForRange)
                {
                    fightHandler.Hover(fightHandler.players[value].playerPosition);
                }
                break;
            case ActionType.BlurTimelinePlayer:
                if (fightActionsDisplay.isLookingForRange)
                {
                    fightHandler.Hover(Vector3Int.back);
                }
                break;
            case ActionType.ClickTimelinePlayer:
                if (PointerOnUI)
                    if (fightActionsDisplay.isLookingForRange)
                    {
                        if (fightActionsDisplay.ConfirmationCanSpellCastFromTimeline())
                            fightActionsDisplay.SpellCastTimeline();
                        else
                            Debug.Log("SHOW PLAYER BUFF");
                    } else
                    {
                        Debug.Log("SHOW PLAYER BUFF");
                    }
                break;
            default:
                break;
        }
    }
}
