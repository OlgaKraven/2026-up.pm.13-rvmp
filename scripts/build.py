"""Pages deploys the exact locally reviewed artifact; never an unchecked rebuild."""
import argparse, runpy
from pathlib import Path
parser = argparse.ArgumentParser()
parser.add_argument('--release', action='store_true')
args = parser.parse_args()
if not args.release:
    parser.error('For editing use python -m methodist build .; for publication use --release.')
runpy.run_path(str(Path(__file__).with_name('check.py')), run_name='__main__')
