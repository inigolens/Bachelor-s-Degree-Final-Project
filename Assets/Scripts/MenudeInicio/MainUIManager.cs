using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Rendering;
using UnityEngine.UI;

public class MainUIManager : MonoBehaviour
{
    [SerializeField] private GameObject[] buttonList;
    [SerializeField] private TMP_InputField intInputField;
    [SerializeField] private Button inputButton, lastSeedButton, randomButton;
    [SerializeField] private RectTransform container;
    [SerializeField] private GameObject principalUI, optionsUI;
    [SerializeField] private AudioMixer mixer;
    [SerializeField] Slider musicSlider, soundsSlider;


    void Start()
    {
        List<int> ints = ReadWrite.Instance.getLastSeedList();
        bool empty = false;
        if (ints == null || ints.Count == 0) empty = true;
        int activeButtonCount = 0;
        // Configura cada bot�n con los n�meros y aseg�rate de que los eventos est�n asignados correctamente
        for (int i = 0; i < buttonList.Length; i++)
        {
            if (empty || i >= ints.Count) // Si el �ndice es mayor o igual que el n�mero de elementos en la lista, desactiva el bot�n
            {
                buttonList[i].SetActive(false);
            }
            else
            {
                // Es seguro acceder a ints[i] porque i es menor que ints.Count
                Button btn = buttonList[i].GetComponentInChildren<Button>(); // Asume que el bot�n es el componente hijo de los GameObjects listados en buttonList
                TMP_Text textComponent = btn.GetComponentInChildren<TextMeshProUGUI>(); // Obtiene el componente de texto del bot�n
                textComponent.text = ints[i].ToString(); // Establece el texto del bot�n
                btn.onClick.AddListener(() => ButtonClicked(textComponent.text)); // A�ade un listener al evento onClick del bot�n
                activeButtonCount++;
            }
        }
        intInputField.onValueChanged.AddListener(ValidateInput);
        inputButton.onClick.AddListener(() => inputButtonSystem());
        randomButton.onClick.AddListener(() => ReadWrite.Instance.WriteRandomIntToFile());
        lastSeedButton.onClick.AddListener(() => ReadWrite.Instance.LoadSceneIfIntExists());


        
        float height = container.sizeDelta.y; // Toma la altura actual
        height = height / buttonList.Length * activeButtonCount;
        container.sizeDelta = new Vector2(container.sizeDelta.x, height); // Ajusta la nueva altura

        float currentVolume;
        mixer.GetFloat("Musica", out currentVolume);
        musicSlider.value = Mathf.Pow(10, currentVolume / 20);
        mixer.GetFloat("Sonido", out currentVolume);
        soundsSlider.value = Mathf.Pow(10, currentVolume / 20);
        optionsUI.SetActive(false);

    }

    void Update()
    {
        if (UnityEngine.Input.GetKeyDown(KeyCode.Escape)) // Comprueba si se presiona la tecla Escape
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false; // Detiene la ejecuci�n en el editor de Unity
#else
            Application.Quit(); // Cierra la aplicaci�n cuando est� en una versi�n compilada
#endif
        }
    }

    private void ButtonClicked(string buttonText)
    {

        ReadWrite.Instance.UpdateLastSeedsList(int.Parse(buttonText));
        ReadWrite.Instance.WriteIntToFile(int.Parse(buttonText));
        Debug.Log("Button clicked with number: " + buttonText); // Imprime el n�mero del bot�n que fue clickeado
    }

    private void inputButtonSystem()
    {
        string numericInput = string.Join("", System.Text.RegularExpressions.Regex.Split(intInputField.text, "[^\\d]"));
        if (long.TryParse(numericInput, out long longValue))  // Usamos long para evitar overflow en la conversi�n
        {
            if (longValue < int.MaxValue)
            {
                ButtonClicked(numericInput);
            }
        }
        else
        {
            print("no no no  :  " + numericInput);
        }
    }

    private void ValidateInput(string input)
    {
        string numericInput = string.Join("", System.Text.RegularExpressions.Regex.Split(input, "[^\\d]"));
        intInputField.text = numericInput;
        if (long.TryParse(numericInput, out long longValue))  // Usamos long para evitar overflow en la conversi�n
        {
            if (longValue > int.MaxValue)
            {
                intInputField.text = int.MaxValue.ToString();  // Establece al m�ximo si se excede
            }
            else
            {
                intInputField.text = longValue.ToString();  // De lo contrario, acepta el valor
            }
        }
        else if (!string.IsNullOrEmpty(numericInput))
        {
            intInputField.text = int.MaxValue.ToString();  // Si hay un problema con la conversi�n, se establece al m�ximo
        }
        else
        {
            intInputField.text = "";  // Si la cadena es vac�a o solo conten�a caracteres no v�lidos, se limpia
        }

    }
    public void activePrincipleUI(bool a)
    {
        principalUI.SetActive(a);
        optionsUI.SetActive(!a);
    }

    public void cambiarVolumenMusica()
    {
        if (musicSlider.value == 0)
        {
            musicSlider.value = 0;
            mixer.SetFloat("Musica", -80);
            return;
        }
        float mixerValue = Mathf.Log10(musicSlider.value) * 20;
        mixer.SetFloat("Musica", mixerValue);
    }

    public void cambiarVolumenSonido()
    {
        if (soundsSlider.value == 0)
        {
            soundsSlider.value = 0;
            mixer.SetFloat("Sonido", -80);
            return;
        }
        float mixerValue = Mathf.Log10(soundsSlider.value) * 20;
        mixer.SetFloat("Sonido", mixerValue);
    }
}
