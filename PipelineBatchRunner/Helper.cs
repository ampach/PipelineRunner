using System;
using System.Linq;
using Sitecore.Data.Fields;
using Sitecore.Data.Items;
using Sitecore.Data.Managers;
using Sitecore.Data.Templates;
using Sitecore.DataExchange.Extensions;
using Sitecore.DataExchange.Local.Extensions;
using Sitecore.DataExchange.Models;

namespace PipelineBatchRunner
{
    public class Helper
    {
        public static PipelineBatch GetPipelineBatch(Item contextItem)
        {
            var itemModel = contextItem?.GetItemModel();

            var converter = itemModel?.GetConverter<PipelineBatch>(Sitecore.DataExchange.Context.ItemModelRepository);

            if (converter == null) return null;

            var convertResult = converter.Convert(itemModel);

            return convertResult.WasConverted ? convertResult.ConvertedValue : null;
        }

        public static bool IsItemEnabled(Item contextItem)
        {
            return IsChecked(contextItem, "Enabled");
        }

        public static bool IsChecked(Item item, string fieldName)
        {
            CheckboxField checkboxField = item.Fields.FirstOrDefault(f => f.Name == fieldName);
            return checkboxField != null && checkboxField.Checked;
        }

        public static bool IsDerived(Item item, Guid templateId)
        {
            return IsDerived(TemplateManager.GetTemplate(item), templateId);
        }

        public static bool IsPipelineBatchItem(Item contextItem)
        {
            var template = TemplateManager.GetTemplate(contextItem);

            return IsDerived(template, Sitecore.DataExchange.Local.TemplateIDs.BasePipelineBatchTemplate.Guid);
        }

        public static bool IsDerived(Template template, Guid templateId)
        {
            return template.ID.Guid == templateId || template.GetBaseTemplates().Any(baseTemplate => IsDerived(baseTemplate, templateId));
        }
    }
}