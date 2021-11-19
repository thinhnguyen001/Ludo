using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Turn : MonoBehaviour
{
    [SerializeField]
    Player _player;

    [SerializeField]
    GameObject playerImage;
    // Start is called before the first frame update
    void Start()
    {
        GameManager.instance.Message += SetTurn;
    }

    void SetTurn(Player player)
    {
        playerImage.SetActive(player == _player);
    }
}
