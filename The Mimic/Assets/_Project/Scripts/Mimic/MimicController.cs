using System;
using UnityEngine;
using UnityEngine.AI;

namespace TheMimic
{
    // Enum state machine for the Mimic: Disguised -> Revealed -> Hunting -> Retreating -> Disguised.
    [RequireComponent(typeof(NavMeshAgent))]
    public class MimicController : MonoBehaviour
    {
        public enum MimicState { Disguised, Revealed, Hunting, Retreating }

        public static event Action OnMimicRevealed;
        public static event Action OnMimicRetreating;

        [SerializeField] MimicConfig config;
        [SerializeField] Prop disguisedProp;      // the designated fake copy (RunDirector may override)
        [SerializeField] Transform player;        // PlayerCapsule
        [SerializeField] GameObject body;         // child holding the visible mesh
        [SerializeField] Transform[] patrolPoints;
        [SerializeField] Transform reDisguisePoint;

        public MimicState State { get; private set; } = MimicState.Disguised;
        public string DisguisePropId => currentProp != null ? currentProp.name : "(unassigned)";
        public Vector3 DisguisePropPosition => currentProp != null ? currentProp.transform.position : Vector3.zero;

        NavMeshAgent agent;
        Prop currentProp;
        PlayerHideState playerHide;
        bool propAssignedExternally;
        float stateTimer;
        int patrolIndex;
        bool pursuing;
        bool playerWasHidden;
        bool hidingIgnored;

        void Awake()
        {
            agent = GetComponent<NavMeshAgent>();
            if (player != null)
                playerHide = player.GetComponentInChildren<PlayerHideState>(); // optional until Task 5 is wired

            if (config == null)
                Debug.LogError("[Mimic] Config is not assigned. Assign a MimicConfig asset in the Inspector.", this);
            if (player == null)
                Debug.LogError("[Mimic] Player is not assigned. Assign the PlayerCapsule transform in the Inspector.", this);
            if (body == null)
                Debug.LogError("[Mimic] Body is not assigned. Assign the child GameObject with the visible mesh.", this);
            if (reDisguisePoint == null)
                Debug.LogError("[Mimic] Re-Disguise Point is not assigned. Assign an empty Transform in the Inspector.", this);
        }

        void Start()
        {
            agent.enabled = false;
            if (body != null)
                body.SetActive(false);

            if (!propAssignedExternally)
            {
                if (disguisedProp != null)
                    AssignFakeProp(disguisedProp);
                else
                    Debug.LogError("[Mimic] Disguised Prop is not assigned. Assign the fake-copy Prop in the Inspector.", this);
            }
        }

        void OnDestroy()
        {
            if (currentProp != null)
                currentProp.Interacted -= HandleFakeInteracted;
        }

        // RunDirector calls this at run start; the Inspector value is only the fallback.
        public void AssignFakeProp(Prop fake)
        {
            if (fake == null)
                return;
            if (currentProp != null)
                currentProp.Interacted -= HandleFakeInteracted;

            currentProp = fake;
            currentProp.Interacted += HandleFakeInteracted;
            propAssignedExternally = true;
        }

        void HandleFakeInteracted(Prop _)
        {
            if (State != MimicState.Disguised)
                return;
            if (GameManager.Instance != null && GameManager.Instance.State != GameManager.RunState.Playing)
                return;
            EnterRevealed();
        }

        void Update()
        {
            if (config == null || player == null)
                return;
            if (GameManager.Instance != null && GameManager.Instance.State != GameManager.RunState.Playing)
                return;

            switch (State)
            {
                case MimicState.Revealed:
                    stateTimer -= Time.deltaTime;
                    if (stateTimer <= 0f)
                        EnterHunting();
                    break;

                case MimicState.Hunting:
                    UpdateHunting();
                    break;

                case MimicState.Retreating:
                    if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance + 0.2f)
                        EnterDisguised();
                    break;
            }
        }

        void EnterRevealed()
        {
            State = MimicState.Revealed;
            stateTimer = config.revealPauseSeconds;

            Vector3 appearAt = currentProp.transform.position;
            currentProp.gameObject.SetActive(false);

            if (NavMesh.SamplePosition(appearAt, out NavMeshHit navHit, 2f, NavMesh.AllAreas))
                transform.position = navHit.position;
            else
                Debug.LogWarning("[Mimic] No NavMesh near the fake prop — appearing at my parked position instead.", this);

            if (body != null)
                body.SetActive(true);
            agent.enabled = true;
            agent.Warp(transform.position);
            agent.isStopped = true;

            Debug.Log($"[Mimic] REVEALED — the fake was '{currentProp.name}'.", this);
            OnMimicRevealed?.Invoke();
        }

