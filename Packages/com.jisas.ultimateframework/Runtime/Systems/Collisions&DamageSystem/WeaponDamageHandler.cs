using System.Collections.Generic;
using UnityEngine;

namespace UltimateFramework.CollisionsAndDamageSystem
{
    public class WeaponDamageHandler : DamageComponent
    {
        public TagSelector weaponDamageTag;
        [Space(10)]

        public LayerMask collisionLayers;
        [Space(10)]

        public Transform start;
        public Transform end;
        [Space(10)]

        public AxisType axisType = AxisType.Z;
        public RayType rayType = RayType.Straight;
        public int rayCount = 5;
        public float raySeparation = 0.2f;
        [Range(0, 90)] public float fanAngle = 45f;

        public bool AllowCollisions { get; set; } = false;
        public enum RayType { Straight, Fan }
        public enum AxisType { X, Z }

        private WeaponBehaviour m_WeaponBehaviour;
        private readonly List<Ray> rays_A = new();
        private readonly List<GameObject> hitObjects = new();

        private void Awake()
        {
            m_WeaponBehaviour = GetComponent<WeaponBehaviour>();
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
                        if (axisType == AxisType.X) rayStart += transform.right * raySeparation * (i - rayCount / 2);
                        else rayStart += transform.forward * raySeparation * (i - rayCount / 2);
                        Vector3 direction = end.position - start.position;
                        rays_A.Add(new Ray(rayStart, direction));
                    }
                    break;

                case RayType.Fan:
                    for (int i = 0; i < rayCount; i++)
                    {
                        Vector3 direction;
                        if (axisType == AxisType.X) direction = Quaternion.Euler(0, fanAngle * (i - rayCount / 2), 0) * (end.position - start.position);
                        else direction = Quaternion.Euler(fanAngle * (i - rayCount / 2), 0, 0) * (end.position - start.position);
                        rays_A.Add(new Ray(start.position, direction));
                    }
                    break;
            }

            return rays_A;
        }

        private void FixedUpdate()
        {
            if (AllowCollisions)
            {
                // Crear lista para almacenar los rayos
                List<Ray> rays = CreateRays();

                // Procesar colisiones para cada rayo
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

                            if (!hitObjects.Contains(mainObject))
                            {
                                hitObjects.Add(mainObject);

                                var weaponDamage = m_WeaponBehaviour.Item.FindStat(weaponDamageTag.tag).CurrentValue;
                                DoDamage(mainObject, weaponDamage);
                            }
                        }
                    }
                }
            }
            else hitObjects.Clear();
        }

        private void DoDamage(GameObject objective, float damage)
        {
            var collisionsComp = objective.GetComponent<CharacterDamageHandler>();
            Vector3 hitDirection = objective.transform.position - this.transform.position;           
            collisionsComp.ReceiveDamage(this.gameObject, damage, hitDirection);
        }

#if UNITY_EDITOR
        void OnDrawGizmosSelected()
        {
            // Crear y dibujar rayos en el editor
            List<Ray> rays = CreateRays();
            Gizmos.color = Color.red;
            foreach (Ray ray in rays)
            {
                Gizmos.DrawRay(ray.origin, ray.direction * Vector3.Distance(ray.origin, end.position));
            }
        }
    }
#endif
}