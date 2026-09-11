from __future__ import annotations

import argparse
from pathlib import Path

from PIL import Image, ImageDraw, ImageFont


TILE_SIZE = 100
GAP = 25
MAX_COLUMNS = 8
DEFAULT_MAX_COUNT = 24
TEXT_GAP = 25
TEXT_FONT_SIZE = 40
TEXT_PADDING = 8
TEXT_STROKE_WIDTH = 3

ROOT = Path(__file__).resolve().parent
ASSETS_DIR = ROOT / "assets"
BUILD_DIR = ROOT / "build"
FONT_PATH = ASSETS_DIR / "Pretendard-SemiBold.ttf"


def parse_args() -> argparse.Namespace:
    parser = argparse.ArgumentParser(
        description="Generate centered KeyLimiter count images."
    )
    parser.add_argument(
        "--max-count",
        type=int,
        default=DEFAULT_MAX_COUNT,
        help=f"largest image number to generate (default: {DEFAULT_MAX_COUNT})",
    )
    return parser.parse_args()


def load_asset(name: str) -> Image.Image:
    path = ASSETS_DIR / name
    with Image.open(path) as source:
        image = source.convert("RGBA")

    expected_size = (TILE_SIZE, TILE_SIZE)
    if image.size != expected_size:
        raise ValueError(
            f"{path} must be {TILE_SIZE}x{TILE_SIZE}, got {image.size[0]}x{image.size[1]}"
        )

    return image


def make_rows(count: int) -> list[list[bool]]:
    """Return rows where True is fill and False is empty."""
    if count < 1:
        raise ValueError("count must be at least 1")

    full_rows, remainder = divmod(count, MAX_COLUMNS)
    rows = [[True] * MAX_COLUMNS for _ in range(full_rows)]

    if remainder:
        final_row = [True] * remainder
        if remainder % 2 == 1:
            final_row.insert(0, False)
        rows.append(final_row)

    return rows


def sequence_size(length: int) -> int:
    return length * TILE_SIZE + max(0, length - 1) * GAP


def load_font() -> ImageFont.FreeTypeFont:
    if not FONT_PATH.is_file():
        raise FileNotFoundError(f"Pretendard font is missing: {FONT_PATH}")

    return ImageFont.truetype(FONT_PATH, TEXT_FONT_SIZE)


def render(
    count: int,
    fill: Image.Image,
    empty: Image.Image,
    font: ImageFont.FreeTypeFont,
) -> Image.Image:
    rows = make_rows(count)
    label = f"{count}  KEY LIMIT"
    label_bounds = ImageDraw.Draw(Image.new("L", (1, 1))).textbbox(
        (0, 0),
        label,
        font=font,
        stroke_width=TEXT_STROKE_WIDTH,
    )
    label_width = label_bounds[2] - label_bounds[0]
    label_height = label_bounds[3] - label_bounds[1]

    tile_width = max(sequence_size(len(row)) for row in rows)
    tile_height = sequence_size(len(rows))
    width = max(tile_width, label_width + TEXT_PADDING * 2)
    height = tile_height + TEXT_GAP + label_height + TEXT_PADDING
    canvas = Image.new("RGBA", (width, height), (0, 0, 0, 0))

    for row_index, row in enumerate(rows):
        row_width = sequence_size(len(row))
        start_x = (width - row_width) // 2
        y = row_index * (TILE_SIZE + GAP)

        for column_index, is_filled in enumerate(row):
            x = start_x + column_index * (TILE_SIZE + GAP)
            canvas.alpha_composite(fill if is_filled else empty, (x, y))

    draw = ImageDraw.Draw(canvas)
    label_x = (width - label_width) // 2 - label_bounds[0]
    label_y = tile_height + TEXT_GAP - label_bounds[1]
    draw.text(
        (label_x, label_y),
        label,
        font=font,
        fill=(255, 255, 255, 255),
        stroke_width=TEXT_STROKE_WIDTH,
        stroke_fill=(0, 0, 0, 255),
    )

    return canvas


def generate(max_count: int) -> None:
    if max_count < 1:
        raise ValueError("--max-count must be at least 1")

    fill = load_asset("fill.png")
    empty = load_asset("empty.png")
    font = load_font()
    BUILD_DIR.mkdir(parents=True, exist_ok=True)

    for count in range(1, max_count + 1):
        output = BUILD_DIR / f"{count}.png"
        render(count, fill, empty, font).save(output, optimize=True)
        print(output.relative_to(ROOT))


if __name__ == "__main__":
    generate(parse_args().max_count)
