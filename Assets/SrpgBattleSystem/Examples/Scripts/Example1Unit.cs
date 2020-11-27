using System;
using UnityEngine;

public class Example1Unit : SrpgSingleImageUnit
{
    public GameObject hpBarObj;

    public enum CustomState
    {
        Idle = 0,
        ReadyToMove,
        ReadyToAttack,
        End
    }
    private CustomState _state = CustomState.Idle;
    public CustomState State
    {
        get { return _state; }
    }

    private int _moveRange = 3;
    public int moveRange
    {
        get { return _moveRange; }
    }

    private int _attackRange = 1;
    public int attackRange
    {
        get { return _attackRange; }
    }

    private int _offence = 7;
    public int offence { get { return _offence; } }
    private int _defence = 5;
    public int defence { get { return _defence; } }
    private int _maxHp = 10;
    private int _hp = 10;
    public int hp { get { return _hp; } }

    public override void Scale(float unitScale)
    {
        base.Scale(unitScale);

        SpriteRenderer unitRndr = GetComponent<SpriteRenderer>();
        SpriteRenderer hpBarRndr = hpBarObj.GetComponent<SpriteRenderer>();
        float hpBarScale = boundWidth / hpBarRndr.bounds.size.x;
        hpBarObj.transform.localScale = new Vector3(hpBarScale, hpBarScale, 1);
        hpBarObj.transform.position = transform.position - new Vector3(boundWidth/2, (boundHeight-unitRndr.bounds.size.y)/2, 0);
    }

    public override void Enliven()
    {
        _state = CustomState.Idle;
        GetComponent<Animator>().SetBool("End", false);
    }

    public override bool IsEnd()
    {
        return (_state == CustomState.End);
    }

    public void SetState(CustomState state)
    {
        _state = state;

        if(state == CustomState.End)
            GetComponent<Animator>().SetBool("End", true);
    }

    public int GetMaximumRange()
    {
        return _moveRange + _attackRange;
    }

    public override void BeginIdleAnimation()
    {
        GetComponent<Animator>().SetBool("Walking", false);
        GetComponent<SpriteRenderer>().color = Color.white;
    }

    public override void BeginMoveAnimation(SrpgNode startNode, SrpgNode destNode)
    {
        Animator animator = GetComponent<Animator>();
        animator.SetBool("Walking", true);

        if (destNode.x == startNode.x && destNode.y > startNode.y)
        {
            // up
            animator.SetFloat("DirectionX", 0f);
            animator.SetFloat("DirectionY", 1f);
        }
        else if (destNode.x == startNode.x && destNode.y < startNode.y)
        {
            // down
            animator.SetFloat("DirectionX", 0f);
            animator.SetFloat("DirectionY", -1f);
        }
        else if (destNode.x > startNode.x && destNode.y == startNode.y)
        {
            // right
            animator.SetFloat("DirectionX", 1f);
            animator.SetFloat("DirectionY", 0f);
        }
        else if (destNode.x < startNode.x && destNode.y == startNode.y)
        {
            // left
            animator.SetFloat("DirectionX", -1f);
            animator.SetFloat("DirectionY", 0f);
        }
        else
        {
            Debug.Log("Move Diagonal");
        }
    }

    public void GetDamage(int damage)
    {
        SpriteRenderer unitRndr = GetComponent<SpriteRenderer>();
        SpriteRenderer hpBarRndr = hpBarObj.GetComponent<SpriteRenderer>();

        unitRndr.color = Color.red;

        _hp -= damage;
        if (_hp < 0)
            _hp = 0;

        if(_hp > 0)
        {
            hpBarObj.transform.localScale = Vector3.one;

            float defaultScale = unitRndr.bounds.size.x / hpBarRndr.bounds.size.x;
            float hpRatio = _maxHp / _hp;
            hpBarObj.transform.localScale = new Vector3(defaultScale / hpRatio, defaultScale, 1);
        }
    }
}
