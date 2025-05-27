using System.Collections.Generic;
using UnityEngine;

public class BezierProjectile : MonoBehaviour {
    [Header("Path Settings")] public Vector3 start;
    public Vector3 end;
    public float arcHeight = 5f;
    public float duration = 1f;
    private Vector3 _p1, _p2;
    private float _distanceTraveled;
    private float _totalLength;
    private float _speed;
    private bool _moving;
    private const int SampleResolution = 200;
    private readonly List<float> _arcLengthTable = new List<float>(); // normalized distance [0-1] → t
    public void Launch(Vector3 startPos, Vector3 endPos, float arcH, float dur) {
        start = startPos;
        end = endPos;
        arcHeight = arcH;
        duration = dur;
        _p1 = start + Vector3.up * arcHeight;
        _p2 = end + Vector3.up * arcHeight;
        BuildArcLengthTable();
        _speed = _totalLength / duration;
        _distanceTraveled = 0f;
        _moving = true;
    }
    void Update() {
        if (!_moving)
            return;
        _distanceTraveled += Time.deltaTime * _speed;
        if (_distanceTraveled >= _totalLength) {
            transform.position = end;
            _moving = false;
            GetComponent<AmmoCollision>().Disintegrate();
            // Damage player or enemy tank depending on ownership
            AmmoCollision ammoCollisionScript = GetComponent<AmmoCollision>();
            int targetId = ammoCollisionScript.OwnerId == 0 ? 1 : 0;
            AmmoData projectileData = ammoCollisionScript.ProjectileData;
            TankDelegates.InvokeOnTakeDamage(targetId, projectileData.Damage);
            return;
        }
        float normalizedDistance = _distanceTraveled / _totalLength;
        float t = GetTFromArcLength(normalizedDistance);
        transform.position = GetPoint(t);
    }
    Vector3 GetPoint(float t) {
        return Mathf.Pow(1 - t, 3) * start + 3 * Mathf.Pow(1 - t, 2) * t * _p1 + 3 * (1 - t) * Mathf.Pow(t, 2) * _p2 + Mathf.Pow(t, 3) * end;
    }
    void BuildArcLengthTable() {
        _arcLengthTable.Clear();
        float length = 0f;
        _arcLengthTable.Add(0f);
        Vector3 prev = GetPoint(0f);
        for (int i = 1; i <= SampleResolution; i++) {
            float t = i / (float)SampleResolution;
            Vector3 point = GetPoint(t);
            length += Vector3.Distance(prev, point);
            prev = point;
            _arcLengthTable.Add(length);
        }
        _totalLength = length;

        // Normalize the table to 0–1 for easier lookup
        for (int i = 0; i < _arcLengthTable.Count; i++) {
            _arcLengthTable[i] /= _totalLength;
        }
    }
    float GetTFromArcLength(float normalizedDistance) {
        for (int i = 0; i < _arcLengthTable.Count - 1; i++) {
            float d0 = _arcLengthTable[i];
            float d1 = _arcLengthTable[i + 1];
            if (normalizedDistance >= d0 && normalizedDistance <= d1) {
                float segmentT = (float)i / SampleResolution;
                float segmentLength = d1 - d0;
                float withinSegment = (normalizedDistance - d0) / segmentLength;
                return segmentT + withinSegment * (1f / SampleResolution);
            }
        }
        return 1f; // fallback
    }
    public void FlipX() {
        transform.Rotate(0f, 180f, 0f);
    }
}
