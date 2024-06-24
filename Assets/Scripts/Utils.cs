using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Utils : MonoBehaviour

{
    public class VisibilityStats
    {
        public List<RaycastHit> SuccessfulHits = new();
        public float Visibility;
    }
    
    public class HitChance
    {
        public int Chance;
        public List<RaycastHit> SucessfulHits;
    }
    
    private static AnimationCurve _distanceCurve;
    
    private static int rayCount = 100;
    private static float rayDuration = 5f;
    private static bool debug = true;
    
    public static void SetDebug(bool _debug)
    {
        debug = _debug;
    }
    
    public static void SetDistanceCurve(AnimationCurve curve)
    {
        _distanceCurve = curve;
    }

    public static VisibilityStats CalculateVisibility(Vector3 origin, GameObject target, Bounds projectileBounds)
    {
        var stats = new VisibilityStats();
        
        if (target == null)
        {
            Debug.LogWarning("Target is not set.");
            return stats;
        }

        Collider targetCollider = target.GetComponent<Collider>();
        if (targetCollider == null)
        {
            Debug.LogWarning("Target does not have a Collider component.");
            return stats;
        }

        Bounds targetBounds = targetCollider.bounds;
        int gridSize = Mathf.CeilToInt(Mathf.Pow(rayCount, 1f / 3f));
        int hitCount = 0;
        int maxHits = 0;

        for (int i = 0; i < gridSize; i++)
        {
            for (int j = 0; j < gridSize; j++)
            {
                for (int k = 0; k < gridSize; k++)
                {
                    Vector3 pointOnBounds = GetPointOnBounds(targetBounds, i, j, k, gridSize, target);
                    Vector3 direction = pointOnBounds - origin;
                    Ray ray = new Ray(origin, direction);

                    float rayDistance = direction.magnitude;
                    
                    LayerMask mask = LayerMask.GetMask("Object") | LayerMask.GetMask("Entity");

                    if (Physics.BoxCast(origin, projectileBounds.extents, direction, out var hit, Quaternion.identity, rayDistance, mask))
                    {
                        maxHits++;

                        if (hit.collider == targetCollider)
                        {
                            if(debug) Debug.DrawLine(ray.origin, hit.point, Color.red, rayDuration);
                            hitCount++;
                            stats.SuccessfulHits.Add(hit);
                        }
                        else if (hit.collider.gameObject.layer == LayerMask.NameToLayer("Object"))
                        {
                            if(debug) Debug.DrawLine(ray.origin, hit.point, Color.green, rayDuration);
                        }
                        else
                        {
                            maxHits--;
                            if(debug) Debug.DrawLine(ray.origin, hit.point, Color.blue, rayDuration);
                        }
                    }
                }
            }
        }

        stats.Visibility = maxHits == 0 ? 0 : (float)hitCount / maxHits;
        return stats;
    }

    private static Vector3 GetPointOnBounds(Bounds bounds, int i, int j, int k, int gridSize, GameObject target)
    {
        float x = Mathf.Lerp(bounds.min.x, bounds.max.x, (float)i / (gridSize - 1));
        float y = Mathf.Lerp(bounds.min.y, bounds.max.y, (float)j / (gridSize - 1));
        float z = Mathf.Lerp(bounds.min.z, bounds.max.z, (float)k / (gridSize - 1));

        Vector3 localPoint = new Vector3(x, y, z);
        return target.transform.TransformPoint(localPoint - target.transform.position);
    }
    
    public static HitChance GetHitChance(GameObject origin, GameObject target, float attackRange, Bounds projectileBounds)
    {
        Vector3 targetPosition = target.transform.position;
        Vector3 originPosition = origin.transform.position;
        
        var distance = Vector3.Distance(originPosition, targetPosition);
        
        var distanceFactor = _distanceCurve.Evaluate(1 - distance / attackRange);
    
        var visibilityFactor = CalculateVisibility(originPosition, target, projectileBounds);

        var hitChance = (int)(distanceFactor * visibilityFactor.Visibility * 100);

        var result = new HitChance
        {
            Chance = hitChance,
            SucessfulHits = visibilityFactor.SuccessfulHits
        };

        return result;
    }
    
    public static Bounds GetBoundsBeforeInstantiate(GameObject obj)
    {
        Vector3 farAwayPosition = new Vector3(1000, 1000, 1000);
        var tempObject = Instantiate(obj, farAwayPosition, Quaternion.identity);

        Renderer renderer = tempObject.GetComponent<Renderer>();
        Collider collider = tempObject.GetComponent<Collider>();

        Bounds bounds;
        
        if (collider != null)
        {
            bounds = collider.bounds;
        }
        else if (renderer != null)
        {
            bounds = renderer.bounds;
        }
        else
        {
            bounds = new Bounds(Vector3.zero, Vector3.zero);
        }

        Destroy(tempObject);

        return bounds;
    }
}
