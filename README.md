# game-server

### Update database

Add-Migration Add-UserName -Context AppDbContext
Update-Database -Context AppDbContext
Remove-Migration -Context AppDbContext

### MagicOnion AOT Generator for IL2CPP

dotnet moc -i "./GBLT.Shared.csproj" -o "..\\..\\..\game\MRTK\UnityProjects\GBLT_MRTKDev\Assets\\_Project\Scripts\Network\Generated\MagicOnion.Generated.cs"
dotnet mpc -i "./GBLT.Shared.csproj" -o "..\\..\\..\game\MRTK\UnityProjects\GBLT_MRTKDev\Assets\\_Project\Scripts\Network\Generated\MessagePack.Generated.cs"
