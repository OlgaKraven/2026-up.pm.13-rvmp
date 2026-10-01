"""Verify the reviewed site, source fingerprint, PDF text and download links."""
import json, sys
from pathlib import Path
ROOT = Path(__file__).resolve().parents[1]
sys.path.insert(0, str(ROOT))
from methodist.content import load, fingerprint
from methodist.build import check_output, check_pdf_content
raw, data = load(ROOT)
manifest = json.loads((ROOT/'site/manifest.json').read_text())
if manifest['fingerprint'] != fingerprint(ROOT, raw):
    raise SystemExit('Sources changed: regenerate and review materials before publication.')
if not manifest.get('release'):
    raise SystemExit('Publication requires a reviewed release site.')
counts = check_output(ROOT/'site')
check_pdf_content(ROOT/'site', data)
print(json.dumps({'verified':True, 'pdf_pages':counts, 'fingerprint':manifest['fingerprint']}, ensure_ascii=False))
