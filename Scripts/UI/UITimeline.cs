using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;


public class UITimeline : MonoBehaviour
{
    [SerializeField] VisualTreeAsset PreviewTemplate;
    private WaitForSeconds oneSecond = new WaitForSeconds(1f);
    private VisualElement Timeline;
    private List<VisualElement> TimelinePreview;
    private int currentIndex = 0;

    public FightUIManager UIManager;

    private void Awake() // need to be in awake before setup log before start
    {
        UIManager = GetComponentInParent<FightUIManager>();
        var root = GetComponent<UIDocument>().rootVisualElement;
        Timeline = root.Q<VisualElement>("timeline");
    }

    public void Setup(List<Player> players)
    {
        TimelinePreview = new List<VisualElement>();
        foreach (Player player in players)
        {
            VisualElement pre = PreviewTemplate.CloneTree();
            if (pre == null)
                throw new MissingReferenceException("Template of Timeline failed");

            pre.RegisterCallback<MouseEnterEvent>(e => onMouseE(player));
            pre.RegisterCallback<MouseLeaveEvent>(e => onMouseL(player));
            pre.RegisterCallback<ClickEvent>(e => onMouseC(player));


            if (player.playerTeam == 0)
                pre.Query<VisualElement>("background").First().style.backgroundColor = new StyleColor(Color.blue);
            else
                pre.Query<VisualElement>("background").First().style.backgroundColor = new StyleColor(Color.red);

            TimelinePreview.Add(pre);
            Timeline.Add(pre);

        }

    }

    public void Display(bool show)
    {
        Timeline.style.display = show ? DisplayStyle.Flex : DisplayStyle.None;
    }

    public void onHover()
    {
        UIManager.onHover();
    }

    public void onBlur()
    {
        UIManager.onBlur();
    }

    public void NextTurn(Observable<int> _, int oldindex ,int index)
    {
        if (TimelinePreview == null)
            return;

        if (oldindex != -1)
        {
            VisualElement oldVE = TimelinePreview[oldindex].Q<VisualElement>("arrow");
            oldVE.style.display = DisplayStyle.None;
            VisualElement oldTimer = TimelinePreview[oldindex].Query<VisualElement>("bgtime").First();
            oldTimer.style.height = 0;
            StopCoroutine("Timer");
        }

        TimelinePreview[index].Q<VisualElement>("arrow").style.display = DisplayStyle.Flex;
        currentIndex = index;
        StartCoroutine("Timer");
    }

    private void onMouseE(Player p)
    {
        onHover();
        p.ShowLPI(true);
        UIManager.UIAction(2, p.playerId);
    }

    private void onMouseL(Player p)
    {
        onBlur();
        p.ShowLPI(false);
        UIManager.UIAction(4, p.playerId);
    }

    private void onMouseC(Player p)
    {
        UIManager.UIAction(3, p.playerId);
    }

    private IEnumerator Timer()
    {
        VisualElement bgTimer = TimelinePreview[currentIndex].Query<VisualElement>("bgtime").First();

        for (int i = 0; i < 60; i++)
        {
            bgTimer.style.height = i*1.4f;
            yield return oneSecond;
        }
    }

}
