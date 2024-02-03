﻿using GameNetcodeStuff;
using System;
using System.Collections;
using Unity.Netcode;
using UnityEngine;

namespace LethalWorkingConditions.Behaviours
{
    public class CrawlerAI : EnemyAI
    {
        public AISearchRoutine searchForPlayers;

        private float checkLineOfSightInterval;

        public float maxSearchAndRoamRadius = 100f;

        [Space(5f)]
        public float noticePlayerTimer;

        private bool hasEnteredChaseMode;

        private bool lostPlayerInChase;

        private bool beginningChasingThisClient;

        private Collider[] nearPlayerColliders;

        public AudioClip shortRoar;

        public AudioClip[] hitWallSFX;

        public AudioClip bitePlayerSFX;

        private Vector3 previousPosition;

        private float previousVelocity;

        private float averageVelocity;

        private float velocityInterval;

        private float velocityAverageCount;

        private float wallCollisionSFXDebounce;

        private float timeSinceHittingPlayer;

        private bool ateTargetPlayerBody;

        private Coroutine eatPlayerBodyCoroutine;

        public Transform mouthTarget;

        public AudioClip eatPlayerSFX;

        public AudioClip[] hitCrawlerSFX;

        public AudioClip[] longRoarSFX;

        public DeadBodyInfo currentlyHeldBody;

        private bool pullingSecondLimb;

        private float agentSpeedWithNegative;

        private Vector3 lastPositionOfSeenPlayer;

        [Space(5f)]
        public float BaseAcceleration = 55f;

        public float SpeedAccelerationEffect = 2f;

        public float SpeedIncreaseRate = 5f;

        private float lastTimeHit;

        public override void Start()
        {
            base.Start();
            nearPlayerColliders = new Collider[4];
        }

        public override void DoAIInterval()
        {
            base.DoAIInterval();
            if (StartOfRound.Instance.livingPlayers == 0 || isEnemyDead)
            {
                return;
            }
            switch (currentBehaviourStateIndex)
            {
                case 0:
                    if (!searchForPlayers.inProgress)
                    {
                        StartSearch(base.transform.position, searchForPlayers);
                        Debug.Log($"Crawler: Started new search; is searching?: {searchForPlayers.inProgress}");
                    }
                    break;
                case 1:
                    CheckForVeryClosePlayer();
                    if (lostPlayerInChase)
                    {
                        movingTowardsTargetPlayer = false;
                        if (!searchForPlayers.inProgress)
                        {
                            searchForPlayers.searchWidth = 30f;
                            StartSearch(lastPositionOfSeenPlayer, searchForPlayers);
                            Debug.Log("Crawler: Lost player in chase; beginning search where the player was last seen");
                        }
                    }
                    else if (searchForPlayers.inProgress)
                    {
                        StopSearch(searchForPlayers);
                        movingTowardsTargetPlayer = true;
                        Debug.Log("Crawler: Found player during chase; stopping search coroutine and moving after target player");
                    }
                    break;
            }
        }

        public override void FinishedCurrentSearchRoutine()
        {
            base.FinishedCurrentSearchRoutine();
            searchForPlayers.searchWidth = Mathf.Clamp(searchForPlayers.searchWidth + 10f, 1f, maxSearchAndRoamRadius);
        }

