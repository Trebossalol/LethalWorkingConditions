using System.Collections;
using GameNetcodeStuff;
using LethalWorkingConditions.Helpers;
using Unity.Netcode;
using UnityEngine;

namespace LethalWorkingConditions.MonoBehaviours
{
    // Based on https://github.com/Trebossalol/ToiletLeechIsReal/blob/main/Plugin/src/ToiletLeechAI.cs
    class PartyMonkeyAI : EnemyAI
    {
        // We set these in our Asset Bundle, so we can disable warning CS0649
        #pragma warning disable 0649
        public Transform turnCompass;

        public Transform attackArea;

        public AISearchRoutine scoutingSearchRoutine;

        #pragma warning restore 0649
        float timeSinceHittingLocalPlayer;

        float timeSinceNewRandPos;

        Vector3 positionRandomness;

        Vector3 StalkPos;

        private LWCLogger logger = new LWCLogger("PartyMonkeyAI");

        bool isSearching;

        System.Random enemyRandom;

        bool isDeadAnimationDone;

        public override void Start()
        {
            base.Start();
            logger.LogInfo("PartyMonkey spawned!");
            timeSinceHittingLocalPlayer = 0;
            //creatureAnimator.SetTrigger("startWalk");
            timeSinceNewRandPos = 0;
            positionRandomness = new Vector3(0, 0, 0);
            isSearching = false;
            enemyRandom = new System.Random(StartOfRound.Instance.randomMapSeed + thisEnemyIndex);
            isDeadAnimationDone = false;
        }
        public override void Update()
        {
            base.Update();
            if (isEnemyDead)
            {
                // For some weird reason I can't get an RPC to get called from HitEnemy() (works from other methods), so we do this workaround. We just want the enemy to stop playing the song.
                if (!isDeadAnimationDone)
                {
                    logger.LogInfo("Stopping enemy voice with janky code.");
                    isDeadAnimationDone = true;
                    creatureVoice.Stop();
                    creatureVoice.PlayOneShot(dieSFX);
                }
                return;
            }
            timeSinceHittingLocalPlayer += Time.deltaTime;
            timeSinceNewRandPos += Time.deltaTime;
            if (targetPlayer != null && PlayerIsTargetable(targetPlayer) && !isSearching)
            {
                turnCompass.LookAt(targetPlayer.gameplayCamera.transform.position);
                transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.Euler(new Vector3(0f, turnCompass.eulerAngles.y, 0f)), 3f * Time.deltaTime);
            }
            if (stunNormalizedTimer > 0f)
            {
                agent.speed = 0f;
            }
            // myLogSource.LogInfo($"Time: {timeSinceNewRandPos}");
        }

        public override void DoAIInterval()
        {
            base.DoAIInterval();

            if (isEnemyDead)
            {
                agent.speed = 0f;
                return;
            }
            if (!isEnemyDead && !StartOfRound.Instance.allPlayersDead)
            {
                if (TargetClosestPlayer(4f) && Vector3.Distance(transform.position, targetPlayer.transform.position) < 25)
                {
                    if (isSearching)
                    {
                        logger.LogInfo("Target Player");
                        StopSearch(scoutingSearchRoutine);
                        isSearching = false;
                        movingTowardsTargetPlayer = true;
                        moveTowardsDestination = false; // I should probably remove this line
                    }
                }
                else
                {
                    if (!isSearching)
                    {
                        logger.LogInfo("Stop Target Player");
                        StartSearch(transform.position, scoutingSearchRoutine);
                        isSearching = true;
                        movingTowardsTargetPlayer = false;
                        moveTowardsDestination = true; // And also this
                    }

                }
            }
            if (targetPlayer != null && PlayerIsTargetable(targetPlayer) && !isSearching)
            {
                if (timeSinceNewRandPos > 0.7f && IsOwner)
                {
                    timeSinceNewRandPos = 0;
                    if (enemyRandom.Next(0, 5) == 0)
                    {
                        // Attack
                        StartCoroutine(SwingAttack());
                    }
                    else
                    {
                        // In front of player
                        positionRandomness = new Vector3(enemyRandom.Next(-2, 2), 0, enemyRandom.Next(-2, 2));
                        StalkPos = targetPlayer.transform.position - Vector3.Scale(new Vector3(-5, 0, -5), targetPlayer.transform.forward) + positionRandomness;
                    }
                    SetDestinationToPosition(StalkPos);
                }
                agent.speed = 5f;
            }
            else
            {
                agent.speed = 3f;
            }
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
                logger.LogInfo("PartyMonkey Collision with Player!");
                timeSinceHittingLocalPlayer = 0f;
                playerControllerB.DamagePlayer(20);
            }
        }

        IEnumerator SwingAttack()
        {
            StalkPos = targetPlayer.transform.position;
            SetDestinationToPosition(StalkPos);
            yield return new WaitForSeconds(0.5f);
            if (isEnemyDead)
            {
                yield break;
            }
            //DoAnimationClientRpc("swingAttack");
            yield return new WaitForSeconds(0.24f);
            SwingAttackHitClientRpc();
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
            //creatureAnimator.SetTrigger(animationName);
        }

        [ClientRpc]
        public void SwingAttackHitClientRpc()
        {
            logger.LogInfo("SwingAttackHitClientRPC");
            int playerLayer = 1 << 3;
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
                        playerControllerB.DamagePlayer(50);
                    }
                }
            }
        }
    }
}