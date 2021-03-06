﻿#if !HS
using ADV;
using System.IO;
using System.Linq;
using XUnity.AutoTranslator.Plugin.Core;
using XUnity.AutoTranslator.Plugin.Core.AssetRedirection;
using XUnity.AutoTranslator.Plugin.Core.Utilities;
using XUnity.ResourceRedirector;

namespace KK_Plugins
{
    public class ScenarioDataResourceRedirector : AssetLoadedHandlerBaseV2<ScenarioData>
    {
        public ScenarioDataResourceRedirector() => CheckDirectory = true;

        protected override string CalculateModificationFilePath(ScenarioData asset, IAssetOrResourceLoadedContext context) =>
            context.GetPreferredFilePathWithCustomFileName(asset, null).Replace(".unity3d", "");

        protected override bool DumpAsset(string calculatedModificationPath, ScenarioData asset, IAssetOrResourceLoadedContext context)
        {
            var defaultTranslationFile = Path.Combine(calculatedModificationPath, "translation.txt");
            var cache = new SimpleTextTranslationCache(
               file: defaultTranslationFile,
               loadTranslationsInFile: false);

            foreach (var param in asset.list)
            {
                if (param.Command == Command.Text)
                {
                    for (int i = 0; i < param.Args.Length; i++)
                    {
                        var key = param.Args[i];

                        if (!string.IsNullOrEmpty(key) && LanguageHelper.IsTranslatable(key))
                        {
                            cache.AddTranslationToCache(key, key);
                        }
                    }
                }
            }

            return true;
        }

        protected override bool ReplaceOrUpdateAsset(string calculatedModificationPath, ref ScenarioData asset, IAssetOrResourceLoadedContext context)
        {
            var defaultTranslationFile = Path.Combine(calculatedModificationPath, "translation.txt");
            var redirectedResources = RedirectedDirectory.GetFilesInDirectory(calculatedModificationPath, ".txt");
            var streams = redirectedResources.Select(x => x.OpenStream());
            var cache = new SimpleTextTranslationCache(
               outputFile: defaultTranslationFile,
               inputStreams: streams,
               allowTranslationOverride: false,
               closeStreams: true);

            foreach (var param in asset.list)
            {
                if (param.Command == Command.Text)
                {
                    for (int i = 0; i < param.Args.Length; i++)
                    {
                        var key = param.Args[i];
                        if (!string.IsNullOrEmpty(key))
                        {
                            if (cache.TryGetTranslation(key, true, out var translated))
                            {
                                param.Args[i] = translated;
                            }
                            else if (AutoTranslatorSettings.IsDumpingRedirectedResourcesEnabled && LanguageHelper.IsTranslatable(key))
                            {
                                cache.AddTranslationToCache(key, key);
                            }
                        }
                    }
                }
            }

            return true;
        }

        protected override bool ShouldHandleAsset(ScenarioData asset, IAssetOrResourceLoadedContext context) => !context.HasReferenceBeenRedirectedBefore(asset);
    }
}
#else //Stub for HS which has no ScenarioData
namespace KK_Plugins
{
    public class ScenarioDataResourceRedirector { }
}
#endif