        public override void Update()
        {
            base.Update();
            if (isEnemyDead)
            {
                return;
            }
            if (!base.IsOwner)
            {
                inSpecialAnimation = false;
            }
            CalculateAgentSpeed();
            timeSinceHittingPlayer += Time.deltaTime;
            if (GameNetworkManager.Instance.localPlayerController.HasLineOfSightToPosition(base.transform.position + Vector3.up * 0.25f, 80f, 25, 5f))
            {
                if (currentBehaviourStateIndex == 1)
                {
                    GameNetworkManager.Instance.localPlayerController.IncreaseFearLevelOverTime(0.8f);
                }
                else
                {
                    GameNetworkManager.Instance.localPlayerController.IncreaseFearLevelOverTime(0.8f, 0.5f);
                }
            }
            switch (currentBehaviourStateIndex)
            {
                case 0:
                    {
                        if (hasEnteredChaseMode)
                        {
                            hasEnteredChaseMode = false;
                            searchForPlayers.searchWidth = 25f;
                            beginningChasingThisClient = false;
                            noticePlayerTimer = 0f;
                            useSecondaryAudiosOnAnimatedObjects = false;
                            openDoorSpeedMultiplier = 0.6f;
                            agent.stoppingDistance = 0f;
                            agent.speed = 7f;
                        }
                        if (checkLineOfSightInterval <= 0.05f)
                        {
                            checkLineOfSightInterval += Time.deltaTime;
                            break;
                        }
                        checkLineOfSightInterval = 0f;
                        PlayerControllerB playerControllerB3;
                        if (stunnedByPlayer != null)
                        {
                            playerControllerB3 = stunnedByPlayer;
                            noticePlayerTimer = 1f;
                        }
                        else
                        {
                            playerControllerB3 = CheckLineOfSightForPlayer(55f);
                        }
                        if (playerControllerB3 == GameNetworkManager.Instance.localPlayerController)
                        {
                            noticePlayerTimer = Mathf.Clamp(noticePlayerTimer + 0.05f, 0f, 10f);
                            if (noticePlayerTimer > 0.2f && !beginningChasingThisClient)
                            {
                                beginningChasingThisClient = true;
                                BeginChasingPlayerServerRpc((int)GameNetworkManager.Instance.localPlayerController.playerClientId);
                                ChangeOwnershipOfEnemy(playerControllerB3.actualClientId);
                                Debug.Log("Begin chasing on local client");
                            }
                        }
                        else
                        {
                            noticePlayerTimer -= Time.deltaTime;
                        }
                        break;
                    }
                case 1:
                    {
                        if (!hasEnteredChaseMode)
                        {
                            hasEnteredChaseMode = true;
                            lostPlayerInChase = false;
                            checkLineOfSightInterval = 0f;
                            noticePlayerTimer = 0f;
                            beginningChasingThisClient = false;
                            useSecondaryAudiosOnAnimatedObjects = true;
                            openDoorSpeedMultiplier = 1.5f;
                            agent.stoppingDistance = 0.5f;
                            agent.speed = 0f;
                        }
                        if (!base.IsOwner || stunNormalizedTimer > 0f)
                        {
                            break;
                        }
                        if (checkLineOfSightInterval <= 0.075f)
                        {
                            checkLineOfSightInterval += Time.deltaTime;
                            break;
                        }
                        checkLineOfSightInterval = 0f;
                        if (!ateTargetPlayerBody && targetPlayer != null && targetPlayer.deadBody != null && targetPlayer.deadBody.grabBodyObject != null && targetPlayer.deadBody.grabBodyObject.grabbableToEnemies && eatPlayerBodyCoroutine == null && Vector3.Distance(base.transform.position, targetPlayer.deadBody.bodyParts[0].transform.position) < 3.3f)
                        {
                            Debug.Log("Crawler: Eat player body start");
                            ateTargetPlayerBody = true;
                            inSpecialAnimation = true;
                            eatPlayerBodyCoroutine = StartCoroutine(EatPlayerBodyAnimation((int)targetPlayer.playerClientId));
                            EatPlayerBodyServerRpc((int)targetPlayer.playerClientId);
                        }
                        if (inSpecialAnimation)
                        {
                            break;
                        }
                        if (lostPlayerInChase)
                        {
                            PlayerControllerB playerControllerB = CheckLineOfSightForPlayer(55f);
                            if ((bool)playerControllerB)
                            {
                                noticePlayerTimer = 0f;
                                lostPlayerInChase = false;
                                MakeScreechNoiseServerRpc();
                                if (playerControllerB != targetPlayer)
                                {
                                    SetMovingTowardsTargetPlayer(playerControllerB);
                                    ateTargetPlayerBody = false;
                                    ChangeOwnershipOfEnemy(playerControllerB.actualClientId);
                                }
                            }
                            else
                            {
                                noticePlayerTimer -= 0.075f;
                                if (noticePlayerTimer < -15f)
                                {
                                    SwitchToBehaviourState(0);
                                }
                            }
                            break;
                        }
                        PlayerControllerB playerControllerB2 = CheckLineOfSightForPlayer(65f, 80);
                        if (playerControllerB2 != null)
                        {
                            noticePlayerTimer = 0f;
                            lastPositionOfSeenPlayer = playerControllerB2.transform.position;
                            if (playerControllerB2 != targetPlayer)
                            {
                                targetPlayer = playerControllerB2;
                                ateTargetPlayerBody = false;
                                ChangeOwnershipOfEnemy(targetPlayer.actualClientId);
                            }
                        }
                        else
                        {
                            noticePlayerTimer += 0.075f;
                            if (noticePlayerTimer > 1.8f)
                            {
                                lostPlayerInChase = true;
                            }
                        }
                        break;
                    }
            }
        }

