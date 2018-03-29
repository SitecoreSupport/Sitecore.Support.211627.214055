using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Sitecore.Data;
using Sitecore.Data.Fields;
using Sitecore.Data.Items;
using Sitecore.Data.LanguageFallback;
using Sitecore.Data.Managers;
using Sitecore.Globalization;

namespace Sitecore.Support.Data.LanguageFallback
{
  public class LanguageFallbackFieldValuesProvider : Sitecore.Data.LanguageFallback.LanguageFallbackFieldValuesProvider
  {
    [NotNull]
    public override LanguageFallbackFieldValue GetLanguageFallbackValue(Field field, bool allowStandardValue)
    {
      var result = this.GetFallbackValuesFromCache(field, allowStandardValue);

      if (result != null)
      {
        return result;
      }

      Item fallbackItem = field.Item;
      Field currentField = null;
      string fieldValue = null;

      // it's necessary to track cyclic fallback
      var usedLanguages = new List<Language>(4);

      using (new EnforceVersionPresenceDisabler())
      using (new LanguageFallbackItemSwitcher(false))
      {
        do
        {
          usedLanguages.Add(fallbackItem.Language);

          var language = allowStandardValue ? fallbackItem.Language : fallbackItem.OriginalLanguage;
          var fallbackLang = LanguageFallbackManager.GetFallbackLanguage(language, fallbackItem.Database, fallbackItem.ID);
          if (fallbackLang != null && !string.IsNullOrEmpty(fallbackLang.Name) && !usedLanguages.Contains(fallbackLang))
          {
            fallbackItem = fallbackItem.Database.GetItem(fallbackItem.ID, fallbackLang, Sitecore.Data.Version.Latest);
          }
          else
          {
            fallbackItem = null;
          }

          if (fallbackItem == null || fallbackItem.RuntimeSettings.TemporaryVersion)
          {
            break;
          }

          currentField = fallbackItem.Fields[field.ID];
          fieldValue = currentField.GetValue(allowStandardValue, false, false);
        }
        while (fieldValue == null);
      }

      if (fieldValue != null)
      {
        ItemUri sourceUri = fallbackItem.Uri;
        bool containsStandardValue = currentField.ContainsStandardValue;
        result = new LanguageFallbackFieldValue(fieldValue, containsStandardValue, sourceUri.Language.Name);
      }
      else
      {
        result = new LanguageFallbackFieldValue(null, false, null);
      }

      this.AddLanguageFallbackValueToCache(field, allowStandardValue, result);

      return result;
    }
  }
}