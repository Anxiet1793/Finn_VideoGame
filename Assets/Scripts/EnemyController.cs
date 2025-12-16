using System.Collections;
using System.Collections.Generic;
using System.Xml.Serialization;
using UnityEngine;
using UnityEngine.SceneManagement;

public class EnemyController : MonoBehaviour
{
    [Header("Target")]
    public Transform player;

    [Header("Movimiento")]
    public float detectionRadius = 5.0f;
    public float speed = 2.0f;

    [Header("Combate")]
    public float fuerzaRebote = 6f;
    public int vida = 3;

    [Header("Puntaje")]
    public int puntosPorMuerte = 10;

    [HideInInspector] public EnemySpawnManager spawnManager;

    private Rigidbody2D rb;
    private Vector2 movement;
    private bool enMovimiento;
    private bool muerto;
    private Animator animator;
    private bool recibiendoDanio;
    private bool playerVivo;

    void Start()
    {
        // Auto-detectar Player por tag
        if (player == null)
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) player = p.transform;
        }

        playerVivo = true;
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        if (player == null)
        {
            // Si por alguna razón no existe aún, reintenta
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) player = p.transform;
            return;
        }

        if (playerVivo && !muerto)
        {
            Movimiento();
        }

        animator.SetBool("enMovimiento", enMovimiento);
        animator.SetBool("muerto", muerto);
    }

    private void Movimiento()
    {
        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        if (distanceToPlayer < detectionRadius)
        {
            Vector2 direction = (player.position - transform.position).normalized;

            if (direction.x < 0) transform.localScale = new Vector3(1, 1, 1);
            else if (direction.x > 0) transform.localScale = new Vector3(-1, 1, 1);

            movement = new Vector2(direction.x, 0);
            enMovimiento = true;
        }
        else
        {
            movement = Vector2.zero;
            enMovimiento = false;
        }

        if (!recibiendoDanio)
        {
            rb.MovePosition(rb.position + movement * speed * Time.deltaTime);
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (muerto) return;

        if (collision.gameObject.CompareTag("Player"))
        {
            Vector2 direccionDanio = new Vector2(transform.position.x, 0);
            PlayerController playerScript = collision.gameObject.GetComponent<PlayerController>();

            playerScript.RecibeDanio(direccionDanio, 1);
            playerVivo = !playerScript.muerto;

            if (!playerVivo) enMovimiento = false;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (muerto) return;

        if (collision.CompareTag("Espada"))
        {
            Vector2 direccionDanio = new Vector2(collision.gameObject.transform.position.x, 0);
            RecibeDanio(direccionDanio, 1);
        }
    }

    public void RecibeDanio(Vector2 direccion, int cantDanio)
    {
        if (!recibiendoDanio && !muerto)
        {
            vida -= cantDanio;
            recibiendoDanio = true;

            if (vida <= 0)
            {
                muerto = true;
                enMovimiento = false;

                // Puntaje
                if (ScoreManager.Instance != null)
                    ScoreManager.Instance.AddScore(puntosPorMuerte);

                // Respawn (en punto random)
                if (spawnManager != null)
                    spawnManager.EnemyDied();
            }
            else
            {
                Vector2 rebote = new Vector2(transform.position.x - direccion.x, 0.2f).normalized;
                rb.AddForce(rebote * fuerzaRebote, ForceMode2D.Impulse);
            }
        }
    }

    // Llamado desde Animation Event cuando termina la animación de muerte
    public void EliminarEnemigo()
    {
        Destroy(gameObject);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
    }
}