using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Timeline;
using UnityEngine.UI;

public class ShipController : MonoBehaviour
{
    public float forwardSpeed = 25f, strafeSpeed = 7.5f, hoverSpeed = 5f;
    private float activeforwardSpeed, activestrafeSpeed, activehoverSpeed;
    private float forwardAcceleration = 5f, strafeAcceleration = 2f, hoverAcceleration = 2f;

    public float lookRateSpeed = 90f;
    private Vector2 lookInput, screenCenter, mouseDistance;

    private float rollInput;
    public float rollSpeed = 90f, rollAcceleration = 3.5f;

    [Range(0, 1)]
    public float deadZonePercentege = 0.05f;

    bool allowMovement = true;

    public float accelerationTime = 2.0f; // Duración de la aceleración
    public float maxSpeed = 20f; // Velocidad máxima alcanzada al final de la aceleración
    public float minScale = 0.1f; // Escala mínima al final del efecto

    [SerializeField] GameObject shipModel;

    [SerializeField] Image img;

    public float turboMultiplier = 2f; // Multiplicador de velocidad en modo turbo
    public float turboDuration = 5f; // Duración del modo turbo
    public float turboCooldown = 1f; // Tiempo de recarga para el modo turbo
    private bool isTurboActive = false;
    private bool isTurboOnCooldown = false;

    // Start is called before the first frame update
    void Start()
    {
        screenCenter.x = Screen.width / 2;
        screenCenter.y = Screen.height / 2;

        Cursor.lockState = CursorLockMode.Confined;

        transform.position = SolarSystemManager.instance.shipTransform.position;
        transform.rotation = SolarSystemManager.instance.shipTransform.rotation;
        StartCoroutine(FadeOut(img, 2));
    }

    // Update is called once per frame
    void Update()
    {
        if (!allowMovement) return;

        lookInput.x = Input.mousePosition.x;
        lookInput.y = Input.mousePosition.y;

        mouseDistance.x = (lookInput.x - screenCenter.x) / screenCenter.y;
        mouseDistance.y = (lookInput.y - screenCenter.y) / screenCenter.y;

        mouseDistance = Vector2.ClampMagnitude(mouseDistance, 1f);

        rollInput = Mathf.Lerp(rollInput, Input.GetAxisRaw("Roll"), rollAcceleration * Time.deltaTime);
        if (mouseDistance.magnitude > deadZonePercentege)
        {
            transform.Rotate(-mouseDistance.y * lookRateSpeed * Time.deltaTime, mouseDistance.x * lookRateSpeed * Time.deltaTime, rollInput * rollSpeed * Time.deltaTime, Space.Self);
        }
        else
        {
            transform.Rotate(0f, mouseDistance.x * lookRateSpeed * Time.deltaTime, rollInput * rollSpeed * Time.deltaTime, Space.Self);
        }

        if (Input.GetKeyDown(KeyCode.LeftShift) && !isTurboOnCooldown)
        {
            StartCoroutine(TurboMode());
        }

        float currentForwardSpeed = forwardSpeed;
        if (isTurboActive)
        {
            currentForwardSpeed *= turboMultiplier;
        }

        activeforwardSpeed = Mathf.Lerp(activeforwardSpeed, Input.GetAxisRaw("Vertical") * currentForwardSpeed, forwardAcceleration * Time.deltaTime);
        activestrafeSpeed = Mathf.Lerp(activestrafeSpeed, Input.GetAxisRaw("Horizontal") * strafeSpeed, strafeAcceleration * Time.deltaTime);
        activehoverSpeed = Mathf.Lerp(activehoverSpeed, Input.GetAxisRaw("Hover") * hoverSpeed, hoverAcceleration * Time.deltaTime);

        transform.position += transform.forward * activeforwardSpeed * Time.deltaTime;
        transform.position += transform.right * activestrafeSpeed * Time.deltaTime;
        transform.position += transform.up * activehoverSpeed * Time.deltaTime;
    }

    private IEnumerator TurboMode()
    {
        isTurboActive = true;
        yield return new WaitForSeconds(turboDuration);
        isTurboActive = false;
        isTurboOnCooldown = true;
        yield return new WaitForSeconds(turboCooldown);
        isTurboOnCooldown = false;
    }

    private void OnCollisionEnter(Collision collision)
    {
        print("Colisiones");
        if (collision.gameObject.layer == 7)
        {
            String input = collision.transform.parent.transform.parent.name;
            Match match = Regex.Match(input, @"\d+"); // Busca una o más cifras numéricas

            if (match.Success)
            {
                int number = int.Parse(match.Value); // Convierte el resultado encontrado a entero
                print("El número de planeta es: " + number);
                SolarSystemManager.instance.lastplanetttouched = number;
                SolarSystemManager.instance.shipTransform.position = transform.position;
                allowMovement = false;
                StartCoroutine(EffectCoroutine(shipModel));
            }
        }
    }

    private IEnumerator EffectCoroutine(GameObject targetObject)
    {
        float startTime = Time.time;
        Vector3 initialScale = targetObject.transform.localScale; // Guardar la escala inicial del objeto
        Vector3 targetScale = initialScale * minScale; // Calcular la escala objetivo
        Vector3 initialPosition = targetObject.transform.position; // Posición inicial para el cálculo de movimiento

        while (Time.time - startTime < accelerationTime)
        {
            float t = (Time.time - startTime) / accelerationTime; // Calcular el porcentaje de tiempo transcurrido

            // Mover el objeto hacia adelante
            targetObject.transform.position += targetObject.transform.right * Mathf.Lerp(0, maxSpeed, t) * Time.deltaTime;

            // Escalar el objeto gradualmente
            targetObject.transform.localScale = Vector3.Lerp(initialScale, targetScale, t);

            yield return null;
        }

        // Asegurarse de que el objeto alcance la velocidad y escala finales
        targetObject.transform.position += targetObject.transform.forward * maxSpeed * Time.deltaTime;
        targetObject.transform.localScale = targetScale;

        SolarSystemManager.instance.MoveShipAwayFromPlanet(150);
        SceneManager.LoadScene("Scenes/6_VoxelGeneration");
    }

    public IEnumerator FadeOut(Image img, float duration)
    {
        float counter = 0;
        Color initialColor = img.color; // Almacena el color inicial
        while (counter < duration)
        {
            counter += Time.deltaTime;
            float alpha = Mathf.Lerp(1, 0, counter / duration);
            img.color = new Color(initialColor.r, initialColor.g, initialColor.b, alpha);
            yield return null;
        }
        img.gameObject.SetActive(false);
    }
}
