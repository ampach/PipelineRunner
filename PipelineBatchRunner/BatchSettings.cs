using System;
using System.Collections.Generic;
using Sitecore.DataExchange.Loggers;
using Sitecore.DataExchange.Models;
using Sitecore.DataExchange.Plugins;
using Sitecore.DataExchange.VerificationLog;

namespace PipelineBatchRunner
{
    public class BatchSettings
    {
        public BatchSettings()
        {
            TelemetryEnabled = false;
            IsIncludeStackTraceForExceptions = true;
            SupportedModes = new List<string>();
            LogLevels = new List<LogLevel>
            {
                LogLevel.Debug,
                LogLevel.Error,
                LogLevel.Fatal,
                LogLevel.Info,
                LogLevel.Warn
            };
            VerificationLogSettings = new VerificationLogSettings
            {
                SaveJson = false,
                VerificationEnabled = false,
                VerificationLog = null
            };
        }

        public bool TelemetryEnabled { get; set; }
        public bool IsIncludeStackTraceForExceptions { get; set; }
        public VerificationLogSettings VerificationLogSettings { get; set; }
        public List<string> SupportedModes { get; set; }

        public List<LogLevel> LogLevels { get; set; }

        public void ApplySettings(PipelineBatch pipelineBatch)
        {
            var telemetryPlugin = new TelemetryActivitySettings
            {
                Enabled = TelemetryEnabled
            };
            pipelineBatch.AddPlugin(telemetryPlugin);

            var supportedModesPlugin2 = new MultiModeSupportSettings
            {
                SupportedModes = new List<string>()
            };
            pipelineBatch.AddPlugin(supportedModesPlugin2);

            var pipelineBatchSummary = new PipelineBatchSummary()
            {
                IncludeStackTraceForExceptions = IsIncludeStackTraceForExceptions
            };

            foreach (var logLevel in LogLevels)
            {
                pipelineBatchSummary.LogLevels.Add(logLevel);
            }
            pipelineBatch.AddPlugin(pipelineBatchSummary);

            SitecoreItemSettings newPlugin = new SitecoreItemSettings()
            {
                ItemId = Guid.Parse(pipelineBatch.Identifier)
            };
            pipelineBatch.AddPlugin(newPlugin);
            pipelineBatch.AddPlugin(VerificationLogSettings);
        }
    }
}