        private void CalculateAgentSpeed()
        {
            if (stunNormalizedTimer >= 0f)
            {
                agent.speed = 0.1f;
                agent.acceleration = 200f;
                creatureAnimator.SetBool("stunned", value: true);
                return;
            }
            creatureAnimator.SetBool("stunned", value: false);
            creatureAnimator.SetFloat("speedMultiplier", Mathf.Clamp(averageVelocity / 12f * 2.5f, 0.1f, 6f));
            float num = (base.transform.position - previousPosition).magnitude / (Time.deltaTime / 1.4f);
            if (velocityInterval <= 0f)
            {
                previousVelocity = averageVelocity;
                velocityInterval = 0.05f;
                velocityAverageCount += 1f;
                if (velocityAverageCount > 5f)
                {
                    averageVelocity += (num - averageVelocity) / 3f;
                }
                else
                {
                    averageVelocity += num;
                    if (velocityAverageCount == 2f)
                    {
                        averageVelocity /= velocityAverageCount;
                    }
                }
            }
            else
            {
                velocityInterval -= Time.deltaTime;
            }
            if (base.IsOwner && averageVelocity - num > Mathf.Clamp(num * 0.17f, 2f, 100f) && num > 3f && currentBehaviourStateIndex == 1)
            {
                if (wallCollisionSFXDebounce > 0.5f)
                {
                    if (base.IsServer)
                    {
                        CollideWithWallServerRpc();
                    }
                    else
                    {
                        CollideWithWallClientRpc();
                    }
                }
                agentSpeedWithNegative *= 0.2f;
                wallCollisionSFXDebounce = 0f;
            }
            wallCollisionSFXDebounce += Time.deltaTime;
            previousPosition = base.transform.position;
            if (currentBehaviourStateIndex == 0)
            {
                agent.speed = 8f;
                agent.acceleration = 26f;
            }
            else if (currentBehaviourStateIndex == 1)
            {
                float num2 = SpeedIncreaseRate;
                if (Time.realtimeSinceStartup - lastTimeHit < 1f)
                {
                    num2 += 4.25f;
                }
                agentSpeedWithNegative += Time.deltaTime * num2;
                agent.speed = Mathf.Clamp(agentSpeedWithNegative, -3f, 16f);
                agent.acceleration = Mathf.Clamp(BaseAcceleration - averageVelocity * SpeedAccelerationEffect, 4f, 40f);
                if (agent.acceleration > 22f)
                {
                    agent.angularSpeed = 800f;
                    agent.acceleration += 20f;
                }
                else
                {
                    agent.angularSpeed = 230f;
                }
            }
        }

        [ServerRpc(RequireOwnership = false)]
        public void CollideWithWallServerRpc()
        {
            NetworkManager networkManager = base.NetworkManager;
            if ((object)networkManager != null && networkManager.IsListening)
            {
                if (__rpc_exec_stage != __RpcExecStage.Server && (networkManager.IsClient || networkManager.IsHost))
                {
                    ServerRpcParams serverRpcParams = default(ServerRpcParams);
                    FastBufferWriter bufferWriter = __beginSendServerRpc(3661877694u, serverRpcParams, RpcDelivery.Reliable);
                    __endSendServerRpc(ref bufferWriter, 3661877694u, serverRpcParams, RpcDelivery.Reliable);
                }
                if (__rpc_exec_stage == __RpcExecStage.Server && (networkManager.IsServer || networkManager.IsHost))
                {
                    CollideWithWallClientRpc();
                }
            }
        }

