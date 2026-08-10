using UnityEngine;

public class MapResetTrigger : MonoBehaviour
{
    MapManager mapManager;

    BoxCollider myCollider;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        mapManager = FindObjectOfType<MapManager>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (mapManager != null && other.CompareTag("Player")) 
        {
            mapManager.generateNewLevel();
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
