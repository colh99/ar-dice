using UnityEngine;
using TMPro;
using System.Runtime.CompilerServices;

public class DiceSpawner : MonoBehaviour
{
    [SerializeField]
    private TMP_Dropdown diceTypeDropdown, diceColorDropdown;
    [SerializeField]
    private GameObject dicePrefabD6, dicePrefabD10;

    [SerializeField]
    private Material d6White, d6Black, d6Red, d6Green, d6Blue, d6Yellow, 
                      d10White, d10Black, d10Red, d10Green, d10Blue, d10Yellow;

    private GameObject dicePrefab;
    private Renderer diceRenderer;

    private float lastTapTime;

    private void Update()
    {
        // Check for double tap
        if (Input.touchCount == 1 && Input.GetTouch(0).phase == TouchPhase.Ended)
        {
            float currentTime = Time.time;
            float timeSinceLastTap = currentTime - lastTapTime;

            if (timeSinceLastTap <= 0.3f)
            {
                // When the user double taps:
                SpawnDice();
            }

            lastTapTime = currentTime;
        }
    }

    private void SpawnDice()
    {
        GameObject arCamera = GameObject.Find("AR Camera");
        if (arCamera == null)
        {
            Debug.LogError("AR Camera not found in the scene!");
            return;
        }

        // Determine if selected die is D6 or D10
        if (diceTypeDropdown.value == 0) // D6
        {
            dicePrefab = dicePrefabD6;
            diceRenderer = dicePrefab.GetComponent<Renderer>();

            // Determine selected color of D6 die
            diceRenderer.material = diceColorDropdown.value switch
            {
                0 => d6White,
                1 => d6Black,
                2 => d6Red,
                3 => d6Green,
                4 => d6Blue,
                5 => d6Yellow,
                _ => d6White,
            };
        }
        else if (diceTypeDropdown.value == 1) // D10
        {
            dicePrefab = dicePrefabD10;
            diceRenderer = dicePrefab.GetComponent<Renderer>();

            // Determine selected color of D10 die
            diceRenderer.material = diceColorDropdown.value switch
            {
                0 => d10White,
                1 => d10Black,
                2 => d10Red,
                3 => d10Green,
                4 => d10Blue,
                5 => d10Yellow,
                _ => d10White,
            };
        }

        Vector3 cameraForward = arCamera.transform.forward;
        Vector3 cameraUp = arCamera.transform.up;
        Vector3 spawnPosition = arCamera.transform.position + cameraForward * 0.2f + cameraUp * 0.05f;

        // Generate random rotation angles
        float randomAngleX = Random.Range(0f, 360f);
        float randomAngleY = Random.Range(0f, 360f);
        float randomAngleZ = Random.Range(0f, 360f);
        Quaternion randomRotation = Quaternion.Euler(randomAngleX, randomAngleY, randomAngleZ);

        GameObject dice = Instantiate(dicePrefab, spawnPosition, randomRotation);
    }
}