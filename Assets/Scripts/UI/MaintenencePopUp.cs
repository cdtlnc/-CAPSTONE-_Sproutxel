using UnityEngine;

public class MaintenencePopUp : MonoBehaviour
{
    public GameObject panels; //Just to open up the panels
    public Camera cam; // Maybe to change cam positions when its time
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

        panels.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void OpenMaintenence()
    {
        panels.SetActive(true);
    }
    public void CloseMaintenence()
    {
        panels.SetActive(false);
    }
}
