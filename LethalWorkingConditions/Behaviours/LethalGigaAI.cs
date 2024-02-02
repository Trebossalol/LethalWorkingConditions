using UnityEngine;
using GameNetcodeStuff;
using System.Collections;
using Unity.Netcode;
using LethalWorkingConditions.Helpers;
using System;
using System.Collections.Generic;

namespace LethalWorkingConditions.MonoBehaviours
{
    public class LethalGigaAIOld : EnemyAI
    {
        public Transform turnCompass;

        // Where he can attack (gets scaled up)
        public Transform attackArea;

        // Scouting Routine
        public AISearchRoutine scoutingSearchRoutine;

        float timeSinceHittingLocalPlayer;

        float timeSinceNewRandPos;

        Vector3 positionRandomness;

        Vector3 StalkPos;

        System.Random enemyRandom;

        bool isDeadAnimationDone;

        enum LGBehaviour
        {
            SearchingForPlayer,
            StickingInFrontOfPlayer,
            HeadSwingAttackInProgress,
            LethalScream
        }

        private LWCLogger logger = new LWCLogger("LethalGigaAI");

        public override void Start()
        {
            base.Start();

            logger.LogInfo("LethalGiga spawned");

            timeSinceHittingLocalPlayer = 0;

            //creatureAnimator.SetTrigger("walk");

            timeSinceNewRandPos = 0;

            positionRandomness = new Vector3(0, 0, 0);

            enemyRandom = new System.Random(StartOfRound.Instance.randomMapSeed + thisEnemyIndex);

            isDeadAnimationDone = false;

            currentBehaviourStateIndex = (int)LGBehaviour.SearchingForPlayer;
        }

        public override void Update()
        {
            base.Update();

            // Handle death
            if (isEnemyDead)
            {
                // For some weird reason I can't get an RPC to get called from HitEnemy() (works from other methods), so we do this workaround. We just want the enemy to stop playing the song.
                if (!isDeadAnimationDone)
                {
                    isDeadAnimationDone = true;
                    creatureVoice.Stop();
                    creatureVoice.PlayOneShot(dieSFX);
                }
                return;
            }
            
            timeSinceHittingLocalPlayer += Time.deltaTime;
            
            timeSinceNewRandPos += Time.deltaTime;
            
            if (targetPlayer != null && PlayerIsTargetable(targetPlayer) && !scoutingSearchRoutine.inProgress)
            {
                turnCompass.LookAt(targetPlayer.gameplayCamera.transform.position);
                transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.Euler(new Vector3(0f, turnCompass.eulerAngles.y, 0f)), 4f * Time.deltaTime);
            }
            
            if (stunNormalizedTimer > 0f)
            {
                agent.speed = 0f;
            }
        }

        public override void DoAIInterval()
        {
            base.DoAIInterval();

            // If dead or players dead
            if (isEnemyDead || StartOfRound.Instance.allPlayersDead) return;
            
            // Sets scoutingSearchRoutine.inProgress to True if searching, False if found player
            // Will set targetPlayer to the closest player
            KeepSearchingForPlayerUnlessInRange(25, ref scoutingSearchRoutine);

            switch (currentBehaviourStateIndex)
            {
                case (int)LGBehaviour.SearchingForPlayer:
                    agent.speed = 3f;
                    break;
                case (int)LGBehaviour.StickingInFrontOfPlayer:
                    agent.speed = 5f;
                    StickingInFrontOfPlayer();
                    break;
                case (int)LGBehaviour.HeadSwingAttackInProgress:
                    // We don't care about doing anything here
                    break;
                case (int)LGBehaviour.LethalScream:
                    agent.speed = 0f;
                    PrepareScream();
                    break;
                default:
                    logger.LogInfo("This Behavior State doesn't exist!");
                    break;
            }
        }

