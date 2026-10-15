using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.UI;

namespace Tanks.Complete
{
    public class TankShooting : MonoBehaviour
    {
        public Rigidbody m_Shell;                   // Prefab of the shell.
        public Transform m_FireTransform;           // A child of the tank where the shells are spawned.
        public Slider m_AimSlider;                  // A child of the tank that displays the current launch force.
        public AudioSource m_ShootingAudio;         // Reference to the audio source used to play the shooting audio. NB: different to the movement audio source.
        public AudioClip m_ChargingClip;            // Audio that plays when each shot is charging up.
        public AudioClip m_FireClip;                // Audio that plays when each shot is fired.
        [Tooltip("The speed in unit/second the shell have when fired at minimum charge")]
        public float m_MinLaunchForce = 5f;
        [Tooltip("The speed in unit/second the shell have when fired at max charge")]
        public float m_MaxLaunchForce = 20f;
        [Tooltip("The maximum time spent charging. When charging reach that time, the shell is fired at MaxLaunchForce")]
        public float m_MaxChargeTime = 0.75f;
        [Tooltip("The time that must pass before being able to shoot again after a shot")]
        public float m_ShotCooldown = 1.0f;
        [Header("Shell Properties")]
        [Tooltip("The amount of health removed to a tank if they are exactly on the landing spot of a shell")]
        public float m_MaxDamage = 100f;
        [Tooltip("The force of the explosion at the shell position. Keep it 50 and below")]
        public float m_ExplosionForce = 50f;
        [Tooltip("The radius of the explosion in Unity unit.")]
        public float m_ExplosionRadius = 5f;

        [HideInInspector]
        public TankInputUser m_InputUser;

        public float CurrentChargeRatio =>
            (m_CurrentLaunchForce - m_MinLaunchForce) / (m_MaxLaunchForce - m_MinLaunchForce);
        public bool IsCharging => m_IsCharging;

        public bool m_IsComputerControlled { get; set; } = false;

        private float m_CurrentLaunchForce;
        private float m_ChargeSpeed;
        private bool m_Fired;
        private bool m_HasSpecialShell;
        private float m_SpecialShellMultiplier;
        private bool m_IsCharging = false;
        private float m_BaseMinLaunchForce;
        private float m_ShotCooldownTimer;
        private TankMovement m_Movement;

        // Jugador 1: Espacio. Jugador 2: Enter.
        private KeyControl FireKey
        {
            get
            {
                Keyboard kb = Keyboard.current;
                if (kb == null) return null;
                if (m_Movement == null) m_Movement = GetComponent<TankMovement>();
                int index = m_Movement != null ? m_Movement.ControlIndex : 1;
                return index == 2 ? kb.enterKey : kb.spaceKey;
            }
        }

        private bool FirePressedThisFrame  => FireKey != null && FireKey.wasPressedThisFrame;
        private bool FireHeld              => FireKey != null && FireKey.isPressed;
        private bool FireReleasedThisFrame => FireKey != null && FireKey.wasReleasedThisFrame;

        private void OnEnable()
        {
            m_CurrentLaunchForce = m_MinLaunchForce;
            m_BaseMinLaunchForce = m_MinLaunchForce;
            m_AimSlider.value = m_BaseMinLaunchForce;
            m_HasSpecialShell = false;
            m_SpecialShellMultiplier = 1.0f;

            m_AimSlider.minValue = m_MinLaunchForce;
            m_AimSlider.maxValue = m_MaxLaunchForce;
        }

        private void Awake()
        {
            m_InputUser = GetComponent<TankInputUser>();
            if (m_InputUser == null)
                m_InputUser = gameObject.AddComponent<TankInputUser>();

            m_Movement = GetComponent<TankMovement>();
        }

        private void Start()
        {
            m_ChargeSpeed = (m_MaxLaunchForce - m_MinLaunchForce) / m_MaxChargeTime;
        }

        private void Update()
        {
            if (!m_IsComputerControlled)
                HumanUpdate();
            else
                ComputerUpdate();
        }

        /// <summary>Used by AI to start charging</summary>
        public void StartCharging()
        {
            m_IsCharging = true;
            m_Fired = false;
            m_CurrentLaunchForce = m_MinLaunchForce;

            m_ShootingAudio.clip = m_ChargingClip;
            m_ShootingAudio.Play();
        }

