using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Users;

using UnityEngine;

public class TankMovement : MonoBehaviour
{
    public int m_PlayerNumber = 1;              // Identifica qué tanque es de cada jugador.
    public float m_Speed = 12f;                 // Rapidez con la que el tanque se mueve alante y atrás.
    public float m_TurnSpeed = 180f;            // Rapidez de giro del tanque en grados por segundo.
    public AudioSource m_MovementAudio;         // Referencia al audio del motor del tanque.
    public AudioClip m_EngineIdling;            // Audio a reproducir cuando el tanque no se mueve.
    public AudioClip m_EngineDriving;           // Audio a reproducir cuando el tanque se está moviendo.
    public float m_PitchRange = 0.2f;           // Variación de afinación del audio del motor.

    public bool m_IsComputerControlled = false; // NUEVO: true si lo controla la IA (TankAI).

    private string m_MovementAxisName;
    private string m_TurnAxisName;
    private Rigidbody m_Rigidbody;
    private float m_MovementInputValue;
    private float m_TurnInputValue;
    private float m_OriginalPitch;

    public void AddExplosionForce (float force, Vector3 position, float radius, float upwardsModifier = 0f, ForceMode mode = ForceMode.Force)
    {
        m_Rigidbody.AddExplosionForce (force, position, radius, upwardsModifier, mode);
    }

    // NUEVO: acceso al Rigidbody para otros scripts (TankAI).
    public Rigidbody Rigidbody
    {
        get { return m_Rigidbody; }
    }

    // NUEVO: índice de control usado por TankManager.
    public int ControlIndex
    {
        get { return m_PlayerNumber; }
        set { m_PlayerNumber = value; }
    }

    private void Awake ()
    {
        m_Rigidbody = GetComponent<Rigidbody>();
    }

    private void OnEnable ()
    {
        m_Rigidbody.isKinematic = false;
        m_MovementInputValue = 0f;
        m_TurnInputValue = 0f;
    }

    private void OnDisable ()
    {
        m_Rigidbody.isKinematic = true;
    }

    private void Start ()
    {
        m_MovementAxisName = "Vertical" + m_PlayerNumber;
        m_TurnAxisName = "Horizontal" + m_PlayerNumber;
        m_OriginalPitch = m_MovementAudio.pitch;
    }

    private void Update ()
    {
        // NUEVO: si lo controla la IA, no leemos el teclado.
        if (!m_IsComputerControlled)
        {
            m_MovementInputValue = Input.GetAxis (m_MovementAxisName);
            m_TurnInputValue = Input.GetAxis (m_TurnAxisName);
        }

        EngineAudio ();
    }

    private void EngineAudio ()
    {
        if (Mathf.Abs (m_MovementInputValue) < 0.1f && Mathf.Abs (m_TurnInputValue) < 0.1f)
        {
            if (m_MovementAudio.clip == m_EngineDriving)
            {
                m_MovementAudio.clip = m_EngineIdling;
                m_MovementAudio.pitch = Random.Range (m_OriginalPitch - m_PitchRange, m_OriginalPitch + m_PitchRange);
                m_MovementAudio.Play ();
            }
        }
        else
        {
            if (m_MovementAudio.clip == m_EngineIdling)
            {
                m_MovementAudio.clip = m_EngineDriving;
                m_MovementAudio.pitch = Random.Range (m_OriginalPitch - m_PitchRange, m_OriginalPitch + m_PitchRange);
                m_MovementAudio.Play ();
            }
        }
    }

    private void FixedUpdate ()
    {
        Move ();
        Turn ();
    }

    private void Move ()
    {
        Vector3 movement = transform.forward * m_MovementInputValue * m_Speed * Time.deltaTime;
        m_Rigidbody.MovePosition (m_Rigidbody.position + movement);
    }

    private void Turn ()
    {
        float turn = m_TurnInputValue * m_TurnSpeed * Time.deltaTime;
        Quaternion turnRotation = Quaternion.Euler (0f, turn, 0f);
        m_Rigidbody.MoveRotation (m_Rigidbody.rotation * turnRotation);
    }
}