        void KeepSearchingForPlayerUnlessInRange(float range, ref AISearchRoutine routine)
        {
            TargetClosestPlayer();

            // has target and is in range
            if (targetPlayer != null && Vector3.Distance(transform.position, targetPlayer.transform.position) <= range)
            {
                // if routine is in progress
                if (routine.inProgress)
                {
                    int rng = enemyRandom.Next(0, 10);

                    if (rng >= 5)
                    {
                        logger.LogInfo("");
                        StopSearch(routine);
                        SwitchToBehaviourClientRpc((int)LGBehaviour.LethalScream);
                    }
                    else
                    {
                        logger.LogInfo("SwitchBehaviour to Sticking");
                        StopSearch(routine);
                        SwitchToBehaviourClientRpc((int)LGBehaviour.StickingInFrontOfPlayer);
                    }
                }
            }
            else
            {
                if (!routine.inProgress)
                {
                    logger.LogInfo("Stop Target Player");
                    StartSearch(transform.position, routine);
                    SwitchToBehaviourClientRpc((int)LGBehaviour.SearchingForPlayer);
                }
            }
        }

        void StickingInFrontOfPlayer()
        {
            // We only run this method for the host because I'm paranoid about randomness not syncing I guess
            // This is fine because the game does sync the position of the enemy.
            // Also the attack is a ClientRpc so it should always sync
            if (targetPlayer == null || !IsOwner)
            {
                return;
            }

            if (timeSinceNewRandPos > 0.7f)
            {
                timeSinceNewRandPos = 0;
                if (enemyRandom.Next(0, 5) == 0)
                {
                    // Attack
                    StartCoroutine(SwingAttack());
                }
                else
                {
                    // Go in front of player
                    positionRandomness = new Vector3(enemyRandom.Next(-2, 2), 0, enemyRandom.Next(-2, 2));
                    StalkPos = targetPlayer.transform.position - Vector3.Scale(new Vector3(-5, 0, -5), targetPlayer.transform.forward) + positionRandomness;
                }
                SetDestinationToPosition(StalkPos);
            }
        }

        void PrepareScream()
        {
            StartCoroutine((IEnumerator)LethalScreamAttack());
        }

        IEnumerator SwingAttack()
        {
            SwitchToBehaviourClientRpc((int)LGBehaviour.HeadSwingAttackInProgress);
            StalkPos = targetPlayer.transform.position;
            SetDestinationToPosition(StalkPos);
            yield return new WaitForSeconds(0.5f);
            if (isEnemyDead)
            {
                yield break;
            }
            DoAnimationClientRpc("swingAttack");
            yield return new WaitForSeconds(0.24f);
            SwingAttackHitClientRpc();

            // In case the player has already gone away, we just yield break (basically same as return, but for IEnumerator)
            if (currentBehaviourStateIndex != (int)LGBehaviour.HeadSwingAttackInProgress)
            {
                yield break;
            }

            SwitchToBehaviourClientRpc((int)LGBehaviour.StickingInFrontOfPlayer);
        }

        IEnumerable LethalScreamAttack()
        {
            SwitchToBehaviourClientRpc((int)LGBehaviour.LethalScream);

            DoAnimationClientRpc("lethalScream");

            yield return new WaitForSeconds(4f);

            LethalScreamAttackClientRpc();

            if (currentBehaviourStateIndex != (int)LGBehaviour.LethalScream) yield break;

            SwitchToBehaviourClientRpc((int)LGBehaviour.SearchingForPlayer);
        }

        public override void OnCollideWithPlayer(Collider other)
        {
            if (timeSinceHittingLocalPlayer < 1f)
            {
                return;
            }
            PlayerControllerB playerControllerB = MeetsStandardPlayerCollisionConditions(other);
            if (playerControllerB != null)
            {
                logger.LogInfo("LethalGiga Collision with Player!");
                timeSinceHittingLocalPlayer = 0f;
                playerControllerB.DamagePlayer(20);
            }
        }

