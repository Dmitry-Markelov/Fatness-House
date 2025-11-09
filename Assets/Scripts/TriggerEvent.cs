using UnityEngine;

public class TriggerEvent : MonoBehaviour
{
    void OnTriggerEnter(Collider other)
    {
        Debug.Log($"Объект вошел в триггер: {other.name}");
    }

    void OnTriggerExit(Collider other)
    {
        Debug.Log($"Объект вышел из триггера: {other.name}");
    }
}