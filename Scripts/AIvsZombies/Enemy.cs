using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem.Processors;

public class Enemy : MonoBehaviour
{
    public int health = 100;
    public int actualHealth;
    private Vector3 initialPos;

    public Transform target;
    public float speed = 5f;

    public void GetShot(int dmg, ShootingAgent shooter)
    {
        //Debug.Log("GetShot");
        actualHealth -= dmg;

        if (actualHealth <= 0)
        {
            Dead(shooter);
        }
    } 

    public void Dead(ShootingAgent shooter)
    {
        shooter.RegisterKill();
        gameObject.SetActive(false);
        Respawn();
        //RespawnInitialPos();
    }

    public void Respawn()
    {
        //transform.localPosition = new Vector3(Random.Range(-70, 70), 3.2f, Random.Range(-70,70));
        float posX = Random.Range(-70f, 70f);
        if (posX <= 5f && posX >= -5f)
        {
            posX += 11f;
        }
        float posZ = Random.Range(-70f, 70f);
        if (posZ<= 5f && posZ >= -5f) { posZ += 11f; }
        transform.localPosition = new Vector3(posX, 3.2f, posZ);
        actualHealth = health;
        gameObject.SetActive(true);
        Debug.Log("Respawn");
    }

    public void RespawnInitialPos()
    {
        transform.localPosition = initialPos;
        actualHealth= health;
        gameObject.SetActive(true);
    }

    // Start is called before the first frame update
    void Start()
    {
        actualHealth = health;
        initialPos = transform.localPosition;
        //Debug.Log(initialPos);
    }

    private void FixedUpdate()
    {
        // Encuentra la dirección hacia el objetivo
        Vector3 direction = (target.position - transform.position).normalized;

        // Evita los objetos con tag "Wall"
        RaycastHit hit;
        if (Physics.Raycast(transform.position, direction, out hit, 20.0f) && hit.collider.tag == "Wall")
        {
            direction = Vector3.Reflect(direction, hit.normal);
        }

        // Mueve el objeto hacia el objetivo a la velocidad especificada
        transform.Translate(direction * speed * Time.deltaTime);

        if (transform.localPosition.x > 75 || transform.localPosition.x < -75 || transform.localPosition.z > 75 || transform.localPosition.z < -75) Respawn();
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("agent"))
        {
            Respawn();
        }
    }
}
