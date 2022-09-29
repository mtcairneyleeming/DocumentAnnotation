dotnet publish ./server/server.csproj -f netcoreapp2.2 -c Release
ssh hetzner "cd /server/DocumentAnnotation; git pull; systemctl restart dotnet-docann.service "
#& 'C:\Program Files\PuTTY\plink.exe' -ssh -i 'C:\Users\Max\Local\keys\hetzner1.ppk'  root@[ip] systemctl stop kestrel-docann.service
#& 'C:\Program Files\PuTTY\pscp.exe' -r -C  -i 'C:\Users\Max\Local\keys\hetzner1.ppk' .\server\bin\Release\netcoreapp2.2\publish\* root@[ip]:/var/aspnetcore/docann
#& 'C:\Program Files\PuTTY\plink.exe' -ssh -i 'C:\Users\Max\Local\Keys\hetzner1.ppk'  root@[ip] systemctl start kestrel-docann.service
Read-Host -Prompt "Press enter to finish"
