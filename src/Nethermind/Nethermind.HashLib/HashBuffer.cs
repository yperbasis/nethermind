//  Copyright (c) 2021 Demerzel Solutions Limited
//  This file is part of the Nethermind library.
// 
//  The Nethermind library is free software: you can redistribute it and/or modify
//  it under the terms of the GNU Lesser General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.
// 
//  The Nethermind library is distributed in the hope that it will be useful,
//  but WITHOUT ANY WARRANTY; without even the implied warranty of
//  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
//  GNU Lesser General Public License for more details.
// 
//  You should have received a copy of the GNU Lesser General Public License
//  along with the Nethermind. If not, see <http://www.gnu.org/licenses/>.

using System;
using System.Diagnostics;

namespace Nethermind.HashLib
{
    [DebuggerNonUserCode]
    public class HashBuffer
    {
        private byte[] m_data;
        private int m_pos;

        public HashBuffer(int a_length)
        {
            Debug.Assert(a_length > 0);

            m_data = new byte[a_length];

            Initialize();
        }

        public void Initialize()
        {
            m_pos = 0;
        }

        public byte[] GetBytes()
        {
            Debug.Assert(IsFull);

            m_pos = 0;
            return m_data;
        }

        public byte[] GetBytesZeroPadded()
        {
            Array.Clear(m_data, m_pos, m_data.Length - m_pos);
            m_pos = 0;
            return m_data;
        }

        public bool Feed(byte[] a_data, ref int a_start_index, ref int a_length, ref ulong a_processed_bytes)
        {
            Debug.Assert(a_start_index >= 0);
            Debug.Assert(a_length >= 0);
            Debug.Assert(a_start_index + a_length <= a_data.Length);
            Debug.Assert(!IsFull);

            if (a_data.Length == 0)
                return false;

            if (a_length == 0)
                return false;

            int length = m_data.Length - m_pos;
            if (length > a_length)
                length = a_length;

            Array.Copy(a_data, a_start_index, m_data, m_pos, length);

            m_pos += length;
            a_start_index += length;
            a_length -= length;
            a_processed_bytes += (ulong)length;

            return IsFull;
        }

        public bool Feed(ReadOnlySpan<byte> a_data, ref int a_start_index, ref int a_length, ref ulong a_processed_bytes)
        {
            Debug.Assert(a_start_index >= 0);
            Debug.Assert(a_length >= 0);
            Debug.Assert(a_start_index + a_length <= a_data.Length);
            Debug.Assert(!IsFull);

            if (a_data.Length == 0)
                return false;

            if (a_length == 0)
                return false;

            int length = m_data.Length - m_pos;
            if (length > a_length)
                length = a_length;

            Span<byte> m_span = m_data.AsSpan(m_pos, length);
            a_data.Slice(a_start_index, length).CopyTo(m_span);

            m_pos += length;
            a_start_index += length;
            a_length -= length;
            a_processed_bytes += (ulong)length;

            return IsFull;
        }

        public bool Feed(byte[] a_data, int a_length)
        {
            Debug.Assert(a_length >= 0);
            Debug.Assert(a_length <= a_data.Length);
            Debug.Assert(!IsFull);

            if (a_data.Length == 0)
                return false;

            if (a_length == 0)
                return false;

            int length = m_data.Length - m_pos;
            if (length > a_length)
                length = a_length;

            Array.Copy(a_data, 0, m_data, m_pos, length);

            m_pos += length;

            return IsFull;
        }

        public bool IsEmpty
        {
            get
            {
                return m_pos == 0;
            }
        }

        public int Pos
        {
            get
            {
                return m_pos;
            }
        }

        public int Length
        {
            get
            {
                return m_data.Length;
            }
        }

        public bool IsFull
        {
            get
            {
                return (m_pos == m_data.Length);
            }
        }

        public override string ToString()
        {
            return String.Format("HashBuffer, Legth: {0}, Pos: {1}, IsEmpty: {2}", Length, Pos, IsEmpty);
        }
    }
}
