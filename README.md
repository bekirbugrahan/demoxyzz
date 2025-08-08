# Azure SQL Web API Sample (.NET 8, Minimal API)

Bu örnek, Azure App Service + Azure SQL Database üzerinde çalışacak şekilde hazırlanmış minimal bir .NET 8 Web API projesidir.

## 1) Yerelde çalıştırma
```bash
dotnet restore
dotnet run
```
- Varsayılan bağlantı dizesi `appsettings.json > ConnectionStrings > DefaultConnection` altındadır.
- Kendi SQL Server/DB bilgilerinizle değiştirin.

## 2) Azure'a en hızlı kurulum (Portal Sihirbazı)
1. Azure Portal > Create a Resource > **Web App + Database**
2. Runtime: **.NET 8 (Linux)**, Database: **Azure SQL Database** oluşturun.
3. Wizard Connection String'i otomatik App Service'e ekler.
4. Bu proje için deploy:
```bash
dotnet publish -c Release -o out
cd out && zip -r ../app.zip * && cd ..
az webapp deploy -g <ResourceGroup> -n <WebAppName> --src-path app.zip --type zip
```

## 3) Managed Identity (Opsiyonel - Parolasız)
Daha güvenli bir kurulum için App Service'e System Assigned Managed Identity verip,
SQL'de AAD kullanıcı tanımlayarak parolasız bağlantı kullanabilirsiniz. (Önceki mesajdaki CLI adımlarını izleyin.)
