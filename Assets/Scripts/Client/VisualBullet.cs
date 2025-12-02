using UnityEngine;

public class VisualBullet : MonoBehaviour
{
    [SerializeField]
    BulletType bulletType;

    public BulletType GetBulletType {get {return bulletType;}}

    private Vector2 direction;
    
    [SerializeField]
    private float speed = 10;

    public void Launch(Vector2 dir)
    {
        direction = dir.normalized;
    }

    void Update()
    {
        transform.position += (Vector3)(direction * speed * Time.deltaTime);
    }
}