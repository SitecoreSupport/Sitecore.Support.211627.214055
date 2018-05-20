using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web;
using Sitecore.Caching;
using Sitecore.Data.Items;
using Sitecore.Diagnostics;
using Sitecore.Events;
using Sitecore.Data.Events;

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

        ClearCache(item);
      }
      catch (Exception ex)
      {
        Log.Error($"[Sitecore.Support.211627.214055] Error occurred while handling event: {ex.Message}\n{ex.StackTrace}", this);
      }      
    }

    public void OnItemSavedRemote(object sender, EventArgs args)
    {
      try
      {
        ItemSavedRemoteEventArgs arguments = args as ItemSavedRemoteEventArgs;
        Assert.IsNotNull(arguments, "Wrong type of args is received. Expected: 'ItemSavedRemoteEventArgs'");

        Item item = arguments.Item;
        Assert.IsNotNull(item, "No item in parameters");

        ClearCache(item);
      }
      catch (Exception ex)
      {
        Log.Error($"[Sitecore.Support.211627.214055] Error occurred while handling event: {ex.Message}\n{ex.StackTrace}", this);
      }
    }

    protected virtual void ClearCache(Item item)
    {
      var fallbackFieldValuesCache = item.Database.Caches.FallbackFieldValuesCache as LanguageFallbackIndexedFieldValuesCache;
      var clearCacheMethod = typeof(LanguageFallbackIndexedFieldValuesCache)
        .GetMethod("ClearCache", BindingFlags.Instance | BindingFlags.NonPublic);

      // Clear caches for all clones of item passed as args.
      foreach (var link in Globals.LinkDatabase.GetItemReferrers(item, false))
      {
        var referrer = Configuration.Factory.GetDatabase(link.SourceDatabaseName).GetItem(link.SourceItemID);
        if (referrer?.Source != null && referrer.Source.ID == item.ID)
        {
          clearCacheMethod.Invoke(fallbackFieldValuesCache, new object[] { referrer });
        }
      }
    }
  }
}