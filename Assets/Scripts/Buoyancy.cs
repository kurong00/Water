using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Buoyancy : MonoBehaviour
{

    float voxelHalfHeight;
    List<Vector3> voxels;
    List<Vector3[]> forces;
    float localArchimedForce;
    const float WaterDensity = 1000;
    const float Dampfer = 0.1f;
    WaterRipple waterRipple;
    bool isMeshCollider;

    public float Density = 700;
    //波浪的速度
    public float WaveVelocity = 0.05f;
    //物体的受力点，物体越复杂，受力点需要的越多
    public int slicedAxis = 2;
    public bool IsConcave;
    public int VoxelsLimit = 16;
    void Start()
    {
        forces = new List<Vector3[]>();
        var originalRotation = transform.rotation;
        var originalPosition = transform.position;
        transform.rotation = Quaternion.identity;
        transform.position = Vector3.zero;
        if (!GetComponent<Collider>())
            gameObject.AddComponent<MeshCollider>();
        isMeshCollider = GetComponent<MeshCollider>() != null;
        var bounds = GetComponent<Collider>().bounds;
        voxelHalfHeight = bounds.size.x < bounds.size.y ? bounds.size.x : bounds.size.y;
        voxelHalfHeight = bounds.size.z < voxelHalfHeight ? bounds.size.z : voxelHalfHeight;
        voxelHalfHeight /= 2 * slicedAxis;
        if (!GetComponent<Rigidbody>())
            gameObject.AddComponent<Rigidbody>();
        GetComponent<Rigidbody>().centerOfMass = new Vector3(0, -bounds.extents.y * 0f, 0) + transform.InverseTransformPoint(bounds.center);
        voxels = SliceIntoVoxels(IsConcave && isMeshCollider);
        transform.rotation = originalRotation;
        transform.position = originalPosition;
        float volume = GetComponent<Rigidbody>().mass / Density;
        LinkPoints(voxels, VoxelsLimit);
        float archimedesForceMagnitude = WaterDensity * Mathf.Abs(Physics.gravity.y) * volume;
        localArchimedForce = archimedesForceMagnitude / voxels.Count;
    }

    void FixedUpdate()
    {
        if (!waterRipple)
            return;
        forces.Clear();
        var length = voxels.Count;
        var pointCache = new Vector3[length];
        for (int i = 0; i < length; ++i)
        {
            var wavePoint = transform.TransformPoint(voxels[i]);
            pointCache[i] = waterRipple.GetOffsetByPosition((wavePoint));
        }
        var normal = (GetNormal(pointCache[0], pointCache[1], pointCache[2]) * WaveVelocity + Vector3.up).normalized;
        for (int i = 0; i < length; ++i)
        {
            var wavePoint = transform.TransformPoint(voxels[i]);
            float waterLevel = pointCache[i].y;
            if (wavePoint.y - voxelHalfHeight < waterLevel)
            {
                float k = (waterLevel - wavePoint.y) / (2 * voxelHalfHeight) + 0.5f;
                if (k > 1)
                    k = 1f;
                else if (k < 0)
                    k = 0f;
                var velocity = GetComponent<Rigidbody>().GetPointVelocity(wavePoint);
                var localDampingForce = -velocity * Dampfer * GetComponent<Rigidbody>().mass;
                Vector3 force = localDampingForce + Mathf.Sqrt(k) * (normal * localArchimedForce);
                GetComponent<Rigidbody>().AddForceAtPosition(force, wavePoint);
                forces.Add(new[] { wavePoint, force });
            }
        }
    }

    void OnDrawGizmos()
    {
        if (voxels == null || forces == null)
        {
            return;
        }
        const float gizmoSize = 0.05f;
        Gizmos.color = Color.yellow;
        foreach (var p in voxels)
        {
            Gizmos.DrawCube(transform.TransformPoint(p), new Vector3(gizmoSize, gizmoSize, gizmoSize));
        }
        Gizmos.color = Color.cyan;
        foreach (var force in forces)
        {
            Gizmos.DrawCube(force[0], new Vector3(gizmoSize, gizmoSize, gizmoSize));
            Gizmos.DrawLine(force[0], force[0] + force[1] / GetComponent<Rigidbody>().mass);
        }
    }
    void OnTriggerEnter(Collider other)
    {
        var temp = other.GetComponent<WaterRipple>();
        if (temp != null)
            waterRipple = temp;
    }

    Vector3 GetNormal(Vector3 a, Vector3 b, Vector3 c)
    {
        var side1 = b - a;
        var side2 = c - a;
        return Vector3.Cross(side1, side2).normalized;
    }

    List<Vector3> SliceIntoVoxels(bool isConcave)
    {
        var pos = new List<Vector3>(slicedAxis * slicedAxis * slicedAxis);
        if (IsConcave)
        {
            var meshCollider = GetComponent<MeshCollider>();
            var convexValue = meshCollider.convex;
            meshCollider.convex = false;
            var bounds = GetComponent<Collider>().bounds;
            for (int ix = 0; ix < slicedAxis; ix++)
            {
                for (int iy = 0; iy < slicedAxis; iy++)
                {
                    for (int iz = 0; iz < slicedAxis; iz++)
                    {
                        float x = bounds.min.x + bounds.size.x / slicedAxis * (0.5f + ix);
                        float y = bounds.min.y + bounds.size.y / slicedAxis * (0.5f + iy);
                        float z = bounds.min.z + bounds.size.z / slicedAxis * (0.5f + iz);
                        var point = transform.InverseTransformPoint(new Vector3(x, y, z));
                        if (PointIsInsideMeshCollider(meshCollider, point))
                        {
                            pos.Add(point);
                        }
                    }
                }
            }
            if (pos.Count == 0)
            {
                pos.Add(bounds.center);
            }
        }
        else
        {
            var bounds = GetComponent<Collider>().bounds;
            for (int ix = 0; ix < slicedAxis; ix++)
            {
                for (int iy = 0; iy < slicedAxis; iy++)
                {
                    for (int iz = 0; iz < slicedAxis; iz++)
                    {
                        float x = bounds.min.x + bounds.size.x / slicedAxis * (0.5f + ix);
                        float y = bounds.min.y + bounds.size.y / slicedAxis * (0.5f + iy);
                        float z = bounds.min.z + bounds.size.z / slicedAxis * (0.5f + iz);
                        var point = transform.InverseTransformPoint(new Vector3(x, y, z));
                        pos.Add(point);
                    }
                }
            }
        }
        return pos;
    }

    bool PointIsInsideMeshCollider(Collider c, Vector3 p)
    {
        Vector3[] directions = { Vector3.up, Vector3.down, Vector3.left, Vector3.right, Vector3.forward, Vector3.back };
        foreach (var ray in directions)
        {
            RaycastHit hit;
            if (c.Raycast(new Ray(p - ray * 1000, ray), out hit, 1000f) == false)
            {
                return false;
            }
        }
        return true;
    }

    void LinkPoints(IList<Vector3> list, int targetCount)
    {
        if (list.Count <= 2 || targetCount <= 1)
        {
            return;
        }
        while (list.Count > targetCount)
        {
            int first, second;
            FindClosestPoints(list, out first, out second);
            var mixed = (list[first] + list[second]) * 0.5f;
            list.RemoveAt(second); 
            list.RemoveAt(first);
            list.Add(mixed);
        }
    }

    void FindClosestPoints(IList<Vector3> list, out int firstIndex, out int secondIndex)
    {
        float minDistance = float.MaxValue, maxDistance = float.MinValue;
        firstIndex = 0;
        secondIndex = 1;
        for (int i = 0; i < list.Count - 1; i++)
        {
            for (int j = i + 1; j < list.Count; j++)
            {
                float distance = Vector3.Distance(list[i], list[j]);
                if (distance < minDistance)
                {
                    minDistance = distance;
                    firstIndex = i;
                    secondIndex = j;
                }
                if (distance > maxDistance)
                {
                    maxDistance = distance;
                }
            }
        }
    }
}
