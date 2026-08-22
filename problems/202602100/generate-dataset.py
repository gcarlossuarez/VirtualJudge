from pathlib import Path
import zipfile

base = Path("./IN")
base.mkdir(exist_ok=True)

cases = [
    ("3 4 + 2 *", 14),
    ("10 2 3 * + 4 -", 12),
    ("8 2 - 5 +", 11),
    ("20 5 / 3 +", 7),
    ("2 3 4 + *", 14),
    ("20 4 2 * -", 12),
    ("5 3 - 2 * 7 +", 11),
    ("4 5 + 2 3 + *", 45),
    ("100 20 5 / - 2 *", 192),
    ("7 2 3 * + 4 2 / -", 11),
    ("15 3 - 2 1 + / 8 2 + 6 - *", 16),
    ("25 5 / 7 3 + * 4 -", 46),
    ("6 2 + 3 4 + * 5 1 - /", 14),
    ("9 3 / 8 2 - * 7 5 - +", 20),
    ("50 5 2 * - 8 4 / 3 + *", 200),
]

summary_lines = []
for i, (expr, result) in enumerate(cases, start=1):
    tokens = expr.split()
    content = f"{len(tokens)}\n{expr}\n"
    filename = base / f"datos{i:04d}.txt"
    filename.write_text(content, encoding="utf-8")
    summary_lines.append(f"datos{i:04d}.txt -> {result}")

answers_path = base / "resultados_esperados.txt"
answers_path.write_text("\n".join(summary_lines) + "\n", encoding="utf-8")

zip_path = Path("./dataset_postfija_15_casos.zip")
with zipfile.ZipFile(zip_path, "w", zipfile.ZIP_DEFLATED) as zf:
    for p in sorted(base.iterdir()):
        zf.write(p, arcname=p.name)

print(zip_path)
