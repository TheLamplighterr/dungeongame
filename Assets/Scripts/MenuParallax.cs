using UnityEngine;

public class MenuParallax : MonoBehaviour
{

    public float offsetMultiplier = 1f;
    public float smoothTime = .3f;        
    
    public Vector2 startPosition;
    public Vector3 velocity;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    startPosition = transform.position;

    }

    // Update is called once per frame
    void Update()
    {
        Vector2 offset = Camera.main.ScreenToViewportPoint(Input.mousePosition);
        transform.position = Vector3.SmoothDamp(transform.position, startPosition + (offset*offsetMultiplier),ref velocity, smoothTime);
    }
}
