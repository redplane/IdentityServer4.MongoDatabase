using System;
using NCrontab;

namespace IdentityServer4.MongoDbAdapter.Models
{
    public class AuthenticationAdapterSettings
    {
        #region Properties

        /// <summary>
        /// Access token cleanup cron job settings.
        /// </summary>
        private string _accessTokenCleanupCronJob = "* 30 * * * *";

        private CrontabSchedule _parsedCronSchedule;

        #endregion

        #region Accessors

        /// <summary>
        /// Cron job setting that should run to cleanup access token.
        /// </summary>
        public string AccessTokenCleanupCronJob
        {
            get => _accessTokenCleanupCronJob;
            set
            {
                try
                {
                    _parsedCronSchedule = CrontabSchedule.Parse(_accessTokenCleanupCronJob);
                    _accessTokenCleanupCronJob = value;
                }
                catch
                {
                    throw new ArgumentException($"Invalid setting for {nameof(AccessTokenCleanupCronJob)}");
                }
            }
        }

        /// <summary>
        /// Parsed cron schedule.
        /// </summary>
        public CrontabSchedule ParsedCronSchedule => _parsedCronSchedule;

        #endregion
    }
}