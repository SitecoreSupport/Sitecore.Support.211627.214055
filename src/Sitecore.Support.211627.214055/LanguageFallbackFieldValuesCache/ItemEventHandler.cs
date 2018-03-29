using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web;
using Sitecore.Caching;
using Sitecore.Data.Items;
using Sitecore.Diagnostics;
using Sitecore.Events;

namespace Sitecore.Support.LanguageFallbackFieldValuesCache
{
  public class ItemEventHandler
  {
    public void OnItemSaved(object sender, EventArgs args)
    {
      try
      {
        Item item = Event.ExtractParameter(args, 0) as Item;
        Assert.IsNotNull(item, "No item in parameters");
        var fallbackFieldValuesCache = item.Database.Caches.FallbackFieldValuesCache as LanguageFallbackIndexedFieldValuesCache;
        var clearCacheMethod = typeof(LanguageFallbackIndexedFieldValuesCache)
          .GetMethod("ClearCache", BindingFlags.Instance | BindingFlags.NonPublic);

        // Clear caches for all clones of item passed as args.
        foreach (var link in Globals.LinkDatabase.GetItemReferrers(item, false))
        {
          var referrer = Configuration.Factory.GetDatabase(link.SourceDatabaseName).GetItem(link.SourceItemID);
          if (referrer.Source != null && referrer.Source.ID == item.ID)
          {
            clearCacheMethod.Invoke(fallbackFieldValuesCache, new object[] { referrer });
          }
        }
      }
      catch (Exception e)
      {
        Log.Error($"[Sitecore.Support.211627.214055] Error occurred while handling event: {Environment.StackTrace}", this);
      }
     
    }
  }
}