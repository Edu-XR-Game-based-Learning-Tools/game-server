# game-server

### Update database

Add-Migration -Context AppDbContext {Migration name}
Update-Database -Context AppDbContext
Remove-Migration -Context AppDbContext
Unapply migration on db: Update-Database -Context AppDbContext {Previous migration name}

### MagicOnion AOT Generator for IL2CPP

dotnet moc -i "./GBLT.Shared.csproj" -o "..\\..\\..\game\MRTK\UnityProjects\GBLT_MRTKDev\Assets\\_Project\Scripts\Network\Generated\MagicOnion.Generated.cs"
dotnet mpc -i "./GBLT.Shared.csproj" -o "..\\..\\..\game\MRTK\UnityProjects\GBLT_MRTKDev\Assets\\_Project\Scripts\Network\Generated\MessagePack.Generated.cs"

### Deployment
```
    Connect to remote linux VM for server
    ssh -i PATH_TO_PRIVATE_KEY USERNAME@EXTERNAL_IP
    ssh -i Tom-Csun-Linux.pem USERNAME@EXTERNAL_IP
    
    Run server
    GameRpc: cd project/game-server/server/GBLT/GBLT.GameRpc
    WebAPI: cd project/game-server/server/GBLT/GBLT.WebAPI
    dotnet watch run --launch-profile "Production"
```