        [ClientRpc]
        public void CollideWithWallClientRpc()
        {
            NetworkManager networkManager = base.NetworkManager;
            if ((object)networkManager == null || !networkManager.IsListening)
            {
                return;
            }
            if (__rpc_exec_stage != __RpcExecStage.Client && (networkManager.IsServer || networkManager.IsHost))
            {
                ClientRpcParams clientRpcParams = default(ClientRpcParams);
                FastBufferWriter bufferWriter = __beginSendClientRpc(461029090u, clientRpcParams, RpcDelivery.Reliable);
                __endSendClientRpc(ref bufferWriter, 461029090u, clientRpcParams, RpcDelivery.Reliable);
            }
            if (__rpc_exec_stage == __RpcExecStage.Client && (networkManager.IsClient || networkManager.IsHost))
            {
                RoundManager.PlayRandomClip(creatureSFX, hitWallSFX);
                float num = Vector3.Distance(GameNetworkManager.Instance.localPlayerController.transform.position, base.transform.position);
                if (num < 15f)
                {
                    HUDManager.Instance.ShakeCamera(ScreenShakeType.Big);
                }
                else if (num < 24f)
                {
                    HUDManager.Instance.ShakeCamera(ScreenShakeType.Small);
                }
            }
        }

        private void CheckForVeryClosePlayer()
        {
            if (Physics.OverlapSphereNonAlloc(base.transform.position, 1.5f, nearPlayerColliders, 8, QueryTriggerInteraction.Ignore) > 0)
            {
                PlayerControllerB component = nearPlayerColliders[0].transform.GetComponent<PlayerControllerB>();
                if (component != null && component != targetPlayer && !Physics.Linecast(base.transform.position + Vector3.up * 0.3f, component.transform.position, StartOfRound.Instance.collidersAndRoomMask))
                {
                    targetPlayer = component;
                }
            }
        }

        [ServerRpc(RequireOwnership = false)]
        public void BeginChasingPlayerServerRpc(int playerObjectId)
        {
            NetworkManager networkManager = base.NetworkManager;
            if ((object)networkManager != null && networkManager.IsListening)
            {
                if (__rpc_exec_stage != __RpcExecStage.Server && (networkManager.IsClient || networkManager.IsHost))
                {
                    ServerRpcParams serverRpcParams = default(ServerRpcParams);
                    FastBufferWriter bufferWriter = __beginSendServerRpc(869452445u, serverRpcParams, RpcDelivery.Reliable);
                    BytePacker.WriteValueBitPacked(bufferWriter, playerObjectId);
                    __endSendServerRpc(ref bufferWriter, 869452445u, serverRpcParams, RpcDelivery.Reliable);
                }
                if (__rpc_exec_stage == __RpcExecStage.Server && (networkManager.IsServer || networkManager.IsHost))
                {
                    BeginChasingPlayerClientRpc(playerObjectId);
                }
            }
        }

        [ClientRpc]
        public void BeginChasingPlayerClientRpc(int playerObjectId)
        {
            NetworkManager networkManager = base.NetworkManager;
            if ((object)networkManager != null && networkManager.IsListening)
            {
                if (__rpc_exec_stage != __RpcExecStage.Client && (networkManager.IsServer || networkManager.IsHost))
                {
                    ClientRpcParams clientRpcParams = default(ClientRpcParams);
                    FastBufferWriter bufferWriter = __beginSendClientRpc(1964892800u, clientRpcParams, RpcDelivery.Reliable);
                    BytePacker.WriteValueBitPacked(bufferWriter, playerObjectId);
                    __endSendClientRpc(ref bufferWriter, 1964892800u, clientRpcParams, RpcDelivery.Reliable);
                }
                if (__rpc_exec_stage == __RpcExecStage.Client && (networkManager.IsClient || networkManager.IsHost))
                {
                    MakeScreech();
                    SwitchToBehaviourStateOnLocalClient(1);
                    SetMovingTowardsTargetPlayer(StartOfRound.Instance.allPlayerScripts[playerObjectId]);
                }
            }
        }

        [ServerRpc(RequireOwnership = false)]
        public void MakeScreechNoiseServerRpc()
        {
            NetworkManager networkManager = base.NetworkManager;
            if ((object)networkManager != null && networkManager.IsListening)
            {
                if (__rpc_exec_stage != __RpcExecStage.Server && (networkManager.IsClient || networkManager.IsHost))
                {
                    ServerRpcParams serverRpcParams = default(ServerRpcParams);
                    FastBufferWriter bufferWriter = __beginSendServerRpc(2716706397u, serverRpcParams, RpcDelivery.Reliable);
                    __endSendServerRpc(ref bufferWriter, 2716706397u, serverRpcParams, RpcDelivery.Reliable);
                }
                if (__rpc_exec_stage == __RpcExecStage.Server && (networkManager.IsServer || networkManager.IsHost))
                {
                    MakeScreechNoiseClientRpc();
                }
            }
        }

