using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DarkRift.Server.Unity;
using DarkRift;
using DarkRift.Server;
using UnityEngine.SceneManagement;
using System;
using Utilities;
using System.Linq;

public class GameServerManager : MonoBehaviourSingletonPersistent<GameServerManager>
{
    /// <summary>
    /// List of connected clients
    /// </summary>
    public List<int> clientsId;

    /// <summary>
    /// Reference to the DarkRift server
    /// </summary>
    public XmlUnityServer serverReference;

    /// <summary>
    /// List of objects handled by the server
    /// </summary>
    public List<NetworkObject> networkObjects;

    public List<IClient> clients;

    public NetworkObject playerSelf;
    public NetworkObject playerRep;

    /// <summary>
    /// Last tick received from the server
    /// </summary>
    public int currentTick = -1;

    // Start is called before the first frame update
    void Start()
    {
        clientsId = new List<int>();
        clients = new List<IClient>();

        playerSelf.id = 1;                  //id used to uniquely identify the controllable player from the other controllable players
        playerRep.id = 1;                 //id used to match the representation of the player with its corresponding controllable player on that player's client

        serverReference = GetComponent<XmlUnityServer>();

        //////////////////
        /// Events subscription
        serverReference.Server.ClientManager.ClientConnected += ClientConnected;
        serverReference.Server.ClientManager.ClientDisconnected += ClientDisconnected;

        SceneManager.LoadScene("MainGameScene", LoadSceneMode.Additive);

        networkObjects = new List<NetworkObject>();
    }

    void FixedUpdate()
    {
        currentTick++;
    }

    // Update is called once per frame
    void Update()
    {
        
    } 

    #region Server events
    /// <summary>
    /// When a client connects to the DarkRift server
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void ClientConnected(object sender, ClientConnectedEventArgs e)
    {
        clients.Add(e.Client);

        //Send all objects to spawn
        //////////////////////////////////prototyping///////////////////////////////////////////////////
        SendObjectToSpawnTo(playerSelf, e.Client);                      //send a controllable player to the client
        playerSelf.id++;                                                //

        SendObjectToClient(playerRep, e.Client);      //send a representation of a controllable player to the client for each other client
        
        SendObjectToOtherClients(playerRep, e.Client);            //send a representation of the controllable player of this client to each other client
        playerRep.id++;
        
        e.Client.MessageReceived += MessageReceived;            //Allows the server to receive messages from this client

        ////////////////////////////////////////////////////////////////////////////////////////////////
    }

    /// <summary>
    /// When a client disconnects to the DarkRift server
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void ClientDisconnected(object sender, ClientDisconnectedEventArgs e)
    {
        clients.Remove(e.Client);
    }
    #endregion

    #region Implementation

    /// <summary>
    /// Use this function to add a network object that must be handle by the server
    /// </summary>
    /// <param name="pNetworkObject"></param>
    public void RegisterNetworkObject(NetworkObject pNetworkObject)
    {
        //Add the object to the list
        networkObjects.Add(pNetworkObject);
    }

    /// <summary>
    /// Send a message to the client to spawn an object into its scene
    /// </summary>
    /// <param name="pClient"></param>
    public void SendObjectToSpawnTo(NetworkObject pNetworkObject, IClient pClient)
    {
        //Spawn data to send
        SpawnMessageModel spawnMessageData = new SpawnMessageModel
        {
            networkID = pNetworkObject.id,
            resourceID = pNetworkObject.resourceId,
            x = pNetworkObject.gameObject.transform.position.x,
            y = pNetworkObject.gameObject.transform.position.y
        };

        //create the message 
        using (Message m = Message.Create(
            NetworkTags.InGame.SPAWN_OBJECT,                //Tag
            spawnMessageData)                               //Data
        )
        {
            //Send the message in TCP mode (Reliable)
            pClient.SendMessage(m, SendMode.Reliable);
        }
    }

    /// <summary>
    /// sends a message to the client
    /// </summary>
    /// <param name="pNetworkObject"></param>
    /// <param name="pClient"></param>
    public void SendObjectToClient(NetworkObject pNetworkObject, IClient pClient)
    {
        int i = 1;

        foreach (IClient client in clients.Where(x => x != pClient))
        {
            //Spawn data to send
            SpawnMessageModel spawnMessageData = new SpawnMessageModel
            {
                networkID = i,
                resourceID = pNetworkObject.resourceId,
                x = pNetworkObject.gameObject.transform.position.x,
                y = pNetworkObject.gameObject.transform.position.y
            };

            //create the message 
            using (Message m = Message.Create(
                NetworkTags.InGame.SPAWN_OBJECT,                //Tag
                spawnMessageData)                               //Data
            )
            {
                //Send the message in TCP mode (Reliable)
                pClient.SendMessage(m, SendMode.Reliable);

                i++;
            }
        }
    }

    /// <summary>
    /// Send a message with all objects to spawn
    /// </summary>
    /// <param name="pClient"></param>
    public void SendAllObjectsToSpawnTo(IClient pClient)
    {
        foreach (NetworkObject networkObject in networkObjects)
            SendObjectToSpawnTo(networkObject, pClient);
    }


    /// <summary>
    /// send a message to spawn an object to every other client
    /// </summary>
    /// <param name="pNetworkObject"></param>
    /// <param name="pClient"></param>
    public void SendObjectToOtherClients(NetworkObject pNetworkObject, IClient pClient)
    {
        foreach (IClient client in clients.Where(x => x != pClient))
            SendObjectToSpawnTo(pNetworkObject, client);
    }

//////////////////////////////////prototyping//////////////////////////////////////////////////////////////
/// <summary>
/// This function recieves a movement message from a client and sends it to the rest of the clients.
/// </summary>
/// <param name="sender"></param>
/// <param name="e"></param>
    void MessageReceived(object sender, MessageReceivedEventArgs e)
    {
        using (Message message = e.GetMessage() as Message)
        {
            if (message.Tag == NetworkTags.InGame.REP_SYNC_POS)
            {
                //Get message data
                PlayerSyncMessageModel syncMessage = e.GetMessage().Deserialize<PlayerSyncMessageModel>();

                //for every client other than the one that sent the message
                foreach (IClient client in clients.Where(x => x != e.Client))
                {
                    PlayerSyncMessageModel movementMessageData = new PlayerSyncMessageModel
                    {
                        networkID = syncMessage.networkID,
                        serverTick = currentTick,
                        position = syncMessage.position,
                        velocity = syncMessage.velocity
                    };

                    //create the message 
                    using (Message m = Message.Create(
                        NetworkTags.InGame.REP_SYNC_POS,                //Tag
                        movementMessageData)                               //Data
                    )
                    {
                        //Send the message in TCP mode (Reliable)
                        client.SendMessage(m, SendMode.Reliable);
                    }
                }
            }
        }
    }
////////////////////////////////////////////////////////////////////////////////////////////////
    #endregion
}
