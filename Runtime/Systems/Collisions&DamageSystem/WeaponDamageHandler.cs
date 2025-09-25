using System.Collections.Generic;
using UltimateFramework.Utils;
using UnityEngine;
using System;

namespace UltimateFramework.CollisionsAndDamageSystem
{
    public class WeaponDamageHandler : DamageComponent
    {
        #region PublicValues
        public TagSelector weaponDamageTag;
        [Space(10)]

        public LayerMask collisionLayers;
        public DetectionType detectionType = DetectionType.Raycast;
        [Space(10)]

        [ConditionalField("detectionType", DetectionType.Collider)] public Transform startPoint;
        [ConditionalField("detectionType", DetectionType.Collider)] public Vector3 size;
        [ConditionalField("detectionType", DetectionType.Raycast)] public Transform start;
        [ConditionalField("detectionType", DetectionType.Raycast)] public Transform end;
        [Space(10)]

        [ConditionalField("detectionType", DetectionType.Raycast)] public AxisType axisType = AxisType.Z;
        [ConditionalField("detectionType", DetectionType.Raycast)] public RayType rayType = RayType.Straight;
        [ConditionalField("detectionType", DetectionType.Raycast)] public int rayCount = 5;
        [ConditionalField("detectionType", DetectionType.Raycast)] public float raySeparation = 0.2f;
        [ConditionalField("rayType", RayType.Fan), Range(0, 90)] public float fanAngle = 45f;
        #endregion

        #region Events
        public event Action OnCollisionDetected;
        #endregion

        #region Properties
        public bool AllowCollisions { get; set; } = false;
        public GameObject CurrentHitedEnemy { get; set; }
        #endregion

        #region PrivateValues
        public bool isCollisioning = false;
        private readonly List<Ray> rays_A = new();
        private readonly List<GameObject> hitObjects = new();

        // References
        private WeaponBehaviour m_WeaponBehaviour;
        #endregion

        #region Mono
        private void Awake()
        {
            m_WeaponBehaviour = GetComponent<WeaponBehaviour>();
        }
        private void FixedUpdate()
        {
            if (AllowCollisions)
            {
                switch (detectionType)
                {
                    case DetectionType.Raycast:
                        HandleRaycastCollisions();
                        break;

                    case DetectionType.Collider:
                        HandleColliderCollisions();
                        break;
                }
            }
            else hitObjects.Clear();
        }
        #endregion

        #region Internal
        private void HandleRaycastCollisions()
        {
            List<Ray> rays = CreateRays();

            foreach (Ray ray in rays)
            {
                float distance = Vector3.Distance(ray.origin, end.position);
                RaycastHit[] hits = Physics.RaycastAll(ray.origin, ray.direction, distance, collisionLayers, QueryTriggerInteraction.Ignore);

                foreach (RaycastHit hit in hits)
                {
                    GameObject objectImpact = hit.collider.gameObject;

                    if (objectImpact != gameObject)
                    {
                        GameObject mainObject = objectImpact.transform.root.gameObject;
                        CurrentHitedEnemy = mainObject;

                        if (!hitObjects.Contains(mainObject))
                        {
                            hitObjects.Add(mainObject);
                            OnCollisionDetected?.Invoke();

                            float weaponDamage = m_WeaponBehaviour.Item.FindStat(weaponDamageTag.tag).CurrentValue;
                            DoDamage(mainObject, weaponDamage, hit.point);
                        }
                    }
                }
            }
        }
        private void HandleColliderCollisions()
        {
            Collider[] colliders = Physics.OverlapBox(startPoint.position, size / 2, transform.rotation, collisionLayers, QueryTriggerInteraction.Ignore);

            foreach (Collider collider in colliders)
            {
                GameObject objectImpact = collider.gameObject;

                if (objectImpact != gameObject)
                {
                    GameObject mainObject = objectImpact.transform.root.gameObject;

                    if (!hitObjects.Contains(mainObject))
                    {
                        hitObjects.Add(mainObject);
                        OnCollisionDetected?.Invoke();

                        float weaponDamage = m_WeaponBehaviour.Item.FindStat(weaponDamageTag.tag).CurrentValue;
                        Vector3 contactPoint = collider.ClosestPoint(startPoint.position);
                        DoDamage(mainObject, weaponDamage, contactPoint);
                    }
                }
            }
        }

        private List<Ray> CreateRays()
        {
            rays_A.Clear();

            // Crear rayos dependiendo del tipo y eje seleccionado
            switch (rayType)
            {
                case RayType.Straight:
                    for (int i = 0; i < rayCount; i++)
                    {
                        Vector3 rayStart = start.position;
                        if (axisType == AxisType.X) rayStart += (i - rayCount / 2) * raySeparation * transform.right;
                        else if (axisType == AxisType.Y) rayStart += (i - rayCount / 2) * raySeparation * transform.up;
                        else if (axisType == AxisType.Z) rayStart += (i - rayCount / 2) * raySeparation * transform.forward;

                        Vector3 direction = end.position - start.position;
                        rays_A.Add(new Ray(rayStart, direction));
                    }
                    break;

                case RayType.Fan:
                    for (int i = 0; i < rayCount; i++)
                    {
                        Vector3 direction = Vector3.zero;
                        if (axisType == AxisType.X) direction = Quaternion.Euler(0, fanAngle * (i - rayCount / 2), 0) * (end.position - start.position);
                        else if (axisType == AxisType.Y) direction = Quaternion.Euler(0, 0, fanAngle * (i - rayCount / 2)) * (end.position - start.position);
                        else if (axisType == AxisType.Z) direction = Quaternion.Euler(fanAngle * (i - rayCount / 2), 0, 0) * (end.position - start.position);

                        rays_A.Add(new Ray(start.position, direction));
                    }
                    break;
            }

            return rays_A;
        }
        private void DoDamage(GameObject objective, float damage, Vector3 contactPoint)
        {
            if (objective.TryGetComponent<CharacterDamageHandler>(out var collisionsComp))
            {
                Vector3 hitDirection = objective.transform.position - this.transform.position;
                collisionsComp.ReceiveDamage(this.gameObject, damage, hitDirection, contactPoint);
            }
        }
        #endregion


#if UNITY_EDITOR
        void OnDrawGizmosSelected()
        {
            if (detectionType == DetectionType.Raycast)
            {
                Gizmos.color = Color.red;
                List<Ray> rays = CreateRays();

                foreach (Ray ray in rays)
                {
                    Gizmos.DrawRay(ray.origin, ray.direction * Vector3.Distance(ray.origin, end.position));
                }
            }
            else if (detectionType == DetectionType.Collider)
            {
                Gizmos.color = Color.black;
                Gizmos.matrix = Matrix4x4.TRS(startPoint.position, transform.rotation, Vector3.one);
                Gizmos.DrawCube(Vector3.zero, size);
            }
        }
#endif
    }
}