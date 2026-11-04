"""HTML design derived from the user's practice template; paginated PDF output."""
import hashlib
import html
import os
from pathlib import Path
from urllib.parse import quote, urlsplit
from .content import source_path

ESC = html.escape


STAND_LABEL = 'Скачать CSV для предметной области'


def area_download_label(data, name, page=None):
    configured = next((item.get('label') for item in data.get('downloads', []) if item['name'] == name), None)
    if configured:
        return configured
    if name == 'Directory-starter.zip':
        return 'Скачать неполную заготовку электронного справочника (ZIP)'
    if name == 'Directory-demo-complete.zip':
        return 'Скачать полный демонстрационный справочник для сверки (ZIP)'
    if name == 'Directory-source.zip':
        return 'Скачать исходный проект электронного справочника (ZIP)'
    if name == 'demo.csv':
        return 'Скачать демонстрационный CSV для проверки импорта'
    if name == 'ServiceDesk-fixed-example.zip':
        return 'Скачать исправленный демонстрационный проект (ZIP)'
    if name not in data.get('shared_area_downloads', []):
        if name == 'Program-starter.cs':
            return 'Скачать исходный код приложения без CSV (Program.cs)'
        if name == 'Program-fixed.cs':
            return 'Скачать код исправленного демонстрационного приложения (Program.cs)'
        if name.startswith('variant-') and page and page.get('subject'):
            return f'Скачать CSV для области «{page["subject"]}» ({name})'
        if name == 'demo.csv':
            return 'Скачать демонстрационный CSV с неполной строкой (demo.csv)'
        if name == 'demo-valid.csv':
            return 'Скачать демонстрационный CSV без дефектной строки (demo-valid.csv)'
        return f'{STAND_LABEL} ({name})'
    labels = {
        'ServiceDesk-source.zip': 'Скачать исходный проект ServiceDesk (ZIP)',
        'ServiceDesk-csv-draft.zip': 'Скачать заготовку подключения CSV (ZIP)',
        'ServiceDesk-fixed-example.zip': 'Скачать исправленный демонстрационный проект (ZIP)',
    }
    return labels.get(name, f'Скачать общий материал для всех вариантов ({name})')


def download_description(name, data=None):
    configured = next((item.get("description") for item in (data or {}).get("downloads", []) if item["name"] == name), None)
    if configured:
        return configured
    return {
        'Directory-starter.zip': 'Начните с неё: дополните интерфейс и функции для своей предметной области.',
        'Directory-demo-complete.zip': 'Сверяйте устройство после самостоятельной разработки; это решение другой области.',
        'Directory-source.zip': 'Начните с него: распакуйте проект, откройте index.html и проверьте импорт.',
        'ServiceDesk-source.zip': 'Начните с него: соберите и проверьте приложение без CSV.',
        'ServiceDesk-csv-draft.zip': 'Откройте после первой проверки, чтобы изучить дефекты импорта и экспорта.',
        'ServiceDesk-fixed-example.zip': 'Сверяйтесь после самостоятельной диагностики; это решение демонстрационной области.',
        'demo.csv': 'Откройте до запуска, затем загрузите для проверки качества входных строк.',
        'demo-valid.csv': 'Используйте для отдельной проверки экспорта и повторного импорта.',
    }.get(name, 'Подключите данные своего варианта после проверки исходного приложения.')


def download_url(data, name, release=False):
    base = data['meta'].get('site_url')
    if not base:
        if release:
            raise ValueError('meta.site_url is required for downloadable links in release PDFs')
        return quote(name)  # Draft PDF and its downloads share a directory.
    parsed = urlsplit(base)
    if parsed.scheme != 'https' or not parsed.netloc or parsed.query or parsed.fragment or parsed.username or parsed.password:
        raise ValueError('meta.site_url must be an HTTPS site root without query or fragment')
    return base.rstrip('/') + '/downloads/' + quote(name)


def image_name(block):
    return hashlib.sha256(block['path'].encode()).hexdigest()[:16] + Path(block['path']).suffix.lower()


