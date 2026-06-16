from __future__ import annotations

import re
from pathlib import Path
from xml.sax.saxutils import escape

from reportlab.lib import colors
from reportlab.lib.enums import TA_CENTER, TA_LEFT
from reportlab.lib.pagesizes import letter
from reportlab.lib.styles import ParagraphStyle, getSampleStyleSheet
from reportlab.lib.units import inch
from reportlab.platypus import (
    PageBreak,
    Paragraph,
    Preformatted,
    SimpleDocTemplate,
    Spacer,
)


ROOT = Path(__file__).resolve().parents[1]
SOURCE = ROOT / "docs" / "Unity_Assessment_Master_Plan.md"
OUTPUT = ROOT / "output" / "pdf" / "Unity_Assessment_Master_Plan.pdf"


def build_styles():
    base = getSampleStyleSheet()

    styles = {
        "title": ParagraphStyle(
            "PlanTitle",
            parent=base["Title"],
            fontName="Helvetica-Bold",
            fontSize=22,
            leading=27,
            alignment=TA_CENTER,
            textColor=colors.HexColor("#1F2A44"),
            spaceAfter=16,
        ),
        "subtitle": ParagraphStyle(
            "PlanSubtitle",
            parent=base["BodyText"],
            fontName="Helvetica",
            fontSize=10,
            leading=14,
            alignment=TA_CENTER,
            textColor=colors.HexColor("#4B5563"),
            spaceAfter=8,
        ),
        "h1": ParagraphStyle(
            "Heading1Custom",
            parent=base["Heading1"],
            fontName="Helvetica-Bold",
            fontSize=17,
            leading=22,
            textColor=colors.HexColor("#1F2A44"),
            spaceBefore=18,
            spaceAfter=8,
        ),
        "h2": ParagraphStyle(
            "Heading2Custom",
            parent=base["Heading2"],
            fontName="Helvetica-Bold",
            fontSize=13,
            leading=17,
            textColor=colors.HexColor("#263B5E"),
            spaceBefore=12,
            spaceAfter=6,
        ),
        "h3": ParagraphStyle(
            "Heading3Custom",
            parent=base["Heading3"],
            fontName="Helvetica-Bold",
            fontSize=11,
            leading=15,
            textColor=colors.HexColor("#374151"),
            spaceBefore=9,
            spaceAfter=4,
        ),
        "body": ParagraphStyle(
            "BodyCustom",
            parent=base["BodyText"],
            fontName="Helvetica",
            fontSize=9.2,
            leading=13.2,
            alignment=TA_LEFT,
            textColor=colors.HexColor("#111827"),
            spaceAfter=5,
        ),
        "bullet": ParagraphStyle(
            "BulletCustom",
            parent=base["BodyText"],
            fontName="Helvetica",
            fontSize=9.1,
            leading=12.8,
            leftIndent=16,
            firstLineIndent=-9,
            bulletIndent=3,
            textColor=colors.HexColor("#111827"),
            spaceAfter=3,
        ),
        "code": ParagraphStyle(
            "CodeCustom",
            parent=base["Code"],
            fontName="Courier",
            fontSize=7.5,
            leading=9.2,
            leftIndent=10,
            rightIndent=6,
            textColor=colors.HexColor("#111827"),
            backColor=colors.HexColor("#F3F4F6"),
            borderColor=colors.HexColor("#E5E7EB"),
            borderWidth=0.5,
            borderPadding=5,
            spaceBefore=4,
            spaceAfter=7,
        ),
    }

    return styles


def normalize_inline(text: str) -> str:
    text = escape(text)
    text = re.sub(r"`([^`]+)`", r'<font name="Courier">\1</font>', text)
    return text


def append_paragraph(story, text: str, style):
    if text.strip():
        story.append(Paragraph(normalize_inline(text.strip()), style))


def parse_markdown(markdown: str):
    styles = build_styles()
    story = []
    lines = markdown.splitlines()
    in_code = False
    code_lines = []
    paragraph_lines = []

    def flush_paragraph():
        nonlocal paragraph_lines
        if paragraph_lines:
            append_paragraph(story, " ".join(paragraph_lines), styles["body"])
            paragraph_lines = []

    def flush_code():
        nonlocal code_lines
        if code_lines:
            story.append(Preformatted("\n".join(code_lines), styles["code"]))
            code_lines = []

    for raw_line in lines:
        line = raw_line.rstrip()

        if line.startswith("```"):
            if in_code:
                flush_code()
                in_code = False
            else:
                flush_paragraph()
                in_code = True
            continue

        if in_code:
            code_lines.append(line)
            continue

        stripped = line.strip()

        if not stripped:
            flush_paragraph()
            story.append(Spacer(1, 0.04 * inch))
            continue

        if stripped.startswith("# "):
            flush_paragraph()
            title = stripped[2:].strip()
            if not story:
                story.append(Paragraph(normalize_inline(title), styles["title"]))
            else:
                story.append(PageBreak())
                story.append(Paragraph(normalize_inline(title), styles["h1"]))
            continue

        if stripped.startswith("## "):
            flush_paragraph()
            story.append(Paragraph(normalize_inline(stripped[3:].strip()), styles["h2"]))
            continue

        if stripped.startswith("### "):
            flush_paragraph()
            story.append(Paragraph(normalize_inline(stripped[4:].strip()), styles["h3"]))
            continue

        if stripped.startswith("- "):
            flush_paragraph()
            story.append(
                Paragraph(
                    normalize_inline(stripped[2:].strip()),
                    styles["bullet"],
                    bulletText="-",
                )
            )
            continue

        if re.match(r"^\d+\.\s+", stripped):
            flush_paragraph()
            match = re.match(r"^(\d+)\.\s+(.*)$", stripped)
            number = match.group(1)
            text = match.group(2)
            story.append(
                Paragraph(
                    normalize_inline(text),
                    styles["bullet"],
                    bulletText=f"{number}.",
                )
            )
            continue

        paragraph_lines.append(stripped)

    flush_paragraph()
    flush_code()
    return story


def draw_header_footer(canvas, doc):
    canvas.saveState()
    width, height = letter

    canvas.setStrokeColor(colors.HexColor("#CBD5E1"))
    canvas.setLineWidth(0.5)
    canvas.line(doc.leftMargin, height - 0.55 * inch, width - doc.rightMargin, height - 0.55 * inch)

    canvas.setFont("Helvetica", 7.5)
    canvas.setFillColor(colors.HexColor("#64748B"))
    canvas.drawString(doc.leftMargin, height - 0.43 * inch, "Unity Engineer Assessment Master Plan")
    canvas.drawRightString(width - doc.rightMargin, 0.42 * inch, f"Page {doc.page}")

    canvas.setStrokeColor(colors.HexColor("#CBD5E1"))
    canvas.line(doc.leftMargin, 0.58 * inch, width - doc.rightMargin, 0.58 * inch)
    canvas.restoreState()


def main():
    OUTPUT.parent.mkdir(parents=True, exist_ok=True)
    markdown = SOURCE.read_text(encoding="utf-8")
    story = parse_markdown(markdown)

    doc = SimpleDocTemplate(
        str(OUTPUT),
        pagesize=letter,
        leftMargin=0.65 * inch,
        rightMargin=0.65 * inch,
        topMargin=0.78 * inch,
        bottomMargin=0.72 * inch,
        title="Unity Engineer Assessment Master Plan",
        author="Codex",
        subject="Architecture, roadmap, and implementation plan",
    )

    doc.build(story, onFirstPage=draw_header_footer, onLaterPages=draw_header_footer)
    print(OUTPUT)


if __name__ == "__main__":
    main()
