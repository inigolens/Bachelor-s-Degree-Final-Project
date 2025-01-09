using UnityEngine;

public class DualRotation : MonoBehaviour
{
    public float rotationSpeed = 30f; // Velocidad de rotaci�n alrededor del objeto central
    public float selfRotationSpeed = 50f; // Velocidad de rotaci�n sobre s� mismo

    private Transform parentTransform;
    private Vector3 axisOfRotation;

    void Start()
    {
        // Almacena la referencia al padre
        parentTransform = transform.parent;

        // Calcula la posici�n relativa al padre
        Vector3 relativePosition = transform.position - parentTransform.position;

        // Genera un eje de rotaci�n perpendicular a la posici�n relativa
        axisOfRotation = Vector3.Cross(relativePosition.normalized, Vector3.up).normalized;

        // Calcula la nueva posici�n en la circunferencia sin cambiar la rotaci�n del objeto
        Vector3 newPosition = Quaternion.AngleAxis(Random.Range(0f, 360f), axisOfRotation) * relativePosition;
        transform.position = parentTransform.position + newPosition;
    }

    void Update()
    {
        RotateAroundParent();
        RotateSelf();
    }

    void RotateAroundParent()
    {
        if (parentTransform != null)
        {
            // Calcula la nueva posici�n en la circunferencia sin cambiar la rotaci�n del objeto
            Quaternion rotation = Quaternion.AngleAxis(rotationSpeed * Time.deltaTime, axisOfRotation);
            Vector3 newPosition = rotation * (transform.position - parentTransform.position);
            transform.position = parentTransform.position + newPosition;
        }
    }

    void RotateSelf()
    {
        // Rota el objeto sobre s� mismo
        transform.Rotate(Vector3.up, selfRotationSpeed * Time.deltaTime);
    }
}