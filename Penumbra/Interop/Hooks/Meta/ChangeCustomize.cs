using FFXIVClientStructs.FFXIV.Client.Graphics.Scene;
using OtterGui.Services;
using Penumbra.GameData;
using Penumbra.GameData.Structs;
using Penumbra.Interop.PathResolving;

namespace Penumbra.Interop.Hooks.Meta;

public sealed unsafe class ChangeCustomize : FastHook<ChangeCustomize.Delegate>
{
    private readonly CollectionResolver _collectionResolver;
    private readonly MetaState          _metaState;

    public ChangeCustomize(HookManager hooks, CollectionResolver collectionResolver, MetaState metaState)
    {
        _collectionResolver = collectionResolver;
        _metaState          = metaState;
        Task                = hooks.CreateHook<Delegate>("Change Customize", Sigs.ChangeCustomize, Detour, true);
    }

    public delegate bool Delegate(Human* human, CustomizeArray* data, byte skipEquipment);

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    private bool Detour(Human* human, CustomizeArray* data, byte skipEquipment)
    {
        _metaState.CustomizeChangeCollection = _collectionResolver.IdentifyCollection((DrawObject*)human, true);
        using var cmp    = _metaState.ResolveRspData(_metaState.CustomizeChangeCollection.ModCollection);
        using var decal1 = _metaState.ResolveDecal(_metaState.CustomizeChangeCollection, true);
        using var decal2 = _metaState.ResolveDecal(_metaState.CustomizeChangeCollection, false);
        var       ret    = Task.Result.Original.Invoke(human, data, skipEquipment);
        Penumbra.Log.Excessive($"[Change Customize] Invoked on {(nint)human:X} with {(nint)data:X}, {skipEquipment} -> {ret}.");
        return ret;
    }
}
