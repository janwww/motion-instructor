
using System.Collections;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.WSA;

public class HolographicRemotingScript : MonoBehaviour
{
    public string hololensIP;
    private bool _connected;

    private void Start()
    {
        if (HolographicRemoting.ConnectionState != HolographicStreamerConnectionState.Connected)
        {
            Debug.Log("Connecting...");
            HolographicRemoting.Connect(hololensIP, 99999, RemoteDeviceVersion.V1);
        }
    }

    private void Update()
    {
        if (!_connected && HolographicRemoting.ConnectionState == HolographicStreamerConnectionState.Connected)
        {
            _connected = true; // run a single time upon connection established
            StartCoroutine(LoadingWindowsMrWrapper());
        };
    }

    void OnApplicationQuit()
    {
        HolographicRemoting.Disconnect();
        Debug.Log("Was Disconnected");
        Debug.Log("Application ending after " + Time.time + " seconds");
    }
    void OnDisable()
    {
        HolographicRemoting.Disconnect();
        Debug.Log("Was Disconnected");
    }

    private IEnumerator LoadingWindowsMrWrapper()
    {
        yield return new WaitForSeconds(1);
        StartCoroutine(LoadDevice("WindowsMR"));
    }
    private static IEnumerator LoadDevice(string newDevice)
    {
        XRSettings.LoadDeviceByName(newDevice);
        yield return null;
        XRSettings.enabled = true;
    }
}