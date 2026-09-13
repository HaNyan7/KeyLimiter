from __future__ import annotations

import argparse
from math import ceil
from pathlib import Path

from PIL import Image, ImageDraw, ImageFont

TILE_SIZE = 100
GAP_SIZE = 10
MAX_COLUMNS = 8
DEFAULT_MAX_COUNT = 100
TEXT_GAP = 25
TEXT_FONT_SIZE = 40
TEXT_PADDING = 8
NUMBER_FONT_WEIGHT = 500
TEXT_FONT_WEIGHT = 700

ROOT = Path(__file__).resolve().parent
ASSETS_DIR = ROOT / "assets"
BUILD_DIR = ROOT / "build"
FONT_PATH = ASSETS_DIR / "PretendardVariable.ttf"


def drawRaw(
    raw_count: int,
    fill: Image.Image,
    empty: Image.Image,
) -> Image.Image:
    if not 1 <= raw_count <= MAX_COLUMNS:
        raise ValueError(f"raw_count must be between 1 and {MAX_COLUMNS}")

    tiles = [empty] if raw_count % 2 == 1 else []
    tiles.extend([fill] * raw_count)

    width = TILE_SIZE * len(tiles) + GAP_SIZE * (len(tiles) - 1)
    canvas = Image.new("RGBA", (width, TILE_SIZE), (0, 0, 0, 0))

    for index, tile in enumerate(tiles):
        canvas.alpha_composite(tile, (index * (TILE_SIZE + GAP_SIZE), 0))

    return canvas


def drawImage(
    count: int,
    fill: Image.Image,
    empty: Image.Image,
) -> Image.Image:
    if count < 1:
        raise ValueError("count must be at least 1")

    if fill.size != (TILE_SIZE, TILE_SIZE) or empty.size != (TILE_SIZE, TILE_SIZE):
        raise ValueError(f"image must be {TILE_SIZE}x{TILE_SIZE}")

    quotient, remainder = divmod(count, MAX_COLUMNS)
    raws = [drawRaw(MAX_COLUMNS, fill, empty) for _ in range(quotient)]

    if remainder:
        raws.append(drawRaw(remainder, fill, empty))

    try:
        raws_width = max(raw.width for raw in raws)
        raws_height = TILE_SIZE * len(raws) + GAP_SIZE * (len(raws) - 1)

        if not FONT_PATH.is_file():
            raise FileNotFoundError(f"Font is missing: {FONT_PATH}")

        number_label = f"{count}  "
        text_label = "KEY LIMIT"

        number_font = ImageFont.truetype(FONT_PATH, TEXT_FONT_SIZE)
        number_font.set_variation_by_axes([NUMBER_FONT_WEIGHT])
        text_font = ImageFont.truetype(FONT_PATH, TEXT_FONT_SIZE)
        text_font.set_variation_by_axes([TEXT_FONT_WEIGHT])

        number_advance = number_font.getlength(number_label)
        number_bounds = number_font.getbbox(number_label, anchor="ls")
        text_bounds = text_font.getbbox(text_label, anchor="ls")

        label_left = min(number_bounds[0], number_advance + text_bounds[0])
        label_top = min(number_bounds[1], text_bounds[1])
        label_right = max(number_bounds[2], number_advance + text_bounds[2])
        label_bottom = max(number_bounds[3], text_bounds[3])
        label_width = ceil(label_right - label_left)
        label_height = ceil(label_bottom - label_top)

        width = max(raws_width, label_width + TEXT_PADDING * 2)
        height = raws_height + TEXT_GAP + label_height + TEXT_PADDING
        canvas = Image.new("RGBA", (width, height), (0, 0, 0, 0))

        for index, raw in enumerate(raws):
            x = (width - raw.width) // 2
            y = index * (TILE_SIZE + GAP_SIZE)
            canvas.alpha_composite(raw, (x, y))

        draw = ImageDraw.Draw(canvas)
        label_x = (width - label_width) / 2 - label_left
        label_y = raws_height + TEXT_GAP - label_top
        text_options = {
            "fill": (255, 255, 255, 255),
            "anchor": "ls",
        }
        draw.text(
            (label_x, label_y),
            number_label,
            font=number_font,
            **text_options,
        )
        draw.text(
            (label_x + number_advance, label_y),
            text_label,
            font=text_font,
            **text_options,
        )

        return canvas
    finally:
        for raw in raws:
            raw.close()


def generate(max_count: int) -> None:
    if max_count < 1:
        raise ValueError("max_count must be at least 1")

    BUILD_DIR.mkdir(parents=True, exist_ok=True)

    with Image.open(ASSETS_DIR / "fill.png") as fill_source, Image.open(
        ASSETS_DIR / "empty.png"
    ) as empty_source:
        fill = fill_source.convert("RGBA")
        empty = empty_source.convert("RGBA")

        try:
            expected_size = (TILE_SIZE, TILE_SIZE)
            if fill.size != expected_size or empty.size != expected_size:
                raise ValueError(
                    f"fill.png and empty.png must both be {TILE_SIZE}x{TILE_SIZE}"
                )

            for count in range(1, max_count + 1):
                output = BUILD_DIR / f"{count}.png"
                with drawImage(count, fill, empty) as image:
                    image.save(output, optimize=True)
                print(output.relative_to(ROOT))
        finally:
            fill.close()
            empty.close()


if __name__ == "__main__":
    parser = argparse.ArgumentParser()
    parser.add_argument(
        "count",
        type=int,
    )
    generate(parser.parse_args().count)
