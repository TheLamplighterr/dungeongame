using UnityEngine;
using UnityEngine.UI;

public class HealthBarUI : MonoBehaviour
{
    public PlayerHealth playerHealth;
    public Image fillImage;

    [Header("Smooth Settings")]
    public float smoothSpeed = 5f;

    private float currentFill;

    void Update()
    {
        float targetFill =
            (float)playerHealth.currentHealth /
            playerHealth.maxHealth;

        //  Smooth Übergang
        currentFill = Mathf.Lerp(currentFill, targetFill, Time.deltaTime * smoothSpeed);

        fillImage.fillAmount = currentFill;

        //  Farbverlauf (Rot → Gelb → Grün)
        Color healthColor;

        if (currentFill > 0.5f)
        {
            healthColor = Color.Lerp(Color.yellow, Color.green, (currentFill - 0.5f) * 2f);
        }
        else
        {
            healthColor = Color.Lerp(Color.red, Color.yellow, currentFill * 2f);
        }

        fillImage.color = healthColor;
    }
}