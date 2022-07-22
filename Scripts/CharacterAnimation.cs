using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterAnimation : MonoBehaviour
{
    private Animator anim;
    private string current;

    const string HAUT = "HAUT";
    const string BAS = "BAS";
    const string GAUCHE = "GAUCHE";
    const string DROITE = "DROITE";

    const string STATIC = "Statique";
    const string WALK = "Walk";
    const string CAST = "Cast";
    const string HIT = "Hit";

    bool isStatic;
    //
    //     Y+
    // X-      X+
    //     Y-
    //

    private void Awake()
    {
        anim = GetComponent<Animator>();
    }

    private void Update()
    {
        if (anim.GetCurrentAnimatorStateInfo(0).normalizedTime > 1 && !isStatic)
            SetAnimation(STATIC, current);
            
    }

    private void SetAnimation(string type, string name)
    {
        if (isStatic)
            isStatic = true;

        anim.Play($"{type} {name}");
        current = name;
    }

    public void WalkTo(Vector2Int position, Vector2Int target)
    {
        if (position.x == target.x)
        {
            if (position.y < target.y)
            {
                SetAnimation(WALK, GAUCHE);
            } else
            {
                SetAnimation(WALK ,DROITE);
            }
        }

        if (position.y == target.y)
        {
            if (position.x < target.x)
            {
                SetAnimation(WALK, HAUT);
            } else
            {
                SetAnimation(WALK, BAS);
            }
        }
    }

    public void StopAnimation()
    {
        anim.Play($"Statique {current}");
    }

    public void LookTo(Vector2Int position, Vector2Int target)
    {
        if (position.x == target.x)
        {
            if (position.y < target.y)
            {
                SetAnimation(STATIC, GAUCHE);
            }
            else
            {
                SetAnimation(STATIC, DROITE);
            }
        }

        if (position.y == target.y)
        {
            if (position.x < target.x)
            {
                SetAnimation(STATIC, HAUT);
            }
            else
            {
                SetAnimation(STATIC, BAS);
            }
        }
    }

    public void LookToDirection(Vector2Int position, Vector2Int target)
    {

        if (position == target)
            return;

        int distanceX = Mathf.Abs(target.x - position.x);
        int distanceY = Mathf.Abs(target.y - position.y);

        if (distanceX < distanceY) // plus proche de X
        {
            if (position.x > target.x)
            {
                // target vers la gauche
                if (position.y > target.y)
                {
                    // target vers le bas
                    SetAnimation(STATIC, DROITE);

                }
                else
                {
                    // target vers le haut
                    SetAnimation(STATIC, GAUCHE);

                }
            }
            else
            {
                // target vers la droite
                if (position.y > target.y)
                {
                    // target vers le bas
                    SetAnimation(STATIC, DROITE);

                }
                else
                {
                    // target vers le haut
                    SetAnimation(STATIC, GAUCHE);

                }
            }
        }

        if (distanceX > distanceY) // plus proche de Y, enlever = pour diagonal après
        {
            if (position.x > target.x)
            {
                // target vers la gauche
                if (position.y > target.y)
                {
                    // target vers le bas
                    SetAnimation(STATIC, BAS);

                }
                else
                {
                    // target vers le haut
                    SetAnimation(STATIC, BAS);

                }
            }
            else
            {
                // target vers la droite
                if (position.y > target.y)
                {
                    // target vers le bas
                    SetAnimation(STATIC, HAUT);

                }
                else
                {
                    // target vers le haut
                    SetAnimation(STATIC, HAUT);

                }
            }
        }

        if (distanceX == distanceY)
        {
            if (position.x > target.x)
            {
                // target vers la gauche
                if (position.y > target.y)
                {
                    // target vers le bas
                    if (!(current == BAS || current == DROITE))
                        SetAnimation(STATIC, BAS);


                }
                else
                {
                    // target vers le haut
                    if (!(current == BAS || current == GAUCHE))
                        SetAnimation(STATIC, GAUCHE);

                }
            }
            else
            {
                // target vers la droite
                if (position.y > target.y)
                {
                    // target vers le bas
                    if (!(current == HAUT || current == DROITE))
                        SetAnimation(STATIC, DROITE);

                }
                else
                {
                    // target vers le haut
                    if (!(current == HAUT || current == GAUCHE))
                        SetAnimation(STATIC, HAUT);

                }
            }
        }
    }

    public void CastSpell()
    {
        SetAnimation(CAST, current);
        isStatic = false;

    }

    public void TakeDamage()
    {
        SetAnimation(HIT, current);
        isStatic = false;
    }
}
