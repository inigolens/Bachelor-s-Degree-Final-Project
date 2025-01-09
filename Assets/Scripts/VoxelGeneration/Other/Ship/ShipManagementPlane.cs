using System.Collections;
using Unity.Burst.CompilerServices;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ShipManagementPlane : MonoBehaviour
{
    public Camera shipCamera;
    public GameObject[] player;
    [SerializeField]
    private ParticleSystem smokeLanding;
    [SerializeField] Image image;

    public bool countNot0, started, touchingGround;

    public float downSpeed = 1f;

    public float interactionDistance = 5f; // Distancia a la que se activa la interacción


    void Update()
    {
        if (!countNot0 && WorldManager.Instance.chunksNeedCreation.Count > 0)
        {
            countNot0 = true;
        }
        if (countNot0 && !started && WorldManager.Instance.chunksNeedCreation.Count == 0)
        {
            started = true;
            StartCoroutine(FadeOut(image, 2f)); // Llama con duración de 2 segundos
            StartCoroutine(MoveDown());
        }
        // Verificar si el jugador está dentro del rango y si se presiona la tecla 'R'
        if (Vector3.Distance(transform.position, player[0].transform.position) < interactionDistance && Input.GetKeyDown(KeyCode.R))
        {
            StartCoroutine(MoveUpAndChangeScene());
        }
    }

    IEnumerator MoveDown()
    {
        RaycastHit hit;
        
        if(Physics.Raycast(transform.position, Vector3.down, out hit, 0.5f))
        {
            touchingGround = true;
            transform.position = hit.point;
            Debug.Log("HEMOS TOACDO EL SUELO Y UNITY ES EL MAYOR MIERDON DE LA HISTORIA");
            
        }
        // Mientras que no se detecte el suelo justo debajo
        while (!touchingGround)
        {
            // Mueve el objeto hacia abajo
            transform.Translate(Vector3.down * downSpeed * Time.deltaTime, Space.World);

            // Espera un frame antes de continuar
            yield return null;
        }

        smokeLanding.Stop();
        yield return new WaitForSeconds(1.5f);
        SwapToMainCamera();
    }

    private void OnCollisionEnter(Collision collision)
    {
        touchingGround = true;
    }

    IEnumerator MoveUpAndChangeScene()
    {
        DeactivatePlayersAndActivateShipCamera();
        smokeLanding.Stop();

        float duration = 2f; // Duración de la subida
        float endTime = Time.time + duration;

        while (Time.time < endTime)
        {
            transform.Translate(Vector3.up * downSpeed * Time.deltaTime, Space.World);
            yield return null;
        }

        CambiarDeEscena();
    }

    void DeactivatePlayersAndActivateShipCamera()
    {
        foreach (GameObject go in player)
        {
            go.SetActive(false);
        }
        shipCamera.gameObject.SetActive(true);
    }

    public void CambiarDeEscena()
    {
        image.gameObject.SetActive(true);
        image.color = Color.black;
        

        SolarSystemManager.instance.DeactiveorActiveChildren(true);
        SceneManager.LoadScene("Scenes/5_SolarSistemGenerator");
    }

    public void SwapToMainCamera()
    {
        shipCamera.gameObject.SetActive(false);
        foreach (GameObject i in player)
        {
            i.gameObject.SetActive(true);
        }

        // Calcula la nueva posición hacia adelante desde la nave
        Vector3 newPosition = transform.position + transform.forward * 5; // 5 bloques de distancia

        // Realiza un raycast hacia abajo desde el punto calculado para encontrar el suelo
        RaycastHit hit;
        if (Physics.Raycast(newPosition + Vector3.up * 100, Vector3.down, out hit, 300))
        {
            // Si el raycast encuentra el suelo, coloca al jugador justo encima de este punto
            player[0].transform.position = hit.point + Vector3.up * 1; // Asegura que el jugador está ligeramente por encima del suelo para evitar cualquier clipping
        }
        else
        {
            // Si no se encuentra suelo, coloca al jugador en la posición calculada sin ajuste de altura
            player[0].transform.position = newPosition;
        }
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
