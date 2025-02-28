using UnityEngine;

public class FPSTracker : MonoBehaviour
{
    public float targetFPS = 50f; // Target FPS
    public float tolerance = 5f; // Allow slight FPS variation before adjusting timeScale
    private float currentFPS;
    private float timeAccumulator = 0f;
    private int frameCount = 0;

    void Update()
    {
        // Calculate FPS
        timeAccumulator += Time.unscaledDeltaTime;
        frameCount++;

        if (timeAccumulator >= 1f) // Update every second
        {
            currentFPS = frameCount / timeAccumulator;
            frameCount = 0;
            timeAccumulator = 0f;

            AdjustTimeScale();
        }
    }

    void AdjustTimeScale()
    {
        float fpsDifference = targetFPS - currentFPS/Time.timeScale;

        // Calculate dynamic adjustment speed
        float dynamicAdjustmentSpeed = -(fpsDifference / targetFPS);

        /*
        if (fpsDifference > tolerance) // FPS is significantly below target
        {
            Time.timeScale = Mathf.Clamp(Time.timeScale - dynamicAdjustmentSpeed, 0.1f, 100.0f);
        }
        else if (fpsDifference < -tolerance) // FPS is significantly above target
        {
            Time.timeScale = Mathf.Clamp(Time.timeScale + dynamicAdjustmentSpeed, 0.1f, 50.0f);
        }
        */

        if (Mathf.Abs(fpsDifference) > tolerance)
        {
            Time.timeScale = Mathf.Clamp(Time.timeScale + dynamicAdjustmentSpeed, 0.1f, 100.0f);
        }

        //Time.fixedDeltaTime = 0.02f / Time.timeScale;

    }

    void OnGUI()
    {
        // Display FPS and TimeScale for debugging
        GUI.Label(new Rect(10, 10, 200, 20), $"FPS: {currentFPS:F2}");
        GUI.Label(new Rect(10, 30, 200, 20), $"Time Scale: {Time.timeScale:F2}");
    }
}