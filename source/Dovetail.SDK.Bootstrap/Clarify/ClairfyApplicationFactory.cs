using System;
using System.Collections.Specialized;
using System.Configuration;
using System.Linq;
using FChoice.Common.State;
using FChoice.Foundation;
using FChoice.Foundation.Clarify;

namespace Dovetail.SDK.Bootstrap.Clarify
{
    public class DovetailDatabaseSettings 
    {
    	public DovetailDatabaseSettings()
    	{
    		ApplicationUsername = "sa";
    	}

    	public string Type { get; set; }
        public string ConnectionString { get; set; }
        public double SessionTimeoutInMinutes { get; set; }
    	public string ApplicationUsername { get; set; }
    }

    public interface IClarifyApplicationFactory
    {
        IClarifyApplication Create();
    }

    public class ClarifyApplicationFactory : IClarifyApplicationFactory
    {
        private readonly DovetailDatabaseSettings _dovetailDatabaseSettings;
	    private readonly ILogger _logger;
	    private static readonly object SyncRoot = new object();

        public ClarifyApplicationFactory(DovetailDatabaseSettings dovetailDatabaseSettings, ILogger logger)
        {
	        _dovetailDatabaseSettings = dovetailDatabaseSettings;
	        _logger = logger;
        }

	    public IClarifyApplication Create()
        {
            if (FCApplication.IsInitialized)
            {
				_logger.LogDebug("Dovetail SDK already initialized");
				return ClarifyApplication.Instance;
            }
            lock (SyncRoot)
            {
                if (FCApplication.IsInitialized)
                {
					_logger.LogWarn("Dovetail SDK already initialized (In Sync Check)");
                    return ClarifyApplication.Instance;
                }

				_logger.LogInfo("Initializing Dovetail SDK");

                var configuration = GetDovetailSDKConfiguration(_dovetailDatabaseSettings);

                var application = ClarifyApplication.Initialize(configuration);

                setSessionDefaultTimeout(_dovetailDatabaseSettings);

                return application;
            }
        }

        public void setSessionDefaultTimeout(DovetailDatabaseSettings dovetailDatabaseSettings)
        {
	        var stateTimeoutTimespan = TimeSpan.FromMinutes(dovetailDatabaseSettings.SessionTimeoutInMinutes);

			_logger.LogDebug("Setting session time out to be {0} minutes long.", stateTimeoutTimespan);

	        StateManager.StateTimeout = stateTimeoutTimespan;
        }

	    public static NameValueCollection GetDovetailSDKConfiguration(DovetailDatabaseSettings dovetailDatabaseSettings)
        {
            var configuration = new NameValueCollection
			                    {
			                    	{"fchoice.dbtype", dovetailDatabaseSettings.Type},
			                    	{"fchoice.connectionstring", dovetailDatabaseSettings.ConnectionString},
			                    	{"fchoice.disableloginfromfcapp", "false"},
									{"fchoice.sessionpasswordrequired", "false"},
			                    	{"fchoice.nocachefile", "true"}
			                    };

            return MergeSDKSettings(configuration, ConfigurationManager.AppSettings);
        }

        public static NameValueCollection MergeSDKSettings(NameValueCollection target, NameValueCollection source)
        {
            var result = new NameValueCollection(target);

            foreach (var settingKey in source.AllKeys.Where(settingKey => settingKey.StartsWith("fchoice.")).Where(settingKey => target[settingKey] == null))
            {
                result.Add(settingKey, source[settingKey]);
            }

            return result;
        }
    }
}