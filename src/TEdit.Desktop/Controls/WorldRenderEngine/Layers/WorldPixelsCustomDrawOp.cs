﻿using Avalonia;
using Avalonia.Controls.Documents;
using Avalonia.Media;
using Avalonia.Platform;
using Avalonia.Rendering.SceneGraph;
using Avalonia.Skia;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using TEdit.Common;
using TEdit.Configuration;
using TEdit.Terraria;

namespace TEdit.Desktop.Controls.WorldRenderEngine.Layers;

public class RasterTileRenderer
{
    private static TEditColor GetBackgroundColor(World _world, int y)
    {
        if (_world == null) { return TEditColor.White; }
        if (y < 80)
            return WorldConfiguration.GlobalColors["Space"];
        else if (y > _world.TilesHigh - 192)
            return WorldConfiguration.GlobalColors["Hell"];
        else if (y > _world.RockLevel)
            return WorldConfiguration.GlobalColors["Rock"];
        else if (y > _world.GroundLevel)
            return WorldConfiguration.GlobalColors["Earth"];
        else
            return WorldConfiguration.GlobalColors["Sky"];
    }

    public static SKBitmap CreateBitmapTile(World _world, int xTile, int yTile, int tileSize)
    {
        ArgumentNullException.ThrowIfNull(_world);

        var bmp = new SKBitmap(tileSize, tileSize, SKImageInfo.PlatformColorType, SKAlphaType.Premul);

        var world = _world;
        int worldWidth = world.TilesWide;
        int worldHeight = world.TilesHigh;

        var blocks = world.Tiles;

        int blockX = xTile * tileSize;
        int blockY = yTile * tileSize;

        for (int x = 0; x + blockX < worldWidth && x < tileSize; x++)
        {
            for (int y = 0; y + blockY < worldHeight && y < tileSize; y++)
            {
                var currentBlockX = x + blockX;
                var currentBlockY = y + blockY;

                var block = blocks[currentBlockX, currentBlockY];
                //if (block.IsActive)
                {
                    var bgColor = GetBackgroundColor(_world, currentBlockY);
                    var tileColor = PixelMap.GetTileColor(block, bgColor).ToSKColor().WithAlpha(255);

                    bmp.SetPixel(
                        x,
                        y,
                        tileColor);
                }
            }
        }

        //bmp.SetImmutable();


        return bmp;
    }
}

public class WorldPixelsCustomDrawOp : ICustomDrawOperation
{
    private readonly Vector _offset;
    private readonly World? _world;
    private readonly IRasterTileCache _tileCache;
    private readonly double _zoom = 1f;

    public WorldPixelsCustomDrawOp(
        Rect bounds,
        Vector offset,
        World? world,
        IRasterTileCache tileCache,
        double tileScale = 1f
        )
    {
        Bounds = bounds;
        _offset = offset;
        _world = world;
        this._tileCache = tileCache;
        _zoom = tileScale;
        LoadContent();
    }

    public void LoadContent()
    {

    }

    public void Dispose()
    {
        // No-op
    }

    public Rect Bounds { get; }
    public bool HitTest(Point p) => false;
    public bool Equals(ICustomDrawOperation other) => false;

    public void Render(ImmediateDrawingContext context)
    {
        if (_world == null) { return; }

        var leaseFeature = context.TryGetFeature<ISkiaSharpApiLeaseFeature>();
        if (leaseFeature == null) { return; }

        using var lease = leaseFeature.Lease();

        var canvas = lease.SkCanvas;
        DrawWorldPixelTiles(canvas);
    }

    int _maxRenderPerFrame = 2;

    private RasterTile? GetTile(int x, int y)
    {
        var tile = _tileCache.GetTile(x, y);

        if (tile != null)
        {
            // update dirty tiles
            if (tile.IsDirty && _maxRenderPerFrame > 0)
            {
                _maxRenderPerFrame--;

                tile.Bitmap = RasterTileRenderer.CreateBitmapTile(_world, x, y, _tileCache.TileSize);
                tile.IsDirty = false;
            }
        }

        return tile;
    }

    SKPaint greenBorder = new SKPaint
    {
        Color = SKColors.Green,
        Style = SKPaintStyle.Stroke,
        StrokeWidth = 1f,
    };

    private void DrawWorldPixelTiles(SKCanvas canvas)
    {
        if (_world == null) { return; }

        canvas.Save();

        int numTilesX = (int)Math.Ceiling((double)Bounds.Width / _tileCache.TileSize / _zoom) + 1; // draw one extra tile to cover border
        int numTilesY = (int)Math.Ceiling((double)Bounds.Height / _tileCache.TileSize / _zoom) + 1;

        using var paint = new SKPaint
        {
            IsAntialias = true,
            FilterQuality = SKFilterQuality.High,
            IsDither = false,
            Color = SKColors.White,
            Style = SKPaintStyle.Fill,
            BlendMode = SKBlendMode.SrcOver,
        };

        var xTileOffset = (int)Math.Floor((double)_offset.X / _tileCache.TileSize / _zoom);
        var yTileOffset = (int)Math.Floor((double)_offset.Y / _tileCache.TileSize / _zoom);

        var maxTileX = Math.Min(_tileCache.TilesX, numTilesX + xTileOffset);
        var maxTileY = Math.Min(_tileCache.TilesY, numTilesY + yTileOffset);

        for (int x = xTileOffset; x < maxTileX; x++)
        {
            for (int y = yTileOffset; y < maxTileY; y++)
            {
                var tileCanvasX = x * _zoom * _tileCache.TileSize - (int)_offset.X + (int)Bounds.X;
                var tileCanvasY = y * _zoom * _tileCache.TileSize - (int)_offset.Y + (int)Bounds.Y;
                var tileCanvasWidth = _tileCache.TileSize * _zoom;
                var tileCanvasHeight = _tileCache.TileSize * _zoom;

                var tile = GetTile(x, y);

                var canvasRect = new SKRect(
                            (float)tileCanvasX,
                            (float)tileCanvasY,
                            (float)(tileCanvasX + tileCanvasWidth),
                            (float)(tileCanvasY + tileCanvasHeight));

                if (tile?.Bitmap != null)
                {
                    canvas.DrawBitmap(
                        tile.Bitmap,
                        new SKRect(0, 0, tile.Bitmap.Width, tile.Bitmap.Height),
                        canvasRect
                        //paint
                        );

                }
                else
                {
                    int b = 0;
                }

                canvas.DrawRect(canvasRect, greenBorder);

            }
        }

        canvas.Restore();
    }
}
