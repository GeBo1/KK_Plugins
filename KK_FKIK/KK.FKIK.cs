﻿using BepInEx;

namespace KK_Plugins
{
    [BepInDependency(KKAPI.KoikatuAPI.GUID)]
    [BepInProcess("CharaStudio")]
    [BepInPlugin(GUID, PluginName, Version)]
    public partial class FKIK : BaseUnityPlugin { }
}
