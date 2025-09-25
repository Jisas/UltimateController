using UltimateFramework.Utils;
using System.Collections;
using UnityEngine;
using UnityEditor;
using System;

namespace UltimateFramework.SoundSystem
{
    [RequireComponent(typeof(AudioSource))]
    public class SoundsFadeManager : MonoBehaviour
    {
        #region Serialized Fields
        [SerializeField] private bool initOnStart;
        [SerializeField] private bool infiniteLoop;
        [SerializeField] private bool useRandomSounds;
        [SerializeField] private bool isSoundZone;
        [SerializeField] private bool changeSoundByGameEvent;
        [Space]

        [SerializeField] private float fadeDuration = 2f;
        [SerializeField] private float maxVolume = 1f;
        [SerializeField, MyBox.ConditionalField(nameof(useRandomSounds), false, true)] private string trackName;
        [Space]

        [SerializeField, MyBox.ConditionalField(nameof(isSoundZone), false, true)] private SoundZoneType zoneType;
        [SerializeField, MyBox.ConditionalField(nameof(isSoundZone), false, true)] private Color shapeColor = Color.white;
        [MyBox.ConditionalField(nameof(zoneType), false, SoundZoneType.Square)] public Square square;
        [MyBox.ConditionalField(nameof(zoneType), false, SoundZoneType.Disc)] public float radius;
        [Space]

        [SerializeField, MyBox.ConditionalField(nameof(changeSoundByGameEvent), false, true)] private AudioClip newEventSound;
        #endregion

        #region Private Fields
        private Coroutine fadeOutCoroutine;
        private AudioSource m_AudioSource;
        private Transform playerTransofrm;
        private bool wasPlayerIntoZone;
        private bool isInZone;
        #endregion

        #region Properties
        public AudioSource AudioSource => m_AudioSource;
        #endregion

        #region Delegates And Events
        public delegate void RuntimeChangeSound();
        public RuntimeChangeSound OnRuntimeChangeSound;
        #endregion

        #region Mono
        void Awake()
        {
            if (isSoundZone) playerTransofrm = GameObject.FindGameObjectWithTag("Player").transform;
            m_AudioSource = GetComponent<AudioSource>();
            m_AudioSource.volume = 0f;
        }
        private void OnEnable()
        {
            if (changeSoundByGameEvent)
            {
                OnRuntimeChangeSound = ChangeSelfSoundHandler;
            }
        }
        private void Start()
        {
            if (useRandomSounds) 
                SoundManager.SetRandomClip(ref m_AudioSource, trackName);

            if (initOnStart) PlayAndFadeLoop();
        }
        private void Update()
        {
            if (isSoundZone)
                CheckPlayerInZone();

            if (initOnStart && infiniteLoop && isInZone && !m_AudioSource.isPlaying)
                PlayAndFadeLoop();
        }
        #endregion

        #region Private Methods
        private void PlayAndFadeLoop()
        {
            m_AudioSource.Play();
            StartCoroutine(FadeIn(m_AudioSource, fadeDuration, maxVolume));
            fadeOutCoroutine = StartCoroutine(FadeOut(m_AudioSource, fadeDuration, 0f));
        }
        private void CheckPlayerInZone()
        {
            var playerPos = playerTransofrm.position;

            isInZone = zoneType switch
            {
                SoundZoneType.Disc => IsInsideDisc(playerPos),
                SoundZoneType.Square => IsInsideSquare(playerPos),
                _ => false
            };

            SetZoneSwitch(isInZone);
        }
        private void SetZoneSwitch(bool comprobement)
        {
            if (comprobement)
            {
                wasPlayerIntoZone = true;
                SoundManager.Instance.CurrentZoneMusic = this;

                if ((SoundManager.Instance.LastZoneMusic != null) && (SoundManager.Instance.CurrentZoneMusic != SoundManager.Instance.LastZoneMusic))
                {
                    if(!m_AudioSource.isPlaying)
                    {
                        m_AudioSource.Play();
                        SoundManager.SwitchBetweenZoneMusic(fadeDuration, maxVolume);
                    }
                }
            }
            else
            {
                if (wasPlayerIntoZone) 
                    SoundManager.Instance.LastZoneMusic = this;
            }
        }
        private bool IsInsideDisc(Vector3 point)
        {
            var distance = Vector3.Distance(transform.position, point);
            return distance < radius;
        }
        private bool IsInsideSquare(Vector3 point)
        {
            // Convertir el punto a las coordenadas locales del objeto
            Vector3 localPoint = Quaternion.Euler(0, -transform.eulerAngles.y, 0) * (point - transform.position);

            return localPoint.x >= square.bottomLeftCorner.x && localPoint.x <= square.topRightCorner.x &&
                   localPoint.z >= square.bottomLeftCorner.y && localPoint.z <= square.topRightCorner.y;
        }
        private IEnumerator ChangeSelfSoundCouroutine()
        {
            fadeOutCoroutine = StartCoroutine(FadeOut(m_AudioSource, fadeDuration, 0f, true));
            yield return fadeOutCoroutine;

            m_AudioSource.clip = newEventSound;
            StartCoroutine(FadeIn(m_AudioSource, fadeDuration, maxVolume));
        }
        private void ChangeSelfSoundHandler() => StartCoroutine(ChangeSelfSoundCouroutine());
        #endregion

