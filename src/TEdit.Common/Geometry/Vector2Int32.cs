﻿/* 
Copyright (c) 2011 BinaryConstruct
 
This source is subject to the Microsoft Public License.
See http://www.microsoft.com/opensource/licenses.mspx#Ms-PL.
All other rights reserved.

THIS CODE AND INFORMATION IS PROVIDED "AS IS" WITHOUT WARRANTY OF ANY KIND, 
EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE IMPLIED 
WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A PARTICULAR PURPOSE.
 */

using System;

namespace TEdit.Geometry;


[Serializable]
public struct Vector2Int32
{
    public int X;
    public int Y;

    public int PosX { get => X; set => X = value; }
    public int PosY { get => Y; set => Y = value; }

    public Vector2Int32(int x, int y)
        : this()
    {
        X = x;
        Y = y;
    }

    public override string ToString()
    {
        return $"({X:0},{Y:0})";
    }

    public static bool Parse(string text, out Vector2Int32 vector)
    {
        vector = new Vector2Int32();
        if (string.IsNullOrWhiteSpace(text)) return false;

        var split = text.Split(',', 'x');
        if (split.Length != 2) return false;
        int x, y;
        if (int.TryParse(split[0], out x) ||
            int.TryParse(split[1], out y))
            return false;

        vector = new Vector2Int32(x, y);
        return true;
    }

    public static Vector2Int32 operator +(Vector2Int32 a, Vector2Int32 b) => new Vector2Int32((short)(a.X + b.X), (short)(a.Y + b.Y));
    public static Vector2Int32 operator -(Vector2Int32 a, Vector2Int32 b) => new Vector2Int32((short)(a.X - b.X), (short)(a.Y - b.Y));
    public static Vector2Int32 operator *(Vector2Int32 a, Vector2Int32 b) => new Vector2Int32((short)(a.X * b.X), (short)(a.Y * b.Y));
    public static Vector2Int32 operator /(Vector2Int32 a, Vector2Int32 b) => new Vector2Int32((short)(a.X / b.X), (short)(a.Y / b.Y));
    public static Vector2Int32 operator *(Vector2Int32 a, short b) => new Vector2Int32((short)(a.X * b), (short)(a.Y * b));
    public static Vector2Int32 operator /(Vector2Int32 a, short b) => new Vector2Int32((short)(a.X / b), (short)(a.Y / b));

    public bool Equals(Vector2Int32 other)
    {
        return other.Y == Y && other.X == X;
    }

    public override bool Equals(object obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        if (obj.GetType() != typeof(Vector2Int32)) return false;
        return Equals((Vector2Int32)obj);
    }

    public override int GetHashCode()
    {
        unchecked
        {
            return Y * 397 ^ X;
        }
    }

    public static bool operator ==(Vector2Int32 left, Vector2Int32 right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(Vector2Int32 left, Vector2Int32 right)
    {
        return !left.Equals(right);
    }
}