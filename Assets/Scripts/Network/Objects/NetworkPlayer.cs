using DarkRift;
using DarkRift.Client.Unity;
using UnityEngine;

public class NetworkPlayer : NetworkObject
{
    const byte MOVEMENT_TAG = 1;

    [SerializeField]
    [Tooltip("The distance we can move before we send a position update.")]
    float moveDistance = 0.05f;

    public UnityClient Client { get; set; }

    Vector3 lastPosition;

    void Awake()
    {
        if (!Equals(ClientManager.instance, null))
        {
            lastPosition = transform.position;
            Client = GameObject.Find("ClientManager").GetComponent<UnityClient>();
        }
    }

    void Update()
    {
        if (!Equals(ClientManager.instance, null))
        {
            if (Vector3.Distance(lastPosition, transform.position) > moveDistance)
            {
                using (DarkRiftWriter writer = DarkRiftWriter.Create())
                {
                    writer.Write(transform.position.x);
                    writer.Write(transform.position.y);

                    using (Message message = Message.Create(NetworkTags.InGame.PLAYER_SYNC_POS, writer))
                        Client.SendMessage(message, SendMode.Unreliable);
                }

                lastPosition = transform.position;
            }
        }
    }

}