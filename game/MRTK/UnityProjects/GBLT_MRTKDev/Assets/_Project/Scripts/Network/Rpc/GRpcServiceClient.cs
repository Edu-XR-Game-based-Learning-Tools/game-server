using Core.Business;
using Core.EventSignal;
using MagicOnion;
using MagicOnion.Client;
using MessagePipe;
using System;
using System.Collections.Generic;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Core.Network
{
    public interface IGRpcServiceClient
    {
        GrpcChannelx GetAliveChannel();

        TService Command<TService>() where TService : IService<TService>;

        TService CreateServiceWithFilter<TService>(IClientFilter[] filters) where TService : IService<TService>;

        void Dispose();
    }

    public class GRpcServiceClient : IGRpcServiceClient, IInitializable, IDisposable
    {
        private GrpcChannelx _channel;

        private Dictionary<Type, IServiceMarker> _serviceCache;
        private readonly EndPointSwitcher _endPointSwitcher;

        [Inject]
        private readonly ISubscriber<GameScreenChangeSignal> _gameScreenChangeSubscriber;

        private DisposableBagBuilder _disposableBagBuilder;

        public GRpcServiceClient(
            EndPointSwitcher endPointSwitcher)
        {
            _endPointSwitcher = endPointSwitcher;
        }

        public TService Command<TService>() where TService : IService<TService>
        {
            TService service;
            Type key = typeof(TService);

            if (!_serviceCache.TryGetValue(key, out IServiceMarker serviceMarker))
                service = CreateNewAndCache<TService>(key);
            else
                service = (TService)serviceMarker;

            return service;
        }

        public TService CreateServiceWithFilter<TService>(IClientFilter[] filters)
            where TService : IService<TService>
        {
            GrpcChannelx channel = GetAliveChannel();
            TService service = MagicOnionClient.Create<TService>(channel, filters);
            return service;
        }

        private TService CreateNewAndCache<TService>(Type key)
            where TService : IService<TService>
        {
            GrpcChannelx channel = GetAliveChannel();
            TService service = MagicOnionClient.Create<TService>(channel);
            _serviceCache.Add(key, service);
            return service;
        }

        private void EstablishServicesConnection()
        {
            //Initial channel and establish service connection.
            _serviceCache = new Dictionary<Type, IServiceMarker>();
            _channel = GetAliveChannel();
        }

        public GrpcChannelx GetAliveChannel()
        {
            if (_channel == null || !_channel.IsConnected)
            {
                Debug.Log($"GRpcServiceClient: GetAliveChannel and connect to the endpoint: {_endPointSwitcher.HubEndpoint}");
                _channel = GrpcChannelx.ForAddress(_endPointSwitcher.HubEndpoint);
            }

            return _channel;
        }

        private void OnStartSession(GameScreenChangeSignal e)
        {
            if (e.Current == ScreenName.Restart)
                Dispose();

            if (e.Current == ScreenName.SessionStart)
                EstablishServicesConnection();
        }

        public void Initialize()
        {
            _disposableBagBuilder = DisposableBag.CreateBuilder();
            _gameScreenChangeSubscriber.Subscribe(OnStartSession).AddTo(_disposableBagBuilder);
        }

        public void Dispose()
        {
            _disposableBagBuilder?.Build().Dispose();
            if (_channel != null)
            {
                _channel.Dispose();
                _channel = null;
            }

            if (_serviceCache != null)
                _serviceCache.Clear();
        }
    }
}
