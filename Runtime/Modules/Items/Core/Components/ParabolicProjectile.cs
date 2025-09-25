using UltimateFramework.CollisionsAndDamageSystem;
using System.Collections;
using UnityEngine;
using MyBox;

public class ParabolicProjectile : MonoBehaviour
{
    [Range(1, 100)] public float launchSpeed = 20f;
    [Range(0, 10)] public float curveHeight = 1f;
    [MinMaxRange(260, 1000)] public MinMaxFloat rotationSpeedRange;

    [Header("Visuals")]
    public GameObject smashVFX;

    [Header("Sound")]
    public AudioClip impactClip;
    [Range(0, 100)] public float impactVolume = 1f;

    private Vector3 targetPosition;
    private Vector3 startPosition;
    private float timeToTarget;

    public WeaponDamageHandler DamageHandler { get; private set; }

    private void Awake()
    {
        DamageHandler = GetComponent<WeaponDamageHandler>();
    }

    public void Launch(Vector3 target)
    {
        targetPosition = target;
        startPosition = transform.position;
        timeToTarget = Vector3.Distance(startPosition, targetPosition) / launchSpeed;
        transform.SetParent(null);

        var collider = gameObject.AddComponent<MeshCollider>();
        collider.convex = true;

        StartCoroutine(LaunchProjectile());
    }

    private IEnumerator LaunchProjectile()
    {
        float elapsedTime = 0f;

        while (elapsedTime < timeToTarget)
        {
            elapsedTime += Time.deltaTime;
            float progress = elapsedTime / timeToTarget;

            // Interpolación lineal entre el punto de inicio y el objetivo
            Vector3 currentPosition = Vector3.Lerp(startPosition, targetPosition, progress);

            // Aplicar la curva de la parábola
            currentPosition.y += curveHeight * Mathf.Sin(Mathf.PI * progress);

            // Aplicar rotación
            var rotationSpeed = Random.Range(rotationSpeedRange.Min, rotationSpeedRange.Max);
            transform.Rotate(Vector3.forward, rotationSpeed * Time.deltaTime);

            transform.position = currentPosition;
            yield return null;
        }

        // Asegurarse de que el proyectil llegue exactamente al objetivo
        transform.position = targetPosition;

        // Reproduce el efecto visual de impacto
        smashVFX.SetActive(true);

        // Reproduce el sonido de impacto
        AudioSource.PlayClipAtPoint(impactClip, transform.position, impactVolume);

        yield return new WaitForEndOfFrame();
        DamageHandler.AllowCollisions = false;

        yield return new WaitForSeconds(5);
        Destroy(gameObject);
    }

#if UNITY_EDITOR
    void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;
        Vector3 previousPosition = startPosition;

        for (float t = 0; t < 1; t += 0.05f)
        {
            float progress = t;
            Vector3 currentPosition = Vector3.Lerp(startPosition, targetPosition, progress);
            currentPosition.y += curveHeight * Mathf.Sin(Mathf.PI * progress);
            Gizmos.DrawLine(previousPosition, currentPosition);
            previousPosition = currentPosition;
        }
    }
#endif
}
