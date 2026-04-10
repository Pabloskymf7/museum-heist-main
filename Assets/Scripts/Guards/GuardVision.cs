using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class GuardVision : MonoBehaviour
{
    [Header("Vision Settings")]
    [SerializeField] private float viewAngle = 60f;
    [SerializeField] private float viewDistance = 2f;
    [SerializeField] private int rayCount = 30;
    [SerializeField] private LayerMask playerLayer;
    [SerializeField] private LayerMask obstacleLayer;

    [Header("References")]
    [SerializeField] private GuardAI guardAI;

    private Mesh visionMesh;

    private void Awake()
    {
        visionMesh = new Mesh { name = "Vision Cone" };
        GetComponent<MeshFilter>().mesh = visionMesh;

        var meshRenderer = GetComponent<MeshRenderer>();
        meshRenderer.material = new Material(Shader.Find("Sprites/Default"))
        {
            color = new Color(1f, 0f, 0f, 0.3f)
        };
        meshRenderer.sortingOrder = 1;

        if (guardAI == null)
            guardAI = GetComponentInParent<GuardAI>();
    }

    private void LateUpdate()
    {
        DrawVisionCone();
        CheckForPlayer();
    }

    private void DrawVisionCone()
    {
        float halfAngle = viewAngle / 2f;
        float angleStep = viewAngle / rayCount;

        Vector3[] vertices = new Vector3[rayCount + 2];
        int[] triangles = new int[rayCount * 3];

        vertices[0] = Vector3.zero;

        for (int i = 0; i <= rayCount; i++)
        {
            float angle = -halfAngle + angleStep * i;
            Vector3 dir = DirFromAngle(angle);
            RaycastHit2D hit = Physics2D.Raycast(transform.position, transform.TransformDirection(dir), viewDistance, obstacleLayer);
            float dist = hit ? hit.distance : viewDistance;
            vertices[i + 1] = dir * dist;
        }

        for (int i = 0; i < rayCount; i++)
        {
            triangles[i * 3] = 0;
            triangles[i * 3 + 1] = i + 1;
            triangles[i * 3 + 2] = i + 2;
        }

        visionMesh.Clear();
        visionMesh.vertices = vertices;
        visionMesh.triangles = triangles;
        visionMesh.RecalculateNormals();
    }

    private void CheckForPlayer()
    {
        Collider2D playerCollider = Physics2D.OverlapCircle(transform.position, viewDistance, playerLayer);

        if (playerCollider == null)
        {
            guardAI?.OnPlayerLost();
            return;
        }

        Vector2 forward = transform.up;
        Vector2 dirToPlayer = (playerCollider.transform.position - transform.position).normalized;
        float angleToPlayer = Vector2.Angle(forward, dirToPlayer);

        if (angleToPlayer > viewAngle / 2f)
        {
            guardAI?.OnPlayerLost();
            return;
        }

        float distToPlayer = Vector2.Distance(transform.position, playerCollider.transform.position);
        RaycastHit2D hit = Physics2D.Raycast(transform.position, dirToPlayer, distToPlayer, obstacleLayer);

        if (hit.collider != null)
        {
            guardAI?.OnPlayerLost();
            return;
        }

        guardAI?.OnPlayerSpotted(playerCollider.transform);
    }

    private Vector3 DirFromAngle(float angleDeg)
    {
        float rad = angleDeg * Mathf.Deg2Rad;
        return new Vector3(Mathf.Sin(rad), Mathf.Cos(rad), 0f);
    }

    public void ResetDetection() { }
}