using System;
using UnityEngine;
using UnityEngine.InputSystem.Users;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;
#endif

namespace Tanks.Complete
{
    [Serializable]
    public class TankManager
    {
        // Gestiona varios ajustes de un tanque y trabaja con el GameManager para
        // controlar si los jugadores tienen control de su tanque en cada fase del juego.

        [HideInInspector] public Color m_PlayerColor;           // Color con el que se tiñe el tanque.
        public Transform m_SpawnPoint;                          // Posición y dirección del tanque al aparecer.
        [HideInInspector] public int m_PlayerNumber;            // Jugador al que pertenece este manager.
        [HideInInspector] public string m_ColoredPlayerText;    // Texto del jugador con su número coloreado.
        [HideInInspector] public GameObject m_Instance;         // Instancia del tanque cuando se crea.
        [HideInInspector] public int m_Wins;                    // Victorias del jugador.
        [HideInInspector] public bool m_ComputerControlled;     // Si el tanque lo controla la IA.

        public int ControlIndex { get; set; } = 1;              // Índice de control: 1 = teclado/mando izquierdo, 2 = teclado derecho, -1 = sin control.

        private TankMovement m_Movement;                        // Script de movimiento, para activar/desactivar el control.
        private TankShooting m_Shooting;                        // Script de disparo, para activar/desactivar el control.
        private GameObject m_CanvasGameObject;                  // UI en espacio de mundo, se oculta al empezar/terminar rondas.

        private TankAI m_AI;                                    // Script de IA para tanques controlados por el ordenador.
        private InputUser m_InputUser;                          // Usuario de entrada asociado al tanque.

        public void Setup (GameManager manager)
        {
            // Obtengo las referencias a los componentes.
            m_Movement = m_Instance.GetComponent<TankMovement> ();
            m_Shooting = m_Instance.GetComponent<TankShooting> ();
            m_AI = m_Instance.GetComponent<TankAI> ();
            m_CanvasGameObject = m_Instance.GetComponentInChildren<Canvas> ().gameObject;

            // Asigno el Input User del tanque al script que controla el input, para que
            // las acciones solo respondan a la entrada de ese usuario.
            var inputUser = m_Instance.GetComponent<TankInputUser>();
            inputUser.SetNewInputUser(m_InputUser);

            // Activo el control por ordenador en movimiento y disparo si corresponde.
            m_Movement.m_IsComputerControlled = m_ComputerControlled;
            m_Shooting.m_IsComputerControlled = m_ComputerControlled;

            // Paso el número de jugador y el índice de control al movimiento.
            m_Movement.m_PlayerNumber = m_PlayerNumber;
            m_Movement.ControlIndex = ControlIndex;

            // Si el tanque es controlado por el ordenador, añado el componente TankAI.
            if(m_ComputerControlled)
            {
                m_AI = m_Instance.AddComponent<TankAI>();
                m_AI.Setup(manager);
            }

            // Creo el texto "PLAYER 1", etc. con el color del tanque.
            m_ColoredPlayerText = "<color=#" + ColorUtility.ToHtmlStringRGB(m_PlayerColor) + ">PLAYER " + m_PlayerNumber + "</color>";

            // Obtengo todos los renderers del tanque.
            MeshRenderer[] renderers = m_Instance.GetComponentsInChildren<MeshRenderer> ();

            for (int i = 0; i < renderers.Length; i++)
            {
                var renderer = renderers[i];
                for (int j = 0; j < renderer.materials.Length; ++j)
                {
                    // Si el material es el del color del tanque, lo cambio al color del jugador.
                    if (renderer.materials[j].name.Contains("TankColor"))
                    {
                        renderer.materials[j].color = m_PlayerColor;
                    }
                }
            }
        }

        // Se usa en las fases en las que el jugador no debe controlar su tanque.
        public void DisableControl ()
        {
            m_Movement.enabled = false;
            m_Shooting.enabled = false;
            if(m_ComputerControlled)
                m_AI.enabled = false;

            m_CanvasGameObject.SetActive (false);
        }

        // Se usa en las fases en las que el jugador sí debe controlar su tanque.
        public void EnableControl ()
        {
            m_Movement.enabled = true;
            m_Shooting.enabled = true;
            if(m_ComputerControlled)
                m_AI.enabled = true;

            m_CanvasGameObject.SetActive (true);
        }

        // Se usa al inicio de cada ronda para dejar el tanque en su estado por defecto.
        public void Reset ()
        {
            m_Instance.transform.position = m_SpawnPoint.position;
            m_Instance.transform.rotation = m_SpawnPoint.rotation;

            m_Instance.SetActive (false);
            m_Instance.SetActive (true);
        }
    }

#if UNITY_EDITOR
    // Solo para el editor: muestra directamente el punto de aparición en el Inspector
    // en lugar de un desplegable dentro del TankManager en el GameManager.
    [CustomPropertyDrawer(typeof(TankManager))]
    public class TankManagerDrawer : PropertyDrawer
    {
        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            var itemSlot = new PropertyField(property.FindPropertyRelative(nameof(TankManager.m_SpawnPoint)));
            return itemSlot;
        }
    }
#endif
}