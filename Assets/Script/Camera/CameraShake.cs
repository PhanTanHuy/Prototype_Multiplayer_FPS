using UnityEngine;
using System.Collections;

public class CameraShake : MonoBehaviour
{
    Quaternion originalLocalRot;
    Coroutine shakeCoroutine;

    void Awake()
    {
        originalLocalRot = transform.localRotation;
    }

    public void Shake(float duration, float strength)
    {
        if (shakeCoroutine != null)
            StopCoroutine(shakeCoroutine);

        shakeCoroutine = StartCoroutine(IEShake(duration, strength));
    }

    IEnumerator IEShake(float duration, float strength)
    {
        float time = 0f;

        float seedX = Random.Range(0f, 100f);
        float seedY = Random.Range(0f, 100f);
        float seedZ = Random.Range(0f, 100f);


        while (time < duration)
        {
            float x = (Mathf.PerlinNoise(seedX, Time.time * 20f) - 0.5f);
            float y = (Mathf.PerlinNoise(seedY, Time.time * 20f) - 0.5f);
            float z = (Mathf.PerlinNoise(seedZ, Time.time * 20f) - 0.5f) * 2f;

            Vector3 shakeEuler = new Vector3(
                x * strength,
                y * strength,
                //0f,
                //0f,
                z * strength
            );

            transform.localRotation =
                originalLocalRot * Quaternion.Euler(shakeEuler);

            time += Time.deltaTime;
            yield return null;
        }

        transform.localRotation = originalLocalRot;
    }
}