        #region Public Methods
        public IEnumerator FadeIn(AudioSource source, float duration, float targetVolume)
        {
            float time = 0f;
            float startVolume = source.volume;

            while (time < duration)
            {
                time += Time.deltaTime;
                source.volume = Mathf.Lerp(startVolume, targetVolume, time / duration);
                yield return null;
            }
        }
        public IEnumerator FadeOut(AudioSource source, float duration, float targetVolume, bool immediate = false)
        {
            if (fadeOutCoroutine != null) 
                StopCoroutine(fadeOutCoroutine);

            if (!immediate)
            {
                double lengthOfClip = (double)source.clip.samples / source.clip.frequency;
                yield return new WaitForSecondsRealtime((float)(lengthOfClip - duration));
            }

            float time = 0f;
            float startVolume = source.volume;

            while (time < duration)
            {
                time += Time.deltaTime;
                source.volume = Mathf.Lerp(startVolume, targetVolume, time / duration);
                yield return null;
            }

            source.Stop();
            if (useRandomSounds) SoundManager.SetRandomClip(ref source, trackName);
        }
        #endregion


#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            Handles.color = shapeColor;

            if (isSoundZone)
            {
                switch (zoneType)
                {
                    case SoundZoneType.Disc:
                        Handles.DrawWireDisc(transform.position, Vector3.up, radius, 3);
                        break;

                    case SoundZoneType.Square:
                        // Guardar la matriz de transformación original
                        Matrix4x4 originalMatrix = Gizmos.matrix;

                        // Obtener la posición del objeto que contiene el script
                        Vector3 objectPosition = transform.position;

                        // Crear la matriz de transformación para la rotación y la posición
                        Gizmos.matrix = Matrix4x4.TRS(objectPosition, Quaternion.Euler(0, transform.eulerAngles.y, 0), Vector3.one);

                        // Dibujar el cuadrado
                        Vector3 bottomLeft = new (square.bottomLeftCorner.x, 0, square.bottomLeftCorner.y);
                        Vector3 topRight = new (square.topRightCorner.x, 0, square.topRightCorner.y);
                        Vector3 bottomRight = new (topRight.x, 0, bottomLeft.z);
                        Vector3 topLeft = new (bottomLeft.x, 0, topRight.z);

                        Gizmos.color = shapeColor;
                        Gizmos.DrawLine(bottomLeft, bottomRight);
                        Gizmos.DrawLine(bottomRight, topRight);
                        Gizmos.DrawLine(topRight, topLeft);
                        Gizmos.DrawLine(topLeft, bottomLeft);

                        // Restaurar la matriz de transformación original
                        Gizmos.matrix = originalMatrix;
                        break;
                }
            }
        }
#endif
    }

    [Serializable]
    public struct Square
    {
        public Vector2 bottomLeftCorner;
        public Vector2 topRightCorner;
    }
}
