using UnityEngine;

// TerrainBase is an abstract class for implementing interactive terrain like fire, smoke, oil, etc.
// To use it, make a new class inheriting from Terrain and implement ApplyEffect.
// For example, see Fire.cs
public abstract class TerrainBase : MonoBehaviour
{

    public BoxCollider2D boxCollider2D;

    // Start is called before the first frame update
    void Start()
    {
        boxCollider2D = this.GetComponent<BoxCollider2D>();        
    }
    
    void OnTriggerEnter2D(Collider2D other) 
    {
        this.ApplyEffect(other.gameObject);
    }

    // Apply the terrain's effect to the game object.
    public abstract void ApplyEffect(GameObject other);
}