        [ClientRpc]
        public void MakeScreechNoiseClientRpc()
        {
            NetworkManager networkManager = base.NetworkManager;
            if ((object)networkManager != null && networkManager.IsListening)
            {
                if (__rpc_exec_stage != __RpcExecStage.Client && (networkManager.IsServer || networkManager.IsHost))
                {
                    ClientRpcParams clientRpcParams = default(ClientRpcParams);
                    FastBufferWriter bufferWriter = __beginSendClientRpc(3572529702u, clientRpcParams, RpcDelivery.Reliable);
                    __endSendClientRpc(ref bufferWriter, 3572529702u, clientRpcParams, RpcDelivery.Reliable);
                }
                if (__rpc_exec_stage == __RpcExecStage.Client && (networkManager.IsClient || networkManager.IsHost))
                {
                    MakeScreech();
                }
            }
        }

        private void MakeScreech()
        {
            int num = UnityEngine.Random.Range(0, longRoarSFX.Length);
            creatureVoice.PlayOneShot(longRoarSFX[num]);
            WalkieTalkie.TransmitOneShotAudio(creatureVoice, longRoarSFX[num]);
            if (Vector3.Distance(GameNetworkManager.Instance.localPlayerController.transform.position, base.transform.position) < 15f)
            {
                GameNetworkManager.Instance.localPlayerController.JumpToFearLevel(0.75f);
            }
        }

        public override void OnCollideWithPlayer(Collider other)
        {
            base.OnCollideWithPlayer(other);
            if (!(timeSinceHittingPlayer < 0.65f))
            {
                PlayerControllerB playerControllerB = MeetsStandardPlayerCollisionConditions(other);
                if (playerControllerB != null)
                {
                    timeSinceHittingPlayer = 0f;
                    playerControllerB.DamagePlayer(40, hasDamageSFX: true, callRPC: true, CauseOfDeath.Mauling);
                    agent.speed = 0f;
                    HitPlayerServerRpc((int)GameNetworkManager.Instance.localPlayerController.playerClientId);
                    GameNetworkManager.Instance.localPlayerController.JumpToFearLevel(1f);
                }
            }
        }

        [ServerRpc(RequireOwnership = false)]
        public void HitPlayerServerRpc(int playerId)
        {
            NetworkManager networkManager = base.NetworkManager;
            if ((object)networkManager != null && networkManager.IsListening)
            {
                if (__rpc_exec_stage != __RpcExecStage.Server && (networkManager.IsClient || networkManager.IsHost))
                {
                    ServerRpcParams serverRpcParams = default(ServerRpcParams);
                    FastBufferWriter bufferWriter = __beginSendServerRpc(3352518565u, serverRpcParams, RpcDelivery.Reliable);
                    BytePacker.WriteValueBitPacked(bufferWriter, playerId);
                    __endSendServerRpc(ref bufferWriter, 3352518565u, serverRpcParams, RpcDelivery.Reliable);
                }
                if (__rpc_exec_stage == __RpcExecStage.Server && (networkManager.IsServer || networkManager.IsHost))
                {
                    HitPlayerClientRpc(playerId);
                }
            }
        }

        [ClientRpc]
        public void HitPlayerClientRpc(int playerId)
        {
            NetworkManager networkManager = base.NetworkManager;
            if ((object)networkManager == null || !networkManager.IsListening)
            {
                return;
            }
            if (__rpc_exec_stage != __RpcExecStage.Client && (networkManager.IsServer || networkManager.IsHost))
            {
                ClientRpcParams clientRpcParams = default(ClientRpcParams);
                FastBufferWriter bufferWriter = __beginSendClientRpc(880045462u, clientRpcParams, RpcDelivery.Reliable);
                BytePacker.WriteValueBitPacked(bufferWriter, playerId);
                __endSendClientRpc(ref bufferWriter, 880045462u, clientRpcParams, RpcDelivery.Reliable);
            }
            if (__rpc_exec_stage == __RpcExecStage.Client && (networkManager.IsClient || networkManager.IsHost))
            {
                if (!inSpecialAnimation)
                {
                    creatureAnimator.SetTrigger("HitPlayer");
                }
                creatureVoice.PlayOneShot(bitePlayerSFX);
                agentSpeedWithNegative = UnityEngine.Random.Range(-2f, 0.25f);
            }
        }

