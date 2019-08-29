using System;
using Sitecore.DataExchange.Contexts;
using Sitecore.DataExchange.Models;
using Sitecore.DataExchange.Processors;
using Sitecore.DataExchange.Processors.PipelineBatches;
using Sitecore.Services.Core.Diagnostics;

namespace PipelineBatchRunner
{
    public class VirtualPipelineBatchProcessor : BasePipelineBatchProcessor
    {
        protected ILogger _logger { get; set; }

        protected override void OnStartingProcessing(PipelineBatch pipelineBatch, PipelineBatchContext pipelineBatchContext, ILogger logger)
        {
            _logger = logger;
            base.OnStartingProcessing(pipelineBatch, pipelineBatchContext, logger);
        }

        protected override void OnStarting(PipelineBatchProcessorEventArgs e)
        {
            if (_logger != null)
                this.Log(new Action<string>(_logger.Error), e.PipelineBatch, e.PipelineBatchContext, "Pipeline processing Requested At:" + DateTime.UtcNow, Array.Empty<string>());
        }

        protected override void OnFinished(PipelineBatchProcessorEventArgs e)
        {
            if (_logger != null)
                this.Log(new Action<string>(_logger.Error), e.PipelineBatch, e.PipelineBatchContext, "Pipeline processing Last Run Finished At:" + DateTime.UtcNow, Array.Empty<string>());
        }

        protected override void OnAborted(PipelineBatchProcessorEventArgs e)
        {
            if (_logger != null)
                this.Log(new Action<string>(_logger.Error), e.PipelineBatch, e.PipelineBatchContext, "Pipeline processing Last Run Finished At:" + DateTime.UtcNow, Array.Empty<string>());
        }
    }
}