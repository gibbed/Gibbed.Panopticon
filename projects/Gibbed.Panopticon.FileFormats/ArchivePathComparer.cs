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

namespace Gibbed.Panopticon.FileFormats
{
    public class ArchivePathComparer : IComparer<string>
    {
        public int Compare(string left, string right)
        {
            if (left == null || right == null)
            {
                return 0;
            }

            int result, prefixLength;

            int leftStart = 0;
            int rightStart = 0;
            while (true)
            {
                var leftEnd = left.IndexOf('/', leftStart);
                var rightEnd = right.IndexOf('/', rightStart);
                if (leftEnd < 0 || rightEnd < 0)
                {
                    if (leftEnd >= 0)
                    {
                        return 1;
                    }
                    else if (rightEnd >= 0)
                    {
                        return -1;
                    }
                    break;
                }

                prefixLength = System.Math.Min(leftEnd - leftStart, rightEnd - rightStart);
                result = string.Compare(
                    left, leftStart,
                    right, rightStart,
                    prefixLength,
                    StringComparison.OrdinalIgnoreCase);
                if (result != 0)
                {
                    return result;
                }

                leftStart = leftEnd + 1;
                rightStart = rightEnd + 1;
            }

            prefixLength = System.Math.Min(left.Length - leftStart, right.Length - rightStart);
            result = string.Compare(
                left, leftStart,
                right, rightStart,
                prefixLength,
                StringComparison.OrdinalIgnoreCase);
            if (result != 0)
            {
                return result;
            }

            var difference = left.Length - right.Length;
            if (difference != 0)
            {
                return difference;
            }

            return string.Compare(left, right, StringComparison.Ordinal);
        }
    }
}
