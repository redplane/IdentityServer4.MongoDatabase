using NCrontab;

namespace Redplane.IdentityServer4.MongoDatabase.Models
{
    internal class AuthenticationAdapterSettings
    {
        #region Properties

        private readonly string _defaultAccessTokenCleanupCronJob = "*/30 * * * *";


        #endregion

        #region Accessors

        /// <summary>
        /// Parsed cron schedule.
        /// </summary>
        public CrontabSchedule CleanupJobSchedule { get; private set; } = null!;

        #endregion

        #region Constructor

        public AuthenticationAdapterSettings()
        {
            UpdateAccessTokenCleanupCronJob(_defaultAccessTokenCleanupCronJob, false);
        }

        #endregion

        #region Methods

        public void UpdateAccessTokenCleanupCronJob(string accessTokenCleanupCronJob, bool shouldValueFallback)
        {
            try
            {
                CleanupJobSchedule = CrontabSchedule.Parse(accessTokenCleanupCronJob);
            }
            catch
            {
                if (!shouldValueFallback)
                    throw;
            }
        }

        #endregion
    }
}