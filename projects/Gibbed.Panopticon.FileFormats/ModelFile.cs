/* Copyright (c) 2025 Rick (rick 'at' gibbed 'dot' us)
 *
 * This software is provided 'as-is', without any express or implied
 * warranty. In no event will the authors be held liable for any damages
 * arising from the use of this software.
 *
 * Permission is granted to anyone to use this software for any purpose,
 * including commercial applications, and to alter it and redistribute it
 * freely, subject to the following restrictions:
 *
 * 1. The origin of this software must not be misrepresented; you must not
 *    claim that you wrote the original software. If you use this software
 *    in a product, an acknowledgment in the product documentation would
 *    be appreciated but is not required.
 *
 * 2. Altered source versions must be plainly marked as such, and must not
 *    be misrepresented as being the original software.
 *
 * 3. This notice may not be removed or altered from any source
 *    distribution.
 */

using System;
using System.Collections.Generic;
using System.Text;
using Gibbed.Memory;
using Gibbed.Panopticon.FileFormats.Models;

namespace Gibbed.Panopticon.FileFormats
{
    public class ModelFile
    {
        private static Encoding StringEncoding = Encoding.ASCII;

        // TODO(gibbed): determine if this has strict ordering or could be switched to a Dictionary
        private readonly List<(string name, Model model)> _Models;

        public ModelFile()
        {
            this._Models = new();
        }

        public Endian Endian { get; set; }
        public FileVersion Version { get; set; }
        public List<(string name, Model model)> Models => this._Models;

        public static ModelFile Load(ReadOnlySpan<byte> span)
        {
            var encoding = StringEncoding;

            int index = 0;
            var header = FileHeader.Read(span, ref index);
            var endian = header.Endian;
            var version = header.Version;

            var modelNames = new string[header.ModelCount];
            int modelNameOffsetIndex = header.ModelNameOffsetTableOffset;
            if (modelNameOffsetIndex != 0)
            {
                for (int i = 0; i < header.ModelCount; i++)
                {
                    var modelNameIndex = span.ReadValueS32(ref modelNameOffsetIndex, endian);
                    var modelName = modelNameIndex != 0
                        ? span.ReadStringZ(ref modelNameIndex, encoding)
                        : null;
                    modelNames[i] = modelName;
                }
            }

            var models = new (string name, Model model)[header.ModelCount];
            int modelOffsetIndex = header.ModelOffsetTableOffset;
            for (int i = 0; i < header.ModelCount; i++)
            {
                var modelOffset = span.ReadValueS32(ref modelOffsetIndex, endian);
                var model = modelOffset != 0 ? LoadModel(span.Slice(modelOffset), version, endian) : null;
                models[i] = (modelNames[i], model);
            }

            ModelFile instance = new();
            instance.Endian = endian;
            instance.Version = version;
            instance.Models.AddRange(models);
            return instance;
        }

        private static Model LoadModel(ReadOnlySpan<byte> span, FileVersion version, Endian endian)
        {
            int index = 0;
            var header = ModelHeader.Read(span, ref index, endian);

            var meshes = new Mesh[header.MeshCount];
            int meshOffsetIndex = header.MeshOffsetTableOffset;
            if (meshOffsetIndex != 0)
            {
                for (int i = 0; i < header.MeshCount; i++)
                {
                    var meshOffset = span.ReadValueS32(ref meshOffsetIndex, endian);
                    meshes[i] = meshOffset != 0 ? LoadMesh(span.Slice(meshOffset), version, endian) : null;
                }
            }

            return new()
            {
                Unknown = header.Unknown,
                Meshes = meshes,
            };
        }

        private static Mesh LoadMesh(ReadOnlySpan<byte> span, FileVersion version, Endian endian)
        {
            var encoding = StringEncoding;

            int index = 0;
            var header = MeshHeader.Read(span, ref index, endian);

            index = header.NameOffset;
            var name = index != 0 ? span.ReadStringZ(ref index, encoding) : null;

            var submeshes = new Submesh[header.SubmeshCount];
            int submeshOffsetIndex = header.SubmeshOffsetTableOffset;
            if (submeshOffsetIndex != 0)
            {
                for (int i = 0; i < header.SubmeshCount; i++)
                {
                    var submeshOffset = span.ReadValueS32(ref submeshOffsetIndex, endian);
                    submeshes[i] = submeshOffset != 0 ? LoadSubmesh(span.Slice(submeshOffset), version, endian) : null;
                }
            }

            return new()
            {
                AABB = header.AABB,
                Name = name,
                Unknown = header.Unknown,
                Submeshes = submeshes,
            };
        }

        private static Submesh LoadSubmesh(ReadOnlySpan<byte> span, FileVersion version, Endian endian)
        {
            var encoding = StringEncoding;

            int index = 0;
            var header = SubmeshHeader.Read(span, ref index, version, endian);

            index = header.NameOffset;
            var name = index != 0
                ? span.ReadStringZ(ref index, encoding)
                : null;

            var materials = new Material[header.MaterialCount];
            var materialIndex = header.MaterialDataOffset;
            if (materialIndex != 0)
            {
                for (int i = 0; i < header.MaterialCount; i++)
                {
                    materials[i] = materialIndex != 0
                        ? Material.Read(span, ref materialIndex, endian)
                        : default;
                }
            }

            var vertexData = header.VertexBufferOffset != 0
                ? span.Slice(header.VertexBufferOffset, header.VertexCount * header.VertexSize).ToArray()
                : null;

            var indexBuffers = new IndexBuffer[header.IndexBufferCount];
            int indexBufferOffsetIndex = header.IndexBufferOffsetTableOffset;
            if (indexBufferOffsetIndex != 0)
            {
                for (int i = 0; i < header.IndexBufferCount; i++)
                {
                    var indexBufferIndex = span.ReadValueS32(ref indexBufferOffsetIndex, endian);
                    indexBuffers[i] = indexBufferIndex != 0
                        ? LoadIndexBuffer(span.Slice(indexBufferIndex), version, endian)
                        : null;
                }
            }

            return new()
            {
                AABB = header.AABB,
                VertexType = header.VertexType,
                VertexSize = header.VertexSize,
                VertexCount = header.VertexCount,
                VertexData = vertexData,
                Name = name,
                PrimitiveType = header.PrimitiveType,
                Materials = materials,
                IndexBuffers = indexBuffers,
            };
        }

        private static IndexBuffer LoadIndexBuffer(ReadOnlySpan<byte> span, FileVersion version, Endian endian)
        {
            int index = 0;
            var header = IndexBufferHeader.Read(span, ref index, version, endian);

            var boneNames = new string[header.BoneCount];
            var boneNameOffsetIndex = header.BoneNameOffsetTableOffset;
            if (boneNameOffsetIndex != 0)
            {
                for (int i = 0; i < header.BoneCount; i++)
                {
                    var boneNameOffset = span.ReadValueS32(ref boneNameOffsetIndex, endian);
                    boneNames[i] = boneNameOffset != 0 ? span.ReadStringZ(ref boneNameOffset, Encoding.ASCII) : null;
                }
            }

            var indexData = header.DataOffset != 0
                ? span.Slice(header.DataOffset, header.IndexCount * 2).ToArray()
                : null;

            return new()
            {
                Data = indexData,
                BoneNames = boneNames,
            };
        }
    }
}
