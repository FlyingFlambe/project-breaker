﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerController : MonoBehaviour {

    /// Initialized Variables ///
    Rigidbody2D player;
    GunController gun;
    ExplosionController explosion;
    ScreenshakeController screenshake;
    AltimeterController altitude;

    public AudioSource engineRev;
    public AudioSource engineExplode;
    public AudioSource engineMash;
    public AudioSource healUp;
    public AudioSource wallCollide;

    [SerializeField]
    CanvasGroup gameOver;
    [SerializeField]
    CanvasGroup pressRToRestart;
    [SerializeField]
    CanvasGroup stageSuccess;

    public bool isThrusting;
    public bool isSmoking;

    /// Airplane Stats ///
    public float thrust;                    // The amount of force exerted onto airplane.
    public bool isBroken;                   // Flags whether the airplane can be flown. If durability is zero, isBroken is true. Do NOT touch this in inspector! Viewing only.
    public bool isLowHealth;                // Flags whether the player has less than 10% health.
    public Stat durability;                 // Engine's current durability.
    public float decayAmt;                  // How much durability is lost.
    public float regenAmt;                  // How much durability is regenerated.
    public float repairAmt;                 // How much the engine's durability increases for each button press to repair.
    public float healthRepairAmt;           // How much health is regained when repairing.
    public float shootDecayAmt;             // How much the durability is lost while shooting.

    public bool isDead;                     // Flags whether the airplane exploded. If health is zero, isDead is true. Do NOT touch this in inspector also.
    public bool isHurt;                     // Flags whether the player just took damage.
    public Stat health;                     // Airplane's current health.
    public int wallCollisionDamage;

    /// Set Airplane Angle ///
    Vector3 mousePos;
    Vector3 planeToMouseDir;
    float airplaneAngle;

    /////////////////////
    // Player Movement //
    /////////////////////

    void Awake()
    {
        // Initialize Stats
        durability.Initialize();
        health.Initialize();
    }

    void Start ()
    {
        player = GetComponent<Rigidbody2D>();
        gun = FindObjectOfType<GunController>();
        explosion = GetComponent<ExplosionController>();
        screenshake = FindObjectOfType<ScreenshakeController>();
        altitude = FindObjectOfType<AltimeterController>();

        engineRev = GetComponentInChildren<AudioSource>();
        engineExplode = GetComponentInChildren<AudioSource>();
        engineMash = GetComponentInChildren<AudioSource>();
        healUp = GetComponentInChildren<AudioSource>();
        wallCollide = GetComponentInChildren<AudioSource>();

        gameOver.GetComponent<CanvasGroup>().alpha = 0.0f;
        pressRToRestart.GetComponent<CanvasGroup>().alpha = 0.0f;
        stageSuccess.GetComponent<CanvasGroup>().alpha = 0.0f;

        isBroken = false;
        isHurt = false;
	}
	
	void Update ()
    {
        SetAirplaneAngle();                                 // Always update airplane's angle to be directed towards the mouse cursor.
        SmokeToggle();
        FireGunToggle();
        BarAnimation();

        // RMB creates Thrust as long as engine is not broken.
        if (!isBroken && Input.GetMouseButton(1))
        {
            Thrust();
            UseDurability();
        }
        else if (!isBroken && Input.GetMouseButton(0))
            GunUsesDurability();
        else
            RepairEngine();

        if (!isBroken && Input.GetMouseButtonDown(1))
            engineRev.Play();

        if (health.CurrentVal <= 0f)
            DieSpectacularly();

        if (altitude.timeCurrent >= altitude.timeMax)
        {
            WinGame();
        }
    }

    void SetAirplaneAngle()
    {
        mousePos = Camera.main.WorldToScreenPoint(transform.position);
        planeToMouseDir = Input.mousePosition - mousePos;
        airplaneAngle = Mathf.Atan2(planeToMouseDir.y, planeToMouseDir.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.AngleAxis(airplaneAngle - 90, Vector3.forward);
    }

    void TakeDamageFor(float damageTaken)
    {
        health.CurrentVal -= damageTaken;
    }

    void HealFor(float healthRestored)
    {
        health.CurrentVal += healthRestored;
    }

    void Thrust()
    {
        player.AddForce(player.transform.up * thrust);
    }

    void UseDurability()
    {
        if (durability.CurrentVal > 0)
            durability.CurrentVal -= decayAmt * Time.deltaTime;
        else
        {
            isBroken = true;
            screenshake.shakeAmount = 0.5f;
            screenshake.shakeDuration = 0.3f;
            engineExplode.Play();
        }
    }

    void GunUsesDurability()
    {
        if (durability.CurrentVal > 0)
            durability.CurrentVal -= shootDecayAmt * Time.deltaTime;
        else
        {
            isBroken = true;
        }
    }

    void RepairEngine()
    {
        if (isBroken)
        {
            if (Input.GetMouseButtonDown(1))
            {
                durability.CurrentVal += repairAmt;
                HealFor(healthRepairAmt);
                screenshake.shakeAmount = 0.45f;
                screenshake.shakeDuration = 0.1f;
                engineMash.Play();
                healUp.Play();
            }
            if (durability.CurrentVal >= durability.MaxVal)
                isBroken = false;
        }
        else if (!isBroken && durability.CurrentVal < durability.MaxVal)
        {
                durability.CurrentVal += regenAmt * Time.deltaTime;
        }
    }

    void FireGunToggle()
    {
        if ((Input.GetKeyDown("space") || Input.GetMouseButtonDown(0)) && !isBroken)
            gun.isFiring = true;
        if ((Input.GetKeyUp("space") || Input.GetMouseButtonUp(0)) || isBroken)
            gun.isFiring = false;
    }

    void SmokeToggle()
    {
        if (Input.GetMouseButtonDown(1) && !isBroken)
            isThrusting = true;
        if (Input.GetMouseButtonUp(1))
            isThrusting = false;

        if (isBroken || durability.CurrentVal <= 20f)
            isSmoking = true;
        else
            isSmoking = false;
    }

    public void BarAnimation()
    {

        if (health.CurrentVal <= 20f)
            GameObject.Find("HealthBarContainer").GetComponent<Animator>().SetBool("isLowHealth", true);
        else
            GameObject.Find("HealthBarContainer").GetComponent<Animator>().SetBool("isLowHealth", false);
        if (isBroken)
            GameObject.Find("EngineBarContainer").GetComponent<Animator>().SetBool("isBroken", true);
        else
            GameObject.Find("EngineBarContainer").GetComponent<Animator>().SetBool("isBroken", false);
    }

    void DieSpectacularly()
    {
        screenshake.shakeAmount = 0.8f;
        screenshake.shakeDuration = 0.5f;
        Destroy(gameObject);
        explosion.Animate();
        gameOver.GetComponent<CanvasGroup>().alpha = 1.0f;
        pressRToRestart.GetComponent<CanvasGroup>().alpha = 1.0f;
    }

    void WinGame()
    {
        health.CurrentVal = health.MaxVal;
        stageSuccess.GetComponent<CanvasGroup>().alpha = 1.0f;
        pressRToRestart.GetComponent<CanvasGroup>().alpha = 1.0f;
        Destroy(FindObjectOfType<EnemySpawnController>().gameObject);
        Destroy(FindObjectOfType<EnemyController>().gameObject);
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.tag == "Wall")
        {
            health.CurrentVal -= wallCollisionDamage;
            screenshake.shakeAmount = 0.2f;
            screenshake.shakeDuration = 0.1f;
        }
    }
}