        void EnterHunting()
        {
            State = MimicState.Hunting;
            stateTimer = config.huntDuration;
            pursuing = false;
            playerWasHidden = playerHide != null && playerHide.IsHidden;
            hidingIgnored = false;
            agent.isStopped = false;

            if (patrolPoints == null || patrolPoints.Length == 0)
                Debug.LogWarning("[Mimic] No patrol points assigned — the Mimic will stand still unless it sees you.", this);
            else
                GoToNextPatrolPoint();
        }

        void UpdateHunting()
        {
            bool rawVisible = CanSeePlayer();
            bool hidden = playerHide != null && playerHide.IsHidden;

            // Simplest correct rule: diving into a spot while the Mimic is pursuing WITH eyes on you
            // doesn't save you — the spot only starts working once it has lost sight of you.
            if (hidden && !playerWasHidden && pursuing && rawVisible)
                hidingIgnored = true;
            if (!rawVisible || !hidden)
                hidingIgnored = false;
            playerWasHidden = hidden;

            bool canSee = rawVisible && (!hidden || hidingIgnored);

            if (canSee)
            {
                pursuing = true;
                stateTimer = config.huntDuration; // seeing you keeps the hunt alive
                agent.speed = config.pursueSpeed;
                agent.SetDestination(player.position);
            }
            else
            {
                pursuing = false;
                agent.speed = config.patrolSpeed;
                if (patrolPoints != null && patrolPoints.Length > 0 &&
                    !agent.pathPending && agent.remainingDistance <= agent.stoppingDistance + 0.2f)
                    GoToNextPatrolPoint();
            }

            if (Vector3.Distance(transform.position, player.position) <= config.killDistance)
            {
                Debug.Log("[Mimic] Caught the player.", this);
                if (GameManager.Instance != null)
                    GameManager.Instance.PlayerDied();
                return;
            }

            stateTimer -= Time.deltaTime;
            if (stateTimer <= 0f)
                EnterRetreating();
        }

        void EnterRetreating()
        {
            State = MimicState.Retreating;
            pursuing = false;
            agent.speed = config.retreatSpeed;
            if (reDisguisePoint != null)
                agent.SetDestination(reDisguisePoint.position);

            Debug.Log("[Mimic] Hunt over — retreating to re-disguise.", this);
            OnMimicRetreating?.Invoke();
        }

        void EnterDisguised()
        {
            State = MimicState.Disguised;
            agent.enabled = false;
            if (body != null)
                body.SetActive(false);

            if (currentProp != null)
            {
                currentProp.transform.position = reDisguisePoint != null ? reDisguisePoint.position : currentProp.HomePosition;
                currentProp.gameObject.SetActive(true);
            }

            Debug.Log($"[Mimic] Re-disguised as '{DisguisePropId}' — interact with it to reveal again.", this);
        }

        bool CanSeePlayer()
        {
            Vector3 eye = transform.position + Vector3.up * config.eyeHeight;
            Vector3 target = player.position + Vector3.up * 1f; // chest height
            Vector3 toPlayer = target - eye;

            if (toPlayer.magnitude > config.viewRange)
                return false;

            Vector3 flat = new Vector3(toPlayer.x, 0f, toPlayer.z);
            if (Vector3.Angle(transform.forward, flat) > config.viewAngle * 0.5f)
                return false;

            // Blocked line of sight = anything hit before the player.
            if (Physics.Raycast(eye, toPlayer.normalized, out RaycastHit hit, toPlayer.magnitude, config.lineOfSightMask, QueryTriggerInteraction.Ignore))
                return hit.transform == player || hit.transform.IsChildOf(player);

            return true;
        }

        void GoToNextPatrolPoint()
        {
            patrolIndex = (patrolIndex + 1) % patrolPoints.Length;
            if (patrolPoints[patrolIndex] != null)
                agent.SetDestination(patrolPoints[patrolIndex].position);
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ResetStaticEvents()
        {
            OnMimicRevealed = null;
            OnMimicRetreating = null;
        }
    }
}
