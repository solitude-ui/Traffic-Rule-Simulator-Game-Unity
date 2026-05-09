using UnityEngine;

public class CarAudioManager : MonoBehaviour
{
    [Header("Audio Sources")]
    public AudioSource engineAudioSource;   // For looping idle
    public AudioSource sfxAudioSource;      // For one-shot sounds

    [Header("Audio Clips")]
    public AudioClip carIdleClip;
    public AudioClip carStartClip;
    public AudioClip carStopClip;
    public AudioClip coinCollectClip;

    [Header("Settings")]
    public float idlePitch = 1f;
    public float drivingPitch = 1.4f;

    private bool isCarRunning = false;

    void Start()
    {
        // Start in stopped state
        StopCar();
    }

    // ─── Call these methods based on your game logic ───

    public void StartCar()
    {
        if (isCarRunning) return;
        isCarRunning = true;

        // Play startup sound once
        sfxAudioSource.PlayOneShot(carStartClip);

        // Start idle loop after the start sound finishes
        Invoke(nameof(PlayIdleLoop), carStartClip.length);
    }

    public void StopCar()
    {
        if (!isCarRunning) return;
        isCarRunning = false;

        // Stop idle loop
        engineAudioSource.Stop();

        // Play stop sound
        sfxAudioSource.PlayOneShot(carStopClip);
    }

    public void PlayIdleLoop()
    {
        if (!isCarRunning) return;
        engineAudioSource.clip = carIdleClip;
        engineAudioSource.loop = true;
        engineAudioSource.pitch = idlePitch;
        engineAudioSource.Play();
    }

    public void CollectCoin()
    {
        // Can play anytime, overlaps with engine
        sfxAudioSource.PlayOneShot(coinCollectClip);
    }

    // Optional: change pitch when accelerating
    public void SetDriving(bool isDriving)
    {
        if (engineAudioSource.isPlaying)
        {
            engineAudioSource.pitch = isDriving ? drivingPitch : idlePitch;
        }
    }

    // ─── Handle input for car control ───
    void Update()
    {
        // Up Arrow = Accelerate (play start/driving sound)
        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            StartCar();
        }

        // Down Arrow = Brake/Reverse (play stop sound)
        if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            StopCar();
        }

        // No keys pressed = Play idle sound
        if (!Input.GetKey(KeyCode.UpArrow) && !Input.GetKey(KeyCode.DownArrow))
        {
            if (!isCarRunning && !engineAudioSource.isPlaying)
            {
                PlayIdleLoop();
                isCarRunning = true;
            }
        }
    }
}