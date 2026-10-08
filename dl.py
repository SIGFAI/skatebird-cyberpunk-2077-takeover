import urllib.request,sys,json
H={'User-Agent':'Mozilla/5.0'}
def get(u,out=None):
    r=urllib.request.urlopen(urllib.request.Request(u,headers=H),timeout=60).read()
    if out: open(out,'wb').write(r)
    return r
