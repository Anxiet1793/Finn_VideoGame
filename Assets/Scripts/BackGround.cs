using UnityEngine;

public class BackGround : MonoBehaviour
{
    void Start()
    {
        var sr = GetComponent<SpriteRenderer>();
        var cam = Camera.main;

        // tamaño del sprite en unidades mundo
        float spriteWidth = sr.bounds.size.x;
        float spriteHeight = sr.bounds.size.y;

        // tamaño de la vista de la cámara en unidades mundo
        float worldHeight = cam.orthographicSize * 2f;
        float worldWidth = worldHeight * cam.aspect;

        // factor de escala necesario para cubrir ambos ejes
        float scaleX = worldWidth / spriteWidth;
        float scaleY = worldHeight / spriteHeight;
        float scale = Mathf.Max(scaleX, scaleY);

        transform.localScale = new Vector3(scale, scale, 1f);

        // aseguramos que esté detrás
        if (transform.position.z <= cam.transform.position.z)
            transform.position = new Vector3(transform.position.x, transform.position.y, cam.transform.position.z + 10f);
    }
}
