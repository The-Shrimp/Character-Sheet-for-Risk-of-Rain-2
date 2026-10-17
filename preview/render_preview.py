"""Render original sample HTML using an already installed local headless browser."""
from pathlib import Path
import argparse,json,subprocess
root=Path(__file__).resolve().parents[1]
parser=argparse.ArgumentParser()
parser.add_argument("--render",action="store_true")
args=parser.parse_args()
out=root/"artifacts/design-preview"
out.mkdir(parents=True,exist_ok=True)
model=json.loads((out/"preview-data.json").read_text(encoding="utf-8-sig"))
html=(root/"preview/character-sheet-preview.html").read_text(encoding="utf-8")
payload=json.dumps(model,ensure_ascii=False).replace("</","<\\/")
assert html.count("/*PREVIEW_DATA*/null")==1
destination=out/"character-sheet-preview.html"
destination.write_text(html.replace("/*PREVIEW_DATA*/null",payload),encoding="utf-8")
print("Prepared "+str(destination))
if args.render:
    candidates=[Path(r"C:\Program Files\Google\Chrome\Application\chrome.exe"),Path(r"C:\Program Files (x86)\Microsoft\Edge\Application\msedge.exe")]
    browser=next((p for p in candidates if p.is_file()),None)
    if browser is None: raise SystemExit("No existing supported Chromium browser found; nothing installed.")
    image=out/"character-sheet-preview.png"
    command=[str(browser),"--headless=new","--no-first-run","--no-default-browser-check","--disable-background-networking",
        "--disable-sync","--disable-component-update","--disable-extensions","--hide-scrollbars",
        "--user-data-dir="+str(out/"browser-profile"),"--window-size=1600,1160",
        "--screenshot="+str(image),destination.as_uri()]
    result=subprocess.run(command,capture_output=True,text=True,timeout=55,creationflags=subprocess.CREATE_NO_WINDOW)
    print("Browser exit: "+str(result.returncode))
    if result.returncode or not image.is_file(): print(result.stderr[-4000:]);raise SystemExit("Headless render failed.")
    print("Rendered "+str(image)+" ("+str(image.stat().st_size)+" bytes)")
