# ZYC.VerbClass

<p align="center">
  <img
    src="src/ZYC.VerbClass.Web/wwwroot/images/verbclass-logo.svg"
    alt="ZYC.VerbClass ロゴ"
    width="160"
  />
</p>

ZYC.VerbClass は、ABP Framework を基盤にした ASP.NET Core アプリケーションです。ユーザー、ロール、権限、テナントなどの共通管理機能に加えて、学期、時限テンプレート、科目、開講、履修者、時間割などの学務系機能を扱います。

## 技術スタック

- .NET 10
- ABP Framework 10.2.0-rc.4
- ASP.NET Core Razor Pages
- Entity Framework Core
- SQLite
- OpenIddict
- Serilog
- Mapperly

## リポジトリ構成

```text
src/
  ZYC.VerbClass.slnx
  ZYC.VerbClass.Domain.Shared/
  ZYC.VerbClass.Domain/
  ZYC.VerbClass.Application.Contracts/
  ZYC.VerbClass.Application/
  ZYC.VerbClass.EntityFrameworkCore/
  ZYC.VerbClass.HttpApi/
  ZYC.VerbClass.HttpApi.Client/
  ZYC.VerbClass.Academic.*                 学務機能モジュール
  ZYC.VerbClass.Web/                       Web アプリケーション
  ZYC.VerbClass.Web.Core/
  ZYC.VerbClass.Web.Modules.Academic/
  ZYC.VerbClass.Web.Modules.Mock/
  ZYC.VerbClass.DbMigrator/                データベース作成と初期データ投入
```

## 前提条件

- .NET SDK 10.x

## ビルド

```powershell
dotnet build src/ZYC.VerbClass.slnx
```

## データベースの初期化

このプロジェクトは SQLite を使用します。ローカルデータベースの作成や初期データ投入は `ZYC.VerbClass.DbMigrator` から実行します。

```powershell
dotnet run --project src/ZYC.VerbClass.DbMigrator/ZYC.VerbClass.DbMigrator.csproj
```

既定の接続文字列は `src/ZYC.VerbClass.DbMigrator/appsettings.json` の `ConnectionStrings:Default` で管理されています。

## Web アプリケーションの起動

```powershell
dotnet run --project src/ZYC.VerbClass.Web/ZYC.VerbClass.Web.csproj
```

既定のアプリケーション URL は `https://localhost:44375` です。設定は `src/ZYC.VerbClass.Web/appsettings.json` の `App:SelfUrl` で管理されています。

## 開発メモ

- ソリューションファイルは `src/ZYC.VerbClass.slnx` です。
- Entity Framework のコマンドを直接実行するのではなく、通常は `ZYC.VerbClass.DbMigrator` を使用してデータベースを更新します。
- リポジトリの作業ルールは `AGENTS.md` にあります。
- Web プロジェクト配下には追加のローカルルールがあります。UI、CSS、Razor Pages、JavaScript を編集する場合は対象ディレクトリの `AGENTS.md` も確認してください。

## ライセンス

このプロジェクトは MIT License の下で公開されています。詳細は [LICENSE](LICENSE) を参照してください。
