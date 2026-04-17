using UnityEngine;

public class MultiplayerLobbyManager : MonoBehaviour
{
    // TEST SCRIPT TO POPULATE LOBBIES

    [SerializeField] private Transform contentParent;
    [SerializeField] private GameObject lobbyItemPrefab;

    void OnEnable()
    {
        PopulateLobbies();
    }

    void PopulateLobbies()
    {
        foreach (Transform child in contentParent)
            Destroy(child.gameObject);

        CreateLobby("Jack", 1);
        CreateLobby("Arthur", 2);
        CreateLobby("Renz The Third", 1);
    }

    void CreateLobby(string owner, int players)
    {
        GameObject lobby = Instantiate(lobbyItemPrefab, contentParent);
        lobby.GetComponent<LobbyItemUI>().Setup(owner, players);
    }
}