def html_page(data, section, release=False):
    meta=data['meta']
    titles={'assignment':meta['title'],'guide':'Инструкция и образец','areas':'Предметные области'}
    routes=[('assignment','index.html','Задание'),('guide','lessons.html','Инструкция и образец'),('areas','areas.html','Предметные области')]
    nav=''.join(f'<a href="{url}" '+('aria-current="page"' if key==section else '')+f'>{label}</a>' for key,url,label in routes)
    body=[]
    if section=='assignment':
        links=[('assignment.pdf','Методические рекомендации (PDF)'),('guide.pdf','Список предметных областей (PDF)'),('Отчет о выполнении.docx','Форма отчёта (DOCX)')]
        body.append('<div class="downloads">'+''.join(f'<a href="downloads/{quote(n)}">{ESC(label)}</a>' for n,label in links)+'</div>')
    specs=data[section]+(data['files'] if section=='assignment' else [])
    for index,page in enumerate(specs,1):
        anchor=f'variant-{page["id"]:02}' if section=='areas' else f'page-{index}'
        blocks=[]
        if section=='areas':
            blocks.append('<p class="context"><b>Индивидуальная работа:</b> '+ESC(page['subject'])+'. <b>Образец:</b> «Кристальный маршрут», показывает способ выполнения.</p>')
        names=page.get('downloads',[]) if section=='areas' else (['CrystalRoute-starter.zip','CrystalRoute-demonstration.zip','Отчет о выполнении.docx'] if page in data['files'] else [])
        if names:
            links=''.join(f'<p><a href="downloads/{quote(n)}">{ESC("Скачать форму отчёта (DOCX)" if n.endswith(".docx") else area_download_label(data,n,page))}</a></p>' for n in names)
            blocks.append('<div class="downloads">'+links+'</div>')
        for n,block in enumerate(page['blocks'],1):
            if block['kind']=='image':
                content=f'<figure><img src="assets/screenshots/{image_name(block)}" alt="{ESC(block.get("alt",block["text"]))}"><figcaption>{ESC(block["text"])}</figcaption></figure>'
            elif block['kind'] in {'code','listing'}:content='<pre>'+ESC(block['text'])+'</pre>'
            else:content=''.join('<p>'+ESC(x).replace('\n','<br>')+'</p>' for x in block['text'].split('\n\n'))
            blocks.append(f'<div class="step"><span class="number">{n}</span><section class="card"><h3>{ESC(block["title"])}</h3>{content}</section></div>')
        kicker='Вариант '+str(page['id']).zfill(2) if 'id' in page else f'ПМ.13 · {index}. '+page.get('kicker','Оформление игрового продукта')
        body.append(f'<article class="chapter" id="{anchor}"><header class="doc-head"><p class="section-id">{ESC(kicker)}</p><h2>{ESC(page["title"])}</h2></header><div class="steps">'+''.join(blocks)+'</div></article>')
    return '<!doctype html><html lang="ru"><head><meta charset="utf-8"><meta name="viewport" content="width=device-width,initial-scale=1"><title>'+ESC(titles[section])+'</title><link rel="icon" href="assets/favicon.svg"><link rel="stylesheet" href="assets/style.css"></head><body class="'+('subject-areas' if section=='areas' else 'method')+'"><a class="skip" href="#main">К содержанию</a><nav aria-label="Разделы практики">'+nav+'</nav><main id="main">'+''.join(body)+'</main></body></html>'


def report_markdown(data):
    font = "font-family:'Times New Roman';color:#000;"
    body = font + 'font-size:14pt;text-align:justify;line-height:1.5;text-indent:1.25cm;'
    heading = font + 'font-size:14pt;text-align:justify;line-height:1.5;page-break-after:avoid;'
    caption_style = font + 'font-size:12pt;text-align:center;line-height:1;text-indent:0;'
    def paragraph(text):
        return f'<p style="{body}">{ESC(text).replace(chr(10), "<br/>")}</p>'
    lines = [f'<div lang="ru" style="{body}">', '',
             f'<h1 style="{font}font-size:16pt;text-align:center;line-height:1.5;page-break-after:avoid;">{ESC(("Отчёт — " + data["meta"]["title"]).upper())}</h1>', '']
    fields = ['ФИО', 'Группа', 'Вариант', 'Предметная область', 'Дата', 'Версия программы']
    if data['report'].get('project_work', True):
        fields.append('Ссылка на репозиторий проекта')
    for field in fields:
        lines += [paragraph(field + ': ____________________'), '']
    number = 0
    for section in data['report']['sections']:
        lines += [f'<h2 style="{heading}">{ESC(section["title"])}</h2>', '',
                  paragraph(section['prompt']), '', paragraph('Заполните по результатам своей работы.'), '']
        for caption in section.get('screenshots', []):
            number += 1
            lines += ['<div style="page-break-inside:avoid;break-inside:avoid;">',
                      '<table width="100%" border="1" cellspacing="0" cellpadding="0" style="border-collapse:collapse;width:100%;page-break-after:avoid;">',
                      '<tr>', f'<td height="220" style="border:1px solid #000;height:220px;text-align:center;vertical-align:middle;{font}font-size:12pt;line-height:1;text-indent:0;">Вставьте скриншот</td>',
                      '</tr>', '</table>',
                      f'<p style="{caption_style}page-break-before:avoid;">Рисунок {number} — {ESC(caption)}</p>',
                      '</div>', '']
    lines += ['</div>', '']
    return '\n'.join(lines)


