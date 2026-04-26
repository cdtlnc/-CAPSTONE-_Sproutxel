using Unity.VisualScripting;
using UnityEngine;

public class MaintenencePopUp : MonoBehaviour
{
    public GameObject panels;
    public Camera cam;
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
