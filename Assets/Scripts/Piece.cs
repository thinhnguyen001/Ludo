using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Piece : MonoBehaviour
{

    [SerializeField]
    List<Color> colors;

    bool canMove;
    int currentIndex;
    List<Vector3> movePos;
    float speed = 10f;

    private void Awake()
    {
        canMove = false;
        currentIndex = 0;
    }

    public void SetColor(Player player)
    {
        SpriteRenderer renderer = GetComponent<SpriteRenderer>();
        renderer.color = colors[(int)player];
    }

    // Update is called once per frame
    void Update()
    {
        if (!canMove) return;

        float step = speed * Time.deltaTime;
        transform.position = Vector3.MoveTowards(transform.position, movePos[currentIndex], step);
        if(Vector3.Distance(transform.position,movePos[currentIndex]) < 0.001f)
        {
            currentIndex++;
            if(currentIndex == movePos.Count)
            {
                currentIndex = 0;
                canMove = false;
                GameManager.instance.MoveEnd();
            }
        }
    }

    public void SetPos(List<Vector3> temp)
    {
        movePos = temp;
        if (temp.Count == 0) return;
        canMove = true;
    }
}
