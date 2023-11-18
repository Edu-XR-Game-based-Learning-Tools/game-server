# game-server

### Update database

Add-Migration -Context AppDbContext {Migration name}
Update-Database -Context AppDbContext
Remove-Migration -Context AppDbContext
Unapply migration on db: Update-Database -Context AppDbContext {Previous migration name}

### MagicOnion AOT Generator for IL2CPP

cd ./GBLT.Shared
dotnet moc -i "./GBLT.Shared.csproj" -o "../../../game/MRTK/UnityProjects/GBLT_MRTKDev/Assets/_Project/Scripts/Network/Generated/MagicOnion.Generated.cs"
dotnet mpc -i "./GBLT.Shared.csproj" -o "../../../game/MRTK/UnityProjects/GBLT_MRTKDev/Assets/_Project/Scripts/Network/Generated/MessagePack.Generated.cs"

### Deployment
```
    Connect to remote linux VM for server and prerequisites
    ssh -i PATH_TO_PRIVATE_KEY USERNAME@EXTERNAL_IP
    Install .NET SDK or .NET Runtime on Ubuntu 22.04: https://learn.microsoft.com/en-us/dotnet/core/install/linux-ubuntu-2204
```

```
    Run Server:
    WebAPI:
    ssh -i Tom-Csun-Linux.pem tom203@4.246.147.72
    cd project/game-server/server/GBLT/GBLT.WebAPI
    dotnet watch run --launch-profile "Production"

    GameRpc:
    ssh -i Tom-Csun-Linux.pem tom203@20.25.54.224
    cd project/game-server/server/GBLT/GBLT.GameRpc
    dotnet watch run --launch-profile "Production"

    Run Client:
    React:
    ssh -i Tom-Csun-Linux.pem tom203@4.246.147.72
    cd project/client-react/build-server
    npm start    
```