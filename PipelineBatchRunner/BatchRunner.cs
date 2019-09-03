using System.Collections.Generic;
using Sitecore.Data;
using Sitecore.DataExchange;
using Sitecore.DataExchange.Contexts;
using Sitecore.DataExchange.Local.Runners;
using Sitecore.DataExchange.Models;
using Sitecore.DataExchange.Plugins;
using Sitecore.DataExchange.Runners;
using Sitecore.Jobs;
using Sitecore.Security.Accounts;
using Sitecore.Services.Core.Extensions;

namespace PipelineBatchRunner
{
    public class BatchRunner
    {
        private bool _isRunnerSet;
        private static IPipelineBatchRunner<Job> _runner;

        public void RunVirtualBatch(List<ID> pipelinesToRun, IPlugin[] plugins)
        {
            var virtualBatch = VirtualPipelineBatchBuilder.GetVirtualPipelineBatch(pipelinesToRun, new BatchSettings());
            if (virtualBatch == null)
                return;

            Run(virtualBatch, plugins);

        }

        public void RunStandardBatch(ID batchItemId, IPlugin[] plugins)
        {
            var batchItem = Sitecore.Configuration.Factory.GetDatabase("master")
                .GetItem(batchItemId);

            if (PipelineBatchRunner == null || batchItem == null || !Helper.IsPipelineBatchItem(batchItem) || !Helper.IsItemEnabled(batchItem))
                return;

            var pipelineBatch = Helper.GetPipelineBatch(batchItem);

            Run(pipelineBatch, plugins);
        }

        protected virtual void Run(PipelineBatch batch, IPlugin[] plugins)
        {
            if (batch == null)
                return;

            var pipelineBatchRunner = (InProcessPipelineBatchRunner)PipelineBatchRunner;

            if (pipelineBatchRunner != null && (!pipelineBatchRunner.IsRunningRemotely(batch) || !PipelineBatchRunner.IsRunning(batch.Identifier)))
            {
                const string category = "Data Exchange";

                var parameters = new object[]
                {
                    batch,
                    GetUser(),
                    plugins
                };
                var options = new JobOptions(batch.Name, category, "Data Exchange Framework", this, "RunPipelineBatch", parameters);
                PipelineBatchRunner.CurrentProcesses[batch.Identifier] = JobManager.Start(options);
            }

        }

        public void RunPipelineBatch(PipelineBatch pipelineBatch, User currentUser, IPlugin[] plugins)
        {
            if (PipelineBatchRunner == null)
                return;

            if (currentUser == null)
                currentUser = Sitecore.Context.User;

            using (new UserSwitcher(currentUser))
            {
                var pipelineBatchContext = GetPipelineBatchContext();
                plugins.ForEach(q => pipelineBatchContext.AddPlugin(q));
                PipelineBatchRunner.Run(pipelineBatch, pipelineBatchContext);
            }
        }
        protected IPipelineBatchRunner<Job> PipelineBatchRunner
        {
            get
            {
                if (!_isRunnerSet && _runner == null)
                {
                    var pipelineBatchRunner = new InProcessPipelineBatchRunner();
                    var logger = Sitecore.DataExchange.Context.Logger;
                    pipelineBatchRunner.Logger = logger;
                    _runner = pipelineBatchRunner;
                    ((InProcessPipelineBatchRunner)_runner).SubscribeRemoteEvents();
                    _runner.Started += PipelineBatchRunnerOnStarted;
                    _runner.Finished += PipelineBatchRunnerOnFinished;
                }
                return _runner;
            }
            set
            {
                _isRunnerSet = true;
                _runner = value;
            }
        }
        protected virtual PipelineBatchContext GetPipelineBatchContext()
        {
            var pipelineBatchContext = new PipelineBatchContext();

            var newPlugin = new PipelineBatchRuntimeSettings
            {
                ShouldPersistSummary = true,
                PipelineBatchMode = string.Empty
            };

            pipelineBatchContext.AddPlugin(newPlugin);

            return pipelineBatchContext;
        }

        protected virtual User GetUser()
        {
            return Sitecore.Context.User;
        }

        protected virtual void PipelineBatchRunnerOnFinished(object sender, PipelineBatchRunnerEventArgs pipelineBatchRunnerEventArgs)
        {
        }

        protected virtual void PipelineBatchRunnerOnStarted(object sender, PipelineBatchRunnerEventArgs pipelineBatchRunnerEventArgs)
        {
        }
    }
}
