dotnet publish ./server/server.csproj -c Release  -p:UseAppHost=false
 tar -c -z -f publish.tar.xz -C .\server\bin\Release\net6.0\publish -  .
 scp -rC .\publish.tar.xz hetzner:\server\DocumentAnnotation

ssh hetzner "systemctl stop dotnet-docann.service; cd /server/DocumentAnnotation; tar -xf publish.tar.xz; systemctl start dotnet-docann.service"

#& 'C:\Program Files\PuTTY\plink.exe' -ssh -i 'C:\Users\Max\Local\keys\hetzner1.ppk'  root@[ip] systemctl stop kestrel-docann.service
#& 'C:\Program Files\PuTTY\pscp.exe' -r -C  -i 'C:\Users\Max\Local\keys\hetzner1.ppk' .\server\bin\Release\netcoreapp2.2\publish\* root@[ip]:/var/aspnetcore/docann
#& 'C:\Program Files\PuTTY\plink.exe' -ssh -i 'C:\Users\Max\Local\Keys\hetzner1.ppk'  root@[ip] systemctl start kestrel-docann.service
Read-Host -Prompt "Press enter to finish"
