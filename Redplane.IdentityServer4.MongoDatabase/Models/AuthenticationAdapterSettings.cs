using NCrontab;

namespace Redplane.IdentityServer4.MongoDatabase.Models
{
    internal class AuthenticationAdapterSettings
    {
        #region Properties

        private readonly string _scheduleExpression = "*/30 * * * *";

        #endregion

        #region Constructor

        public AuthenticationAdapterSettings()
        {
            UpdateSchedule(_scheduleExpression, false);
        }

        #endregion

        #region Accessors

        /// <summary>
        ///     Parsed cron schedule.
        /// </summary>
        public CrontabSchedule CleanupJobSchedule { get; private set; }

        #endregion

        #region Methods

        /// <summary>
        ///     Update schedule expression
        /// </summary>
        /// <param name="accessTokenCleanupCronJob"></param>
        /// <param name="shouldValueFallback"></param>
        /// <returns></returns>
        public AuthenticationAdapterSettings UpdateSchedule(string accessTokenCleanupCronJob, bool shouldValueFallback)
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

            return this;
        }

        #endregion
    }
}