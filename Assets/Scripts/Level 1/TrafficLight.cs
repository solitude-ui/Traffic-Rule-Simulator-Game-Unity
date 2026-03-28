using UnityEngine;

public class TrafficLight : MonoBehaviour
{
    public Renderer redRenderer;
    public Renderer yellowRenderer;
    public Renderer greenRenderer;

    public float redTime = 5f;
    public float yellowTime = 2f;
    public float greenTime = 5f;

    // ✅ ADD STATE SYSTEM
    public enum State { Red, Yellow, Green }
    private State currentState;

    // ✅ USED BY UI / AI / ZONE
    public State GetState()
    {
        return currentState;
    }

    void Start()
    {
        StartCoroutine(Cycle());
    }

    System.Collections.IEnumerator Cycle()
    {
        while (true)
        {
            // 🔴 RED
            SetRed();
            yield return new WaitForSeconds(redTime);

            // 🟡 YELLOW (before green)
            SetYellow();
            yield return new WaitForSeconds(yellowTime);

            // 🟢 GREEN
            SetGreen();
            yield return new WaitForSeconds(greenTime);

            // 🟡 YELLOW (before red)
            SetYellow();
            yield return new WaitForSeconds(yellowTime);
        }
    }

    void SetRed()
    {
        currentState = State.Red; // ✅ IMPORTANT

        ApplyEmission(redRenderer, new Color(0.5f, 0f, 0f), 1.2f);
        ApplyOff(yellowRenderer);
        ApplyOff(greenRenderer);
    }

    void SetYellow()
    {
        currentState = State.Yellow; // ✅ IMPORTANT

        ApplyOff(redRenderer);
        ApplyEmission(yellowRenderer, Color.yellow, 2f);
        ApplyOff(greenRenderer);
    }

    void SetGreen()
    {
        currentState = State.Green; // ✅ IMPORTANT

        ApplyOff(redRenderer);
        ApplyOff(yellowRenderer);
        ApplyEmission(greenRenderer, Color.green, 2f);
    }

    void ApplyEmission(Renderer rend, Color color, float intensity)
    {
        var mat = rend.material;
        mat.EnableKeyword("_EMISSION");
        mat.SetColor("_EmissionColor", color * intensity);
    }

    void ApplyOff(Renderer rend)
    {
        var mat = rend.material;
        mat.SetColor("_EmissionColor", Color.black);
    }
}