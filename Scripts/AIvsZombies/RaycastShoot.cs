using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RaycastShoot : MonoBehaviour
{
    // Prefab de la bala
    public GameObject bulletPrefab;

    // Velocidad de la bala
    public float bulletSpeed = 30f;

    // Efecto de sonido de disparo
    // public AudioClip shootSound;

    // Efecto de partículas de disparo
    // public ParticleSystem muzzleFlash;

    // LineRenderer para representar el rayo del disparo
    private LineRenderer lineRenderer;

    // Punto de origen para el rayo del disparo
    private Transform muzzle;

    private void Start()
    {
        // Obtener la referencia al LineRenderer
        lineRenderer = GetComponent<LineRenderer>();

        // Obtener la referencia al punto de origen del rayo
        muzzle = transform.Find("Gun");
    }

    // Método para realizar el disparo
    public void Shoot()
    {
        // Instanciar la bala
        if (bulletPrefab == null)
        {
            Debug.Log("bullet null");
            return;
        }
        GameObject bullet = Instantiate(bulletPrefab, muzzle.position, muzzle.rotation);

        // Aplicar velocidad a la bala
        bullet.GetComponent<Rigidbody>().velocity = muzzle.forward * bulletSpeed;

        // Representar el rayo del disparo con el LineRenderer
        lineRenderer.enabled = true;
        lineRenderer.SetPosition(0, muzzle.position);
        lineRenderer.SetPosition(1, muzzle.position + muzzle.forward * 100f);

        // Desactivar el LineRenderer después de un corto tiempo
        Invoke("DisableLineRenderer", 0.05f);
    }

    // Método para desactivar el LineRenderer
    private void DisableLineRenderer()
    {
        lineRenderer.enabled = false;
    }
}
