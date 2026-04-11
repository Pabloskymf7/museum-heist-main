using UnityEngine;

public class GuardAI : MonoBehaviour
{
    public enum GuardState { Patrol, Suspicious, Alert }
    public GuardState CurrentState { get; private set; } = GuardState.Patrol;

    public Transform[] waypoints;
    public int startWaypointIndex = 0;
    public float moveSpeed = 2f;
    public float waypointWaitTime = 1f;
    public float waypointReachThreshold = 0.1f;
    public float separationRadius = 1f;
    public float separationForce = 3f;
    public float suspicionTime = 0.5f;
    public float alertSpeed = 4f;
    public float loseSightTime = 3f;

    // Se buscan automáticamente en Awake
    private GameObject alertIndicator;
    private Transform visionTransform;

    // Internal
    private Rigidbody2D rb;
    private Vector2 desiredVelocity;
    private int currentWaypointIndex;
    private float waitTimer = 0f;
    private bool isWaiting = false;

    private float suspicionTimer = 0f;
    private float loseSightTimer = 0f;
    private Vector2 lastKnownPosition;
    private Transform playerTransform;
    private bool returningToLastKnown = false;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        rb.freezeRotation = true;
        currentWaypointIndex = startWaypointIndex;

        GuardVision vision = GetComponentInChildren<GuardVision>();
        if (vision != null) visionTransform = vision.transform;

        // Busca el indicador "!" por nombre entre los hijos
        Transform indicator = transform.Find("AlertIndicator");
        if (indicator != null)
        {
            alertIndicator = indicator.gameObject;
            alertIndicator.SetActive(false);
        }
    }

    private void Update()
    {
        desiredVelocity = Vector2.zero;
        switch (CurrentState)
        {
            case GuardState.Patrol:     UpdatePatrol();     break;
            case GuardState.Suspicious: UpdateSuspicious(); break;
            case GuardState.Alert:      UpdateAlert();      break;
        }
    }

    private void FixedUpdate()
    {
        rb.linearVelocity = desiredVelocity + GetSeparationVelocity();
    }

    private Vector2 GetSeparationVelocity()
    {
        Vector2 separation = Vector2.zero;
        Collider2D[] nearby = Physics2D.OverlapCircleAll(rb.position, separationRadius);
        foreach (Collider2D col in nearby)
        {
            if (col.gameObject == gameObject) continue;
            if (col.GetComponent<GuardAI>() == null) continue;
            Vector2 away = rb.position - (Vector2)col.transform.position;
            float dist = away.magnitude;
            if (dist < 0.001f) continue;
            separation += away.normalized * (separationRadius - dist) / separationRadius;
        }
        return separation * separationForce;
    }

    // ── Called by GuardVision ─────────────────────────────────────────────

    public void OnPlayerSpotted(Transform player)
    {
        playerTransform = player;
        lastKnownPosition = player.position;

        switch (CurrentState)
        {
            case GuardState.Patrol:
                SetState(GuardState.Suspicious);
                break;
            case GuardState.Suspicious:
                suspicionTimer += Time.deltaTime;
                RotateToward(player.position);
                if (suspicionTimer >= suspicionTime)
                    SetState(GuardState.Alert);
                break;
            case GuardState.Alert:
                lastKnownPosition = player.position;
                loseSightTimer = 0f;
                break;
        }
    }

    public void OnPlayerLost()
    {
        if (CurrentState == GuardState.Alert)
        {
            loseSightTimer += Time.deltaTime;
            if (loseSightTimer >= loseSightTime)
            {
                returningToLastKnown = true;
                SetState(GuardState.Patrol);
            }
        }
        else if (CurrentState == GuardState.Suspicious)
        {
            // Lost sight during suspicion — reset
            suspicionTimer = 0f;
            SetState(GuardState.Patrol);
        }
    }

    public void SetState(GuardState newState)
    {
        CurrentState = newState;

        switch (newState)
        {
            case GuardState.Patrol:
                if (alertIndicator != null) alertIndicator.SetActive(false);
                suspicionTimer = 0f;
                loseSightTimer = 0f;
                break;

            case GuardState.Suspicious:
                suspicionTimer = 0f;
                if (alertIndicator != null) alertIndicator.SetActive(false);
                break;

            case GuardState.Alert:
                if (alertIndicator != null) alertIndicator.SetActive(true);
                loseSightTimer = 0f;
                AudioManager.Instance?.PlayPlayerDetected();
                if (GameManager.Instance != null)
                    GameManager.Instance.PlayerDetected();
                break;
        }
    }

    // ── State updates ─────────────────────────────────────────────────────

    private void UpdatePatrol()
    {
        if (waypoints == null || waypoints.Length == 0) return;

        if (returningToLastKnown)
        {
            MoveToward(lastKnownPosition, moveSpeed);
            if (Vector2.Distance(transform.position, lastKnownPosition) < waypointReachThreshold)
                returningToLastKnown = false;
            return;
        }

        if (isWaiting)
        {
            waitTimer -= Time.deltaTime;
            if (waitTimer <= 0f) isWaiting = false;
            return;
        }

        PatrolToWaypoint();
    }

    private void UpdateSuspicious()
    {
        // Guard stops and rotates toward last known — handled in OnPlayerSpotted
        // If no new spotted call comes this frame, do nothing (vision handles it)
    }

    private void UpdateAlert()
    {
        if (playerTransform == null) return;
        MoveToward(playerTransform.position, alertSpeed);
    }

    // ── Helpers ───────────────────────────────────────────────────────────

    private void PatrolToWaypoint()
    {
        Transform target = waypoints[currentWaypointIndex];
        MoveToward(target.position, moveSpeed);

        if (Vector2.Distance(transform.position, target.position) <= waypointReachThreshold)
        {
            currentWaypointIndex = (currentWaypointIndex + 1) % waypoints.Length;
            isWaiting = true;
            waitTimer = waypointWaitTime;
        }
    }

    private void MoveToward(Vector2 target, float speed)
    {
        Vector2 direction = ((Vector2)target - rb.position).normalized;
        desiredVelocity = direction * speed;
        RotateToward(target);
    }

    private void RotateToward(Vector2 target)
    {
        Transform t = visionTransform != null ? visionTransform : transform;
        Vector2 dir = ((Vector3)target - transform.position).normalized;
        if (dir == Vector2.zero) return;
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg - 90f;
        t.rotation = Quaternion.Euler(0f, 0f, angle);
    }
}