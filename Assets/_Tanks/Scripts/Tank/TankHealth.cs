using UnityEngine;
using UnityEngine.UI;

namespace Tanks
{
    public class TankHealth : MonoBehaviour
    {
        public bool m_HasShield = false;                // true mientras hay escudo activo.
        public bool m_IsInvincible = false;             // true mientras es invencible.
        private float m_ShieldValue = 0f;               // Porcentaje de reducción de daño [0, 1].
        public float m_StartingHealth = 100f;           // Vida inicial del tanque.
        public Slider m_Slider;                         // Slider que representa la vida.
        public Image m_FillImage;                       // Imagen de relleno del slider.
        public Color m_FullHealthColor = Color.green;   // Color con vida completa.
        public Color m_ZeroHealthColor = Color.red;     // Color con vida a cero.
        public GameObject m_ExplosionPrefab;            // Prefab de la explosión al morir.

        private AudioSource m_ExplosionAudio;
        private ParticleSystem m_ExplosionParticles;
        private float m_CurrentHealth;
        private bool m_Dead;

        private void Awake ()
        {
            m_ExplosionParticles = Instantiate (m_ExplosionPrefab).GetComponent<ParticleSystem> ();
            m_ExplosionAudio = m_ExplosionParticles.GetComponent<AudioSource> ();
            m_ExplosionParticles.gameObject.SetActive (false);
        }

        private void OnEnable ()
        {
            m_CurrentHealth = m_StartingHealth;
            m_Dead = false;
            m_HasShield = false;
            m_IsInvincible = false;
            SetHealthUI ();
        }

        public void TakeDamage (float amount)
        {
            // Invencible: no recibe daño.
            if (m_IsInvincible)
                return;

            // Con escudo, el daño se reduce según el porcentaje.
            if (m_HasShield)
                amount *= (1f - m_ShieldValue);

            m_CurrentHealth -= amount;
            SetHealthUI ();

            if (m_CurrentHealth <= 0f && !m_Dead)
                OnDeath ();
        }

        // Activa o desactiva el escudo.
        public void ToggleShield (float shieldAmount)
        {
            m_HasShield = !m_HasShield;
            m_ShieldValue = m_HasShield ? shieldAmount : 0f;
        }

        // Activa o desactiva la invencibilidad.
        public void ToggleInvincibility ()
        {
            m_IsInvincible = !m_IsInvincible;
        }

        // Suma vida sin pasar del máximo.
        public void IncreaseHealth (float amount)
        {
            m_CurrentHealth = Mathf.Min (m_CurrentHealth + amount, m_StartingHealth);
            SetHealthUI ();
        }

        private void SetHealthUI ()
        {
            m_Slider.value = m_CurrentHealth;
            m_FillImage.color = Color.Lerp (m_ZeroHealthColor, m_FullHealthColor, m_CurrentHealth / m_StartingHealth);
        }

        private void OnDeath ()
        {
            m_Dead = true;

            m_ExplosionParticles.transform.position = transform.position;
            m_ExplosionParticles.gameObject.SetActive (true);
            m_ExplosionParticles.Play ();
            m_ExplosionAudio.Play ();

            gameObject.SetActive (false);
        }
    }
}