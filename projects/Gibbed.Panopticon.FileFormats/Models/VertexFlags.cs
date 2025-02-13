﻿/* Copyright (c) 2025 Rick (rick 'at' gibbed 'dot' us)
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

namespace Gibbed.Panopticon.FileFormats.Models
{
    [Flags]
    public enum VertexFlags : uint
    {
        None = 0,

        Position = 1 << 0, // POSITION
        Normal = 1 << 1, // NORMAL
        TexCoord0 = 1 << 2, // TEXCOORD 0
        TexCoord1 = 1 << 3, // TEXCOORD 1
        TexCoord2 = 1 << 4, // TEXCOORD 2
        TexCoord3 = 1 << 5, // TEXCOORD 3
        Color0 = 1 << 6, // COLOR 0
        Tangent = 1 << 7, // TANGENT
        Binormal = 1 << 8, // BINORMAL
        Blend = 1 << 9, // BLENDINDICES / BLENDWEIGHTS
        Color1 = 1 << 10, // COLOR 1

        Unknown = 1 << 15,
    }
}