        public void StopCharging()
        {
            if (m_IsCharging)
            {
                Fire();
                m_IsCharging = false;
            }
        }

        void ComputerUpdate()
        {
            m_AimSlider.value = m_BaseMinLaunchForce;

            if (m_CurrentLaunchForce >= m_MaxLaunchForce && !m_Fired)
            {
                m_CurrentLaunchForce = m_MaxLaunchForce;
                Fire();
            }
            else if (m_IsCharging && !m_Fired)
            {
                m_CurrentLaunchForce += m_ChargeSpeed * Time.deltaTime;
                m_AimSlider.value = m_CurrentLaunchForce;
            }
        }

        void HumanUpdate()
        {
            if (m_ShotCooldownTimer > 0.0f)
                m_ShotCooldownTimer -= Time.deltaTime;

            m_AimSlider.value = m_BaseMinLaunchForce;

            if (m_CurrentLaunchForce >= m_MaxLaunchForce && !m_Fired)
            {
                m_CurrentLaunchForce = m_MaxLaunchForce;
                Fire();
            }
            else if (m_ShotCooldownTimer <= 0 && FirePressedThisFrame)
            {
                m_Fired = false;
                m_CurrentLaunchForce = m_MinLaunchForce;

                m_ShootingAudio.clip = m_ChargingClip;
                m_ShootingAudio.Play();
            }
            else if (FireHeld && !m_Fired)
            {
                m_CurrentLaunchForce += m_ChargeSpeed * Time.deltaTime;
                m_AimSlider.value = m_CurrentLaunchForce;
            }
            else if (FireReleasedThisFrame && !m_Fired)
            {
                Fire();
            }
        }

        private void Fire()
        {
            m_Fired = true;

            Rigidbody shellInstance =
                Instantiate(m_Shell, m_FireTransform.position, m_FireTransform.rotation) as Rigidbody;

            shellInstance.linearVelocity = m_CurrentLaunchForce * m_FireTransform.forward;

            ShellExplosion explosionData = shellInstance.GetComponent<ShellExplosion>();
            explosionData.m_ExplosionForce = m_ExplosionForce;
            explosionData.m_ExplosionRadius = m_ExplosionRadius;
            explosionData.m_MaxDamage = m_MaxDamage;

            if (m_HasSpecialShell)
            {
                explosionData.m_MaxDamage *= m_SpecialShellMultiplier;
                m_HasSpecialShell = false;
                m_SpecialShellMultiplier = 1f;

                PowerUpDetector powerUpDetector = GetComponent<PowerUpDetector>();
                if (powerUpDetector != null)
                    powerUpDetector.m_HasActivePowerUp = false;

                PowerUpHUD powerUpHUD = GetComponentInChildren<PowerUpHUD>();
                if (powerUpHUD != null)
                    powerUpHUD.DisableActiveHUD();
            }

            m_ShootingAudio.clip = m_FireClip;
            m_ShootingAudio.Play();

            m_CurrentLaunchForce = m_MinLaunchForce;
            m_ShotCooldownTimer = m_ShotCooldown;
        }

        public void EquipSpecialShell(float damageMultiplier)
        {
            m_HasSpecialShell = true;
            m_SpecialShellMultiplier = damageMultiplier;
        }

        /// <summary>Estimated landing position for a charging level between 0 and 1.</summary>
        public Vector3 GetProjectilePosition(float chargingLevel)
        {
            float chargeLevel = Mathf.Lerp(m_MinLaunchForce, m_MaxLaunchForce, chargingLevel);
            Vector3 velocity = chargeLevel * m_FireTransform.forward;

            float a = 0.5f * Physics.gravity.y;
            float b = velocity.y;
            float c = m_FireTransform.position.y;

            float sqrtContent = b * b - 4 * a * c;
            if (sqrtContent <= 0)
                return m_FireTransform.position;

            float answer1 = (-b + Mathf.Sqrt(sqrtContent)) / (2 * a);
            float answer2 = (-b - Mathf.Sqrt(sqrtContent)) / (2 * a);
            float answer = answer1 > 0 ? answer1 : answer2;

            Vector3 position = m_FireTransform.position +
                               new Vector3(velocity.x, 0, velocity.z) * answer;
            position.y = 0;

            return position;
        }
    }
}