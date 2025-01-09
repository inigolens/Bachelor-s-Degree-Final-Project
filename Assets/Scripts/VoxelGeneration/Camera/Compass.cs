using TMPro; // Importa TextMeshPro
using UnityEngine;
using UnityEngine.UI;

public class Compass : MonoBehaviour
{
    public RawImage CompassImage;
    public Transform Player, truePlayer;
    public TextMeshProUGUI CompassDirectionText; // Cambia Text por TextMeshProUGUI
    
    public void Update()
    {
        //Get a handle on the Image's uvRect
        CompassImage.uvRect = new Rect(Player.localEulerAngles.y / 360, 0, 1, 1);

        // Get a copy of your forward vector
        Vector3 forward = Player.transform.forward;

        // Zero out the y component of your forward vector to only get the direction in the X,Z plane
        forward.y = 0;

        //Clamp our angles to only 5 degree increments
        float headingAngle = Quaternion.LookRotation(forward).eulerAngles.y;
        headingAngle = 5 * (Mathf.RoundToInt(headingAngle / 5.0f));

        //Convert float to int for switch
        int displayangle;
        displayangle = Mathf.RoundToInt(headingAngle);

        //Set the text of Compass Degree Text to the clamped value, but change it to the letter if it is a True direction
        switch (displayangle)
        {
            case 0:
                //Do this
                CompassDirectionText.text = "N";
                break;
            case 360:
                //Do this
                CompassDirectionText.text = "N";
                break;
            case 45:
                //Do this
                CompassDirectionText.text = "NE";
                break;
            case 90:
                //Do this
                CompassDirectionText.text = "E";
                break;
            case 130:
                //Do this
                CompassDirectionText.text = "SE";
                break;
            case 180:
                //Do this
                CompassDirectionText.text = "S";
                break;
            case 225:
                //Do this
                CompassDirectionText.text = "SW";
                break;
            case 270:
                //Do this
                CompassDirectionText.text = "W";
                break;
            default:
                CompassDirectionText.text = headingAngle.ToString();
                break;
        }
    }

    public Transform Target; // Transform del objetivo, en este caso, la nave

    // Función para obtener el ángulo y la dirección cardinal en el compás hacia el que debe apuntar el jugador para encontrar la nave
    public string GetCompassDirection()
    {
        // Obtén la dirección desde el jugador hacia el objetivo
        Vector3 directionToTarget = Target.position - truePlayer.position;

        // Ignora la componente Y para trabajar solo en el plano X,Z
        directionToTarget.y = 0;

        // Obtén el ángulo de la dirección
        float angleToTarget = Quaternion.LookRotation(directionToTarget).eulerAngles.y;

        // Redondea el ángulo a los 5 grados más cercanos
        angleToTarget = 5 * (Mathf.RoundToInt(angleToTarget / 5.0f));

        // Determina la dirección cardinal
        string cardinalDirection;
        int displayAngle = Mathf.RoundToInt(angleToTarget);

        switch (displayAngle)
        {
            case 0:
            case 360:
                cardinalDirection = "N";
                break;
            case 45:
                cardinalDirection = "NE";
                break;
            case 90:
                cardinalDirection = "E";
                break;
            case 135:
                cardinalDirection = "SE";
                break;
            case 180:
                cardinalDirection = "S";
                break;
            case 225:
                cardinalDirection = "SW";
                break;
            case 270:
                cardinalDirection = "W";
                break;
            case 315:
                cardinalDirection = "NW";
                break;
            default:
                cardinalDirection = displayAngle.ToString();
                break;
        }

        // Devuelve el ángulo y la dirección cardinal como un string
        return $"{cardinalDirection}";
    }
}