        [ServerRpc(RequireOwnership = false)]
        public void EatPlayerBodyServerRpc(int playerId)
        {
            NetworkManager networkManager = base.NetworkManager;
            if ((object)networkManager != null && networkManager.IsListening)
            {
                if (__rpc_exec_stage != __RpcExecStage.Server && (networkManager.IsClient || networkManager.IsHost))
                {
                    ServerRpcParams serverRpcParams = default(ServerRpcParams);
                    FastBufferWriter bufferWriter = __beginSendServerRpc(3781293737u, serverRpcParams, RpcDelivery.Reliable);
                    BytePacker.WriteValueBitPacked(bufferWriter, playerId);
                    __endSendServerRpc(ref bufferWriter, 3781293737u, serverRpcParams, RpcDelivery.Reliable);
                }
                if (__rpc_exec_stage == __RpcExecStage.Server && (networkManager.IsServer || networkManager.IsHost))
                {
                    EatPlayerBodyClientRpc(playerId);
                }
            }
        }

        [ClientRpc]
        public void EatPlayerBodyClientRpc(int playerId)
        {
            NetworkManager networkManager = base.NetworkManager;
            if ((object)networkManager != null && networkManager.IsListening)
            {
                if (__rpc_exec_stage != __RpcExecStage.Client && (networkManager.IsServer || networkManager.IsHost))
                {
                    ClientRpcParams clientRpcParams = default(ClientRpcParams);
                    FastBufferWriter bufferWriter = __beginSendClientRpc(2460625110u, clientRpcParams, RpcDelivery.Reliable);
                    BytePacker.WriteValueBitPacked(bufferWriter, playerId);
                    __endSendClientRpc(ref bufferWriter, 2460625110u, clientRpcParams, RpcDelivery.Reliable);
                }
                if (__rpc_exec_stage == __RpcExecStage.Client && (networkManager.IsClient || networkManager.IsHost) && !base.IsOwner && eatPlayerBodyCoroutine == null)
                {
                    StartCoroutine(EatPlayerBodyAnimation(playerId));
                }
            }
        }

        private IEnumerator EatPlayerBodyAnimation(int playerId)
        {
            PlayerControllerB playerScript = StartOfRound.Instance.allPlayerScripts[playerId];
            float startTime = Time.realtimeSinceStartup;
            yield return new WaitUntil(() => (playerScript.deadBody != null && playerScript.deadBody.grabBodyObject != null) || Time.realtimeSinceStartup - startTime > 2f);
            DeadBodyInfo deadBody = null;
            if (StartOfRound.Instance.allPlayerScripts[playerId].deadBody != null)
            {
                if (debugEnemyAI)
                {
                    Debug.Log("Thumper: Body is not null!");
                }
                deadBody = StartOfRound.Instance.allPlayerScripts[playerId].deadBody;
            }
            yield return null;
            if (debugEnemyAI)
            {
                Debug.Log($"{deadBody != null}; {deadBody.grabBodyObject != null}; {!deadBody.isInShip}; {!deadBody.grabBodyObject.isHeld}; {Vector3.Distance(base.transform.position, deadBody.bodyParts[0].transform.position)}");
            }
            if (deadBody != null && deadBody.grabBodyObject != null && !deadBody.isInShip && !deadBody.grabBodyObject.isHeld && !isEnemyDead && Vector3.Distance(base.transform.position, deadBody.bodyParts[0].transform.position) < 6.7f)
            {
                creatureAnimator.SetTrigger("EatPlayer");
                creatureVoice.pitch = UnityEngine.Random.Range(0.85f, 1.1f);
                creatureVoice.PlayOneShot(eatPlayerSFX);
                deadBody.canBeGrabbedBackByPlayers = false;
                currentlyHeldBody = deadBody;
                pullingSecondLimb = deadBody.attachedTo != null;
                if (pullingSecondLimb)
                {
                    deadBody.secondaryAttachedLimb = deadBody.bodyParts[3];
                    deadBody.secondaryAttachedTo = mouthTarget;
                }
                else
                {
                    deadBody.attachedLimb = deadBody.bodyParts[0];
                    deadBody.attachedTo = mouthTarget;
                }
                yield return new WaitForSeconds(2.75f);
            }
            Debug.Log("Crawler: leaving special animation");
            inSpecialAnimation = false;
            DropPlayerBody();
            eatPlayerBodyCoroutine = null;
        }