        public override void HitEnemy(int force = 1, PlayerControllerB playerWhoHit = null, bool playHitSFX = false)
        {
            base.HitEnemy(force, playerWhoHit, playHitSFX);
            if (isEnemyDead)
            {
                return;
            }
            enemyHP -= force;
            if (IsOwner)
            {
                if (enemyHP <= 0 && !isEnemyDead)
                {
                    // Our death sound will be played through creatureVoice when KillEnemy() is called.
                    // KillEnemy() will also attempt to call creatureAnimator.SetTrigger("KillEnemy"),
                    // so we don't need to call a death animation ourselves.
                    StopCoroutine(SwingAttack());
                    KillEnemyOnOwnerClient();
                }
            }
        }

        [ClientRpc]
        public void DoAnimationClientRpc(string animationName)
        {
            logger.LogInfo($"Animation: {animationName}");
            creatureAnimator.SetTrigger(animationName);
        }

        [ClientRpc]
        public void SwingAttackHitClientRpc()
        {
            logger.LogInfo("SwingAttackHitClientRPC");
            int playerLayer = 1 << 3; // This can be found from the game's Asset Ripper output in Unity

            Collider[] hitColliders = Physics.OverlapBox(attackArea.position, attackArea.localScale, Quaternion.identity, playerLayer);

            if (hitColliders.Length > 0)
            {
                foreach (var player in hitColliders)
                {
                    PlayerControllerB playerControllerB = MeetsStandardPlayerCollisionConditions(player);
                    if (playerControllerB != null)
                    {
                        logger.LogInfo("Swing attack hit player!");
                        timeSinceHittingLocalPlayer = 0f;
                        playerControllerB.DamagePlayer(40);
                    }
                }
            }
        }