def fonts():
    from reportlab.pdfbase import pdfmetrics
    from reportlab.pdfbase.ttfonts import TTFont
    assets=Path(__file__).parent/'assets/fonts'
    for alias,name in [('Body','Regular'),('Semi','Semibold'),('Bold','Bold')]:
        pdfmetrics.registerFont(TTFont(alias,str(assets/f'GolosText-{name}.ttf')))
    choices=[Path('C:/Windows/Fonts/consola.ttf'),Path('/usr/share/fonts/truetype/dejavu/DejaVuSansMono.ttf')]
    mono=next((p for p in choices if p.is_file()),None)
    if mono is None:raise ValueError('A monospaced font is required')
    pdfmetrics.registerFont(TTFont('Mono',str(mono)))
    symbols=Path('C:/Windows/Fonts/seguisym.ttf')
    if not symbols.is_file():symbols=Path('/usr/share/fonts/truetype/dejavu/DejaVuSans.ttf')
    pdfmetrics.registerFont(TTFont('Symbols',str(symbols)))


def pdf(root, target, data, section, release=False):
    from reportlab.lib import colors
    from reportlab.lib.enums import TA_LEFT
    from reportlab.lib.styles import ParagraphStyle
    from reportlab.lib.units import mm
    from reportlab.platypus import BaseDocTemplate, Frame, PageTemplate, Paragraph, Spacer, PageBreak, Image, Flowable, HRFlowable
    fonts()
    def E(text):
        return ESC(text).replace('✓','<font name="Symbols">✓</font>')
    W,H=210*mm,297*mm
    available=160*mm
    cwidth=available-33
    inner=cwidth-26
    body=ParagraphStyle('Body',fontName='Body',fontSize=10.5,leading=16,alignment=TA_LEFT,spaceAfter=6)
    heading=ParagraphStyle('Heading',fontName='Semi',fontSize=13,leading=18,spaceAfter=10)
    mono=ParagraphStyle('Mono',fontName='Mono',fontSize=8.5,leading=12,splitLongWords=True,wordWrap='CJK')
    caption=ParagraphStyle('Caption',fontName='Body',fontSize=9.5,leading=15,spaceAfter=0)
    title=ParagraphStyle('Title',fontName='Bold',fontSize=24,leading=28,textColor=colors.white,spaceAfter=0)
    kicker=ParagraphStyle('Kicker',fontName='Semi',fontSize=9,leading=13,textColor=colors.white,spaceAfter=0)
    check=ParagraphStyle('Check',parent=caption,spaceBefore=0)

    class Banner(Flowable):
        keepWithNext=True
        def __init__(self,label,name,key):
            Flowable.__init__(self)
            self.label=Paragraph(ESC(label),kicker)
            self.name=Paragraph(ESC(name),title)
            self.key=key
            self.outline=name
            self.label.wrap(available-36,1000)
            self.height=19+13+12+self.name.wrap(available-36,1000)[1]+20+4
            self.width=available
        def wrap(self,w,h):return self.width,self.height
        def draw(self):
            c=self.canv;c.setFillColor(colors.HexColor('#1c1c1c'));c.rect(0,0,self.width,self.height,fill=1,stroke=0)
            c.bookmarkPage(self.key);c.addOutlineEntry(self.outline,self.key,level=0,closed=False)
            c.setFillColor(colors.HexColor('#ed131c'));c.rect(0,0,self.width,4,fill=1,stroke=0)
            self.label.drawOn(c,18,self.height-19-13)
            h=self.name.wrap(available-36,1000)[1];self.name.drawOn(c,18,24)

    class Card(Flowable):
        def __init__(self,n,name,parts,continued=False):
            Flowable.__init__(self);self.n=n;self.name=name;self.parts=parts;self.continued=continued
            self.width=available;self.spaceAfter=13
            self.head=Paragraph(ESC(name)+(' — продолжение' if continued else ''),heading)
        def measures(self):
            self.hh=self.head.wrap(inner,10000)[1]
            self.hs=[p.wrap(inner,10000)[1]+p.getSpaceAfter() for p in self.parts]
            return 26+self.hh+10+sum(self.hs)
        def wrap(self,w,h):self.height=self.measures();return self.width,self.height
        def split(self,w,h):
            if self.measures() <= H-40*mm:
                return []
            if h<155:return []
            fixed=26+self.head.wrap(inner,10000)[1]+10
            used=fixed;first=[];rest=[]
            for i,p in enumerate(self.parts):
                ph=p.wrap(inner,10000)[1]+p.getSpaceAfter()
                if used+ph<=h:
                    first.append(p);used+=ph;continue
                split=p.split(inner,h-used-p.getSpaceAfter()) if h-used>=32 else []
                if split:
                    first.append(split[0]);rest=split[1:]+self.parts[i+1:]
                else:rest=self.parts[i:]
                break
            if not first or not rest:return []
            return [Card(self.n,self.name,first,self.continued),Card(self.n,self.name,rest,True)]
        def draw(self):
            self.measures();c=self.canv
            c.setStrokeColor(colors.HexColor('#dfe1e5'));c.setLineWidth(.6);c.line(12,0,12,self.height-35)
            c.setFillColor(colors.HexColor('#f4f4f6'));c.roundRect(33,0,cwidth,self.height,10,fill=1,stroke=0)
            c.setFillColor(colors.white);c.setStrokeColor(colors.HexColor('#ed131c'));c.setLineWidth(1.1);c.circle(12,self.height-24,11,fill=1,stroke=1)
            c.setFillColor(colors.HexColor('#1c1c1c'));c.setFont('Body',9);c.drawCentredString(12,self.height-27,str(self.n))
            y=self.height-13-self.hh;self.head.drawOn(c,46,y);y-=10
            for p,ph in zip(self.parts,self.hs):
                height=p.wrap(inner,10000)[1];y-=height;p.drawOn(c,46,y);y-=p.getSpaceAfter()

    def paras(text,style=body):
        return [Paragraph(E(x).replace('\n','<br/>'),style) for x in text.split('\n\n') if x.strip()]
    def card(n,block):
        parts=[]
        if block['kind']=='image':
            image=Image(str(source_path(root,block['path'])))
            image._restrictSize(inner,250)
            parts=[image,Spacer(1,8),Paragraph(ESC(block['text']),caption)]
        elif block['kind'] in {'code','listing'}:
            for line in block['text'].split('\n'):
                text=ESC(line.expandtabs(4)).replace(' ','&#160;').replace('✓','<font name="Symbols">✓</font>')
                parts.append(Paragraph(text or '&#160;',mono))
        else:
            parts=paras(block['text'])
            if 'провер' in block['title'].lower():
                parts=[HRFlowable(width=inner,thickness=.6,color=colors.HexColor('#dfe1e5'),spaceAfter=11)]+parts
        return Card(n,block['title'],parts)

    specs=data['assignment']+data['files']+data['guide'] if section=='assignment' else data['areas']
    label='Методические рекомендации' if section=='assignment' else 'Список предметных областей'
    story=[]
    for i,page in enumerate(specs,1):
        if story:story.append(PageBreak() if section=='guide' else Spacer(1,18))
        tag=f'ПМ.13 · {i}. '+('Вариант '+str(page['id']).zfill(2) if 'id' in page else page.get('kicker','Оформление игрового продукта'))
        gap=Spacer(1,18);gap.keepWithNext=True
        story.extend([Banner(tag,page['title'],f'{section}-{i}'),gap])
        if 'id' in page:
            story.append(Card('•','Индивидуальная работа и образец',paras('Работа — '+page['subject']+'. Образец — «Кристальный маршрут»; показывает способ, его значения не заменяют условия карточки.')))
        if page in data['files'] or 'id' in page:
            names=page.get('downloads',[]) if 'id' in page else ['CrystalRoute-starter.zip','CrystalRoute-demonstration.zip','Отчет о выполнении.docx']
            parts=[]
            for name in names:
                link=download_url(data,name,True)
                text='Скачать форму отчёта (DOCX)' if name.endswith('.docx') else area_download_label(data,name,page)
                parts.append(Paragraph(f'<link href="{ESC(link)}" color="#b01010"><u>{ESC(text)}</u></link>',body))
            story.append(Card('↓','Скачивания',parts))
        for n,block in enumerate(page['blocks'],1):story.append(card(n,block))
    def footer(c,doc):
        c.saveState();c.setFillColor(colors.HexColor('#606773'));c.setFont('Body',8);c.drawCentredString(W/2,10*mm,str(doc.page));c.restoreState()
    doc=BaseDocTemplate(str(target),pagesize=(W,H),leftMargin=30*mm,rightMargin=20*mm,topMargin=20*mm,bottomMargin=20*mm,title=label+' — ПМ.13',author='',invariant=1)
    frame=Frame(30*mm,20*mm,160*mm,H-40*mm,leftPadding=0,rightPadding=0,topPadding=0,bottomPadding=0)
    doc.addPageTemplates(PageTemplate(id='page',frames=[frame],onPage=footer))
    doc.build(story)