        private void DropPlayerBody()
        {
            if (currentlyHeldBody != null)
            {
                if (pullingSecondLimb)
                {
                    currentlyHeldBody.secondaryAttachedLimb = null;
                    currentlyHeldBody.secondaryAttachedTo = null;
                }
                else
                {
                    currentlyHeldBody.attachedLimb = null;
                    currentlyHeldBody.attachedTo = null;
                }
            }
        }

        public override void KillEnemy(bool destroy = false)
        {
            base.KillEnemy();
            if (eatPlayerBodyCoroutine != null)
            {
                StopCoroutine(eatPlayerBodyCoroutine);
            }
            DropPlayerBody();
        }

        public override void HitEnemy(int force = 1, PlayerControllerB playerWhoHit = null, bool playHitSFX = false)
        {
            base.HitEnemy(force, playerWhoHit);
            if (!isEnemyDead)
            {
                agent.speed = 2f;
                if (!inSpecialAnimation)
                {
                    creatureAnimator.SetTrigger("HurtEnemy");
                }
                enemyHP -= force;
                agentSpeedWithNegative = UnityEngine.Random.Range(-2.8f, -2f);
                lastTimeHit = Time.realtimeSinceStartup;
                averageVelocity = 0f;
                RoundManager.PlayRandomClip(creatureVoice, hitCrawlerSFX);
                if (enemyHP <= 0 && base.IsOwner)
                {
                    KillEnemyOnOwnerClient();
                }
            }
        }

        protected override void __initializeVariables()
        {
            base.__initializeVariables();
        }

        [RuntimeInitializeOnLoadMethod]
        internal static void InitializeRPCS_CrawlerAI()
        {
            NetworkManager.__rpc_func_table.Add(3661877694u, __rpc_handler_3661877694);
            NetworkManager.__rpc_func_table.Add(461029090u, __rpc_handler_461029090);
            NetworkManager.__rpc_func_table.Add(869452445u, __rpc_handler_869452445);
            NetworkManager.__rpc_func_table.Add(1964892800u, __rpc_handler_1964892800);
            NetworkManager.__rpc_func_table.Add(2716706397u, __rpc_handler_2716706397);
            NetworkManager.__rpc_func_table.Add(3572529702u, __rpc_handler_3572529702);
            NetworkManager.__rpc_func_table.Add(3352518565u, __rpc_handler_3352518565);
            NetworkManager.__rpc_func_table.Add(880045462u, __rpc_handler_880045462);
            NetworkManager.__rpc_func_table.Add(3781293737u, __rpc_handler_3781293737);
            NetworkManager.__rpc_func_table.Add(2460625110u, __rpc_handler_2460625110);
        }

        private static void __rpc_handler_3661877694(NetworkBehaviour target, FastBufferReader reader, __RpcParams rpcParams)
        {
            NetworkManager networkManager = target.NetworkManager;
            if ((object)networkManager != null && networkManager.IsListening)
            {
                target.__rpc_exec_stage = __RpcExecStage.Server;
                ((CrawlerAI)target).CollideWithWallServerRpc();
                target.__rpc_exec_stage = __RpcExecStage.None;
            }
        }

        private static void __rpc_handler_461029090(NetworkBehaviour target, FastBufferReader reader, __RpcParams rpcParams)
        {
            NetworkManager networkManager = target.NetworkManager;
            if ((object)networkManager != null && networkManager.IsListening)
            {
                target.__rpc_exec_stage = __RpcExecStage.Client;
                ((CrawlerAI)target).CollideWithWallClientRpc();
                target.__rpc_exec_stage = __RpcExecStage.None;
            }
        }

