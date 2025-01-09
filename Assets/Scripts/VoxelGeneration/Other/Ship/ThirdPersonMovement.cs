using Newtonsoft.Json;
using System.Collections.Generic;
using UnityEngine;

public class ThirdPersonMovement : MonoBehaviour
{
    public CharacterController controller;
    public Transform cam, camFirstPerson; // Cámaras de tercera y primera persona

    public float speed = 6f;
    public float turnSmoothTime = 0.1f;
    float turnSmoothVelocity;

    public float mouseSensitivity = 100f; // Sensibilidad del ratón
    float xRotation = 0f; // Rotación vertical acumulada

    public float jumpHeight = 3.0f;
    public float gravity = -9.81f;

    public float jetpackTime = 5.0f;
    private float jetpackTimer;
    private bool isUsingJetpack = false;

    [SerializeField] ParticleSystem jetpackParticle;
    [SerializeField] Animator animator;

    private Vector3 velocity;
    private bool isGrounded;

    private bool isFirstPerson = false; // Estado de control de cámara

    [SerializeField] PlaneUIManager planeUIManager;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked; // Bloquear el cursor al centro de la pantalla
        Cursor.visible = false; // Ocultar el cursor
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F))
        {
            ToggleCamera(); // Cambiar entre cámaras
        }

        if (isFirstPerson)
        {
            FirstPersonMovement();
            if (Input.GetMouseButtonDown(1)) // Right mouse button
            {
                FireRaycast();
            }
        }
        else
        {
            ThirdPersonMovements();
        }
    }

    void ToggleCamera()
    {
        isFirstPerson = !isFirstPerson;
        cam.gameObject.SetActive(!isFirstPerson);
        camFirstPerson.gameObject.SetActive(isFirstPerson);
        planeUIManager.changeFirstPerson(isFirstPerson);
    }

    void ThirdPersonMovements()
    {
        isGrounded = controller.isGrounded;
        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
            jetpackTimer = jetpackTime;
            if (isUsingJetpack)
            {
                isUsingJetpack = false;
                jetpackParticle.Stop();
            }
        }

        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");
        Vector3 direction = new Vector3(horizontal, 0f, vertical).normalized;

        if (direction.magnitude >= 0.1f)
        {
            float targetAngle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg + cam.eulerAngles.y;
            float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref turnSmoothVelocity, turnSmoothTime);
            transform.rotation = Quaternion.Euler(0f, angle, 0f);
            Vector3 moveDir = Quaternion.Euler(0f, targetAngle, 0f) * Vector3.forward;
            controller.Move(moveDir.normalized * speed * Time.deltaTime);
            animator.SetFloat("speed", 1f);
        }
        else
        {
            animator.SetFloat("speed", 0f);
        }

        ApplyMovement();
    }

    void FirstPersonMovement()
    {
        isGrounded = controller.isGrounded;
        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
        }
        

        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        xRotation -= mouseY; // Actualizar rotación vertical basada en entrada vertical del ratón
        xRotation = Mathf.Clamp(xRotation, -90f, 90f); // Limitar la rotación para evitar el volcamiento

        camFirstPerson.localRotation = Quaternion.Euler(xRotation, 0f, 0f); // Aplicar rotación vertical
        transform.Rotate(Vector3.up * mouseX); // Aplicar rotación horizontal al transform del jugador

        // Movimiento basado en teclado
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");
        Vector3 direction = new Vector3(horizontal, 0f, vertical).normalized;

        if (direction.magnitude >= 0.1f)
        {
            Vector3 moveDir = transform.right * horizontal + transform.forward * vertical;
            controller.Move(moveDir.normalized * speed * Time.deltaTime);
        }

        ApplyMovement();
    }

    void ApplyMovement()
    {
        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            animator.SetTrigger("Jump");
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }

        if (Input.GetKey(KeyCode.Space) && !isGrounded && jetpackTimer > 0)
        {
            if (!isUsingJetpack)
            {
                isUsingJetpack = true;
                jetpackParticle.Play();
            }
            jetpackTimer -= Time.deltaTime;
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }
        else if (isUsingJetpack && (!Input.GetKey(KeyCode.Space) || jetpackTimer <= 0))
        {
            isUsingJetpack = false;
            jetpackParticle.Stop();
        }

        if (!isUsingJetpack)
        {
            velocity.y += gravity * Time.deltaTime;
        }

        if (isGrounded)
        {
            animator.SetBool("IsFlying", false);
        }
        else
        {
            animator.SetBool("IsFlying", true);
        }

        controller.Move(velocity * Time.deltaTime);

    }

    void FireRaycast()
    {
        RaycastHit hit;
        Ray ray = new Ray(camFirstPerson.transform.position, camFirstPerson.transform.forward);
        int layerMask = 1 << 0;  // Asumiendo que los chunks están en el layer "Default"
        float maxDistance = 4f;

        if (Physics.Raycast(ray, out hit, maxDistance, layerMask))
        {
            Chunk hitChunk = hit.collider.gameObject.GetComponent<Chunk>();
            if (hitChunk != null && hitChunk.voxelArray != null)
            {
                Vector3 hitPointInChunk = hit.point - hitChunk.chunkPosition;
                Vector3 voxelCoord = new Vector3(
                    Mathf.FloorToInt(hitPointInChunk.x),
                    Mathf.FloorToInt(hitPointInChunk.y),
                    Mathf.FloorToInt(hitPointInChunk.z)
                );

                // Verificar que las coordenadas del voxel estén dentro del rango permitido
                if (voxelCoord.x >= 0 && voxelCoord.x < WorldManager.WorldSettings.chunkSize &&
                    voxelCoord.y >= 0 && voxelCoord.y < WorldManager.WorldSettings.maxHeight &&
                    voxelCoord.z >= 0 && voxelCoord.z < WorldManager.WorldSettings.chunkSize)
                {
                    Voxel hitVoxel = hitChunk.voxelArray[voxelCoord];
                    if (hitVoxel.isSolid)
                    {
                        Debug.Log($"Hit solid voxel ID: {hitVoxel.ID} at {voxelCoord} the total list: {SolarSystemManager.instance.detectedVoxelIDs[SolarSystemManager.instance.lastplanetttouched].Count}");

                        if (!SolarSystemManager.instance.detectedVoxelIDs[SolarSystemManager.instance.lastplanetttouched].Contains(hitVoxel.ID))
                        {
                            SolarSystemManager.instance.detectedVoxelIDs[SolarSystemManager.instance.lastplanetttouched].Add(hitVoxel.ID);
                            planeUIManager.UpdatePlanetUI();
                            planeUIManager.startFoundMaterialAnimation();
                        }
                    }
                    else
                    {
                        // Busca en los alrededores inmediatos
                        Vector3[] offsets = new Vector3[]
                        {
                        Vector3.up, Vector3.down,
                        Vector3.left, Vector3.right,
                        Vector3.forward, Vector3.back
                        };

                        foreach (Vector3 offset in offsets)
                        {
                            Vector3 neighborCoord = voxelCoord + offset;
                            if (neighborCoord.x >= 0 && neighborCoord.x < WorldManager.WorldSettings.chunkSize &&
                                neighborCoord.y >= 0 && neighborCoord.y < WorldManager.WorldSettings.maxHeight &&
                                neighborCoord.z >= 0 && neighborCoord.z < WorldManager.WorldSettings.chunkSize)
                            {
                                Voxel neighborVoxel = hitChunk.voxelArray[neighborCoord];
                                if (neighborVoxel.isSolid && neighborVoxel.ID != 1)
                                {
                                    if (!SolarSystemManager.instance.detectedVoxelIDs[SolarSystemManager.instance.lastplanetttouched].Contains(neighborVoxel.ID))
                                    {
                                        SolarSystemManager.instance.detectedVoxelIDs[SolarSystemManager.instance.lastplanetttouched].Add(neighborVoxel.ID);
                                        planeUIManager.UpdatePlanetUI();
                                        planeUIManager.startFoundMaterialAnimation();
                                    }
                                    break;
                                }
                            }
                        }
                    }
                }
                else
                {
                    Debug.LogWarning("Voxel coordinates out of bounds.");
                }
            }
            else
            {
                Debug.LogWarning("Chunk or voxel array is null.");
            }
        }
    }

}
