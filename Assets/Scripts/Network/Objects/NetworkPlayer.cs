using DarkRift;
using DarkRift.Client.Unity;
using DarkRift.Server;
using DarkRift.Server.Unity;
using UnityEngine;




//Notes: Need to Rewrite the message writer to fit the BouncyBallSyncMessageModel. Also, I need to rename the BouncyBallSyncMessageModel




public class NetworkPlayer : NetworkObject
{
    const byte MOVEMENT_TAG = 1;

    [SerializeField]
    [Tooltip("The distance we can move before we send a position update.")]
    float moveDistance = 0.05f;

    public UnityClient Client { get; set; }

    Vector3 lastPosition;

    public int clientTick = -1;

    public Rigidbody rigidbodyReference;

    public override void Start()
    {
        base.Start();

        rigidbodyReference = GetComponent<Rigidbody>();

        if (!Equals(ClientManager.instance, null))
        {
            lastPosition = transform.position;
            Client = GameObject.Find("ClientManager").GetComponent<UnityClient>();
        }
    }

    private void FixedUpdate()
    {
        if (!Equals(ClientManager.instance, null))
        {
            clientTick++;

            if (Vector3.Distance(lastPosition, transform.position) > moveDistance)
            {
                SendBallPositionToClients();
                Debug.Log("Message sent from " + id);

                lastPosition = transform.position;
            }
        }
    }
    private void SendBallPositionToClients()
    {
        //Create the message
        BouncyBallSyncMessageModel bouncyBallPositionMessageData = new BouncyBallSyncMessageModel
        {
            networkID = base.id,
            serverTick = clientTick,
            position = rigidbodyReference.transform.position,
            velocity = rigidbodyReference.velocity
        };

        //create the message 
        using (Message m = Message.Create(
            NetworkTags.InGame.REP_SYNC_POS,        //Tag
            bouncyBallPositionMessageData)                  //Data
        )
        {
            foreach (IClient client in GameServerManager.instance.serverReference.Server.ClientManager.GetAllClients())
            {
                client.SendMessage(m, SendMode.Reliable);
            }
        }
    }

    public void ClientConnected()
    {

    }
}

