using System;
using System.Collections.Generic;
using System.Linq;
using Sitecore.Common;
using Sitecore.Data;
using Sitecore.Data.Items;
using Sitecore.DataExchange.Extensions;
using Sitecore.DataExchange.Local.Extensions;
using Sitecore.DataExchange.Loggers;
using Sitecore.DataExchange.Models;
using Sitecore.DataExchange.Plugins;
using Sitecore.DataExchange.Repositories;
using Sitecore.DataExchange.VerificationLog;
using Sitecore.Services.Core.Model;
using System.Security.Cryptography;
using System.Text;

namespace PipelineBatchRunner
{
    public class VirtualPipelineBatchBuilder
    {
        public const string TenantTemplateId = "{327A381B-59F8-4E88-B331-BEBC7BD87E4E}";

        public static PipelineBatch GetVirtualPipelineBatch(List<ID> pipelinesToRun, BatchSettings settings)
        {
            var db = Sitecore.Configuration.Factory.GetDatabase("master");
            return GetVirtualPipelineBatch(pipelinesToRun.Select(w => db.GetItem(w)).Where(q => q != null).Select(q => q.GetItemModel()).ToList(), settings);
        }
        public static PipelineBatch GetVirtualPipelineBatch(List<Item> pipelinesToRun, BatchSettings settings)
        {
            return GetVirtualPipelineBatch(pipelinesToRun.Where(q => q != null).Select(q => q.GetItemModel()).ToList(), settings);
        }

        public static PipelineBatch GetVirtualPipelineBatch(List<ItemModel> pipelinesToRun, BatchSettings settings)
        {
            if (pipelinesToRun == null)
                return null;

            var db = Sitecore.Configuration.Factory.GetDatabase("master");

            var virtualBatch = new PipelineBatch();
            virtualBatch.Enabled = true;

            var hash = GetHash(pipelinesToRun.Select(q => q.GetItemId().ToID().ToShortID().ToString())
                .Aggregate((f, s) => f + "|" + s));

            virtualBatch.Identifier = hash;
            virtualBatch.PipelineBatchProcessor = new VirtualPipelineBatchProcessor();
            virtualBatch.Tenant = GetTenant(db.GetItem(pipelinesToRun.First().GetItemId().ToID()));

            settings.ApplySettings(virtualBatch);
            
            foreach (var pipeline in pipelinesToRun)
            {
                if(pipeline == null) 
                    continue;

                var pipelineModel = GetPipeline(pipeline);

                virtualBatch.Pipelines.Add(pipelineModel);
            }

            if (!virtualBatch.Pipelines.Any())
                return null;

            virtualBatch.Name = "VirtualBatch (" + virtualBatch.Pipelines.Select(q => q.Name.Replace(" ", ".")).Aggregate((q,w) => q + "|" + w) + ")";
            

            return virtualBatch;

        }

        public static Tenant GetTenant(Item item)
        {

            if (item != null)
            {
                var tenantTemplateId = new ID(TenantTemplateId);
                var tenantItem = item.Axes.GetAncestors().Reverse().FirstOrDefault(x => x.TemplateID == tenantTemplateId);

                var tenantModel = tenantItem?.GetItemModel();

                var converter = tenantModel?.GetConverter<Tenant>(Sitecore.DataExchange.Context.ItemModelRepository);
                if (converter == null) return null;

                var convertResult = converter.Convert(tenantModel);

                return convertResult.WasConverted ? convertResult.ConvertedValue : null;
            }

            return null;
        }

        protected static Pipeline GetPipeline(ItemModel itemModel)
        {
            var converter = itemModel?.GetConverter<Pipeline>(Sitecore.DataExchange.Context.ItemModelRepository);

            if (converter == null) return null;

            var convertResult = converter.Convert(itemModel);

            return convertResult.WasConverted ? convertResult.ConvertedValue : null;
        }

        public static string GetHash(string inputString)
        {
            HashAlgorithm algorithm = SHA256.Create();
            var bytes = algorithm.ComputeHash(Encoding.UTF8.GetBytes(inputString));
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < bytes.Length; i++)
            {
                sb.Append(bytes[i].ToString("X2"));
            }
            return sb.ToString();
        }
    }
}