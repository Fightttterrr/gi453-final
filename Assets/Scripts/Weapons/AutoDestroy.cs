using UnityEngine;

public class AutoDestroy : MonoBehaviour
{
    public float lifetime = 0.5f;
    void Start()
    {
        Destroy(gameObject, lifetime);
    }

    
}
