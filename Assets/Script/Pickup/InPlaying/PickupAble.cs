using UnityEngine;

public class PickupAble : MonoBehaviour
{
    [SerializeField] private PickupAbleItem itemOffThisObject;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            other.GetComponent<PlayerPickup>().ShowPickupItemInfo(itemOffThisObject);
            gameObject.SetActive(false);
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            other.GetComponent<PlayerPickup>().ClosePickupItemInfo();
            gameObject.SetActive(false);
        }
    }
}

