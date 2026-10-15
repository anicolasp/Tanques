using UnityEngine;
using UnityEngine.InputSystem;

namespace Tanks.Complete
{
    public class TankMovement : MonoBehaviour
    {
        public int m_PlayerNumber = 1;              // Identifica qué tanque es de cada jugador.
        public float m_Speed = 12f;                 // Rapidez con la que el tanque se mueve alante y atrás.
        public float m_TurnSpeed = 180f;            // Rapidez de giro del tanque en grados por segundo.
        public AudioSource m_MovementAudio;         // Referencia al audio del motor del tanque.
        public AudioClip m_EngineIdling;            // Audio a reproducir cuando el tanque no se mueve.
        public AudioClip m_EngineDriving;           // Audio a reproducir cuando el tanque se está moviendo.
        public float m_PitchRange = 0.2f;           // Variación de afinación del audio del motor.

        [HideInInspector]
        public TankInputUser m_InputUser;           // Componente con las Input Actions del tanque.

        public bool m_IsComputerControlled { get; set; } = false;   // true si lo controla la IA.

        // Índice de control usado por TankManager (1 = WASD, 2 = flechas).
        public int ControlIndex
        {
            get { return m_PlayerNumber; }
            set { m_PlayerNumber = value; }
        }

        // Acceso al Rigidbody para otros scripts (TankAI).
        public Rigidbody Rigidbody => m_Rigidbody;

        // Valores de entrada. La IA puede escribirlos directamente.
        public float MovementInputValue
        {
            get { return m_MovementInputValue; }
            set { m_MovementInputValue = value; }
        }

        public float TurnInputValue
        {
            get { return m_TurnInputValue; }
            set { m_TurnInputValue = value; }
        }

        private Rigidbody m_Rigidbody;
        private float m_MovementInputValue;
        private float m_TurnInputValue;
        private float m_OriginalPitch;

        private void Awake()
        {
            m_Rigidbody = GetComponent<Rigidbody>();

            m_InputUser = GetComponent<TankInputUser>();
            if (m_InputUser == null)
                m_InputUser = gameObject.AddComponent<TankInputUser>();
        }

        private void OnEnable()
        {
            m_Rigidbody.isKinematic = false;
            m_MovementInputValue = 0f;
            m_TurnInputValue = 0f;
        }

        private void OnDisable()
        {
            m_Rigidbody.isKinematic = true;
        }

        private void Start()
        {
            if (m_MovementAudio != null)
                m_OriginalPitch = m_MovementAudio.pitch;
        }

        private void Update()
        {
            // Si lo controla una persona, leemos el teclado; la IA escribe sus propios valores.
            if (!m_IsComputerControlled)
            {
                Keyboard kb = Keyboard.current;
                m_MovementInputValue = 0f;
                m_TurnInputValue = 0f;

                if (kb != null)
                {
                    if (ControlIndex == 1)          // Jugador 1: WASD
                    {
                        m_MovementInputValue = (kb.wKey.isPressed ? 1f : 0f) - (kb.sKey.isPressed ? 1f : 0f);
                        m_TurnInputValue     = (kb.dKey.isPressed ? 1f : 0f) - (kb.aKey.isPressed ? 1f : 0f);
                    }
                    else if (ControlIndex == 2)     // Jugador 2: flechas
                    {
                        m_MovementInputValue = (kb.upArrowKey.isPressed ? 1f : 0f) - (kb.downArrowKey.isPressed ? 1f : 0f);
                        m_TurnInputValue     = (kb.rightArrowKey.isPressed ? 1f : 0f) - (kb.leftArrowKey.isPressed ? 1f : 0f);
                    }
                }
            }

            EngineAudio();
        }

        private void EngineAudio()
        {
            if (m_MovementAudio == null || m_EngineIdling == null || m_EngineDriving == null)
                return;

            if (Mathf.Abs(m_MovementInputValue) < 0.1f && Mathf.Abs(m_TurnInputValue) < 0.1f)
            {
                if (m_MovementAudio.clip == m_EngineDriving)
                {
                    m_MovementAudio.clip = m_EngineIdling;
                    m_MovementAudio.pitch = Random.Range(m_OriginalPitch - m_PitchRange, m_OriginalPitch + m_PitchRange);
                    m_MovementAudio.Play();
                }
            }
            else
            {
                if (m_MovementAudio.clip == m_EngineIdling)
                {
                    m_MovementAudio.clip = m_EngineDriving;
                    m_MovementAudio.pitch = Random.Range(m_OriginalPitch - m_PitchRange, m_OriginalPitch + m_PitchRange);
                    m_MovementAudio.Play();
                }
            }
        }

        private void FixedUpdate()
        {
            Move();
            Turn();
        }

        private void Move()
        {
            Vector3 movement = transform.forward * m_MovementInputValue * m_Speed * Time.deltaTime;
            m_Rigidbody.MovePosition(m_Rigidbody.position + movement);
        }

        private void Turn()
        {
            float turn = m_TurnInputValue * m_TurnSpeed * Time.deltaTime;
            Quaternion turnRotation = Quaternion.Euler(0f, turn, 0f);
            m_Rigidbody.MoveRotation(m_Rigidbody.rotation * turnRotation);
        }
    }
}