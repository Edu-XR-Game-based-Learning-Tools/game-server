using Grpc.Core;
using MagicOnion;
using MagicOnion.Client;
using MagicOnion.Unity;
using Shared.Network;
using UnityEngine;

public class TempScript : MonoBehaviour
{
    private async void TestGrpc()
    {
        var timerHub = new TimerHub();
        var channel = GrpcChannelx.ForAddress("http://192.168.1.191:5000");

        var serviceClient = MagicOnionClient.Create<IGenericService>(channel);

        int value = await timerHub.ConnectAsync(channel, 5, 15);
        Debug.Log($"SumAsync {value}");

        var item = await serviceClient.GetServerTime();
        Debug.Log($"GetServerTime {item}");
    }

    // Start is called before the first frame update
    private void Start()
    {
        TestGrpc();
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    public static void OnRuntimeInitialize()
    {
        // Initialize gRPC channel provider when the application is loaded.
        GrpcChannelProviderHost.Initialize(new DefaultGrpcChannelProvider(new[]
        {
        // send keepalive ping every 5 second, default is 2 hours
        new ChannelOption("grpc.keepalive_time_ms", 5000),
        // keepalive ping time out after 5 seconds, default is 20 seconds
        new ChannelOption("grpc.keepalive_timeout_ms", 5 * 1000),
    }));
    }
}
