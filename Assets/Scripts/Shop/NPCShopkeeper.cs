using UnityEngine;

public class NPCShopkeeper : MonoBehaviour
{
    [SerializeField] string npcName = "Pierre";
    [SerializeField] float interactDistance = 2f;
    [SerializeField] ShopUI shopUI;
    [SerializeField] GameObject interactPrompt;

    Transform player;
    bool playerInRange;

    void Start()
    {
        var pc = FindObjectOfType<PlayerController>();
        if (pc != null) player = pc.transform;
        if (interactPrompt != null) interactPrompt.SetActive(false);
    }

    void Update()
    {
        if (player == null) return;

        float dist = Vector3.Distance(transform.position, player.position);
        bool inRange = dist <= interactDistance;

        if (inRange != playerInRange)
        {
            playerInRange = inRange;
            if (interactPrompt != null) interactPrompt.SetActive(inRange);
        }

        if (playerInRange && Input.GetKeyDown(KeyCode.F))
        {
            shopUI.Open();
        }
    }
}
