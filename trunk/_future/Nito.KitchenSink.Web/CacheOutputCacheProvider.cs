using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Nito.KitchenSink.Web
{
    using System.Collections.Specialized;
    using System.Configuration.Provider;
    using System.Reflection;
    using System.Runtime.Caching;
    using System.Web.Caching;

    public sealed class CacheOutputCacheProvider : OutputCacheProvider
    {
        private ObjectCache cache;

        public override void Initialize(string name, NameValueCollection config)
        {
            if (config == null)
            {
                throw new ArgumentNullException("config");
            }

            if (string.IsNullOrEmpty(name))
            {
                name = "CacheOutputCacheProvider";
            }

            if (string.IsNullOrEmpty(config["description"]))
            {
                config.Remove("description");
                config.Add(
                    "description", "OutputCacheProvider that forwards cache interfaces to a generic ObjectCache.");
            }

            base.Initialize(name, config);

            Type type;
            try
            {
                type = Type.GetType(config["cacheType"], true);
            }
            catch (ArgumentNullException ex)
            {
                throw new ProviderException("cacheType not specified in config.", ex);
            }
            catch (TargetInvocationException ex)
            {
                throw new ProviderException("Cache class initializer threw exception.", ex);
            }
            catch (TypeLoadException ex)
            {
                throw new ProviderException("cacheType not found", ex);
            }
            catch (Exception ex)
            {
                throw new ProviderException("Unexpected error: " + ex.Message, ex);
            }

            var constructorArguments = config["constructorArguments"];
            object cache;
            try
            {
                cache = !string.IsNullOrEmpty(constructorArguments) ? Activator.CreateInstance(type, constructorArguments.Split(',')) : Activator.CreateInstance(type);
            }
            catch (TargetInvocationException ex)
            {
                throw new ProviderException("Cache instance initializer threw exception.", ex);
            }
            catch (MissingMethodException ex)
            {
                throw new ProviderException("cacheType does not have a matching constructor.", ex);
            }
            catch (Exception ex)
            {
                throw new ProviderException("Unexpected error: " + ex.Message, ex);
            }

            this.cache = cache as ObjectCache;
            if (this.cache == null)
            {
                throw new ProviderException("cacheType is not an ObjectCache.");
            }
        }

        public override object Add(string key, object entry, DateTime utcExpiry)
        {
            throw new NotImplementedException();
        }

        public override object Get(string key)
        {
            throw new NotImplementedException();
        }

        public override void Remove(string key)
        {
            throw new NotImplementedException();
        }

        public override void Set(string key, object entry, DateTime utcExpiry)
        {
            throw new NotImplementedException();
        }

        private sealed class CacheEntry
        {
            public DateTime UtcExpiry { get; set; }
            public object Value { get; set; }
        }
    }
}
