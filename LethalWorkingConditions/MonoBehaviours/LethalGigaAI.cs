using System.Collections.Generic;
using UnityEngine.InputSystem.XR;
using UnityEngine;
using GameNetcodeStuff;
using System.Collections;
using Unity.Netcode;
using LethalWorkingConditions.Helpers;

namespace LethalWorkingConditions.MonoBehaviours
{
    /*public class LethalGigaAI : CrawlerAI { }
    */

    public class LethalGigaAI : EnemyAI
    {
        #pragma warning disable 0649
        public Transform turnCompass;

        public Transform attackArea;

        public AISearchRoutine scoutingSearchRoutine;

        #pragma warning restore 0649
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
        }

        private LWCLogger logger = new LWCLogger("LethalGigaAI");

        public override void Start()
        {
            base.Start();

            logger.LogInfo("LethalGiga spawned");

            timeSinceHittingLocalPlayer = 0;

            //creatureAnimator.SetTrigger()

            timeSinceNewRandPos = 0;

            positionRandomness = new Vector3(0, 0, 0);

            enemyRandom = new System.Random(StartOfRound.Instance.randomMapSeed + thisEnemyIndex);

            isDeadAnimationDone = false;

            currentBehaviourStateIndex = (int)LGBehaviour.SearchingForPlayer;
        }

        public override void Update()
        {
            base.Update();
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
            if (isEnemyDead || StartOfRound.Instance.allPlayersDead) return;
            
            // Sets scoutingSearchRoutine.inProgress to True if serching, False if found player
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
                default:
                    logger.LogInfo("This Behavior State doesn't exist!");
                    break;
            }
        }

        void KeepSearchingForPlayerUnlessInRange(float range, ref AISearchRoutine routine)
        {
            TargetClosestPlayer();
            if (targetPlayer != null && Vector3.Distance(transform.position, targetPlayer.transform.position) <= range)
            {
                if (routine.inProgress)
                {
                    logger.LogInfo("Start Target Player");
                    StopSearch(routine);
                    SwitchToBehaviourClientRpc((int)LGBehaviour.StickingInFrontOfPlayer);
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





    }
}