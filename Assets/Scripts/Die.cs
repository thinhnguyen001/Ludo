using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Die : MonoBehaviour
{
    [SerializeField]
    List<Sprite> die;

    int roll;

    public void RollRandom()
    {
        SpriteRenderer renderer = GetComponent<SpriteRenderer>();
        renderer.sprite = die[Random.Range(0, die.Count)];
    }

    public void RollDie(int temp)
    {
        roll = temp;
        Animator animator = GetComponent<Animator>();
        animator.Play("Roll", -1, 0f);
    }

    public void SetRoll()
    {
        SpriteRenderer renderer = GetComponent<SpriteRenderer>();
        renderer.sprite = die[roll - 1];
        GameManager.instance.RollEnd();
    }
}
