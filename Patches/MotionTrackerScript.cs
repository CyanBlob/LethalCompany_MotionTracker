using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using Random = UnityEngine.Random;

namespace MotionTracker.Patches;

public struct ScannedEntity
{
    public Collider obj;
    public Vector3 position;
    public Vector3 rawPosition;
    public float speed;
    public GameObject blip;
}

public class MotionTrackerScript : PhysicsProp
{
    ParticleSystem particleSystem;
    AudioSource audioSource;

    public AudioClip shootSound;
    public AudioClip noammoSound;

    RoundManager roundManager;

    private GameObject baseRadar;
    private RectTransform baseRadarRect;
    private GameObject baseRadarOff;
    private GameObject LED;
    private GameObject blip;
    private GameObject blipParent;

    private float searchRadius = 50f;

    private Hashtable scannedEntities = new();

    private List<GameObject> blipPool = new List<GameObject>();

    private int maxEntities = 50;

    Collider[] colliders = new Collider[200];

    // Update is called once per frame
    public new void Start()
    {
        base.Start();
        //this.transform.SetParent(GameObject.Find("/Environment/Props").transform);

        Debug.Log($"Found GameObject: {GameObject.Find("/Environment/Props").transform}");
    }

    public void Awake()
    {
        particleSystem = GetComponentInChildren<ParticleSystem>();
        audioSource = transform.gameObject.AddComponent<AudioSource>();
        roundManager = FindObjectOfType<RoundManager>();


        grabbable = true;
        grabbableToEnemies = true;
        mainObjectRenderer = GetComponent<MeshRenderer>();
        useCooldown = 1f;
        insertedBattery = new Battery(false, 1);

        baseRadar = transform.Find("Canvas/BaseRadar").gameObject;
        baseRadarRect = baseRadar.GetComponent<RectTransform>();

        baseRadarOff = transform.Find("Canvas/BaseRadar_off").gameObject;
        LED = transform.Find("LED").gameObject;
        blipParent = transform.Find("Canvas/BlipParent").gameObject;
        blip = transform.Find("Canvas/BlipParent/Blip").gameObject;
        blip.SetActive(false);

        for (var i = 0; i < maxEntities; ++i)
        {
            var _blip = Instantiate(blip, baseRadar.transform);
            _blip.transform.parent = blipParent.transform;
            _blip.SetActive(false);
            blipPool.Add(_blip);
        }

        Enable(false);
    }

    private void Enable(bool enable, bool inHand = true)
    {
        baseRadar.SetActive(enable);
        LED.SetActive(enable);

        if (inHand)
        {
            baseRadarOff.SetActive(!enable);
        }
        else
        {
            baseRadarOff.SetActive(false);
        }

        if (!enable)
        {
            foreach (var blip in blipPool)
            {
                blip.SetActive(false);
            }
        }
    }

    public override void ItemActivate(bool used, bool buttonDown = true)
    {
        base.ItemActivate(used, buttonDown);

        Enable(used);

        Debug.Log($"Motion tracker activate? : {used}");
    }

    public override void UseUpBatteries()
    {
        base.UseUpBatteries();
        Enable(false, false);
    }

    public override void Update()
    {
        base.Update();

        if (isPocketed)
        {
            Enable(false, false);
            return;
        }

        if (isBeingUsed)
        {
            if (isHeld)
            {
                Enable(true);
                blipParent.transform.localRotation = Quaternion.Euler(0, 0, playerHeldBy.transform.eulerAngles.y + 0);
            }
            else
            {
                Enable(false, false);
            }

            if (insertedBattery.empty)
            {
                Enable(false);
                return;
            }

            for (var j = 0; j < maxEntities; ++j)
            {
                blipPool[j].SetActive(false);
            }

            var lastScannedEntities = new Hashtable(scannedEntities);
            scannedEntities.Clear();

            var playerPos = transform.position;

            var colliderCount = Physics.OverlapSphereNonAlloc(playerPos, searchRadius, colliders,
                layerMask: 8 | 524288); // Player | Enemies


            var i = 0;
            for (int c = 0; c < colliderCount; ++c)
            {
                var collider = colliders[c];
                var entity = new ScannedEntity
                    { obj = collider, position = collider.transform.position - baseRadar.transform.position, rawPosition = collider.transform.position};

                if (lastScannedEntities.Contains(entity.obj.transform.GetHashCode()))
                {
                    entity.speed = (collider.transform.position -
                                    ((ScannedEntity)lastScannedEntities[entity.obj.transform.GetHashCode()]).rawPosition)
                        .magnitude;
                }
                else
                {
                    entity.speed = 0;
                }

                if (!scannedEntities.Contains(entity.obj.transform.GetHashCode()))
                {
                    entity.blip = blipPool[i];

                    i += 1;

                    var blipLocalPos = entity.blip.transform.localPosition;
                    blipLocalPos = entity.position;

                    entity.blip.transform.localPosition = new Vector3(
                        Remap(blipLocalPos.x, -searchRadius, searchRadius, -45, 45),
                        Remap(blipLocalPos.z, -searchRadius, searchRadius, -45, 45),
                        -.1f);

                    // only enable blips for moving objects
                    if (entity.speed > .06) // faster than a crouch walk
                    {
                        entity.blip.SetActive(true);
                    }
                    else
                    {
                        entity.blip.SetActive(false);
                    }

                    scannedEntities.Add(entity.obj.transform.GetHashCode(), entity);
                }
            }

        }
    }

    public float Remap(float from, float fromMin, float fromMax, float toMin, float toMax)
    {
        var fromAbs = from - fromMin;
        var fromMaxAbs = fromMax - fromMin;

        var normal = fromAbs / fromMaxAbs;

        var toMaxAbs = toMax - toMin;
        var toAbs = toMaxAbs * normal;

        var to = toAbs + toMin;

        return to;
    }
}