//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Threading;
//using Microsoft.Extensions.Configuration;
//using Microsoft.Extensions.Primitives;
//using Newtonsoft.Json.Linq;
//using Weikio.ApiFramework.Core.Endpoints;
//
//namespace Weikio.ApiFramework.Core.Configuration
//{
//    public class ConfigurationMonitor : IDisposable
//    {
//        private readonly IConfiguration _configuration;
//
//        private readonly EndpointManager _manager;
//
//        private IDisposable _changeToken = null;
//
//        private int _latestChangeHashCode = 0;
//
//        private readonly Dictionary<string, int> _endpointConfigHashCodes = new Dictionary<string, int>();
//
//        private ConfigurationReloadToken _reloadToken;
//
//        public static CancellationTokenSource TokenSource { get; set; }
//
//        public IChangeToken GetReloadToken()
//        {
//            return _reloadToken;
//        }
//
//        public ConfigurationMonitor(IConfiguration configuration, EndpointManager manager)
//        {
//            _configuration = configuration;
//            _manager = manager;
//            _changeToken = ChangeToken.OnChange(configuration.GetReloadToken, ConfigChanged);
//            _reloadToken = new ConfigurationReloadToken();
//        }
//
//        public void SetEndpointConfigHashCode(IConfigurationSection config)
//        {
//            _endpointConfigHashCodes[config.Key] = CalculateHashCode(config);
//        }
//
//        public void RemoveEndpointConfigHashCode(string route)
//        {
//            _endpointConfigHashCodes.Remove(route);
//        }
//
//        public bool IsEndpointConfigChanged(string route, IConfiguration config)
//        {
//            if (!_endpointConfigHashCodes.TryGetValue(route, out var oldHashCode))
//            {
//                return true;
//            }
//
//            var newHashCode = CalculateHashCode(config);
//
//            return newHashCode != oldHashCode;
//        }
//
//        private void ConfigChanged()
//        {
//            var configHash = CalculateHashCode(_configuration);
//
//            if (configHash == _latestChangeHashCode)
//            {
//                return;
//            }
//
//            var endpointsSection = _configuration.GetSection("Endpoints");
//
//            if (endpointsSection == null)
//            {
//                return;
//            }
//
//            var endpointConfigsByRoute = endpointsSection.GetChildren()
//                .ToDictionary(e => e.Key);
//
//            var endpointsByRoute = _manager.Endpoints
//                .ToDictionary(p => p.Route);
//
//            var reloadRequired = false;
//
//            var removedEndpointRoutes = endpointsByRoute.Keys.Except(endpointConfigsByRoute.Keys).ToArray();
//
//            foreach (var route in removedEndpointRoutes)
//            {
//                var endpoint = endpointsByRoute[route];
//                _manager.RemoveEndpoint(endpoint);
//
//                RemoveEndpointConfigHashCode(route);
//
//                reloadRequired = true;
//            }
//
//            var addedEndpoints = endpointConfigsByRoute.Keys.Except(endpointsByRoute.Keys).ToArray();
//
//            foreach (var endpoint in addedEndpoints)
//            {
//                var endpointConfig = endpointConfigsByRoute[endpoint];
//                _manager.AddEndpoint(endpointConfig);
//
//                SetEndpointConfigHashCode(endpointConfig);
//
//                reloadRequired = true;
//            }
//
//            foreach (var key in endpointConfigsByRoute.Keys)
//            {
//                var endpointConfig = endpointConfigsByRoute[key];
//
//                if (IsEndpointConfigChanged(key, endpointConfig))
//                {
//                    var endpoint = endpointsByRoute[key];
//                    _manager.ReplaceEndpoint(endpoint, endpointConfig);
//
//                    SetEndpointConfigHashCode(endpointConfig);
//
//                    reloadRequired = true;
//                }
//            }
//
//            if (reloadRequired)
//            {
//                var token = _reloadToken;
//                _reloadToken = new ConfigurationReloadToken();
//
//                token.OnReload();
//            }
//
//            _latestChangeHashCode = configHash;
//        }
//
//        private static int CalculateHashCode(IConfiguration config)
//        {
//            return Serialize(config).ToString().GetHashCode();
//        }
//
//        private static JToken Serialize(IConfiguration config)
//        {
//            var obj = new JObject();
//
//            foreach (var child in config.GetChildren())
//            {
//                obj.Add(child.Key, Serialize(child));
//            }
//
//            if (!obj.HasValues && config is IConfigurationSection section)
//            {
//                return new JValue(section.Value);
//            }
//
//            return obj.ToString();
//        }
//
//        public void Dispose()
//        {
//            if (_changeToken == null)
//            {
//                return;
//            }
//
//            _changeToken.Dispose();
//            _changeToken = null;
//        }
//    }
//}


