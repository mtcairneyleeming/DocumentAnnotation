& 'C:\Program Files\PuTTY\plink.exe' -ssh -i 'C:\Users\Max\Local\keys\hetzner1.ppk'  root@[ip] "cd ~; pg_dump -U postgres -f currentprod.bk -F c annotations"
& 'C:\Program Files\PuTTY\pscp.exe' -C  -i 'C:\Users\Max\Local\keys\hetzner1.ppk' root@[ip]:/root/currentprod.bk .\currentprod.bk
