using AsyncIO;
using NetMQ;
using NetMQ.Sockets;
using UnityEngine;

/// <summary>
///     Example of requester who only sends Hello. Very nice guy.
///     You can copy this class and modify Run() to suits your needs.
///     To use this class, you just instantiate, call Start() when you want to start and Stop() when you want to stop.
/// </summary>
public class BoneDataRequester : RunAbleThread
{
    /// <summary>
    ///     Request Hello message to server and receive message back. Do it 10 times.
    ///     Stop requesting when Running=false.
    /// </summary>
    public bool hasData = false;
    public bool inHasData = false;
    public bool lastHasData = false;
    public string message = null;
    private string msg;

    protected override void Run()
    {

        ForceDotNet.Force(); // this line is needed to prevent unity freeze after one use, not sure why yet
        using (RequestSocket client = new RequestSocket())
        {
            client.Connect("tcp://localhost:5555");

            while ((message != "--end--") && Running)
            {
                // ReceiveFrameString() blocks the thread until you receive the string, but TryReceiveFrameString()
                // do not block the thread, you can try commenting one and see what the other does, try to reason why
                // unity freezes when you use ReceiveFrameString() and play and stop the scene without running the server

                bool gotMessage = false;

                if (hasData != inHasData) hasData = inHasData;
                if (lastHasData != hasData) {
                    if (!hasData) {
                        Debug.Log("Sending Start");
                        client.SendFrame("--start--");
                        message = null;
                    }
                    lastHasData = hasData;
                }
                while (Running && !hasData)
                {
                    gotMessage = client.TryReceiveFrameString(out msg); // this returns true if it's successful
                    if (gotMessage) break;
                }
                if (gotMessage && (msg == "--end--")) {
                    Debug.Log("Got END");
                    break;
                }
                if (gotMessage && (msg != "--eof--")) {
                    message += msg;
                    client.SendFrame("--cont--");
                }
                if (gotMessage && (msg == "--eof--")) {
                    Debug.Log("Got EOF");
                    hasData = true;
                    inHasData = true;
                }
            }
            Debug.Log("Data receiving ended");
        }

        NetMQConfig.Cleanup(); // this line is needed to prevent unity freeze after one use, not sure why yet
    }
}
