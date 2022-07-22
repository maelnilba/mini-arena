using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class UIDockSpell : MonoBehaviour
{
    public Button turnbutton;
    public Button[] spellButtons = new Button[6];
    private GroupBox spellButtonUI;
    public FightUIManager UIManager;

    private void Awake()
    {
        UIManager = GetComponentInParent<FightUIManager>();

        var root = GetComponent<UIDocument>().rootVisualElement;
        spellButtonUI = root.Q<GroupBox>("spellbuttons");
        spellButtonUI.RegisterCallback<MouseEnterEvent>(e => onHover());
        spellButtonUI.RegisterCallback<MouseLeaveEvent>(e => onBlur());

        spellButtons[0] = root.Query<Button>("spellbutton1");
        spellButtons[0].clicked += SelectSpell1;
        spellButtons[1] = root.Query<Button>("spellbutton2");
        spellButtons[1].clicked += SelectSpell2;
        spellButtons[2] = root.Query<Button>("spellbutton3");
        spellButtons[2].clicked += SelectSpell3;
        spellButtons[3] = root.Query<Button>("spellbutton4");
        spellButtons[3].clicked += SelectSpell4;
        spellButtons[4] = root.Query<Button>("spellbutton5");
        spellButtons[4].clicked += SelectSpell5;
        spellButtons[5] = root.Query<Button>("spellbutton6");
        spellButtons[5].clicked += SelectSpell6;
    }

    public void Display(bool show)
    {
        spellButtonUI.style.display = show ? DisplayStyle.Flex : DisplayStyle.None;
    }

    public void onHover()
    {
        UIManager.onHover();
    }

    public void onBlur()
    {
        UIManager.onBlur();
    }

    private void SelectSpell1()
    {
        UIManager.UIAction(1, 0);
    }

    private void SelectSpell2()
    {
        UIManager.UIAction(1, 1);
    }

    private void SelectSpell3()
    {
        UIManager.UIAction(1, 2);
    }

    private void SelectSpell4()
    {
        UIManager.UIAction(1, 3);
    }

    private void SelectSpell5()
    {
        UIManager.UIAction(1, 4);
    }

    private void SelectSpell6()
    {
        UIManager.UIAction(1, 5);
    }
}
