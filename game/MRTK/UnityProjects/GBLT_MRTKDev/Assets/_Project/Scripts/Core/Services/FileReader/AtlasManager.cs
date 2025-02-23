﻿using Core.Business;
using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.U2D;
using VContainer;

namespace Core.Framework
{
    public class AtlasManager
    {
        private readonly GameStore.Atlas _atlas;
        private readonly IBundleLoader _bundleLoader;

        private Dictionary<string, AtlasInfo> _atlasDict = new();
        private Dictionary<string, SpriteAtlas> _spriteBundlePathAndAtlasDict = new();

        public AtlasManager(
            IObjectResolver container,
            GameStore.Atlas atlases)
        {
            _bundleLoader = container.Resolve<IReadOnlyList<IBundleLoader>>().ElementAt((int)BundleLoaderName.Addressable);
            _atlas = atlases;

            Init();
        }

        private void Init()
        {
            foreach (var atlasPath in _atlas.Atlases)
            {
                string atlasNameNoExt = Path.GetFileNameWithoutExtension(atlasPath);
                AtlasInfo info = new AtlasInfo(atlasPath);
                _atlasDict.Add(atlasNameNoExt, info);
            }
        }

        public Sprite GetSpriteFromDataAtlas(string atlasName, string spriteName)
        {
            if (_atlasDict.TryGetValue(atlasName, out AtlasInfo info))
                return info.Atlas.GetSprite(spriteName);

            throw new SpriteNotFound(atlasName + "/" + spriteName);
        }

        private (string, string) GetPathAndName(string path)
        {
            string[] p = path.Split('/');
            string atlas = p[0];
            string spName = p[1];
            return (atlas, spName);
        }

        public async UniTask<Sprite> GetSpriteFromDataAtlas(string spritePath)
        {
            (string atlas, string spName) atlasPath = GetPathAndName(spritePath);
            if (_atlasDict.TryGetValue(atlasPath.atlas, out AtlasInfo info))
                return await GetSprite(info, atlasPath.spName);

            throw new SpriteNotFound(spritePath);
        }

        public bool IsSpriteBundlePathExist(string path)
        {
            if (!_spriteBundlePathAndAtlasDict.ContainsKey(path))
            {
                _spriteBundlePathAndAtlasDict.Add(path, null);
                return false;
            }

            return true;
        }

        private async UniTask<Sprite> GetSprite(AtlasInfo info, string spName)
        {
            SpriteAtlas atlas;
            bool isSpritePathExist = IsSpriteBundlePathExist(info.Path);
            if (!isSpritePathExist)
                atlas = await GetSpriteAtlas(info.Path);
            else
                atlas = await WaitAtlasInSameViewLoad(info.Path);

            return atlas.GetSprite(spName);
        }

        /// <summary>
        /// There is a case that multiple objects belong/beneath would access to the
        /// Atlas at the same time. In that case, while the first object is making the loading call
        /// for the atlas, then the others have to wait until the atlas is loaded completely.
        /// </summary>
        public async UniTask<SpriteAtlas> WaitAtlasInSameViewLoad(string path)
        {
            while (_spriteBundlePathAndAtlasDict[path] == null)
            {
                await UniTask.NextFrame();
            }

            return _spriteBundlePathAndAtlasDict[path];
        }

        public async UniTask<SpriteAtlas> GetSpriteAtlas(string path)
        {
            var atlas = await _bundleLoader.LoadAssetAsync<SpriteAtlas>(path);
            _spriteBundlePathAndAtlasDict[path] = atlas;
            return atlas;
        }

        private class SpriteNotFound : Exception
        {
            public SpriteNotFound(string mess) : base(mess)
            {
            }
        }

        private struct AtlasInfo
        {
            public string Path;
            public SpriteAtlas Atlas;

            public AtlasInfo(string path)
            {
                Path = path;
                Atlas = null;
            }
        }
    }
}
