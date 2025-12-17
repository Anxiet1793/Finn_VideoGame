using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float velocidad = 5f;
    public int vida = 3;

    public float fuerzaSalto = 10f;
    public float fuerzaRebote = 6f;
    public float longitudRaycast = 0.1f;
    public LayerMask capaSuelo;

    private bool enSuelo;
    private bool recibiendoDanio;
    private bool atacando;
    public bool muerto;

    private Rigidbody2D rb;
    public Animator animator;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        if (!muerto)
        {
            if (!atacando)
            {
                Movimiento();

                RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.down, longitudRaycast, capaSuelo);
                enSuelo = hit.collider != null;

                if (enSuelo && Input.GetKeyDown(KeyCode.Space) && !recibiendoDanio)
                {
                    rb.AddForce(new Vector2(0f, fuerzaSalto), ForceMode2D.Impulse);
                }
            }

            if (Input.GetKeyDown(KeyCode.Z) && !atacando && enSuelo)
            {
                Atacando();
            }
        }

        animator.SetBool("ensuelo", enSuelo);
        animator.SetBool("Atacando", atacando);
        animator.SetBool("recibeDanio", recibiendoDanio);
        animator.SetBool("muerto", muerto);
    }

    public void Movimiento()
    {
        float velocidadX = Input.GetAxis("Horizontal") * Time.deltaTime * velocidad;

        animator.SetFloat("movement", velocidadX);

        if (velocidadX < 0) transform.localScale = new Vector3(-1, 1, 1);
        if (velocidadX > 0) transform.localScale = new Vector3(1, 1, 1);

        if (!recibiendoDanio)
        {
            Vector3 pos = transform.position;
            transform.position = new Vector3(velocidadX + pos.x, pos.y, pos.z);
        }
    }

    public void RecibeDanio(Vector2 direccion, int cantDanio)
    {
        if (recibiendoDanio || muerto) return;

        recibiendoDanio = true;
        vida -= cantDanio;

        if (vida <= 0)
        {
            vida = 0;
            Die();
            return;
        }

        Vector2 rebote = new Vector2(transform.position.x - direccion.x, 1).normalized;
        rb.AddForce(rebote * fuerzaRebote, ForceMode2D.Impulse);
    }

    private void Die()
{
    muerto = true;
    recibiendoDanio = false;
    atacando = false;
    Debug.Log("PLAYER MURIO -> llamando GameOver");

    rb.linearVelocity = Vector2.zero;

    if (GameManager.Instance != null)
        GameManager.Instance.TriggerGameOver();
}


    public void DesactivaDanio()
    {
        recibiendoDanio = false;
        rb.linearVelocity = Vector2.zero; // <- corregido
    }

    public void Atacando()
    {
        atacando = true;
    }

    public void DesactivaAtaque()
    {
        atacando = false;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawLine(transform.position, transform.position + Vector3.down * longitudRaycast);
    }
}
