using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using Random = UnityEngine.Random;

namespace MotionTracker.Patches;

public class MotionTrackerScript : GrabbableObject
{
    ParticleSystem particleSystem;
    AudioSource audioSource;

    public AudioClip shootSound;
    public AudioClip noammoSound;

    public GameObject BulletTrail;

    Transform BulletSpawnPoint;

    public float spread = 7f, range = 35;

    RoundManager roundManager;

    // Update is called once per frame
    public new void Start()
    {
        base.Start();
        //this.transform.SetParent(GameObject.Find("/Environment/Props").transform);

        Debug.Log($"Found GameObject: {GameObject.Find("/Environment/Props").transform}");

    }
    public void Awake()
    {
        Debug.Log("MOTION TRACKER AWAKE");

        particleSystem = GetComponentInChildren<ParticleSystem>();
        audioSource = GetComponent<AudioSource>();
        roundManager = FindObjectOfType<RoundManager>();

        BulletSpawnPoint = transform.GetChild(0);

        grabbable = true;
        grabbableToEnemies = true;
        mainObjectRenderer = GetComponent<MeshRenderer>();
        useCooldown = 1f;
        insertedBattery = new Battery(false, 100);

        //Fix Materials


        Debug.Log("MOTION TRACKER END AWAKE");
    }

    private void OnDestroy()
    {
        Debug.Log("MOTION TRACKER DESTROYED");
        Debug.Log("MOTION TRACKER DESTROYED");
        Debug.Log("MOTION TRACKER DESTROYED");
        Debug.Log("MOTION TRACKER DESTROYED");
        Debug.Log("MOTION TRACKER DESTROYED");
        Debug.Log("MOTION TRACKER DESTROYED");
    }

    public override void ItemActivate(bool used, bool buttonDown = true)
    {
        base.ItemActivate(used, buttonDown);

        if (insertedBattery.charge >= 0.16f)
        {
            insertedBattery.charge -= 0.16f;
            if (insertedBattery.charge < 0.16f) insertedBattery.charge = 0;

            audioSource.PlayOneShot(shootSound);
            particleSystem.Play();

            for (int i = 0; i < 5; i++)
            {
                float rx = Random.Range(-spread - 3, spread + 3);
                float ry = Random.Range(-spread, spread);

                Vector3 targetPos = transform.position + transform.forward * range;
                targetPos = new Vector3(targetPos.x + rx, targetPos.y + ry, targetPos.z + rx);

                Vector3 direction = targetPos - transform.position;

                LineRenderer Trail = Instantiate(BulletTrail, BulletSpawnPoint).GetComponent<LineRenderer>();

                if (Physics.Raycast(transform.position, direction.normalized, out RaycastHit rayHit, range))
                {
                    SpawnTrail(Trail, rayHit.point);

                    if (rayHit.collider.TryGetComponent(out IHittable hittable))
                    {
                        if (hittable == null) return;

                        hittable.Hit(1, playerHeldBy.gameplayCamera.transform.forward, playerHeldBy);
                    }
                    else if (rayHit.collider.TryGetComponent(out EnemyAI enemy))
                    {
                        if (enemy == null) return;
                        if (enemy.name.ToLower().Contains("ghost") || enemy.name.ToLower().Contains("spring")) return;
                        if (!enemy.enemyType.canDie) return;

                        if (enemy.enemyHP > 0)
                        {
                            enemy.enemyHP -= 1;

                            if (enemy.enemyHP == 0)
                            {
                                switch (enemy.enemyType.enemyName)
                                {
                                    case "HoarderBug":
                                        SpawnScrap(roundManager.currentLevel.spawnableScrap[2],
                                            enemy.transform.position, 50);
                                        break;
                                    case "Centipede":
                                        SpawnScrap(roundManager.currentLevel.spawnableScrap[2],
                                            enemy.transform.position, 50);
                                        break;
                                    case "SandSpider":
                                        SpawnScrap(roundManager.currentLevel.spawnableScrap[2],
                                            enemy.transform.position, 100);
                                        break;
                                    case "MouthDog":
                                        SpawnScrap(roundManager.currentLevel.spawnableScrap[2],
                                            enemy.transform.position, 100);
                                        break;
                                    case "Flowerman":
                                        SpawnScrap(roundManager.currentLevel.spawnableScrap[2],
                                            enemy.transform.position, 200);
                                        break;
                                    case "Jester":
                                        SpawnScrap(roundManager.currentLevel.spawnableScrap[2],
                                            enemy.transform.position, 500);
                                        break;
                                    default:
                                        SpawnScrap(roundManager.currentLevel.spawnableScrap[2],
                                            enemy.transform.position, 100);
                                        break;
                                }
                            }
                        }
                    }
                }
                else
                {
                    SpawnTrail(Trail, transform.position + direction * 100);
                }
            }

            StartCoroutine(ShakeCamera());
        }
        else
        {
            audioSource.PlayOneShot(noammoSound);
        }
    }

    private void SpawnTrail(LineRenderer Trail, Vector3 HitPoint)
    {
    }

    public void SpawnScrap(SpawnableItemWithRarity scrapItem, Vector3 enemyPos, int customWorth = 0)
    {
        List<int> list = new List<int>();
        List<NetworkObjectReference> list3 = new List<NetworkObjectReference>();
        List<Item> ScrapToSpawn = new List<Item>();

        GameObject gameObject = Instantiate(scrapItem.spawnableItem.spawnPrefab, enemyPos, new Quaternion(),
            roundManager.spawnedScrapContainer);
        GrabbableObject component = gameObject.GetComponent<GrabbableObject>();
        component.transform.rotation = Quaternion.Euler(component.itemProperties.restingRotation);
        component.fallTime = 0f;
        ScrapToSpawn.Add(scrapItem.spawnableItem);
        list.Add((int)(roundManager.AnomalyRandom.Next(ScrapToSpawn[0].minValue, ScrapToSpawn[0].maxValue) *
                       roundManager.scrapValueMultiplier));
        component.scrapValue = list[list.Count - 1];
        NetworkObject component2 = gameObject.GetComponent<NetworkObject>();
        component2.Spawn(false);
        list3.Add(component2);

        if (customWorth != 0)
        {
            component.SetScrapValue(customWorth);
        }

        //StartCoroutine(roundManager.waitForScrapToSpawnToSync(list3.ToArray(), list.ToArray()));
    }

    IEnumerator ShakeCamera()
    {
        Vector3 originalPosition = playerHeldBy.gameplayCamera.transform.localPosition;

        float elapsed = 0.0f;

        while (elapsed < 0.4f)
        {
            float x = Random.Range(-1f, 1f) * 0.3f;
            float y = Random.Range(-1f, 1f) * 0.3f;

            playerHeldBy.gameplayCamera.transform.localPosition = new Vector3(x, y, originalPosition.z);

            elapsed += Time.deltaTime;

            yield return null;
        }

        playerHeldBy.gameplayCamera.transform.localPosition = originalPosition;
    }
}