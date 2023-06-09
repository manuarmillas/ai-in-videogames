using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThiefScript : MonoBehaviour
{
    public float moveSpeed = 2f; // The speed at which the object moves
    public float rotationSpeed = 100f; // The speed at which the object rotates
    public float movementRange = 5f; // The range within which the object moves
    public float maxY = 5f; // The maximum height of the object above the ground

    private Vector3 movementDirection; // The direction that the object is currently moving in
    private float movementDistance; // The remaining distance for the object to move in its current direction
    private RaycastHit hitInfo; // The raycast hit information for the ground below the object

    // Start is called before the first frame update
    void Start()
    {
        // Set the initial movement direction to a random direction within the movement range
        movementDirection = Random.insideUnitSphere.normalized;
        movementDirection.y = 0f;
        movementDistance = Random.Range(movementRange * 0.5f, movementRange);
    }

    // Update is called once per frame
    void Update()
    {
        // Move the object along its current movement direction
        transform.position += movementDirection * moveSpeed * Time.deltaTime;

        // Decrease the remaining movement distance
        movementDistance -= moveSpeed * Time.deltaTime;

        // If the object has reached the end of its current movement path, generate a new random direction and distance
        if (movementDistance <= 0f)
        {
            movementDirection = Random.insideUnitSphere.normalized;
            movementDirection.y = 0f;
            movementDistance = Random.Range(movementRange * 0.5f, movementRange);
        }

        // Rotate the object towards its current movement direction
        transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.LookRotation(movementDirection), rotationSpeed * Time.deltaTime);

        // Raycast down to check the ground height below the object
        if (Physics.Raycast(transform.position, Vector3.down, out hitInfo))
        {
            // Restrict the object's height above the ground to the maximum height
            if (transform.position.y > hitInfo.point.y + maxY)
            {
                transform.position = new Vector3(transform.position.x, hitInfo.point.y + maxY, transform.position.z);
            }
        }

        // Restrict the object's movement within the 65x65 surface
        float halfSurface = 32.5f;
        float xPos = Mathf.Clamp(transform.position.x, -halfSurface, halfSurface);
        float zPos = Mathf.Clamp(transform.position.z, -halfSurface, halfSurface);
        transform.position = new Vector3(xPos, transform.position.y, zPos);
    }
}
