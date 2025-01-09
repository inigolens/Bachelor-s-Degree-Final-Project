using UnityEngine;

public class DualRotation : MonoBehaviour
{
    public float rotationSpeed = 30f; // Velocidad de rotación alrededor del objeto central
    public float selfRotationSpeed = 50f; // Velocidad de rotación sobre sí mismo

    private Transform parentTransform;
    private Vector3 axisOfRotation;

    void Start()
    {
        // Almacena la referencia al padre
        parentTransform = transform.parent;

        // Calcula la posición relativa al padre
        Vector3 relativePosition = transform.position - parentTransform.position;

        // Genera un eje de rotación perpendicular a la posición relativa
        axisOfRotation = Vector3.Cross(relativePosition.normalized, Vector3.up).normalized;

        // Calcula la nueva posición en la circunferencia sin cambiar la rotación del objeto
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
            // Calcula la nueva posición en la circunferencia sin cambiar la rotación del objeto
            Quaternion rotation = Quaternion.AngleAxis(rotationSpeed * Time.deltaTime, axisOfRotation);
            Vector3 newPosition = rotation * (transform.position - parentTransform.position);
            transform.position = parentTransform.position + newPosition;
        }
    }

    void RotateSelf()
    {
        // Rota el objeto sobre sí mismo
        transform.Rotate(Vector3.up, selfRotationSpeed * Time.deltaTime);
    }
}