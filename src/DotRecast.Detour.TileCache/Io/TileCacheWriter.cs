/*
Copyright (c) 2009-2010 Mikko Mononen memon@inside.org
recast4j copyright (c) 2015-2019 Piotr Piastucki piotr@jtilia.org
DotRecast Copyright (c) 2023 Choi Ikpil ikpil@naver.com

This software is provided 'as-is', without any express or implied
warranty.  In no event will the authors be held liable for any damages
arising from the use of this software.
Permission is granted to anyone to use this software for any purpose,
including commercial applications, and to alter it and redistribute it
freely, subject to the following restrictions:
1. The origin of this software must not be misrepresented; you must not
 claim that you wrote the original software. If you use this software
 in a product, an acknowledgment in the product documentation would be
 appreciated but is not required.
2. Altered source versions must be plainly marked as such, and must not be
 misrepresented as being the original software.
3. This notice may not be removed or altered from any source distribution.
*/

using System.IO;
using DotRecast.Core;
using DotRecast.Detour.Io;

namespace DotRecast.Detour.TileCache.Io
{
    public class TileCacheWriter : DetourWriter
    {
        private readonly NavMeshParamWriter paramWriter = new NavMeshParamWriter();
        private readonly TileCacheBuilder builder = new TileCacheBuilder();

        public void Write(BinaryWriter stream, TileCache cache, RcByteOrder order, bool cCompatibility)
        {
            Write(stream, TileCacheSetHeader.TILECACHESET_MAGIC, order);
            Write(stream, cCompatibility
                ? TileCacheSetHeader.TILECACHESET_VERSION
                : TileCacheSetHeader.TILECACHESET_VERSION_RECAST4J, order);
            int numTiles = 0;
            for (int i = 0; i < cache.GetTileCount(); ++i)
            {
                CompressedTile tile = cache.GetTile(i);
                if (tile == null || tile.data == null)
                    continue;
                numTiles++;
            }

            Write(stream, numTiles, order);
            paramWriter.Write(stream, cache.GetNavMesh().GetParams(), order);
            WriteCacheParams(stream, cache.GetParams(), order);
            for (int i = 0; i < cache.GetTileCount(); i++)
            {
                CompressedTile tile = cache.GetTile(i);
                if (tile == null || tile.data == null)
                    continue;
                Write(stream, (int)cache.GetTileRef(tile), order);
                byte[] data = tile.data;
                TileCacheLayer layer = cache.DecompressTile(tile);
                data = builder.CompressTileCacheLayer(layer, order, cCompatibility);
                Write(stream, data.Length, order);
                stream.Write(data);
            }
        }

        private void WriteCacheParams(BinaryWriter stream, TileCacheParams option, RcByteOrder order)
        {
            Write(stream, option.orig.x, order);
            Write(stream, option.orig.y, order);
            Write(stream, option.orig.z, order);

            Write(stream, option.cs, order);
            Write(stream, option.ch, order);
            Write(stream, option.width, order);
            Write(stream, option.height, order);
            Write(stream, option.walkableHeight, order);
            Write(stream, option.walkableRadius, order);
            Write(stream, option.walkableClimb, order);
            Write(stream, option.maxSimplificationError, order);
            Write(stream, option.maxTiles, order);
            Write(stream, option.maxObstacles, order);
        }
    }
}