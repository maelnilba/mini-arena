using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public enum TargetMask
{
    None = 0,
    Allies = 1,
    Ennemies = 2,
    All = 3,
}

public enum Area
{
    None = 0,
    Normal = 1,
    Diagonal = 2,
    Line = 3,
    Square = 4,
    Cross = 5,
    Mono = 10,
    VLine = 11,
    HLine = 12,
    Plus = 13,
    Hammer = 15,
    Ring = 16,
    Halfcircle = 17,
}

public enum EffectType
{
    None = 0,
    Damage = 1,
    Attraction = 2,
    Push = 3,
    Transposition = 4,
    Boost = 5,
    Malus = 6,
    Heal = 8,

}

public enum BuffType
{
    None,
    PA,
    PM,
    PO,
    Do,
    DoMe,
    DoDi,
    PV
}

public class Spell
{
    public string name;
    public int id;
    public int spellId;
    public int paCost;
    public int minRange;
    public int range;
    public List<Area> areas;
    public bool lineOfSigh;
    public bool needTarget;
    public bool needFreeCell;
    public bool rangeCanBeBoosted;
    public int maxStack;
    public int maxCastPerTurn;
    public int maxCastPerTarget;
    public int minCastInterval;
    public int initialCooldown;
    public List<SpellEffect> effects;

    public Spell(string name, int id, int spellId, int paCost, int minRange, int range, List<int> areasId, bool lineOfSigh, bool needTarget, bool needFreeCell, bool rangeCanBeBoosted, int maxStack, int maxCastPerTurn, int maxCastPerTarget, int minCastInterval, int initialCooldown, List<SpellEffect> effects)
    {
        this.name = name;
        this.id = id;
        this.spellId = spellId;
        this.paCost = paCost;
        this.minRange = minRange;
        this.range = range;

        foreach (int aid in areasId)
            areas.Add((Area)aid);

        this.lineOfSigh = lineOfSigh;
        this.needTarget = needTarget;
        this.needFreeCell = needFreeCell;
        this.rangeCanBeBoosted = rangeCanBeBoosted;
        this.maxStack = maxStack;
        this.maxCastPerTurn = maxCastPerTurn;
        this.maxCastPerTarget = maxCastPerTarget;
        this.minCastInterval = minCastInterval;
        this.initialCooldown = initialCooldown;
        this.effects = effects;
    }

    public Spell(string name, int id, int spellId, int paCost, int minRange, int range, int area, bool lineOfSigh, bool needTarget, bool needFreeCell, bool rangeCanBeBoosted, int maxStack, int maxCastPerTurn, int maxCastPerTarget, int minCastInterval, int initialCooldown, List<SpellEffect> effects)
    {
        this.name = name;
        this.id = id;
        this.spellId = spellId;
        this.paCost = paCost;
        this.minRange = minRange;
        this.range = range;
        areas = new List<Area>() { (Area)area };
        this.lineOfSigh = lineOfSigh;
        this.needTarget = needTarget;
        this.needFreeCell = needFreeCell;
        this.rangeCanBeBoosted = rangeCanBeBoosted;
        this.maxStack = maxStack;
        this.maxCastPerTurn = maxCastPerTurn;
        this.maxCastPerTarget = maxCastPerTarget;
        this.minCastInterval = minCastInterval;
        this.initialCooldown = initialCooldown;
        this.effects = effects;
    }

    public Spell(string name, int id, int spellId, int paCost, int minRange, int range, List<int> areasId, bool lineOfSigh, bool needTarget, bool needFreeCell, bool rangeCanBeBoosted, int maxStack, int maxCastPerTurn, int maxCastPerTarget, int minCastInterval, int initialCooldown, SpellEffect effect)
    {
        this.name = name;
        this.id = id;
        this.spellId = spellId;
        this.paCost = paCost;
        this.minRange = minRange;
        this.range = range;

        foreach (int aid in areasId)
            areas.Add((Area)aid);

        this.lineOfSigh = lineOfSigh;
        this.needTarget = needTarget;
        this.needFreeCell = needFreeCell;
        this.rangeCanBeBoosted = rangeCanBeBoosted;
        this.maxStack = maxStack;
        this.maxCastPerTurn = maxCastPerTurn;
        this.maxCastPerTarget = maxCastPerTarget;
        this.minCastInterval = minCastInterval;
        this.initialCooldown = initialCooldown;
        effects = new List<SpellEffect>() { effect };
    }

    public Spell(string name, int id, int spellId, int paCost, int minRange, int range, int area, bool lineOfSigh, bool needTarget, bool needFreeCell, bool rangeCanBeBoosted, int maxStack, int maxCastPerTurn, int maxCastPerTarget, int minCastInterval, int initialCooldown, SpellEffect effect)
    {
        this.name = name;
        this.id = id;
        this.spellId = spellId;
        this.paCost = paCost;
        this.minRange = minRange;
        this.range = range;
        areas = new List<Area>() { (Area)area };
        this.lineOfSigh = lineOfSigh;
        this.needTarget = needTarget;
        this.needFreeCell = needFreeCell;
        this.rangeCanBeBoosted = rangeCanBeBoosted;
        this.maxStack = maxStack;
        this.maxCastPerTurn = maxCastPerTurn;
        this.maxCastPerTarget = maxCastPerTarget;
        this.minCastInterval = minCastInterval;
        this.initialCooldown = initialCooldown;
        effects = new List<SpellEffect>() { effect };
    }


}


public class SpellEffect
{
    public int id;
    public TargetMask targetMask;
    public EffectType effectType;
    public BuffType buffType;
    public int diceStart;
    public int diceEnd;
    public Area area;
    public int areaRange;
    public bool inLog;

    public SpellEffect(int id, EffectType effectType, BuffType buffType, int diceStart, int diceEnd, int areaId, int areaRange, bool inLog)
    {
        this.id = id;
        this.effectType = effectType;
        this.buffType = buffType;
        this.diceStart = diceStart;
        this.diceEnd = diceEnd;
        area = (Area)areaId;
        this.areaRange = areaRange;
        this.inLog = inLog;
    }
}

public static class SpellUtils
{
    public static HighlightType EffectTypeToHighlighType(EffectType effectType)
    {
        switch (effectType)
        {
            case EffectType.None:
                return HighlightType.None;
            case EffectType.Damage:
                return HighlightType.Target;
            case EffectType.Attraction:
                return HighlightType.Target;
            case EffectType.Push:
                return HighlightType.Target;
            case EffectType.Transposition:
                return HighlightType.Target;
            case EffectType.Boost:
                return HighlightType.Buff;
            case EffectType.Heal:
                return HighlightType.Target;
            default:
                return HighlightType.None;
        }
    }
}