using NGettext;
using NGettext.Loaders;
using NGettext.Wpf;
using NGettext.Wpf.Common;
using NGettext.Wpf.EnumTranslation;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SpleeterGui
{
    public static class I18n
    {
        public class MyLocalizer : IDisposable, ILocalizer
        {
            private readonly string _domainName;

            public ICatalog Catalog { get; private set; }

            public ICultureTracker CultureTracker { get; }

            public MyLocalizer(string domainName, ICultureTracker cultureTracker)
            {
                _domainName = domainName ?? throw new ArgumentNullException(nameof(domainName));
                CultureTracker = cultureTracker ?? throw new ArgumentNullException(nameof(cultureTracker));

                cultureTracker.CultureChanging += ResetCatalog;
                ResetCatalog(cultureTracker.CurrentCulture);
            }

            private void ResetCatalog(object sender, CultureEventArgs e)
            {
                ResetCatalog(e.CultureInfo);
            }

            private void ResetCatalog(CultureInfo cultureInfo)
            {
                Catalog = GetCatalog(cultureInfo);
            }

            public ICatalog GetCatalog(CultureInfo cultureInfo)
            {
                Assembly assembly = Assembly.GetExecutingAssembly();

                // We have set the LogicalName for EmbeddedResource, so there is no default namespace as prefix
                // https://stackoverflow.com/questions/2454139/embed-resource-in-a-net-assembly-without-an-assembly-prefix
                string moFileResourceName = "locale." + cultureInfo.IetfLanguageTag.Replace('-', '_') + ".message.mo";

                Stream stream = assembly.GetManifestResourceStream(moFileResourceName);
                if (stream == null)
                {
                    return new Catalog();
                }
                else
                {
                    return new Catalog(new MoLoader(stream));
                }
            }

            public void Dispose()
            {
                CultureTracker.CultureChanging -= ResetCatalog;
            }
        }

        public static void Compose(string domainName, NGettextWpfDependencyResolver dependencyResolver = null)
        {
            if (dependencyResolver == null)
            {
                dependencyResolver = new NGettextWpfDependencyResolver();
            }

            ICultureTracker cultureTracker = dependencyResolver.ResolveCultureTracker();
            MyLocalizer localizer = new MyLocalizer(domainName, cultureTracker);

            ChangeCultureCommand.CultureTracker = cultureTracker;
            GettextExtension.Localizer = localizer;
            TrackCurrentCultureBehavior.CultureTracker = cultureTracker;
            LocalizeEnumConverter.EnumLocalizer = new EnumLocalizer(localizer);
            Translation.Localizer = localizer;
            GettextStringFormatConverter.Localizer = localizer;
        }

        public static void ChangeCultureInfo(string name)
        {
            CultureInfo newCultureInfo = CultureInfo.GetCultureInfo(name);

            ChangeCultureCommand.CultureTracker.CurrentCulture = newCultureInfo;
        }
    }
}