        private static void __rpc_handler_869452445(NetworkBehaviour target, FastBufferReader reader, __RpcParams rpcParams)
        {
            NetworkManager networkManager = target.NetworkManager;
            if ((object)networkManager != null && networkManager.IsListening)
            {
                ByteUnpacker.ReadValueBitPacked(reader, out int value);
                target.__rpc_exec_stage = __RpcExecStage.Server;
                ((CrawlerAI)target).BeginChasingPlayerServerRpc(value);
                target.__rpc_exec_stage = __RpcExecStage.None;
            }
        }

        private static void __rpc_handler_1964892800(NetworkBehaviour target, FastBufferReader reader, __RpcParams rpcParams)
        {
            NetworkManager networkManager = target.NetworkManager;
            if ((object)networkManager != null && networkManager.IsListening)
            {
                ByteUnpacker.ReadValueBitPacked(reader, out int value);
                target.__rpc_exec_stage = __RpcExecStage.Client;
                ((CrawlerAI)target).BeginChasingPlayerClientRpc(value);
                target.__rpc_exec_stage = __RpcExecStage.None;
            }
        }

        private static void __rpc_handler_2716706397(NetworkBehaviour target, FastBufferReader reader, __RpcParams rpcParams)
        {
            NetworkManager networkManager = target.NetworkManager;
            if ((object)networkManager != null && networkManager.IsListening)
            {
                target.__rpc_exec_stage = __RpcExecStage.Server;
                ((CrawlerAI)target).MakeScreechNoiseServerRpc();
                target.__rpc_exec_stage = __RpcExecStage.None;
            }
        }

        private static void __rpc_handler_3572529702(NetworkBehaviour target, FastBufferReader reader, __RpcParams rpcParams)
        {
            NetworkManager networkManager = target.NetworkManager;
            if ((object)networkManager != null && networkManager.IsListening)
            {
                target.__rpc_exec_stage = __RpcExecStage.Client;
                ((CrawlerAI)target).MakeScreechNoiseClientRpc();
                target.__rpc_exec_stage = __RpcExecStage.None;
            }
        }

        private static void __rpc_handler_3352518565(NetworkBehaviour target, FastBufferReader reader, __RpcParams rpcParams)
        {
            NetworkManager networkManager = target.NetworkManager;
            if ((object)networkManager != null && networkManager.IsListening)
            {
                ByteUnpacker.ReadValueBitPacked(reader, out int value);
                target.__rpc_exec_stage = __RpcExecStage.Server;
                ((CrawlerAI)target).HitPlayerServerRpc(value);
                target.__rpc_exec_stage = __RpcExecStage.None;
            }
        }

        private static void __rpc_handler_880045462(NetworkBehaviour target, FastBufferReader reader, __RpcParams rpcParams)
        {
            NetworkManager networkManager = target.NetworkManager;
            if ((object)networkManager != null && networkManager.IsListening)
            {
                ByteUnpacker.ReadValueBitPacked(reader, out int value);
                target.__rpc_exec_stage = __RpcExecStage.Client;
                ((CrawlerAI)target).HitPlayerClientRpc(value);
                target.__rpc_exec_stage = __RpcExecStage.None;
            }
        }

        private static void __rpc_handler_3781293737(NetworkBehaviour target, FastBufferReader reader, __RpcParams rpcParams)
        {
            NetworkManager networkManager = target.NetworkManager;
            if ((object)networkManager != null && networkManager.IsListening)
            {
                ByteUnpacker.ReadValueBitPacked(reader, out int value);
                target.__rpc_exec_stage = __RpcExecStage.Server;
                ((CrawlerAI)target).EatPlayerBodyServerRpc(value);
                target.__rpc_exec_stage = __RpcExecStage.None;
            }
        }

        private static void __rpc_handler_2460625110(NetworkBehaviour target, FastBufferReader reader, __RpcParams rpcParams)
        {
            NetworkManager networkManager = target.NetworkManager;
            if ((object)networkManager != null && networkManager.IsListening)
            {
                ByteUnpacker.ReadValueBitPacked(reader, out int value);
                target.__rpc_exec_stage = __RpcExecStage.Client;
                ((CrawlerAI)target).EatPlayerBodyClientRpc(value);
                target.__rpc_exec_stage = __RpcExecStage.None;
            }
        }

        protected internal override string __getTypeName()
        {
            return "CrawlerAI";
        }
    }

}