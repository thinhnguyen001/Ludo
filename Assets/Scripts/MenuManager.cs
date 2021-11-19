using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MenuManager : MonoBehaviour
{
    [SerializeField]
    List<GameObject> playerNames;

    // Start is called before the first frame update
    private void Awake()
    {
        for (int i = 0; i < 4; i++)
        {
            playerNames[i].GetComponentInChildren<InputField>().text =
                PlayerPrefs.HasKey((i + 1).ToString()) ? PlayerPrefs.GetString((i + 1).ToString()) : "Player" + (i + 1);
            playerNames[i].GetComponentInChildren<InputField>().placeholder.GetComponent<Text>().text =
                PlayerPrefs.HasKey((i + 1).ToString()) ? PlayerPrefs.GetString((i + 1).ToString()) : "Player" + (i + 1);
        }
    }

    public void SetPlayerNumber()
    {
        string playerNumber = UnityEngine.EventSystems.EventSystem.current.currentSelectedGameObject.name;
        foreach(GameObject playerName in playerNames)
        {
            playerName.SetActive(false);
        }

        for (int i = 0; i < int.Parse(playerNumber); i++)
        {
            playerNames[i].SetActive(true);
        }

        PlayerPrefs.SetInt("players", int.Parse(playerNumber));
    }

    public void SetPlayer1Name(string temp)
    {
        PlayerPrefs.SetString("1", temp);
    }

    public void SetPlayer2Name(string temp)
    {
        PlayerPrefs.SetString("2", temp);
    }

    public void SetPlayer3Name(string temp)
    {
        PlayerPrefs.SetString("3", temp);
    }

    public void SetPlayer4Name(string temp)
    {
        PlayerPrefs.SetString("4", temp);
    }

    public void StartGame()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(1);
    }
}
