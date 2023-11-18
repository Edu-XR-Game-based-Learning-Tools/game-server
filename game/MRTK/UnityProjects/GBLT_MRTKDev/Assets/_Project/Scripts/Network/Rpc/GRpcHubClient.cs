using Cysharp.Threading.Tasks;
using Grpc.Core;
using MagicOnion;
using MagicOnion.Client;
using System;
using System.Collections.Generic;
using System.Text;

namespace Core.Network
{
    public interface IGRpcHubClient
    {
        UniTask<THub> Subscribe<THub, THubReceiver>(THubReceiver receiver, bool requiredAuthentication = true) where THub : IStreamingHub<THub, THubReceiver>;

        void Dispose();
    }

    public class GRpcHubClient : IGRpcHubClient
    {
        private GrpcChannelx _channel;
        private Dictionary<Type, IStreamingHubMarker> _clientCache;
        private readonly GRpcAuthenticationFilter _gRpcAuthenticationFilter;
        private readonly UserAuthentication _userAuthenticationData;
        private readonly EndPointSwitcher _endPointSwitcher;

        private IClientFilter[] _clientFilter;

        public GRpcHubClient(
            GRpcAuthenticationFilter gRpcAuthenticationFilter,
            EndPointSwitcher endPointSwitcher, UserAuthentication userAuthenticationData)
        {
            _clientCache = new Dictionary<Type, IStreamingHubMarker>();
            _gRpcAuthenticationFilter = gRpcAuthenticationFilter;

            _clientFilter = new[] { _gRpcAuthenticationFilter };
            _endPointSwitcher = endPointSwitcher;
            _userAuthenticationData = userAuthenticationData;
        }

        public async UniTask<THub> Subscribe<THub, THubReceiver>(THubReceiver receiver, bool requiredAuthentication = true) where THub : IStreamingHub<THub, THubReceiver>
        {
            THub client;
            CheckNetworkSetting();

            if (!GetClientHub(typeof(THub), out IStreamingHubMarker clientMaker))
            {
                if (requiredAuthentication)
                {
                    CallOptions authenOption = GetAuthenticationData();
                    client = await StreamingHubClient.ConnectAsync<THub, THubReceiver>(_channel, receiver, option: authenOption);
                }
                else
                    client = await StreamingHubClient.ConnectAsync<THub, THubReceiver>(_channel, receiver);

                CheckClientHub<THub, THubReceiver>(client);
            }
            else
                client = (THub)clientMaker;

            return client;
        }

        private void CheckNetworkSetting()
        {
            if (_channel == null || !_channel.IsConnected)
                _channel = GrpcChannelx.ForAddress(_endPointSwitcher.HubEndpoint);
        }

        private bool GetClientHub(Type key, out IStreamingHubMarker value)
        {
            return _clientCache.TryGetValue(key, out value);
        }

        private void CheckClientHub<THub, THubReceiver>(THub clientHub) where THub : IStreamingHub<THub, THubReceiver>
        {
            if (_clientCache.ContainsKey(clientHub.GetType()))
                _clientCache[clientHub.GetType()] = clientHub;
            else
                _clientCache.Add(clientHub.GetType(), clientHub);
        }

        private CallOptions GetAuthenticationData()
        {
            CheckAuthenticationData();
            CallOptions option = new CallOptions().WithHeaders(new Metadata()
            {
                { "auth-token-bin", Encoding.ASCII.GetBytes(_userAuthenticationData.Token) }
            });
            return option;
        }

        private void CheckAuthenticationData()
        {
            if (_userAuthenticationData.IsExpired)
                throw new UnauthorizedAccessException("GRpcHubClient: Token is expired");
        }

        public void Dispose()
        {
            _channel.Dispose();
        }
    }
}
