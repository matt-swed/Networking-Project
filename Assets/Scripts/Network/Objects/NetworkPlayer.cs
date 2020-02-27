using DarkRift;
using DarkRift.Client.Unity;
using DarkRift.Server;
using DarkRift.Server.Unity;
using System.Collections.Generic;
using UnityEngine;


public class NetworkPlayer : NetworkObject
{
    const byte MOVEMENT_TAG = 1;

    [SerializeField]
    [Tooltip("The distance we can move before we send a position update.")]
    float moveDistance = 0.05f;

    public UnityClient Client { get; set; }  //Reference of the client

    Vector3 lastPosition;                   //A vector to hold the last position of the object

    public int clientTick = -1;             //A variable to keep track of timing; may not be necessary

    public Rigidbody rigidbodyReference;    //A reference to the rigidbody, to get velocity data

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

                lastPosition = transform.position;
            }
        }
    }
    private void SendBallPositionToClients()
    {
        //Create the message
        PlayerSyncMessageModel bouncyBallPositionMessageData = new PlayerSyncMessageModel
        {
            networkID = base.id,
            serverTick = 0,
            position = rigidbodyReference.transform.position,
            velocity = rigidbodyReference.velocity
        };

        //create the message
        using (Message m = Message.Create(
            NetworkTags.InGame.REP_SYNC_POS,        //Tag
            bouncyBallPositionMessageData)                  //Data
        )    
        {
            Client.SendMessage(m, SendMode.Reliable);
        }
    }
}