        [ClientRpc]
        private void LethalScreamAttackClientRpc()
        {
            logger.LogInfo("LethalScreamAttackClientRpc");

            int playerLayer = 1 << 3; // This can be found from the game's Asset Ripper output in Unity

            Collider[] hitColliders = Physics.OverlapBox(attackArea.position, attackArea.localScale, Quaternion.identity, playerLayer);

            if (hitColliders.Length > 0)
            {
                foreach (var player in hitColliders)
                {
                    PlayerControllerB playerControllerB = MeetsStandardPlayerCollisionConditions(player);
                    if (playerControllerB != null)
                    {
                        logger.LogInfo("Lethal scream attack player!");
                        timeSinceHittingLocalPlayer = 0f;
                        playerControllerB.DamagePlayer(5);
                        playerControllerB.IncreaseFearLevelOverTime(10, 10);
                    }
                }
            }

            Turret[] turrets = UnityEngine.Object.FindObjectsOfType<Turret>();

            foreach (var turret in turrets)
            {
                turret.EnterBerserkModeServerRpc(-1);
            }
        }
    }

    public class LethalGigaAI : EnemyAI
    {
        // Routines
        public AISearchRoutine searchForPlayers;

        // Properties
        public Transform turnCompass;
        public Transform attackArea;
        public Transform mouthTarget;

        // Agent
        private bool beginningChasingThisClient;
        private float checkLineOfSightInterval;
        private float noticePlayerTimer;
        private Vector3 lastPositionOfSeenPlayer;
        private float BaseAcceleration = 55f;
        private float SpeedIncraseRate = 3f;
        private bool hasEnteredChaseMode = false;
        private bool lostPlayerInChase = false;
        private bool ateTargetPlayer = false;
        private bool ateTargetPlayerBody;
        private Collider[] nearPlayerColliders;


        // Coroutine


        // Sounds
        public AudioClip SpawnSFX;
        public AudioClip LethalScreamSFX;
        public AudioClip AttackSFX;

        // Other
        System.Random enemyRandom;
        private LWCLogger logger = new LWCLogger("LethalGigaAI");

        private enum LGBehaviour
        {
            SearchingForPlayers,
            Chasing,
            Rage
        }

        // Init
        public override void Start()
        {
            base.Start();

            logger.LogInfo("LethalGiga spawned");

            creatureAnimator.SetBool("isWalking", true);

            enemyRandom = new System.Random(StartOfRound.Instance.randomMapSeed + thisEnemyIndex);

            nearPlayerColliders = new Collider[4];

            currentBehaviourStateIndex = (int)LGBehaviour.SearchingForPlayers;
        }

        // Every 0.2s
        public override void DoAIInterval()
        {
            base.DoAIInterval();

            if (StartOfRound.Instance.livingPlayers == 0 || isEnemyDead) return;

           switch (currentBehaviourStateIndex)
            {
                case (int)LGBehaviour.SearchingForPlayers:
                    if (!searchForPlayers.inProgress)
                    {
                        StartSearch(base.transform.position, searchForPlayers);
                        logger.LogInfo("Startet new search");
                    }
                    break;

                case (int)LGBehaviour.Chasing:
                    CheckForVeryClosePlayer();
                    if (lostPlayerInChase)
                    {
                        movingTowardsTargetPlayer = false;
                        if (!searchForPlayers.inProgress)
                        {
                            searchForPlayers.searchWidth = 30f;
                            StartSearch(lastPositionOfSeenPlayer, searchForPlayers);
                            logger.LogInfo("Lost player in chase; beginning search where the player was last seen");
                        }
                    }
                    else if (searchForPlayers.inProgress)
                    {
                        StopSearch(searchForPlayers);
                        movingTowardsTargetPlayer = true;
                        logger.LogInfo("Found player during chase; stopping search coroutine and moving after target player");
                    }
                    break;
            }
        }

        // Every frame
        public override void Update()
        {
            base.Update();

            // Checks if stunned
            CalculateAgentSpeed();

            bool playerHasLineOfSight = GameNetworkManager.Instance.localPlayerController.HasLineOfSightToPosition(base.transform.position + Vector3.up * 0.25f, 80f, 25, 5f);
        
            // Add Fear Level to client
            if (playerHasLineOfSight)
            {
                if (currentBehaviourStateIndex == (int)LGBehaviour.Rage)
                {
                    GameNetworkManager.Instance.localPlayerController.IncreaseFearLevelOverTime(5f, 5f);
                }
                else
                {
                    GameNetworkManager.Instance.localPlayerController.IncreaseFearLevelOverTime(1f, 2f);
                }
            }
            
            
            switch(currentBehaviourStateIndex)
            {
                case (int)LGBehaviour.SearchingForPlayers:

                    if (hasEnteredChaseMode)
                    {
                        hasEnteredChaseMode = false;
                        searchForPlayers.searchWidth = 35f;
                        openDoorSpeedMultiplier = 0.2f;
                        agent.stoppingDistance = 0f;
                        agent.speed = 7f;
                    }

                    if (checkLineOfSightInterval <= 0.05f)
                    {
                        checkLineOfSightInterval += Time.deltaTime;
                        break;
                    }

                    checkLineOfSightInterval = 0f;

                    // Assign a target player
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

                    // If target player is own client
                    if (playerControllerB3 == GameNetworkManager.Instance.localPlayerController)
                    {
                        noticePlayerTimer = Mathf.Clamp(noticePlayerTimer + 0.05f, 0f, 10f);
                        if (noticePlayerTimer > 0.2f && !beginningChasingThisClient)
                        {
                            beginningChasingThisClient = true;
                            BeginChasingPlayerServerRpc((int)GameNetworkManager.Instance.localPlayerController.playerClientId);
                            ChangeOwnershipOfEnemy(playerControllerB3.actualClientId);
                        }
                    }
                    else
                    {
                        noticePlayerTimer -= Time.deltaTime;
                    }
                    break;

                case (int)LGBehaviour.Chasing:
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

                    // Is stunned
                    if (!base.IsOwner || stunNormalizedTimer > 0f) break;
                    
                    if (checkLineOfSightInterval <= 0.075f)
                    {
                        checkLineOfSightInterval += Time.deltaTime;
                        break;
                    }

                    // If player is in range
                    checkLineOfSightInterval = 0f;
                    Vector3 targetPlayerPosition = targetPlayer.deadBody.bodyParts[0].transform.position;
                    if (
                        !ateTargetPlayerBody && 
                        targetPlayer != null &&
                        targetPlayer.deadBody != null &&
                        Vector3.Distance(base.transform.position, targetPlayerPosition) < 3.3f)
                    {
                        logger.LogInfo("Damage player");
                        ateTargetPlayerBody = true;
                        inSpecialAnimation = true;

                        creatureAnimator.SetTrigger("HitPlayer");

                        base.HitEnemy(2, targetPlayer, true);
                    }

                    if (inSpecialAnimation) break;
                    
                    if (lostPlayerInChase)
                    {
                        PlayerControllerB playerControllerB = CheckLineOfSightForPlayer(55f);

                        if ((bool)playerControllerB)
                        {
                            noticePlayerTimer = 0f;
                            lostPlayerInChase = false;
                            LethalScreamServerRpc();
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
                            if (noticePlayerTimer < -15f) SwitchToBehaviourState((int)LGBehaviour.SearchingForPlayers);
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

                case (int)LGBehaviour.Rage: break;
            }
        }

        


        // Networking

        [ServerRpc(RequireOwnership = false)]
        private void LethalScreamServerRpc()
        {
            LethalScreamClientRpc();
        }
        [ClientRpc]
        private void LethalScreamClientRpc()
        {
            PlaySFXOnce(LethalScreamSFX);

            creatureAnimator.SetTrigger("LethalScream");

            Vector3 playerPosition = GameNetworkManager.Instance.localPlayerController.transform.position;

            if (Vector3.Distance(playerPosition, base.transform.position) < 15f)
            {
                GameNetworkManager.Instance.localPlayerController.JumpToFearLevel(2f);
            }

            Turret[] turrets = UnityEngine.Object.FindObjectsOfType<Turret>();

            foreach (Turret t in turrets)
            {
                t.EnterBerserkModeServerRpc(-1);
            }
        }

        [ServerRpc(RequireOwnership = false)]
        private void BeginChasingPlayerServerRpc(int playerClientId)
        {
            BeginChasingPlayerClientRpc(playerClientId);
        }
        [ClientRpc]
        private void BeginChasingPlayerClientRpc(int playerClientId)
        {
            SwitchToBehaviourState((int)LGBehaviour.Chasing);
            PlayerControllerB player = StartOfRound.Instance.allPlayerScripts[playerClientId];
            SetMovingTowardsTargetPlayer(player);
        }


        // Util
        private void CheckForVeryClosePlayer()
        {
            if (Physics.OverlapSphereNonAlloc(base.transform.position, 1.5f, nearPlayerColliders, 8, QueryTriggerInteraction.Ignore) > 0)
            {
                PlayerControllerB component = nearPlayerColliders[0].transform.GetComponent<PlayerControllerB>();

                if (
                    component != null && 
                    component != targetPlayer && 
                    !Physics.Linecast(
                        base.transform.position + Vector3.up * 0.3f, 
                        component.transform.position, 
                        StartOfRound.Instance.collidersAndRoomMask
                    ))
                {
                    targetPlayer = component;
                }
            }
        }

        private void PlaySFXOnce(AudioClip clip)
        {
            creatureVoice.PlayOneShot(clip);
            WalkieTalkie.TransmitOneShotAudio(creatureVoice, clip);
        }

        private void CalculateAgentSpeed()
        {
            // Is stunned
            if (stunNormalizedTimer >= 0f)
            {
                agent.speed = 0.5f;
                agent.acceleration = 200f;
                creatureAnimator.SetBool("stunned", value: true);
            }

            creatureAnimator.SetBool("stunned", value: false);
        }
    }
}